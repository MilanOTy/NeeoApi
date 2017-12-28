using Home.Neeo.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Neeo.Device.Validation
{
    public class ValidationModule
    {
        public static string GetUniqueName (string name, string uniqueString = null)
        {
            return UniqueName.CreateUniqueName (name, uniqueString);
        }
        public static string[] GetButtonGroup (BUTTONGROUP name)
        {
            var buttons = ButtonGroup.Get(name);
            var list = new List<string>();
            foreach (var button in buttons)
            {
                list.Add(NEEOButton.Get(button));
            }
            return list.ToArray();
        }

        public static string GetAnyIpAddress (string startsWith = null)
        {
            return null;
        }

        public static string ValidateCapability (string capability)
        {
            return Capability.GetCapability(capability);
        }

        public static DeviceType.TYPE GetDeviceType (string type)
        {
            return DeviceType.GetDeviceType(type);
        }

        public static bool DeviceTypeNeedsInputCommand (DeviceType.TYPE type)
        {
            return DeviceType.NeedsInputCommand(type);
        }

        public static bool DeviceTypeDoesNotSupportTiming (DeviceType.TYPE type)
        {
            return DeviceType.DoesNotSupportTiming(type);
        }
        public static bool HasNoInputButtonsDefined (IEnumerable<Parameter> buttons)
        {
            return InputMacroChecker.HasNoInputButtonsDefined(buttons);
        }
    }
}
