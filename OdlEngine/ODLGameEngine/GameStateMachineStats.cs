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
        /// <summary>
        /// Adds the value to the stat as a modifier
        /// </summary>
        /// <param name="stat">The stat</param>
        /// <param name="value">The value to add</param>
        void STATS_AddToStat(Stat stat, int value)
        {
            ENGINE_SetStatModifierValue(stat, stat.Modifier + value);
        }
        /// <summary>
        /// Multiplies the stat by a value
        /// </summary>
        /// <param name="stat">The stat</param>
        /// <param name="value">The value to multiply</param>
        void STATS_MultiplyStat(Stat stat, int value)
        {
            int total = stat.Total;
            total *= value; // This is the new total amount
            ENGINE_SetStatModifierValue(stat, total - stat.BaseValue);
        }
        /// <summary>
        /// Modifies stat so that it's exactly equal to the value
        /// </summary>
        /// <param name="stat">The stat</param>
        /// <param name="value">The new value</param>
        void STATS_SetStat(Stat stat, int value)
        {
            ENGINE_SetStatModifierValue(stat, value - stat.BaseValue);
        }
    }
}
