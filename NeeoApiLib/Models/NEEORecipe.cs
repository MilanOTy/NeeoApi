using Home.Neeo.Device.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Threading.Tasks;

namespace Home.Neeo.Models
{
    public class NEEORecipe
    {
        public class DetailData
        {
            [JsonProperty("deviceName")]
            public string               DeviceName      { get; set; }
            [JsonProperty("rootName")]
            public string               RootName        { get; set; }
            [JsonProperty("model")]
            public string               Model           { get; set; }
            [JsonProperty("manufacturer")]
            public string               Manufacturer    { get; set; }
            [JsonProperty("deviceType")]
            [JsonConverter(typeof(StringEnumConverter))]
            public DeviceType.TYPE      DeviceType      { get; set; }
        }
        public class UrlData
        {
            [JsonProperty("identify")]
            public string               Identify        { get; set; }
            [JsonProperty("setPowerOn")]
            public string               SetPowerOn      { get; set; }
            [JsonProperty("setPowerOff")]
            public string               SetPowerOff     { get; set; }
            [JsonProperty("getPowerState")]
            public string               GetPowerState   { get; set; }

        }
        public class ActionData
        {
            public Func<Task<bool>>       Identify        { get; set; }
            public Func<Task<bool>>       PowerOn         { get; set; }
            public Func<Task<bool>>       PowerOff        { get; set; }
            public Func<Task<bool>>       GetPowerState   { get; set; }
        }
        [JsonProperty("type")]
        public string          Type            { get; set; }
        [JsonProperty("detail")]
        public DetailData      Detail          { get; set; }
        [JsonProperty("url")]
        public UrlData         Url             { get; set; }
        [JsonProperty("isCustom")]
        public bool            IsCustom        { get; set; }
        [JsonProperty("isPoweredOn")]
        public bool            IsPoweredOn     { get; set; }
        [JsonProperty("uid")]
        public string          UID             { get; set; }
        [JsonProperty("powerKey")]
        public string          PowerKey        { get; set; }
        [JsonIgnore]
        public  ActionData      Action          { get; set; }
    }
}
