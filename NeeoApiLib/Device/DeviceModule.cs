using Home.Neeo.Device.Brain;
using Home.Neeo.Device.Handler;
using Home.Neeo.Device.ImplementationService;
using Home.Neeo.Device.Validation;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home.Neeo.Device
{
    public class DeviceModule
    {
        private static IRequestHandler   _requestHandler;

        internal static DeviceBuilder BuildCustomDevice(string adaptername, string uniqueString)
        {
            if (adaptername == null)
            {
                throw new NEEOException("MISSING_ADAPTERNAME");
            }
            return new DeviceBuilder(adaptername, uniqueString, NEEOEnvironment.Logger);
        }
        internal static DeviceState<T> BuildDeviceState<T>() where T:class
        {
            return DeviceState<T>.BuildInstance();
        }
        internal static Task<bool> StartServer(NEEOConf conf)
        {
            if (conf == null || conf.Brain == null || conf.Name == null || conf.Devices == null)
            {
                throw new NEEOException("INVALID_STARTSERVER_PARAMETER");
            }

            string adapterName = GenerateAdapterName(conf);
            var devicesDatabase = BuildDevicesDatabase(conf, adapterName);
            var requestHandler = HandlerModule.Build(devicesDatabase);
            _requestHandler = new HttpRequestHandler(requestHandler);
            conf.BaseUrl = conf.BaseUrl ?? GenerateBaseUrl(conf);

            return StartSdkAndRetryIfConnectionFailed(conf, adapterName, requestHandler);
        }
        internal static async Task<bool> StopServer(NEEOConf conf)
        {
            if (conf == null || conf.Brain == null || conf.Name == null)
            {
                throw new NEEOException("INVALID_STOPSERVER_PARAMETER");
            }
            //var adapterName = ValidationModule.GetUniqueName(conf.Name);      probably wrong in node.js source
            var adapterName = GenerateAdapterName(conf);
            bool rc = await BrainModule.Stop(conf, adapterName);
            return rc;
        }
        internal static IRequestHandler GetRequestHandler()
        {
            return _requestHandler;
        }

        private static string GenerateAdapterName(NEEOConf conf)
        {
            if (conf.Name == "neeo-deviceadapter")
            {
                return conf.Name;
            }
            return "src-" + ValidationModule.GetUniqueName(conf.Name);
        }
        private static DataBase BuildDevicesDatabase(NEEOConf conf, string adapterName)
        {
            List<NEEODevice> devices = new List<NEEODevice>();
            foreach (var device in conf.Devices)
            {
                var dev = BuildAndRegisterDevice(device, adapterName);
                devices.Add(dev);
            }
            return DataBase.Build(devices.ToArray());
        }
        private static NEEODevice BuildAndRegisterDevice(DeviceBuilder device, string adapterName)
        {
            NEEOOptionalCallbacks optionalCallbacks = null;
            var deviceModel = device.Build(adapterName);
            if (deviceModel.SubscriptionFunction != null)
            {
                NEEONotificationFunc boundNotificationFunction = async (param) =>
                {
                    NEEOEnvironment.Logger.LogDebug($"Device | notification {param.Component}.{param.Value}");
                    await BrainModule.SendNotification(param, deviceModel.AdapterName);
                };

                if (device._hasPowerStateSensor)
                {
                    NEEOPowerNotificationFunc powerOnNotificationFunction = async (uniqueDeviceId) =>
                    {
                        var msg = new NEEONotification { UniqueDeviceId = uniqueDeviceId, Component = "powerstate", Value = true };
                        await BrainModule.SendNotification(msg, deviceModel.AdapterName);
                    };
                    NEEOPowerNotificationFunc powerOffNotificationFunction = async (uniqueDeviceId) =>
                    {
                        var msg = new NEEONotification { UniqueDeviceId = uniqueDeviceId, Component = "powerstate", Value = false };
                        await BrainModule.SendNotification(msg, deviceModel.AdapterName);
                    };
                    optionalCallbacks = new NEEOOptionalCallbacks(powerOnNotificationFunction, powerOffNotificationFunction);
                }
                deviceModel.SubscriptionFunction(boundNotificationFunction, optionalCallbacks);
            }
            return deviceModel;
        }
        private static string GenerateBaseUrl(NEEOConf conf)
        {
            var ipaddress = ValidationModule.GetAnyIpAddress();
            var baseUrl = $"http://{ipaddress}:{conf.Port}";
            NEEOEnvironment.Logger.LogDebug($"Device | Adapter baseUrl {baseUrl}");
            return baseUrl;
        }

        private static async Task<bool> StartSdkAndRetryIfConnectionFailed(NEEOConf conf, string adapterName, RequestHandler requestHandler)
        {
            for (int retryCount = 0; retryCount < conf.MaxConnectionAttempts; retryCount++)
            {
                await Task.Delay(retryCount * 1000);
                bool rc = await BrainModule.Start(conf, adapterName);
                return rc;
            }
            return false;
        }
    }
}




