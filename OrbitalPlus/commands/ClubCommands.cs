using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TakoBot.Attributes;

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
    }

    [SlashCommandGroup("Club", "Club Command Module")]
    public class SlashClubCommands : ApplicationCommandModule
    {
        [SlashCommand("apply", "Start Staff Application Process")]
        public async Task ApplyForStaff(InteractionContext ctx)
        {
              var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (gsettings.acceptingApplications)
            {
                if (gsettings.staffappcat != default)
                    Utils.staffAppCat = ctx.Guild.GetChannel(gsettings.staffappcat);
                if (Utils.staffAppCat == null)
                {

                    var _Overwrites = new List<DiscordOverwriteBuilder>();
                    _Overwrites.Add(await Utils.MakeOverwrite(ctx.Guild.EveryoneRole, Denied: Permissions.All));
                    Utils.staffAppCat = await ctx.Guild.CreateChannelCategoryAsync("== Opened Applications ==", reason: "An application was opened.", overwrites: _Overwrites);
                    foreach (var role in gsettings.roles.FindAll(x => x.priority > 22))
                    {
                        var drole = ctx.Guild.GetRole((ulong)role.permID);
                        if (drole != null)
                        {
                            await Utils.staffAppCat.AddOverwriteAsync(drole, Permissions.All);
                        }
                    }
                    var appchannel = ctx.Guild.Channels.Select(x => x.Value).ToList().Find(x => x.Name == "applications");
                    await Utils.staffAppCat.ModifyAsync(c => c.Position = appchannel.Parent.Position - 1);
                    gsettings.staffappcat = Utils.staffAppCat.Id;
                    gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                }

                await ctx.CreateResponseAsync("Application Started.", true);

                var Overwrites = new[]
                {
                    await Utils.MakeOverwrite(ctx.Guild.EveryoneRole, Denied: Permissions.All, fromAsync: Utils.staffAppCat.PermissionOverwrites.First()),
                };
                var ch = await ctx.Guild.CreateChannelAsync($"{ctx.Member.DisplayName}-Staff-Application", ChannelType.Text, Utils.staffAppCat, overwrites: Overwrites);

                foreach (var role in ctx.Guild.Roles.Where(x => x.Value.Name.StartsWith("..") && x.Value.Position > 22))
                    await ch.AddOverwriteAsync(role.Value, allow: Permissions.All, reason: $"Respond to Application {ctx.Member.DisplayName}");
                await ch.AddOverwriteAsync(ctx.Member, allow: Permissions.SendMessages | Permissions.ReadMessageHistory | Permissions.AccessChannels | Permissions.UseApplicationCommands, reason: $"Application started for {ctx.Member.DisplayName}");
                await ch.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.All, reason: "Private channel for the applications process");
                try
                {
                    var embed = new DiscordEmbedBuilder();
                    embed.WithTitle("Club Orbital Staff Applications");
                    embed.WithThumbnail("https://cdn.discordapp.com/attachments/904470997376327681/930955099335962724/CO2048x2048.png");

                    try
                    {
                        var member = gsettings.member_list.Find(x => x.Item1 == ctx.Member.Id);
                        embed.WithFooter($"Submiting applications as: {member.Item2}");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }

                    var header = File.ReadAllText("Resources\\StaffAppDesc.txt");
                    embed.WithDescription($"{ctx.Member.Mention}\n{header}\n\n**Staff Questionnaire**\n{File.ReadAllText("Resources\\StaffAppQuestionnaire.txt")}");
                    await ch.SendMessageAsync(embed);

                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                }

                catch (Exception ex) { Debug.Log(ex); }
            }
            else await ctx.CreateResponseAsync("sorry we are not accepting applications at this time");
        }
       

        [SlashCommand("app-close", "Indicates that the we are not accepting staff applications at this time.")]
        public async Task CloseApplicationQueue(InteractionContext ctx)
        {
            if(Utils.IsOrbitalStaff(ctx, ctx.Member))
            {
                var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
                gsettings.acceptingApplications = false;
                gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                await ctx.CreateResponseAsync("Applications are now closed");
            }
        }
        [SlashCommand("app-open", "Indicates that the we are not accepting staff applications at this time.")]
        public async Task OpenApplicationQueue(InteractionContext ctx)
        {
            if (Utils.IsOrbitalStaff(ctx, ctx.Member))
            {
                var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
                gsettings.acceptingApplications = true;
                gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                await ctx.CreateResponseAsync("Applications are now open");
            }
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

    public class SlashClubCommandsExtra : ApplicationCommandModule
    {
        [SlashCommand("submit", "Finish application process")]
        public async Task submitApplication(InteractionContext ctx)
        {
            
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (gsettings.staffappcat != default)
                Utils.staffAppCat = ctx.Guild.GetChannel(gsettings.staffappcat);
            if (ctx.Channel.Parent == Utils.staffAppCat)
            {
                var ob = new DiscordOverwriteBuilder(ctx.Member);
                ob.Allowed = (Permissions.SendMessages | Permissions.ReadMessageHistory | Permissions.AccessChannels);
                await ctx.Channel.DeleteOverwriteAsync(ctx.Member, "User has submited their staff application");

                var roles = ctx.Guild.Roles.Where(x => x.Value.Name.StartsWith("..") && x.Value.Position >= 22 && x.Value.Position <= ctx.Guild.Roles.Count - 1);
                var sb = new StringBuilder();
                foreach (var item in roles)
                    sb.Append(item.Value.Mention);
                await ctx.Channel.SendMessageAsync(sb.ToString());
            }
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("accept", "Accept the staff application")]
        public async Task acceptApplication(InteractionContext ctx)
        {
            if(Utils.IsOrbitalStaff(ctx, ctx.Member))
            {
                var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
                if (gsettings.staffappcat != default)
                    Utils.staffAppCat = ctx.Guild.GetChannel(gsettings.staffappcat);
                if (ctx.Channel.Parent == Utils.staffAppCat)
                {
                    var title = ctx.Channel.Name;
                    var messages = await ctx.Channel.GetMessagesAsync(100);
                    await ctx.Channel.DeleteAsync("Application has been accepted and we no longer need channel");
                    var appchannel = ctx.Guild.Channels.Select(x => x.Value).ToList().Find(x => x.Name == "applications");
                    if (appchannel != null)
                    {
                        var embed = new DiscordEmbedBuilder();
                        embed.WithTitle($"**{title}** [Accepted]");
                        embed.WithThumbnail("https://cdn.discordapp.com/attachments/904470997376327681/930955099335962724/CO2048x2048.png");

                        var sb = new StringBuilder();

                        messages = messages.OrderBy(x => x.Timestamp).ToList();

                        foreach (var message in messages)
                        {
                            sb.AppendLine(message.Content);
                            sb.AppendLine();
                        }

                        embed.WithDescription(sb.ToString());

                        try
                        {
                            var member = gsettings.member_list.Find(x => x.Item1 == ctx.Member.Id);
                            embed.WithFooter($"Accepted by: {member.Item2}");
                        }
                        catch (Exception ex) { Debug.Log(ex); }

                        await appchannel.SendMessageAsync(embed);
                    }
                }
            }
            else await ctx.CreateResponseAsync("You dont have permissions to use this slash command");
        }
        [SlashCommand("decline", "Decline the staff application")]
        public async Task declineApplication(InteractionContext ctx)
        {
            if (Utils.IsOrbitalStaff(ctx, ctx.Member))
            {
                var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
                if (gsettings.staffappcat != default)
                    Utils.staffAppCat = ctx.Guild.GetChannel(gsettings.staffappcat);
                if (ctx.Channel.Parent == Utils.staffAppCat)
                {
                    var title = ctx.Channel.Name;
                    var messages = await ctx.Channel.GetMessagesAsync(100);
                    await ctx.Channel.DeleteAsync("Application has been declined and we no longer need channel");
                    var appchannel = ctx.Guild.Channels.Select(x => x.Value).ToList().Find(x => x.Name == "applications");
                    if (appchannel != null)
                    {
                        var embed = new DiscordEmbedBuilder();
                        embed.WithTitle($"**{ title}** [Declined]");
                        embed.WithThumbnail("https://cdn.discordapp.com/attachments/904470997376327681/930955099335962724/CO2048x2048.png");

                        var sb = new StringBuilder();
                        messages = messages.OrderBy(x => x.Timestamp).ToList();

                        foreach (var message in messages)
                        {
                            sb.AppendLine(message.Content);
                            sb.AppendLine();
                        }

                        embed.WithDescription(sb.ToString());

                        try
                        {
                            var member = gsettings.member_list.Find(x => x.Item1 == ctx.Member.Id);
                            embed.WithFooter($"{member.Item2} ● VRCHAT: {member.Item3}");
                        }
                        catch (Exception ex) { Debug.Log(ex); }

                        await appchannel.SendMessageAsync(embed);
                    }
                }
            }
            else await ctx.CreateResponseAsync("You dont have permissions to use this slash command");
        }
    }
}
