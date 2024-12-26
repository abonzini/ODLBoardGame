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
        public int movement { get; set; } = 0;
        public int movementDenominator { get; set; } = 1;
        public int attack { get; set; } = 0;
        // Internal Status Var
        public int mvtCooldownTimer { get; set; } = 0;
    }
}
