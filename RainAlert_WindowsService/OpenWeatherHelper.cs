using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace RainAlert_WindowsService
{
    public class OpenWeatherHelper
    {
        private string _owmEndpoint;
        public string OwmEndpoint
        {
            get { return _owmEndpoint; }
            set { _owmEndpoint = value; }
        }

        private string _owmApiKey;
        public string OwmApiKey
        {
            get { return _owmApiKey; }
            set { _owmApiKey = value; }
        }

        private readonly string _requestConstParams = "exclude=current,minutely,daily,alerts&units=metrics&lang=pl&";
        private readonly string _latitude;
        private readonly string _longitude;

        public OpenWeatherHelper(LocationCoordinates locationCoordinates)
        {
            _latitude= locationCoordinates.Latitude;
            _longitude= locationCoordinates.Longitude;
        }

        public string GenerateApiRequest()
        {
            var owmRequestBuilder = new StringBuilder();
            owmRequestBuilder.Append(_owmEndpoint);
            owmRequestBuilder.Append($"lat={_latitude}&");
            owmRequestBuilder.Append($"lon={_longitude}&");
            owmRequestBuilder.Append(_requestConstParams);
            owmRequestBuilder.Append($"appid={_owmApiKey}");
            Log.Information($"OWM_Generated_Request: {owmRequestBuilder}");
            return owmRequestBuilder.ToString();         
        }






    }
}
