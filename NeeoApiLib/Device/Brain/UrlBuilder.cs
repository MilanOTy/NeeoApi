using Home.Neeo.Models;
using System;


namespace Home.Neeo.Device.Brain
{
    internal class UrlBuilder
    {
        const string    PROTOCOL = "http://";

        public static string BuildBrainUrl (NEEOBrain brain, string baseUrl = null, int brainport = NEEOConf.DEFAULT_BRAIN_PORT)
        {
            if (brain == null)
            {
                throw new NEEOException("URLBUILDER_MISSING_PARAMETER_BRAIN");
            }
            if (baseUrl == null)
            {
                baseUrl = string.Empty;
            }
            if (brain.Host != null && brain.Port != 0)
            {
                return PROTOCOL + brain.Host + ':' + brain.Port.ToString() + baseUrl;
            }
            throw new NEEOException("URLBUILDER_INVALID_PARAMETER_BRAIN");
        }
        public static string BuildBrainUrl(string brain, string baseUrl = null, int brainport = NEEOConf.DEFAULT_BRAIN_PORT)
        {
            if (brain == null)
            {
                throw new NEEOException("URLBUILDER_MISSING_PARAMETER_BRAIN");
            }
            if (baseUrl == null)
            {
                baseUrl = string.Empty;
            }
            return PROTOCOL + brain + ':' + brainport + baseUrl;
        }

    }
}

