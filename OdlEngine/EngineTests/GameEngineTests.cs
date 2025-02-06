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
        [TestMethod]
        public void GameStatesInit() // To make sure step by step, player 1, player 2 until first draw without issues, proper init
        {
            GameStateMachine sm = new GameStateMachine();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.START); // Ensure start in start state
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer();
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer();
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
            // Now assert states of players
            AuxStateVerify.VerifyPlayerInitialised(sm.GetDetailedState().PlayerStates[0]);
            Assert.IsTrue(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
            AuxStateVerify.VerifyPlayerInitialised(sm.GetDetailedState().PlayerStates[1]);
            Assert.IsTrue(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            // Now the undo
            sm.UndoPreviousStep(); // Goes back to P2 Init
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.OMNISCIENT); // And P1 should be active
            Assert.IsFalse(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[1]));
            sm.UndoPreviousStep();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
            Assert.IsFalse(AuxStateVerify.IsDeckShuffled(sm.GetDetailedState().PlayerStates[0]));
            sm.UndoPreviousStep(); // Should stop going back here
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
        }
        [TestMethod]
        public void GameStatesLoadPlayers() // Loads from P1, does init but loading a game state, does whole test procedure as test#1
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(InitialStatesGenerator.GetInitialPlayerState(id, (int)DateTime.Now.Ticks)); // Don't care about seed in this test
                if(id == CurrentPlayer.PLAYER_1)
                {
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
                    sm.Step();
                }
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
                sm.Step();
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
                Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.PLAYER_1); // And P1 should be active
                // Now the undo
                sm.UndoPreviousStep(); // Goes back to P2 Init
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now game should be started
                Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, CurrentPlayer.OMNISCIENT); // And P1 should be reverted
                sm.UndoPreviousStep();
                if (id == CurrentPlayer.PLAYER_1)
                {
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
                    sm.UndoPreviousStep(); // Should stop going back here
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT);
                }
                else
                {
                    Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Should stop going back here
                }
            }
        }
        [TestMethod]
        public void TestDeterminismInit() // Start, check seed initial, move forward, then move back, and then forward again. Seeds should remain 100% same
        {
            int p1Seed, p2Seed, drawSeed;
            GameStateMachine sm = new GameStateMachine();
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer();
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer();
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
                if(p.Deck.Cards[i] != i + 1)
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
            Assert.AreEqual(p.Hand.HandSize, GameConstants.STARTING_CARDS);
            Assert.AreEqual(p.Deck.DeckSize, GameConstants.DECK_SIZE - GameConstants.STARTING_CARDS);
        }
        /// <summary>
        /// Verifies from a state machine in draw phase, that draw phase succeeds properly. Leaves the state machine as it began
        /// </summary>
        /// <param name="sm">State machine to try</param>
        public static void VerifyDrawPhaseResult(GameStateMachine sm)
        {
            GameStateStruct testState = sm.GetDetailedState();
            Assert.AreEqual(testState.CurrentState, States.DRAW_PHASE); // Am I in draw phase
            int preCards = testState.PlayerStates[(int)testState.CurrentPlayer].Hand.HandSize;
            int preGold = testState.PlayerStates[(int)testState.CurrentPlayer].Gold;
            int preDeck = testState.PlayerStates[(int)testState.CurrentPlayer].Deck.DeckSize;
            // Now draw!
            sm.Step();
            testState = sm.GetDetailedState();
            int postCards = testState.PlayerStates[(int)testState.CurrentPlayer].Hand.HandSize;
            int postGold = testState.PlayerStates[(int)testState.CurrentPlayer].Gold;
            int postDeck = testState.PlayerStates[(int)testState.CurrentPlayer].Deck.DeckSize;
            Assert.AreEqual(testState.CurrentState, States.ACTION_PHASE); // Am I in next phase
            Assert.AreEqual(postCards - preCards, GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player draw exact amount of cards
            Assert.AreEqual(postGold - preGold, GameConstants.DRAW_PHASE_GOLD_OBTAINED); // Did player gain exact amount of gold
            Assert.AreEqual(postDeck - preDeck, -GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player deck shrink the exact amount
            // Now revert
            sm.UndoPreviousStep(); // Go back to beginning of drawphase
            testState = sm.GetDetailedState();
            preCards = testState.PlayerStates[(int)testState.CurrentPlayer].Hand.HandSize;
            preGold = testState.PlayerStates[(int)testState.CurrentPlayer].Gold;
            preDeck = testState.PlayerStates[(int)testState.CurrentPlayer].Deck.DeckSize;
            Assert.AreEqual(testState.CurrentState, States.DRAW_PHASE); // Am I in draw phase again
            Assert.AreEqual(postCards - preCards, GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player restore cards
            Assert.AreEqual(postGold - preGold, GameConstants.DRAW_PHASE_GOLD_OBTAINED); // Did player restore gold
            Assert.AreEqual(postDeck - preDeck, -GameConstants.DRAW_PHASE_CARDS_DRAWN); // Did player deck recover the card
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
            ret.PlayerStates[0].Deck.InitializeDeck(decc);
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
        public static PlayerInitialData GetDummyPlayer()
        {
            PlayerInitialData ret = new PlayerInitialData();
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                ret.InitialDecklist.Add(i);
            }
            return ret;
        }
    }
}
