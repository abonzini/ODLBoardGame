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
        NONE,
        WHEN_PLAYED,
        UNIT_ENTERS_BUILDING,
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerType
    {
        NONE,
        DEBUG_TRIGGER
    }
    // Effects
    /// <summary>
    /// What sort of effect is going to be made
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EffectType
    {
        TRIGGER_DEBUG,
        DEBUG_STORE,
        SELECT_ENTITY,
        FIND_ENTITIES,
        SUMMON_UNIT,
        MODIFIER
    }
    /// <summary>
    /// When searching for a target, which entity is found
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchCriterion
    {
        NOTHING,
        ORDINAL,
        QUANTITY,
        ALL,
        EFFECT_OWNING_ENTITY,
        ACTOR_ENTITY,
        AFFECTED_ENTITY
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
    /// Type of modifier the effect has
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ModifierOperation
    {
        NONE,
        SET,
        ADD,
        MULTIPLY,
        ABSOLUTE_SET
    }
    /// <summary>
    /// Source/target of data for input/output
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Variable
    {
        TEMP_VARIABLE,
        ACC,
        TARGET_HP,
        TARGET_ATTACK,
        TARGET_MOVEMENT,
        TARGET_MOVEMENT_DENOMINATOR,
        PLAYERS_GOLD,
        // Todo, damage, advancement, playability check, etc
    }
    /// <summary>
    /// Register to use in an effect
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MultiInputProcessing
    {
        FIRST,
        COUNT,
        SUM,
        AVERAGE,
        MAX,
        MIN
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
        [JsonConverter(typeof(StringEnumConverter))]
        public ModifierOperation ModifierOperation;
        [JsonConverter(typeof(StringEnumConverter))]
        public Variable Input;
        [JsonConverter(typeof(StringEnumConverter))]
        public Variable Output;
        [JsonConverter(typeof(StringEnumConverter))]
        public MultiInputProcessing MultiInputProcessing;
        public int TempVariable;

        public override string ToString()
        {
            return EffectType.ToString();
        }
    }

    // CONTEXT CONTANERS
    /// <summary>
    /// An effect, contains an actor (i.e. a doer of the activity in question).
    /// Also contains a Triggered entity which is either the actor in interactions, but in triggers it can be a third party.
    /// </summary>
    public class EffectContext
    {
        public IngameEntity ActivatedEntity = null;
        public IngameEntity Actor = null;
    }
    /// <summary>
    /// When card is played, contains extra info about playability of card.
    /// TODO: In future may contain extra info or even modifiers depending what happens
    /// </summary>
    public class PlayContext : EffectContext
    {
        public TargetLocation LaneTargets;
    }
    /// <summary>
    /// An action that occurs upon an affected entity (like when an entity is being built, attacked, etc). Logically these can only be BoardEntities
    /// </summary>
    public class AffectingEffectContext : EffectContext
    {
        public LivingEntity Affected = null;
    }
    /// <summary>
    /// When a damage step ocurred, maybe also death
    /// </summary>
    public class DamageContext : AffectingEffectContext
    {
        public int DamageAmount = 0;
        public int OverflowDamage = 0;
        public bool TargetDead = false;
    }
    /// <summary>
    /// When unit is advancing
    /// </summary>
    public class MarchingContext : EffectContext
    {
        public int InitialMovement = 0;
        public int CurrentMovement = 0;
    }
    /// <summary>
    /// When a construction takes place
    /// </summary>
    public class ConstructionContext : AffectingEffectContext
    {
        public int AbsoluteTile = -1;
        public int RelativeTile = -1;
        public int FirstAvailableOption = -1;
    }
    /// <summary>
    /// When something (I guess unit) steps on a building
    /// </summary>
    public class EntersBuildingContext : AffectingEffectContext
    {

    }
    /// <summary>
    /// The CPU state of effect chain resolutions
    /// </summary>
    public class CpuState
    {
        public EffectContext CurrentSpecificContext = null;
        public List<int> ReferenceEntities;
        public int TempValue = 0;
        public int Acc = 0;
    }
}
