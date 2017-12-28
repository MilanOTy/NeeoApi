using Home.Neeo.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace Home.Neeo
{
    public class NEEOEnvironment
    {
        public static IRestClient           RestClient      { get; set; }
        public static ILogger               Logger          { get; set; }
        public static string                MachineName     { get; set; }
        public static bool                  IsSimulation    { get; set; }
    }
}
