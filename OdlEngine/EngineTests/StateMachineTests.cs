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
        public void NewGameStates() // To make sure step by step, player 1, player 2
        {
            GameStateMachine sm = new GameStateMachine();
            PlayerInitialData dummyPlayer1 = new PlayerInitialData();
            PlayerInitialData dummyPlayer2 = new PlayerInitialData();
            dummyPlayer1.InitialDecklist.Add(1); dummyPlayer1.InitialDecklist.Add(2); dummyPlayer1.InitialDecklist.Add(3);
            dummyPlayer2.InitialDecklist.Add(4); dummyPlayer2.InitialDecklist.Add(5); dummyPlayer2.InitialDecklist.Add(6);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.START); // Ensure start in start state
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.PLAYER_1); // And P1 should be active
        }
    }
}
