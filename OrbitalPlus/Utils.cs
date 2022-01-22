using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using Orbital.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using Debug = Orbital.Init.Debug;

namespace Orbital
{
    internal class Utils
    {
        internal static DiscordChannel staffAppCat;

        internal static string GetDomain(string item)
        {
            if (item.ToLower().Contains("discord.gg")) return "Discord";
            if (item.ToLower().Contains("gumroad.com")) return "Gumroad";
            if (item.ToLower().Contains("twitter.com")) return "Twitter";
            if (item.ToLower().Contains("youtube.com")) return "Youtube";
            if (item.ToLower().Contains("udonvr.com")) return "UdonVR";
            return new Uri(item).Host;
        }
        internal static String HexConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        internal static String HexConverter(DiscordColor c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        internal static String RGBConverter(System.Drawing.Color c)
        {
            return "RGB(" + c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString() + ")";
        }

        public static Task UpdateSettingsAsync(DiscordGuild guild)
        {
            var gsettings = Resources.Get<GuildSettings>(guild.Name);
            if (gsettings != null)
            {
                gsettings.id = guild.Id;
                gsettings.name = guild.Name;
                if (gsettings.member_list == null)
                    gsettings.member_list = new List<Tuple<ulong, string, string>>();

                // Create or Update member list 
                foreach (var member in guild.Members)
                {
                    if (!member.Value.IsBot)
                        gsettings.AutoRegisterUser(member.Value.Id, member.Value.Username);
                    else
                        gsettings.AutoUnregisterUser(member.Value.Username);
                }
                try
                {
                    var json = File.ReadAllText(gsettings.permissionFile);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var obj = JsonConvert.DeserializeObject<TempRole>(json);
                        if (obj == null) gsettings.roles = null;
                        else  gsettings.roles = obj.roles;
                    }
                }
                catch (Exception ex) { }


                if (!string.IsNullOrEmpty(gsettings.permissionFile))
                {
                    gsettings.roles =  gsettings.roles.OrderByDescending(x => x.priority).ToList();

                    // Clean Up Role
                    foreach (var role in gsettings.roles)
                    {
                        var _role = guild.GetRole(role.permID);
                        if(_role == null)
                            gsettings.roles.RemoveAt(gsettings.roles.IndexOf(_role));
                    }

                    // Setup all the roles
                    foreach (var role in guild.Roles.Select(v => v.Value).ToList().OrderByDescending(x => x.Position).ToList())
                    {
                        var _role = (Role)role;
                        if (gsettings.roles == null) gsettings.roles = new List<Role>();
                        foreach (var _role_ in gsettings.roles)
                            if (guild.GetRole(_role_.permID) == null) 
                                gsettings.roles.RemoveAt(gsettings.roles.IndexOf(_role_));

                        if (gsettings.roles.Find(x => x.permID == _role.permID) == null)
                            gsettings.roles.Add(_role);
                        else
                        {
                            for (int i = 0; i < gsettings.roles.Count; i++)
                            {
                                if (gsettings.roles[i].permID == _role.permID)
                                {
                                    gsettings.roles[i].priority = _role.priority;
                                    gsettings.roles[i].permID = _role.permID;
                                    gsettings.roles[i].isRoot = _role.isRoot;
                                    gsettings.roles[i].permName = _role.permName.Replace("/", "_");
                                    gsettings.roles[i].permColor = _role.permColor;
                                    if (string.IsNullOrEmpty(gsettings.roles[i].permIcon))
                                        gsettings.roles[i].permIcon = _role.permIcon;

                                    foreach (var item in _role.members)
                                    {
                                        if (!gsettings.roles[i].members.Contains(item))
                                            gsettings.roles[i].members.Add(item);
                                    }
                                }
                            }
                        }

                    }
                    gsettings.roles = gsettings.roles.OrderByDescending(x => x.priority).ToList();

                    // Remove member if they don't have the role
                    for (int i = 0; i < gsettings.roles.Count; i++)
                    {
                        var role = gsettings.roles[i];
                        foreach (var member in guild.Members)
                        {
                            var mrols = member.Value.Roles.ToList();
                            var hasRole = mrols.Find(x => x.Id == role.permID) != null;

                            if(!hasRole)
                            {
                                var gmember = gsettings.member_list.Find(x => x.Item1 == member.Key);
                                if (gmember != null)
                                {
                                    var index = gsettings.roles[i].members.IndexOf($"{gmember.Item2}|{gmember.Item3}");
                                    if(index > -1) 
                                        gsettings.roles[i].members.RemoveAt(index);
                                }
                            }                                                
                        }
                    }

                    // Add all memebers to role if they have the permission
                    foreach (var member in guild.Members)
                    {
                        var _member = member.Value;
                        var mli = gsettings.member_list.Find(x => x.Item1 == _member.Id);
                        var memberrolse = _member.Roles.ToList();
                        foreach (var _role in memberrolse)
                        {
                            var perms = gsettings.roles.Find(x => x.permID == _role.Id);
                            var contains = mli == null ? false : perms.members.ToList().Find(x => x.Contains(mli.Item2)) != null;
                            if (perms != null && mli != null && !contains)
                                perms.members.Add($"{_member.Username}|{mli.Item3}");
                        }
                    }


                    var tmp = new TempRole() { roles = gsettings.roles };
                    var json = JsonConvert.SerializeObject(tmp, Formatting.Indented);
                    File.WriteAllText(gsettings.permissionFile, json);
                }
                gsettings.Save(GuildSettings.savefile(guild.Name));
            }
            return Task.CompletedTask;
        }

        internal static async Task<DiscordOverwriteBuilder>  MakeOverwrite(DiscordRole role, Permissions Allowed = default, Permissions Denied = default, DiscordOverwrite fromAsync = default)
        {
            var overwrite = new DiscordOverwriteBuilder(role);
            if (fromAsync != null)
                await overwrite.FromAsync(fromAsync);
            overwrite.Allowed = Allowed;
            overwrite.Denied = Denied;
            return await Task.FromResult(overwrite);
        }
        internal static async Task<DiscordOverwriteBuilder> MakeOverwrite(DiscordMember member, Permissions Allowed = default, Permissions Denied = default, DiscordOverwrite fromAsync = default)
        {
            var overwrite = new DiscordOverwriteBuilder(member);
            if (fromAsync != null)
                await overwrite.FromAsync(fromAsync);
            overwrite.Allowed = Allowed;
            overwrite.Denied = Denied;
            return await Task.FromResult(overwrite);
        }

        internal static bool IsOrbitalStaff(InteractionContext ctx, DiscordMember member)
        {

            var roles = ctx.Guild.Roles.Where(x => x.Value.Name.StartsWith("..") || (x.Value.Position >= 22 && x.Value.Position <= ctx.Guild.Roles.Count - 1)).Select(x => x.Key).ToList();
            foreach (var role in ctx.Member.Roles)
            {
                if (roles.Contains(role.Id))
                {
                    return true;
                }
            }
            return false;
        }

        internal static async Task CheckClubMusicChannel(DiscordMessage data)
        {
            if (Uri.TryCreate(data.Content, UriKind.Absolute, out Uri result))
            {
                var _result = await RunSearchAsync(result.AbsoluteUri);
                await data.RespondAsync($"Found {(_result != null ? _result.Title : string.Empty)}");
            }
            else
            {
                var _result = await RunSearchAsync(data.Content);
                await data.RespondAsync($"Found {(_result != null ? _result.Title : string.Empty)}");
            }

            await data.RespondAsync($"Song was requested by {data.Author.Username}");
        }

        internal static async Task<VideoData> RunSearchAsync(string url)
        {
            var cts = new CancellationTokenSource();
            Debug.Log(await GetYoutubeDL().RunUpdate());
            var result = await GetYoutubeDL().RunVideoDataFetch(GetQuery(url), cts.Token, overrideOptions: new YoutubeDLSharp.Options.OptionSet()
            {
                NoPlaylist = true,
                FlatPlaylist = true,
            });
            return result.Data;
        }

        private static YoutubeDL GetYoutubeDL()
        {
            var ytdl = new YoutubeDL();
            // set the path of the youtube-dl and FFmpeg if they're not in PATH or current directory
            ytdl.YoutubeDLPath = "Resources\\youtube-dl.exe";
            ytdl.FFmpegPath = "Resources\\ffmpeg.exe";
            // optional: set a different download folder
            ytdl.OutputFolder = "cache\\downloads";
            return ytdl;
        }

        private static string GetQuery(string url)
        {
            var query = string.Empty;

            if (url.StartsWith("http://") || url.StartsWith("https://"))
                query = url;
            else
                query = $"ytsearch50:\"{url}\"";

            return query;
        }

        internal static async Task<string> GetLocation(string value)
        {
            var split = value.Split('(', '~');
            value = split[0];

            if (value == string.Empty) return "N/A";
            if (value == "offline") return value;
            if (split.ToList().Contains("hidden")) return "In a Private World";
            var _world = await VRC.GetWorldByIdAsync(value);
            if (_world != null)
                return $"**{_world.Name}** {_world.AuthorName}";
            return "In a Private World";
        }


        public static ConsoleColor FromColor(Color c)
        {
            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
            index |= (c.R > 64) ? 4 : 0; // Red bit
            index |= (c.G > 64) ? 2 : 0; // Green bit
            index |= (c.B > 64) ? 1 : 0; // Blue bit
            return (ConsoleColor)index;
        }

        public static bool hasRole(InteractionContext ctx, DiscordMember member, DiscordRole role)
        {
            var memberRoles = member.Roles.ToList();
            return memberRoles.Find(x => x.Id == role.Id) != null;
        }
        public static bool hasRole(CommandContext ctx, DiscordMember member, DiscordRole role)
        {
            var memberRoles = member.Roles.ToList();
            return memberRoles.Find(x => x.Id == role.Id) != null;
        }

        public static bool IsDiscordRole(DiscordRole role)
        {
            if (role.Name == "@everyone") return true;
            if (role.Color.Value == DiscordColor.Black.Value) return true;
            return false;
        }
    }
} 