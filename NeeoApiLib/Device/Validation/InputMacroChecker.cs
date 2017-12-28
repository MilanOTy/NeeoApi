using System.Collections.Generic;
using System.Linq;

namespace Home.Neeo.Device.Validation
{
    internal class InputMacroChecker
    {
        internal static bool HasNoInputButtonsDefined(IEnumerable<Parameter> buttons)
        {
            if (buttons == null)
                throw new NEEOException("NO BUTTONS ARGUMENT");
            var result = buttons.FirstOrDefault((element) => {
                if (element == null || element.Name == null)
                {
                    return false;
                }
                return element.Name.StartsWith("INPUT");
            });
            return result == null;
        }
    }
}
