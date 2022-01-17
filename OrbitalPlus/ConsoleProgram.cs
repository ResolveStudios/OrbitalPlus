using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;
using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Orbital
{
    public class ConsoleProgram
    {
        public static DateTime preStartUpTime;
        public enum StateEnum { Stopped, Running, Restarting }
        public static StateEnum state;

        public static void Main(params string[] args)
        {
            state = StateEnum.Running;
            Task.Run(async () =>
            {
                preStartUpTime = DateTime.UtcNow;

                Task.Run(async () => await VRC.StartAsync()).GetAwaiter();
                Task.Run(async () => await Bot.StartAsync()).GetAwaiter();
            });
            while(state != StateEnum.Stopped)
            {
                var cmd = Console.ReadLine();
                if (!string.IsNullOrEmpty(cmd))
                    CommandProcessor.Process(Assembly.GetAssembly(typeof(ConsoleProgram)).GetName().Name, cmd);
            }
        }
    }
}
