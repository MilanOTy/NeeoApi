using System;
using System.Collections.Generic;

namespace Home.Neeo.Device.Validation
{
    public class Capability
    {
        public static List<string> CAPABILITY = new List<string> { "alwaysOn" };
        internal static string GetCapability (string capability)
        {
            if (CAPABILITY.Contains(capability))
                return capability;
            throw new NEEOException("INVALID CAPABILITY");
        }
    }
}
