using Home.Neeo;
using Home.Neeo.Device;
using Home.Neeo.Device.ImplementationService;
using Home.Neeo.Device.Validation;
using Home.Neeo.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TestNEEOServer.Services
{
    public class NEEOClima : IBuildDevice
    {
        DeviceState<ClimaState>        _deviceState;
        ClimaState                     _state;
        ClimaState                     _initialState;



        public class ClimaState
        {
            public string   Temperature;
        }

        public async Task<ClimaState> GetState ()
        {
            var cache = _deviceState.GetCachePromise(0);

            if (cache == null)
            {
                return _initialState;
            }
            return await cache.GetValue(AcquireState);
        }

        public NEEOClima()
        {
            _initialState = _state = new ClimaState { Temperature = "Aussen : 12 - Innen : 12" };
        }
        private Task<ClimaState> AcquireState ()
        {
            return Task.FromResult(_state);
        }
        public DeviceBuilder BuildDevice()
        {
            var deviceBuilder = NEEOModule.BuildDevice("Klima")
                .SetManufacturer("RIC")
                .AddAdditionalSearchToken("ricklima")
                .SetType(DeviceType.TYPE.CLIMA)
                .AddCapability("alwaysOn")
                .AddButton("buLinksUp", "Links AUF")
                .AddButton("buLinksDo", "Links Runter")
                .AddButton("buMitteUp", "Mitte AUF")
                .AddButton("buMitteDo", "Mitte Runter")
                .AddButton("buRechtsUp", "Rechts AUF")
                .AddButton("buRechtsDo", "Rechts Runter")
                .AddButton("bustop", "STOP")
                .AddButtonHandler((name, id) =>
                {
                    NEEOEnvironment.Logger.LogInformation($"Button {name}.{id}");
                    return Task.CompletedTask;
                })
                .AddTextLabel("tlTemp", "Temp ",
                async (id) =>
                {
                    NEEOEnvironment.Logger.LogInformation("Label myTextLabel  Getter " + id);
                    return (await GetState()).Temperature;
                });
 
            _deviceState = NEEOModule.BuildDeviceState<ClimaState>(1000);
            _deviceState.AddDevice(0, _initialState, false);

            return deviceBuilder;
        }
    }
}
