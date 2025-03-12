using System;
using System.Collections.Generic;
using System.Linq;
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
        INVALID_GAME_STATE
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
        public Tuple<PlayOutcome, CardTargets> GetPlayableOptions(int card)
        {
            // Check whether we're in the right place first (can only do this on play state)
            if(_detailedState.CurrentState != States.ACTION_PHASE)
            {
                return new Tuple<PlayOutcome, CardTargets>(PlayOutcome.INVALID_GAME_STATE, CardTargets.INVALID); // Reutnr
            }
            // An extra check first, whether card actually exists in hand
            AssortedCardCollection hand = _detailedState.PlayerStates[(int)_detailedState.CurrentPlayer].Hand;
            if (!hand.HasCard(card)) // Card not in hand!
            {
                return new Tuple<PlayOutcome, CardTargets>(PlayOutcome.INVALID_CARD, CardTargets.INVALID); // Return this (invalid card in hand!)
            }
            // Now, no other option but to retrieve the actual card I'm attempting to play
            EntityBase cardData = CardDb.GetCard(card);
            return PLAYABLE_GetOptions(cardData);
        }
        /// <summary>
        /// Player choses card to play and where to play it.
        /// If not failed, this will change game state, function returns last step
        /// </summary>
        /// <param name="card">Which card to play</param>
        /// <param name="chosenTarget">Where to play card</param>
        /// <returns>Outcome, and Step result (as in step() if successful</returns>
        public Tuple<PlayOutcome, StepResult> PlayCard(int card, CardTargets chosenTarget)
        {
            // I need to verify whether chosen card is playable
            Tuple<PlayOutcome, CardTargets> cardOptions = GetPlayableOptions(card); // Does same checks as before, whether a card can be played, and where
            if (cardOptions.Item1 != PlayOutcome.OK)
            {
                return new Tuple<PlayOutcome, StepResult>(cardOptions.Item1, null); // If failure, return type of failure, can't be played!
            }
            // Then, make sure chosen target makes sense
            if ((chosenTarget & chosenTarget - 1) != 0)
            {
                // Invalid target, either 0 or a specific single lane, not multiple!
                return new Tuple<PlayOutcome, StepResult>(PlayOutcome.INVALID_TARGET, null);
            }
            // Otherwise, card can be played somewhere, need to see if user option is valid!            
            EntityBase cardData = CardDb.GetCard(card);
            if ((cardData.EntityPlayInfo.TargetOptions & chosenTarget) != 0 || (cardData.EntityPlayInfo.TargetOptions == chosenTarget)) // Then just need to verify tagets match
            {
                // Ok shit is going down, card needs to be paid and played now, this will result in a step and change of game state
                try // Also, a player may die!
                {
                    PLAYABLE_PayCost(cardData);
                    ENGINE_DiscardCardFromHand((int)_detailedState.CurrentPlayer, card);
                    // Then the play effects
                    PLAYABLE_PlayCard(cardData, chosenTarget);
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

        // Back-end (private)

        /// <summary>
        /// Plays a card effect on current player, play is verified and card not anymore in hand, but all effects need to be made
        /// </summary>
        /// <param name="card"></param>
        /// <param name="chosenTarget"></param>
        void PLAYABLE_PlayCard(EntityBase card, CardTargets chosenTarget)
        {
            switch (card.EntityPlayInfo.EntityType)
            {
                case EntityType.UNIT:
                    UNIT_PlayUnit((int)_detailedState.CurrentPlayer, (Unit) card, chosenTarget); // Plays the unit in corresponding place
                    break;
                case EntityType.SKILL:
                case EntityType.BUILDING:
                    // TODO!
                    break;
                default:
                    throw new NotImplementedException("Trying to play a non-supported type!");
            }
        }

        /// <summary>
        /// Checks where the player can play a card
        /// </summary>
        /// <param name="card">Card they want to play</param>
        /// <returns>Whether the play outcome would be ok, and which targets could be picked</returns>
        Tuple<PlayOutcome, CardTargets> PLAYABLE_GetOptions(EntityBase card)
        {
            PlayOutcome outcome = PlayOutcome.CANT_AFFORD;
            CardTargets possibleTargets = CardTargets.INVALID;
            // First check if player can afford
            if (!PLAYABLE_PlayerCanAfford(card))
            {
                // Can't afford!
                return new Tuple<PlayOutcome, CardTargets>(outcome, possibleTargets);
            }
            // Otherwise I can def afford, check if playable
            outcome = PlayOutcome.NO_TARGET_AVAILABLE;
            if (card.EntityPlayInfo.TargetOptions == CardTargets.GLOBAL)
            {
                outcome = PLAYABLE_IsPlayableGlobal(card) ? PlayOutcome.OK : outcome;
                possibleTargets = CardTargets.GLOBAL;
                // If filled requirements, card playable
            }
            else if (card.EntityPlayInfo.TargetOptions <= CardTargets.ANY_LANE) // Otherwise need to verify individual VALID(!) lanes
            {
                int laneCandidate;
                CardTargets validTargetsIfPossible = CardTargets.GLOBAL;
                for (int i = 0; i < GameConstants.BOARD_LANES_NUMBER; i++)
                {
                    laneCandidate = 1 << i;
                    if (card.EntityPlayInfo.TargetOptions.HasFlag((CardTargets)laneCandidate)) // If this lane is one of the possible ones
                    {
                        if (PLAYABLE_IsPlayableLane(card, (CardTargets)laneCandidate))
                        {
                            outcome = PlayOutcome.OK; // Card is playable atleast somewhere!
                            validTargetsIfPossible |= (CardTargets)laneCandidate; // Add this option to list
                        }
                    }
                }
                possibleTargets = (validTargetsIfPossible != CardTargets.GLOBAL) ? validTargetsIfPossible : CardTargets.INVALID;
            }
            return new Tuple<PlayOutcome, CardTargets>(outcome, possibleTargets); // Return my findings
        }
        /// <summary>
        /// Checks if the player can afford to play a card
        /// </summary>
        /// <param name="card">Which card</param>
        /// <returns>True if can afford</returns>
        bool PLAYABLE_PlayerCanAfford(EntityBase card)
        {
            // May need to be made smarter if someone does variable cost cards
            return (_detailedState.PlayerStates[(int)_detailedState.CurrentPlayer].Gold >= int.Parse(card.EntityPrintInfo.Cost));
        }
        /// <summary>
        /// Pays the cost of a card (e.g. if has variable cost of some weird stuff going on)
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <returns>Cost in gold of card</returns>
        void PLAYABLE_PayCost(EntityBase card)
        {
            ENGINE_PlayerGoldChange((int)_detailedState.CurrentPlayer, -int.Parse(card.EntityPrintInfo.Cost));
        }
        /// <summary>
        /// Checks for a card with "global" tageting whether conditions are fulfilled
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <returns>True if playable</returns>
        bool PLAYABLE_IsPlayableGlobal(EntityBase card)
        {
            bool playable = true; // By default playable unless something happens
            foreach (TargetCondition cond in card.EntityPlayInfo.TargetConditions) // Verify individual conditions of board
            {
                switch (cond)
                {
                    case TargetCondition.NONE:
                    default:
                        playable &= true;
                        break;
                }
            }
            return playable; // If no conditions, all good
        }
        /// <summary>
        /// Whether a card can be played in the desired lane
        /// </summary>
        /// <param name="card">Which card</param>
        /// <param name="lane">Which lane</param>
        /// <returns>True if can be played in this lane</returns>
        bool PLAYABLE_IsPlayableLane(EntityBase card, CardTargets laneCandidate)
        {
            Lane laneToCheck = _detailedState.BoardState.GetLane(laneCandidate);
            bool playable = true; // By default playable unless something happens

            foreach (TargetCondition cond in card.EntityPlayInfo.TargetConditions) // Verify individual conditions of board
            {
                switch (cond)
                {
                    case TargetCondition.NONE:
                    default:
                        playable &= true;
                        break;
                }
            }
            return playable; // If no conditions, all good
        }
    }
}
