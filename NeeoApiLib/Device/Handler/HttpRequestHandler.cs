using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home.Neeo.Device.Handler
{
    internal class HttpRequestHandler : IRequestHandler
    {
        private readonly RequestHandler _requestHandler;
        private readonly ILogger        _logger;
        public HttpRequestHandler (RequestHandler requestHandler)
        {
            _requestHandler = requestHandler;
            _logger = NEEOEnvironment.Logger;
        }
        // /db/search
        public List<DataBase.SearchItem> DBSearchDevice(string query)
        {
            return _requestHandler.SearchDevice(query);
        }
        // /db/<deviceId>
        public DataBase.DeviceEntry DBGetDevice  (int id)
        {
            return _requestHandler.GetDevice(id);
        }

        public async Task<HandlerParameter> GetHandler (string adapterId, string componentId)
        {
            var adapter = await _requestHandler.GetDeviceByAdapterId(adapterId);
            if (adapter != null)
            {
                HandlerParameter handler;
                adapter.Handler.TryGetValue(componentId, out handler);
                if (handler != null)
                {
                    return handler;
                }
                else
                {
                    _logger.LogWarning($"RequestHandler | GetHandler {adapterId}.{componentId} - no handler");
                }
            }
            else
            {
                _logger.LogWarning($"RequestHandler | GetHandler {adapterId} - not found");
            }
            return null;
        }
        // /device/<adapterId>/discover
        public async Task<NEEODiscoveredDevice[]> DeviceDiscover (string adapterId)
        {
            var handler = await GetHandler(adapterId, "discover");
            if (handler == null)
                return null;

            return await _requestHandler.Discover(handler);
        }

        // /device/<adapterid>/<component>/<deviceid>
        public async Task<object> DeviceGetValue (string adapterId, string componentId, string deviceId)
        {
            var handler = await GetHandler(adapterId, componentId);
            if (handler == null)
                return null;

            return await _requestHandler.HandleGet (handler, deviceId);
        }
        public async Task<object> DeviceGetValue(HandlerParameter handler, string deviceId)
        {
            return await _requestHandler.HandleGet(handler, deviceId);
        }
        // /device/<adapterid>/<component>/<deviceid>/<value>
        public async Task<bool> DeviceSetValue (string adapterId, string componentId, string deviceId, string value)
        {
            var handler = await GetHandler(adapterId, componentId);
            if (handler == null)
                return false;

            return await _requestHandler.HandleSet(handler, deviceId, value);
        }


        // /<adapterName>/subscribe/<deviceId>/<eventUriPrefix>
        public Task<bool> Subscribe (string adapterId, string deviceId, string eventUriPrefix)
        {
            _logger.LogDebug ($"RequestHandler | NOT IMPLEMENTED: subscribe to { deviceId }, {eventUriPrefix }");
            return Task.FromResult(true);
        }

        // /<adapterName>/unsubscribe/<deviceId>
        public Task<bool> Unsubscribe  (string adapterId, string deviceId)
        {
            _logger.LogDebug($"RequestHandler | NOT IMPLEMENTED: unsubscribe from { deviceId }");
            return Task.FromResult(true);
        }

        public async Task<object> HandleRequest(string path)
        {
            string[] split = path.Split('/');
            if (split != null && split.Length > 2)
            {
                if (split[0] == "db")
                {
                    if (split[1] == "search")
                    {
                        string query = split[2];
                        var searchResult = DBSearchDevice(query);
                        return searchResult;
                    }
                    var result = DBGetDevice(Convert.ToInt32(split[1]));
                    return result;
                }
                else if (split[0] == "device")
                {
                    if (split.Length == 3 && split[2] == "discover")
                    {
                        var boolResult = await DeviceDiscover(split[1]);
                        return boolResult;
                    }
                    else
                    {
                        if (split.Length == 4)
                        {
                            var getResult = await DeviceGetValue(split[1], split[2], split[3]);
                            return getResult;
                        }
                        else if (split.Length == 5)
                        {
                            var setResult = await DeviceSetValue(split[1], split[2], split[3], split[4]);
                            return setResult;
                        }
                    }
                }
                else if (split.Length == 4 && split[1] == "subscribe")
                {
                    var subResult = Subscribe(split[0], split[2], split[3]);
                    return subResult;
                }
                else if (split.Length == 3 && split[1] == "unsubscribe")
                {
                    var unsubResult = Unsubscribe(split[0], split[2]);
                    return unsubResult;
                }
            }
            return null;
        }
    }
}
