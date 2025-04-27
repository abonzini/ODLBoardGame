using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine // Handles stat calculation, stat modification
    {
        /// <summary>
        /// Completely overrides a stat's absolute value (i.e. sets a Base and no Modifier), only important in init or extreme effects
        /// </summary>
        /// <param name="stat">The stat to set</param>
        /// <param name="value">The new value</param>
        void STATS_SetAbsoluteBaseStat(Stat stat, int value)
        {
            ENGINE_SetStatModifierValue(stat, 0); // Clears modifier as the stat  will be the unmodified value
            ENGINE_SetStatBaseValue(stat, value); // Then set the set value, now stat will be (total = value + 0)
        }

        // TODO: Buff additive, buff multiply, buff set, buff absolute HP set (also clears damage)?
    }
}
