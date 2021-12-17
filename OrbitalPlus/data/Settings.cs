using DSharpPlus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Orbital.Data
{
    [Serializable]
    public class Settings
    {
        [JsonIgnore] public static string savefile
        {
            get
            {
                var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                var _file = Path.Combine(dir, "orbital_settings.ocf");
                return _file;
            }
        }
     
        
        public string token;
        public TokenType tokentype;
        public LogLevel minimumloglevel;
        public string[] prefixes;
        public bool casesensitive;
        public bool enabledms;
        public bool enablementionprefix;
        public bool dmhelp;
        public bool enabledefaulthelp;

        public string vrc_username;
        public string vrc_password;
        public string vrc_apikey;
        public string vrc_worldid;

        public Settings()
        {
            token = string.Empty;
            tokentype = TokenType.Bot;
            minimumloglevel = LogLevel.Information;
            prefixes = new string[] { "!orbital ", "!o " };
            casesensitive = false;
            enabledms = true;
            enablementionprefix = true;
            dmhelp = false;
            enabledefaulthelp = false;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(savefile, json);
            if (File.Exists(savefile))
                Debug.Log("Settings file has been saved successfully!");
        }
    }
}