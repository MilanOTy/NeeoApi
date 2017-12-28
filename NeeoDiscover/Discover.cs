using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Zeroconf;

namespace Home.Neeo
{
    public class NEEODiscover : IDiscover
    {
        BrowseDomainsOptions    _options;
        string                  _neeoKey;
        public NEEODiscover (string neeoKey = "_neeo.", int waitMs = 5000, int retries = 2, int retryWaitMs = 500)
        {
            _neeoKey = neeoKey;
            _options = new BrowseDomainsOptions
            {
                Retries = retries,
                ScanQueryType = ScanQueryType.Any,
                RetryDelay = TimeSpan.FromMilliseconds(retryWaitMs),
                ScanTime = TimeSpan.FromMilliseconds(waitMs)
            };
        }

        public async Task<NEEOBrain> DiscoverOneBrain()
        {
            var lookup = await ZeroconfResolver.BrowseDomainsAsync(_options);
          
            var found = lookup.FirstOrDefault((l) => l.Key.StartsWith(_neeoKey));
            if (found == null)
                return null;

            var brain = new NEEOBrain(); 
            var hosts = await ZeroconfResolver.ResolveAsync(found.Key);
            foreach (var host in hosts)
            {
                brain.IpArray = host.IPAddresses.ToList().ToArray();
                foreach (var service in host.Services)
                {
                    brain.Name = host.DisplayName;
                    brain.Host = brain.IpArray[0];
                    brain.Port = service.Value.Port;
                    foreach (var dict in service.Value.Properties)
                    {
                        brain.Version = dict["rel"];
                        brain.Region = dict["reg"];
                    }
                }
            }
            return brain;
        }
    }
}

