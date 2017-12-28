using Home.Neeo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rts.Base.Rest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestNEEOServer.Controllers
{
    public class BRAINController : Controller
    {
        public class RegisterParams
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("baseUrl")]
            public string BaseUrl { get; set; }
        }
        public class Entry
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("label")]
            public string Label { get; set; }
            [JsonProperty("eventKey")]
            public string EventKey { get; set; }
        }

        public class MessageData
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("data")]
            public object Data { get; set; }
        }

        static Dictionary<string, Home.Neeo.Device.DataBase.DeviceEntry>    _devices = new Dictionary<string, Home.Neeo.Device.DataBase.DeviceEntry>();
        static int                                                          _eventKey = 0;

        [Route("/v1/api/notificationkey/{adapterName}/{deviceId}/{uniqueDeviceId}")] 
        [HttpGet]
        public IActionResult GetNotificationKey (string adapterName, string deviceId, string uniqueDeviceid)
        {
            NEEOEnvironment.Logger.LogInformation($"Get NotificationKey {adapterName} - {deviceId} - {uniqueDeviceid}");
            Home.Neeo.Device.DataBase.DeviceEntry device;
            if (_devices.TryGetValue (deviceId, out device))
            {
                string componentName = deviceId;
                var entries = new List<Entry>();
                foreach (var comp in device.Capabilities)
                {
                    _eventKey++;
                    entries.Add(new Entry { Name = comp.Name, Label = comp.Label, Type = comp.TypeString, EventKey = _eventKey.ToString() });
                }
                return Json(entries);
            }
            else
            {
                return Json(new Entry[0]);
            }
        }
        [Route("/v1/notifications")]
        [HttpPost]
        public IActionResult Notifications([FromBody]MessageData msg)
        {
            NEEOEnvironment.Logger.LogInformation($"Notification {msg.Type} - {msg.Data}");
            return Ok();
        }
        [Route("/v1/api/registerSdkDeviceAdapter")]
        [HttpPost]
        public IActionResult Register ([FromBody]RegisterParams par)
        {
            NEEOEnvironment.Logger.LogInformation($"Register {par.Name} - {par.BaseUrl}");
            Task.Run(async () => await ReadDevice(par.BaseUrl));
            return Ok();
        }
        [Route("/v1/api/unregisterSdkDeviceAdapter")]
        [HttpPost]
        public IActionResult UnRegister([FromBody]RegisterParams par)
        {
            return Ok();
        }

        private async Task ReadDevice(string url)
        {
            if (url.EndsWith("/"))
                url = url + "db/0";
            else
                url = url + "/db/0";
            RestClient restClient = new RestClient(new Rts.Base.Json.NewtonSoftJson(false, false, true, false));
            var result = await restClient.HttpGet<Home.Neeo.Device.DataBase.DeviceEntry>(url);
            if (result != null)
                _devices[result.AdapterName] = result;
        }
    }
}
