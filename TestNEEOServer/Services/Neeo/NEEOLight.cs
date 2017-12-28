using Home.Neeo;
using Home.Neeo.Device;
using Home.Neeo.Device.ImplementationService;
using Home.Neeo.Device.Validation;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestNEEOServer.Services
{
    public class NEEOLight : IBuildDevice
    {
        DeviceState<LightStates>        _deviceState;
        LightStates                     _state;
        LightStates                     _initialState;
        NEEONotificationFunc            _notifyFunc;
        bool                            _multiple;

        public class LightState
        {
            public string   Device;
            public string   Key;
            public string   Name;
            public bool     On;
            public double   Level;
        }
        public class LightStates : Dictionary<string,LightState>
        {
        }

        public async Task<LightStates> GetState()
        {
            var cache = _deviceState.GetCachePromise(0);

            if (cache == null)
            {
                return _initialState;
            }
            return await cache.GetValue(AcquireState);
        }

        public NEEOLight(bool multiple, string[] device, string[] keys, string[] names)
        {
            _state = _initialState = new LightStates();
            _multiple = multiple;
            for (int i = 0; i < keys.Length; i++)
            {
                string compName = keys[i].Replace(' ', '_');
                _state.Add(compName, new LightState { Device = device[i], Key = keys[i], Name = names[i] });
            }
        }
        private Task<LightStates> AcquireState()
        {
            return Task.FromResult(_state);
        }
        public DeviceBuilder BuildDevice()
        {
            var deviceBuilder = NEEOModule.BuildDevice("Licht")
                .SetManufacturer("RIC")
                .AddAdditionalSearchToken("riclicht")
                .SetType(DeviceType.TYPE.LIGHT)
                .RegisterSubscriptionFunction((notify, opt) =>
                {
                    _notifyFunc = notify;
                })
                .AddCapability("alwaysOn");
            if (_multiple)
            {
                foreach (var light in _initialState)
                {
                    deviceBuilder
                    .AddSwitch("sw" + light.Key, "EIN",
                        async (id) => await GetOn (light.Key),
                        async (id, value) => await SetOn (light.Key, value)
                    )
                    .AddSlider("sl" + light.Key, light.Value.Name, 0, 100, "%",
                        async (id) => await GetLevel(light.Key),
                        async (id, value) => await SetLevel(light.Key, value)
                    );
                }
            }
            else
            {
                deviceBuilder
                    .AddSwitch("sw", "EIN",
                        async (id) => await GetOn(id),
                        async (id, value) => await SetOn(id, value)
                    )
                    .AddSlider("sl" , "Level", 0, 100, "%",
                        async (id) => await GetLevel(id),
                        async (id, value) => await SetLevel(id, value)
                    )
                    .EnableDiscovery("header", "description", () =>
                     {
                         NEEOEnvironment.Logger.LogInformation("Discover ");
                         List<NEEODiscoveredDevice> ddev = new List<NEEODiscoveredDevice>();
                         foreach (var light in _initialState)
                         {
                             ddev.Add(new NEEODiscoveredDevice { Id = light.Key, Name = light.Value.Key, Reachable = true });
                         };
                         return Task.FromResult(ddev.ToArray()); ;
                     });
            }

            _deviceState = NEEOModule.BuildDeviceState<LightStates>(1000);
            _deviceState.AddDevice(0, _initialState, false);

            return deviceBuilder;
        }
        async Task<bool> GetOn (string key)
        {
            NEEOEnvironment.Logger.LogInformation($"Get Light {key} On");
            return (await GetState())[key].On;
        }
        async Task<double> GetLevel(string key)
        {
            NEEOEnvironment.Logger.LogInformation($"Get Light {key} Level");
            return (await GetState())[key].Level;
        }
        async Task<bool> SetOn(string key, object value)
        {
            NEEOEnvironment.Logger.LogInformation($"Set Light {key} On : {value}");
            return (await GetState())[key].On = Convert.ToBoolean(value);
        }
        async Task<double> SetLevel(string key, object value)
        {
            NEEOEnvironment.Logger.LogInformation($"Set Light {key} Level : {value}");
            return (await GetState())[key].Level = Convert.ToDouble(value);
        }
    }
}
