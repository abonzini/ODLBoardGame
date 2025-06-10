namespace ODLGameEngine
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
                            IngameEntity tgt = effect.SearchCriterion switch
                            {
                                SearchCriterion.EFFECT_OWNING_ENTITY => specificContext.ActivatedEntity,
                                SearchCriterion.ACTOR_ENTITY => specificContext.Actor,
                                SearchCriterion.AFFECTED_ENTITY => ((AffectingEffectContext)specificContext).Affected,
                                _ => throw new NotImplementedException("Invalid target for entity selection")
                            };
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
                        {
                            BoardElement[] searchLocations = GetTargetLocationsFromReference(effect.EffectLocation, cpu); // First, I'll find all the locations for searching
                            List<int> newList = new List<int>(); // Now I'll prepare the new list result
                            for (int i = 0; i < cpu.ReferenceEntities.Count; i++) // Attach the whole sets of units found (this may duplicate findings! be careful!)
                            {
                                newList.AddRange(GetTargets(effect.TargetPlayer, effect.TargetType, effect.SearchCriterion, searchLocations[i], inputValue, FetchEntity(cpu.ReferenceEntities[i]).Owner));
                            }
                            cpu.ReferenceEntities = newList;
                        }
                        break;
                    case EffectType.SUMMON_UNIT:
                        {
                            BoardElement[] targets = GetTargetLocationsFromReference(effect.EffectLocation, cpu); // Get the actual place where this is played, even if relative
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
                                        _ => throw new NotImplementedException("Modifier operation not supported for stats"),
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
                    case EffectType.KILL_ENTITIES:
                        foreach (int entityTarget in cpu.ReferenceEntities)
                        {
                            LivingEntity nextEntity = (LivingEntity)FetchEntity(entityTarget);
                            LIVINGENTITY_Kill(nextEntity);
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
        /// <param name="cpu">Cpu context</param>
        /// <param name="input">Where to get the input from</param>
        /// <param name="multiInputOperation">If multiple inputs, how are they processed into a number</param>
        /// <returns>Value</returns>
        private int GetInput(CpuState cpu, Variable input, MultiInputProcessing multiInputOperation)
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
                default:
                    break;
            }
            int result = 0; // This will require iteration...
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
                if (multiInputOperation == MultiInputProcessing.FIRST) // If I only needed the first value...
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
        /// Will fetch, for each entity in the CPU entity list, the BoardElement location specified form a search location
        /// Useful for targeted effects and similar
        /// </summary>
        /// <param name="searchLocation">Place to search, can be absolute lanes, board, or even relative things</param>
        /// <param name="cpuContext">Context where I find the relevant info</param>
        /// <returns>One board element per entity</returns>
        BoardElement[] GetTargetLocationsFromReference(EffectLocation searchLocation, CpuState cpuContext)
        {
            int numberOfReferences = cpuContext.ReferenceEntities.Count;
            BoardElement[] res = new BoardElement[numberOfReferences];
            for (int i = 0; i < numberOfReferences; i++) // One target per reference!
            {
                res[i] = searchLocation switch
                {
                    EffectLocation.BOARD => DetailedState.BoardState,
                    EffectLocation.PLAINS => DetailedState.BoardState.PlainsLane,
                    EffectLocation.FOREST => DetailedState.BoardState.ForestLane,
                    EffectLocation.MOUNTAIN => DetailedState.BoardState.MountainLane,
                    EffectLocation.PLAY_TARGET => (((PlayContext)cpuContext.CurrentSpecificContext).PlayTarget == PlayTargetLocation.BOARD) ? DetailedState.BoardState : DetailedState.BoardState.GetLane(((PlayContext)cpuContext.CurrentSpecificContext).PlayTarget), // Expects play context
                    EffectLocation.CURRENT_TILE => DetailedState.BoardState.Tiles[((PlacedEntity)DetailedState.EntityData[cpuContext.ReferenceEntities[i]]).TileCoordinate], // Expects the entities here to have current tile coordinate
                    _ => throw new Exception("Reference search location not implemented")
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
            bool tileSectioning = false; // When searching along a lane, check tile-by-tile or the whole body as is
            int referencePlayer; // Order reference depends on indexing
            int playerFilter = -1; // Filter of which player to search for (defautl is -1 both players)
            // Prepare settings/masks for this target search
            if (searchLocation.ElementType == BoardElementType.LANE) // Since its a lane, it'll be lane search
            {
                if (searchCriterion != SearchCriterion.ALL) // If target is everything, no need to check tile by tile
                {
                    tileSectioning = true;
                }
            }
            if (n < 0) // Negative indexing implies reverse indexing
            {
                n *= -1;
                if (searchCriterion == SearchCriterion.ORDINAL) // In ordinals, -1 is 0 from reverse. In quant, -1 is 1 in reverse
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
                        if (tileSectioning) // Lane needs to be divided in tiles
                        {
                            if (tileCounter < ((Lane)searchLocation).Len) // Check if I still have ongoing lane available
                            {
                                entities = ((Lane)searchLocation).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, tileCounter, referencePlayer).GetPlacedEntities(targetType, playerFilter); // Search for requested target for requested players
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
                            entities = searchLocation.GetPlacedEntities(targetType, playerFilter);
                            currentSearchState = TargetingStateMachine.TARGETS_IN_REGION;
                        }
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
                            if (tileSectioning) // May want to look for next tile (if available)
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
            UnitPlayContext unitPlayCtx = new UnitPlayContext
            {
                Actor = auxCardData
            };
            if (placeToSummon.ElementType == BoardElementType.TILE) // In this case, I know exactly where the unit will be placed
            {
                unitPlayCtx.AbsoluteInitialTile = ((Tile)placeToSummon).Coord;
            }
            else if (placeToSummon.ElementType == BoardElementType.LANE) // In this case I assume its just beginning of tile (for now?!)
            {
                unitPlayCtx.AbsoluteInitialTile = ((Lane)placeToSummon).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerOwner);
            }
            else
            {
                throw new Exception("Invalid location where a unit can be placed");
            }
            // Got the summoning location, onwards to play it
            UNIT_PlayUnit(playerOwner, unitPlayCtx);
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
