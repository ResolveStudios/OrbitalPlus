using Newtonsoft.Json;
using Orbital.Data;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orbital
{
    public static class Resources
    {
                
        public static T Get<T>(string name = default)
        {
            Debug.Log($"Loading {(string.IsNullOrEmpty(name) ? string.Empty : $"{name} ")}{typeof(T).Name}...");
            if (typeof(T).Equals(typeof(Settings)))
            {
                if (!File.Exists(Settings.savefile))
                {
                    Debug.Log($"{typeof(T).Name} file has not been setup, creating it now...");
                    var value = new Settings();
                    return (T)(object)value;
                }
                else
                {
                    var json = File.ReadAllText(Settings.savefile);
                    var value = JsonConvert.DeserializeObject<Settings>(json);
                    if (value != null)
                        Debug.Log($"{typeof(T).Name} file has been loaded successfully!");
                    else
                        value = new Settings();
                    return (T)(object)value;
                }
            }
            if (typeof(T).Equals(typeof(GuildSettings)))
            {
                if (!File.Exists(GuildSettings.savefile(name)))
                {
                    Debug.Log($"{typeof(T).Name} file has not been setup, creating it now...");
                    var value = new GuildSettings();
                    return (T)(object)value;
                }
                else
                {
                    try
                    {
                        var json = File.ReadAllText(GuildSettings.savefile(name));
                        var value = JsonConvert.DeserializeObject<T>(json);
                        if (value != null)
                            Debug.Log($"{typeof(T).Name} file has been loaded successfully!");
                        else
                            value = Activator.CreateInstance<T>();
                        return (T)(object)value;
                    }
                    catch(Exception ex)
                    {
                        Debug.Log(ex, ConsoleColor.Red);
                    }
                }
            }
            return default(T);
        }
    }
}
