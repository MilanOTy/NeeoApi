using Newtonsoft.Json;

namespace Home.Neeo.Models
{
    public class NEEONotification
    {
        [JsonProperty("uniqueDeviceId")]
        public string UniqueDeviceId    { get; set; }
        [JsonProperty("component")]
        public string Component         { get; set; }
        [JsonProperty("value")]
        public object Value             { get; set; }
    }
}
