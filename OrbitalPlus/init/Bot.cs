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
using System.Linq;

namespace Orbital.Init
{
    public static class Bot
    {
        public static DiscordClient ctx { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        
        public static void Init()
        {
            var settings = Resources.Get<Settings>();


            if (string.IsNullOrEmpty(Resources.Get<Settings>().token))
            {
                Debug.Log($"Error({Errors.NoToken}) {Errors.GetReason(Errors.NoToken)}", ConsoleColor.Red);
                Errors.SetError();
                return;
            }
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = Resources.Get<Settings>().token,
                TokenType = Resources.Get<Settings>().tokentype,
                MinimumLogLevel = Resources.Get<Settings>().minimumloglevel,
                Intents = DiscordIntents.All,
            });
            var CommandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = Resources.Get<Settings>().prefixes,
                CaseSensitive = Resources.Get<Settings>().casesensitive,
                EnableDms = Resources.Get<Settings>().enabledms,
                EnableMentionPrefix = Resources.Get<Settings>().enablementionprefix,
                DmHelp = Resources.Get<Settings>().dmhelp,
                EnableDefaultHelp = Resources.Get<Settings>().enabledefaulthelp,
            };

            Commands = discord.UseCommandsNext(CommandsConfig);
            Commands.RegisterCommands<Commands.UtilityCommands>();
            Commands.RegisterCommands<Commands.VRChatCommands>();
            Commands.RegisterCommands<Commands.ClubCommands>();
            ctx = discord;
            ctx.Ready += async (sender, e) =>
            {                
                await Events.OnReady.OnTrigger(sender, e);
            };
            ctx.GuildAvailable += async (sender, e) => await Utils.UpdateSettingsAsync(e.Guild);
        }

        internal static async Task StartAsync()
        {
            await ctx.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
