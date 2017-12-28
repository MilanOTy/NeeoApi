using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Neeo
{
    public class NEEOUrls
    {
        public const string BASE_URL_NOTIFICATIONS          = "/v1/notifications/";
        public const string BASE_URL_NOTIFICATIONKEY        = "/v1/api/notificationkey/";
        public const string BASE_URL_REGISTER_SDK_ADAPTER   = "/v1/api/registerSdkDeviceAdapter";
        public const string BASE_URL_UNREGISTER_SDK_ADAPTER = "/v1/api/unregisterSdkDeviceAdapter";

        public const string BASE_URL_GETRECIPES             = "/v1/api/recipes";
        public const string BASE_URL_GETACTIVERECIPES       = "/v1/api/activeRecipes";

        // Get the brain configuration:
        public const string BASE_URL_GETBRAINCONFIGURATION  = "/v1/projects/home/";

        // Get all rooms and it's child configurations.
        public const string BASE_URL_GETALLROOMS            = "/v1/projects/home/rooms/";

        //Get a specific room and it's child configurations.
        public const string BASE_URL_GETROOM                = "/v1/projects/home/rooms/{0}/";

        //Get all devices from a specific room and it's child configurations.
        public const string BASE_URL_GETROOMEVICES          = "/v1/projects/home/rooms/{0}/devices/";

        //Get a specific device and it's child configurations.
        public const string BASE_URL_GETROOMDEVICE          = "/v1/projects/home/rooms/{0}/devices/{1}/";

        //Get all macros from a specific device.
        public const string BASE_URL_GETROOMDEVICEMACROS    = "/v1/projects/home/rooms/{0}/devices/{1}/macros";

        //Trigger a Macro (Push a button).
        public const string BASE_URL_TRIGGERROOMDEVICEMACRO = "/v1/projects/home/rooms/{0}/devices/{1}/macros/{2}/trigger";

        // trigger a recipe.
        public const string BASE_URL_TRIGGERROOMDEVICERECIPE ="/v1/projects/home/rooms/{0}/devices/{1}/recipes/{2}/trigger";

        // power off a Scenario.
        public const string BASE_URL_POWEROFFSZENARIO       = "/v1/projects/home/rooms/{0}/scenarios/{1}/poweroff";

        // Start favourite Channel 3.
        public const string BASE_URL_STARTVAFVORITECHANNEL  = "/v1/projects/home/rooms/{0}/devices/{1}/favorites/{2}/trigger";


        // Set a slider to value 24. (include application/json as contenttype) - POST { content : value }
        public const string BASE_URL_SETSLIDERVALUEL        = "/v1/projects/home/rooms/{0}/devices/{1}/sliders/{2}/";

        // Set a switch state.
        public const string BASE_URL_SETSWITCH_ON           = "v1/projects/home/rooms/{0}/devices/{1}/switches/{2}/on";
        public const string BASE_URL_SETSWITCH_OFF          = "v1/projects/home/rooms/{0}/devices/{1}/switches/{2}/off";

        // Get system info.
        public const string BASE_URL_GETSYSTEMINFO          = "/v1/systeminfo/";

        // Blink NEEO Brain LED
        public const string BASE_URL_BLINKLED               = "/v1/systeminfo/identbrain";

        // Is recipe Active?
        public const string BASE_URL_ISRECEIPEACTIVE        = "/v1/projects/home/rooms/{0}/recipes/{1}/isactive";

    }
}
