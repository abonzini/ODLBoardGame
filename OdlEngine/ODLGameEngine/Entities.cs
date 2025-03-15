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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
        UNKNOWN,
        UNIT,
        BUILDING,
        SKILL,
        PLAYER
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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardTargets
    {
        GLOBAL = 0,
        PLAINS = 1,
        FOREST = 2,
        MOUNTAIN = 4,
        ALL_BUT_MOUNTAIN = 3,
        ALL_BUT_FOREST = 5,
        ALL_BUT_PLAINS = 6,
        ANY_LANE = 7,
        INVALID = 99
    }

    /// <summary>
    /// The condition that makes the card say "yes, this (lane?) is a valid target"
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TargetCondition
    {
        NONE, /// Can be played always
        // Could be, but only implement as needed
        //BLUEPRINT, /// Subject to blueprint
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
        [JsonConverter(typeof(StringEnumConverter))]
        public EntityType EntityType { get; set; } = EntityType.UNKNOWN;
        [JsonConverter(typeof(StringEnumConverter))]
        public CardTargets TargetOptions { get; set; } = CardTargets.GLOBAL; // Which lane(s) if any the card could work on
        [JsonConverter(typeof(StringEnumConverter))]
        public List<TargetCondition> TargetConditions { get; set; } = new List<TargetCondition>(); // What needs to happen for a card to be "playable" in a lane
        public bool StealthPlay { get; set; } = false; // Whether card triggers a stealth case
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class EntityBase : ICloneable, IHashable
    {
        [JsonProperty]
        public EntityPlayInfo EntityPlayInfo { get; set; } = new EntityPlayInfo(); // Non-hashed, non-cloned as I just want to reference once
        [JsonProperty]
        public EntityPrintInfo EntityPrintInfo { get; set; } // Non-hashed, non-cloned as I just want to reference once
        [JsonProperty]
        public Dictionary<InteractionType, List<Effect>> Interactions { get; set; } = null; // Non hashed, also when cloned, it links to the same reference and doesn't duplicate this

        // TODO: Interactions
        public virtual object Clone()
        {
            object newEntity = MemberwiseClone();
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
        public int Owner { get; set; } = 0;
        [JsonProperty]
        public int Hp { get; set; } = 0;
        [JsonProperty]
        public int DamageTokens { get; set; } = 0;
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetGameStateHash());
            hash.Add(Name);
            hash.Add(Owner);
            hash.Add(Hp);
            hash.Add(DamageTokens);
            return hash.ToHashCode();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PlacedEntity : BoardEntity
    {
        [JsonProperty]
        public int UniqueId { get; set; } = 0;
        [JsonProperty]
        public LaneID LaneCoordinate { get; set; } = LaneID.NO_LANE;
        [JsonProperty]
        public int TileCoordinate { get; set; } = -1;
        // Stealth shenanigans
        [JsonProperty]
        public bool IsHidden { get; set; } = false;
        [JsonProperty]
        public bool IsTheRealOne { get; set; } = true;
        public List<HiddenCorrelation> HiddenCorrelations { get; set; } = new List<HiddenCorrelation>(); // Non-hashed (as it just represents a relation between hashed objects) and non-serialized (only game engine is meant to care)
        public override object Clone()
        {
            object newEntity = MemberwiseClone();
            ((PlacedEntity)newEntity).HiddenCorrelations = new List<HiddenCorrelation>(); // No need to fill as the cloning is normally used to copy card details to board
            return newEntity;
        }
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(UniqueId);
            hash.Add(DamageTokens);
            hash.Add(IsHidden);
            hash.Add(IsTheRealOne);
            return hash.ToHashCode();
        }
    }
}
