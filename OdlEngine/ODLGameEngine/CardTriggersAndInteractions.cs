using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /*
     * TODO:
     * enum interaction type
     * class for filtering interactions and the sort?
     * Effect, with structure:
     * effect type -> bag of caharacteristics such as damage, damage value (abs, delta, overflow, whatever is needed)
     */
    public abstract class EffectContext { }
    public class DamageContext : EffectContext
    {
        public EntityBase AttackingEntity = null;
        public BoardEntity DefendingEntity = null;
        public int DamageAmount = 0;
        public int OverflowDamage = 0;
        public bool TargetDead = false;
    }

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
