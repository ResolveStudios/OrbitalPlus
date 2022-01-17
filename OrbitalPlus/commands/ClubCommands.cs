using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Orbital.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Orbital.Commands
{
    [Group("Club")]
    public class ClubCommands : BaseCommandModule
    {
        [Command("set-permissionsfile")]
        [Description("Sets the file path to the permission file")]
        public async Task SetPermissionsFile(CommandContext ctx, [RemainingText, Description("The file name you want for your permisions file")] string filepath = null)
        {
            filepath = filepath.Replace("\"", string.Empty);
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(filepath))
            {
                try
                {
                    var att = File.GetAttributes(filepath);
                }
                catch
                {
                    var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                    var _file = Path.Combine(dir, $"{ctx.Guild.Name.Replace(" ", "")}_{filepath}.perm");
                    var fs = new FileStream(_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fs.Close();
                    gsettings.permissionFile = _file;
                    gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                    await ctx.Message.RespondAsync("File location has been created!\nif you want to request your perm file just run `!o club get-permissionsfile` at any time to retrive it.");
                    await Utils.UpdateSettingsAsync(ctx.Guild);
                }
                return;
            }
            gsettings.permissionFile = filepath;
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
            await ctx.Message.RespondAsync("File location has been saved!\nif you want to request your perm file just run `!o club get-permissionsfile` at any time to retrive it.");
            await Utils.UpdateSettingsAsync(ctx.Guild);

        }
        
        [Command("get-permissionsfile")]
        [Description("Gets the permission file form your guilds settings")]
        public async Task GetPermissionsFile(CommandContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(gsettings.permissionFile))
            {
                await ctx.Message.RespondAsync("Permission file has not been set or exist.");
                await ctx.Message.RespondAsync("Please use the set-permisionsfile to set this file or create this file");
                return;
            }
            if (File.Exists(gsettings.permissionFile))
            {
                var fs = new FileStream(gsettings.permissionFile, FileMode.Open);
                await ctx.Message.RespondAsync(new DiscordMessageBuilder().WithContent($"{ctx.Guild.Name} Permissions File").WithFile(fs, true));
                fs.Close();
            }

        }


        [Command("update-permissionsfile")]
        [Description("Updates the unity permission file.")]
        public async Task UpdatePermissionsFile(CommandContext ctx)
        {
            await Utils.UpdateSettingsAsync(ctx.Guild);
            await ctx.Message.RespondAsync("Permission file has been updated!");
        }


        [Command("apply")]
        [Description("Start Staff Application Process")]
        public async Task ApplyForStaff(CommandContext ctx)
        {
            
        }
    }

    [SlashCommandGroup("Club", "Club Command Module")]
    public class SlashClubCommands : ApplicationCommandModule
    {
        [SlashCommand("apply", "Start Staff Application Process")]
        public async Task ApplyForStaff(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithTitle("Club Orbital Staff Applications");
            embed.WithThumbnail("https://cdn.discordapp.com/attachments/904470997376327681/930955099335962724/CO2048x2048.png");
            embed.WithDescription(File.ReadAllText("Resources\\StaffAppDesc.txt"));

            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var member = gsettings.member_list.Find(x => x.Item1 == ctx.Member.Id);
            embed.WithFooter($"{member.Item2} ● VRCHAT: {member.Item3}");
            await ctx.CreateResponseAsync(embed);


        }

        #region Permissions 
        [SlashCommand("set-permissionsfile", "Sets the file path to the permission file")]
        public async Task SetPermissionsFile(InteractionContext ctx, [RemainingText, Option("FilePath", "The file name you want for your permisions file")] string filepath = null)
        {
            filepath = filepath.Replace("\"", string.Empty);
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(filepath))
            {
                try
                {
                    var att = File.GetAttributes(filepath);
                }
                catch
                {
                    var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                    var _file = Path.Combine(dir, $"{ctx.Guild.Name.Replace(" ", "")}_{filepath}.perm");
                    var fs = new FileStream(_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fs.Close();
                    gsettings.permissionFile = _file;
                    gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                    await ctx.CreateResponseAsync("File location has been created!\nif you want to request your perm file just run `!o club get-permissionsfile` at any time to retrive it.");
                    await Utils.UpdateSettingsAsync(ctx.Guild);
                }
                return;
            }
            gsettings.permissionFile = filepath;
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
            await ctx.CreateResponseAsync("File location has been saved!\nif you want to request your perm file just run `!o club get-permissionsfile` at any time to retrive it.");
            await Utils.UpdateSettingsAsync(ctx.Guild);

        }

        [SlashCommand("get-permissionsfile", "Gets the permission file form your guilds settings")]
        public async Task GetPermissionsFile(InteractionContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(gsettings.permissionFile))
            {
                await ctx.CreateResponseAsync("Permission file has not been set or exist.");
                await ctx.CreateResponseAsync("Please use the set-permisionsfile to set this file or create this file");
                return;
            }
            if (File.Exists(gsettings.permissionFile))
            {
                var fs = new FileStream(gsettings.permissionFile, FileMode.Open);
                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.Guild.Name} Permissions File").WithFile(fs, true));
                fs.Close();
            }

        }

        [SlashCommand("update-permissionsfile", "Updates the unity permission file.")]
        public async Task UpdatePermissionsFile(InteractionContext ctx)
        {
            await Utils.UpdateSettingsAsync(ctx.Guild);
            await ctx.CreateResponseAsync("Permission file has been updated!");
        }

        #endregion

        #region Playlist
        [SlashCommand("set-playlistfile", "Sets the file path to the playlist file")]
        public async Task SetPlaylistFile(InteractionContext ctx, [RemainingText, Option("FilePath", "The file name you want for your permisions file")] string filepath = null)
        {
            filepath = filepath.Replace("\"", string.Empty);
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(filepath))
            {
                try
                {
                    var att = File.GetAttributes(filepath);
                }
                catch
                {
                    var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                    var _file = Path.Combine(dir, $"{ctx.Guild.Name.Replace(" ", "")}_{filepath}.perm");
                    var fs = new FileStream(_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fs.Close();
                    gsettings.playlistfile = _file;
                    gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                    await ctx.CreateResponseAsync("File location has been created!\nif you want to request your playlist file just run `get-playlistfile` at any time to retrive it.");
                    await Utils.UpdateSettingsAsync(ctx.Guild);
                }
                return;
            }
            gsettings.playlistfile = filepath;
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
            await ctx.CreateResponseAsync("File location has been saved!\nif you want to request your perm file just run `get-playlistfile` at any time to retrive it.");
            await Utils.UpdateSettingsAsync(ctx.Guild);

        }

        [SlashCommand("get-playlistfile", "Gets the playlist file form your guilds settings")]
        public async Task GetPlaylistFile(InteractionContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(gsettings.playlistfile))
            {
                await ctx.CreateResponseAsync("Playlist file has not been set or exist.");
                await ctx.CreateResponseAsync("Please use the set-playlistfile to set this file or create this file");
                return;
            }
            if (File.Exists(gsettings.playlistfile))
            {
                var fs = new FileStream(gsettings.playlistfile, FileMode.Open);
                await ctx.Member.SendMessageAsync(new DiscordMessageBuilder().WithContent($"{ctx.Guild.Name} Playlist File").WithFile(fs, true));
                fs.Close();
            }

        }

        [SlashCommand("update-playlistfile", "Updates the unity playlist file.")]
        public async Task UpdatePlaylistFile(InteractionContext ctx)
        {
            await Utils.UpdateSettingsAsync(ctx.Guild);
            await ctx.CreateResponseAsync("Playlist file has been updated!");
        }
        #endregion

        [SlashCommand("request-song", "Request a song to be added to the club music playlist")]
        public async Task RequestSong(InteractionContext ctx, [RemainingText, Option("URL", "The url of the youtube video you want to be added to the playlist")] string url)
        {
            var _result = await Utils.RunSearchAsync(url);
            await ctx.Channel.SendMessageAsync($"Found {(_result != null ? _result.Title : string.Empty)}");
        }
    }
}
