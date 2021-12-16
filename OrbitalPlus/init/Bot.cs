using DSharpPlus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using Orbital.Models;
using VRChat.API.Api;
using VRChat.API.Client;
using Orbital.Data;

namespace Orbital.Init
{
    public static class Bot
    {
        public static DiscordClient DiscordCtx { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        
        public static void Init()
        {
            Resoruces.Load<Settings>();

            if (string.IsNullOrEmpty(Resoruces.Get<Settings>().token))
            {
                Debug.Log($"Error({Errors.NoToken}) {Errors.GetReason(Errors.NoToken)}");
                Errors.SetError();
                return;
            }
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Resoruces.Get<Settings>().token,
                TokenType = Resoruces.Get<Settings>().tokentype,
                MinimumLogLevel = Resoruces.Get<Settings>().minimumloglevel,
            });
            var CommandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = Resoruces.Get<Settings>().prefixes,
                CaseSensitive = Resoruces.Get<Settings>().casesensitive,
                EnableDms = Resoruces.Get<Settings>().enabledms,
                EnableMentionPrefix = Resoruces.Get<Settings>().enablementionprefix,
                DmHelp = Resoruces.Get<Settings>().dmhelp,
                EnableDefaultHelp = Resoruces.Get<Settings>().enabledefaulthelp,
            };

            Commands = discord.UseCommandsNext(CommandsConfig);
            Commands.RegisterCommands<Commands.UtilityCommands>();
            Commands.RegisterCommands<Commands.VRChatCommands>();
            DiscordCtx = discord;
            DiscordCtx.Ready += Events.OnReady.OnTrigger;
        }

        internal static async Task StartAsync()
        {
            await DiscordCtx.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
