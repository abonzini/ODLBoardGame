using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    [TestClass]
    public class UnitAndBattleTests
    {
        [TestMethod]
        public void UnitInstantiatedCorrectly() // Checks if unit is played and appears in lists normally
        {
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117, i); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine();
                sm.CardDb = TestCardGenerator.GenerateTestCardGenerator(); // Add test cardDb
                sm.LoadGame(state); // Start from here
                // Will play a random card (they all cost 0)
                int cardPlayed = _rng.Next(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize);
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Also choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(cardPlayed, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure now there's a unit in the list
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits[playerIndex].Count, 1); // Player now has 1 unit summoned
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize, 9); // Player spent a card
                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits[playerIndex].Count, 0); // Player now has 1 unit summoned
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize, 10); // Player spent a card
            }
        }
        [TestMethod]
        public void UnitsAreIndependent() // Manually modify a unit, and check if next unit s independent of modified
        // THIS IS A SANITY TEST not actual gameplay that would happen as it
        {
            Assert.Fail();
            /*
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117, i); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine();
                sm.CardDb = TestCardGenerator.GenerateTestCardGenerator(); // Add test cardDb
                sm.LoadGame(state); // Start from here
                // Will play a random card (they all cost 0)
                int cardPlayed = _rng.Next(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize);
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Also choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(cardPlayed, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure now there's a unit in the list
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits[playerIndex].Count, 1); // Player now has 1 unit summoned
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize, 9); // Player spent a card
                // Modify unit (shady)
                sm.GetDetailedState().BoardState.PlayerUnits[playerIndex][0].Attack += 5; // Add 5 to attack, whatever
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.GLOBAL) ? chosenTarget : CardTargets.MOUNTAIN;
                // Play new one
                res = sm.PlayCard(cardPlayed, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits[playerIndex].Count, 2); // Player now has 2 units summoned
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize, 8); // Player spent a card
                // Finally check they're 
                
            }
            */
        }
        // TODO: Create more tests as unit behaviour improves. When unique id is implemented, check that stats and id are different (in above test)
        // Then, check Coordinate too, and eventually battle tests
    }
}
