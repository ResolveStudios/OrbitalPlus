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
using System.Threading.Tasks;

namespace Orbital
{
    class Program
    {
        public static DateTime preStartUpTime;
        static void Main(string[] args)
        {
            preStartUpTime = DateTime.UtcNow;
            MainAsync().GetAwaiter().GetResult();
            Console.ReadKey();
        }

        static async Task MainAsync()
        {
            Resoruces.Load<Settings>();
            Resoruces.Load<MemberManager>();

            VRC.Init();
            Bot.Init();

            if(!Errors.has)
            {
                await VRC.StartAsync();
                await Bot.StartAsync();
            }
            Console.ReadKey();
        }
    }
}
