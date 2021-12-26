using DSharpPlus;
using DSharpPlus.Entities;
using Orbital.Init;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Orbital.Data
{
    [Serializable]
    public class Role
    {
        public int priority;
        public bool isRoot;
        public int permID;
        public string permName;
        public string permColor;
        public string permIcon;
        public List<string> members = new List<string>();

        public string PrettyName()
        {
            return char.ToUpper(permName[0]) + permName.Substring(1);
        }

        public static implicit operator Role(DiscordRole r)
        {
            return new Role()
            {
                permID = (int)r.Id,
                priority = r.Position,
                isRoot = r.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed,
                permName = r.Name.Replace("/", "_"),
                permColor = Utils.HexConverter(r.Color),                
            };
        }
    }
}