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
                int auxInt;
                EntityBase auxCardData;
                //EffectContext sequenceContext = null; // TODO: Basically stores local info about this context but not needed right now
                foreach (Effect effect in effects) // Execute series of events for the card in question
                {
                    switch(effect.EffectType)
                    {
                        case EffectType.EFFECT_EXCEPTION:
                            throw new EffectDrivenException("Card's effect has triggered an exception on purpose");
                        case EffectType.SUMMON_UNIT:
                            // Unit summoning is made without considering cost and the sort, so just go to Playables, play card
                            auxCardData = CardDb.GetCard(effect.CardNumber);
                            auxInt = effect.TargetPlayer switch
                            {
                                PlayerTarget.CARD_PLAYER => ((PlayContext)specificContext).Player,
                                PlayerTarget.CARD_PLAYER_OPPONENT => 1 - ((PlayContext)specificContext).Player,
                                _ => throw new NotImplementedException("Invalid player target"),
                            };
                            for (int i = 0; i < GameConstants.BOARD_LANES_NUMBER; i++)
                            {
                                CardTargets nextLane = (CardTargets)(1 << i); // Get lanes in order, can be randomized if needed
                                if(effect.LaneTargets.HasFlag(nextLane)) // If this lane is a valid target for this unt
                                {
                                    UNIT_PlayUnit(auxInt, (Unit)auxCardData, nextLane); // Plays the unit
                                }
                            }
                            break;
                        default:
                            throw new NotImplementedException("Effect type not implemented yet");
                    }
                }
            }
        }
    }
}
