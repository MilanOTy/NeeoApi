using Newtonsoft.Json;
using System.Collections.Generic;

namespace Home.Neeo.Device
{
    public enum SensorType
    {
        TYPE_SENSOR_UNKNOWN = 0,
        TYPE_SENSOR_CUSTOM = 1,
        TYPE_SENSOR_RANGE = 2,
        TYPE_SLIDER_RANGE = 3,
        TYPE_SENSOR_BINARY = 4,
        TYPE_SENSOR_POWER = 5
    }
    public class SensorBase
    {
        private static List<string> _sensorTypes = new List<string> { "unknown", "custom", "range", "range", "binary", "power" };
        [JsonConstructor]
        public SensorBase(string type)
        {
            Type = GetType(type);
        }
        public SensorBase(SensorType type)
        {
            Type = type;
        }
        [JsonIgnore]
        public SensorType Type { get; set; }
        [JsonProperty("type")]
        public string TypeString { get => GetTypeString(); set => Type = GetType(value); }
        public string GetTypeString()
        {
            return _sensorTypes[(int)Type];
        }
        public static SensorType GetType(string typeString)
        {
            int idx = _sensorTypes.IndexOf(typeString);
            return idx < 0 ? SensorType.TYPE_SENSOR_UNKNOWN : (SensorType)idx;
        }
    }
    public class SensorBinary : SensorBase
    {
        public SensorBinary() : base(SensorType.TYPE_SENSOR_BINARY)
        {
        }
    }
    public class SensorPower : SensorBase
    {
        public SensorPower() : base(SensorType.TYPE_SENSOR_POWER)
        {
        }
    }
    public class SensorRange : SensorBase
    {
        public SensorRange(double rangeLow, double rangeHigh, string unit) : base(SensorType.TYPE_SENSOR_RANGE)
        {
            RangeLow = rangeLow;
            RangeHigh = rangeHigh;
            Unit = unit;
        }
        [JsonIgnore]
        public double RangeLow { get; }
        [JsonIgnore]
        public double RangeHigh { get; }
        [JsonProperty("range")]
        public double[] Range { get => new double[2] { RangeLow, RangeHigh }; }
        [JsonProperty("unit")]
        public string Unit { get; }
    }
    public class SensorRangeName : SensorRange
    {
        public SensorRangeName(double rangeLow, double rangeHigh, string unit, string sensorName) : base(rangeLow,  rangeHigh, unit)
        {
            Sensor = sensorName;
        }
        [JsonProperty("sensor")]
        public string Sensor { get; }
    }
    public class SensorCustom : SensorBase
    {
        public SensorCustom() : base(SensorType.TYPE_SENSOR_CUSTOM)
        {
        }
    }
}
