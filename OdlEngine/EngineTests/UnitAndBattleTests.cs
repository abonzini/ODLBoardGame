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
                int boardHash;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                boardHash = sm.GetDetailedState().BoardState.GetHash(); // Store hash
                // Will play one of them
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-1011117, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure now there's a unit in the list
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 1); // Player now has 1 unit summoned
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false).Count, 1); // Check also back end for now
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(chosenTarget).PlayerUnitCount[playerIndex], 1); // Lane also contains 1 unit
                // Check board hash has changed
                Assert.AreNotEqual(boardHash, sm.GetDetailedState().BoardState.GetHash());
                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 0); // Reverted
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false).Count, 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(chosenTarget).PlayerUnitCount[playerIndex], 0);
                // Check board hash has been properly reverted
                Assert.AreEqual(boardHash, sm.GetDetailedState().BoardState.GetHash());
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
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                // Will play one of them
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                int unitCounter1 = sm.GetDetailedState().NextUnitIndex;
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-1011117, chosenTarget); // Play it
                int unitCounter2 = sm.GetDetailedState().NextUnitIndex;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Modify unit (shady)
                sm.GetDetailedState().BoardState.GetUnitContainer()[0].Attack += 5; // Add 5 to attack, whatever
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.GLOBAL) ? chosenTarget : CardTargets.MOUNTAIN;
                // Play new one
                res = sm.PlayCard(-1011117, chosenTarget); // Play it
                int futureUnitCounter = sm.GetDetailedState().NextUnitIndex;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Check they're different
                Assert.AreNotEqual(unitCounter1, unitCounter2);
                Assert.AreEqual(futureUnitCounter - unitCounter2, 1); // have a diff of 1 too
                Assert.AreEqual(unitCounter2 - unitCounter1, 1); // have a diff of 1 too
                // Some stats should be similar, some should be different
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false)[unitCounter2].Hp, sm.GetDetailedState().BoardState.GetUnitContainer(false)[unitCounter1].Hp);
                Assert.AreNotEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false)[unitCounter2].Attack, sm.GetDetailedState().BoardState.GetUnitContainer(false)[unitCounter1].Attack);
                // Finally, roll back!
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter2, sm.GetDetailedState().NextUnitIndex); // And reverts properly
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 1);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false).Count, 1);
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter1, sm.GetDetailedState().NextUnitIndex); // And reverts properly
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false).Count, 0);
            }
        }
        [TestMethod]
        public void UnitsStartOnTheirSide()
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
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                bool plainsInit = false, forestInit = false, mountainInit = false;
                plainsInit |= chosenTarget == CardTargets.PLAINS;
                forestInit |= chosenTarget == CardTargets.FOREST;
                mountainInit |= chosenTarget == CardTargets.MOUNTAIN;
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-1011117, chosenTarget); // Play card in some lane
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                void verifyLaneStates(int playerIndex, bool plainsInit, bool forestInit, bool mountainInit)
                {
                    Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.PLAINS).PlayerUnitCount[playerIndex], plainsInit?1:0);
                    Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.FOREST).PlayerUnitCount[playerIndex], forestInit?1:0);
                    Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.MOUNTAIN).PlayerUnitCount[playerIndex], mountainInit?1:0);
                    if (playerIndex == 0)
                    {
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.PLAINS).GetTile(0).PlayerUnitCount[playerIndex], plainsInit ? 1 : 0);
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.FOREST).GetTile(0).PlayerUnitCount[playerIndex], forestInit ? 1 : 0);
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.MOUNTAIN).GetTile(0).PlayerUnitCount[playerIndex], mountainInit ? 1 : 0);
                    }
                    else
                    {
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.PLAINS).GetTile(GameConstants.PLAINS_TILES_NUMBER-1).PlayerUnitCount[playerIndex], plainsInit ? 1 : 0);
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.FOREST).GetTile(GameConstants.FOREST_TILES_NUMBER - 1).PlayerUnitCount[playerIndex], forestInit ? 1 : 0);
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(CardTargets.MOUNTAIN).GetTile(GameConstants.MOUNTAIN_TILES_NUMBER - 1).PlayerUnitCount[playerIndex], mountainInit ? 1 : 0);
                    }
                }
                // And check all lanes and tiles for both players
                verifyLaneStates(playerIndex, plainsInit, forestInit, mountainInit);
                // Change lane and play and verify
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.GLOBAL) ? chosenTarget : CardTargets.MOUNTAIN;
                plainsInit |= chosenTarget == CardTargets.PLAINS;
                forestInit |= chosenTarget == CardTargets.FOREST;
                mountainInit |= chosenTarget == CardTargets.MOUNTAIN;
                res = sm.PlayCard(-1011117, chosenTarget);
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                verifyLaneStates(playerIndex, plainsInit, forestInit, mountainInit);
                // Change lane and play and verify again!
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.GLOBAL) ? chosenTarget : CardTargets.MOUNTAIN;
                plainsInit |= chosenTarget == CardTargets.PLAINS;
                forestInit |= chosenTarget == CardTargets.FOREST;
                mountainInit |= chosenTarget == CardTargets.MOUNTAIN;
                res = sm.PlayCard(-1011117, chosenTarget);
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                verifyLaneStates(playerIndex, plainsInit, forestInit, mountainInit);
                // Finaly, revert all actions and should have 0 in all lanes again
                sm.UndoPreviousStep();
                sm.UndoPreviousStep();
                sm.UndoPreviousStep();
                verifyLaneStates(playerIndex, false, false, false);
            }
        }
        [TestMethod]
        public void UnitsOnBothSides()
        {
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                HashSet<int> boardHashes = new HashSet<int>();
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 5; i++)
                {
                    // Insert token cards, 1 in all stats, summonable in any lane 
                    // Insert to both players hands and decks
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1011117);
                    state.PlayerStates[playerIndex].Deck.InsertCard(-1011117);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(-1011117);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(-1011117);
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, false); // Also ensure hashes are unique for board here
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-1011117, chosenTarget); // Play card in some lane
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Check also for the lane, unit is in the correct place
                Lane lane = sm.GetDetailedState().BoardState.GetLane(chosenTarget);
                Assert.AreEqual(lane.PlayerUnitCount[playerIndex], 1); // Lane has 1 unit of player and 0 units of other one
                Assert.AreEqual(lane.PlayerUnitCount[otherPlayerIndex], 0);
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(playerIndex)).PlayerUnitCount[playerIndex], 1); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(otherPlayerIndex)).PlayerUnitCount[otherPlayerIndex], 0);
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(playerIndex)).PlayerUnitCount[otherPlayerIndex], 0); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(otherPlayerIndex)).PlayerUnitCount[playerIndex], 1);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, false); // Also ensure hashes are unique for board here
                // Ok! Now I end turn, do the draw phase
                sm.EndTurn();
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.DRAW_PHASE);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, true); // Hash should be here as board didn't change!
                sm.Step();
                Assert.AreEqual(sm.GetDetailedState().CurrentState, States.ACTION_PHASE);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, true); // Hash should be here as board didn't change!
                // Ok now other player plays card...
                res = sm.PlayCard(-1011117, chosenTarget); // Play card in exactly the same lane
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Check also for the lane, unit is in the correct place for the other player
                lane = sm.GetDetailedState().BoardState.GetLane(chosenTarget);
                Assert.AreEqual(lane.PlayerUnitCount[playerIndex], 1); // Lane has 1 unit of each now
                Assert.AreEqual(lane.PlayerUnitCount[otherPlayerIndex], 1);
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(playerIndex)).PlayerUnitCount[playerIndex], 1); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(otherPlayerIndex)).PlayerUnitCount[otherPlayerIndex], 1);
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(playerIndex)).PlayerUnitCount[otherPlayerIndex], 1); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(otherPlayerIndex)).PlayerUnitCount[playerIndex], 1);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, false); // Also ensure hashes are unique for board here
                // Finally do reversions
                // Revert playing and draw
                sm.UndoPreviousStep(); // Unplay p2
                lane = sm.GetDetailedState().BoardState.GetLane(chosenTarget);
                Assert.AreEqual(lane.PlayerUnitCount[playerIndex], 1); // Lane has 1 unit of player and 0 units of other one
                Assert.AreEqual(lane.PlayerUnitCount[otherPlayerIndex], 0);
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(playerIndex)).PlayerUnitCount[playerIndex], 1); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(otherPlayerIndex)).PlayerUnitCount[otherPlayerIndex], 0);
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(playerIndex)).PlayerUnitCount[otherPlayerIndex], 0); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(otherPlayerIndex)).PlayerUnitCount[playerIndex], 1);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, true); // Hash should be present
                sm.UndoPreviousStep(); // Undraw
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, true); // Hash should be present
                sm.UndoPreviousStep(); // Un-end p1 turn
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, true); // Hash should be present
                sm.UndoPreviousStep(); // Unplay p1
                lane = sm.GetDetailedState().BoardState.GetLane(chosenTarget);
                Assert.AreEqual(lane.PlayerUnitCount[playerIndex], 0); // Lane has 1 unit of player and 0 units of other one
                Assert.AreEqual(lane.PlayerUnitCount[otherPlayerIndex], 0);
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(playerIndex)).PlayerUnitCount[playerIndex], 0); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTile(lane.GetFirstTileCoord(otherPlayerIndex)).PlayerUnitCount[otherPlayerIndex], 0);
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(playerIndex)).PlayerUnitCount[otherPlayerIndex], 0); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTile(lane.GetLastTileCoord(otherPlayerIndex)).PlayerUnitCount[playerIndex], 0);
                TestHelperFunctions.HashSetVerification(sm.GetDetailedState().BoardState, boardHashes, true); // Hash should be present
            }
        }
        [TestMethod]
        public void SummonedUnitDiesIf0Hp()
        {
            // Summons dead unit and verifies that the unit properly died
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int boardHash;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1001117); // Insert token cards, 1 in all stats but 0 HP, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                boardHash = sm.GetDetailedState().BoardState.GetHash(); // Store hash
                // Will play one of them
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayCard(-1001117, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure unit has insta-died (nothing in field, 1 card in GY
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 0); // Player has no units
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false,true).Count, 0); // Field has no units
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false, false).Count, 1); // GY has 1 unit tho
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(chosenTarget).PlayerUnitCount[playerIndex], 0); // Lane doesn't have the unit
                // Check board hash has changed
                Assert.AreNotEqual(boardHash, sm.GetDetailedState().BoardState.GetHash());
                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NUnits, 0); // Player still has no units
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false, true).Count, 0); // And field has no units
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetUnitContainer(false, false).Count, 0); // GY has 0 units again tho
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(chosenTarget).PlayerUnitCount[playerIndex], 0); // Lane doesn't have the unit
                // Check board hash has been properly reverted
                Assert.AreEqual(boardHash, sm.GetDetailedState().BoardState.GetHash());
            }
        }
        [TestMethod]
        public void HashTest()
        {
            Unit u1, u2;
            u1 = new Unit()
            {
                UniqueId = 1,
                Card = 100,
                Owner = 0,
                LaneCoordinate = LaneID.PLAINS,
                TileCoordinate = 2,
                Hp = 10,
                Movement = 10,
                MovementDenominator = 10,
                Attack = 10,
                MvtCooldownTimer = 10
            };
            u2 = (Unit)u1.Clone();
            Assert.AreEqual(u1.GetHash(), u2.GetHash());
            // Now change a few things
            u2.TileCoordinate = 3;
            Assert.AreNotEqual(u1.GetHash(), u2.GetHash());
            // Revert
            u2.TileCoordinate = u1.TileCoordinate;
            Assert.AreEqual(u1.GetHash(), u2.GetHash());
        }
        //[TestMethod]
        //public void HashStressTest()
        //{
        //    HashSet<int> hashes = new HashSet<int>();
        //    float total = 0;
        //    float collisions = 0;
        //    Random _rng = new Random();
        //    for (int i=0; i<1000000; i++) // Test 1000000 times, create unique units and verify few collisions
        //    {
        //        Unit unit = new Unit()
        //        {
        //            UniqueId = _rng.Next(100),
        //            Card = _rng.Next(100),
        //            Owner = _rng.Next(3),
        //            LaneCoordinate = (LaneID)_rng.Next(1,4),
        //            TileCoordinate = _rng.Next(8),
        //            Hp = _rng.Next(11),
        //            Movement = _rng.Next(11),
        //            MovementDenominator = _rng.Next(11),
        //            Attack = _rng.Next(11),
        //            MvtCooldownTimer = _rng.Next(11)
        //        };
        //        total++;
        //        if(hashes.Contains(unit.GetHash()))
        //        {
        //            collisions++;
        //        }
        //        else
        //        {
        //            hashes.Add(unit.GetHash());
        //        }
        //    }
        //    Assert.IsTrue(collisions / total < 0.01); // Try for 1% or less of collisions
        //}
        // TODO when end of turn implemented add 2 simultaneously.
        // TODO eventually battle tests
    }
}
