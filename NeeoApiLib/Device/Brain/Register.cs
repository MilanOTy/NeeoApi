using Home.Neeo.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Home.Neeo.Device.Brain
{
    internal class Register
    {

        private static readonly IRestClient                    _restClient;
        private static readonly ILogger                        _logger;

        private class Params
        {
            [JsonProperty("name")]
            public string Name      { get; set; }
            [JsonProperty("baseUrl")]
            public string BaseUrl   { get; set; }
        }

        static Register()
        {
            _restClient = NEEOEnvironment.RestClient;
            _logger = NEEOEnvironment.Logger;
        }
        static internal async Task<bool> RegisterAdapterOnTheBrain (string url, string baseUrl, string adapterName)
        {
            url = url + NEEOUrls.BASE_URL_REGISTER_SDK_ADAPTER;
            _logger.LogDebug ($"Register | RegisterAdapterOnTheBrain POST {url} : {adapterName} - {baseUrl}");

            var pars = new Params { Name = adapterName, BaseUrl = baseUrl };
            if (NEEOEnvironment.IsSimulation)
            {
                return true;
            }
            SuccessResult result = await _restClient.HttpPost<SuccessResult, Params>(pars, url);
            return result != null && result.Success;
        }
        static internal async Task<bool> UnregisterAdapterOnTheBrain(string url, string adapterName)
        {
            url = url + NEEOUrls.BASE_URL_UNREGISTER_SDK_ADAPTER;
            _logger.LogDebug($"Register | UnregisterAdapterOnTheBrain POST {url} : {adapterName}");

            var pars = new Params { Name = adapterName };
            if (NEEOEnvironment.IsSimulation)
            {
                return true;
            }
            SuccessResult result  = await _restClient.HttpPost<SuccessResult, Params>(pars, url);
            return result != null && result.Success; 
        }
    }
}
