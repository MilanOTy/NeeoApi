using Home.Neeo;
using Home.Neeo.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TestNEEOServer.Services;
using TestNEEOServer.Services.Neeo;

namespace TestNEEOServer.Controllers
{
    [Route("Neeo")]
    public class NEEOController : Controller
    {
        public class Query
        {
            [JsonProperty("q")]
            public string Q { get; set; }
        }
        IRequestHandler _requestHandler;
        public NEEOController(NeeoService neeoService)
        {
            _requestHandler = neeoService.RequestHandler;
        }

        [Route("db/search")]
        [HttpGet]
        public IActionResult SearchDevice (Query query)
        {
            var res = _requestHandler.DBSearchDevice(query.Q);
            return Json(res);
        }
        [Route("db/{deviceId}")]
        [HttpGet]
        public IActionResult GetDevice(int deviceId)
        {
            var res = _requestHandler.DBGetDevice(deviceId);
            return Json(res);
        }
        [Route("device/{adapterId}/discover")]
        [HttpGet]
        public async Task<IActionResult> Discover(string adapterId)
        {
            var res = await _requestHandler.DeviceDiscover(adapterId);
            return Json(res);
        }
        [Route("device/{adapterId}/{component}/{deviceId}/{value}")]
        [HttpGet]
        public async Task<IActionResult> Setter(string adapterId, string component, string deviceId, string value)
        {
            var res = await _requestHandler.DeviceSetValue(adapterId, component, deviceId, value);
            return Json(new SuccessResult(res));
        }
        [Route("device/{adapterId}/{component}/{deviceId}")]
        [HttpGet]
        public async Task<IActionResult> Getter (string adapterId, string component, string deviceId)
        {
            var handler = await _requestHandler.GetHandler(adapterId, component);
            if (handler == null)
                return Json(new SuccessResult(false));

            var res = await _requestHandler.DeviceGetValue(handler, deviceId);
            if (handler.ComponentType == Home.Neeo.Device.ComponentType.TYPE_BUTTON)
                return Json(new SuccessResult(Convert.ToBoolean(res)));

            return Json(new ValueResult(res));
        }
        [Route("{adapterName}/subscribe/{deviceId}/{eventUriPrefix}")]
        [HttpGet]
        public async Task<IActionResult> Subscribe(string adapterId, string deviceId, string eventUriPrefix)
        {
            var res = await _requestHandler.Subscribe(adapterId, deviceId, eventUriPrefix);
            return Json(new SuccessResult(res));
        }

        [Route("{adapterName}/unsubscribe/{deviceId}")]
        [HttpGet]
        public async Task<IActionResult> UnSubscribe(string adapterId, string deviceId)
        {
            var res = await _requestHandler.Unsubscribe(adapterId, deviceId);
            return Json(new SuccessResult(res));
        }
    }
}
