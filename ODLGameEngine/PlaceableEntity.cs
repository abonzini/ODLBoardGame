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
        /// Who owns this card
        /// </summary>
        public PlayerId owner { get; set; } = PlayerId.SPECTATOR;
        /// <summary>
        /// Which lane is it on
        /// </summary>
        public LaneID laneCoordinate = LaneID.NO_LANE;
        /// <summary>
        /// Which tile of lane (absolute to board)
        /// </summary>
        public int tileCoordinate = -1;
        /// <summary>
        /// Hp of thing
        /// </summary>
        public int hp { get; set; } = 0;
        /// <summary>
        /// Damage taken
        /// </summary>
        public int damage { get; set; } = 0;
        // Stealth shenanigans, important but never serialized
        protected bool isHidden { get; set; } = false;
        protected bool isTheRealOne { get; set; } = true;
        protected List<HiddenCorrelation> hiddenCorrelations { get; set; } = new List<HiddenCorrelation>(); // If stealth unit, need to define correlations for when they're discovered
    }
}