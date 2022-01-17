using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakoBot.Attributes;

namespace Orbital.Commands
{
    [Group("vrc")]
    public class VRChatCommands : BaseCommandModule
    {
        [Command("set-name")]
        [Description("Sets your vrchat name in the discord server")]
        public async Task SetVRCName(CommandContext ctx, [RemainingText, Description("your vrchat name")] string vrchatname = null)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            gsettings.RegisterUser(ctx.Member.Id, ctx.Member.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.Member.GrantRoleAsync(role);
                await ctx.Message.RespondAsync("Your vrchat username has been added and your discord is now linked.");
                await VRCAdd(ctx);
            }
            else await ctx.Message.RespondAsync("Your vrchat account is already linked to your discord");
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));

            await Utils.UpdateSettingsAsync(ctx.Guild);
        }
        
        [Command("set-theirname")]
        [Description("Sets the vrchat name of another user in the discord server")]
        public async Task SetVRCName(CommandContext ctx, [Description("The member you want to register")] DiscordMember member, [RemainingText, Description("their vrchat name")] string vrchatname = null)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            gsettings.RegisterUser(member.Id, member.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!member.Roles.Contains(role))
            {
                await member.GrantRoleAsync(role);
                await ctx.Message.RespondAsync($"{member.Username} vrchat username has been added and their discord is now linked.");
                await VRCAddThem(ctx, member);
            }
            else
            {
                await ctx.Message.RespondAsync($"{member.Username} vrchat username has been Updated!");
            }
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));

            await Utils.UpdateSettingsAsync(ctx.Guild);
        }

        [Command("get")]
        [Description("Get's a list of all vrc accounts on the server")]
        public async Task GetRoster(CommandContext ctx)
        {
            var guild_members = (await ctx.Guild.GetAllMembersAsync()).ToList();
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            await ctx.Channel.SendMessageAsync($"**=== {ctx.Guild.Name} Roster ===**");
            gsettings.member_list = gsettings.member_list.ToList().OrderBy(x => x.Item2).ThenBy(x => x.Item3).ToList();
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
            var roles = ctx.Guild.Roles.OrderByDescending(x => x.Value.Position).ToList();

            foreach (var _role in roles)
            {
                if (Utils.IsDiscordRole(_role.Value)) continue;
                try
                {
                    var sb = new StringBuilder();


                    var members = gsettings.member_list.FindAll(x => Utils.hasRole(ctx, guild_members.Find(z => z.Id == x.Item1), _role.Value));
                    
                    foreach (var item in gsettings.member_list)
                    {
                        var member = ctx.Guild.Members.ToList().Find(x => x.Key == item.Item1);
                        if (member.Value == null)
                        {
                            gsettings.member_list.RemoveAt(gsettings.member_list.IndexOf(gsettings.member_list.ToList().Find(x => x.Item1 == item.Item1)));
                            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                        }                       
                    }

                    if (members.Count > 0)
                    {
                        sb.AppendLine($"=== {_role.Value.Name} ===");
                        members.ForEach(async m => sb.AppendLine($"D: {(await ctx.Guild.GetMemberAsync(m.Item1)).DisplayName } ● V: {m.Item3}"));
                    }

                    if (!string.IsNullOrEmpty(sb.ToString()))
                        await ctx.Channel.SendMessageAsync(sb.ToString());
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error {_role.Value.Name}\n{ex}", Color.Red);
                }
            }

            await ctx.Channel.SendMessageAsync($"*({gsettings.member_list.Count}) Members In Roster*");
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



        [Command("inv")]
        [Description("Give you information about your vrc account")]
        public async Task InviteMe(CommandContext ctx)
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
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3, false) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendEventInvite(vrcuser);
                await ctx.Message.RespondAsync($"World Invite has ben sent to **{vrcuser.DisplayName}**");
            }
        }
        [Command("inv")]
        [RequireRoles(DSharpPlus.CommandsNext.Attributes.RoleCheckMode.Any, "VRCLinked")]
        [Description("Invites the member to the world evetnt instance ")]
        public async Task InviteMe(CommandContext ctx, [Description("The member you would like to get info on")] DiscordMember member)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!member.Roles.Contains(role))
            {
                await ctx.Message.RespondAsync("Sorry you have not registered your VRChat name with me yet.");
                await ctx.Message.DeleteAsync();
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == member.Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendEventInvite(vrcuser);
                await ctx.Message.RespondAsync($"World Invite has ben sent to **{vrcuser.DisplayName}**");
            }
        }
        [Command("inv")]
        [RequireRoles(DSharpPlus.CommandsNext.Attributes.RoleCheckMode.Any, "VRCLinked")]
        public async Task InviteMe(CommandContext ctx, [Description("The member you would like to get info on")] string query)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            var vrcuser = await VRC.GetUserByNameAsync(query) ?? await VRC.SearchAsync(query);
            if (vrcuser != default)
            {
                await VRC.SendEventInvite(vrcuser);
                await ctx.Message.RespondAsync($"World Invite has ben sent to **{vrcuser.DisplayName}**");
            }
        }



        [Command("add"),  Description("Have our discord bot send you a vrc friend request so that you can get world invites")]
        public async Task VRCAdd(CommandContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.Message.RespondAsync("Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == ctx.Member.Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3, false) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendFriendInvite(vrcuser);
                await ctx.Message.RespondAsync($"Friend request has ben sent to **{vrcuser.DisplayName}**");
            }
        }

        [Command("add-them"), Description("Have our discord bot send you a vrc friend request so that you can get world invites")]
        public async Task VRCAddThem(CommandContext ctx, [Description("The member who the bot needs to add")] DiscordUser user)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)user).Roles.Contains(role))
            {
                await ctx.Message.RespondAsync($"Sorry {((DiscordMember)user).DisplayName} has not yet registered their VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == ((DiscordMember)user).Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3, false) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendFriendInvite(vrcuser);
                await ctx.Message.RespondAsync($"Friend request has ben sent to **{vrcuser.DisplayName}**");
            }
        }
    }


    [SlashCommandGroup("vrc", "VRChat Module")]
    public class SlashVRChatCommands : ApplicationCommandModule
    {
        [SlashCommand("set-name", "Sets your vrchat name in the discord server")]
        public async Task SetVRCName(InteractionContext ctx, [RemainingText, Option("VRChatName", "your vrchat name")] string vrchatname = null)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            gsettings.RegisterUser(ctx.Member.Id, ctx.Member.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.Member.GrantRoleAsync(role);
                await ctx.CreateResponseAsync("Your vrchat username has been added and your discord is now linked.");
                await VRCAdd(ctx);
            }
            else await ctx.CreateResponseAsync("Your vrchat account is already linked to your discord");
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));

            await Utils.UpdateSettingsAsync(ctx.Guild);
        }

        [SlashCommand("set-theirname", "Sets the vrchat name of another user in the discord server")]
        public async Task SetTheirVRCName(InteractionContext ctx, [Option("DiscordUser", "The member you want to register")] DiscordUser user, [Option("VRChatName", "their vrchat name", true)] string vrchatname = null)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            gsettings.RegisterUser(user.Id, user.Username, vrchatname);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)user).Roles.Contains(role))
            {
                await ((DiscordMember)user).GrantRoleAsync(role);
                await ctx.CreateResponseAsync($"{user.Username} vrchat username has been added and their discord is now linked.");
                await VRCAddThem(ctx, (DiscordMember)user);
            }
            else
            {
                await ctx.CreateResponseAsync($"{user.Username} vrchat username has been Updated!");
            }
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));

            await Utils.UpdateSettingsAsync(ctx.Guild);
        }

        [SlashCommand("get", "Get's a list of all vrc accounts on the server")]
        public async Task GetRoster(InteractionContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);            
            await ctx.CreateResponseAsync($"**=== {ctx.Guild.Name} Roster ===**");
            gsettings.member_list = gsettings.member_list.ToList().OrderBy(x => x.Item2).ThenBy(x => x.Item3).ToList();
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
            var roles = ctx.Guild.Roles;
            foreach (var _role in roles)
            {
                if (Utils.IsDiscordRole(_role.Value)) continue;
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"=== {_role.Value.Name} ===");
                    foreach (var item in gsettings.member_list)
                    {
                        var member = ctx.Guild.Members.ToList().Find(x => x.Key == item.Item1);
                        if (member.Value == null)
                        {
                            gsettings.member_list.RemoveAt(gsettings.member_list.IndexOf(gsettings.member_list.ToList().Find(x => x.Item1 == item.Item1)));
                            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                        }
                        else
                        {
                            if (Utils.hasRole(ctx, member.Value, _role.Value))
                                sb.AppendLine($"D: {(member.Value != null ? member.Value.Mention : item.Item2)} ● V: {item.Item3}");
                        }
                    }
                    await ctx.CreateResponseAsync($"{sb.ToString()}\n");

                }
                catch(Exception ex)
                {
                    Debug.Log($"Error {_role.Value.Name}\n{ex}", Color.Red);
                }
            }

            await ctx.CreateResponseAsync($"*({gsettings.member_list.Count}) Members In Roster*");
        }

        [SlashCommand("whoami", "Give you information about your vrc account")]
        public async Task WhoAmI(InteractionContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.CreateResponseAsync("Sorry you have not registered your VRChat name with me yet.");
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
                await ctx.CreateResponseAsync(e.Build());
            }
        }

        [SlashCommand("whoarethey", "Give you information about their vrc account")]
        public async Task WhoAreThey(InteractionContext ctx, [Option("DiscordUser", "The member you would like to get info on")] DiscordUser user)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)user).Roles.Contains(role))
            {
                await ctx.CreateResponseAsync("Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == user.Username);
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
                await ctx.CreateResponseAsync(e.Build());
            }
        }

        [SlashCommand("whoarethey2", "Gets the vrchat info of the user that is passes in query")]
        public async Task WhoAreThey2(InteractionContext ctx, [Option("Query", "The member you would like to get info on")] string query)
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
                    Url = vrcuser.UserIcon ?? vrcuser.CurrentAvatarImageUrl
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
                await ctx.CreateResponseAsync(e.Build());
            }
        }

        [SlashCommand("inv-self", "Give you information about your vrc account")]
        [SlashRequireRole(TakoBot.Attributes.RoleCheckMode.Any, "VRCLinked")]
        public async Task InviteSelf(InteractionContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.CreateResponseAsync("Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == ctx.Member.Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3, false) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendEventInvite(vrcuser);
                await ctx.CreateResponseAsync($"World Invite has ben sent to **{vrcuser.DisplayName}**");
            }
        }
        
        [SlashCommand("inv-me", "Invites the member to the world evetnt instance")]
        [SlashRequireRole(TakoBot.Attributes.RoleCheckMode.Any, "VRCLinked")]
        public async Task InviteMe(InteractionContext ctx, [Option("Member", "The member you would like to get info on")] DiscordUser user)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)user).Roles.Contains(role))
            {
                await ctx.CreateResponseAsync( "Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == user.Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendEventInvite(vrcuser);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"World Invite has ben sent to **{vrcuser.DisplayName}**"
                });
            }
        }
        
        [SlashCommand("inv", "Invites the member to the world evetnt instance")]
        [SlashRequireRole(TakoBot.Attributes.RoleCheckMode.Any, "VRCLinked")]
        public async Task Invite(InteractionContext ctx, [Option("Query", "The member you would like to get info on")] string query)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            var vrcuser = await VRC.GetUserByNameAsync(query) ?? await VRC.SearchAsync(query);
            if (vrcuser != default)
            {
                await VRC.SendEventInvite(vrcuser);
                await ctx.CreateResponseAsync($"World Invite has ben sent to **{vrcuser.DisplayName}**");
            }
        }


        [SlashCommand("add", "Have our discord bot send you a vrc friend request so that you can get world invites")]
        public async Task VRCAdd(InteractionContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.CreateResponseAsync("Sorry you have not registered your VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == ctx.Member.Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3, false) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendFriendInvite(vrcuser);
                await ctx.CreateResponseAsync($"Friend request has ben sent to **{vrcuser.DisplayName}**");
            }
        }

        [SlashCommand("add-them", "Have our discord bot send you a vrc friend request so that you can get world invites")]
        public async Task VRCAddThem(InteractionContext ctx, [Option("Member", "The member who the bot needs to add")] DiscordUser user)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == "VRCLinked").Value;
            if (!((DiscordMember)user).Roles.Contains(role))
            {
                await ctx.CreateResponseAsync($"Sorry {((DiscordMember)user).DisplayName} has not yet registered their VRChat name with me yet.");
                return;
            }
            var mem = gsettings.member_list.Find(x => x.Item2 == ((DiscordMember)user).Username);
            var vrcuser = await VRC.GetUserByNameAsync(mem.Item3, false) ?? await VRC.SearchAsync(mem.Item3);
            if (vrcuser != default)
            {
                await VRC.SendFriendInvite(vrcuser);
                await ctx.CreateResponseAsync($"Friend request has ben sent to **{vrcuser.DisplayName}**");
            }
        }
    }
}
