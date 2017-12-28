using Home.Neeo;
using Home.Neeo.Discover;
using Home.Neeo.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace TestNEEOServer.Services.Neeo
{
    public static class NeeoServiceExtensions
    {
        public static void AddNEEO(this IServiceCollection services, IConfiguration neeoConf)
        {
            if (neeoConf != null && neeoConf["Enabled"] == "True")
            {
                List<IBuildDevice> neeoDevices = new List<IBuildDevice>();
                neeoDevices.Add(new NEEOTestDevice());
                neeoDevices.Add(new NEEOLight(false,
                    new string[] { "HomeMatic", "HomeMatic", "HomeMatic", "HomeMatic", "Lightify", "HomeMatic" },
                    new string[] { "Wohnzimmer Links", "Wohnzimmer Mitte", "Wohnzimmer Rechts", "Arbeitsplatz", "Terrasse" },
                    new string[] { "Wohnzimmer Links", "Wohnzimmer Mitte", "Wohnzimmer Rechts", "Arbeitsplatz", "Terrasse" })
                    );
                neeoDevices.Add(new NEEOClima());
                neeoDevices.Add(new NEEOTV());
                IDiscover discover = null;
                if (neeoConf["BrainIPAddress"] != null)
                {
                    discover = new DiscoverModule("MyBrain", neeoConf["BrainIPAddress"], Int32.Parse(neeoConf["BrainPort"]), "1.0", "de-at", new string[] { neeoConf["BrainIPAddress"] });
                }
                else
                {
                    discover = new DiscoverModule(new NEEODiscover());
                }
                services.AddSingleton<NeeoService>((provider) => {
                    ILogger<NeeoService> logger = provider.GetService<ILogger<NeeoService>>();
                    return new NeeoService(neeoConf["Name"], discover, neeoDevices, logger, Boolean.Parse(neeoConf["Simulation"]));
                });
            }
        }
        public async static void UseNEEO(this IApplicationBuilder app, string address)
        {
            NeeoService service = app.ApplicationServices.GetService<NeeoService>();

            if (service == null)
            {
                return;
            }

            await service.Initialize(address);
        }
    }
}
