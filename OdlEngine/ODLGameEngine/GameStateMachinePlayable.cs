using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine
    {
        // --------------------------------------------------------------------------------------
        // ------------------------------  PLAY REQUESTS ----------------------------------------
        // --------------------------------------------------------------------------------------

        // Public (access points)

        /// <summary>
        /// User selects this function to check if a specific card in their hand can be played, and when/where
        /// </summary>
        /// <param name="cardInHand">Card index in hand</param>
        /// <returns>If playable, and where (if playable)</returns>
        public Tuple<PlayOutcome, ValidTargets> PLAYABLE_GetOptions(int cardInHand)
        {
            // Check whether we're in the right place first (can only do this on play state)
            if(_detailedState.CurrentState != States.ACTION_PHASE)
            {
                return new Tuple<PlayOutcome, ValidTargets>(PlayOutcome.INVALID_GAME_STATE, ValidTargets.INVALID); // Reutnr
            }
            // An extra check first, whether card actually exists in hand
            Hand hand = _detailedState.PlayerStates[GetPlayerIndexFromId(_detailedState.CurrentPlayer)].Hand;
            if (cardInHand >= hand.HandSize || cardInHand < 0) // Out of bounds!
            {
                return new Tuple<PlayOutcome, ValidTargets>(PlayOutcome.INVALID_CARD, ValidTargets.INVALID); // Return this (invalid card in hand!)
            }
            // Now, no other option but to retrieve the actual card I'm attempting to play
            cardInHand = hand.CardsInHand[cardInHand]; // Extract card id
            CardDb.LoadCard(cardInHand); // Load if not loaded before
            Card card = CardDb.cardBasicData[cardInHand];
            return PLAYABLE_GetOptions(card);
        }
        /// <summary>
        /// Player choses card to play and where to play it.
        /// If not failed, this will change game state, function returns last step
        /// </summary>
        /// <param name="cardInHand">Which card to play</param>
        /// <param name="chosenTarget">Where to play card</param>
        /// <returns>Outcome, and Step result (as in step() if successful</returns>
        public Tuple<PlayOutcome, StepResult> PlayCard(int cardInHand, ValidTargets chosenTarget)
        {
            // Check whether we're in the right place first (can only do this on play state)
            if (_detailedState.CurrentState != States.ACTION_PHASE)
            {
                return new Tuple<PlayOutcome, StepResult>(PlayOutcome.INVALID_GAME_STATE, null); // Return failure
            }
            // An extra check first, whether card actually exists in hand
            Hand hand = _detailedState.PlayerStates[GetPlayerIndexFromId(_detailedState.CurrentPlayer)].Hand;
            if (cardInHand >= hand.HandSize || cardInHand < 0) // Out of bounds!
            {
                return new Tuple<PlayOutcome, StepResult>(PlayOutcome.INVALID_CARD, null); // Return this (invalid card in hand!)
            }
            // Now, no other option but to retrieve the actual card I'm attempting to play
            int cardId = hand.CardsInHand[cardInHand]; // Extract card id
            CardDb.LoadCard(cardId); // Load if not loaded before
            Card card = CardDb.cardBasicData[cardId];
            // Otherwise, we can play stuff, but we'll need to verify...
            PlayOutcome outcome = PLAYABLE_VerifyPlayable(card, chosenTarget);
            if(outcome != PlayOutcome.OK)
            {
                return new Tuple<PlayOutcome, StepResult>(outcome, null); // If failure, return type of failure
            }
            else
            {
                // Ok shit is going down, card needs to be played now, this will result in a step
                // Todo: remove from hand, check card type and execute effects (in new function that does more engine calls as needed)
                // E.g. if skill, PlaySkill(skillcard) and that will be the one that does the switch of effect type, gets target, etc
                // Ends by transitioning to next action phase
                ENGINE_ChangeState(States.ACTION_PHASE);
            }
            return new Tuple<PlayOutcome, StepResult> ( outcome, _stepHistory.Last() ); // Returns the thing
        }

        // Back-end (private)

        /// <summary>
        /// When player selects to play, needs to verify it's a valid selection.
        /// Ideally stems from PLAYABLE_GetOptions result, but this filters out if multiple lanes are chosen or sth.
        /// </summary>
        /// <param name="card">Which card to be played</param>
        /// <param name="chosenTarget">What was the chosen target</param>
        /// <returns>If OK or not</returns>
        PlayOutcome PLAYABLE_VerifyPlayable(Card card, ValidTargets chosenTarget)
        {
            if (!PLAYABLE_PlayerCanAfford(card))
            {
                // Can't afford!
                return PlayOutcome.NO_GOLD;
            }
            if ((chosenTarget & chosenTarget - 1) != 0)
            {
                // Invalid target, either 0 or a specific single lane, not multiple!
                return PlayOutcome.INVALID_TARGET;
            }
            if (chosenTarget == ValidTargets.GLOBAL)
            {
                return PLAYABLE_IsPlayableGlobal(card) ? PlayOutcome.OK : PlayOutcome.NO_TARGET_AVAILABLE;
            }
            else
            {
                return PLAYABLE_IsPlayableLane(card, chosenTarget) ? PlayOutcome.OK : PlayOutcome.NO_TARGET_AVAILABLE;
            }
        }

        /// <summary>
        /// Checks where the player can play a card
        /// </summary>
        /// <param name="card">Card they want to play</param>
        /// <returns>Whether the play outcome would be ok, and which targets could be picked</returns>
        Tuple<PlayOutcome, ValidTargets> PLAYABLE_GetOptions(Card card)
        {
            PlayOutcome outcome = PlayOutcome.NO_GOLD;
            ValidTargets possibleTargets = ValidTargets.INVALID;
            // First check if player can afford
            if (!PLAYABLE_PlayerCanAfford(card))
            {
                // Can't afford!
                return new Tuple<PlayOutcome, ValidTargets>(outcome, possibleTargets);
            }
            // Otherwise I can def afford, check if playable
            outcome = PlayOutcome.NO_TARGET_AVAILABLE;
            if (card.TargetMode == ValidTargets.GLOBAL)
            {
                outcome = PLAYABLE_IsPlayableGlobal(card) ? PlayOutcome.OK : outcome;
                possibleTargets = ValidTargets.GLOBAL;
                // If filled requirements, card playable
            }
            else // Otherwise need to verify individual lanes
            {
                int laneCandidate;
                ValidTargets validTargetsIfPossible = ValidTargets.GLOBAL;
                for (int i = 0; i < GameConstants.BOARD_LANES_NUMBER; i++)
                {
                    laneCandidate = 1 << i;
                    if (card.TargetMode.HasFlag((ValidTargets)laneCandidate)) // If this lane is one of the possible ones
                    {
                        if (PLAYABLE_IsPlayableLane(card, (ValidTargets)laneCandidate))
                        {
                            outcome = PlayOutcome.OK; // Card is playable atleast somewhere!
                            validTargetsIfPossible &= (ValidTargets)laneCandidate; // Add this option to list
                        }
                    }
                }
                possibleTargets = (validTargetsIfPossible != ValidTargets.GLOBAL) ? validTargetsIfPossible : ValidTargets.INVALID;
            }
            return new Tuple<PlayOutcome, ValidTargets>(outcome, possibleTargets); // Return my findings
        }
        /// <summary>
        /// Checks if the player can afford to play a card
        /// </summary>
        /// <param name="card">Which card</param>
        /// <returns>True if can afford</returns>
        bool PLAYABLE_PlayerCanAfford(Card card)
        {
            // May need to be made smarter if someone does variable cost cards
            return (_detailedState.PlayerStates[GetPlayerIndexFromId(_detailedState.CurrentPlayer)].Gold >= int.Parse(card.Cost));
        }
        /// <summary>
        /// Checks for a card with "global" tageting whether conditions are fulfilled
        /// </summary>
        /// <param name="card">Card to check</param>
        /// <returns>True if playable</returns>
        bool PLAYABLE_IsPlayableGlobal(Card card)
        {
            bool playable = true; // By default playable unless something happens
            foreach (TargetCondition cond in card.TargetConditions) // Verify individual conditions of board
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
        bool PLAYABLE_IsPlayableLane(Card card, ValidTargets laneCandidate)
        {
            Lane laneToCheck = _detailedState.BoardState.GetLane(laneCandidate);
            bool playable = true; // By default playable unless something happens

            foreach (TargetCondition cond in card.TargetConditions) // Verify individual conditions of board
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
