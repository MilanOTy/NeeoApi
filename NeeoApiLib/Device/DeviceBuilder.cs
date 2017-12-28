using Home.Neeo.Device.Validation;
using Home.Neeo.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Home.Neeo.Device
{
    public class DeviceBuilder : DeviceBase
    {
        const string                DEFAULT_MANUFACTURER    = "NEEO";
        const DeviceType.TYPE       DEFAULT_TYPE            = DeviceType.TYPE.ACCESSOIRE;
        const string                API_VERSION             = "1.0";
        const int                   MAXIMAL_TIMING_VALUE_MS = 60 * 1000;

        private ILogger                         _logger;
        private string                          _manufacturer;
        private DeviceType.TYPE                 _type;
        private string                          _devicename;
        private List<string>                    _additionalSearchTokens;
        private NEEOButtonHandlerFunc           _buttonHandler;
        internal bool                           _hasPowerStateSensor;
        private Timeing                         _timeing;
        private NEEODevice.SetupPar             _setup;
        private NEEORegisterSubscriptionFunc    _subscriptionFunction;
        private NEEOInitializeFunc              _initializeFunction;

        internal DeviceBuilder(string name, string uniqueString, ILogger logger)
        {
            _logger = logger;

            _deviceIdentifier = "apt-" + ValidationModule.GetUniqueName(name, uniqueString);
            _manufacturer = DEFAULT_MANUFACTURER;
            _type = DEFAULT_TYPE;
            _devicename = name;
            _additionalSearchTokens = new List<string>();
            _buttons = new List<Parameter>();
            _sensors = new List<Parameter>();
            _sliders = new List<Parameter>();
            _switches = new List<Parameter>();
            _textLabels = new List<Parameter>();
            _imageUrls = new List<Parameter>();
            _discovery = new List<Controller>();
            _deviceCapabilities = new List<string>();
        }


        public DeviceBuilder SetManufacturer(string value)
        {
            _manufacturer = value; return this;
        }
        public DeviceBuilder SetType(DeviceType.TYPE type)
        {
            _type = type; return this;
        }
        public DeviceBuilder AddAdditionalSearchToken(string value)
        {
            _additionalSearchTokens.Add(value); return this;
        }
        public DeviceBuilder AddButton(string name, string label, NEEOGetFunc function = null)
        {
            Parameter par = new Parameter(ComponentType.TYPE_BUTTON, name, label, function == null ? null : new Controller(function));
            CheckParamName(par);
            _logger.LogDebug($"Add button {par.Name}");
            _buttons.Add(par);
            return this;
        }
        public DeviceBuilder AddButton(BUTTON button, string label, NEEOGetFunc function = null)
        {
            return AddButton(NEEOButton.Get(button), label, function);
        }
        public DeviceBuilder AddButtonGroup(BUTTONGROUP name)
        {
            _logger.LogDebug($"Add buttongroup {name}");
            var buttonGroup = ValidationModule.GetButtonGroup(name);
            if (buttonGroup != null)
            {
                foreach (string button in buttonGroup)
                {
                    _buttons.Add(new Parameter(ComponentType.TYPE_BUTTON, button, null));
                }
            }
            return this;
        }
        public DeviceBuilder AddButtonHandler(NEEOButtonHandlerFunc buttonHandler)
        {
            _logger.LogDebug("add buttonhandler");
            if (_buttonHandler != null)
            {
                throw new NEEOException("BUTTONHANDLER_ALREADY_DEFINED");
            }
            _buttonHandler = buttonHandler;
            return this;
        }
        public DeviceBuilder AddSlider(string name, string label, double rangeLow, double rangeHigh, string unit, NEEOGetFunc getterFunc, NEEOSetFunc setterFunc)
        {
            Parameter param = new Parameter(ComponentType.TYPE_SLIDER, name, rangeLow, rangeHigh, unit, label, new Controller(getterFunc, setterFunc));
            CheckParamName(param);
            _logger.LogDebug($"Add slider {param.Name}");
            _sliders.Add(param);
            return this;
        }
        public DeviceBuilder AddSensor(string name, string label, double rangeLow, double rangeHigh, string unit, NEEOGetFunc getterFunc)
        {
            Parameter param = new Parameter(ComponentType.TYPE_SLIDER, name, rangeLow, rangeHigh, unit, label, new Controller(getterFunc));
            CheckParamName(param);
            _logger.LogDebug($"Add sensor {param.Name}");
            _sensors.Add(param);
            return this;
        }
        public DeviceBuilder AddPowerStateSensor (NEEOGetFunc getterFunc)
        {
            _logger.LogDebug("add power sensor");
            var par = new Parameter(ComponentType.TYPE_POWER_SENSOR, "powerstate", "PowerState", new Controller(getterFunc));
            _sensors.Add(par);
            _hasPowerStateSensor = true;
            return this;
        }
        public DeviceBuilder AddSwitch(string name, string label, NEEOGetFunc getterFunc, NEEOSetFunc setterFunc)
        {
            Parameter param = new Parameter(ComponentType.TYPE_SWITCH, name, label, new Controller(getterFunc, setterFunc));
            CheckParamName(param);
            _logger.LogDebug($"Add switch {param.Name}");
            _switches.Add(param);
            return this;
        }
        public DeviceBuilder AddTextLabel(string name, string label, NEEOGetFunc getterFunc)
        {
            Parameter param = new Parameter(ComponentType.TYPE_TEXTLABEL, name, label, new Controller(getterFunc));
            CheckParamName(param);
            _logger.LogDebug($"Add textlabel {param.Name}");
            _textLabels.Add(param);
            return this;
        }
        public DeviceBuilder AddImageUrl(string name, string label, string size, string imageUrl, NEEOGetFunc getterFunc)
        {
            Parameter param = new Parameter(ComponentType.TYPE_IMAGEURL, name, label, size, imageUrl, new Controller(getterFunc));
            CheckParamName(param);
            _logger.LogDebug($"Add imageUrl {param.Name}");
            _imageUrls.Add(param);
            return this;
        }
        public DeviceBuilder AddCapability (string capability)
        {
            _logger.LogDebug($"Add capability {capability}");
            string cap = ValidationModule.ValidateCapability(capability);
            _deviceCapabilities.Add(cap);
            return this;
        }
        public NEEODevice Build(string adapterName)
        {
            if (adapterName == null)
            {
                throw new NEEOException("MISSING_ADAPTERNAME");
            }
            if (_buttons.Count > 0 && _buttonHandler == null)
            {
                _logger.LogError("BUTTONS_DEFINED_BUT_NO_BUTTONHANDLER_DEFINED");
                //throw new NEEOException("BUTTONS_DEFINED_BUT_NO_BUTTONHANDLER_DEFINED");
            }
            if (_timeing != null && ValidationModule.DeviceTypeDoesNotSupportTiming(_type))
            {
                throw new NEEOException("TIMING_DEFINED_BUT_DEVICETYPE_HAS_NO_SUPPORT");
            }

            foreach (var button in _buttons)
            {
                if (_buttonHandler != null)
                {
                    button.Controller = new Controller(async (deviceId) => {
                        await _buttonHandler(button.Name, deviceId);
                        return (object)true;
                    });
                }
            }

            var deviceCapability = DeviceCapability.Build(this, adapterName, _logger);
            var capabilities = deviceCapability.Capabilities;
            var handler = deviceCapability.HandlerMap;

            if (capabilities.Count == 0)
            {
                throw new NEEOException("INVALID_DEVICE_DESCRIPTION_NO_CAPABILITIES");
            }

            if (ValidationModule.DeviceTypeNeedsInputCommand(_type) && ValidationModule.HasNoInputButtonsDefined(_buttons))
            {
                _logger.LogWarning("\nWARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING!");
                _logger.LogWarning("WARNING: no input commands defined! Your device might not work as desired, check the docs");
            }

            NEEODevice device = new NEEODevice
            {
                AdapterName = _deviceIdentifier,
                ApiVersion = API_VERSION,
                Type = _type,
                Manufacturer = _manufacturer,
                Setup = _setup,
                Devices = new NEEODevice.DevicePar[] { new NEEODevice.DevicePar(_type, _manufacturer, _devicename, _additionalSearchTokens.ToArray()) },
                Capabilities = capabilities.ToArray(),
                DeviceCapabilities = _deviceCapabilities.ToArray(),
                Handler = handler,
                SubscriptionFunction = _subscriptionFunction,
                InitializeFunction = _initializeFunction,
                Timing = _timeing
            };
            return device;
        }
        public DeviceBuilder EnableDiscovery(string headerText, string description, NEEODiscoveryFunc controller)
        {
            _logger.LogDebug ("enable discovery ");
            if (headerText == null || description == null)
            {
                throw new NEEOException("INVALID_DISCOVERY_PARAMETER");
            }
            if (_setup != null && _setup.Discovery)
            {
                throw new NEEOException("DISCOVERHANLDER_ALREADY_DEFINED");
            }
            _setup = new NEEODevice.SetupPar(headerText, description);
            _discovery.Add(new Controller (controller));
            return this;
        }

        public bool SupportsTiming()
        {
            return !ValidationModule.DeviceTypeDoesNotSupportTiming(_type);
        }
        public DeviceBuilder DefineTimeing(int powerOnDelayMs, int sourceSwitchDelayMs, int shutdownDelayMs)
        {
            _logger.LogDebug($"define timing {powerOnDelayMs}, {sourceSwitchDelayMs}, {shutdownDelayMs}");

            _timeing = new Timeing (ValidateTime(powerOnDelayMs), ValidateTime(sourceSwitchDelayMs), ValidateTime(shutdownDelayMs));
            return this;

        }
        public DeviceBuilder RegisterSubscriptionFunction(NEEORegisterSubscriptionFunc controller)
        {
            _logger.LogDebug("Register subsription function");
            if (_subscriptionFunction != null)
            {
                throw new NEEOException ("SUBSCRIPTIONHANDLER_ALREADY_DEFINED");
            }
            _subscriptionFunction = controller;
            return this;
        }
        public DeviceBuilder RegisterInitializeFunction(NEEOInitializeFunc controller)
        {
            _logger.LogDebug("Register initialize function");
            if (_initializeFunction != null)
            {
                throw new NEEOException("INITIALISATION_FUNCTION_ALREADY_DEFINED");
            }
            _initializeFunction = controller;
            return this;
        }


        private void CheckParamName(Parameter param)
        {
            if (param == null || param.Name == null)
            {
                throw new NEEOException("MISSING_ELEMENT_NAME");
            }
        }
        private int ValidateTime (int timeMs)
        {
            if (timeMs < 0 || timeMs > MAXIMAL_TIMING_VALUE_MS)
            {
                throw new NEEOException("INVALID_TIMING_VALUE");
            }
            return timeMs;
        }

    }
}
