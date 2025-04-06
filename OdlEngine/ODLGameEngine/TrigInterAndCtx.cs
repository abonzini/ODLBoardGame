using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    // Triggers and interactions
    /// <summary>
    /// When the interaction is triggered
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InteractionType
    {
        WHEN_PLAYED
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerType
    {
        DEBUG_TRIGGER
    }
    // Effects
    /// <summary>
    /// What sort of effect is going to be made
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EffectType
    {
        DEBUG,
        FIND_ENTITIES,
        SUMMON_UNIT
    }
    /// <summary>
    /// When searching for a target, which entity is found
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchCriterion
    {
        ORDINAL,
        NUMBERED,
        ALL
    }
    /// <summary>
    /// Player who is target of a card effect
    /// </summary>
    [JsonConverter(typeof(FlagEnumJsonConverter))]
    [Flags]
    public enum EntityOwner
    {
        NONE = 0,
        OWNER = 1,
        OPPONENT = 2,
        BOTH = 3,
    }
    /// <summary>
    /// Effect is described by a type and a series of modifiers that define the effect
    /// </summary>
    public class Effect
    {
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EffectType EffectType;
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public TargetLocation TargetLocation;
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EntityOwner TargetPlayer;
        [JsonConverter(typeof(StringEnumConverter))]
        public SearchCriterion SearchCriterion;
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EntityType TargetType;
        public int CardNumber;
        public int Value;
    }

    // CONTEXT CONTANERS
    public abstract class EffectContext { }
    /// <summary>
    /// When card is played, contains extra info about playability of card.
    /// TODO: In future may contain extra info or even modifiers depending what happens
    /// </summary>
    public class PlayContext : EffectContext
    {
        public TargetLocation LaneTargets;
    }
    /// <summary>
    /// When a damage step ocurred, maybe also death
    /// </summary>
    public class DamageContext : EffectContext
    {
        public EntityBase AttackingEntity = null;
        public BoardEntity DefendingEntity = null;
        public int DamageAmount = 0;
        public int OverflowDamage = 0;
        public bool TargetDead = false;
    }
    /// <summary>
    /// When unit is advancing
    /// </summary>
    public class AdvancingContext : EffectContext
    {
        public Unit AdvancingUnit = null;
        public int InitialMovement = 0;
        public int CurrentMovement = 0;
    }
    /// <summary>
    /// When a construction takes place
    /// </summary>
    public class ConstructionContext : EffectContext
    {
        public Building Building = null;
        public Unit Builder = null;
        public int AbsoluteTile = -1;
        public int RelativeTile = -1;
        public int FirstAvailableOption = -1;
    }
}
