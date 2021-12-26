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
using System.Threading;
using System.Threading.Tasks;

namespace Orbital
{
    class Program
    {
        public static DateTime preStartUpTime;
        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                preStartUpTime = DateTime.UtcNow;

                await VRC.InitAsync();
                await Bot.InitAsync();

                if (!Errors.has)
                {
                    Task.Run(async () => await VRC.StartAsync());
                    Task.Run(async () => await Bot.StartAsync());
                }
            });
            Console.ReadLine();
        }
    }
}
