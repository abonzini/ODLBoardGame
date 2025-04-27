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
            OngoingEffectContext ctx = new OngoingEffectContext()
            {
                BaseEffectContext = specificContext
            };
            foreach (Effect effect in effects) // Execute series of events for the card in question
            {
                switch (effect.EffectType)
                {
                    case EffectType.DEBUG:
                        ENGINE_AddDebugEvent(ctx);
                        break;
                    case EffectType.FIND_ENTITIES:
                        ctx.EffectTargets = TRIGINTER_GetTargets(entity, effect, specificContext);
                        break;
                    case EffectType.SUMMON_UNIT:
                        // Unit summoning is made without considering cost and the sort, so just go to Playables, play card (for now, may need more complex checking later)
                        for (int i = 0; i < 2; i++) // Check for which player is the summon
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
                            // Find where to play unit
                            if(effect.TargetLocation == TargetLocation.PLAY_TARGET) // If card has the "played" target
                            {
                                UNIT_PlayUnit(playerOwner, auxCardData, ((PlayContext)specificContext).LaneTargets); // Plays the unit same played as original card
                            }
                            else // Otherwise, could be hardcoded lanes
                            {
                                for (int j = 0; j < GameConstants.BOARD_LANES_NUMBER; j++)
                                {
                                    TargetLocation nextLane = (TargetLocation)(1 << j); // Get lanes in order, can be randomized if needed
                                    if(effect.TargetLocation.HasFlag(nextLane)) // If this lane is a valid target for this unt
                                    {
                                        UNIT_PlayUnit(playerOwner, auxCardData, nextLane); // Plays the unit
                                    }
                                }
                            }
                        }
                        break;
                    case EffectType.MODIFIER:
                        switch (effect.ModifierTarget) // Will check wxactly what I need to modify!
                        {
                            case ModifierTarget.TARGET_HP:
                            case ModifierTarget.TARGET_ATTACK:
                            case ModifierTarget.TARGET_MOVEMENT:
                            case ModifierTarget.TARGET_MOVEMENT_DENOMINATOR:
                                { // Ok this means I need to change a stat of a series of targets obtained by FIND_ENTITIES
                                    Action<Stat, int> functionToApply = effect.ModifierOperation switch // Get correct operation
                                    {
                                        ModifierOperation.SET => STATS_SetStat,
                                        ModifierOperation.ADD => STATS_AddToStat,
                                        ModifierOperation.MULTIPLY => STATS_MultiplyStat,
                                        ModifierOperation.ABSOLUTE_SET => STATS_SetAbsoluteBaseStat,
                                        _ => throw new NotImplementedException("Modifier operation not implemented yet"),
                                    };
                                    foreach (int entityTarget in ctx.EffectTargets)
                                    {
                                        BoardEntity nextEntity = DetailedState.EntityData[entityTarget];
                                        Stat targetStat = effect.ModifierTarget switch
                                        {
                                            ModifierTarget.TARGET_HP => nextEntity.Hp,
                                            ModifierTarget.TARGET_ATTACK => ((Unit)nextEntity).Attack,
                                            ModifierTarget.TARGET_MOVEMENT => ((Unit)nextEntity).Movement,
                                            ModifierTarget.TARGET_MOVEMENT_DENOMINATOR => ((Unit)nextEntity).MovementDenominator,
                                            _ => throw new NotImplementedException("Modifier type not implemented yet")
                                        };
                                        // Got the stat, now modify it
                                        functionToApply(targetStat, effect.Value);
                                    }
                                }
                                break;
                            default:
                                throw new NotImplementedException("Modifier type not implemented yet");
                        }
                        break;
                    default:
                        throw new NotImplementedException("Effect type not implemented yet");
                }
            }
        }
        /// <summary>
        /// Just a bunch of enums for the target-finding state machine
        /// </summary>
        enum TargetingStateMachine
        {
            BEGIN,
            INITIAL_PLAYER,
            GET_ENTITIES_IN_REGION,
            TARGETS_IN_REGION,
            LAST_PLAYER,
            END
        }
        /// <summary>
        /// Function that gets serch parameters as well as reference observer, and returns a series of valid entity targets.
        /// </summary>
        /// <param name="entityRef">Observer, i.e. which side of the board</param>
        /// <param name="searchParameters">Contains extra info necessary for the search</param>
        /// <returns></returns>
        List<int> TRIGINTER_GetTargets(EntityBase entityRef, Effect searchParameters, EffectContext specificContext)
        {
            // If nothing to search for, just return an empty list
            if(searchParameters.TargetPlayer == EntityOwner.NONE || searchParameters.TargetType == EntityType.NONE || (searchParameters.SearchCriterion == SearchCriterion.QUANTITY && searchParameters.Value == 0))
            {
                return new List<int>();
            }
            List<int> res = new List<int>();
            // Search variables
            bool reverseSearch = false; // Order of search
            int requiredTargets; // How many targets I'll extract maximum
            bool laneIsTarget = false; // Target is a lane
            bool tileSectioning = false; // When searching along a lane, check tile-by-tile or the whole body as is
            BoardElement targetBoardArea; // The board area that will be searched
            int referencePlayer; // Order reference depends on indexing
            int playerFilter = -1; // Filter of which player to search for (defautl is -1 both players)
            int n = searchParameters.Value;
            // Prepare settings/masks for this target search
            switch (searchParameters.TargetLocation)
            {
                case TargetLocation.BOARD: // Return board
                    targetBoardArea = DetailedState.BoardState;
                    break;
                case TargetLocation.PLAINS: // For lane-based, get the lane
                case TargetLocation.FOREST:
                case TargetLocation.MOUNTAIN:
                    targetBoardArea = DetailedState.BoardState.GetLane(searchParameters.TargetLocation);
                    laneIsTarget = true;
                    break;
                case TargetLocation.PLAY_TARGET: // Need to check where the card had been played
                    targetBoardArea = DetailedState.BoardState.GetLane(((PlayContext)specificContext).LaneTargets);
                    laneIsTarget = true;
                    break;
                default:
                    throw new NotImplementedException("Search not yet implemented for other targets!"); // This may be a later TODO once new needs appear
            }
            if (laneIsTarget && (searchParameters.SearchCriterion != SearchCriterion.ALL)) // If target is everything, no need to check tile by tile
            {
                tileSectioning = true;
            }
            if (n < 0) // Negative indexing implies reverse indexing
            {
                n *= -1;
                if(searchParameters.SearchCriterion == SearchCriterion.ORDINAL) // In ordinals, -1 is 0 from reverse. In quant, -1 is 1 in reverse
                {
                    n -= 1; // Need to index starting from 0!
                }
                reverseSearch = true;
            }
            referencePlayer = reverseSearch ? 1 - entityRef.Owner : entityRef.Owner;
            requiredTargets = searchParameters.SearchCriterion switch
            {
                SearchCriterion.ORDINAL => 1, // Only one target in position N
                SearchCriterion.QUANTITY => n, // First/last N
                SearchCriterion.ALL => int.MaxValue, // Get everything possible
                _ => throw new NotImplementedException("Invalid search criterion"),
            };
            if(searchParameters.TargetPlayer == EntityOwner.OWNER)
            {
                playerFilter = entityRef.Owner;
            }
            else if (searchParameters.TargetPlayer == EntityOwner.OPPONENT)
            {
                playerFilter = 1 - entityRef.Owner;
            }
            // Search loop, continues until I get the number of targets I want or nothing else to look for
            TargetingStateMachine currentSearchState = TargetingStateMachine.BEGIN;
            SortedSet<int> entities = null; // Container of location's entities
            int tileCounter = 0; // Aux counter to explore lane in order
            int localOrdinal = 0, totalOrdinal = 0;
            while (res.Count < requiredTargets && currentSearchState != TargetingStateMachine.END)
            {
                int nextCandidateEntity = -1; // Start with a (invalid possible unit candidate)
                switch (currentSearchState) // Checks values, last thing it does is to add candidate to evaluate, and possible next state
                {
                    case TargetingStateMachine.BEGIN:
                        currentSearchState = TargetingStateMachine.INITIAL_PLAYER; // Nothign to do but to go to next state
                        break;
                    case TargetingStateMachine.INITIAL_PLAYER:
                        if (searchParameters.TargetType.HasFlag(EntityType.PLAYER)) // Player targeting enabled, check if player is valid
                        {
                            // Need to add if player (w.r.t to card ownership) are valid targets 
                            if (reverseSearch && searchParameters.TargetPlayer.HasFlag(EntityOwner.OPPONENT)) // If searching backwards and opponent enabled, then add opp
                            {
                                nextCandidateEntity = 1 - entityRef.Owner;
                            }
                            else if (!reverseSearch && searchParameters.TargetPlayer.HasFlag(EntityOwner.OWNER)) // If searching normally and owner enabled, then add it
                            {
                                nextCandidateEntity = entityRef.Owner;
                            }
                        }
                        currentSearchState = TargetingStateMachine.GET_ENTITIES_IN_REGION; // Once finished, go to next state (get elements to search for entities)
                        break;
                    case TargetingStateMachine.GET_ENTITIES_IN_REGION: // Gets the list of entities in the desired board element, also checks if need to subdivide lane
                        if(tileSectioning) // Lane needs to be divided in tiles
                        {
                            if(tileCounter < ((Lane)targetBoardArea).Len) // Check if I still have ongoing lane available
                            {
                                entities = ((Lane)targetBoardArea).GetTileRelative(tileCounter, referencePlayer).GetPlacedEntities(searchParameters.TargetType, playerFilter); // Search for requested target for requested players
                                tileCounter++;
                                currentSearchState = TargetingStateMachine.TARGETS_IN_REGION;
                            }
                            else // Ran out of lane, finished region
                            {
                                currentSearchState = TargetingStateMachine.LAST_PLAYER;
                            }
                        }
                        else // Otherwise just get units in the region
                        {
                            entities = targetBoardArea.GetPlacedEntities(searchParameters.TargetType, playerFilter);
                            currentSearchState = TargetingStateMachine.TARGETS_IN_REGION;
                        }
                        localOrdinal = 0;
                        break;
                    case TargetingStateMachine.TARGETS_IN_REGION: // This searches the region to add as many entities as it can find
                        if(localOrdinal < entities.Count) // Can still explore this one
                        {
                            nextCandidateEntity = entities.ElementAt(localOrdinal); // Get next element
                            localOrdinal++;
                        }
                        else // Means I finished region
                        {
                            if(tileSectioning) // May want to look for next tile (if available)
                            {
                                currentSearchState = TargetingStateMachine.GET_ENTITIES_IN_REGION;
                            }
                            else // Finished whole region, go to next part
                            {
                                currentSearchState = TargetingStateMachine.LAST_PLAYER;
                            }
                        }
                        break;
                    case TargetingStateMachine.LAST_PLAYER: // Identical to other player but reverse logic
                        if (searchParameters.TargetType.HasFlag(EntityType.PLAYER))
                        {
                            if (!reverseSearch && searchParameters.TargetPlayer.HasFlag(EntityOwner.OPPONENT))
                            {
                                nextCandidateEntity = 1 - entityRef.Owner;
                            }
                            else if (reverseSearch && searchParameters.TargetPlayer.HasFlag(EntityOwner.OWNER))
                            {
                                nextCandidateEntity = entityRef.Owner;
                            }
                        }
                        currentSearchState = TargetingStateMachine.END; // Finished the board.. nothign else to look for
                        break;
                    case TargetingStateMachine.END:
                        break;
                }
                if(nextCandidateEntity != -1) // Found next potential candidate entity, need to verify if applies
                {
                    bool addEntity = true;
                    if(searchParameters.SearchCriterion == SearchCriterion.ORDINAL) // Need to ensure the unit # matches!
                    {
                        if(totalOrdinal != n) // Not the entity I was looking for, unfortunately
                        {
                            addEntity = false;
                        }
                    }
                    if (addEntity) // Found a valid candidate, so i add the entity
                    {
                        res.Add(nextCandidateEntity);
                    }
                    totalOrdinal++; // Regardless, I already examined this entity, update the global ordinal
                }
            }
            return res;
        }
    }
}
