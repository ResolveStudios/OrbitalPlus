using Newtonsoft.Json;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Orbital.Data
{
    [Serializable]
    public class MemberManager
    {
        [JsonIgnore] public static string savefile
        {
            get
            {
                var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                var _file = Path.Combine(dir, "orbital_members.ocf");
                Debug.Log(_file);
                return _file;
            }
        }

        public List<KeyValuePair<string, string>> member_list;


        public MemberManager()
        {
            member_list = new List<KeyValuePair<string, string>>();
        }
        
        public void RegisterUser(string discordname, string vrchatname)
        {
            Resoruces.Load<MemberManager>();

            if (member_list.FindAll(x => x.Key == discordname).Count <= 0)
                member_list.Add(new KeyValuePair<string, string>(discordname, vrchatname));
            else
            {
                Debug.Log("Your already in our system your vrchat name will be updated");
                for (int i = 0; i < member_list.Count; i++)
                {
                    if (member_list[i].Key == discordname)
                        member_list[i] = new KeyValuePair<string, string>(discordname, vrchatname);
                }
            }
            Resoruces.Save<MemberManager>();
        }
        public void UnregisterUser(string discordname)
        {
            Resoruces.Load<MemberManager>();

            for (int i = 0; i < member_list.Count; i++)
            {
                if (member_list[i].Key == discordname)
                    member_list.RemoveAt(i);
                break;
            }

            Resoruces.Save<MemberManager>();
        }
    }
}