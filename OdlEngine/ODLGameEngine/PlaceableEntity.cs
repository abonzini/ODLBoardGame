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

    public class PlaceableEntity : ICloneable
    {
        /// <summary>
        /// Unique ID that this entity has, no other one has it. Equivalent to the global order of play
        /// </summary>
        public int UniqueId { get; set; } = 0;
        /// <summary>
        /// Card id for reference
        /// </summary>
        public int Card { get; set; } = 0;
        /// <summary>
        /// Who owns this card
        /// </summary>
        public PlayerId Owner { get; set; } = PlayerId.SPECTATOR;
        /// <summary>
        /// Which lane is it on
        /// </summary>
        public LaneID LaneCoordinate { get; set; } = LaneID.NO_LANE;
        /// <summary>
        /// Which tile of lane (absolute to board)
        /// </summary>
        public int TileCoordinate { get; set; } = -1;
        /// <summary>
        /// Hp of thing
        /// </summary>
        public int Hp { get; set; } = 0;
        /// <summary>
        /// Damage taken
        /// </summary>
        public int Damage { get; set; } = 0;
        // Stealth shenanigans
        protected bool IsHidden { get; set; } = false;
        protected bool IsTheRealOne { get; set; } = true;
        protected List<HiddenCorrelation> HiddenCorrelations { get; set; } = new List<HiddenCorrelation>(); // If stealth unit, need to define correlations for when they're discovered
    
        public object Clone()
        {
            object newEntity = MemberwiseClone();
            ((PlaceableEntity)newEntity).HiddenCorrelations = new List<HiddenCorrelation>();
            return newEntity;
        }
    }
}