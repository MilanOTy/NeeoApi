using Home.Neeo.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home.Neeo.Device.Brain
{
    internal class Notification
    {
        internal class MessageData
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("data")]
            public object Data { get; set; }
        }

        const int MAXIMAL_CACHED_ENTRIES = 50;
        const int MAXIMAL_MESSAGE_QUEUE_SIZE = 20;

        private readonly IRestClient _restClient;
        private readonly string _brainUri;
        private readonly ILogger _logger;
        private readonly Dictionary<string, MessageData> _cache;
        private int _queueSize;

        public Notification(IRestClient restClient, string url, ILogger logger)
        {
            _restClient = restClient;
            _queueSize = 0;
            _brainUri = url + NEEOUrls.BASE_URL_NOTIFICATIONS;
            _logger = logger;
            _cache = new Dictionary<string, MessageData>();
        }
        internal void DecreaseQueueSize()
        {
            if (_queueSize > 0)
            {
                _queueSize--;
            }
        }
        internal bool IsDuplicateMessage(MessageData msg)
        {
            if (msg == null)
            {
                return false;
            }
            if (msg.Type == null || msg.Data == null)
            {
                return false;
            }
            MessageData lastSensorValue;
            if (_cache.TryGetValue(msg.Type, out lastSensorValue))
                return (lastSensorValue.Data == msg.Data);
            return false;
        }
        internal void UpdateCache(MessageData msg)
        {
            if (msg == null)
            {
                return;
            }
            if (msg.Type == null || msg.Data == null)
            {
                return;
            }
            if (_cache.Count > MAXIMAL_CACHED_ENTRIES)
            {
                _logger.LogInformation("Notification | clear message cache");
                _cache.Clear();
            }
            _cache[msg.Type] = msg;
        }
        internal async Task<bool> Send(MessageData msg)
        {
            bool rc = true;
            if (msg == null)
            {
                _logger.LogWarning("Notification | empty notification ignored");
                return false;
            }
            if (IsDuplicateMessage(msg))
            {
                _logger.LogWarning("Notification | DUPLICATE_MESSAGE");
                return false;
            }
            if (_queueSize >= MAXIMAL_MESSAGE_QUEUE_SIZE)
            {
                _logger.LogDebug($"Notification | MAX_QUEUESIZE_REACHED : {MAXIMAL_MESSAGE_QUEUE_SIZE}");
                return false;
            }
            _logger.LogDebug($"Notification | POST {_brainUri} : {msg.Type} - {msg.Data} ");
            _queueSize++;

            // JS : HTTPAGENT

            if (!NEEOEnvironment.IsSimulation)
            {
                SuccessResult res = await _restClient.HttpPost<SuccessResult, MessageData>(msg, _brainUri);
                rc = res != null && res.Success;
            }
            if (rc)
            {
                UpdateCache(msg);
                DecreaseQueueSize();
                return true;
            }
            else
            {
                _logger.LogWarning("Notification | failed to send notification");
                DecreaseQueueSize();
                return false;
            }
        }
    }
}





