using System;
using System.Collections.Generic;

namespace Home.Neeo.Device.Validation
{
    public class DeviceType
    {
        public enum TYPE
        {
            UNKNOWN = 0,
            ACCESSOIRE,
            AVRECEIVER,
            DVB,
            DVD,
            GAMECONSOLE,
            LIGHT,
            MEDIAPLAYER,
            PROJECTOR,
            TV,
            VOD,
            AUDIO,
            SOUNDBAR,
            TUNER,
            THERMOSTAT,
            CLIMA,
            SONOS
        }
        static readonly List<TYPE> TYPES = new List<TYPE>
        {
          TYPE.ACCESSOIRE, TYPE.AVRECEIVER, TYPE.DVB, TYPE.DVD,
          TYPE.GAMECONSOLE, TYPE.LIGHT, TYPE.MEDIAPLAYER, TYPE.PROJECTOR, TYPE.TV, TYPE.VOD
        };

        static readonly List<TYPE> INPUTTYPES = new List<TYPE> { TYPE.AVRECEIVER, TYPE.TV, TYPE.PROJECTOR };
        static readonly List<TYPE> NOTIMINGTYPES = new List<TYPE> { TYPE.ACCESSOIRE, TYPE.LIGHT };
        internal static bool IsDefaultTYPE(TYPE type)
        {
            return TYPES.Contains(type);
        }
        internal static TYPE GetDeviceType(string type)
        {
            type = type.ToUpper();
            if (type == "ACCESSORY")
                return TYPE.ACCESSOIRE;
            TYPE deviceType;
            if (Enum.TryParse (type, out deviceType))
                return deviceType;
            throw new NEEOException("INVALID_DEVICETYPE");
        }
        internal static bool NeedsInputCommand(TYPE type)
        {
            return INPUTTYPES.Contains(type);
        }
        internal static bool DoesNotSupportTiming(TYPE type)
        {
            return NOTIMINGTYPES.Contains(type);
        }
    }
}
