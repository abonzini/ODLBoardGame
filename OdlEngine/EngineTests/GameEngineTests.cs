using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EngineTests
{
    [TestClass]
    public class GameEngineTests // For debugging and control, verify that rulebook and backend works properly
    {
        // Also testing hashes at the same time to verify uniqueness and repeatability
        [TestMethod]
        public void GameStatesInit() // To make sure step by step, player 1, player 2 until first draw without issues, proper init
        {
            HashSet<int> playerHashes = new HashSet<int>(); // Stores all player hashes
            HashSet<int> stateHashes = new HashSet<int>(); // Stores all states
            GameStateMachine sm = new GameStateMachine();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.START); // Ensure start in start state
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer("p1");
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer("p2");
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            // Initial hashes of players and whole game
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            // Now only P1 should've changed
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            // Now only P2 should've changed
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
            // Now assert states of players
            AuxStateVerify.VerifyPlayerInitialised(sm.GetDetailedState().PlayerStates[0]);
            Assert.IsTrue(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
            AuxStateVerify.VerifyPlayerInitialised(sm.GetDetailedState().PlayerStates[1]);
            Assert.IsTrue(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            // Check hashes already present
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, true); // Game state should exist!!
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT);
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.OMNISCIENT);
            Assert.IsFalse(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            sm.UndoPreviousStep();
            // Check hashes already present
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
            AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, true); // Game state should exist!!
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
            Assert.IsFalse(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
            sm.UndoPreviousStep(); // Should stop going back here
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
            // Total 4 hashes, 2 for uninitialized and 2 for initialized
            Assert.AreEqual(playerHashes.Count, 4);
            Assert.AreEqual(stateHashes.Count, 3); // 3 states at different initialization stages
        }
        [TestMethod]
        public void GameStatesLoadPlayers() // Loads from P1, does init but loading a game state, does whole test procedure as test#1
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                HashSet<int> playerHashes = new HashSet<int>(); // Stores all player hashes
                HashSet<int> stateHashes = new HashSet<int>(); // Stores all state hashes
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(InitialStatesGenerator.GetInitialPlayerState(id, (int)DateTime.Now.Ticks)); // Don't care about seed in this test
                // First hashes
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
                if (id == CurrentPlayer.PLAYER_1)
                {
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
                    sm.Step();
                    // Only p1 hash should've changed
                    AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
                    AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
                    AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
                }
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
                sm.Step();
                // Only p2 hash should've changed
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
                Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
                // Now the undo
                sm.UndoPreviousStep(); // Goes back to P2 Init
                // Both hashes unchanged
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, true);
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now game should be started
                Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.OMNISCIENT); // And P1 should be reverted
                sm.UndoPreviousStep();
                // Both hashes unchanged
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
                AuxStateVerify.HashSetVerification(sm.GetDetailedState(), stateHashes, true);
                if (id == CurrentPlayer.PLAYER_1)
                {
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
                    sm.UndoPreviousStep(); // Should stop going back here
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
                    Assert.AreEqual(playerHashes.Count, 4); // 4 hashes, 2 init, 2 uninit
                    Assert.AreEqual(stateHashes.Count, 3);
                }
                else
                {
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Should stop going back here
                    Assert.AreEqual(playerHashes.Count, 3); // 3 hashes, only p2 changed this time
                    Assert.AreEqual(stateHashes.Count, 2);

                }
            }
        }
        [TestMethod]
        public void TestDeterminismInit() // Start, check seed initial, move forward, then move back, and then forward again. Seeds should remain 100% same
        {
            int p1Seed, p2Seed, drawSeed;
            GameStateMachine sm = new GameStateMachine();
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer("p1");
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer("p2");
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            p1Seed = sm.GetDetailedState().Seed;
            sm.Step();
            p2Seed = sm.GetDetailedState().Seed;
            sm.Step();
            drawSeed = sm.GetDetailedState().Seed;
            // Good, now I go back and check that seeds are correct
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(p2Seed, sm.GetDetailedState().Seed);
            sm.UndoPreviousStep(); // Back to P1
            Assert.AreEqual(p1Seed, sm.GetDetailedState().Seed);
            // Ok now I go forward twice again and it should generate same exact seed
            sm.Step();
            Assert.AreEqual(p2Seed, sm.GetDetailedState().Seed);
            sm.Step();
            Assert.AreEqual(drawSeed, sm.GetDetailedState().Seed);
        }
        [TestMethod]
        public void TestDeterminismLoadPlayers() // Same as above, but starting from a chosen manual seed
        {
            const int p1Seed = 24601;
            const int p2Seed = -1572819080;
            const int drawSeed = 1304835662;
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                // Seeds pre-loaded already
                GameStateMachine sm = new GameStateMachine();
                int seed = (id == CurrentPlayer.PLAYER_2) ? p2Seed : p1Seed;
                sm.LoadGame(InitialStatesGenerator.GetInitialPlayerState(id, seed));
                if(id  == CurrentPlayer.PLAYER_1)
                {
                    sm.Step();
                }
                Assert.AreEqual(p2Seed, sm.GetDetailedState().Seed);
                sm.Step();
                Assert.AreEqual(drawSeed, sm.GetDetailedState().Seed);
                // Good, now I go back and check that seeds are correct
                sm.UndoPreviousStep(); // Goes back to P2 Init
                Assert.AreEqual(p2Seed, sm.GetDetailedState().Seed);
                if(id == CurrentPlayer.PLAYER_1)
                {
                    sm.UndoPreviousStep(); // Back to P1
                    Assert.AreEqual(p1Seed, sm.GetDetailedState().Seed);
                }
            }
        }
        [TestMethod]
        public void TestDrawPhase()
        {
            // Test that draw phase occurs, draws cards and gold, then goes to action phase
            GameStateMachine sm = new GameStateMachine();
            sm.LoadGame(InitialStatesGenerator.GetInitialPlayerState(CurrentPlayer.PLAYER_2, (int)DateTime.Now.Ticks));
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT);// In P2 init phase
            // Now advance step and check data pre draw
            sm.Step();
            AuxStateVerify.VerifyDrawPhaseResult(sm); // Checks the draw step passes well
            // Quick trick, from here do the same but from P2 draw phase
            GameStateStruct testState = sm.GetDetailedState();
            testState.CurrentPlayer = CurrentPlayer.PLAYER_2;
            sm = new GameStateMachine();
            sm.LoadGame(testState);
            AuxStateVerify.VerifyDrawPhaseResult(sm); // Checks the draw step passes well again
        }
        [TestMethod]
        public void BoardHashVerify() // Verify that an unchanged board has an unchanged hash
        {
            int playerIndex = 0;
            GameStateStruct state = new GameStateStruct
            {
                CurrentState = States.ACTION_PHASE,
                CurrentPlayer = CurrentPlayer.PLAYER_1
            };
            state.PlayerStates[playerIndex].Hand.InsertCard(-1011117); // Insert token card
            state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
            GameStateMachine sm = new GameStateMachine
            {
                CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
            };
            sm.LoadGame(state); // Start from here
            // HASH CHECK
            int emptyBoardHash = sm.GetDetailedState().BoardState.GetHash();
            int emptyBoardStateHash = sm.GetDetailedState().GetHash();
            sm.GetDetailedState().BoardState.GetUnitContainer(); // Will pretend im editing something although im not
            Assert.AreEqual(emptyBoardHash, sm.GetDetailedState().BoardState.GetHash()); // Hash would be recalculated but still the same
            Assert.AreEqual(emptyBoardStateHash, sm.GetDetailedState().GetHash()); // Hash would be recalculated but still the same
            // Will play card now
            Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-1011117, CardTargets.PLAINS); // Play it
            // Make sure card was played ok
            Assert.AreEqual(res.Item1, PlayOutcome.OK);
            Assert.IsNotNull(res.Item2);
            // And check hash again
            int boardWUnitHash = sm.GetDetailedState().BoardState.GetHash();
            int stateWUnitHash = sm.GetDetailedState().GetHash();
            Assert.AreNotEqual(emptyBoardHash, boardWUnitHash);
            Assert.AreNotEqual(emptyBoardStateHash, stateWUnitHash);
            sm.GetDetailedState().BoardState.GetUnitContainer(); // Will pretend im editing something although im not
            Assert.AreEqual(boardWUnitHash, sm.GetDetailedState().BoardState.GetHash()); // Hash would be recalculated but still the same
            Assert.AreEqual(stateWUnitHash, sm.GetDetailedState().GetHash()); // Hash would be recalculated but still the same
            // Modify unit (shady)
            sm.GetDetailedState().BoardState.GetUnitContainer()[0].Attack += 5; // Add 5 to attack, whatever
            Assert.AreNotEqual(boardWUnitHash, sm.GetDetailedState().BoardState.GetHash()); // But now the board hash should fail bc its a brand new unit (and therefore board)
            Assert.AreNotEqual(stateWUnitHash, sm.GetDetailedState().GetHash()); // But now the board hash should fail bc its a brand new unit (and therefore board)
            sm.UndoPreviousStep();
            Assert.AreEqual(emptyBoardHash, sm.GetDetailedState().BoardState.GetHash()); // Finally hash should've reverted and known
            Assert.AreEqual(emptyBoardStateHash, sm.GetDetailedState().GetHash()); // Finally hash should've reverted and known
        }
    }
    public static class AuxStateVerify // Aux functions to verify stuff
    {
        /// <summary>
        /// Checks if deck 1-30 is properly shuffled, has a 2.6525286e+32 chance of messing up because you may get a perfect shuffle
        /// </summary>
        /// <returns>If deck's shuffled</returns>
        public static bool IsDeckShuffled(PlayerState p)
        {
            for (int i = 0; i < p.Deck.DeckSize; i++)
            {
                if(p.Deck.PeepAt(i) != i + 1)
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
            Assert.AreEqual(p.Hp, GameConstants.STARTING_HP);
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
            GameStateStruct testState = sm.GetDetailedState();
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
            testState = sm.GetDetailedState();
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
            testState = sm.GetDetailedState();
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
        /// Checks if player state has is already present or not in a set
        /// </summary>
        /// <param name="st">State of player</param>
        /// <param name="set">Hash set</param>
        /// <param name="shouldBe">Should it be present?</param>
        public static void HashSetVerification(IHashable st, HashSet<int> set, bool shouldBe)
        {
            Assert.IsTrue(shouldBe == set.Contains(st.GetHash()));
            if(!set.Contains(st.GetHash()))
            {
                set.Add(st.GetHash());
            }
        }
    }
    public static class InitialStatesGenerator // Generates a game state for test
    {
        /// <summary>
        /// Creates a blank INIT_P state for test
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="seed">Seed</param>
        /// <returns></returns>
        public static GameStateStruct GetInitialPlayerState(CurrentPlayer p, int seed) /// Returns a game state consisting of initialization of a desired player
        {
            GameStateStruct ret = new GameStateStruct();
            List<int> decc = new List<int>();
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                decc.Add(i);
            }
            ret.Seed = seed;
            ret.PlayerStates[0].Name = "p1";
            ret.PlayerStates[0].Deck.InitializeDeck(decc);
            ret.PlayerStates[0].Name = "p2";
            ret.PlayerStates[1].Deck.InitializeDeck(decc);
            ret.CurrentState = p switch
            {
                CurrentPlayer.PLAYER_1 => States.P1_INIT,
                CurrentPlayer.PLAYER_2 => States.P2_INIT,
                _ => States.START,
            };
            return ret;
        }
        /// <summary>
        /// Creates a brand new dummy player with a 30-card test deck
        /// </summary>
        /// <returns></returns>
        public static PlayerInitialData GetDummyPlayer(string name)
        {
            PlayerInitialData ret = new PlayerInitialData()
            {
                Name = name
            };
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                ret.InitialDecklist.Add(i);
            }
            return ret;
        }
    }
}
