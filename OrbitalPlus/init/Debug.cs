using System;

namespace Orbital.Init
{
    public static class Debug
    {
        public static void Log(object value, ConsoleColor color = ConsoleColor.White, bool header = true)
        {
#if DEBUG
            if (value == null) return;
            if(header)
            {
                var time = DateTime.Now.ToString("[hh:mm:ss]");
                var date = DateTime.Now.ToString("[MM/dd/yy]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Orbital+");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("]");
                Console.Write($"{date}{time} ");
            }
            Console.ForegroundColor = color;
            Console.WriteLine($"{value}");
#endif
        }
    }
}