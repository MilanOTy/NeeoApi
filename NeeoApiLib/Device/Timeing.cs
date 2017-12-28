using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Neeo.Device
{
    public class Timeing
    {
        public Timeing (int powerOnDelayMs = 2000, int sourceSwitchDelayMs=  500, int shutdownDelayMs = 1000)
        {
            PowerOnDelayMs = powerOnDelayMs;
            SourceSwitchDelayMs = sourceSwitchDelayMs;
            ShutdownDelayMs = shutdownDelayMs;
        }
        [JsonProperty("powerOnDelayMs")]
        public int PowerOnDelayMs       { get;  }
        [JsonProperty("sourceSwitchDelayMs")]
        public int SourceSwitchDelayMs  { get; }
        [JsonProperty("shutdownDelayMs")]
        public int ShutdownDelayMs      { get; }
    }
}
