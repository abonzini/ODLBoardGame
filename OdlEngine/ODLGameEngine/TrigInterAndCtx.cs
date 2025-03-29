using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    // FLEXIBLE DEFINITION OF EFFECTS
    /// <summary>
    /// When the interaction is triggered
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InteractionType
    {
        WHEN_PLAYED
    }
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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlayerTarget
    {
        CARD_PLAYER,
        CARD_PLAYER_OPPONENT
    }
    /// <summary>
    /// Effect is described by a type and a series of modifiers that define the effect
    /// </summary>
    public class Effect
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EffectType EffectType;
        public int CardNumber;
        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerTarget TargetPlayer;
        [JsonConverter(typeof(StringEnumConverter))]
        public CardTargets LaneTargets;
    }

    // CONTEXT CONTANERS
    public abstract class EffectContext { }
    /// <summary>
    /// When card is played
    /// </summary>
    public class PlayContext : EffectContext
    {
        public int Player;
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
    // TODO:
    // Playability context? E.g for activities like player, card, target, etc. Pass it to entity init
}
