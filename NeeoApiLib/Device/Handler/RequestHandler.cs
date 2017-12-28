using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Home.Neeo.Device.Handler
{
    internal class RequestHandler
    {
        private DataBase _deviceDatabase;
        private ILogger _logger;
        internal RequestHandler (DataBase deviceDataBase)
        {
            _deviceDatabase = deviceDataBase;
            _logger = NEEOEnvironment.Logger;
        }
        internal List<DataBase.SearchItem> SearchDevice(string query)
        {
            return _deviceDatabase.Search(query);
        }

        internal DataBase.DeviceEntry GetDevice(int id)
        {
            return _deviceDatabase.GetDevice(id);
        }

        internal Task<NEEODevice> GetDeviceByAdapterId(string adapterId)
        {
            return _deviceDatabase.GetDeviceByAdapterId(adapterId);
        }

        internal async Task<NEEODiscoveredDevice[]> Discover(HandlerParameter handler)
        {
            if (handler == null || handler.Controller == null || handler.Controller.Discover == null)
            {
                throw new NEEOException("INVALID_DISCOVER_PARAMETER");
            }
            var devices = await handler.Controller.Discover();
            return devices;
        }
        internal async Task<object> HandleGet (HandlerParameter handler, string deviceId)
        {
            if (DeviceIsInvalid(handler, deviceId))
            {
                _logger.LogDebug($"RequestHandler | handlerget failed for {deviceId}");
                throw new NEEOException ("INVALID_GET_PARAMETER");
            }
            _logger.LogDebug($"RequestHandler | process get request for {deviceId}");

            object result = null;
            switch (handler.ComponentType)
            {
                case ComponentType.TYPE_BUTTON:
                case ComponentType.TYPE_TEXTLABEL:
                case ComponentType.TYPE_IMAGEURL:
                case ComponentType.TYPE_SENSOR:
                case ComponentType.TYPE_SLIDER:
                case ComponentType.TYPE_SWITCH:
                    NEEOGetFunc handlerFunction = handler.Controller.Getter;
                    if (handlerFunction != null)
                    {
                        result = await handlerFunction(deviceId);
                    }
                    break;
                default:
                    _logger.LogWarning($"RequestHandler | INVALID_GET_COMPONENT { handler.ComponentType }");
                    break;
            }
            _logger.LogDebug($"RequestHandler | process get request for {deviceId} : {result}");
            return result;
        }
        internal async Task<bool> HandleSet(HandlerParameter handler, string deviceId, string value)
        {
            if (DeviceIsInvalid(handler, deviceId))
            {
                _logger.LogError($"RequestHandler | handlerset failed for {deviceId}");
                throw new NEEOException("INVALID_GET_PARAMETER");
            }
            NEEOSetFunc handlerFunction;
            _logger.LogDebug($"RequestHandler | process set request for {deviceId}");

            bool result = false;
            switch (handler.ComponentType)
            {
                case ComponentType.TYPE_SLIDER:
                    handlerFunction = handler.Controller.Setter;
                    if (handlerFunction != null)
                    {
                        result = true;
                        double dval;
                        if (value.GetType() == typeof(string) && value.ToString().IndexOf(',') >= 0)
                            dval = Convert.ToDouble(value);
                        else
                            dval = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        await handlerFunction(deviceId, dval);
                    }
                    break;
                case ComponentType.TYPE_SWITCH:
                    handlerFunction = handler.Controller.Setter;
                    if (handlerFunction != null)
                    {
                        result = true;
                        await handlerFunction(deviceId, Convert.ToBoolean(value));
                    }
                    break;
                default:
                    _logger.LogWarning($"RequestHandler | INVALID_SET_COMPONENT { handler.ComponentType }");
                    break;
            }
            return result;
        }


        private bool DeviceIsInvalid(HandlerParameter handler, string deviceId)
        {
            return (deviceId == null || handler == null || handler.ComponentType == ComponentType.TYPE_UNKNOWN || handler.Controller == null);
        }

        private bool CheckForFunction(Controller controller)
        {
            return (controller != null && (controller.Getter != null || controller.Setter != null));
        }

    }
}
