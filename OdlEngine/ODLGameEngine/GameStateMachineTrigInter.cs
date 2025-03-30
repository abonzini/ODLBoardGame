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
        void TRIGINTER_ProcessInteraction(EntityBase entity, InteractionType inter, EffectContext specificContext)
        {
            if (entity.Interactions != null && entity.Interactions.TryGetValue(inter, out List<Effect> effects))
            {
                TRIGINTER_ProcessEffects(effects, specificContext);
            }
        }

        /// <summary>
        /// Excecutes all effects of a specific trigger
        /// </summary>
        /// <param name="trigger">Trigger to run</param>
        /// <param name="specificContext">Context of trigger, if available</param>
        void TRIGINTER_ProcessTrigger(TriggerType trigger, EffectContext specificContext)
        {
            if (DetailedState.Triggers.TryGetValue(trigger, out SortedSet<int> entities)) // Find if there's entities with this trigger
            {
                foreach (int entity in entities)
                {
                    List<Effect> effects = GetBoardEntity(entity).Triggers[trigger]; // Get unit's effects for this trigger
                    TRIGINTER_ProcessEffects(effects, specificContext);
                }

            }
        }
        /// <summary>
        /// Executes a list of effects for triggers or interactions
        /// </summary>
        /// <param name="effects">List of effects to execute</param>
        /// <param name="specificContext">Additional context that accompanies the desired effect (e.g. when killed, implies killed by someone, etc)</param>
        void TRIGINTER_ProcessEffects(List<Effect> effects, EffectContext specificContext)
        {
            int auxInt;
            EntityBase auxCardData;
            foreach (Effect effect in effects) // Execute series of events for the card in question
            {
                switch (effect.EffectType)
                {
                    case EffectType.DEBUG:
                        ENGINE_AddDebugEvent();
                        break;
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
