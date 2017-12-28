using Home.Neeo;
using Home.Neeo.Device;
using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using Rts.Base.Json;
using Rts.Base.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestNEEOServer.Services.Neeo
{
    public class NeeoService : RestClient, Home.Neeo.Interfaces.IRestClient
    {
        string                      _name;
        IEnumerable<IBuildDevice>   _devices;
        ILogger                     _logger;
        public NeeoService(string name, IDiscover discover, IEnumerable<IBuildDevice> devices, ILogger logger, bool isSimulation) :
            base(new NewtonSoftJson(true, false, true, true), logger: logger)
        {
            _name = name;
            _devices = devices;

            NEEOEnvironment.Logger = _logger = logger;
            NEEOEnvironment.RestClient = this;
            NEEOEnvironment.MachineName = Environment.MachineName;
            NEEOEnvironment.IsSimulation = isSimulation;
        }

        public IEnumerable<IBuildDevice> Devices => _devices;

        public async Task<bool> Initialize(string hostAddress)
        {
            List<DeviceBuilder> buildDevices = new List<DeviceBuilder>();
            if (_devices != null)
            {
                foreach (var device in _devices)
                {
                    buildDevices.Add(device.BuildDevice());
                }
            }
            var brain = await NEEOModule.DiscoverOneBrain();
            if (brain == null)
            {
                _logger.LogWarning("NEEOService : No Brain detected / configured");
                return false;
            }
            var conf = new NEEOConf(_name, 0, buildDevices, brain, hostAddress);

            bool rc = await NEEOModule.StartServer(conf);
            if (!rc)
            {
                _logger.LogWarning("NEEOService : Error starting NEEO server");
            }
            else
            {
                _logger.LogInformation("NEEOService : NEEO server started");
            }
            return rc;
        }
        public IRequestHandler RequestHandler
        {
            get
            {
                return NEEOModule.GetRequestHandler();
            }
        }
        protected override HttpClient GetHttpClient()
        {
            var httpClient = base.GetHttpClient();
            if (httpClient.DefaultRequestHeaders.ConnectionClose == null || httpClient.DefaultRequestHeaders.ExpectContinue == null)
            {
                httpClient.DefaultRequestHeaders.Connection.Clear();
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.ConnectionClose = false;
                // The next line isn't needed in HTTP/1.1
                httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            }
            return httpClient;
        }

    }
}
