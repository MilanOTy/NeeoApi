using Home.Neeo.Device;
using System.Collections.Generic;

namespace Home.Neeo.Models
{
    public class NEEOBrain
    {
        public string               Name                    { get; set; }
        public string               Host                    { get; set; }
        public int                  Port                    { get; set; }
        public string               Version                 { get; set; }
        public string               Region                  { get; set; }
        public string[]             IpArray                 { get; set; }
    }   
}
