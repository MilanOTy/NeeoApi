using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Neeo.Models
{
    public enum BUTTON
    {
        UNKNOWN = 0,
        BACK,
        CHANNEL_UP,
        CHANNEL_DOWN,
        CURSOR_UP,
        CURSOR_DOWN,
        CURSOR_ENTER,
        CURSOR_LEFT,
        CURSOR_RIGHT,
        DIGIT_0,
        DIGIT_1,
        DIGIT_2,
        DIGIT_3,
        DIGIT_4,
        DIGIT_5,
        DIGIT_6,
        DIGIT_7,
        DIGIT_8,
        DIGIT_9,
        ENTER,
        EXIT,
        FORWARD,
        FUNCTION_RED,
        FUNCTION_GREEN,
        FUNCTION_YELLOW,
        FUNCTION_BLUE,
        GUIDE,
        INFO,
        HOME,
        LANGUAGE,
        LIVE,
        MENU,
        MUTE_TOGGLE,
        MY_RECORDINGS,
        NEXT,
        PAUSE,
        PLAY,
        POWER_OFF,
        POWER_ON,
        POWER_TOGGLE,
        PREVIOUS,
        REVERSE,
        RECORD,
        SKIP_BACKWARD,
        SKIP_FORWARD,
        SKIP_SECONDS_BACKWARD,
        SKIP_SECONDS_FORWARD,
        STOP,
        SUBTITLE,
        VOLUME_DOWN,
        VOLUME_UP
    }
    public enum BUTTONGROUP
    {
        Color_Buttons,  
        Controlpad,     
        Numpad,         
        Power,          
        Channel_Zapper, 
        Transport,      
        Transport_Search,
        Transport_Scan, 
        Transport_Skip, 
        Language,       
        Menu_and_Back,  
        Volume,         
        Record
    }
    public class NEEOButton
    {
        public static string Get (BUTTON button)
        {
            return button.ToString().Replace('_', ' ');
        }
        public static string Get(BUTTONGROUP buttongroup)
        {
            return buttongroup.ToString().Replace('_', ' ');
        }
    }
}
