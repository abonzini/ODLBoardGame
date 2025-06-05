namespace ODLGameEngine
{
    public enum PlayOutcome
    {
        OK,
        CANT_AFFORD,
        NO_TARGET_AVAILABLE,
        INVALID_TARGET,
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
        /// Checker that verifies whether a card is playable, and if is, returns the whole data of how it could be played
        /// </summary>
        /// <param name="card">The card number, of the that would be played</param>
        /// <param name="playType">Type of play (i.e. where the card is played from)</param>
        /// <param name="playLocationFilter">If added, will only check in those specific locations, important for actually playing the card, to not overcheck</param>
        /// <returns>The context that tells us whether the card is playable</returns>
        public PlayContext GetPlayabilityOptions(int card, PlayType playType, PlayTargetLocation playLocationFilter = PlayTargetLocation.ALL_LANES)
        {
            PlayContext resultingPlayContext = new PlayContext();
            resultingPlayContext.PlayType = playType;
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
                AssortedCardCollection hand = DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].Hand;
                if (!hand.HasCard(card)) // Card not in hand!
                {
                    resultingPlayContext.PlayOutcome = PlayOutcome.INVALID_CARD;
                    return resultingPlayContext;
                }
            }
            // In this case we check instead if the active power is allowed
            else if (playType == PlayType.ACTIVE_POWER)
            {
                if (!DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].PowerAvailable) // Power not available!
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
            // Otherwise I can def afford, check if playable in the desired place
            resultingPlayContext.PlayOutcome = PlayOutcome.NO_TARGET_AVAILABLE;
            if (cardData.TargetOptions == PlayTargetLocation.BOARD) // Card is board-targetable
            {
                // TODO: Here we'd raise a playability context request to ask the card if special conditions
                resultingPlayContext.PlayOutcome = PlayOutcome.OK;
                resultingPlayContext.PlayTarget = PlayTargetLocation.BOARD;
            }
            else if (cardData.TargetOptions <= PlayTargetLocation.ALL_LANES) // Otherwise need to verify individual VALID(!) lanes
            {
                PlayTargetLocation validTargetsIfPossible = PlayTargetLocation.BOARD;
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_LANES; i++)
                {
                    PlayTargetLocation nextLaneCandidate = (PlayTargetLocation)(1 << i);
                    if (!playLocationFilter.HasFlag(nextLaneCandidate)) // Check only the ones I'm interested in
                    {
                        continue;
                    }
                    if (cardData.TargetOptions.HasFlag(nextLaneCandidate)) // If this lane is one of the valid ones for this card
                    {
                        bool canPlay = true; // By default, can play
                        if (cardData.EntityType == EntityType.BUILDING) // Buildings have an extra check, where they need to see if/how they can be built
                        {
                            // Get construction context
                            ConstructionContext constructionContext = BUILDING_GetBuildingOptions((int)DetailedState.CurrentPlayer, (Building)cardData, nextLaneCandidate);
                            resultingPlayContext.LastAuxContext = constructionContext;
                            canPlay = (constructionContext.AbsoluteConstructionTile >= 0); // Playable if this found a valid coordinate
                        }
                        else if (cardData.EntityType == EntityType.UNIT)
                        {
                            // Get unit play context, canPlay may depend on other extra things
                            UnitPlayContext unitPlayContext = UNIT_GetUnitPlayData((int)DetailedState.CurrentPlayer, (Unit)cardData, nextLaneCandidate);
                            resultingPlayContext.LastAuxContext = unitPlayContext;
                        }
                        if (canPlay) // If building (or card in general) can play normally, then check
                        {
                            // TODO: Here we raise an extra playability context and ask the card if has any extra conditions
                            resultingPlayContext.PlayOutcome = PlayOutcome.OK;
                            validTargetsIfPossible |= nextLaneCandidate; // Add this lane option to list
                        }
                    }
                }
                resultingPlayContext.PlayTarget = (validTargetsIfPossible != PlayTargetLocation.BOARD) ? validTargetsIfPossible : PlayTargetLocation.INVALID;
            }
            // Returns our findings
            return resultingPlayContext;
        }
        /// <summary>
        /// Begins attempt to play a card from hand
        /// </summary>
        /// <param name="card">Which card</param>
        /// <param name="chosenTarget">Target location</param>
        /// <returns>Tuple of play context and the steps themselves</returns>
        public Tuple<PlayContext, StepResult> PlayFromHand(int card, PlayTargetLocation chosenTarget)
        {
            return PLAYABLE_PlayCard(card, chosenTarget, PlayType.PLAY_FROM_HAND);
        }
        /// <summary>
        /// Plays the active power for a character
        /// </summary>
        /// <returns>Tuple of play context and the steps themselves</returns>
        public Tuple<PlayContext, StepResult> PlayActivePower()
        {
            return PLAYABLE_PlayCard(DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].ActivePowerId, PlayTargetLocation.BOARD, PlayType.ACTIVE_POWER);
        }
        // Back-end (private)
        /// <summary>
        /// Player choses card to play and where to play it.
        /// If not failed, this will change game state, function returns last step
        /// </summary>
        /// <param name="card">Which card to play</param>
        /// <param name="chosenTarget">Where to play card</param>
        /// <param name="playType">The type of play, default is standard "play from hand"</param>
        /// <returns>Outcome, and Step result (as in step() if successful)</returns>
        Tuple<PlayContext, StepResult> PLAYABLE_PlayCard(int card, PlayTargetLocation chosenTarget, PlayType playType)
        {
            PlayContext playCtx = new PlayContext();
            // First, make sure chosen target makes sense, card should be only playable exactly where I asked
            if (((chosenTarget & chosenTarget - 1) != 0) || (chosenTarget >= PlayTargetLocation.INVALID)) // Check only power of 2 or 0, and less than invalid
            {
                // Invalid target, either 0 or a specific single lane, not multiple or values higher than the allowed lanes!
                playCtx.PlayOutcome = PlayOutcome.INVALID_TARGET;
                playCtx.PlayType = playType;
                return new Tuple<PlayContext, StepResult>(playCtx, null);
            }
            // Then, all other checks
            playCtx = GetPlayabilityOptions(card, playType, chosenTarget); // Checks where a card can be played and how
            if (playCtx.PlayOutcome != PlayOutcome.OK)
            {
                return new Tuple<PlayContext, StepResult>(playCtx, null); // If failure, return type of failure, can't be played!
            }
            // Otherwise, card can be played somewhere. Should be only playable where I targeted         
            if (playCtx.PlayTarget == chosenTarget) // Then just need to verify tagets match with playable options
            {
                // Ok shit is going down, card needs to be paid and played now, this will result in a step and change of game state
                try // Also, a player may die!
                {
                    PLAYABLE_PayCost(playCtx.PlayCost);
                    if (playType == PlayType.PLAY_FROM_HAND)
                    {
                        ENGINE_DiscardCardFromHand((int)DetailedState.CurrentPlayer, card);
                    }
                    else if (playType == PlayType.ACTIVE_POWER)
                    {
                        ENGINE_ChangePlayerPowerAvailability(DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer], false);
                    }
                    IngameEntity createdEntity = (IngameEntity)CardDb.GetCard(card); // The card has to be a game entity...
                    playCtx.ActivatedEntity = createdEntity; // Set this reference entity which will be played
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
            else
            {
                playCtx.PlayOutcome = PlayOutcome.INVALID_TARGET;
                return new Tuple<PlayContext, StepResult>(playCtx, null);
            }
        }
        /// <summary>
        /// Resolves the playable state where an entity is instantiated and played
        /// </summary>
        /// <param name="playCtx">Context, contains all the relevant info for an entity to be created</param>
        /// <returns>The instantiated entity</returns>
        IngameEntity PLAYABLE_PlayEntity(PlayContext playCtx)
        {
            IngameEntity entity = playCtx.ActivatedEntity;
            switch (entity.EntityType)
            {
                case EntityType.UNIT:
                    return UNIT_PlayUnit((int)DetailedState.CurrentPlayer, (UnitPlayContext)playCtx.LastAuxContext);
                case EntityType.SKILL: // Nothing needed as skills don't introduce new entities
                    Skill skillData = (Skill)entity.Clone(); // Instances a local version of skill
                    skillData.Owner = (int)DetailedState.CurrentPlayer;
                    skillData.UniqueId = -1; // Default id for a skill (they don't persist after played)
                    return skillData;
                case EntityType.BUILDING:
                    return BUILDING_ConstructBuilding((int)DetailedState.CurrentPlayer, (ConstructionContext)playCtx.LastAuxContext);
                default:
                    throw new NotImplementedException("Trying to play a non-supported type!");
            }
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
            TRIGINTER_ModifyPlayersGold(player.Owner, -cost, ModifierOperation.ADD);
        }
    }
}
