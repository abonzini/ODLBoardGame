using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EngineTests
{
    [TestClass]
    public class GameEngineTests // For debugging and control, verify that rulebook and backend works properly
    {
        [TestMethod]
        public void GameStatesInitForward() // To make sure step by step, player 1, player 2 until first draw without issues, proper init
        {
            GameStateMachine sm = new GameStateMachine();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.START); // Ensure start in start state
            PlayerInitialData dummyPlayer1 = AuxStateVerify.GetDummyPlayer();
            PlayerInitialData dummyPlayer2 = AuxStateVerify.GetDummyPlayer();
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.PLAYER_1); // And P1 should be active
            // Now assert states of players
            Assert.IsTrue(AuxStateVerify.IsPlayerInitialised(sm.GetDetailedState().PlayerStates[0]));
            Assert.IsTrue(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
            Assert.IsTrue(AuxStateVerify.IsPlayerInitialised(sm.GetDetailedState().PlayerStates[1]));
            Assert.IsTrue(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.OMNISCIENT); // And P1 should be active
            Assert.IsFalse(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            sm.UndoPreviousStep();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
            Assert.IsFalse(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
            sm.UndoPreviousStep(); // Should stop going back here
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
        }
        [TestMethod]
        public void GameStatesLoadForwardP1() // Loads from P1, does init but loading a game state, does whole test procedure as test#1
        {
            GameStateMachine sm = new GameStateMachine();
            sm.LoadGame(AuxStateVerify.GetInitialPlayerState(PlayerId.PLAYER_1, (int)DateTime.Now.Ticks)); // Don't care about seed in this test
            // Same as before...
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.PLAYER_1); // And P1 should be active
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.OMNISCIENT); // And P1 should be active
            sm.UndoPreviousStep();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
            sm.UndoPreviousStep(); // Should stop going back here
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
        }
        [TestMethod]
        public void GameStatesLoadForwardP2() // Loads from P2, does init but loading a game state, does whole test procedure as test#1
        {
            GameStateMachine sm = new GameStateMachine();
            sm.LoadGame(AuxStateVerify.GetInitialPlayerState(PlayerId.PLAYER_2, (int)DateTime.Now.Ticks)); // Don't care about seed in this test
            // Same as before...
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.PLAYER_1); // And P1 should be active
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.OMNISCIENT); // And P1 should be active
            sm.UndoPreviousStep(); // Should stop going back here
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT);
        }
        [TestMethod]
        public void TestDeterminismInit() // Start, check seed initial, move forward, then move back, and then forward again. Seeds should remain 100% same
        {
            int p1Seed, p2Seed, drawSeed;
            GameStateMachine sm = new GameStateMachine();
            PlayerInitialData dummyPlayer1 = AuxStateVerify.GetDummyPlayer();
            PlayerInitialData dummyPlayer2 = AuxStateVerify.GetDummyPlayer();
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
        public void TestDeterminismLoadP1() // Same as above, but starting from a chosen manual seed
        {
            int p1Seed = 24601;
            int p2Seed = -1572819080;
            int drawSeed = 1304835662;
            // Seeds pre-loaded already
            GameStateMachine sm = new GameStateMachine();
            sm.LoadGame(AuxStateVerify.GetInitialPlayerState(PlayerId.PLAYER_1, p1Seed));
            sm.Step();
            Assert.AreEqual(p2Seed, sm.GetDetailedState().Seed);
            sm.Step();
            Assert.AreEqual(drawSeed, sm.GetDetailedState().Seed);
            // Good, now I go back and check that seeds are correct
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(p2Seed, sm.GetDetailedState().Seed);
            sm.UndoPreviousStep(); // Back to P1
            Assert.AreEqual(p1Seed, sm.GetDetailedState().Seed);
        }
        [TestMethod]
        public void TestDeterminismLoadP2() // Same as above, but P2, determinism should be the exact same
        {
            int p2Seed = -1572819080;
            int drawSeed = 1304835662;
            // Seeds pre-loaded already
            GameStateMachine sm = new GameStateMachine();
            sm.LoadGame(AuxStateVerify.GetInitialPlayerState(PlayerId.PLAYER_2, p2Seed));
            sm.Step();
            Assert.AreEqual(drawSeed, sm.GetDetailedState().Seed);
            // Good, now I go back and check that seeds are correct
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(p2Seed, sm.GetDetailedState().Seed);
        }
    }
    public static class AuxStateVerify // Aux functions to verify stuff and return initial cases
    {
        /// <summary>
        /// Creates a brand new dummy player with a 30-card test deck
        /// </summary>
        /// <returns></returns>
        public static PlayerInitialData GetDummyPlayer()
        {
            PlayerInitialData ret = new PlayerInitialData();
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                ret.InitialDecklist.Add(i);
            }
            return ret;
        }
        /// <summary>
        /// Checks if deck 1-30 is properly shuffled, has a 2.6525286e+32 chance of messing up because you may get a perfect shuffle
        /// </summary>
        /// <returns>If deck's shuffled</returns>
        public static bool IsDeckShuffled(PlayerState p)
        {
            for (int i = 0; i < p.Deck.DeckSize; i++)
            {
                if(p.Deck.Cards[i] != i + 1)
                {
                    return true; // A single difference is all it takes
                }
            }
            return false;
        }
        public static bool IsPlayerInitialised(PlayerState p)
        {
            bool playerIsInit = true;
            playerIsInit = p.Hp == GameConstants.STARTING_HP;
            playerIsInit = p.Gold == GameConstants.STARTING_GOLD;
            playerIsInit = p.Hand.HandSize == GameConstants.STARTING_CARDS;
            return playerIsInit;
        }

        public static GameStateStruct GetInitialPlayerState(PlayerId p, int seed) /// Returns a game state consisting of initialization of a desired player
        {
            GameStateStruct ret = new GameStateStruct();
            List<int> decc = new List<int>();
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                decc.Add(i);
            }
            ret.Seed = seed;
            ret.PlayerStates[0].Deck.InitializeDeck(decc);
            ret.PlayerStates[1].Deck.InitializeDeck(decc);
            ret.CurrentState = p switch
            {
                PlayerId.PLAYER_1 => States.P1_INIT,
                PlayerId.PLAYER_2 => States.P2_INIT,
                _ => States.START,
            };
            return ret;
        }
    }
}
