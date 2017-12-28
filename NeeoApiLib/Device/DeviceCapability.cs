using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Neeo.Device
{
    internal class DeviceCapability : DeviceBase
    {
        internal string                                 AdapterName;
        internal List<Component>                        Capabilities;
        internal Dictionary<string, HandlerParameter>   HandlerMap;

        private ILogger                     _logger;
        internal DeviceCapability(DeviceBase data, string adapterName, ILogger logger)
        {
            _buttons = data._buttons;
            _sliders = data._sliders;
            _switches = data._switches;
            _textLabels = data._textLabels;
            _imageUrls = data._imageUrls;
            _sensors = data._sensors;
            _discovery = data._discovery;
            _deviceIdentifier = data._deviceIdentifier;
            _logger = logger;
            AdapterName = adapterName;
            Capabilities = new List<Component>();
            HandlerMap = new Dictionary<string, HandlerParameter>();
        }

        private bool IsUniquePath (string path)
        {
            var component = Capabilities.FirstOrDefault((c) => c.Path == path);
            return component == null;
        }
        private void AddCapability (Component capability, Controller controller)
        {
            _logger.LogDebug($"register capability {capability.Type}, {capability.Path}");
            if (IsUniquePath(capability.Path))
            {
                Capabilities.Add(capability);
                string name = Uri.UnescapeDataString(capability.Name);
                HandlerMap[name] = new HandlerParameter(capability.Type, controller);
            }
            else
            {
                _logger.LogError($"path is not unique {capability.Name}, {capability.Type}, {capability.Path}");
                throw new NEEOException("DUPLICATE_PATH_DETECTED");
            }
        }
        private void AddRouteHandler(Component capability, Controller controller)
        {
            _logger.LogDebug($"register route {capability.Type}, {capability.Path}");
            if (IsUniquePath(capability.Path))
            {
                string name = Uri.UnescapeDataString(capability.Name);
                HandlerMap[name] = new HandlerParameter(capability.Type, controller);
            }
            else
            {
                _logger.LogError($"path is not unique {capability.Name}, {capability.Type}, {capability.Path}");
                throw new NEEOException("DUPLICATE_PATH_DETECTED");
            }
        }
        private void build()
        {
            var pathPrefix = ComponentFactory.BuildPathPrefix(AdapterName, _deviceIdentifier);

            foreach (var button in _buttons)
            {
                var actor = ComponentFactory.BuildButton(pathPrefix, button);
                AddCapability(actor, button.Controller);
            }
            foreach (var slider in _sliders)
            {
                var sensor = ComponentFactory.BuildRangeSliderSensor(pathPrefix, slider);
                AddCapability(sensor, slider.Controller);
                var actor = ComponentFactory.BuildRangeSlider(pathPrefix, slider);
                AddCapability(actor, slider.Controller);
            }
            foreach (var swi in _switches)
            {
                var sensor = ComponentFactory.BuildSwitchSensor(pathPrefix, swi);
                AddCapability(sensor, swi.Controller);
                var actor = ComponentFactory.BuildSwitch(pathPrefix, swi);
                AddCapability(actor, swi.Controller);
            }
            foreach (var label in _textLabels)
            {
                var sensor = ComponentFactory.BuildCustomSensor(pathPrefix, label);
                AddCapability(sensor, label.Controller);
                var actor = ComponentFactory.BuildTextLabel(pathPrefix, label);
                AddCapability(actor, label.Controller);
            }
            foreach (var img in _imageUrls)
            {
                var sensor = ComponentFactory.BuildCustomSensor(pathPrefix, img);
                AddCapability(sensor, img.Controller);
                var actor = ComponentFactory.BuildImageUrl(pathPrefix, img);
                AddCapability(actor, img.Controller);
            }
            foreach (var s in _sensors)
            {
                var sensor = ComponentFactory.BuildSensor(pathPrefix, s);
                AddCapability(sensor, s.Controller);
            }
            foreach (var disco in _discovery)
            {
                var element = ComponentFactory.BuildDiscovery(pathPrefix);
                this.AddRouteHandler(element, disco);
            }
        }
        internal static DeviceCapability Build (DeviceBase data, string adapterName, ILogger logger)
        {
            var capability = new DeviceCapability(data, adapterName, logger);
            capability.build();
            return capability;
        }   
    }
}
