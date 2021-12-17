using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital.Commands
{
    [Group("vrchat")]
    public class VRChatCommands : BaseCommandModule
    {
        [Command("setmyname")]
        [Description("Sets your vrchat name in the discord server")]
        public async Task SetMyVRCName(CommandContext ctx, [RemainingText, Description("your vrchat name")] string vrchatname = null)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            gsettings.RegisterUser(ctx.Member.Id, ctx.Member.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.Member.GrantRoleAsync(role);
                await ctx.Message.DeleteAsync();
                await ctx.RespondAsync("Your vrchat username has been added and your discord is now linked.");
            }
            else await ctx.RespondAsync("Your vrchat account is already linked to your discord");
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));

            Utils.UpdateSettingsAsync(ctx.Guild);
        }
        [Command("settheirname")]
        [Description("Sets the vrchat name of another user in the discord server")]
        public async Task SetTheirVRCName(CommandContext ctx, [Description("The member you want to register")] DiscordMember member, [RemainingText, Description("their vrchat name")] string vrchatname = null)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            gsettings.RegisterUser(member.Id, member.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!member.Roles.Contains(role))
            {
                await member.GrantRoleAsync(role);
                await ctx.Message.DeleteAsync();
                await ctx.RespondAsync($"{member.Username} vrchat username has been added and their discord is now linked.");
            }
            else await ctx.RespondAsync("Their vrchat account is already linked to your discord");
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));

            Utils.UpdateSettingsAsync(ctx.Guild);
        }
        
        [Command("whoami")]
        [Description("Give you information about your vrc account")]
        public async Task WhoAmI(CommandContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.Message.DeleteAsync();
                await ctx.RespondAsync("Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == ctx.Member.Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                var e = new DiscordEmbedBuilder();
                e.Title = $"{vrcuser.DisplayName} ({vrcuser.Username})";
                e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{vrcuser.StatusDescription} ({vrcuser.Status})"
                };
                e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() 
                {
                    Width = 64, 
                    Height = 64,
                    Url = (!string.IsNullOrEmpty(vrcuser.UserIcon) ? vrcuser.UserIcon.Substring(0, vrcuser.UserIcon.Length - 1) + ".png" : vrcuser.CurrentAvatarImageUrl)
                };
                e.Description += vrcuser.Bio;
                e.AddField("Location", await Utils.GetLocation(vrcuser.Location));
                foreach (var item in vrcuser.BioLinks)
                {
                    e.AddField(Utils.GetDomain(item), item, true);
                }
                e.AddField("Date Joined", vrcuser.DateJoined.ToShortDateString());
                e.AddField("Avatar", "** **");
                e.ImageUrl = vrcuser.CurrentAvatarThumbnailImageUrl;
                await ctx.Message.RespondAsync(e.Build());
            }
        }

        [Command("whoarethey")]
        [Description("Give you information about their vrc account")]
        public async Task WhoAreThey(CommandContext ctx, [Description("The member you would like to get info on")]DiscordMember member)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!member.Roles.Contains(role))
            {
                await ctx.Message.DeleteAsync();
                await ctx.RespondAsync("Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == member.Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                var e = new DiscordEmbedBuilder();
                e.Title = $"{vrcuser.DisplayName} ({vrcuser.Username})";
                e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{vrcuser.StatusDescription} ({vrcuser.Status})"
                };
                e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() 
                { 
                    Width = 64,
                    Height = 64,
                    Url = (!string.IsNullOrEmpty(vrcuser.UserIcon) ? vrcuser.UserIcon.Substring(0, vrcuser.UserIcon.Length - 1) + ".png" : vrcuser.CurrentAvatarImageUrl)
                };
                e.Description += vrcuser.Bio;
                e.AddField("Location", await Utils.GetLocation(vrcuser.Location));
                foreach (var item in vrcuser.BioLinks)
                {
                    if (!string.IsNullOrEmpty(item))
                        e.AddField(Utils.GetDomain(item), item, true);
                }
                e.AddField("Date Joined", vrcuser.DateJoined.ToShortDateString());
                e.AddField("Avatar", "** **");
                e.ImageUrl = vrcuser.CurrentAvatarThumbnailImageUrl;
                await ctx.Message.RespondAsync(e.Build());
            }
        }
        [Command("whoarethey")]
        public async Task WhoAreThey(CommandContext ctx, [Description("The member you would like to get info on")] string query)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            var vrcuser = await VRC.GetUserByNameAsync(query) ?? await VRC.SearchAsync(query);
            if (vrcuser != default)
            {
                var e = new DiscordEmbedBuilder();
                e.Title = $"{vrcuser.DisplayName} ({vrcuser.Username})";
                e.Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = $"{vrcuser.StatusDescription} ({vrcuser.Status})"
                };
                e.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() 
                { 
                    Width = 64, 
                    Height = 64,
                    Url =  vrcuser.UserIcon ?? vrcuser.CurrentAvatarImageUrl 
                };
                e.Description += vrcuser.Bio;
                e.AddField("Location", await Utils.GetLocation(vrcuser.Location));
                foreach (var item in vrcuser.BioLinks)
                {
                    if (!string.IsNullOrEmpty(item))
                        e.AddField(Utils.GetDomain(item), item, true);
                }
                e.AddField("Date Joined", vrcuser.DateJoined.ToShortDateString());
                e.AddField("Avatar", "** **");
                e.ImageUrl = vrcuser.CurrentAvatarThumbnailImageUrl;
                await ctx.Message.RespondAsync(e.Build());
            }
        }
    } 
}
