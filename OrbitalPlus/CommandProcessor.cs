using Orbital.Init;
using System.Drawing;

namespace Orbital
{
    public class CommandProcessor
    {
        public static void Process(object sender, string cmd)
        {
            Debug.Log($"Processing command from {sender}", Color.LightBlue);
        }
    }
}
