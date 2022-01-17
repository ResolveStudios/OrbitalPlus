using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakoBot.Attributes
{
    /// <summary>
    /// Defines that usage of this command is restricted to members with specified role.
    /// </summary>
    public class SlashRequireRoleAttribute : SlashCheckBaseAttribute
    {
        /// <summary>
        /// Gets the name of the role required to execute this command.
        /// </summary>
        public IReadOnlyList<string> RoleNames { get; }

        /// <summary>
        /// Gets the role checking mode. Refer to <see cref="RoleCheckMode"/> for more information.
        /// </summary>
        public RoleCheckMode CheckMode { get; }
        
        /// <summary>
        /// Defines that usage of this command is restricted to members with specified role.
        /// </summary>
        /// <param name="checkMode">Role checking mode.</param>
        /// <param name="roleNames">Names of the role to be verified by this check.</param>
        public SlashRequireRoleAttribute(RoleCheckMode checkMode, params string[] roleNames)
        {
            this.CheckMode = checkMode;
            this.RoleNames = new ReadOnlyCollection<string>(roleNames);
        }
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            var rns = ctx.Member.Roles.Select(xr => xr.Name);
            var rnc = rns.Count();
            var ins = rns.Intersect(this.RoleNames,StringComparer.OrdinalIgnoreCase);
            var inc = ins.Count();

            return this.CheckMode switch
            {
                RoleCheckMode.All => Task.FromResult(this.RoleNames.Count == inc),
                RoleCheckMode.SpecifiedOnly => Task.FromResult(rnc == inc),
                RoleCheckMode.None => Task.FromResult(inc == 0),
                _ => Task.FromResult(inc > 0),
            };
        }

    }
    /// <summary>
    /// Specifies how does <see cref="SlashRequireRoleAttribute"/> check for roles.
    /// </summary>
    public enum RoleCheckMode
    {
        /// <summary>
        /// Member is required to have any of the specified roles.
        /// </summary>
        Any,

        /// <summary>
        /// Member is required to have all of the specified roles.
        /// </summary>
        All,

        /// <summary>
        /// Member is required to have exactly the same roles as specified; no extra roles may be present.
        /// </summary>
        SpecifiedOnly,

        /// <summary>
        /// Member is required to have none of the specified roles.
        /// </summary>
        None
    }
}
