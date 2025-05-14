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
            CardFinder cardFinder = new CardFinder();
            TestHelperFunctions.InjectBasePlayerToDb(cardFinder);
            GameStateMachine sm = new GameStateMachine(cardFinder);
            Assert.AreEqual(sm.DetailedState.CurrentState, States.START); // Ensure start in start state
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer("p1");
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer("p2");
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            // Initial hashes of players and whole game
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, false);
            Assert.AreEqual(sm.DetailedState.CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            // Now only P1 should've changed
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, false);
            Assert.AreEqual(sm.DetailedState.CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            // Now only P2 should've changed
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, false);
            Assert.AreEqual(sm.DetailedState.CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.DetailedState.CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
            // Now assert states of players
            TestHelperFunctions.VerifyPlayerInitialised(sm.DetailedState.PlayerStates[0]);
            Assert.IsTrue(TestHelperFunctions.IsDeckShuffled(sm.DetailedState.PlayerStates[0]));
            TestHelperFunctions.VerifyPlayerInitialised(sm.DetailedState.PlayerStates[1]);
            Assert.IsTrue(TestHelperFunctions.IsDeckShuffled(sm.DetailedState.PlayerStates[1]));
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            // Check hashes already present
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, true); // Game state should exist!!
            Assert.AreEqual(sm.DetailedState.CurrentState, States.P2_INIT);
            Assert.AreEqual(sm.DetailedState.CurrentPlayer, CurrentPlayer.OMNISCIENT);
            Assert.IsFalse(TestHelperFunctions.IsDeckShuffled(sm.DetailedState.PlayerStates[1]));
            sm.UndoPreviousStep();
            // Check hashes already present
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, true);
            TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, true); // Game state should exist!!
            Assert.AreEqual(sm.DetailedState.CurrentState, States.P1_INIT);
            Assert.IsFalse(TestHelperFunctions.IsDeckShuffled(sm.DetailedState.PlayerStates[0]));
            sm.UndoPreviousStep(); // Should stop going back here
            Assert.AreEqual(sm.DetailedState.CurrentState, States.P1_INIT);
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
                CardFinder cardFinder = new CardFinder();
                TestHelperFunctions.InjectBasePlayerToDb(cardFinder);
                GameStateMachine sm = new GameStateMachine(cardFinder);
                sm.LoadGame(InitialStatesGenerator.GetInitialPlayerState(id, (int)DateTime.Now.Ticks)); // Don't care about seed in this test
                // First hashes
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, false);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, false);
                TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, false);
                if (id == CurrentPlayer.PLAYER_1)
                {
                    Assert.AreEqual(sm.DetailedState.CurrentState, States.P1_INIT); // Now should be about to init P1
                    sm.Step();
                    // Only p1 hash should've changed
                    TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, false);
                    TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, true);
                    TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, false);
                }
                Assert.AreEqual(sm.DetailedState.CurrentState, States.P2_INIT); // Now should be about to init P2
                sm.Step();
                // Only p2 hash should've changed
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, false);
                TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, false);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.DRAW_PHASE); // Now game should be started
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
                // Now the undo
                sm.UndoPreviousStep(); // Goes back to P2 Init
                // Both hashes unchanged
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, true);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.P2_INIT); // Now game should be started
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, CurrentPlayer.OMNISCIENT); // And P1 should be reverted
                sm.UndoPreviousStep();
                // Both hashes unchanged
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, true);
                TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, true);
                if (id == CurrentPlayer.PLAYER_1)
                {
                    Assert.AreEqual(sm.DetailedState.CurrentState, States.P1_INIT);
                    sm.UndoPreviousStep(); // Should stop going back here
                    Assert.AreEqual(sm.DetailedState.CurrentState, States.P1_INIT);
                    Assert.AreEqual(playerHashes.Count, 4); // 4 hashes, 2 init, 2 uninit
                    Assert.AreEqual(stateHashes.Count, 3);
                }
                else
                {
                    Assert.AreEqual(sm.DetailedState.CurrentState, States.P2_INIT); // Should stop going back here
                    Assert.AreEqual(playerHashes.Count, 3); // 3 hashes, only p2 changed this time
                    Assert.AreEqual(stateHashes.Count, 2);

                }
            }
        }
        [TestMethod]
        public void TestDeterminismInit() // Start, check seed initial, move forward, then move back, and then forward again. Seeds should remain 100% same
        {
            int p1Seed, p2Seed, drawSeed;
            CardFinder cardFinder = new CardFinder();
            TestHelperFunctions.InjectBasePlayerToDb(cardFinder);
            GameStateMachine sm = new GameStateMachine(cardFinder);
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer("p1");
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer("p2");
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            p1Seed = sm.DetailedState.Seed;
            sm.Step();
            p2Seed = sm.DetailedState.Seed;
            sm.Step();
            drawSeed = sm.DetailedState.Seed;
            // Good, now I go back and check that seeds are correct
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(p2Seed, sm.DetailedState.Seed);
            sm.UndoPreviousStep(); // Back to P1
            Assert.AreEqual(p1Seed, sm.DetailedState.Seed);
            // Ok now I go forward twice again and it should generate same exact seed
            sm.Step();
            Assert.AreEqual(p2Seed, sm.DetailedState.Seed);
            sm.Step();
            Assert.AreEqual(drawSeed, sm.DetailedState.Seed);
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
                CardFinder cardFinder = new CardFinder();
                TestHelperFunctions.InjectBasePlayerToDb(cardFinder);
                GameStateMachine sm = new GameStateMachine(cardFinder);
                int seed = (id == CurrentPlayer.PLAYER_2) ? p2Seed : p1Seed;
                sm.LoadGame(InitialStatesGenerator.GetInitialPlayerState(id, seed));
                if(id  == CurrentPlayer.PLAYER_1)
                {
                    sm.Step();
                }
                Assert.AreEqual(p2Seed, sm.DetailedState.Seed);
                sm.Step();
                Assert.AreEqual(drawSeed, sm.DetailedState.Seed);
                // Good, now I go back and check that seeds are correct
                sm.UndoPreviousStep(); // Goes back to P2 Init
                Assert.AreEqual(p2Seed, sm.DetailedState.Seed);
                if(id == CurrentPlayer.PLAYER_1)
                {
                    sm.UndoPreviousStep(); // Back to P1
                    Assert.AreEqual(p1Seed, sm.DetailedState.Seed);
                }
            }
        }
        [TestMethod]
        public void TestDrawPhase()
        {
            // Test that draw phase occurs, draws cards and gold, then goes to action phase
            CardFinder cardFinder = new CardFinder();
            TestHelperFunctions.InjectBasePlayerToDb(cardFinder);
            GameStateMachine sm = new GameStateMachine(cardFinder);
            sm.LoadGame(InitialStatesGenerator.GetInitialPlayerState(CurrentPlayer.PLAYER_2, (int)DateTime.Now.Ticks));
            Assert.AreEqual(sm.DetailedState.CurrentState, States.P2_INIT);// In P2 init phase
            // Now advance step and check data pre draw
            sm.Step();
            TestHelperFunctions.VerifyDrawPhaseResult(sm); // Checks the draw step passes well
            // Quick trick, from here do the same but from P2 draw phase
            GameStateStruct testState = sm.DetailedState;
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
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                // Ensure all in order before EOT
                GameStateStruct preEotState = sm.DetailedState;
                int preEotHash = preEotState.GetHashCode(); // Keep hash
                Assert.AreEqual(preEotState.CurrentState, States.ACTION_PHASE); // in action phase
                Assert.AreEqual(preEotState.CurrentPlayer, id); // Current player
                // Now I activate end of turn!
                sm.EndTurn();
                GameStateStruct postEotState = sm.DetailedState;
                int postEotHash = preEotState.GetHashCode(); // Keep hash
                Assert.AreEqual(postEotState.CurrentState, States.DRAW_PHASE); // in draw phase now!
                Assert.AreEqual(postEotState.CurrentPlayer, (CurrentPlayer)otherPlayerIndex); // Current player has changed and will soon initialize drawing (draw phase already tested)
                Assert.AreNotEqual(preEotHash, postEotHash); // Hope hash are different (only because new current player)
                // Reversion should also apply
                sm.UndoPreviousStep();
                postEotState = sm.DetailedState;
                postEotHash = preEotState.GetHashCode();
                Assert.AreEqual(postEotState.CurrentState, States.ACTION_PHASE); // Back to action
                Assert.AreEqual(postEotState.CurrentPlayer, id); // Current player back to original
                Assert.AreEqual(preEotHash, postEotHash); // Hash repeatability check
            }
        }
        // Deck out tests
        [TestMethod]
        public void TheresNoDeckOut()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            Random _rng = new Random();
            foreach (CurrentPlayer id in ids)
            {
                int currentPlayer = (int)id;
                int playerHp = _rng.Next(GameConstants.DECKOUT_DAMAGE + 1, 31);
                Player pl1 = new Player();
                pl1.Hp.BaseValue = playerHp;
                Player pl2 = new Player();
                pl2.Hp.BaseValue = playerHp;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.DRAW_PHASE,
                    CurrentPlayer = id,
                    PlayerStates = [pl1, pl2]
                };
                state.PlayerStates[currentPlayer].Deck.InsertCard(-1); // Adds useless card to player deck
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 1);
                // Draw
                sm.Step();
                Assert.AreNotEqual(deckHash, plState.Deck.GetHashCode()); // Changed because card was drawn
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 1);
            }
        }
        [TestMethod]
        public void DeckOutDamage()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            Random _rng = new Random();
            foreach (CurrentPlayer id in ids)
            {
                int playerHp = _rng.Next(GameConstants.DECKOUT_DAMAGE + 1, 31);
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.DRAW_PHASE;
                state.CurrentPlayer = id;
                state.PlayerStates[0].Hp.BaseValue = playerHp;
                state.PlayerStates[1].Hp.BaseValue = playerHp;
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                // Draw
                sm.Step();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode()); // No change in deck contents
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
            }
        }
        [TestMethod]
        public void DeckOutKill()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.DRAW_PHASE;
                state.CurrentPlayer = id;
                state.PlayerStates[0].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE;
                state.PlayerStates[1].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE;
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.DRAW_PHASE);
                // Draw
                sm.Step();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode()); // No change in deck contents
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.DamageTokens, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, (CurrentPlayer)(1-(int)id)); // other player won
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.DRAW_PHASE);
            }
        }
        [TestMethod]
        public void DeckOutOverkill()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.DRAW_PHASE;
                state.CurrentPlayer = id;
                state.PlayerStates[0].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE - 1;
                state.PlayerStates[1].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE - 1;
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.DRAW_PHASE);
                // Draw
                sm.Step();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode()); // No change in deck contents
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.DamageTokens, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, (CurrentPlayer)(1 - (int)id)); // other player won
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.DRAW_PHASE);
            }
        }
        // Hash tests
        [TestMethod]
        public void BoardHashVerify() // Verify that an unchanged board has an unchanged hash
        {
            int playerIndex = 0;
            GameStateStruct state = TestHelperFunctions.GetBlankGameState();
            state.CurrentState = States.ACTION_PHASE;
            state.CurrentPlayer = CurrentPlayer.PLAYER_1;
            CardFinder cardDb = new CardFinder();
            // Card 1: basic unit
            cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1));
            state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert token card
            state.PlayerStates[playerIndex].CurrentGold = 4; // Set gold to 4
            GameStateMachine sm = new GameStateMachine(cardDb);
            sm.LoadGame(state); // Start from here
            // HASH CHECK
            int emptyBoardHash = sm.DetailedState.BoardState.GetHashCode();
            int emptyBoardStateHash = sm.DetailedState.GetHashCode();
            Assert.AreEqual(emptyBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Hash would be recalculated but still the same
            Assert.AreEqual(emptyBoardStateHash, sm.DetailedState.GetHashCode()); // Hash would be recalculated but still the same
            // Will play card now
            Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.PLAINS); // Play it
            // Make sure card was played ok
            Assert.AreEqual(res.Item1, PlayOutcome.OK);
            Assert.IsNotNull(res.Item2);
            // And check hash again
            int boardWUnitHash = sm.DetailedState.BoardState.GetHashCode();
            int stateWUnitHash = sm.DetailedState.GetHashCode();
            Assert.AreNotEqual(emptyBoardHash, boardWUnitHash);
            Assert.AreNotEqual(emptyBoardStateHash, stateWUnitHash);
            Assert.AreEqual(boardWUnitHash, sm.DetailedState.BoardState.GetHashCode()); // Hash would be recalculated but still the same
            Assert.AreEqual(stateWUnitHash, sm.DetailedState.GetHashCode()); // Hash would be recalculated but still the same
            // Modify unit (shady)
            int unitIndex = sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).First();
            ((Unit)sm.DetailedState.EntityData[unitIndex]).Attack.BaseValue += 5; // Add 5 to attack, whatever
            Assert.AreEqual(boardWUnitHash, sm.DetailedState.BoardState.GetHashCode()); // Board is 100% positional so this hash should remain the same
            Assert.AreNotEqual(stateWUnitHash, sm.DetailedState.GetHashCode()); // But now the state changed because unit data is different
            sm.UndoPreviousStep();
            Assert.AreEqual(emptyBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Finally hash should've reverted and known
            Assert.AreEqual(emptyBoardStateHash, sm.DetailedState.GetHashCode()); // Finally hash should've reverted and known
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
            GameStateStruct state = TestHelperFunctions.GetBlankGameState();
            List<int> decc = new List<int>();
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                decc.Add(i);
            }
            state.Seed = seed;
            state.PlayerStates[0].Name = "p1";
            state.PlayerStates[0].Deck.InitializeDeck(decc);
            state.PlayerStates[0].Name = "p2";
            state.PlayerStates[1].Deck.InitializeDeck(decc);
            state.CurrentState = p switch
            {
                CurrentPlayer.PLAYER_1 => States.P1_INIT,
                CurrentPlayer.PLAYER_2 => States.P2_INIT,
                _ => States.START,
            };
            return state;
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
