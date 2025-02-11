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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117, i); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                // Will play a random card (they all cost 0)
                int cardPlayed = _rng.Next(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize);
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Also choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(cardPlayed, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure now there's a unit in the list
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 1); // Player now has 1 unit summoned
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits.Count, 1); // Check also back end for now

                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 0); // Reverted
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits.Count, 0);
            }
        }
        [TestMethod]
        public void UnitsAreIndependent() // Manually modify a unit, and check if next unit s independent of modified
        // THIS IS A SANITY TEST not actual gameplay that would happen like this
        {
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117, i); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                // Will play a random card (they all cost 0)
                int cardPlayed = _rng.Next(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize);
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Also choose a random lane as target
                int unitCounter1 = sm.GetDetailedState().PlaceableTotalCount;
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(cardPlayed, chosenTarget); // Play it
                int unitCounter2 = sm.GetDetailedState().PlaceableTotalCount;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure now there's a unit in the list
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 1); // Player now has 1 unit summoned
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits.Count, 1); // Also check back end, list of units content (may be different in real gameplay due to mechanics)
                // Modify unit (shady)
                sm.GetDetailedState().BoardState.PlayerUnits[0].Attack += 5; // Add 5 to attack, whatever
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.GLOBAL) ? chosenTarget : CardTargets.MOUNTAIN;
                // Play new one
                cardPlayed = _rng.Next(sm.GetDetailedState().PlayerStates[playerIndex].Hand.HandSize);
                res = sm.PlayCard(cardPlayed, chosenTarget); // Play it
                int futureUnitCounter = sm.GetDetailedState().PlaceableTotalCount;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 2); // Player now has 2 units summoned
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits.Count, 2); // Also check back end, list of units content (may be different in real gameplay due to mechanics)
                // Check they're different
                Assert.AreNotEqual(unitCounter1, unitCounter2);
                Assert.AreEqual(futureUnitCounter - unitCounter2, 1); // have a diff of 1 too
                Assert.AreEqual(unitCounter2 - unitCounter1, 1); // have a diff of 1 too
                // Some stats should be similar, some should be different
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits[unitCounter2].Hp, sm.GetDetailedState().BoardState.PlayerUnits[unitCounter1].Hp);
                Assert.AreNotEqual(sm.GetDetailedState().BoardState.PlayerUnits[unitCounter2].Attack, sm.GetDetailedState().BoardState.PlayerUnits[unitCounter1].Attack);
                // Finally, roll back!
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter2, sm.GetDetailedState().PlaceableTotalCount); // And reverts properly
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 1);
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits.Count, 1);
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter1, sm.GetDetailedState().PlaceableTotalCount); // And reverts properly
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.PlayerUnits.Count, 0);
            }
        }
        // Then, check Coordinate too, and eventually battle tests
    }
}
