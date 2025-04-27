using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    [JsonConverter(typeof(FlagEnumJsonConverter))]
    [Flags]
    public enum EntityType
    {
        NONE        = 0,
        UNIT        = 1,
        BUILDING    = 2,
        SKILL       = 4,
        PLAYER      = 8
    }
    /// <summary>
    /// Which expansion the card belongs to
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExpansionId
    {
        VANILLA
    }

    /// <summary>
    /// Defines how/where card can be targeted, useful for giving options at a first glance to a player
    /// </summary>
    [Flags]
    [JsonConverter(typeof(FlagEnumJsonConverter))]
    public enum TargetLocation
    {
        BOARD = 0,
        PLAINS = 1,
        FOREST = 2,
        MOUNTAIN = 4,
        ALL_BUT_MOUNTAIN = 3,
        ALL_BUT_FOREST = 5,
        ALL_BUT_PLAINS = 6,
        ALL_LANES = 7,
        INVALID = 8,
        PLAY_TARGET
    }

    /// <summary>
    /// The condition that makes the card say "yes, this (lane?) is a valid target"
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TargetCondition
    {
        NONE, /// Can be played always
        BLUEPRINT, /// Subject to blueprint targeting
        // Could be, but only implement as needed
        //LANE_HAS_ENEMY_UNIT,
        //LANE_HAS_ENEMY_BUILDING,
        //LANE_HAS_FRIENDLY_UNIT,
        //LANE_HAS_FRIENDLY_BUILDING
    }
    /// <summary>
    /// Will define how a card looks
    /// </summary>
    public class EntityPrintInfo
    {
        public int Id { get; set; } = 0;
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        // Playable info (will be in card)
        public string Cost { get; set; } = "";
        public string Hp { get; set; } = "";
        public string Movement { get; set; } = "";
        public string Attack { get; set; } = "";
        public int Rarity { get; set; } = 0;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExpansionId Expansion { get; set; } = ExpansionId.VANILLA;
        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerClassType ClassType { get; set; } = PlayerClassType.BASE;
    }
    /// <summary>
    /// Defines how a card is played
    /// </summary>
    public class EntityPlayInfo
    {
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EntityType EntityType { get; set; } = EntityType.NONE;
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public TargetLocation TargetOptions { get; set; } = TargetLocation.BOARD; // Which lane(s) if any the card could work on
        [JsonConverter(typeof(StringEnumConverter))]
        public TargetCondition TargetConditions { get; set; } = TargetCondition.NONE; // What needs to happen for a card to be "playable" in a lane
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class EntityBase : ICloneable, IHashable
    {
        [JsonProperty]
        public int Owner { get; set; } = 0;
        [JsonProperty]
        public EntityPlayInfo EntityPlayInfo { get; set; } = new EntityPlayInfo(); // Non-hashed, non-cloned as I just want to reference once
        [JsonProperty]
        public EntityPrintInfo EntityPrintInfo { get; set; } // Non-hashed, non-cloned as I just want to reference once
        [JsonProperty]
        public Dictionary<InteractionType, List<Effect>> Interactions { get; set; } = null; // Non hashed, also when cloned, it links to the same reference and doesn't duplicate this

        public virtual object Clone()
        {
            EntityBase newEntity = (EntityBase)MemberwiseClone();
            return newEntity;
        }
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public virtual int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            return hash.ToHashCode();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BoardEntity : EntityBase
    {
        [JsonProperty]
        public string Name { get; set; } = "";
        [JsonProperty]
        [JsonConverter(typeof(StatJsonConverter))]
        public Min0Stat Hp { get; set; } = new Min0Stat();
        [JsonProperty]
        public int DamageTokens { get; set; } = 0;
        [JsonProperty]
        public int UniqueId { get; set; } = 0;
        [JsonProperty]
        public Dictionary<TriggerType, List<Effect>> Triggers { get; set; } = null; // Non hashed, also when cloned, it links to the same reference and doesn't duplicate this

        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetGameStateHash());
            hash.Add(UniqueId);
            hash.Add(Name);
            hash.Add(Owner);
            hash.Add(Hp.GetGameStateHash());
            hash.Add(DamageTokens);
            return hash.ToHashCode();
        }
        public override object Clone()
        {
            BoardEntity newEntity = (BoardEntity) base.Clone(); // Clones parent first
            // Now my individual elements
            newEntity.Name = Name;
            newEntity.Hp = (Min0Stat) Hp.Clone();
            newEntity.DamageTokens = DamageTokens;
            newEntity.UniqueId = UniqueId;
            return newEntity;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PlacedEntity : BoardEntity
    {
        [JsonProperty]
        public LaneID LaneCoordinate { get; set; } = LaneID.NO_LANE; // Non serialized, doesn't define unit and info is kept in the board serialization
        [JsonProperty]
        public int TileCoordinate { get; set; } = -1; // Non serialized, doesn't define unit and info is kept in the board serialization
        
        public override object Clone()
        {
            PlacedEntity newEntity = (PlacedEntity) base.Clone();
            newEntity.LaneCoordinate = LaneCoordinate;
            newEntity.TileCoordinate = TileCoordinate;
            return newEntity;
        }
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetGameStateHash());
            hash.Add(DamageTokens);
            return hash.ToHashCode();
        }
    }
}
