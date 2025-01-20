using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /// <summary>
    /// Defines a unit that was placed on the board
    /// </summary>
    public class Unit : PlaceableEntity
    {
        // Stats
        public int Movement { get; set; } = 0;
        public int MovementDenominator { get; set; } = 1;
        public int Attack { get; set; } = 0;
        // Internal Status Var
        public int MvtCooldownTimer { get; set; } = 0;
    }
}
