using System;

namespace Orbital.Init
{
    public static class Debug
    {
        public static void Log(object value)
        {
            var time = DateTime.Now.ToString("[hh:mm:ss]");
            var date = DateTime.Now.ToString("[MM/dd/yy]");
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Orbital+");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]");
            Console.WriteLine($"{date}{time} {value}");
        }
    }
}