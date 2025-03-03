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
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            // Now only P1 should've changed
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            // Now only P2 should've changed
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
            // Now assert states of players
            TestHelperFunctions.VerifyPlayerInitialised(sm.GetDetailedState().PlayerStates[0]);
            Assert.IsTrue(TestHelperFunctions.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
            TestHelperFunctions.VerifyPlayerInitialised(sm.GetDetailedState().PlayerStates[1]);
            Assert.IsTrue(TestHelperFunctions.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            // Check hashes already present
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, true); // Game state should exist!!
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT);
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.OMNISCIENT);
            Assert.IsFalse(TestHelperFunctions.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            sm.UndoPreviousStep();
            // Check hashes already present
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, true); // Game state should exist!!
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
            Assert.IsFalse(TestHelperFunctions.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
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
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
                if (id == CurrentPlayer.PLAYER_1)
                {
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
                    sm.Step();
                    // Only p1 hash should've changed
                    TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, false);
                    TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
                    TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
                }
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
                sm.Step();
                // Only p2 hash should've changed
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, false);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, false);
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
                Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
                // Now the undo
                sm.UndoPreviousStep(); // Goes back to P2 Init
                // Both hashes unchanged
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, true);
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now game should be started
                Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.OMNISCIENT); // And P1 should be reverted
                sm.UndoPreviousStep();
                // Both hashes unchanged
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[0], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().PlayerStates[1], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState(), stateHashes, true);
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
            TestHelperFunctions.VerifyDrawPhaseResult(sm); // Checks the draw step passes well
            // Quick trick, from here do the same but from P2 draw phase
            GameStateStruct testState = sm.GetDetailedState();
            testState.CurrentPlayer = CurrentPlayer.PLAYER_2;
            sm = new GameStateMachine();
            sm.LoadGame(testState);
            TestHelperFunctions.VerifyDrawPhaseResult(sm); // Checks the draw step passes well again
        }
        [TestMethod]
        public void EndOfTurnTest() // Verify that an unchanged board has an unchanged hash
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                int playerIndex = (int)id;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = id
                };
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                // Ensure all in order before EOT
                GameStateStruct preEotState = sm.GetDetailedState();
                int preEotHash = preEotState.GetHash(); // Keep hash
                Assert.AreEqual(preEotState.CurrentState, States.ACTION_PHASE); // in action phase
                Assert.AreEqual(preEotState.CurrentPlayer, id); // Current player
                // Now I activate end of turn!
                sm.EndTurn();
                GameStateStruct postEotState = sm.GetDetailedState();
                int postEotHash = preEotState.GetHash(); // Keep hash
                Assert.AreEqual(postEotState.CurrentState, States.DRAW_PHASE); // in draw phase now!
                Assert.AreEqual(postEotState.CurrentPlayer, (CurrentPlayer)otherPlayerIndex); // Current player has changed and will soon initialize drawing (draw phase already tested)
                Assert.AreNotEqual(preEotHash, postEotHash); // Hope hash are different (only because new current player)
                // Reversion should also apply
                sm.UndoPreviousStep();
                postEotState = sm.GetDetailedState();
                postEotHash = preEotState.GetHash();
                Assert.AreEqual(postEotState.CurrentState, States.ACTION_PHASE); // Back to action
                Assert.AreEqual(postEotState.CurrentPlayer, id); // Current player back to original
                Assert.AreEqual(preEotHash, postEotHash); // Hash repeatability check
            }
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
            Assert.AreEqual(boardWUnitHash, sm.GetDetailedState().BoardState.GetHash()); // Hash would be recalculated but still the same
            Assert.AreEqual(stateWUnitHash, sm.GetDetailedState().GetHash()); // Hash would be recalculated but still the same
            // Modify unit (shady)
            sm.GetDetailedState().BoardState.Units[0].Attack += 5; // Add 5 to attack, whatever
            Assert.AreNotEqual(boardWUnitHash, sm.GetDetailedState().BoardState.GetHash()); // But now the board hash should fail bc its a brand new unit (and therefore board)
            Assert.AreNotEqual(stateWUnitHash, sm.GetDetailedState().GetHash()); // But now the board hash should fail bc its a brand new unit (and therefore board)
            sm.UndoPreviousStep();
            Assert.AreEqual(emptyBoardHash, sm.GetDetailedState().BoardState.GetHash()); // Finally hash should've reverted and known
            Assert.AreEqual(emptyBoardStateHash, sm.GetDetailedState().GetHash()); // Finally hash should've reverted and known
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
