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
        /// Processes the given interactions of a given entity
        /// </summary>
        /// <param name="entity">The enitity whose interactions were triggered</param>
        /// <param name="inter">What interaction</param>
        /// <param name="specificContext">Specific context of that interaction</param>
        void TRIGINTER_ProcessInteraction(EntityBase entity, InteractionType inter, EffectContext specificContext)
        {
            if (entity.Interactions != null && entity.Interactions.TryGetValue(inter, out List<Effect> effects)) // This entity has an interaction
            {
                TRIGINTER_ProcessEffects(entity, effects, specificContext);
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
                    BoardEntity entityData = DetailedState.EntityData[entity]; // Triger will only apply when entity still exists
                    if (entityData != null)
                    {
                        List<Effect> effects = entityData.Triggers[trigger]; // Get unit's effects for this trigger
                        TRIGINTER_ProcessEffects(entityData, effects, specificContext);
                    }
                }

            }
        }
        /// <summary>
        /// Executes a list of effects for triggers or interactions
        /// </summary>
        /// <param name="entity">The entity that is going to "perform" the effects</param>
        /// <param name="effects">List of effects to execute</param>
        /// <param name="specificContext">Additional context that accompanies the desired effect (e.g. when killed, implies killed by someone, etc)</param>
        void TRIGINTER_ProcessEffects(EntityBase entity, List<Effect> effects, EffectContext specificContext)
        {
            List<int> targets; // Potential target for potential card effects
            foreach (Effect effect in effects) // Execute series of events for the card in question
            {
                switch (effect.EffectType)
                {
                    case EffectType.DEBUG:
                        ENGINE_AddDebugEvent();
                        break;
                    case EffectType.FIND_ENTITIES:
                        targets = TRIGINTER_GetTargets(entity, effect);
                        break;
                    case EffectType.SUMMON_UNIT:
                        // Unit summoning is made without considering cost and the sort, so just go to Playables, play card (for now, may need more complex checking later)
                        for (int i = 0; i < 2; i++) // Check for which order is the summon
                        {
                            EntityOwner nextPossibleOwner = (EntityOwner)(1 << i);
                            int playerOwner;
                            if(effect.TargetPlayer.HasFlag(nextPossibleOwner)) // Valid owner!
                            {
                                playerOwner = effect.TargetPlayer switch
                                {
                                    EntityOwner.OWNER => entity.Owner,
                                    EntityOwner.OPPONENT => 1 - entity.Owner,
                                    _ => throw new NotImplementedException("Invalid player target"),
                                };
                            }
                            else // If this owner option invalid, then move to the next one
                            {
                                continue;
                            }
                            Unit auxCardData = (Unit)CardDb.GetCard(effect.CardNumber);
                            for (int j = 0; j < GameConstants.BOARD_LANES_NUMBER; j++)
                            {
                                TargetLocation nextLane = (TargetLocation)(1 << j); // Get lanes in order, can be randomized if needed
                                if(effect.TargetLocation.HasFlag(nextLane)) // If this lane is a valid target for this unt
                                {
                                    UNIT_PlayUnit(playerOwner, auxCardData, nextLane); // Plays the unit
                                }
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException("Effect type not implemented yet");
                }
            }
        }
        /// <summary>
        /// Function that gets serch parameters as well as reference observer, and returns a series of valid entity targets.
        /// </summary>
        /// <param name="entityRef">Observer, i.e. which side of the board</param>
        /// <param name="searchParameters">Contains extra info necessary for the search</param>
        /// <returns></returns>
        List<int> TRIGINTER_GetTargets(EntityBase entityRef, Effect searchParameters)
        {
            List<int> res = new List<int>();
            // Obviously TODO
            return res;
        }
    }
}
