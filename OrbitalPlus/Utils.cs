using DSharpPlus.Entities;
using Newtonsoft.Json;
using Orbital.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orbital
{
    internal class Utils
    {
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
            gsettings.id = guild.Id;
            gsettings.name = guild.Name;
            if (gsettings.member_list == null)
                gsettings.member_list = new List<Tuple<ulong, string, string>>();

            var json = File.ReadAllText(gsettings.permissionFile);
            var obj = JsonConvert.DeserializeObject<TempRole>(json);
            gsettings.roles = obj.roles;

            if (File.Exists(gsettings.permissionFile))
            {
                foreach (var role in guild.Roles)
                {
                    var _role = (Role)role.Value;

                    if (gsettings.roles.Find(x => x.permID == _role.permID) == null)
                        gsettings.roles.Add(_role);
                    else
                    {
                        for (int i = 0; i < gsettings.roles.Count; i++)
                        {
                            if (gsettings.roles[i].permID == _role.permID)
                            {
                                gsettings.roles[i].permID = _role.permID;
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
                foreach (var member in guild.Members)
                {
                    var _member = member.Value;
                    var mli = gsettings.member_list.Find(x => x.Item1 == _member.Id);
                    var memberrolse = _member.Roles.ToList();
                    foreach (var _role in memberrolse)
                    {
                        var perms = gsettings.roles.Find(x => x.permID == (int)_role.Id);
                        if (perms != null && mli != null && !perms.members.Contains(mli.Item3))
                            perms.members.Add(mli.Item3);
                    }
                }


                var tmp = new TempRole() { roles = gsettings.roles };
                json = JsonConvert.SerializeObject(tmp, Formatting.Indented);
                File.WriteAllText(gsettings.permissionFile, json);
            }
            gsettings.Save(GuildSettings.savefile(guild.Name));
            return Task.CompletedTask;
        }

        internal static async Task<string> GetLocation(string value)
        {
            var split = value.Split('(', '~');
            value = split[0];

            if (value == "offline") return value;
            if (split.ToList().Contains("hidden")) return "In a Private World";
            var _world = await VRC.GetWorldByIdAsync(value);

            return $"**{_world.Name}** {_world.AuthorName}";
        }
    }
}