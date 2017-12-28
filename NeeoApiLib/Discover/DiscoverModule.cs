using Home.Neeo.Interfaces;
using Home.Neeo.Models;
using System.Threading.Tasks;

namespace Home.Neeo.Discover
{
    public class DiscoverModule : IDiscover
    {
        //return Task.FromResult(BuildNEEOBrainModel("MYNEO", "host", 4711, "1.0", "AUT", new string[] { "192.168.1.130" }));
        NEEOBrain               _brain;
        IDiscover               _discover;
        static DiscoverModule   _instance;

        public DiscoverModule(string name, string host, int port, string version, string region, string[] ipArray)
        {
            _brain = new NEEOBrain
            {
                Name = name,
                Host = host,
                Port = port,
                Version = version,
                Region = region,
                IpArray = ipArray
            };
            _instance = this;
        }
        public DiscoverModule(IDiscover discover)
        {
            _discover = discover;
            _instance = this;
        }
        public static DiscoverModule Instance => _instance;
        public async Task<NEEOBrain> DiscoverOneBrain ()
        {
            if (_brain == null)
            {
                if (_discover == null)
                {
                    throw new NEEOException("brain or discover must be defined");
                }
                _brain = await _discover.DiscoverOneBrain();
            }
            return _brain;
        }
    }
}

