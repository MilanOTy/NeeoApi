using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Home.Neeo.Models
{
    public class NEEODiscoveredDevice
    {
        [JsonProperty("id")]
        public string   Id          { get; set; }
        [JsonProperty("name")]
        public string   Name        { get; set; }
        [JsonProperty("reachable")]
        public bool     Reachable   { get; set; }
    }
    public class NEEOOptionalCallbacks
    {
        public NEEOOptionalCallbacks(NEEOPowerNotificationFunc powOn, NEEOPowerNotificationFunc powOff)
        {
            PowerOnNotificationFunction = powOn;
            PowerOffNotificationFunction = powOff;
        }
        public NEEOPowerNotificationFunc PowerOnNotificationFunction { get; }
        public NEEOPowerNotificationFunc PowerOffNotificationFunction { get; }
    }
    public delegate Task                            NEEONotificationFunc            (NEEONotification msg);
    public delegate Task                            NEEOPowerNotificationFunc       (string deviceId);
    public delegate void                            NEEORegisterSubscriptionFunc    (NEEONotificationFunc notificationFunc, NEEOOptionalCallbacks optionalCallbacks = null);
    public delegate Task<bool>                      NEEOInitializeFunc              ();
    public delegate Task<object>                    NEEOGetFunc                     (string id);
    public delegate Task                            NEEOSetFunc                     (string id, object value);
    public delegate Task                            NEEOButtonHandlerFunc           (string buttonName, string id);
    public delegate Task<NEEODiscoveredDevice[]>    NEEODiscoveryFunc               ();
}
