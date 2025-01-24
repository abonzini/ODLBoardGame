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
    public class StateMachineTests // For debugging and control
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
        public void GameStatesLoadForward() // Loads from P1, does init but loading a game state, does whole test procedure as test#1
        {

        }
        [TestMethod]
        public void TestDeterminismInit() // Start, check seed initial, move forward, then move back, and then forward again. Seeds should remain 100% same
        {

        }
        [TestMethod]
        public void TestDeterminismLoad() // Same as above, but starting from a chosen manual seed
        {
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
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                if(p.Deck.Cards[i-1] != i)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsPlayerInitialised(PlayerState p)
        {
            bool playerIsInit = true;
            playerIsInit = p.Hp == GameConstants.STARTING_HP;
            playerIsInit = p.Gold == GameConstants.STARTING_GOLD;
            playerIsInit = p.Hand.CardsInHand.Count == GameConstants.STARTING_CARDS;
            return playerIsInit;
        }
    }
}
