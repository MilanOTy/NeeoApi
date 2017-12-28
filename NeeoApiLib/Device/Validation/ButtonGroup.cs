using Home.Neeo.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Neeo.Device.Validation
{
    internal class ButtonGroup
    {
        static readonly Dictionary<BUTTONGROUP, BUTTON[]> _buttonGroups = new Dictionary<BUTTONGROUP, BUTTON[]> {
          { BUTTONGROUP.Color_Buttons,    new BUTTON[] {BUTTON.FUNCTION_RED, BUTTON.FUNCTION_GREEN, BUTTON.FUNCTION_YELLOW, BUTTON.FUNCTION_BLUE} },
          { BUTTONGROUP.Controlpad,       new BUTTON[] {BUTTON.CURSOR_ENTER, BUTTON.CURSOR_UP, BUTTON.CURSOR_DOWN, BUTTON.CURSOR_LEFT, BUTTON.CURSOR_RIGHT} },
          { BUTTONGROUP.Numpad,           new BUTTON[] {BUTTON.DIGIT_0, BUTTON.DIGIT_1, BUTTON.DIGIT_2, BUTTON.DIGIT_3, BUTTON.DIGIT_4, BUTTON.DIGIT_5, BUTTON.DIGIT_6, BUTTON.DIGIT_7, BUTTON.DIGIT_8, BUTTON.DIGIT_9} },
          { BUTTONGROUP.Power,            new BUTTON[] {BUTTON.POWER_ON, BUTTON.POWER_OFF} },
          { BUTTONGROUP.Channel_Zapper,   new BUTTON[] {BUTTON.CHANNEL_UP, BUTTON.CHANNEL_DOWN} },
          { BUTTONGROUP.Transport,        new BUTTON[] {BUTTON.PLAY, BUTTON.PAUSE, BUTTON.STOP} },
          { BUTTONGROUP.Transport_Search, new BUTTON[] {BUTTON.REVERSE, BUTTON.FORWARD} },
          { BUTTONGROUP.Transport_Scan,   new BUTTON[] {BUTTON.PREVIOUS, BUTTON.NEXT} },
          { BUTTONGROUP.Transport_Skip,   new BUTTON[] {BUTTON.SKIP_SECONDS_BACKWARD, BUTTON.SKIP_SECONDS_FORWARD} },
          { BUTTONGROUP.Language,         new BUTTON[] {BUTTON.SUBTITLE, BUTTON.LANGUAGE} },
          { BUTTONGROUP.Menu_and_Back,    new BUTTON[] {BUTTON.MENU, BUTTON.BACK} },
          { BUTTONGROUP.Volume,           new BUTTON[] {BUTTON.VOLUME_UP, BUTTON.VOLUME_DOWN, BUTTON.MUTE_TOGGLE} },
          { BUTTONGROUP.Record,           new BUTTON[] {BUTTON.MY_RECORDINGS, BUTTON.RECORD, BUTTON.LIVE} }
        };

        internal static BUTTON[] Get (BUTTONGROUP key)
        {
            BUTTON[] result = null;
            if (_buttonGroups.TryGetValue(key, out result))
                return result;
            return null;
        }
    };

}
