using Cipher;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading;


namespace RainAlert_WindowsService
{
    public partial class RainAlert : ServiceBase
    {   
        private static bool _willRain;

        //Timer
        private Timer _timer = null;
        private int _executionHour = Convert.ToInt32(ConfigurationManager.AppSettings["ExecutionHour"]);

        //OWM
        private static string _owmRequest;
        private string _owmApiKey;
        private static LocationCoordinates _locationCoordinates = new LocationCoordinates();

        //Twilio
        private static SmsService _smsService;
        private string _accountSid;
        private string _authToken;

        //String Cipher
        private static StringCipher _stringCipher = new StringCipher(
            "AFBA97FB-9C3F-4007-B77E-78661D28AFBC");

        public RainAlert()
        {
            InitializeComponent();
            Log.Logger = new LoggerConfiguration()
                        .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs.log"))
                        .CreateLogger();
        }

        protected override void OnStart(string[] args)
        {
            Log.Information($"{nameof(RainAlert)} service starts");
            
            base.OnStart(args);
            try
            {
                _locationCoordinates.Latitude = args[0];
                _locationCoordinates.Longitude = args[1];
                Log.Information($"Podaono nowe koordynaty: lat={_locationCoordinates.Latitude}," +
                    $" lon={_locationCoordinates.Longitude}");
            }
            catch
            {
                _locationCoordinates.Latitude = ConfigurationManager.AppSettings["OWM_DefaultLatitude"];
                _locationCoordinates.Longitude = ConfigurationManager.AppSettings["OWM_DefaultLongitude"];
                Log.Error("Brak podanych parametrów lokalizacji podczas uruchomienia. " +
                    "Wczytano domyślne koordynaty dla Warszawy");
            }

            try
            {
                GetDataFromConfigurationOnStart();
                StartTimer(new TimeSpan(_executionHour, 0, 0), new TimeSpan(24, 0, 0));
                
            }
            catch (Exception exc)
            {
                Log.Error(exc, exc.Message);
            }

        }
        protected void StartTimer(TimeSpan scheduledRunTime, TimeSpan timeBetweenEachRun)
        {
            double currentTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            double scheduledTime = scheduledRunTime.TotalMilliseconds;
            double intervalPeriod = timeBetweenEachRun.TotalMilliseconds;
   
            double firstExecution = currentTime > scheduledTime ? intervalPeriod - (currentTime - scheduledTime) : scheduledTime - currentTime;

            TimerCallback callback = new TimerCallback(RunService);

            _timer = new Timer(callback, null, Convert.ToInt32(firstExecution), Convert.ToInt32(intervalPeriod));
        }


        private void GetDataFromConfigurationOnStart()
        {
            _owmApiKey = DecryptSensitiveData(ConfigurationManager.AppSettings["OWM_ApiKey"], "OWM_ApiKey");

            var openWeatherHelper = new OpenWeatherHelper(_locationCoordinates)
            {
                OwmEndpoint = ConfigurationManager.AppSettings["OWM_Endpoint"],
                OwmApiKey = _owmApiKey,
            };

            _accountSid = DecryptSensitiveData(ConfigurationManager.AppSettings["TWILIO_AccountSID"], "TWILIO_AccountSID");
            _authToken = DecryptSensitiveData(ConfigurationManager.AppSettings["TWILIO_AuthToken"], "TWILIO_AuthToken");

            _smsService = new SmsService()
            {
                AccountSid = _accountSid,
                AuthToken = _authToken,
                From = ConfigurationManager.AppSettings["TWILIO_PhoneNumberFrom"],
                To = ConfigurationManager.AppSettings["PhoneNumberToSendSms"],
            };

            _owmRequest = openWeatherHelper.GenerateApiRequest();

        }

        public void RunService(object state)
        {
            var t = new System.Threading.Tasks.Task(GetWeatherForecastFromApi);
            t.Start();
            Console.ReadLine();
        }

        protected override void OnStop()
        {
            Log.Information($"{nameof(RainAlert)} service stopped");        
            Log.CloseAndFlush();
        }

        private static string DecryptSensitiveData(string encryptedSensitiveData, string configKey)
        {
            if (encryptedSensitiveData.StartsWith("encrypt:"))
            {
                encryptedSensitiveData = _stringCipher.Encrypt(encryptedSensitiveData.Replace("encrypt:", ""));
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configFile.AppSettings.Settings[configKey].Value = encryptedSensitiveData;
                configFile.Save();
            }
            return _stringCipher.Decrypt(encryptedSensitiveData);
        }

        static async void GetWeatherForecastFromApi()
        {
            var numberOfHoursToCheckForecast = 12;

            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(_owmRequest))
            using (HttpContent content = response.Content)
            {
                Log.Information($"OWM response status code: {response.StatusCode}");
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _smsService.SendSms("Sorry, had some problem to get forecast.");
                    Log.Error("Wysłany SMS z informacją o błędzie");
                    return;
                }
                
                var result = await content.ReadAsStringAsync();

                if (result != null)
                {
                    JsonTextReader reader = new JsonTextReader(new StringReader(result));

                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.Value.ToString() == "id" 
                                && numberOfHoursToCheckForecast >= 1)
                            {
                                reader.Read();
                                var weatherCode = int.Parse(reader.Value.ToString());

                                if (weatherCode < 700)
                                {
                                    _willRain = true;
                                    break;
                                }
                                numberOfHoursToCheckForecast -= 1;
                            }
                        }
                    }
                    reader.Close();
                }
                switch (_willRain)
                {
                    case true:
                        _smsService.SendSms("Take Your umbrella ☔! Will rain today!");
                        Log.Information("Wysłany SMS z informacją - WILL RAIN");
                        break;
                    case false:
                        _smsService.SendSms("Have a nice day!😄 No rain today!");
                        Log.Information("Wysłany SMS z informacją - NO RAIN");
                        break;
                }
            }
        }
    }
}
