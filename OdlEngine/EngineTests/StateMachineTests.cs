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
            Player dummyPlayer = new Player();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.START); // Ensure start in start state
            sm.StartNewGame(dummyPlayer, dummyPlayer);
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P1_INIT); // Now should be about to init P1
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.P2_INIT); // Now should be about to init P2
            sm.Step();
            Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE); // Now game should be started
            Assert.AreEqual(sm.GetDetailedState().CurrentPlayer, PlayerId.PLAYER_1); // And P1 should be active
        }
    }
}
