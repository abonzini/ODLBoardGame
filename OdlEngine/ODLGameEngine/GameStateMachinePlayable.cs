using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        // ------------------------------  PLAY REQUESTS ----------------------------------------
        // --------------------------------------------------------------------------------------

        // Public (access points)

        /// <summary>
        /// User selects this function to check if a specific card in their hand can be played, and when/where
        /// </summary>
        /// <param name="card">Card to play</param>
        /// <returns>If playable, and where (if playable)</returns>
        public Tuple<PlayOutcome, TargetLocation> GetPlayableOptions(int card, PlayType playType)
        {
            // Check whether we're in the right place first (can only do this on play state)
            if(DetailedState.CurrentState != States.ACTION_PHASE)
            {
                return new Tuple<PlayOutcome, TargetLocation>(PlayOutcome.INVALID_GAME_STATE, TargetLocation.INVALID); // Return
            }
            // An extra check first, whether card actually exists in hand (if applicable)
            if(playType == PlayType.PLAY_FROM_HAND)
            {
                AssortedCardCollection hand = DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].Hand;
                if (!hand.HasCard(card)) // Card not in hand!
                {
                    return new Tuple<PlayOutcome, TargetLocation>(PlayOutcome.INVALID_CARD, TargetLocation.INVALID); // Return this (invalid card in hand!)
                }
            }
            else if (playType == PlayType.ACTIVE_POWER)
            {
                if (!DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].PowerAvailable) // Power not available!
                {
                    return new Tuple<PlayOutcome, TargetLocation>(PlayOutcome.POWER_ALREADY_USED, TargetLocation.INVALID); // Return this (invalid card in hand!)
                }
            }
            else
            {
                //??
            }
            // Now, no other option but to retrieve the actual card I'm attempting to play
            EntityBase cardData = CardDb.GetCard(card);
            return PLAYABLE_GetOptions(cardData);
        }
        public Tuple<PlayOutcome, StepResult> PlayFromHand(int card, TargetLocation chosenTarget)
        {
            return PlayCard(card, chosenTarget, PlayType.PLAY_FROM_HAND);
        }
        /// <summary>
        /// Plays the active power for a character
        /// </summary>
        /// <returns>Like PlayCard, chain of effects after power was played</returns>
        public Tuple<PlayOutcome, StepResult> PlayActivePower()
        {
            return PlayCard(DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].ActivePowerCast, TargetLocation.BOARD, PlayType.ACTIVE_POWER);
        }
        // Back-end (private)
        /// <summary>
        /// Player choses card to play and where to play it.
        /// If not failed, this will change game state, function returns last step
        /// </summary>
        /// <param name="card">Which card to play</param>
        /// <param name="chosenTarget">Where to play card</param>
        /// <param name="playType">The type of play, default is standard "play from hand"</param>
        /// <returns>Outcome, and Step result (as in step() if successful</returns>
        Tuple<PlayOutcome, StepResult> PlayCard(int card, TargetLocation chosenTarget, PlayType playType)
        {
            // I need to verify whether chosen card is playable
            Tuple<PlayOutcome, TargetLocation> cardOptions = GetPlayableOptions(card, playType); // Does same checks as before, whether a card can be played, and where
            if (cardOptions.Item1 != PlayOutcome.OK)
            {
                return new Tuple<PlayOutcome, StepResult>(cardOptions.Item1, null); // If failure, return type of failure, can't be played!
            }
            // Then, make sure chosen target makes sense
            if (((chosenTarget & chosenTarget - 1) != 0) || (chosenTarget > TargetLocation.ALL_LANES))
            {
                // Invalid target, either 0 or a specific single lane, not multiple or values higher than the allowed lanes!
                return new Tuple<PlayOutcome, StepResult>(PlayOutcome.INVALID_TARGET, null);
            }
            // Otherwise, card can be played somewhere, need to see if user option is valid!            
            if ((cardOptions.Item2 & chosenTarget) != 0 || (cardOptions.Item2 == chosenTarget)) // Then just need to verify tagets match with playable options
            {
                // Ok shit is going down, card needs to be paid and played now, this will result in a step and change of game state
                try // Also, a player may die!
                {
                    EntityBase cardData = CardDb.GetCard(card);
                    PLAYABLE_PayCost(cardData);
                    if (playType == PlayType.PLAY_FROM_HAND)
                    {
                        ENGINE_DiscardCardFromHand((int)DetailedState.CurrentPlayer, card);
                    }
                    else if (playType == PlayType.ACTIVE_POWER)
                    {
                        ENGINE_ChangePlayerPowerAvailability(DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer], false);
                    }
                    // Then the play effects
                    IngameEntity createdEntity = PLAYABLE_PlayCard(cardData, chosenTarget); // Once played, an entity exists now
                    // INTERACTION: CARD IS PLAYED
                    PlayContext playCtx = new PlayContext() { ActivatedEntity = createdEntity, Actor = createdEntity, LaneTargets = chosenTarget };
                    TRIGINTER_ProcessInteraction(InteractionType.WHEN_PLAYED, playCtx);
                    // Ends by transitioning to next action phase
                    ENGINE_ChangeState(States.ACTION_PHASE);
                }
                catch (EndOfGameException e)
                {
                    STATE_TriggerEndOfGame(e.PlayerWhoWon);
                }
                return new Tuple<PlayOutcome, StepResult>(PlayOutcome.OK, _stepHistory.Last()); // Returns the thing
            }
            else
            {
                return new Tuple<PlayOutcome, StepResult>(PlayOutcome.INVALID_TARGET, null);
            }
        }
        /// <summary>
        /// Plays a card effect on current player, play is verified and card not anymore in hand, but all effects need to be made
        /// </summary>
        /// <param name="card"></param>
        /// <param name="chosenTarget"></param>
        /// <returns>The entity that was generated for this play</returns>
        IngameEntity PLAYABLE_PlayCard(EntityBase card, TargetLocation chosenTarget)
        {
            switch (card.PreInstanceInfo.EntityType)
            {
                case EntityType.UNIT:
                    return UNIT_PlayUnit((int)DetailedState.CurrentPlayer, (Unit) card, chosenTarget); // Plays the unit in corresponding place
                case EntityType.SKILL: // Nothing needed as skills don't introduce new entities
                    Skill skillData = (Skill)card.Clone(); // Instances a local version of skill
                    skillData.Owner = (int)DetailedState.CurrentPlayer;
                    skillData.UniqueId = -1; // Default id for a skill (they don't persist after played)
                    return skillData;
                case EntityType.BUILDING:
                    return BUILDING_PlayBuilding((int)DetailedState.CurrentPlayer, (Building)card, chosenTarget); // Plays building in tile
                default:
                    throw new NotImplementedException("Trying to play a non-supported type!");
            }
        }

        /// <summary>
        /// Checks where the player can play a card
        /// </summary>
        /// <param name="card">Card they want to play</param>
        /// <returns>Whether the play outcome would be ok, and which targets could be picked</returns>
        Tuple<PlayOutcome, TargetLocation> PLAYABLE_GetOptions(EntityBase card)
        {
            PlayOutcome outcome = PlayOutcome.CANT_AFFORD;
            TargetLocation possibleTargets = TargetLocation.INVALID;
            // First check if player can afford
            if (!PLAYABLE_PlayerCanAfford(card))
            {
                // Can't afford!
                return new Tuple<PlayOutcome, TargetLocation>(outcome, possibleTargets);
            }
            // Otherwise I can def afford, check if playable in the desired place
            outcome = PlayOutcome.NO_TARGET_AVAILABLE;
            if (card.PreInstanceInfo.TargetOptions == TargetLocation.BOARD)
            {
                // TODO: Here we raise a playability context and ask the card
                outcome = PlayOutcome.OK;
                possibleTargets = TargetLocation.BOARD;
                // If filled requirements, card playable
            }
            else if (card.PreInstanceInfo.TargetOptions <= TargetLocation.ALL_LANES) // Otherwise need to verify individual VALID(!) lanes
            {
                int laneCandidate;
                TargetLocation validTargetsIfPossible = TargetLocation.BOARD;
                for (int i = 0; i < GameConstants.BOARD_LANES_NUMBER; i++)
                {
                    laneCandidate = 1 << i;
                    if (card.PreInstanceInfo.TargetOptions.HasFlag((TargetLocation)laneCandidate)) // If this lane is one of the possible ones
                    {
                        bool canPlay = true; // By default, can play
                        if(card.PreInstanceInfo.EntityType == EntityType.BUILDING) // Buildings have an extra check, where they need to see if they can be built
                        {
                            canPlay = (BUILDING_GetBuildingOptions((int)DetailedState.CurrentPlayer, (Building)card, (TargetLocation)laneCandidate).FirstAvailableOption >= 0);
                        }
                        if (canPlay) // If building (or card in general) can play normally, then check
                        {
                            // TODO: Here we raise a playability context and ask the card
                            outcome = PlayOutcome.OK;
                            validTargetsIfPossible |= (TargetLocation)laneCandidate; // Add this lane option to list
                        }
                    }
                }
                possibleTargets = (validTargetsIfPossible != TargetLocation.BOARD) ? validTargetsIfPossible : TargetLocation.INVALID;
            }
            return new Tuple<PlayOutcome, TargetLocation>(outcome, possibleTargets); // Return my findings
        }
        /// <summary>
        /// Checks if the player can afford to play a card
        /// </summary>
        /// <param name="card">Which card</param>
        /// <returns>True if can afford</returns>
        bool PLAYABLE_PlayerCanAfford(EntityBase card)
        {
            // May need to be made smarter if someone does variable cost cards
            return (DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer].Gold >= int.Parse(card.PreInstanceInfo.Cost));
        }
        /// <summary>
        /// Pays the cost of a card (e.g. if has variable cost of some weird stuff going on)
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <returns>Cost in gold of card</returns>
        void PLAYABLE_PayCost(EntityBase card)
        {
            PlayerState player = DetailedState.PlayerStates[(int)DetailedState.CurrentPlayer];
            TRIGINTER_ModifyPlayersGold(player.Owner, -int.Parse(card.PreInstanceInfo.Cost), ModifierOperation.ADD);
        }
    }
}
