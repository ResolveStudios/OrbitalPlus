using Newtonsoft.Json;
using Orbital.Data;
using Orbital.Init;
using System.IO;

namespace Orbital
{
    public static class Resoruces
    {
        private static Settings settings;
        private static MemberManager members;

        public static T Load<T>()
        {
            if(typeof(T).Equals(typeof(Settings)))
            {
                if (!File.Exists(Settings.savefile))
                {
                    Debug.Log("Settings file does not exist, creating it now...");
                    settings = new Settings();
                    settings = Save<Settings>();
                    return (T)(object)settings;
                }
                else
                {
                    Debug.Log("Loading Settings file...");
                    var json = File.ReadAllText(Settings.savefile);
                    settings = JsonConvert.DeserializeObject<Settings>(json);
                    if (settings != null)
                        Debug.Log("Settings file has been loaded successfully!");
                    else
                        settings = new Settings();
                    return (T)(object)settings;
                }
            }
            if (typeof(T).Equals(typeof(MemberManager)))
            {
                if (!File.Exists(MemberManager.savefile))
                {
                    Debug.Log("Members file does not exist, creating it now...");
                    members = new MemberManager();
                    members = Save<MemberManager>();
                    return (T)(object)members;
                }
                else
                {
                    Debug.Log("Loading Members file...");
                    var json = File.ReadAllText(MemberManager.savefile);
                    members = JsonConvert.DeserializeObject<MemberManager>(json);
                    if (members != null)
                        Debug.Log("Members file has been loaded successfully!");
                    else 
                        members = new MemberManager();
                    return (T)(object)members;
                }
            }
            return default(T);
        }

        public static T Save<T>()
        {
            if(typeof(T).Equals(typeof(Settings)))
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(Settings.savefile, json);
                if (File.Exists(Settings.savefile))
                    Debug.Log("Settings file has been saved successfully!");
                return (T)(object)settings;
            }
            if (typeof(T).Equals(typeof(MemberManager)))
            {
                var json = JsonConvert.SerializeObject(members, Formatting.Indented);
                File.WriteAllText(MemberManager.savefile, json);
                if (File.Exists(MemberManager.savefile))
                    Debug.Log("Members file has been saved successfully!");
                return (T)(object)members;
            }
            return default(T);
        }
        
        public static T Get<T>()
        {
            var value = Load<T>();
            if (value == null && typeof(T).Equals(typeof(Settings))) 
                value = (T)(object)new Settings();
            if (value == null && typeof(T).Equals(typeof(MemberManager))) 
                value = (T)(object)new MemberManager();
            return value;
        }
    }
}
