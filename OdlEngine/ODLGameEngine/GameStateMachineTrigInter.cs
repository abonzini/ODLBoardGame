﻿using System;
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
        void TRIGINTER_ProcessInteraction(InteractionType inter, EffectContext specificContext)
        {
            if (specificContext.ActivatedEntity.Interactions != null && specificContext.ActivatedEntity.Interactions.TryGetValue(inter, out List<Effect> effects)) // This entity has an interaction of this type
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
                    LivingEntity entityData = DetailedState.EntityData[entity]; // Triger will only apply when entity still exists
                    if (entityData != null)
                    {
                        specificContext.ActivatedEntity = entityData; // For the next trigger, this entity is the one
                        List<Effect> effects = entityData.Triggers[trigger]; // Get unit's effects for this trigger
                        TRIGINTER_ProcessEffects(effects, specificContext);
                    }
                }
            }
        }
        
        readonly Dictionary<int, CpuState> _chainContext = new Dictionary<int, CpuState>(); // To be used only in effect resolution chain
        IngameEntity FetchEntity(int id) // Helper fn that gets the desired entity from an id, can search for skills so I need to operate this properly
        {
            // Returns skill data (located in the bottom of effect chains), or just get the entity from the entity database
            return (id == -1) ? _chainContext[id].CurrentSpecificContext.ActivatedEntity : DetailedState.EntityData[id];
        }
        /// <summary>
        /// Executes a list of effects for triggers or interactions
        /// </summary>
        /// <param name="entity">The entity that is going to "perform" the effects</param>
        /// <param name="effects">List of effects to execute</param>
        /// <param name="specificContext">Additional context that accompanies the desired effect (e.g. when killed, implies killed by someone, etc)</param>
        void TRIGINTER_ProcessEffects(List<Effect> effects, EffectContext specificContext)
        {
            // Get unique ID of activated entity. Skills don't have these as they're volatile, so I assign a temporary value of -1
            int activatedEntityId = specificContext.ActivatedEntity.UniqueId;
            bool firstEntryInChain = false;
            CpuState cpu;
            if (_chainContext.TryGetValue(activatedEntityId, out CpuState value))
            {
                cpu = value;
            }
            else
            {
                cpu = new CpuState()
                {
                    ReferenceEntities = [activatedEntityId], // by default, entity starts with itself as a reference
                };
                _chainContext.Add(activatedEntityId, cpu);
                firstEntryInChain = true;
            }
            cpu.CurrentSpecificContext = specificContext;
            // Now that the CPU has been configured, can execute effect chain
            foreach (Effect effect in effects) // Execute series of events for the card in question
            {
                // Define values of registers as may be needed
                cpu.TempValue = effect.TempVariable;
                int inputValue = GetInput(cpu, effect.Input, effect.MultiInputProcessing);
                // Now to process the effect
                switch (effect.EffectType)
                {
                    case EffectType.TRIGGER_DEBUG:
                        TRIGINTER_ProcessTrigger(TriggerType.DEBUG_TRIGGER, new EffectContext()); // Triggers a debug step, to test trigger hooks
                        break;
                    case EffectType.DEBUG_STORE:
                        ENGINE_AddDebugEvent(cpu); // Stores triginter CPU context for checking of intermediate values
                        break;
                    case EffectType.SELECT_ENTITY:
                        { // In this case there's a simple, single BoardEntity target related to the ctx in question
                            List<int> res = new List<int>();
                            IngameEntity tgt = effect.SearchCriterion switch
                            {
                                SearchCriterion.EFFECT_OWNING_ENTITY => specificContext.ActivatedEntity,
                                SearchCriterion.ACTOR_ENTITY => specificContext.Actor,
                                SearchCriterion.AFFECTED_ENTITY => ((AffectingEffectContext)specificContext).Affected,
                                _ => throw new NotImplementedException("Invalid target for entity selection")
                            };
                            if(effect.TargetType.HasFlag(tgt.EntityType)) // The unit is of the valid type
                            {
                                // Determine who owns this unit then
                                EntityOwner owner = (tgt.Owner == specificContext.ActivatedEntity.Owner) ? EntityOwner.OWNER : EntityOwner.OPPONENT;
                                if(effect.TargetPlayer.HasFlag(owner)) // If it's a valid owner, then target is valid
                                {
                                    res.Add(tgt.UniqueId); // This is now the target
                                }
                            }
                            cpu.ReferenceEntities = res;
                        }
                        break;
                    case EffectType.FIND_ENTITIES:
                        // Searches for entities, reference being the selected reference (since there can be multiple references, a single one (the first) is used
                        cpu.ReferenceEntities = GetTargets(effect.TargetPlayer, effect.TargetType, effect.SearchCriterion, GetTargetLocationsFromReference(effect.TargetLocation, cpu)[0], inputValue, FetchEntity(cpu.ReferenceEntities[0]).Owner);
                        break;
                    case EffectType.SUMMON_UNIT:
                        {
                            TargetLocation[] targets = GetTargetLocationsFromReference(effect.TargetLocation, cpu); // Get the actual place where this is played, even if relative
                            for (int i = 0; i < cpu.ReferenceEntities.Count; i++) // Summon sth for each entity found (!!)
                            {
                                IngameEntity entity = FetchEntity(cpu.ReferenceEntities[i]); // Got the next reference entity
                                if (effect.TargetPlayer.HasFlag(EntityOwner.OWNER)) // Will its owner or its opponent get the unit?
                                {
                                    SummonUnitToPlayer(entity.Owner, inputValue, targets[i]);
                                }
                                if (effect.TargetPlayer.HasFlag(EntityOwner.OPPONENT))
                                {
                                    SummonUnitToPlayer(1 - entity.Owner, inputValue, targets[i]);
                                }
                            }
                        }
                        break;
                    case EffectType.MODIFIER:
                        switch (effect.Output) // Will check exactly what I need to modify!
                        {
                            case Variable.ACC:
                                cpu.Acc = GetModifiedValue(cpu.Acc, inputValue, effect.ModifierOperation);
                                break;
                            case Variable.TARGET_HP:
                            case Variable.TARGET_ATTACK:
                            case Variable.TARGET_MOVEMENT:
                            case Variable.TARGET_MOVEMENT_DENOMINATOR:
                                { // Ok this means I need to change a stat of a series of targets obtained by FIND_ENTITIES
                                    Action<Stat, int> functionToApply = effect.ModifierOperation switch // Get correct operation
                                    {
                                        ModifierOperation.SET => STATS_SetStat,
                                        ModifierOperation.ADD => STATS_AddToStat,
                                        ModifierOperation.MULTIPLY => STATS_MultiplyStat,
                                        ModifierOperation.ABSOLUTE_SET => STATS_SetAbsoluteBaseStat,
                                        _ => throw new NotImplementedException("Modifier operation not implemented yet"),
                                    };
                                    foreach (int entityTarget in cpu.ReferenceEntities)
                                    {
                                        IngameEntity nextEntity = FetchEntity(entityTarget);
                                        Stat targetStat = effect.Output switch
                                        {
                                            Variable.TARGET_HP => ((LivingEntity)nextEntity).Hp,
                                            Variable.TARGET_ATTACK => ((Unit)nextEntity).Attack,
                                            Variable.TARGET_MOVEMENT => ((Unit)nextEntity).Movement,
                                            Variable.TARGET_MOVEMENT_DENOMINATOR => ((Unit)nextEntity).MovementDenominator,
                                            _ => throw new NotImplementedException("Modifier type not implemented yet")
                                        };
                                        // Got the stat, now modify it
                                        functionToApply(targetStat, inputValue);
                                    }
                                }
                                break;
                            case Variable.PLAYERS_GOLD:
                                foreach (int entityTarget in cpu.ReferenceEntities) // Modify gold for each entity found (!!)
                                {
                                    IngameEntity entity = FetchEntity(entityTarget); // Got the entity
                                    if(effect.TargetPlayer.HasFlag(EntityOwner.OWNER)) // Does owner of this entity get gold?
                                    {
                                        TRIGINTER_ModifyPlayersGold(entity.Owner, inputValue, effect.ModifierOperation);
                                    }
                                    if (effect.TargetPlayer.HasFlag(EntityOwner.OPPONENT)) // Does opp owner of this entity get gold?
                                    {
                                        TRIGINTER_ModifyPlayersGold(1 - entity.Owner, inputValue, effect.ModifierOperation);
                                    }
                                }
                                break;
                            default:
                                throw new NotImplementedException("Variable is read only!");
                        }
                        break;
                    default:
                        throw new NotImplementedException("Effect type not implemented yet");
                }
            }
            // End of effect chain
            if(firstEntryInChain) // It was I who created this so I need to remove context now
            {
                _chainContext.Remove(activatedEntityId);
            }
        }
        /// <summary>
        /// Gets the input value for the operations that need it
        /// </summary>
        /// <param name="cpu">Cpu context</param>
        /// <param name="input">Where to get the input from</param>
        /// <param name="multiInputOperation">If multiple inputs, how are they processed into a number</param>
        /// <returns>Value</returns>
        private int GetInput(CpuState cpu, Variable input, MultiInputProcessing multiInputOperation)
        {
            switch(input)
            {
                case Variable.TEMP_VARIABLE:
                    return cpu.TempValue;
                case Variable.ACC:
                    return cpu.Acc;
                default: 
                    break;
            }
            int result = 0; // This will require iteration...
            if (multiInputOperation == MultiInputProcessing.COUNT) // ...unless its a count operation
            {
                return cpu.ReferenceEntities.Count;
            }
            else
            {
                foreach (int entityTarget in cpu.ReferenceEntities)
                {
                    int auxInt = input switch
                    {
                        Variable.TARGET_HP => ((LivingEntity)FetchEntity(entityTarget)).Hp.Total,
                        Variable.TARGET_ATTACK => ((Unit)FetchEntity(entityTarget)).Attack.Total,
                        Variable.TARGET_MOVEMENT => ((Unit)FetchEntity(entityTarget)).Movement.Total,
                        Variable.TARGET_MOVEMENT_DENOMINATOR => ((Unit)FetchEntity(entityTarget)).MovementDenominator.Total,
                        Variable.PLAYERS_GOLD => DetailedState.PlayerStates[FetchEntity(entityTarget).Owner].CurrentGold,
                        _ => throw new Exception("This shouldn't have gotten here! Invalid input source!")
                    };
                    if(multiInputOperation == MultiInputProcessing.FIRST) // If I only needed the first value...
                    {
                        return auxInt;
                    }
                    // Otherwise I need to process data
                    result = multiInputOperation switch
                    {
                        MultiInputProcessing.SUM => result + auxInt,
                        MultiInputProcessing.AVERAGE => result + auxInt, // Average will be later
                        MultiInputProcessing.MAX => Math.Max(result, auxInt),
                        MultiInputProcessing.MIN => Math.Min(result, auxInt),
                        _ => throw new Exception("Invalid MultiInputOperation")
                    };
                }
                if(multiInputOperation == MultiInputProcessing.AVERAGE) // Check if I need to apply average
                {
                    result /= cpu.ReferenceEntities.Count;
                }
                return result;
            }
        }
        /// <summary>
        /// Creates an array of targets, as many as the amount of references we searched. This way we may chain repeated summon or cast effects that require location, one per each reference
        /// </summary>
        /// <param name="target">The sort of target we're looking for</param>
        /// <param name="cpuContext">The cpu context, this'll have the references</param>
        /// <returns></returns>
        static TargetLocation[] GetTargetLocationsFromReference(TargetLocation target, CpuState cpuContext)
        {
            int numberOfReferences = cpuContext.ReferenceEntities.Count;
            TargetLocation[] res = new TargetLocation[numberOfReferences];
            for(int i = 0; i < numberOfReferences; i++) // One target per reference!
            {
                res[i] = target switch
                {
                    TargetLocation.PLAY_TARGET => ((PlayContext)cpuContext.CurrentSpecificContext).LaneTargets, // Where card is played (can only be used when card is played)...
                    _ => target,
                };
            }
            return res;
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
        /// <param name="targetPlayer">Target owner relative to card </param>
        /// <param name="targetType">Entity types to search</param>
        /// <param name="searchCriterion">Criterion of search</param>
        /// <param name="targetLocation">Where to search for targets</param>
        /// <param name="n">The value n whose meaning depends on search criterion</param>
        /// <param name="ownerPlayerPov">POV of who the search is relative to</param>
        /// <returns></returns>
        List<int> GetTargets(EntityOwner targetPlayer, EntityType targetType, SearchCriterion searchCriterion, TargetLocation targetLocation, int n, int ownerPlayerPov)
        {
            // If nothing to search for, just return an empty list
            if(targetPlayer == EntityOwner.NONE || targetType == EntityType.NONE || (searchCriterion == SearchCriterion.QUANTITY && n == 0))
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
            // Prepare settings/masks for this target search
            switch (targetLocation)
            {
                case TargetLocation.BOARD: // Return board
                    targetBoardArea = DetailedState.BoardState;
                    break;
                case TargetLocation.PLAINS: // For lane-based, get the lane
                case TargetLocation.FOREST:
                case TargetLocation.MOUNTAIN:
                    targetBoardArea = DetailedState.BoardState.GetLane(targetLocation);
                    laneIsTarget = true;
                    break;
                default:
                    throw new NotImplementedException("Search not yet implemented for other targets!"); // This may be a later TODO once new needs appear
            }
            if (laneIsTarget && (searchCriterion != SearchCriterion.ALL)) // If target is everything, no need to check tile by tile
            {
                tileSectioning = true;
            }
            if (n < 0) // Negative indexing implies reverse indexing
            {
                n *= -1;
                if(searchCriterion == SearchCriterion.ORDINAL) // In ordinals, -1 is 0 from reverse. In quant, -1 is 1 in reverse
                {
                    n -= 1; // Need to index starting from 0!
                }
                reverseSearch = true;
            }
            referencePlayer = reverseSearch ? 1 - ownerPlayerPov : ownerPlayerPov;
            requiredTargets = searchCriterion switch
            {
                SearchCriterion.ORDINAL => 1, // Only one target in position N
                SearchCriterion.QUANTITY => n, // First/last N
                SearchCriterion.ALL => int.MaxValue, // Get everything possible
                _ => throw new NotImplementedException("Invalid search criterion"),
            };
            if(targetPlayer == EntityOwner.OWNER)
            {
                playerFilter = ownerPlayerPov;
            }
            else if (targetPlayer == EntityOwner.OPPONENT)
            {
                playerFilter = 1 - ownerPlayerPov;
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
                        if (targetType.HasFlag(EntityType.PLAYER)) // Player targeting enabled, check if player is valid
                        {
                            // Need to add if player (w.r.t to card ownership) are valid targets 
                            if (reverseSearch && targetPlayer.HasFlag(EntityOwner.OPPONENT)) // If searching backwards and opponent enabled, then add opp
                            {
                                nextCandidateEntity = 1 - ownerPlayerPov;
                            }
                            else if (!reverseSearch && targetPlayer.HasFlag(EntityOwner.OWNER)) // If searching normally and owner enabled, then add it
                            {
                                nextCandidateEntity = ownerPlayerPov;
                            }
                        }
                        currentSearchState = TargetingStateMachine.GET_ENTITIES_IN_REGION; // Once finished, go to next state (get elements to search for entities)
                        break;
                    case TargetingStateMachine.GET_ENTITIES_IN_REGION: // Gets the list of entities in the desired board element, also checks if need to subdivide lane
                        if(tileSectioning) // Lane needs to be divided in tiles
                        {
                            if(tileCounter < ((Lane)targetBoardArea).Len) // Check if I still have ongoing lane available
                            {
                                entities = ((Lane)targetBoardArea).GetTileRelative(tileCounter, referencePlayer).GetPlacedEntities(targetType, playerFilter); // Search for requested target for requested players
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
                            entities = targetBoardArea.GetPlacedEntities(targetType, playerFilter);
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
                        if (targetType.HasFlag(EntityType.PLAYER))
                        {
                            if (!reverseSearch && targetPlayer.HasFlag(EntityOwner.OPPONENT))
                            {
                                nextCandidateEntity = 1 - ownerPlayerPov;
                            }
                            else if (reverseSearch && targetPlayer.HasFlag(EntityOwner.OWNER))
                            {
                                nextCandidateEntity = ownerPlayerPov;
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
                    if(searchCriterion == SearchCriterion.ORDINAL) // Need to ensure the unit # matches!
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
        /// <summary>
        /// Calculates a new value given a value, modifier and operation. Sort of a mini calculator
        /// </summary>
        /// <param name="value"></param>
        /// <param name="modifier"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        static int GetModifiedValue(int value, int modifier, ModifierOperation operation)
        {
            return operation switch // Returns the number depending on the operation
            {
                ModifierOperation.SET => modifier,
                ModifierOperation.ABSOLUTE_SET => modifier,
                ModifierOperation.ADD => value + modifier,
                ModifierOperation.MULTIPLY => value * modifier,
                _ => throw new NotImplementedException("Modifier operation not supported")
            };

        }
        /// <summary>
        /// Will summon a unit to a specific player, bypassing playables, can choose multiple lanes at once
        /// </summary>
        /// <param name="playerOwner">Would be owner</param>
        /// <param name="cardNumber">Which card (hope its a unit!)</param>
        /// <param name="lanesToSummon">Lane targets</param>
        void SummonUnitToPlayer(int playerOwner, int cardNumber, TargetLocation lanesToSummon)
        {
            Unit auxCardData = (Unit)CardDb.GetCard(cardNumber);
            // Unit summoning is made without considering cost and the sort, so just go to Playables, play card (for now, may need more complex checking later)
            for (int j = 0; j < GameConstants.BOARD_LANES_NUMBER; j++)
            {
                TargetLocation nextLane = (TargetLocation)(1 << j); // Get lanes in order, can be randomized if needed
                if (lanesToSummon.HasFlag(nextLane)) // If this lane is a valid target for this unt
                {
                    UNIT_PlayUnit(playerOwner, auxCardData, nextLane); // Plays the unit
                }
            }
        }
        /// <summary>
        /// Modifies a player's gold according to desired op+value
        /// </summary>
        /// <param name="playerId">Which player</param>
        /// <param name="value">What number</param>
        /// <param name="operation">What to do with the number</param>
        void TRIGINTER_ModifyPlayersGold(int playerId, int value, ModifierOperation operation)
        {
            ENGINE_SetPlayerGold(playerId, GetModifiedValue(DetailedState.PlayerStates[playerId].CurrentGold, value, operation));
        }
    }
}
