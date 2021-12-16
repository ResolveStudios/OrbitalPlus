using System;

namespace Orbital
{
    public static class Errors
    {
        public static bool has { get; private set; }

        public static int NoToken => -27;
        public static int NoVRCInfo => -30;

        internal static string GetReason(int code)
        {
            switch(code)
            {
                case -27: return "No discord bot token found in your settings file.";
                case -30: return "No vrchat data was found in your settings file.";

                default: return "Ok!";
            }
        }

        internal static void SetError() => has = true;
    }
}