using Home.Neeo.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Home.Neeo.Device
{
    public class Controller
    {
        public Controller (NEEOGetFunc getter, NEEOSetFunc setter = null)
        {
            Getter = getter;
            Setter = setter;
        }
        public Controller (NEEODiscoveryFunc discover)
        {
            Discover = discover;
        }
        public NEEOGetFunc          Getter  { get;  }
        public NEEOSetFunc          Setter  { get;  }
        public NEEODiscoveryFunc    Discover { get; }
    }
}
