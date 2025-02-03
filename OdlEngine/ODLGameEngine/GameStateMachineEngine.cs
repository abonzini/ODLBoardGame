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
        // -------------------------------  GAME ENGINE REQUESTS --------------------------------
        // --------------------------------------------------------------------------------------
        // Use ENGINE_ in fn names, the more convoluted game mechanics (e.g. when player builds X, or when player draws card) need to be dealt here
        /// <summary>
        /// Advances state machine
        /// </summary>
        /// <param name="state">State to go to</param>
        void ENGINE_ChangeState(States state)
        {
            ExecuteEvent(
                new TransitionEvent<States>()
                {
                    eventType = EventType.STATE_TRANSITION,
                    newValue = state,
                    description = $"Next state: {Enum.GetName(state)}"
                });
        }
        /// <summary>
        /// Toggles active player
        /// </summary>
        void ENGINE_TogglePlayer()
        {
            var nextPlayer = _detailedState.CurrentPlayer switch // Player is always 1 unless it goes from 1 -> 2
            {
                PlayerId.PLAYER_1 => PlayerId.PLAYER_2,
                _ => PlayerId.PLAYER_1,
            };
            ExecuteEvent(
                new TransitionEvent<PlayerId>()
                {
                    eventType = EventType.PLAYER_TRANSITION,
                    newValue = nextPlayer,
                    description = $"Switched to {Enum.GetName(nextPlayer)}"
                });
        }
        /// <summary>
        /// Next step will have a new rng seed, important to mantain determinism
        /// </summary>
        /// <param name="seed">Seed to adopt</param>
        void ENGINE_NewRngSeed(int seed)
        {
            ExecuteEvent(
                new TransitionEvent<int>()
                {
                    eventType = EventType.RNG_TRANSITION,
                    newValue = seed
                });
        }
        /// <summary>
        /// Sets a player HP to new value
        /// </summary>
        /// <param name="p">Which player</param>
        /// <param name="hp">Which value</param>
        void ENGINE_SetPlayerHp(PlayerId p, int hp)
        {
            ExecuteEvent(
                new PlayerTransitionEvent<int>()
                {
                    eventType = EventType.PLAYER_HP_TRANSITION,
                    playerId = p,
                    newValue = hp,
                    description = $"P{GetPlayerIndexFromId(p) + 1} now has {hp} HP"
                });
        }
        /// <summary>
        /// Sets a player gold to new value
        /// </summary>
        /// <param name="p">Which player</param>
        /// <param name="gold">Which value</param>
        void ENGINE_SetPlayerGold(PlayerId p, int gold)
        {
            ExecuteEvent(
                new PlayerTransitionEvent<int>()
                {
                    eventType = EventType.PLAYER_GOLD_TRANSITION,
                    playerId = p,
                    newValue = gold,
                    description = $"P{GetPlayerIndexFromId(p) + 1} now has {gold} gold"
                });
        }
        /// <summary>
        /// Adds message that can be seen by player
        /// </summary>
        /// <param name="msg">Message</param>
        void ENGINE_AddMessageEvent(string msg)
        {
            ExecuteEvent(
                new Event()
                {
                    eventType = EventType.MESSAGE,
                    description = msg
                });
        }
        /// <summary>
        /// Swaps 2 cards in a player's deck, for shuffling
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="card1">Card 1</param>
        /// <param name="card2">Card 2</param>
        void ENGINE_SwapCardsInDeck(PlayerId p, int card1, int card2)
        {
            ExecuteEvent(
                new PlayerTransitionEvent<int>()
                {
                    eventType = EventType.CARD_DECK_SWAP,
                    playerId = p,
                    newValue = card1,
                    oldValue = card2
                });
        }
        /// <summary>
        /// Draws a single card for a player
        /// </summary>
        /// <param name="p">Player</param>
        void ENGINE_DeckDrawSingle(PlayerId p)
        {
            ExecuteEvent(
                new PlayerEvent()
                {
                    eventType = EventType.DECK_DRAW,
                    playerId = p
                });
        }
        /// <summary>
        /// Change the gold of player (gain or loss)
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="goldDelta">How much gold to gain/lose</param>
        void ENGINE_PlayerGoldChange(PlayerId p, int goldDelta)
        {
            ExecuteEvent(
                new PlayerValueEvent<int>()
                {
                    eventType = EventType.PLAYER_GOLD_CHANGE,
                    playerId = p,
                    value = goldDelta,
                    description = $"P{GetPlayerIndexFromId(p) + 1} {((goldDelta > 0) ? "gains" : "loses")} {Math.Abs(goldDelta)} gold"
                });
        }
    }
}
