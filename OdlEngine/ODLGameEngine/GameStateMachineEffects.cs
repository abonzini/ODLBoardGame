﻿namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with triginter effect execution
    {
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
        void EFFECTS_ProcessEffects(List<Effect> effects, EffectContext specificContext)
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
            // Now that the CPU has been configured, can execute effect chain
            for (int effectIndex = 0; effectIndex < effects.Count; effectIndex++) // Execute series of events for the card in question
            {
                Effect effect = effects[effectIndex]; // Next effect
                bool breakLoop = false; // Wether loop goes on or is broken by an assert operation
                // Define values of registers as may be needed
                cpu.CurrentSpecificContext = specificContext; // Refresh context just in case cpu context was highjacked by a trigger in the meantime or something
                cpu.TempValue = effect.TempVariable;
                int inputValue = GetInput(cpu, effect.Input, effect.MultiInputProcessing);
                // Now to process the effect
                switch (effect.EffectType)
                {
                    case EffectType.STORE_DEBUG_IN_EVENT_PILE:
                        ENGINE_AddDebugEvent(cpu); // Stores triginter CPU context for checking of intermediate values
                        break;
                    case EffectType.ACTIVATE_TEST_TRIGGER_IN_LOCATION:
                        // Triggers this in the place chosen
                        EFFECT_ActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, effect.EffectLocation, new EffectContext());
                        break;
                    case EffectType.SELECT_ENTITY:
                        { // In this case there's a simple, single BoardEntity target related to the ctx in question
                            List<int> res = new List<int>();
                            IngameEntity tgt = SelectEntity(effect.SearchCriterion, cpu);
                            if (effect.TargetType.HasFlag(tgt.EntityType)) // The unit is of the valid type
                            {
                                // Determine who owns this unit then
                                EntityOwner owner = (tgt.Owner == specificContext.ActivatedEntity.Owner) ? EntityOwner.OWNER : EntityOwner.OPPONENT;
                                if (effect.TargetPlayer.HasFlag(owner)) // If it's a valid owner, then target is valid
                                {
                                    res.Add(tgt.UniqueId); // This is now the target
                                }
                            }
                            cpu.ReferenceEntities = res;
                        }
                        break;
                    case EffectType.FIND_ENTITIES:
                        // Searches for entities, reference being the selected reference (since there can be multiple references, a single one (the first) is used
                        List<int> newList = new List<int>(); // Now I'll prepare the new list result
                        foreach (BoardElement nextSearchLocation in cpu.ReferenceLocations) // Look spot by spot
                        {
                            newList.AddRange(GetTargets(effect.TargetPlayer, effect.TargetType, effect.SearchCriterion, nextSearchLocation, inputValue, cpu.CurrentSpecificContext.ActivatedEntity.Owner));
                        }
                        cpu.ReferenceEntities = newList;
                        break;
                    case EffectType.ADD_LOCATION_REFERENCE:
                        // Adds one or more locations to list of reference locations
                        cpu.ReferenceLocations.AddRange(GetTargetLocations(effect.EffectLocation, cpu));
                        break;
                    case EffectType.SUMMON_UNIT:
                        int referencePlayer = cpu.CurrentSpecificContext.ActivatedEntity.Owner;
                        foreach (BoardElement nextSummonLocation in cpu.ReferenceLocations)
                        {

                            if (effect.TargetPlayer.HasFlag(EntityOwner.OWNER)) // Will its owner or its opponent get the unit?
                            {
                                SummonUnitToPlayer(referencePlayer, inputValue, nextSummonLocation);
                            }
                            if (effect.TargetPlayer.HasFlag(EntityOwner.OPPONENT))
                            {
                                SummonUnitToPlayer(1 - referencePlayer, inputValue, nextSummonLocation);
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
                                        _ => throw new NotImplementedException("Modifier operation not supported for stats"),
                                    };
                                    for (int i = 0; i < cpu.ReferenceEntities.Count; i++)
                                    {
                                        if (effect.MultiInputProcessing == MultiInputProcessing.EACH) // Actually multiple input values...
                                        {
                                            inputValue = GetInput(cpu, effect.Input, effect.MultiInputProcessing, i);
                                        }
                                        IngameEntity nextEntity = FetchEntity(cpu.ReferenceEntities[i]);
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
                                for (int i = 0; i < cpu.ReferenceEntities.Count; i++) // Modify gold for each entity found (!!)
                                {
                                    if (effect.MultiInputProcessing == MultiInputProcessing.EACH) // Actually multiple input values...
                                    {
                                        inputValue = GetInput(cpu, effect.Input, effect.MultiInputProcessing, i);
                                    }
                                    IngameEntity entity = FetchEntity(cpu.ReferenceEntities[i]); // Got the entity
                                    if (effect.TargetPlayer.HasFlag(EntityOwner.OWNER)) // Does owner of this entity get gold?
                                    {
                                        EFFECTS_ModifyPlayersGold(entity.Owner, inputValue, effect.ModifierOperation);
                                    }
                                    if (effect.TargetPlayer.HasFlag(EntityOwner.OPPONENT)) // Does opp owner of this entity get gold?
                                    {
                                        EFFECTS_ModifyPlayersGold(1 - entity.Owner, inputValue, effect.ModifierOperation);
                                    }
                                }
                                break;
                            case Variable.MARCH_CURRENT_MOVEMENT:
                                {
                                    MarchingContext marchCtx = (MarchingContext)cpu.CurrentSpecificContext;
                                    marchCtx.CurrentMovement = GetModifiedValue(marchCtx.CurrentMovement, inputValue, effect.ModifierOperation);
                                }
                                break;
                            case Variable.DAMAGE_AMOUNT:
                                {
                                    DamageContext dmgCtx = (DamageContext)cpu.CurrentSpecificContext;
                                    dmgCtx.DamageAmount = GetModifiedValue(dmgCtx.DamageAmount, inputValue, effect.ModifierOperation);
                                }
                                break;
                            default:
                                throw new NotImplementedException("Variable is read only!");
                        }
                        break;
                    case EffectType.ASSERT:
                        breakLoop = effect.ModifierOperation switch // Breaks the loop on assert, but allows to use the NOT modifier to assert posi
                        {
                            ModifierOperation.NOT => (inputValue != 0),
                            _ => (inputValue == 0)
                        };
                        break;
                    case EffectType.ASSERT_ROLE:
                        IngameEntity comparedEntity = SelectEntity(effect.SearchCriterion, cpu);
                        breakLoop = effect.ModifierOperation switch // Breaks the loop on assert, but allows to use the NOT modifier to assert posi
                        {
                            ModifierOperation.NOT => (specificContext.ActivatedEntity.UniqueId == comparedEntity.UniqueId),
                            _ => (specificContext.ActivatedEntity.UniqueId != comparedEntity.UniqueId)
                        };
                        break;
                    case EffectType.KILL_ENTITIES:
                        foreach (int entityTarget in cpu.ReferenceEntities)
                        {
                            LivingEntity nextEntity = (LivingEntity)FetchEntity(entityTarget);
                            LIVINGENTITY_Kill(nextEntity);
                        }
                        break;
                    case EffectType.EFFECT_DAMAGE:
                        for (int i = 0; i < cpu.ReferenceEntities.Count; i++)
                        {
                            if (effect.MultiInputProcessing == MultiInputProcessing.EACH) // Actually multiple input values...
                            {
                                inputValue = GetInput(cpu, effect.Input, effect.MultiInputProcessing, i);
                            }
                            LivingEntity nextEntity = (LivingEntity)FetchEntity(cpu.ReferenceEntities[i]);
                            DamageContext effectDamageContext = new DamageContext
                            {
                                Actor = cpu.CurrentSpecificContext.ActivatedEntity,
                                Affected = nextEntity,
                                DamageAmount = inputValue,
                                ActivatedEntity = cpu.CurrentSpecificContext.ActivatedEntity // The current activated entity will do the damage
                            };
                            // Damage
                            effectDamageContext = LIVINGENTITY_DamageStep(effectDamageContext);
                            // TODO: Trigger death interactions
                        }
                        break;
                    case EffectType.CARD_DRAW:
                        for (int i = 0; i < cpu.ReferenceEntities.Count; i++) // Will draw a number of cards for each reference (!!)
                        {
                            if (effect.MultiInputProcessing == MultiInputProcessing.EACH) // Actually multiple input values...
                            {
                                inputValue = GetInput(cpu, effect.Input, effect.MultiInputProcessing, i);
                            }
                            IngameEntity entity = FetchEntity(cpu.ReferenceEntities[i]); // Got the entity
                            DrawContext nextDrawContext = new DrawContext()
                            {
                                DrawAmount = inputValue
                            };
                            if (effect.TargetPlayer.HasFlag(EntityOwner.OWNER)) // Does owner of this entity draw?
                            {
                                nextDrawContext.Actor = DetailedState.PlayerStates[entity.Owner];
                                STATE_DeckDrawMultiple(nextDrawContext);
                            }
                            if (effect.TargetPlayer.HasFlag(EntityOwner.OPPONENT)) // Does opp owner of this entity draw?
                            {
                                nextDrawContext.Actor = DetailedState.PlayerStates[1 - entity.Owner];
                                STATE_DeckDrawMultiple(nextDrawContext);
                            }
                        }
                        break;
                    case EffectType.MARCH_ENTITIES:
                        foreach (int unitId in cpu.ReferenceEntities)
                        {
                            if (DetailedState.EntityData.TryGetValue(unitId, out LivingEntity unit)) // Check if unit is still alive, if not, no need to march
                            {
                                UNIT_UnitMarch((Unit)unit); // Then the unit marches
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException("Effect type not implemented yet");
                }
                if (breakLoop)
                {
                    break;
                }
            }
            // End of effect chain
            if (firstEntryInChain) // It was I who created this so I need to remove context now
            {
                _chainContext.Remove(activatedEntityId);
            }
        }
        /// <summary>
        /// Gets the input value for the operations that need it
        /// </summary>
        /// <param name="cpu">Cpu context to find the values</param>
        /// <param name="input">Input variable</param>
        /// <param name="multiInputOperation">What to do if it is a value of a (potentially multiple) reference entity list</param>
        /// <param name="ordinalN">If it is in ordinal mode, gets the next N value</param>
        /// <returns></returns>
        private int GetInput(CpuState cpu, Variable input, MultiInputProcessing multiInputOperation, int ordinalN = 0)
        {
            switch (input)
            {
                case Variable.TEMP_VARIABLE:
                    return cpu.TempValue;
                case Variable.ACC:
                    return cpu.Acc;
                case Variable.TARGET_COUNT:
                    return cpu.ReferenceEntities.Count;
                case Variable.MARCH_START_FLAG:
                    return ((MarchingContext)cpu.CurrentSpecificContext).FirstTileMarch ? 1 : 0;
                case Variable.MARCH_CURRENT_MOVEMENT:
                    return ((MarchingContext)cpu.CurrentSpecificContext).CurrentMovement;
                case Variable.DAMAGE_AMOUNT:
                    return ((DamageContext)cpu.CurrentSpecificContext).DamageAmount;
                default:
                    break;
            }
            int result = 0; // This will require iteration...
            for (int i = 0; i < cpu.ReferenceEntities.Count; i++)
            {
                int entityTarget;
                if (multiInputOperation != MultiInputProcessing.EACH) // In this cases I need to iterate the whole array
                {
                    entityTarget = cpu.ReferenceEntities[i];
                }
                else // Otherwise get the right entity directly
                {
                    entityTarget = cpu.ReferenceEntities[ordinalN];
                }
                int auxInt = input switch
                {
                    Variable.TARGET_HP => ((LivingEntity)FetchEntity(entityTarget)).Hp.Total,
                    Variable.TARGET_ATTACK => ((Unit)FetchEntity(entityTarget)).Attack.Total,
                    Variable.TARGET_MOVEMENT => ((Unit)FetchEntity(entityTarget)).Movement.Total,
                    Variable.TARGET_MOVEMENT_DENOMINATOR => ((Unit)FetchEntity(entityTarget)).MovementDenominator.Total,
                    Variable.PLAYERS_GOLD => DetailedState.PlayerStates[FetchEntity(entityTarget).Owner].CurrentGold,
                    _ => throw new Exception("This shouldn't have gotten here! Invalid input source!")
                };
                if (multiInputOperation == MultiInputProcessing.EACH || multiInputOperation == MultiInputProcessing.FIRST) // If I only needed the Nth value then I already have it
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
            if (multiInputOperation == MultiInputProcessing.AVERAGE) // Check if I need to apply average
            {
                result /= cpu.ReferenceEntities.Count;
            }
            return result;
        }
        /// <summary>
        /// Return one or more reference locations for use in effects
        /// </summary>
        /// <param name="searchLocation">Place to search, can be absolute lanes, board, or even relative things</param>
        /// <param name="cpuContext">Context where I find the relevant info</param>
        /// <returns>One board element per entity</returns>
        List<BoardElement> GetTargetLocations(EffectLocation searchLocation, CpuState cpuContext)
        {
            List<BoardElement> res = new List<BoardElement>();
            switch (searchLocation)
            {
                case EffectLocation.BOARD:
                    res.Add(DetailedState.BoardState);
                    break;
                case EffectLocation.PLAINS:
                    res.Add(DetailedState.BoardState.PlainsLane);
                    break;
                case EffectLocation.FOREST:
                    res.Add(DetailedState.BoardState.ForestLane);
                    break;
                case EffectLocation.MOUNTAIN:
                    res.Add(DetailedState.BoardState.MountainLane);
                    break;
                case EffectLocation.PLAY_TARGET:
                    res.Add(GetPlayTarget((PlayContext)cpuContext.CurrentSpecificContext));
                    break;
                case EffectLocation.CURRENT_TILE:
                    {
                        foreach (int reference in cpuContext.ReferenceEntities)
                        {
                            res.Add(DetailedState.BoardState.Tiles[((PlacedEntity)DetailedState.EntityData[reference]).TileCoordinate]);
                        }
                    }
                    break;
                default:
                    throw new Exception("Reference search location not implemented");
            }
            return res;
        }
        /// <summary>
        /// If a card has been played and the "play target" is requested, get the board element of this play target
        /// </summary>
        /// <param name="playCtx">Play context</param>
        /// <returns>The board element</returns>
        BoardElement GetPlayTarget(PlayContext playCtx)
        {
            return playCtx.TargetingType switch
            {
                CardTargetingType.BOARD => DetailedState.BoardState,
                CardTargetingType.LANE => DetailedState.BoardState.GetLane(playCtx.PlayedTarget),
                CardTargetingType.TILE => DetailedState.BoardState.Tiles[playCtx.PlayedTarget],
                CardTargetingType.UNIT or CardTargetingType.BUILDING or CardTargetingType.UNIT_AND_BUILDING => DetailedState.BoardState.Tiles[((PlacedEntity)DetailedState.EntityData[playCtx.PlayedTarget]).TileCoordinate],
                _ => throw new Exception("No other supported play targets for now")
            };
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
        /// Selects a single entity given a criterion and a possibly necessary context
        /// </summary>
        /// <param name="searchCriterion">Defines what entity to look for</param>
        /// <param name="cpuContext">Context used to find the entity</param>
        /// <returns>The entity found</returns>
        IngameEntity SelectEntity(SearchCriterion searchCriterion, CpuState cpuContext)
        {
            return searchCriterion switch
            {
                SearchCriterion.EFFECT_OWNING_ENTITY => cpuContext.CurrentSpecificContext.ActivatedEntity,
                SearchCriterion.ACTOR_ENTITY => cpuContext.CurrentSpecificContext.Actor,
                SearchCriterion.AFFECTED_ENTITY => ((AffectingEffectContext)cpuContext.CurrentSpecificContext).Affected,
                SearchCriterion.PLAY_TARGET_ENTITY => DetailedState.EntityData[((PlayContext)cpuContext.CurrentSpecificContext).PlayedTarget],
                _ => throw new NotImplementedException("Invalid target for entity selection")
            };
        }
        /// <summary>
        /// Function that gets serch parameters as well as reference observer, and returns a series of valid entity targets.
        /// </summary>
        /// <param name="targetPlayer">Target owner relative to card </param>
        /// <param name="targetType">Entity types to search</param>
        /// <param name="searchCriterion">Criterion of search</param>
        /// <param name="searchLocation">Where to search for targets</param>
        /// <param name="n">The value n whose meaning depends on search criterion</param>
        /// <param name="ownerPlayerPov">POV of who the search is relative to</param>
        /// <returns></returns>
        static List<int> GetTargets(EntityOwner targetPlayer, EntityType targetType, SearchCriterion searchCriterion, BoardElement searchLocation, int n, int ownerPlayerPov)
        {
            // If nothing to search for, just return an empty list
            if (targetPlayer == EntityOwner.NONE || targetType == EntityType.NONE || (searchCriterion == SearchCriterion.QUANTITY && n == 0))
            {
                return new List<int>();
            }
            List<int> res = new List<int>();
            // Search variables
            bool reverseSearch = false; // Order of search
            int requiredTargets; // How many targets I'll extract maximum
            int playerFilter = -1; // Filter of which player to search for (defautl is -1 both players)
            // Prepare settings/masks for this target search
            if (n < 0) // Negative indexing implies reverse indexing
            {
                n *= -1;
                if (searchCriterion == SearchCriterion.ORDINAL) // In ordinals, -1 is 0 from reverse. In quant, -1 is 1 in reverse
                {
                    n -= 1; // Need to index starting from 0!
                }
                reverseSearch = true;
            }
            requiredTargets = searchCriterion switch
            {
                SearchCriterion.ORDINAL => 1, // Only one target in position N
                SearchCriterion.QUANTITY => n, // First/last N
                SearchCriterion.ALL => int.MaxValue, // Get everything possible
                _ => throw new NotImplementedException("Invalid search criterion"),
            };
            if (targetPlayer == EntityOwner.OWNER)
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
                        entities = searchLocation.GetPlacedEntities(targetType, playerFilter);
                        currentSearchState = TargetingStateMachine.TARGETS_IN_REGION;
                        localOrdinal = 0;
                        break;
                    case TargetingStateMachine.TARGETS_IN_REGION: // This searches the region to add as many entities as it can find
                        if (localOrdinal < entities.Count) // Can still explore this one
                        {
                            nextCandidateEntity = entities.ElementAt(localOrdinal); // Get next element
                            localOrdinal++;
                        }
                        else // Means I finished region
                        {
                            currentSearchState = TargetingStateMachine.LAST_PLAYER;
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
                if (nextCandidateEntity != -1) // Found next potential candidate entity, need to verify if applies
                {
                    bool addEntity = true;
                    if (searchCriterion == SearchCriterion.ORDINAL) // Need to ensure the unit # matches!
                    {
                        if (totalOrdinal != n) // Not the entity I was looking for, unfortunately
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
                ModifierOperation.NOT => (modifier == 0) ? 1 : 0,
                _ => throw new NotImplementedException("Modifier operation not supported")
            };
        }
        /// <summary>
        /// Will summon a unit to a specific player, bypassing playables, can choose multiple lanes at once
        /// </summary>
        /// <param name="playerOwner">Would be owner</param>
        /// <param name="cardNumber">Which card (hope its a unit!)</param>
        /// <param name="placeToSummon">BoardElement where to attempt the summon</param>
        void SummonUnitToPlayer(int playerOwner, int cardNumber, BoardElement placeToSummon)
        {
            Unit auxCardData = (Unit)CardDb.GetCard(cardNumber);
            // Ctx I'll fill to place the unit exactly where I want
            PlayContext playCtx = new PlayContext()
            {
                Actor = auxCardData
            };
            if (placeToSummon.ElementType == BoardElementType.TILE) // In this case, I know exactly where the unit will be placed
            {
                playCtx.PlayedTarget = ((Tile)placeToSummon).Coord;
            }
            else if (placeToSummon.ElementType == BoardElementType.LANE) // In this case I assume its just beginning of tile (for now?!)
            {
                playCtx.PlayedTarget = ((Lane)placeToSummon).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerOwner);
            }
            else
            {
                throw new Exception("Invalid location where a unit can be placed");
            }
            // Got the summoning location, onwards to play it
            UNIT_PlayUnit(playerOwner, playCtx);
        }
        /// <summary>
        /// Modifies a player's gold according to desired op+value
        /// </summary>
        /// <param name="playerId">Which player</param>
        /// <param name="value">What number</param>
        /// <param name="operation">What to do with the number</param>
        void EFFECTS_ModifyPlayersGold(int playerId, int value, ModifierOperation operation)
        {
            ENGINE_SetPlayerGold(playerId, GetModifiedValue(DetailedState.PlayerStates[playerId].CurrentGold, value, operation));
        }
        void EFFECT_ActivateTrigger(TriggerType trigger, EffectLocation location, EffectContext specificContext)
        {
            BoardElement place = location switch
            {
                EffectLocation.BOARD => DetailedState.BoardState,
                EffectLocation.PLAINS => DetailedState.BoardState.PlainsLane,
                EffectLocation.FOREST => DetailedState.BoardState.ForestLane,
                EffectLocation.MOUNTAIN => DetailedState.BoardState.MountainLane,
                _ => throw new Exception("Not a valid absolute location for triggers")
            };
            TRIGINTER_ProcessTrigger(trigger, place, specificContext);
        }
    }
}
