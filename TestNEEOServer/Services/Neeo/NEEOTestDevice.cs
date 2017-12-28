//#define SIMPLE
using Home.Neeo;
using Home.Neeo.Device;
using Home.Neeo.Device.ImplementationService;
using Home.Neeo.Device.Validation;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

/*
 * test requests

http://192.168.1.130:59290:/Neeo/db/search?q=neeo

http://192.168.1.130:59290:/Neeo/db/search?q=access

http://192.168.1.130:59290:/Neeo/db/search?q=myadapter

http://192.168.1.130:59290:/Neeo/db/0

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/discover

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/myTextLabel/deviceId1

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySwitch1/deviceId1

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySwitch1/deviceId1/true

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySwitch2/deviceId1

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/powerstate/deviceId1

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySwitch2/deviceId1/false

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySwitch2/deviceId1/false

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySlider1/deviceId1

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySlider1/deviceId1/40

http://192.168.1.130:59290:/Neeo/device/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/mySlider1/deviceId1

http://192.168.1.130:59290:/Neeo/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/subscribe/deviceId1/prefix1

http://192.168.1.130:59290:/Neeo/apt-F9378729EB4702F580453D62157BDC0C5F8F6BD6/unsubscribe/deviceId1
 
     
*/


namespace TestNEEOServer.Services
{
    public class NEEOTestDevice : IBuildDevice
    {
        NEEONotificationFunc                _notifyFunc = null;
        NEEOOptionalCallbacks               _optCallbacks;
        DeviceState<TestDeviceState>        _deviceState;
        TestDeviceState                     _state;
        TestDeviceState                     _initialState;



        public class TestDeviceState
        {
            public bool     Switch1;
            public bool     Switch2;
            public bool     Power;
            public double   Slider1;
            public string   TextLabel;
        }

        public async Task<TestDeviceState> GetState ()
        {
            var cache = _deviceState.GetCachePromise(0);

            if (cache == null)
            {
                return _initialState;
            }
            return await cache.GetValue(AcquireState);
        }

        public NEEOTestDevice ()
        {
            _initialState = _state = new TestDeviceState { Switch1 = false, Switch2 = true, Power = true, Slider1 = 50, TextLabel = "1234" };
        }
        private Task<TestDeviceState> AcquireState ()
        {
            return Task.FromResult(_state);
        }
        public DeviceBuilder BuildDevice()
        {
#if SIMPLE
            var deviceBuilder = NEEOModule.BuildDevice("Simple Accessory")
                .SetManufacturer("NEEO")
                .AddAdditionalSearchToken("foo")
                .SetType(DeviceType.TYPE.ACCESSORY")

                // Then we add the capabilities of the device
                .AddButton("button-a", "Button A")
                .AddButton("button-b", "Button B")
                //.DefineTimeing (500, 500, 500)
                /*
                .AddCapability("alwaysOn")
                .AddButtonGroup(BUTTONGROUP.Color_Buttons)
                .AddTextLabel("textlabel-a", "TextLabel A", (id) => { return Task.FromResult((object)"XXXXX"); })
                .AddImageUrl("image-a", "Image A", "small", "http://localhost:6336/picture.png", (id) => { return Task.FromResult((object)"http://localhost:6336/picture.png"); })
                .AddSwitch("switch-a", "Switch A'", (id) => { return Task.FromResult((object)true); }, (id, value) =>  { return Task.CompletedTask; })
                .AddSensor("sensor-a", "Sensor A", 0, 100, "°", (id) => { return Task.FromResult((object)20.0); })
                .AddSlider("power-slider", "Dimmer", 0, 200, "%", (id) => { return Task.FromResult((object)20.0); }, (id, value) => { return Task.CompletedTask; })
                */
                .AddButtonHandler((button, id) => {
                    NEEOEnvironment.Logger.LogInformation($"Button {button}-{id} pressed");
                    return Task.CompletedTask;
                });
#else
            var deviceBuilder = NEEOModule.BuildDevice("RICAdapter")
                .SetManufacturer("RIC")
                .AddAdditionalSearchToken("rictest")
                .SetType(DeviceType.TYPE.CLIMA)
                .AddButton("myButton1", "Button1")
                .AddButton("myButton2", "Button2")
                .AddSwitch("mySwitch1", "Switch1",
                async (id) =>
                {
                    NEEOEnvironment.Logger.LogInformation("Switch mySwitch1 Getter " + id);
                    return (await GetState()).Switch1;
                },
                (id, value) =>
                {
                    _state.Switch1 = Convert.ToBoolean(value);
                    NEEOEnvironment.Logger.LogInformation($"Switch mySwitch1 Setter {id} : {value}");
                    return Task.CompletedTask;
                })
                .AddSwitch("mySwitch2", "Switch2",
                async (id) =>
                {
                    NEEOEnvironment.Logger.LogInformation("Switch mySwitch2 Getter " + id);
                    return (await GetState()).Switch2;
                },
                (id, value) =>
                {
                    _state.Switch2 = Convert.ToBoolean(value);
                    NEEOEnvironment.Logger.LogInformation($"Switch mySwitch2 Setter {id} : {value}");
                    if (_state.Power != _state.Switch2)
                    {
                        _state.Power = _state.Switch2;
                        if (_state.Power && _optCallbacks.PowerOnNotificationFunction != null)
                        {
                            _optCallbacks.PowerOnNotificationFunction(id);
                        }
                        if (!_state.Power && _optCallbacks.PowerOffNotificationFunction != null)
                        {
                            _optCallbacks.PowerOffNotificationFunction(id);
                        }
                    }
                    return Task.CompletedTask;
                })
                .AddSlider("mySlider1", "Slider1", 0, 100, "%", 
                async (id) =>
                {
                    NEEOEnvironment.Logger.LogInformation("Slider mySlider1 Getter " + id);
                    return (await GetState()).Slider1;
                },
                (id, value) =>
                {
                    _state.Slider1 = Convert.ToDouble(value);
                    NEEOEnvironment.Logger.LogInformation($"Slider mySlider1 Setter {id} : {value}");
                    return Task.CompletedTask;
                })
                .AddButtonHandler((name, id) =>
                {
                    NEEOEnvironment.Logger.LogInformation($"Button {name}.{id}");
                    return Task.CompletedTask;
                })
                .AddTextLabel("myTextLabel", "TextLabel", 
                async (id) =>
                {
                    NEEOEnvironment.Logger.LogInformation("Label myTextLabel  Getter " + id);
                    return (await GetState()).Slider1;
                })
                .AddPowerStateSensor(async (id) =>
                {
                    NEEOEnvironment.Logger.LogInformation("Powerstate Getter " + id);
                    return (await GetState()).Power;
                })
                /*
                .EnableDiscovery("header", "description", () =>
                {
                    NEEOEnvironment.Logger.LogInformation("Discover ");
                    NEEODiscoveredDevice[] ddev = new NEEODiscoveredDevice[]
                    {
                        new NEEODiscoveredDevice { Id = "dev1", Name = "dev_1", Reachable = true },
                        new NEEODiscoveredDevice { Id = "dev2", Name = "dev_2", Reachable = true }
                    };
                    return Task.FromResult(ddev); ;
                })
                */
                .RegisterInitializeFunction(() =>
                {
                    NEEOEnvironment.Logger.LogInformation("Initialize ");
                    _deviceState.UpdateReachable(0, true);
                    return Task.FromResult(true);
                })
                .RegisterSubscriptionFunction((notify, opt) =>
                {
                    _notifyFunc = notify;
                    _optCallbacks = opt;
                });
#endif

            _deviceState = NEEOModule.BuildDeviceState<TestDeviceState>(1000);
            _deviceState.AddDevice(0, _initialState, false);

            return deviceBuilder;
        }
        public void SetReachable (bool reachable)
        {
            _deviceState.UpdateReachable(0, reachable);
        }
        public void GetAllDevices()
        {
            var all = _deviceState.GetAllDevices ();
        }
        public async void GetDevices()
        {
            var promise = _deviceState.GetCachePromise (0);
            if (promise != null)
            {
                var state = await promise.GetValue(GetState);
            }
            var reachable = _deviceState.GetClientObjectIfReachable(0);
        }
    }
}
