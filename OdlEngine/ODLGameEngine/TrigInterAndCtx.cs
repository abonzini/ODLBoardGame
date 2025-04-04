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
        SUMMON_UNIT
    }
    /// <summary>
    /// Player who is target of a card effect
    /// </summary>
    [JsonConverter(typeof(FlagEnumJsonConverter))]
    [Flags]
    public enum PlayerTarget
    {
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
        public int CardNumber;
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public PlayerTarget TargetPlayer;
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public CardTargets LaneTargets;
        /*
         * TODO:
         * Entity targets, first, last, random, all (create enum)
         * Entity type filter, unit, building, player, (spell?!)
         * Combined with lane and player, we can quickly do a series of filterings and get target(s) for an effect
         */
    }

    // CONTEXT CONTANERS
    public abstract class EffectContext { }
    /// <summary>
    /// When card is played, contains extra info about playability of card.
    /// TODO: In future may contain extra info or even modifiers depending what happens
    /// </summary>
    public class PlayContext : EffectContext
    {
        public CardTargets LaneTargets;
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
