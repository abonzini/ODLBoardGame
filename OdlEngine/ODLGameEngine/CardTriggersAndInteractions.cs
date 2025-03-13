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
    public enum InteractionType
    {
        WHEN_PLAYED
    }
    /// <summary>
    /// What sort of effect is going to be made
    /// </summary>
    public enum EffectType
    {
        SUMMON_UNIT
    }
    /// <summary>
    /// Effect is described by a type and a series of modifiers that define the effect
    /// </summary>
    public class Effect
    {
        public readonly EffectType EffectType;
        public readonly int CardNumber;
        public readonly CardTargets LaneTargets;
    }

    // CONTEXT CONTANERS

    public abstract class EffectContext { }
    /// <summary>
    /// When card is played
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
    // TODO:
    // Building Context
    // Healing Context?
}
