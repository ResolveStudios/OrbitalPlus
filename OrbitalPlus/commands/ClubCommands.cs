using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Orbital.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Orbital.Commands
{
    [Group("Club")]
    public class ClubCommands : BaseCommandModule
    {
        [Command("set-permissionsfile")]
        [Description("Sets the file path to the permission file")]
        public async Task SetPermissionsFile(CommandContext ctx, [RemainingText, Description("your vrchat name")] string filepath = null)
        {
            filepath = filepath.Replace("\"", string.Empty);
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(filepath))
            {
                try
                {
                    var att = File.GetAttributes(filepath);
                }
                catch
                {
                    var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
                    var _file = Path.Combine(dir, $"{filepath}.perm");
                    var fs = new FileStream(_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fs.Close();
                    gsettings.permissionFile = _file;
                    gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
                    await ctx.Message.RespondAsync("File location has been created!\nif you want to request your perm file just run `!o club get-permissionsfile` at any time to retrive it.");
                    await Utils.UpdateSettingsAsync(ctx.Guild);
                }
                return;
            }
            gsettings.permissionFile = filepath;
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
            await ctx.Message.RespondAsync("File location has been saved!\nif you want to request your perm file just run `!o club get-permissionsfile` at any time to retrive it.");
            await Utils.UpdateSettingsAsync(ctx.Guild);

        }
        
        [Command("get-permissionsfile")]
        [Description("Gets the permission file form your guilds settings")]
        public async Task GetPermissionsFile(CommandContext ctx)
        {
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(gsettings.permissionFile))
            {
                await ctx.Message.RespondAsync("Permission file has not been set or exist.");
                await ctx.Message.RespondAsync("Please use the set-permisionsfile to set this file or create this file");
                return;
            }
            if (File.Exists(gsettings.permissionFile))
            {
                var fs = new FileStream(gsettings.permissionFile, FileMode.Open);
                await ctx.Message.RespondAsync(new DiscordMessageBuilder().WithContent($"{ctx.Guild.Name} Permissions File").WithFile(fs, true));
                fs.Close();
            }

        }


        [Command("update-permissionsfile")]
        [Description("Updates the unity permission file.")]
        public async Task UpdatePermissionsFile(CommandContext ctx)
        {
            await Utils.UpdateSettingsAsync(ctx.Guild);
            await ctx.Message.RespondAsync("Permission file has been updated!");
        }
    }
}
