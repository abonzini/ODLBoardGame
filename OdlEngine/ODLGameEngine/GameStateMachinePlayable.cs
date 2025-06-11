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
                if (!hand.HasCard(card)) // Card not in hand!
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
                EntityType.UNIT => CardTargetingType.TILE_RELATIVE,
                EntityType.BUILDING => CardTargetingType.UNIT_RELATIVE,
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
                        if (onlyRelevantTarget == -1) optionsToCheck = cardData.TargetOptions;
                        else if (cardData.TargetOptions.Contains(onlyRelevantTarget)) optionsToCheck = [onlyRelevantTarget];
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
                case CardTargetingType.TILE:
                case CardTargetingType.TILE_RELATIVE:
                    {
                        // Check tile by tile
                        HashSet<int> optionsToCheck;
                        // Check if I need to convert tiles to relative first
                        if (targetType == CardTargetingType.TILE_RELATIVE)
                        {
                            optionsToCheck = new HashSet<int>();
                            foreach (int targetOption in cardData.TargetOptions)
                            {
                                // target option is an absolute but it flips depending on player
                                Lane refLane = DetailedState.BoardState.GetLaneContainingTile(targetOption);
                                int coordRelativeToLane = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_LANE, LaneRelativeIndexType.ABSOLUTE, targetOption); // Get relative to lane
                                int absoluteCoord = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, coordRelativeToLane, playerChecking); // Obtain the equivalent for the player who's checking
                                optionsToCheck.Add(absoluteCoord);
                            }
                        }
                        else
                        {
                            optionsToCheck = cardData.TargetOptions;
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
                case CardTargetingType.UNIT_RELATIVE:
                    {
                        // What am I looking for?
                        EntityType typeToLookFor = targetType switch
                        {
                            CardTargetingType.UNIT or CardTargetingType.UNIT_RELATIVE => EntityType.UNIT,
                            CardTargetingType.BUILDING => EntityType.BUILDING,
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
                        HashSet<int> tilesToCheck;
                        // Check if I need to convert tiles to relative first
                        if (targetType == CardTargetingType.UNIT_RELATIVE)
                        {
                            tilesToCheck = new HashSet<int>();
                            foreach (int targetOption in cardData.TargetOptions)
                            {
                                Lane refLane = DetailedState.BoardState.GetLaneContainingTile(targetOption);
                                int coordRelativeToLane = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_LANE, LaneRelativeIndexType.ABSOLUTE, targetOption); // Get relative to lane
                                int absoluteCoord = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, coordRelativeToLane, playerChecking); // Obtain the equivalent for the player who's checking
                                tilesToCheck.Add(absoluteCoord);
                            }
                        }
                        else
                        {
                            tilesToCheck = cardData.TargetOptions;
                        }
                        // This one is kinda tricky, will go tile by tile, but look for entities instead so...
                        if (onlyRelevantTarget == -1) // Get all valid entities in this case
                        {
                            foreach (int possibleTarget in tilesToCheck)
                            {
                                if (possibleTarget >= 0 && possibleTarget < GameConstants.BOARD_NUMBER_OF_TILES) // Check if valid tile
                                {
                                    // Check all of the units in a tile
                                    foreach (int entity in DetailedState.BoardState.Tiles[possibleTarget].GetPlacedEntities(typeToLookFor, entityOwnerCheck))
                                    {
                                        // TODO: Here we'd raise an extra playability context and ask the card if has any extra conditions for unit checking
                                        resultingPlayContext.ValidTargets.Add(entity);
                                    }
                                }
                            }
                        }
                        else if (DetailedState.EntityData.TryGetValue(onlyRelevantTarget, out LivingEntity foundEntity)) // Check if unit truly exists
                        {
                            if (entityOwnerCheck == -1 || entityOwnerCheck == foundEntity.Owner) // Check to make sure it's the same ownership I'm looking for
                            {
                                if (tilesToCheck.Contains(((PlacedEntity)foundEntity).TileCoordinate)) // Check if unit in a valid tile
                                {
                                    // TODO: Here we'd raise an extra playability context and ask the card if has any extra conditions for unit checking
                                    resultingPlayContext.ValidTargets.Add(onlyRelevantTarget);
                                }
                            }
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
                    Building newBuilding = BUILDING_ConstructBuilding((int)DetailedState.CurrentPlayer, playCtx);
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
                CardTargetingType.TILE or CardTargetingType.TILE_RELATIVE => playCtx.PlayedTarget,
                CardTargetingType.UNIT or CardTargetingType.UNIT_RELATIVE or CardTargetingType.BUILDING => ((PlacedEntity)DetailedState.EntityData[playCtx.PlayedTarget]).TileCoordinate,
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
