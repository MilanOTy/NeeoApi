using Home.Neeo.Device;
using Home.Neeo.Device.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Home.Neeo.Models
{
    public class NEEODevice
    {
        public class DevicePar
        {
            public DevicePar (DeviceType.TYPE type, string manufacturer, string name, string[] tokens)
            {
                Type = type;
                Manufacturer = manufacturer;
                Name = name;
                Tokens = tokens;
            }
            [JsonProperty("name")]
            public string           Name            { get; }
            [JsonProperty("tokens")]
            public string[]         Tokens          { get; }
            //[JsonProperty("type")]
            //[JsonConverter(typeof(StringEnumConverter))]
            [JsonIgnore]
            public DeviceType.TYPE  Type            { get; }
            //[JsonProperty("manufacturer")]
            [JsonIgnore]
            public string           Manufacturer    { get; }
        }
        public class SetupPar
        {
            public SetupPar (string introheader, string introText)
            {
                Discovery = true;
                Registration = false;
                IntroHeader = introheader;
                IntroText = IntroText;
            }
            [JsonProperty("discovery")]
            public bool             Discovery       { get; }
            [JsonProperty("registration")]
            public bool             Registration    { get; }
            [JsonProperty("introHeader")]
            public string           IntroHeader     { get; }
            [JsonProperty("introText")]
            public string           IntroText       { get; }
        }

        public string                       AdapterName             { get; set; }
        public string                       ApiVersion              { get; set; }
        public string                       Manufacturer            { get; set; }
        public DeviceType.TYPE              Type                    { get; set; }
        public DevicePar[]                  Devices                 { get; set; }
        public Component[]                  Capabilities            { get; set; }
        public string[]                     DeviceCapabilities      { get; set; }
        public Dictionary<string, HandlerParameter>  Handler        { get; set; }         
        public NEEORegisterSubscriptionFunc SubscriptionFunction    { get; set; }
        public NEEOInitializeFunc           InitializeFunction      { get; set; }
        public Timeing                      Timing                  { get; set; }
        public SetupPar                     Setup                   { get; set; }
    }
}