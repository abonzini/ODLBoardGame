﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace ODLGameEngine
{
    /// <summary>
    /// Describes what type of entity a card is
    /// </summary>
    [JsonConverter(typeof(FlagEnumJsonConverter))]
    [Flags]
    public enum EntityType
    {
        NONE = 0,
        UNIT = 1,
        BUILDING = 2,
        SKILL = 4,
        PLAYER = 8
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
    /// Defines what sort of target a card has, normally fixed on many card type but skills may have different ones
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardTargetingType
    {
        BOARD,
        LANE,
        TILE,
        UNIT,
        BUILDING,
        UNIT_AND_BUILDING
    }
    /// <summary>
    /// Will define how a card looks and works before it creates an instance in the board
    /// </summary>
    public class CardIllustrationInfo
    {
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EntityType EntityType { get; set; } = EntityType.NONE;
        public int Id { get; set; } = 0;
        [DefaultValue("")]
        public string Name { get; set; } = "";
        [DefaultValue("")]
        public string Text { get; set; } = "";
        // Playable info (will be in card)
        [DefaultValue("")]
        public string Cost { get; set; } = "";
        [DefaultValue("")]
        public string Hp { get; set; } = "";
        [DefaultValue("")]
        public string Movement { get; set; } = "";
        [DefaultValue("")]
        public string Attack { get; set; } = "";
        public int Rarity { get; set; } = 0;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExpansionId Expansion { get; set; } = ExpansionId.VANILLA;
        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerTribe Tribe { get; set; } = PlayerTribe.BASE;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class EntityBase : ICloneable
    {
        [JsonProperty]
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EntityType EntityType { get; set; } = EntityType.NONE;
        [JsonProperty]
        public HashSet<int> TargetOptions { get; set; } = null; // Which are the valid targets for this, meaning depends on the type of card
        [JsonProperty]
        public int Id { get; set; } = 0;
        [JsonProperty]
        public int Cost { get; set; } = 0;
        [JsonProperty]
        public int Rarity { get; set; } = 0;
        [JsonProperty]
        public Dictionary<InteractionType, List<Effect>> Interactions { get; set; } = null; // Non hashed, also when cloned, it links to the same reference and doesn't duplicate this

        public virtual object Clone()
        {
            EntityBase newEntity = (EntityBase)MemberwiseClone();
            return newEntity;
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Id); // Important as this essentially describes the entity's interactions (and triggers)
            return hash.ToHashCode();
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class IngameEntity : EntityBase
    {
        [JsonProperty]
        public int Owner { get; set; } = 0;
        [JsonProperty]
        public int UniqueId { get; set; } = 0;
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Owner);
            return hash.ToHashCode();
        }
        /// <summary>
        /// Cloning
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            IngameEntity newEntity = (IngameEntity)base.Clone(); // Clones parent first
            // Now my individual elements
            newEntity.Owner = Owner;
            newEntity.UniqueId = UniqueId;
            return newEntity;
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class LivingEntity : IngameEntity
    {
        [JsonProperty]
        [DefaultValue("")]
        public string Name { get; set; } = "";
        [JsonProperty]
        [JsonConverter(typeof(StatJsonConverter))]
        public Min0Stat Hp { get; set; } = new Min0Stat();
        [JsonProperty]
        public int DamageTokens { get; set; } = 0;
        [JsonProperty]
        public Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>> Triggers { get; set; } = null;
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(Hp.GetHashCode());
            hash.Add(DamageTokens);
            return hash.ToHashCode();
        }
        public override object Clone()
        {
            LivingEntity newEntity = (LivingEntity)base.Clone(); // Clones parent first
            // Now my individual elements
            newEntity.Name = Name;
            newEntity.Hp = (Min0Stat)Hp.Clone();
            newEntity.DamageTokens = DamageTokens;
            return newEntity;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PlacedEntity : LivingEntity
    {
        [JsonProperty]
        [DefaultValue(-1)]
        public int TileCoordinate { get; set; } = -1;
        public override object Clone()
        {
            PlacedEntity newEntity = (PlacedEntity)base.Clone();
            newEntity.TileCoordinate = TileCoordinate;
            return newEntity;
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            return hash.ToHashCode();
        }
    }
}