using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Home.Neeo.Device
{
    public enum ComponentType
    {
        TYPE_UNKNOWN = 0,
        TYPE_BUTTON = 1,
        TYPE_SWITCH = 2,
        TYPE_SLIDER = 3,
        TYPE_SENSOR = 4,
        TYPE_TEXTLABEL = 5,
        TYPE_IMAGEURL = 6,
        TYPE_DISCOVER_ROUTE = 7,
        TYPE_POWER_SENSOR = 8,
    }
    public class Component
    {
        private static List<string> _componentTypes = new List<string> { "unknown", "button", "switch", "slider", "sensor", "textlabel", "imageurl", "discover", "power" };

        [JsonIgnore]
        public ComponentType Type       { get; set; }
        [JsonProperty("type")]
        public string TypeString
        {
            get
            {
                return GetTypeString();
            }
            set
            {
                Type = GetType(value);
            }
        }
        [JsonProperty("name")]
        public string       Name        { get; }
        [JsonProperty("label")]
        public string       Label       { get; }
        [JsonProperty("path")]
        public string       Path        { get; }

        [JsonConstructor]
        public Component(string type, string name, string label, string path)
        {
            Type = GetType(type); Name = name; Label = label; Path = path;
        }
        public Component(ComponentType type, string name, string label, string path)
        {
            Type = type; Name = name; Label = label; Path = path;
        }
        public string GetTypeString()
        {
            return GetTypeString(Type);
        }
        public static string GetTypeString(ComponentType type)
        {
            return _componentTypes[(int)type];
        }
        public static ComponentType GetType (string typeString)
        {
            int idx = _componentTypes.IndexOf(typeString);
            return idx < 0 ? ComponentType.TYPE_UNKNOWN : (ComponentType)idx;
        } 
    }

    public class ComponentButton : Component
    {
        public ComponentButton(string name, string label, string path) : base(ComponentType.TYPE_BUTTON, name, label, path)
        {
        }
    }
    public class ComponentTextLabel : Component
    {
        public ComponentTextLabel(string name, string label, string path, string sensor = null) : base(ComponentType.TYPE_TEXTLABEL, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("sensor")]
        public string Sensor { get; }
    }
    public class ComponentImage : Component
    {
        public ComponentImage(string name, string label, string path, string size, string imageUrl, string sensor) :
            base(ComponentType.TYPE_IMAGEURL, name, label, path)
        {
            Size = size;
            ImageUrl = imageUrl;
            Sensor = sensor;
        }
        [JsonProperty("size")]
        public string Size { get; }
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; }
        [JsonProperty("sensor")]
        public string Sensor { get; }
    }
    public class ComponentSwitch : Component
    {
        public ComponentSwitch(string name, string label, string path, string sensor = null) : base(ComponentType.TYPE_SWITCH, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("sensor")]
        public string Sensor { get; }
    }
    public class ComponentSwitchSensor : Component
    {
        public ComponentSwitchSensor(string name, string label, string path, SensorBase sensor = null) : base(ComponentType.TYPE_SENSOR, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("sensor")]
        public SensorBase Sensor { get; }
    }
    public class ComponentRangeSlider : Component
    {
        public ComponentRangeSlider(string name, string label, string path, SensorRangeName sensor = null) : base(ComponentType.TYPE_SLIDER, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("slider")]
        public SensorRange Sensor { get; }
    }
    public class ComponentRangeSensor : Component
    {

        public ComponentRangeSensor(string name, string label, string path, SensorRange sensor = null) : base(ComponentType.TYPE_SENSOR, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("sensor")]
        public SensorRange Sensor { get; }
    }
    public class ComponentRangeSliderSensor : Component
    {
        public ComponentRangeSliderSensor(string name, string label, string path, SensorRange sensor = null) : base(ComponentType.TYPE_SENSOR, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("sensor")]
        public SensorRange Sensor { get; }
    }
    public class ComponentPowerSensor : Component
    {
        public ComponentPowerSensor(string name, string label, string path, SensorPower sensor = null) : base(ComponentType.TYPE_SENSOR, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("sensor")]
        public SensorPower Sensor { get; }
    }
    public class ComponentCustomSensor : Component
    {
        public ComponentCustomSensor(string name, string label, string path, SensorCustom sensor = null) : base(ComponentType.TYPE_SENSOR, name, label, path)
        {
            Sensor = sensor;
        }
        [JsonProperty("sensor")]
        public SensorCustom Sensor { get; }
    }
    public class ComponentDiscovery : Component
    {
        public ComponentDiscovery(string name, string path) : base(ComponentType.TYPE_DISCOVER_ROUTE, name, null, path)
        {
        }
    }

}
