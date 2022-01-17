using DSharpPlus;
using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Orbital.Data;
using Orbital.Commands;
using DSharpPlus.SlashCommands;
using System.Linq;
using System.Drawing;
using DSharpPlus.Entities;
using System.Diagnostics;
using System.Threading;

namespace Orbital.Init
{
    public static class Bot
    {
        public static DiscordClient ctx { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }

        

        private static async Task InitAsync()
        {
            var settings = Resources.Get<Settings>();
            if (string.IsNullOrEmpty(Resources.Get<Settings>().token))
            {
                Debug.Log($"Error({Errors.NoToken}) {Errors.GetReason(Errors.NoToken)}", Color.Red);
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

            var slash = discord.UseSlashCommands(new SlashCommandsConfiguration {  });

            var CommandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = Resources.Get<Settings>().prefixes,
                CaseSensitive = Resources.Get<Settings>().casesensitive,
                EnableDms = Resources.Get<Settings>().enabledms,
                EnableMentionPrefix = Resources.Get<Settings>().enablementionprefix,
                DmHelp = Resources.Get<Settings>().dmhelp,
                EnableDefaultHelp = Resources.Get<Settings>().enabledefaulthelp,
                IgnoreExtraArguments = Resources.Get<Settings>().ignoreExtraArguments,
            };

            Commands = discord.UseCommandsNext(CommandsConfig);
            Commands.RegisterCommands<ManagementCommands>();
            Commands.RegisterCommands<UtilityCommands>();
            Commands.RegisterCommands<VRChatCommands>();
            Commands.RegisterCommands<ClubCommands>();

            slash.RegisterCommands<SlashManagementCommands>(866821480515108904);
            slash.RegisterCommands<SlashUtilityCommands>(866821480515108904);
            slash.RegisterCommands<SlashVRChatCommands>(866821480515108904);
            slash.RegisterCommands<SlashClubCommands>(866821480515108904);

            ctx = discord;
            ctx.Ready += async (sender, e) =>
            {                
                await Events.OnReady.OnTrigger(sender, e);
            };
            ctx.GuildAvailable += async (sender, e) =>
            {   
                Debug.Log($"Initializing **{e.Guild.Name}**");
                await Utils.UpdateSettingsAsync(e.Guild);
            };

            ctx.MessageCreated += async (sender, e) =>
            {
                if (!e.Author.IsBot && e.Guild.Id == 866821480515108904 && (e.Channel.Id == 898105898420023296 || e.Channel.Name == "club-music"))
                    await Utils.CheckClubMusicChannel(e.Message);
            };
            await Task.Delay(1);
        }

        private static async Task DiscordLoop()
        {
            var guild = await ctx.GetGuildAsync(866821480515108904);
            var role = guild.GetRole(877291754234212432);
            var timer = new Timer(async (e) => await AutoAssignRole(guild, role), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            while (ConsoleProgram.state == ConsoleProgram.StateEnum.Running)
            {

            }
            await Task.Delay(1);
           
        }

        private static async Task AutoAssignRole(DiscordGuild guild, DiscordRole role)
        {
            try
            {
                var members = await guild.GetAllMembersAsync();
                foreach (var m in members)
                {
                    if (m.JoinedAt.Minute >= 3)
                    {
                        var roles = m.Roles.ToList();
                        var _role = roles.Find(x => x.Id == role.Id);
                        if (_role == null)
                            await m.GrantRoleAsync(role, "Member has been on the discord server for more then 3 min.");
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        public static async Task StartAsync()
        {
            await InitAsync();
            if (Errors.has) return;
            Debug.Log("Starting Discord Client...", header: true);
            await ctx.ConnectAsync();
            Task.Run(async () => await DiscordLoop()).GetAwaiter();
            while (ConsoleProgram.state == ConsoleProgram.StateEnum.Running) { }
            Debug.Log("Stoping  Discord Client...", header: true);
            await ctx.DisconnectAsync();
        }
        public static async Task StopAsync()
        {
            Debug.Log("Stoping Discord Client", header: true);
            await ctx.DisconnectAsync();
        }
    }
}
