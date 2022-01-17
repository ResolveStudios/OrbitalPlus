using DSharpPlus.Entities;
using Newtonsoft.Json;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Orbital.Data
{
    [Serializable]
    public class GuildSettings
    {
        public static string savefile(string name)
        {
            var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            var _file = Path.Combine(dir, $"guild_settings_{(!string.IsNullOrEmpty(name) ? name : typeof(GuildSettings).GetHashCode())}.ocf");
            return _file;
        }

        public string name;
        public ulong id;
        public string permissionFile;
        public string playlistfile;
        public ulong announcementsChannel;
        public List<Tuple<ulong, string, string>> member_list = new List<Tuple<ulong, string, string>>();
        public List<Role> roles = new List<Role>();

        public GuildSettings()
        {
            member_list = new List<Tuple<ulong, string, string>>();
            roles = new List<Role>();
        }
        public GuildSettings(DiscordGuild guild)
        {
            name = guild.Name;
            id = guild.Id;
            member_list = new List<Tuple<ulong, string, string>>();
            permissionFile = string.Empty;
            roles = new List<Role>();
        }

        public void RegisterUser(ulong discordid, string discordname, string vrchatname)
        {
            if (member_list.FindAll(x => x.Item2 == discordname).Count <= 0)
                member_list.Add(new Tuple<ulong, string, string>(discordid, discordname, vrchatname));
            else
            {
                Debug.Log("Your already in our system your vrchat name will be updated", header: true);
                for (int i = 0; i < member_list.Count; i++)
                {
                    if (member_list[i].Item2 == discordname)
                        member_list[i] = new Tuple<ulong, string, string>(discordid, discordname, vrchatname);
                }
            }
            Save(savefile(name));
        }
        public void UnregisterUser(string discordname)
        {
            for (int i = 0; i < member_list.Count; i++)
            {
                if (member_list[i].Item2 == discordname)
                    member_list.RemoveAt(i);
                break;
            }
            Save(savefile(name));
        }

        internal void Save(string _filepath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(_filepath, json);
                if (File.Exists(_filepath))
                    Debug.Log($"{GetType().Name} File has been saved successfully!", header: true);

                if (permissionFile == null || !File.Exists(permissionFile)) return;
                var obj = JsonConvert.DeserializeObject<TempRole>(File.ReadAllText(permissionFile));
                json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                if (!string.IsNullOrEmpty(permissionFile))
                    File.WriteAllText(permissionFile, json);
            } 
            catch (Exception ex)
            {
                Debug.Log($"ERROR\n{ex}", Color.Red);
            }
                
        }
    }
}
