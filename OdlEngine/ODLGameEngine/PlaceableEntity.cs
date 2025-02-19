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
        protected bool _dirtyHash = true; // To see if hash needs to be recalculated
        protected int _hash;
        [JsonProperty]
        protected int _uniqueId = 0;
        /// <summary>
        /// Unique ID that this entity has, no other one has it. Equivalent to the global order of play
        /// </summary>
        public int UniqueId { get { return _uniqueId; } set { _dirtyHash = true; _uniqueId = value; } }
        [JsonProperty]
        protected int _card = 0;
        /// <summary>
        /// Card id for reference
        /// </summary>
        public int Card { get { return _card; } set { _dirtyHash = true; _card = value; } }
        /// <summary>
        /// Name of placeable (usually same as card)?
        /// </summary>
        [JsonProperty]
        public string Name { get; set; } = ""; // Hash won't care about this as its only useful for real game state stuff
        [JsonProperty]
        protected int _owner = 0;
        /// <summary>
        /// Player owner of unit
        /// </summary>
        public int Owner { get { return _owner; } set { _dirtyHash = true; _owner = value; } }
        [JsonProperty]
        protected LaneID _laneCoordinate = LaneID.NO_LANE;
        /// <summary>
        /// Which lane is it on
        /// </summary>
        public LaneID LaneCoordinate { get { return _laneCoordinate; } set { _dirtyHash = true; _laneCoordinate = value; } }
        [JsonProperty]
        protected int _tileCoordinate = -1;
        /// <summary>
        /// Which tile of lane (absolute to board)
        /// </summary>
        public int TileCoordinate { get { return _tileCoordinate; } set { _dirtyHash = true; _tileCoordinate = value; } }
        [JsonProperty]
        protected int _hp = 0;
        /// <summary>
        /// Hp of thing
        /// </summary>
        public int Hp { get { return _hp; } set { _dirtyHash = true; _hp = value; } }
        // Stealth shenanigans
        [JsonProperty]
        protected bool _isHidden = false;
        /// <summary>
        /// Whether unit is hidden alongside other units
        /// </summary>
        public bool IsHidden { get { return _isHidden; } set { _dirtyHash = true; _isHidden = value; } }
        [JsonProperty]
        protected bool _isTheRealOne = true;
        /// <summary>
        /// If, when hidden, this is the real one or not
        /// </summary>
        public bool IsTheRealOne { get { return _isTheRealOne; } set { _dirtyHash = true; _isTheRealOne = value; } }
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
        public virtual int GetHash()
        {
            if (_dirtyHash) // Recalculates only when dirty
            {
                HashCode hash = new HashCode();
                hash.Add(_uniqueId);
                hash.Add(_card);
                hash.Add(_owner);
                hash.Add(_laneCoordinate);
                hash.Add(_tileCoordinate);
                hash.Add(_hp);
                hash.Add(_isHidden);
                hash.Add(_isTheRealOne);
                _hash = hash.ToHashCode();
                _dirtyHash = false; // Currently updated hash
            }
            return _hash;
        }
        public bool IsHashDirty()
        {
            return _dirtyHash;
        }
    }
}