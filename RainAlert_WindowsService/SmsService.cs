using Serilog;
using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace RainAlert_WindowsService
{
    public class SmsService
    {

        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public void SendSms(string smsBody)
        {
            try
            {
                TwilioClient.Init(AccountSid, AuthToken);

                MessageResource.Create(
                    body: smsBody,
                    from: From,
                    to: To
                    );
            }
            catch (Exception exc)
            {
                Log.Error($"Failed to send sms: Error - {exc.Message}");
              }
        }
    }
}
