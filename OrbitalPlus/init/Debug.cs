using System;
using System.Drawing;
using System.Text;

namespace Orbital.Init
{
    public static class Debug
    {
        public delegate void OnLogOutput(object output, Color color = default, bool newline = false);
        public static event OnLogOutput onLogOutput;

        public static void Log(object value, Color color = default, bool header = true)
        {
            if (value == null) return;
            if (header)
            {
                var time = DateTime.Now.ToString("[hh:mm:ss]");
                var date = DateTime.Now.ToString("[MM/dd/yy]");

                onLogOutput?.Invoke("[", Color.White);
                onLogOutput?.Invoke("Orbital+", Color.Cyan);
                onLogOutput?.Invoke("]", Color.White);
                onLogOutput?.Invoke($"{date}{time} ", Color.White);
            }
            onLogOutput?.Invoke($"{value}", color == default ? Color.White : color, true);

#if DEBUG
            if (header)
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
            Console.ForegroundColor = Utils.FromColor(color == default ? Color.White : color);
            Console.WriteLine($"{value}");
#endif
        }
        
            

    }
}