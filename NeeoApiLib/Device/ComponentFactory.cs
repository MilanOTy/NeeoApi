using System;
using System.Collections.Generic;

namespace Home.Neeo.Device
{
    internal class ComponentFactory
    {

        const string SENSOR_SUFFIX = "_SENSOR";

        const string DEFAULT_SLIDER_UNIT = "%";

        static readonly List<string> VALID_IMAGEURL_SIZES = new List<string> { "small", "large" };

        static readonly double[] DEFAULT_SLIDER_RANGE = new double[] { 0, 100 };
        static private void ValidateParameter(string pathPrefix, string name)
        {
            if (pathPrefix == null)
            {
                throw new NEEOException("INVALID_PATHPREFIX");
            }
            if (name == null)
            {
                throw new NEEOException("INVALID_BUILD_PARAMETER");
            }
        }
        static private void ValidateImageSize(string size)
        {
            if (!VALID_IMAGEURL_SIZES.Contains(size))
            {
                throw new NEEOException("INVALID_IMAGEURL_SIZE");
            }
        }
        static private string BuildSensorName(string name)
        {
            return name.ToUpper() + SENSOR_SUFFIX;
        }
        static internal string BuildPathPrefix(string adaptername, string deviceuid)
        {
            return $"/device/{deviceuid}/";
        }
        static internal ComponentButton BuildButton(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var path = pathPrefix + name;
            var button = new ComponentButton(name, param.Label ?? name, path);
            return button;
        }
        static internal ComponentSwitch BuildSwitch(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var path = pathPrefix + name;
            var swi = new ComponentSwitch(name, param.Label, path, BuildSensorName(name));
            return swi;
        }
        static internal ComponentSwitchSensor BuildSwitchSensor(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var sensorName = BuildSensorName(name);
            var path = pathPrefix + sensorName;
            var swi = new ComponentSwitchSensor(sensorName, param.Label, path, new SensorBinary());
            return swi;
        }
        static internal ComponentRangeSliderSensor BuildRangeSliderSensor(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var sensorName = BuildSensorName(name);
            var path = pathPrefix + sensorName;
            var unit = param.Unit != null ? Uri.EscapeDataString(param.Unit) : DEFAULT_SLIDER_UNIT;
            var sli = new ComponentRangeSliderSensor(sensorName, param.Label, path, new SensorRange(param.RangeLow, param.RangeHigh, unit));
            return sli;
        }
        static private ComponentRangeSensor BuildRangeSensor(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var path = pathPrefix + name;
            var unit = param.Unit != null ? Uri.EscapeDataString(param.Unit) : DEFAULT_SLIDER_UNIT;
            var rs = new ComponentRangeSensor(name, param.Label ?? param.Name, path, new SensorRange (param.RangeLow, param.RangeHigh, unit));
            return rs;
        }
        static private ComponentPowerSensor BuildPowerSensor(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var path = pathPrefix + name;
            var pow = new ComponentPowerSensor(name, param.Label ?? param.Name, path, new SensorPower());
            return pow;
        }

        static internal Component BuildSensor(string pathPrefix, Parameter param)
        {
            if (param.Type == ComponentType.TYPE_POWER_SENSOR)
            {
                return BuildPowerSensor(pathPrefix, param);
            }
            return BuildRangeSensor(pathPrefix, param);
        }

        static internal ComponentRangeSlider BuildRangeSlider(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var path = pathPrefix + name;
            var unit = param.Unit != null ? Uri.EscapeDataString(param.Unit) : DEFAULT_SLIDER_UNIT;
            var sli = new ComponentRangeSlider(name, param.Label ?? param.Name, path, new SensorRangeName(param.RangeLow, param.RangeHigh, unit, BuildSensorName(name)));
            return sli;
        }
        static internal ComponentTextLabel BuildTextLabel(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var path = pathPrefix + name;
            var textLabel = new ComponentTextLabel(name, param.Label ?? name, path, BuildSensorName(name));
            return textLabel;
        }
        static internal ComponentImage BuildImageUrl(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var path = pathPrefix + name;
            var size = param.Size == null ? "large" : param.Size;
            ValidateImageSize(size);
            var image = new ComponentImage(name, param.Label ?? name, path, size, param.ImageUrl, BuildSensorName(name));
            return image;
        }
        static internal ComponentCustomSensor BuildCustomSensor(string pathPrefix, Parameter param)
        {
            ValidateParameter(pathPrefix, param.Name);
            var name = Uri.EscapeDataString(param.Name);
            var sensorName = BuildSensorName(name);
            var path = pathPrefix + sensorName;
            var custom = new ComponentCustomSensor(sensorName, param.Label, path, new SensorCustom());
            return custom;
        }
        static internal ComponentDiscovery BuildDiscovery(string pathPrefix)
        {
            ValidateParameter(pathPrefix, "dummy");
            var path = pathPrefix + Component.GetTypeString(ComponentType.TYPE_DISCOVER_ROUTE);
            var disco = new ComponentDiscovery (Component.GetTypeString(ComponentType.TYPE_DISCOVER_ROUTE), path);
            return disco;
        }
    }
}
