using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    public static class TestHelperFunctions
    {
        /// <summary>
        /// Checks if player state has is already present or not in a set
        /// </summary>
        /// <param name="st">State of player</param>
        /// <param name="set">Hash set</param>
        /// <param name="shouldBe">Should it be present?</param>
        public static void HashSetVerification(IHashable st, HashSet<int> set, bool shouldBe)
        {
            Assert.IsTrue(shouldBe == set.Contains(st.GetGameStateHash()));
            if (!set.Contains(st.GetGameStateHash()))
            {
                set.Add(st.GetGameStateHash());
            }
        }
        /// <summary>
        /// Checks if deck 1-30 is properly shuffled, has a 2.6525286e+32 chance of messing up because you may get a perfect shuffle
        /// </summary>
        /// <returns>If deck's shuffled</returns>
        public static bool IsDeckShuffled(PlayerState p)
        {
            for (int i = 0; i < p.Deck.DeckSize; i++)
            {
                if (p.Deck.PeepAt(i) != i + 1)
                {
                    return true; // A single difference is all it takes
                }
            }
            return false;
        }
        /// <summary>
        /// Checks whether a player left init stage perfectly
        /// </summary>
        /// <param name="p">Player</param>
        /// <returns>True if properly init</returns>
        public static void VerifyPlayerInitialised(PlayerState p)
        {
            Assert.AreEqual(p.Hp.Total, GameConstants.STARTING_HP);
            Assert.AreEqual(p.Gold, GameConstants.STARTING_GOLD);
            Assert.AreEqual(p.Hand.CardCount, GameConstants.STARTING_CARDS);
            Assert.AreEqual(p.Deck.DeckSize, GameConstants.DECK_SIZE - GameConstants.STARTING_CARDS);
        }
        /// <summary>
        /// Verifies from a state machine in draw phase, that draw phase succeeds properly. Leaves the state machine as it began
        /// </summary>
        /// <param name="sm">State machine to try</param>
        public static void VerifyDrawPhaseResult(GameStateMachine sm)
        {
            HashSet<int> hashes = new HashSet<int>(); // Checks all hashes resulting
            GameStateStruct testState = sm.DetailedState;
            Assert.AreEqual(testState.CurrentState, States.DRAW_PHASE); // Am I in draw phase
            int preCards = testState.PlayerStates[(int)testState.CurrentPlayer].Hand.CardCount;
            int preGold = testState.PlayerStates[(int)testState.CurrentPlayer].Gold;
            int preDeck = testState.PlayerStates[(int)testState.CurrentPlayer].Deck.DeckSize;
            // Player hashes init for first time, also hands and decks, also state!
            HashSetVerification(testState.PlayerStates[0], hashes, false);
            HashSetVerification(testState.PlayerStates[1], hashes, false);
            HashSetVerification(testState.PlayerStates[0].Hand, hashes, false);
            HashSetVerification(testState.PlayerStates[1].Hand, hashes, false);
            HashSetVerification(testState.PlayerStates[0].Deck, hashes, false);
            HashSetVerification(testState.PlayerStates[1].Deck, hashes, false);
            HashSetVerification(testState, hashes, false);
            // Now draw!
            sm.Step();
            testState = sm.DetailedState;
            int postCards = testState.PlayerStates[(int)testState.CurrentPlayer].Hand.CardCount;
            int postGold = testState.PlayerStates[(int)testState.CurrentPlayer].Gold;
            int postDeck = testState.PlayerStates[(int)testState.CurrentPlayer].Deck.DeckSize;
            Assert.AreEqual(testState.CurrentState, States.ACTION_PHASE); // Am I in next phase
            Assert.AreEqual(postCards - preCards, GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player draw exact amount of cards
            Assert.AreEqual(postGold - preGold, GameConstants.DRAW_PHASE_GOLD_OBTAINED); // Did player gain exact amount of gold
            Assert.AreEqual(postDeck - preDeck, -GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player deck shrink the exact amount
            // Only one player should've changed, the current one, so that one should have brand new hashes, state is obviosuly always new
            HashSetVerification(testState.PlayerStates[0], hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_1);
            HashSetVerification(testState.PlayerStates[1], hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_2);
            HashSetVerification(testState.PlayerStates[0].Hand, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_1);
            HashSetVerification(testState.PlayerStates[1].Hand, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_2);
            HashSetVerification(testState.PlayerStates[0].Deck, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_1);
            HashSetVerification(testState.PlayerStates[1].Deck, hashes, testState.CurrentPlayer != CurrentPlayer.PLAYER_2);
            HashSetVerification(testState, hashes, false);
            // Now revert
            sm.UndoPreviousStep(); // Go back to beginning of drawphase
            testState = sm.DetailedState;
            preCards = testState.PlayerStates[(int)testState.CurrentPlayer].Hand.CardCount;
            preGold = testState.PlayerStates[(int)testState.CurrentPlayer].Gold;
            preDeck = testState.PlayerStates[(int)testState.CurrentPlayer].Deck.DeckSize;
            Assert.AreEqual(testState.CurrentState, States.DRAW_PHASE); // Am I in draw phase again
            Assert.AreEqual(postCards - preCards, GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player restore cards
            Assert.AreEqual(postGold - preGold, GameConstants.DRAW_PHASE_GOLD_OBTAINED); // Did player restore gold
            Assert.AreEqual(postDeck - preDeck, -GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player deck recover the card
            // All hashes should be present still
            HashSetVerification(testState.PlayerStates[0], hashes, true);
            HashSetVerification(testState.PlayerStates[1], hashes, true);
            HashSetVerification(testState.PlayerStates[0].Hand, hashes, true);
            HashSetVerification(testState.PlayerStates[1].Hand, hashes, true);
            HashSetVerification(testState.PlayerStates[0].Deck, hashes, true);
            HashSetVerification(testState.PlayerStates[1].Deck, hashes, true);
            HashSetVerification(testState, hashes, true);
        }
        /// <summary>
        /// Adds entity to board, no checks
        /// </summary>
        /// <param name="state">GameState</param>
        /// <param name="lane">Which lane</param>
        /// <param name="tileCoord">Which tile</param>
        /// <param name="uniqueId">Desired ID</param>
        /// <param name="owner">Entity owner index</param>
        /// <param name="entity">Entity to add</param>
        static public void ManualInitEntity(GameStateStruct state, TargetLocation lane, int tileCoord, int uniqueId, int owner, PlacedEntity entity)
        {
            entity.Owner = owner;
            entity.UniqueId = uniqueId;
            // Add to board and sm
            state.EntityData.Add(uniqueId, entity);
            state.BoardState.EntityListOperation(entity, EntityListOperation.ADD);
            // Add to lane
            entity.LaneCoordinate = (LaneID)lane;
            state.BoardState.GetLane(lane).EntityListOperation(entity, EntityListOperation.ADD);
            // Add to tile
            entity.TileCoordinate = state.BoardState.GetLane(lane).GetAbsoluteTileCoord(tileCoord, entity.Owner);
            state.BoardState.GetLane(lane).GetTileAbsolute(entity.TileCoordinate).EntityListOperation(entity, EntityListOperation.ADD);
        }
    }
}
