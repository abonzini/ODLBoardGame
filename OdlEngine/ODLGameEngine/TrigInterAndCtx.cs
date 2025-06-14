using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        PRE_DAMAGE,
        POST_DAMAGE
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TriggerType
    {
        ON_DEBUG_TRIGGERED,
        ON_MARCH,
        ON_END_OF_TURN
    }
    // Effects
    /// <summary>
    /// What sort of effect is going to be made
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EffectType
    {
        STORE_DEBUG_IN_EVENT_PILE,
        ACTIVATE_TEST_TRIGGER_IN_LOCATION,
        SELECT_ENTITY,
        FIND_ENTITIES,
        ADD_LOCATION_REFERENCE,
        SUMMON_UNIT,
        MODIFIER,
        ASSERT,
        KILL_ENTITIES,
        EFFECT_DAMAGE
        // TODO: Board element operations like shuffle, trim, crop, select
        // TODO: Tile extension operations to overwrite search, do AOE and other stuff
    }
    /// <summary>
    /// When searchign for a target, which location is searched
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EffectLocation
    {
        BOARD = 0,
        PLAINS,
        FOREST,
        MOUNTAIN,
        PLAY_TARGET,
        CURRENT_TILE
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
        AFFECTED_ENTITY,
        PLAY_TARGET_ENTITY
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
        ABSOLUTE_SET,
        NOT
    }
    /// <summary>
    /// Source/target of data for input/output
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Variable
    {
        TEMP_VARIABLE,
        ACC,
        TARGET_COUNT,
        TARGET_HP,
        TARGET_ATTACK,
        TARGET_MOVEMENT,
        TARGET_MOVEMENT_DENOMINATOR,
        PLAYERS_GOLD,
        MARCH_START_FLAG,
        MARCH_CURRENT_MOVEMENT,
        DAMAGE_AMOUNT
    }
    /// <summary>
    /// Register to use in an effect
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MultiInputProcessing
    {
        FIRST,
        SUM,
        AVERAGE,
        MAX,
        MIN,
        EACH,
    }
    /// <summary>
    /// Effect is described by a type and a series of modifiers that define the effect
    /// </summary>
    public class Effect
    {
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EffectType EffectType;
        [JsonConverter(typeof(StringEnumConverter))]
        public EffectLocation EffectLocation;
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
    /// Context when there's end of turn effects happening. Actor is the player ending their turn
    /// </summary>
    public class EndOfTurnContext : EffectContext
    {
    }
    /// <summary>
    /// Context regarding playing a card! This is helpful to determine both how a card could be played
    /// But also is raised as a context when a card has just been played, and will be port of entry when such an interaction occurs
    /// </summary>
    public class PlayContext : EffectContext
    {
        public CardTargetingType TargetingType = CardTargetingType.BOARD;
        public HashSet<int> ValidTargets = null;
        public int PlayedTarget = -1;
        public int PlayCost = 0;
        public PlayType PlayType = PlayType.PLAY_FROM_HAND;
        public PlayOutcome PlayOutcome = PlayOutcome.NO_TARGET_AVAILABLE;
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
        public bool FirstTileMarch = false;
        public int CurrentMovement = 0;
    }
    /// <summary>
    /// When a construction takes place
    /// </summary>
    public class ConstructionContext : AffectingEffectContext
    {
        public int AbsoluteConstructionTile = -1;
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
        public List<BoardElement> ReferenceLocations = [];
        public int TempValue = 0;
        public int Acc = 0;
    }
}
