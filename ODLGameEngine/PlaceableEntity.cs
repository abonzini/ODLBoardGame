using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public enum CorrelationType
    {
        NONE,
        MUTUALLY_EXCLUSIVE, // If A then not B
        CORRELATED // If A then B
    }

    public class HiddenCorrelation
    {
        public CorrelationType corrType;
        public PlaceableEntity corrEntity;
    }

    public class PlaceableEntity
    {
        /// <summary>
        /// Unique ID that this entity has, no other one has it. Equivalent to the global order of play
        /// </summary>
        public int uniqueId = 0;
        /// <summary>
        /// Card for reference
        /// </summary>
        public Card card;
        /// <summary>
        /// Hp of thing
        /// </summary>
        public int hp { get; set; } = 0;
        // Internal status
        protected int damage = 0;
        public int getCurrentHp() { return hp - damage; }
        // Stealth shenanigans
        protected bool isHidden = false;
        protected bool isTheRealOne = true;
        protected List<HiddenCorrelation> hiddenCorrelations = new List<HiddenCorrelation>(); // If stealth unit, need to define correlations for when they're discovered
    }
}