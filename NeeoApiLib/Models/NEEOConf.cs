using Home.Neeo.Device;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Neeo.Models
{
    public class NEEOConf
    {
        public const int DEFAULT_BRAIN_PORT = 3000;
        public const int MAXIMAL_CONNECTION_ATTEMPTS_TO_BRAIN = 8;
        public NEEOConf(string name, int port, List<DeviceBuilder> devices, NEEOBrain brain, string baseUrl = null, int brainPort = DEFAULT_BRAIN_PORT, int maxConnectionAttempts = MAXIMAL_CONNECTION_ATTEMPTS_TO_BRAIN)
        {
            Name = name;
            Port = port;
            Devices = devices;
            Brain = brain;
            BaseUrl = baseUrl;
            BrainPort = brainPort;
            MaxConnectionAttempts = maxConnectionAttempts;
        }
        public string               Name                    { get; set; }
        public string               BaseUrl                 { get; set; }
        public int                  BrainPort               { get; set; }
        public int                  Port                    { get; set; }
        public NEEOBrain            Brain                   { get; set; }
        public int                  MaxConnectionAttempts   { get; set; }
        public List<DeviceBuilder>  Devices                 { get; set; }
    }
}
