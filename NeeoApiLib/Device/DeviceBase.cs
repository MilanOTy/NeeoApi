using Home.Neeo.Device.Validation;
using System.Collections.Generic;

namespace Home.Neeo.Device
{
    public class DeviceBase
    {
        internal List<Parameter>                _sensors;
        internal List<Parameter>                _buttons;
        internal List<Parameter>                _sliders;
        internal List<Parameter>                _textLabels;
        internal List<Parameter>                _imageUrls;
        internal List<Parameter>                _switches;
        internal List<string>                   _deviceCapabilities;
        internal List<Controller>               _discovery;
        internal string                         _deviceIdentifier;
    }
}
