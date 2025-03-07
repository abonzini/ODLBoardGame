using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        public int corrEntityId;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PlaceableEntity : ICloneable, IHashable
    {
        [JsonProperty]
        public int UniqueId { get; set; } = 0;
        [JsonProperty]
        public int Card { get; set; } = 0;
        /// <summary>
        /// Name of placeable (usually same as card)?
        /// </summary>
        [JsonProperty]
        public string Name { get; set; } = ""; // Hash won't care about this as its only useful for real game state stuff
        [JsonProperty]
        public int Owner { get; set; } = 0;
        [JsonProperty]
        public LaneID LaneCoordinate { get; set; } = LaneID.NO_LANE;
        [JsonProperty]
        public int TileCoordinate { get; set; } = -1;
        [JsonProperty]
        public int Hp { get; set; } = 0;
        [JsonProperty]
        public int DamageTokens { get; set; } = 0;
        // Stealth shenanigans
        [JsonProperty]
        public bool IsHidden { get; set; } = false;
        [JsonProperty]
        public bool IsTheRealOne { get; set; } = true;
        [JsonProperty]
        public List<HiddenCorrelation> HiddenCorrelations { get; set; } = new List<HiddenCorrelation>();
    
        public object Clone()
        {
            object newEntity = MemberwiseClone();
            ((PlaceableEntity)newEntity).HiddenCorrelations = new List<HiddenCorrelation>(); // No need to fill as the cloning is normally used to copy card details to board
            return newEntity;
        }
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public virtual int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(UniqueId);
            hash.Add(Card);
            hash.Add(Owner);
            hash.Add(Hp);
            hash.Add(DamageTokens);
            hash.Add(IsHidden);
            hash.Add(IsTheRealOne);
            return hash.ToHashCode();
        }
    }
}