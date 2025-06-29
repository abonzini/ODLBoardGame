namespace ODLGameEngine
{
    public enum PlayOutcome
    {
        OK,
        CANT_AFFORD,
        NO_TARGET_AVAILABLE,
        INVALID_CARD,
        INVALID_GAME_STATE,
        POWER_ALREADY_USED
    }
    public enum PlayType
    {
        PLAY_FROM_HAND,
        ACTIVE_POWER
    }
    public partial class GameStateMachine
    {
        // --------------------------------------------------------------------------------------
        // ------------------------------  PLAY INFO REQUESTS -----------------------------------
        // --------------------------------------------------------------------------------------
        static readonly HashSet<int> allPlayableLanes = new HashSet<int>(Enumerable.Range(0, GameConstants.BOARD_NUMBER_OF_LANES));
        static readonly HashSet<int> allPlayableTiles = new HashSet<int>(Enumerable.Range(0, GameConstants.BOARD_NUMBER_OF_TILES));
        // Public (access points)
        /// <summary>
        /// Checks wether a card is playable and the valid targets if it is. Also used to verify before playing
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <param name="playType">Context of which play type</param>
        /// <param name="onlyRelevantTarget">If a target is already chosen, just verifies this one</param>
        /// <returns>Playcontext which tells you whether it's played ok</returns>
        public PlayContext GetPlayabilityOptions(int card, PlayType playType, int onlyRelevantTarget = -1)
        {
            int playerChecking = (int)DetailedState.CurrentPlayer; // For now only the current player is checking
            PlayContext resultingPlayContext = new PlayContext
            {
                PlayType = playType
            };
            // First stage, non card-related
            // Check whether we're in the right place first (can only do this on play state)
            if (DetailedState.CurrentState != States.ACTION_PHASE)
            {
                resultingPlayContext.PlayOutcome = PlayOutcome.INVALID_GAME_STATE;
                return resultingPlayContext;
            }
            // An extra check first, whether card actually exists in hand (if applicable)
            if (playType == PlayType.PLAY_FROM_HAND)
            {
                AssortedCardCollection hand = DetailedState.PlayerStates[playerChecking].Hand;
                if (!hand.HasCardInCollection(card)) // Card not in hand!
                {
                    resultingPlayContext.PlayOutcome = PlayOutcome.INVALID_CARD;
                    return resultingPlayContext;
                }
            }
            // In this case we check instead if the active power is allowed
            else if (playType == PlayType.ACTIVE_POWER)
            {
                if (!DetailedState.PlayerStates[playerChecking].PowerAvailable) // Power not available!
                {
                    resultingPlayContext.PlayOutcome = PlayOutcome.POWER_ALREADY_USED;
                    return resultingPlayContext;
                }
            }
            else
            {
                throw new Exception("Invalid play type");
            }
            // Second stage, got the actual card
            EntityBase cardData = CardDb.GetCard(card);
            resultingPlayContext.PlayCost = cardData.Cost;
            // First check if player can afford
            if (!PLAYABLE_PlayerCanAfford(resultingPlayContext.PlayCost))
            {
                // Can't afford!
                resultingPlayContext.PlayOutcome = PlayOutcome.CANT_AFFORD;
                return resultingPlayContext;
            }
            // Does it have any valid targets?
            if (cardData.TargetOptions == null)
            {
                resultingPlayContext.PlayOutcome = PlayOutcome.NO_TARGET_AVAILABLE;
                return resultingPlayContext;
            }
            // Otherwise I can def afford, and in principle playable somewhere
            CardTargetingType targetType = cardData.EntityType switch
            {
                EntityType.UNIT => CardTargetingType.TILE,
                EntityType.BUILDING => CardTargetingType.UNIT,
                EntityType.PLAYER => CardTargetingType.BOARD, // ? We'll need to see
                EntityType.SKILL => ((Skill)cardData).TargetType,
                _ => throw new Exception("Invalid card type, no play data")
            };
            resultingPlayContext.TargetingType = targetType;
            resultingPlayContext.ValidTargets = new HashSet<int>();
            switch (targetType) // Finally, get list of valid targets depending on what to look for
            {
                case CardTargetingType.BOARD:
                    // TODO: Here we'd raise a playability context request to ask the card if special conditions
                    if (onlyRelevantTarget == 0 || onlyRelevantTarget == -1) // Valid answer always only 0 for board
                    {
                        resultingPlayContext.ValidTargets.Add(0);
                    }
                    break;
                case CardTargetingType.LANE:
                    {
                        // Check lane by lane
                        HashSet<int> optionsToCheck = null;
                        if (onlyRelevantTarget == -1)
                        {
                            if (cardData.TargetOptions.Count == 0) // All lanes targeted?
                            {
                                optionsToCheck = allPlayableLanes;
                            }
                            else
                            {
                                optionsToCheck = cardData.TargetOptions;
                            }
                        }
                        else if (cardData.TargetOptions.Contains(onlyRelevantTarget))
                        {
                            optionsToCheck = [onlyRelevantTarget];
                        }
                        else
                        {
                            // Keep null
                        }
                        foreach (int possibleTarget in optionsToCheck ?? []) // Check all options
                        {
                            if (possibleTarget >= 0 && possibleTarget < GameConstants.BOARD_NUMBER_OF_LANES)
                            {
                                // TODO: Here we'd raise an extra playability context and ask the card if has any extra conditions for lane checking
                                resultingPlayContext.ValidTargets.Add(possibleTarget);
                            }
                        }
                    }
                    break;
                case CardTargetingType.TILE: // POSSIBLE OPTIMIZATION ONLY DO FLIP CHECK ON P2
                    {
                        // Check tile by tile
                        HashSet<int> optionsToCheck = new HashSet<int>();
                        if (cardData.TargetOptions.Count == 0) // This means every option is valid
                        {
                            optionsToCheck = allPlayableTiles;
                        }
                        else
                        {
                            // Otherwise there needs to be some sort of reflection
                            foreach (int targetOption in cardData.TargetOptions)
                            {
                                // target option is an absolute but it flips depending on player
                                Lane refLane = DetailedState.BoardState.GetLaneContainingTile(targetOption);
                                int coordRelativeToLane = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_LANE, LaneRelativeIndexType.ABSOLUTE, targetOption); // Get relative to lane
                                int absoluteCoord = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, coordRelativeToLane, playerChecking); // Obtain the equivalent for the player who's checking
                                optionsToCheck.Add(absoluteCoord);
                            }
                        }
                        // Now, check if I need to provide all tiles or just match a tile
                        if (onlyRelevantTarget != -1) // In this case I need to extra filter to only the thing I'm looking for
                        {
                            if (optionsToCheck.Contains(onlyRelevantTarget)) // If target was present, then this is the one I want
                            {
                                optionsToCheck = [onlyRelevantTarget];
                            }
                            else
                            {
                                optionsToCheck = null;
                            }
                        }
                        foreach (int possibleTarget in optionsToCheck ?? [])
                        {
                            if (possibleTarget >= 0 && possibleTarget < GameConstants.BOARD_NUMBER_OF_TILES)
                            {
                                // TODO: Here we'd raise an extra playability context and ask the card if has any extra conditions for tile checking
                                resultingPlayContext.ValidTargets.Add(possibleTarget);
                            }
                        }
                    }
                    break;
                case CardTargetingType.BUILDING:
                case CardTargetingType.UNIT:
                case CardTargetingType.UNIT_AND_BUILDING:
                    {
                        // What am I looking for?
                        EntityType typeToLookFor = targetType switch
                        {
                            CardTargetingType.UNIT => EntityType.UNIT,
                            CardTargetingType.BUILDING => EntityType.BUILDING,
                            CardTargetingType.UNIT_AND_BUILDING => (EntityType.UNIT | EntityType.BUILDING),
                            _ => throw new NotImplementedException("Can never get here")
                        };
                        // Owner of the stuff I'm looking for
                        int entityOwnerCheck = cardData.EntityType switch
                        {
                            EntityType.SKILL => ((Skill)cardData).TargetOwner switch
                            {
                                EntityOwner.OPPONENT => (1 - playerChecking),
                                EntityOwner.BOTH => -1,
                                _ => playerChecking,
                            },
                            _ => playerChecking,
                        };
                        HashSet<int> possibleEntityTargets = new HashSet<int>();
                        // Check for "all" targeting
                        if (cardData.TargetOptions.Count == 0) // This means every option is valid
                        {
                            foreach (int entity in DetailedState.BoardState.GetPlacedEntities(typeToLookFor, entityOwnerCheck))
                            {
                                if (cardData.EntityType == EntityType.BUILDING) // For building, ensure the tile is available for building
                                {
                                    Unit constructingUnit = (Unit)DetailedState.EntityData[entity];
                                    // Specifically for building construction, need to ensure there's no building already here...
                                    if (DetailedState.BoardState.Tiles[constructingUnit.TileCoordinate].GetPlacedEntities(EntityType.BUILDING, -1).Count > 0)
                                    {
                                        continue; // So if there's a building already, skip this tile asap
                                    }
                                }
                                // TODO: Here we'd raise an extra playability context and ask the card if has any extra conditions for unit checking
                                possibleEntityTargets.Add(entity);
                            }
                        }
                        else // Otherwise need to find all entities tile by tile
                        {
                            foreach (int targetOption in cardData.TargetOptions) // These are tiles
                            {
                                if (targetOption >= 0 && targetOption < GameConstants.BOARD_NUMBER_OF_TILES) // Make sure tile is valid!
                                {
                                    Lane refLane = DetailedState.BoardState.GetLaneContainingTile(targetOption);
                                    int coordRelativeToLane = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_LANE, LaneRelativeIndexType.ABSOLUTE, targetOption); // Get relative to lane
                                    int absoluteCoord = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, coordRelativeToLane, playerChecking); // Obtain the equivalent for the player who's checking
                                    // Got the coord, get the entities here
                                    if (cardData.EntityType == EntityType.BUILDING) // For building, ensure the tile is available for building
                                    {
                                        // Specifically for building construction, need to ensure there's no building already here...
                                        if (DetailedState.BoardState.Tiles[absoluteCoord].GetPlacedEntities(EntityType.BUILDING, -1).Count > 0)
                                        {
                                            continue; // So if there's a building already, skip this tile
                                        }
                                    }
                                    foreach (int entity in DetailedState.BoardState.Tiles[absoluteCoord].GetPlacedEntities(typeToLookFor, entityOwnerCheck))
                                    {
                                        // TODO: Here we'd raise an extra playability context and ask the card if has any extra conditions for unit checking
                                        possibleEntityTargets.Add(entity);
                                    }
                                }
                            }
                        }
                        // Got possible target entities, if user chose one already, will need to ensure it's here
                        if (onlyRelevantTarget != -1) // Specific entity in mind
                        {
                            if(possibleEntityTargets.Contains(onlyRelevantTarget))
                            {
                                resultingPlayContext.ValidTargets = [onlyRelevantTarget]; // Ok got what I was looking for
                            }
                        }
                        else // Otherwise I return the valid targets
                        {
                            resultingPlayContext.ValidTargets = possibleEntityTargets;
                        }
                    }
                    break;
                default:
                    throw new Exception("Not a valid/implemented target type");
            }
            if (resultingPlayContext.ValidTargets.Count == 0)
            {
                resultingPlayContext.PlayOutcome = PlayOutcome.NO_TARGET_AVAILABLE;
            }
            else
            {
                resultingPlayContext.PlayOutcome = PlayOutcome.OK;
            }
            // Returns our findings
            return resultingPlayContext;
        }
        /// <summary>
        /// Begins attempt to play a card from hand
        /// </summary>
        /// <param name="card">Which card</param>
        /// <param name="chosenTarget">Target. ALWAYS AN ABSOLUTE PLACE</param>
        /// <returns>Tuple of play context and the steps themselves</returns>
        public Tuple<PlayContext, StepResult> PlayFromHand(int card, int chosenTarget)
        {
            return PLAYABLE_PlayCard(card, chosenTarget, PlayType.PLAY_FROM_HAND);
        }
        /// <summary>
        /// Plays the active power for a character
        /// </summary>
        /// <returns>Tuple of play context and the steps themselves</returns>
        public Tuple<PlayContext, StepResult> PlayActivePower()
        {
            return PLAYABLE_PlayCard(DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].ActivePowerId, 0, PlayType.ACTIVE_POWER);
        }
        // Back-end (private)
        /// <summary>
        /// Player choses card to play and where to play it.
        /// If not failed, this will change game state, function returns last step
        /// </summary>
        /// <param name="card">Which card to play</param>
        /// <param name="chosenTarget">Target</param>
        /// <param name="playType">The type of play, default is standard "play from hand"</param>
        /// <returns>Outcome, and Step result (as in step() if successful)</returns>
        Tuple<PlayContext, StepResult> PLAYABLE_PlayCard(int card, int chosenTarget, PlayType playType)
        {
            PlayContext playCtx;
            // Perform playability checks
            playCtx = GetPlayabilityOptions(card, playType, chosenTarget); // Checks if this selection can be played
            if (playCtx.PlayOutcome != PlayOutcome.OK)
            {
                return new Tuple<PlayContext, StepResult>(playCtx, null); // If failure, return type of failure, can't be played!
            }
            // Otherwise, card can be played exactly on the chosen, and checks should've been properly made in the PlayOptions step
            playCtx.PlayedTarget = (playCtx.ValidTargets.Count == 1) ? playCtx.ValidTargets.First() : throw new Exception("GetPlayabilityOptions return an invalid number of filtered targets");
            // Ok shit is going down, card needs to be paid and played now, this will result in a step and change of game state
            try // Also, a player may die!
            {
                int currentPlayer = (int)DetailedState.CurrentPlayer;
                PLAYABLE_PayCost(playCtx.PlayCost);
                switch (playType)
                {
                    case PlayType.ACTIVE_POWER:
                        ENGINE_ChangePlayerPowerAvailability(DetailedState.PlayerStates[currentPlayer], false);
                        break;
                    case PlayType.PLAY_FROM_HAND:
                        ENGINE_DiscardCardFromHand(currentPlayer, card);
                        break;
                    default:
                        throw new Exception("Invalid play type");
                }
                IngameEntity createdEntity = (IngameEntity)CardDb.GetCard(card); // The card has to be a game entity...
                playCtx.Actor = createdEntity; // Entity performs the action of "is played"
                createdEntity = PLAYABLE_PlayEntity(playCtx); // Once played, an new entity is instantiated
                // INTERACTION: CARD IS PLAYED (effect on created entity)
                playCtx.ActivatedEntity = createdEntity;
                playCtx.Actor = createdEntity;
                TRIGINTER_ProcessInteraction(InteractionType.WHEN_PLAYED, playCtx);
                // Ends by transitioning to next action phase
                ENGINE_ChangeState(States.ACTION_PHASE);
            }
            catch (EndOfGameException e)
            {
                STATE_TriggerEndOfGame(e.PlayerWhoWon);
            }
            playCtx.PlayOutcome = PlayOutcome.OK;
            return new Tuple<PlayContext, StepResult>(playCtx, _stepHistory.Last()); // Returns the thing
        }
        /// <summary>
        /// Resolves the playable state where an entity is instantiated and played
        /// </summary>
        /// <param name="playCtx">Context, contains all the relevant info for an entity to be created</param>
        /// <returns>The instantiated entity</returns>
        IngameEntity PLAYABLE_PlayEntity(PlayContext playCtx)
        {
            IngameEntity entity = playCtx.Actor;
            switch (entity.EntityType)
            {
                case EntityType.UNIT:
                    Unit newUnit = UNIT_PlayUnit((int)DetailedState.CurrentPlayer, playCtx); // Creates unit
                    PLAYABLE_RegisterOnPlayTrigger(newUnit, playCtx); // If unit has triggers on play location, register them
                    return newUnit;
                case EntityType.SKILL: // Nothing needed as skills don't introduce new entities
                    Skill skillData = (Skill)entity.Clone(); // Instances a local version of skill
                    skillData.Owner = (int)DetailedState.CurrentPlayer;
                    skillData.UniqueId = -1; // Default id for a skill (they don't persist after played)
                    return skillData;
                case EntityType.BUILDING:
                    Unit constructor = (Unit)DetailedState.EntityData[playCtx.PlayedTarget];
                    ConstructionContext constrCtx = new ConstructionContext() // This is complex so I need to make sure its ok
                    {
                        Actor = constructor, // Because target was building
                        Affected = (Building)entity,
                        AbsoluteConstructionTile = constructor.TileCoordinate
                    };
                    Building newBuilding = BUILDING_ConstructBuilding((int)DetailedState.CurrentPlayer, constrCtx);
                    PLAYABLE_RegisterOnPlayTrigger(newBuilding, playCtx); // If building has triggers on play location, register them
                    return newBuilding;
                default:
                    throw new NotImplementedException("Trying to play a non-supported type!");
            }
        }
        /// <summary>
        /// For entities with triggers, registers them where played if needed
        /// </summary>
        /// <param name="entity">Trigger owning entity</param>
        /// <param name="playCtx">Play context containing where to register</param>
        void PLAYABLE_RegisterOnPlayTrigger(LivingEntity entity, PlayContext playCtx)
        {
            // TRIGGER REGISTRATION: ON PLAYED LOCATION
            // Since it's a placed entity, target was either on a tile (unit) or on a unit (building), the result is always going to be on a tile then
            int registerTile = playCtx.TargetingType switch
            {
                CardTargetingType.TILE => playCtx.PlayedTarget,
                CardTargetingType.UNIT or CardTargetingType.BUILDING or CardTargetingType.UNIT_AND_BUILDING => ((PlacedEntity)DetailedState.EntityData[playCtx.PlayedTarget]).TileCoordinate,
                _ => throw new NotImplementedException("How was the play target in the other locations?")
            };
            BoardElement finalPlayTargetPlace = DetailedState.BoardState.Tiles[registerTile];
            TRIGINTER_VerifyEntityAndRegisterTriggers(finalPlayTargetPlace, EffectLocation.PLAY_TARGET, entity);
        }
        /// <summary>
        /// Checks if the player can afford to play a card
        /// </summary>
        /// <param name="card">Which card</param>
        /// <returns>True if can afford</returns>
        bool PLAYABLE_PlayerCanAfford(int cost)
        {
            // May need to be made smarter if someone does variable cost cards
            return (DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].CurrentGold >= cost);
        }
        /// <summary>
        /// Pays the cost of a card (e.g. if has variable cost of some weird stuff going on)
        /// </summary>
        /// <param name="cost">Cost to pay</param>
        void PLAYABLE_PayCost(int cost)
        {
            Player player = DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer];
            EFFECTS_ModifyPlayersGold(player.Owner, -cost, ModifierOperation.ADD);
        }
    }
}
