using Home.Neeo.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Neeo.Device.Brain
{
    internal class NotificationMapping
    {
        private class Entry
        {
            [JsonProperty("name")]
            public string Name      { get; set;  }
            [JsonProperty("type")]
            public string Type      { get; set; }
            [JsonProperty("label")]
            public string Label     { get; set; }
            [JsonProperty("eventKey")]
            public string EventKey  { get; set; }
        }

        private readonly IRestClient                    _restClient;
        private readonly string                         _adapterName;
        private readonly string                         _brainUri;
        private readonly ILogger                        _logger;
        private readonly Dictionary<string, Entry[]>    _cache;

        public NotificationMapping (IRestClient restClient, string url, string adapterName, ILogger logger)
        {
            _restClient = restClient;
            _adapterName = adapterName;
            _brainUri = url + NEEOUrls.BASE_URL_NOTIFICATIONKEY + adapterName;
            _logger = logger;
            _cache = new Dictionary<string, Entry[]>();
        }
        private Task<Entry[]> FetchDataFromBrain (string uniqueDeviceId, string deviceId, string componentName)
        {
            _logger.LogTrace($"getNotificationKey {componentName} {uniqueDeviceId} -  {_adapterName} - { deviceId}");
            string url = $"{_brainUri}/{deviceId}/{uniqueDeviceId}";
            _logger.LogDebug($"GET {url}");
            if (NEEOEnvironment.IsSimulation)
            {
                return Task.FromResult(new Entry[] { new Entry { Name = componentName, Label = componentName, Type = "", EventKey = "123" } });
            }
            return _restClient.HttpGet<Entry[]>(url);
        }
        private string FindNotificationKey (string id, string componentName)
        {
            Entry[] entries = _cache[id];

            var correctEntryByName = entries.FirstOrDefault((e) => e.Name == componentName);
            if (correctEntryByName != null && correctEntryByName.EventKey != null)
                return correctEntryByName.EventKey;

            var correctEntryByLabel = entries.FirstOrDefault((e) => e.Label == componentName);
            if (correctEntryByLabel != null && correctEntryByLabel.EventKey != null)
                return correctEntryByLabel.EventKey;

            _cache.Remove(id);
            return null;
        }
        internal async Task<string> GetNotificationKey (string uniqueDeviceId, string deviceId, string componentName)
        {
            string id = CreateRequestId(_adapterName, uniqueDeviceId, deviceId);
            if (_cache.ContainsKey(id))
            {
                return FindNotificationKey(id, componentName);
            }
            Entry[] entries = await FetchDataFromBrain(uniqueDeviceId, deviceId, componentName);
            if (NEEOEnvironment.IsSimulation && entries == null)
            {
                entries = new Entry[] { new Entry { Name = componentName, Label = componentName, Type = "", EventKey = "123" } };
            }
            if (entries == null)
                return null;

            _cache[id] = entries;
            return FindNotificationKey(id, componentName);
        }
        private string CreateRequestId(string adapterName, string uniqueDeviceId, string deviceId)
        {
            return $"{uniqueDeviceId}-{deviceId}-{ adapterName}";
        }
    }
}




