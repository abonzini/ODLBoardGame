using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with triggers and interactions
    {
        /// <summary>
        /// Processes any arbitraty sequence of effects
        /// </summary>
        /// <param name="effectSequence">Effects are always defined by an initial element of class EffectType </param>
        /// <param name="specificContext"></param>
        void TRIGINTER_ProcessInteraction(EntityBase entity, InteractionType inter, EffectContext specificContext = null)
        {
            if (entity.Interactions != null && entity.Interactions.TryGetValue(inter, out List<Effect> effects))
            {
                EffectContext sequenceContext = null; // TODO: Basically stores local info about this context but not needed right now
                foreach (Effect effect in effects) // Execute series of events for the card in question
                {
                    // Start doing each one
                }
            }
        }
    }
}
