using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Orbital.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var gsettings = Resources.Get<GuildSettings>(ctx.Guild.Name);
            if (!File.Exists(filepath))
            {
                await ctx.Message.RespondAsync("File does not exist");
                return;
            }
            gsettings.permissionFile = filepath;
            await ctx.Message.RespondAsync("File location has been saved!");
            gsettings.Save(GuildSettings.savefile(ctx.Guild.Name));
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
