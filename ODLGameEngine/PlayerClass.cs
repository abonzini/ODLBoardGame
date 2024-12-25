using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /// <summary>
    /// Classes types
    /// </summary>
    public enum ClassType
    {
        BASE
    }

    public class PlayerClass
    {
        /// <summary>
        /// Name of class (default)
        /// </summary>
        public string name { get; set; } = "";
        /// <summary>
        /// Starting HP
        /// </summary>
        public int startingHp { get; set; } = 20;
        /// <summary>
        /// Starting card number in hand
        /// </summary>
        public int startingHandNumber { get; set; } = 4;
        /// <summary>
        /// Starting gold
        /// </summary>
        public int startingGold { get; set; } = 5;
    }
}
