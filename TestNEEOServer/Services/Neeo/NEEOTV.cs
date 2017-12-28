using Home.Neeo;
using Home.Neeo.Device;
using Home.Neeo.Device.ImplementationService;
using Home.Neeo.Device.Validation;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TestNEEOServer.Services
{
    public class NEEOTV : IBuildDevice
    {
        public DeviceBuilder BuildDevice()
        {
            var deviceBuilder = NEEOModule.BuildDevice("TV")
                .SetManufacturer("RIC")
                .AddAdditionalSearchToken("rictv")
                .SetType(DeviceType.TYPE.TV)
                .AddButtonGroup(BUTTONGROUP.Power)
                .AddButtonGroup(BUTTONGROUP.Channel_Zapper)
                .AddButtonGroup(BUTTONGROUP.Numpad)
                .AddButtonGroup(BUTTONGROUP.Volume)
                .AddButtonGroup(BUTTONGROUP.Controlpad)
                .AddButtonHandler((name, id) =>
                {
                    NEEOEnvironment.Logger.LogInformation($"Button {name}.{id}");
                    return Task.CompletedTask;
                });
            return deviceBuilder;
        }
    }
}
