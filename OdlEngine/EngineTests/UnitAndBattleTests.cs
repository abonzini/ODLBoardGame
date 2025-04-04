using ODLGameEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    // UNIT INSTANTIATION TESTS
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
                CardFinder cardDb = new CardFinder();
                // Card 1: test unit with 1 in all stats, summonable anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 1, 1, 1));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                boardHash = sm.DetailedState.BoardState.GetGameStateHash(); // Store hash
                // Will play one of them
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure now there's a unit in the list
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1); // Player now has 1 unit summoned
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 1); // Check also back end for now
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(chosenTarget).PlayerUnits[playerIndex].Count, 1); // Lane also contains 1 unit
                // Check board hash has changed
                Assert.AreNotEqual(boardHash, sm.DetailedState.BoardState.GetGameStateHash());
                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 0); // Reverted
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(chosenTarget).PlayerUnits[playerIndex].Count, 0);
                // Check board hash has been properly reverted
                Assert.AreEqual(boardHash, sm.DetailedState.BoardState.GetGameStateHash());
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
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 1, 1, 1));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Will play one of them
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                int unitCounter1 = sm.DetailedState.NextUniqueIndex;
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play it
                int unitCounter2 = sm.DetailedState.NextUniqueIndex;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Modify unit (shady)
                ((Unit)sm.GetBoardEntity(0)).Attack += 5; // Add 5 to attack, whatever
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.BOARD) ? chosenTarget : CardTargets.MOUNTAIN;
                // Play new one
                res = sm.PlayFromHand(1, chosenTarget); // Play it
                int futureUnitCounter = sm.DetailedState.NextUniqueIndex;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Check they're different
                Assert.AreNotEqual(unitCounter1, unitCounter2);
                Assert.AreEqual(futureUnitCounter - unitCounter2, 1); // have a diff of 1 too
                Assert.AreEqual(unitCounter2 - unitCounter1, 1); // have a diff of 1 too
                // Some stats should be similar, some should be different
                Assert.AreEqual(sm.GetBoardEntity(unitCounter2).Hp, sm.GetBoardEntity(unitCounter1).Hp);
                Assert.AreNotEqual(((Unit)sm.GetBoardEntity(unitCounter2)).Attack, ((Unit)sm.GetBoardEntity(unitCounter1)).Attack);
                // Finally, roll back!
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter2, sm.DetailedState.NextUniqueIndex); // And reverts properly
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 1);
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter1, sm.DetailedState.NextUniqueIndex); // And reverts properly
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 0);
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
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 1, 1, 1));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                bool plainsInit = false, forestInit = false, mountainInit = false;
                plainsInit |= chosenTarget == CardTargets.PLAINS;
                forestInit |= chosenTarget == CardTargets.FOREST;
                mountainInit |= chosenTarget == CardTargets.MOUNTAIN;
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play card in some lane
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                void verifyLaneStates(int playerIndex, bool plainsInit, bool forestInit, bool mountainInit)
                {
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.PLAINS).PlayerUnits[playerIndex].Count, plainsInit?1:0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.FOREST).PlayerUnits[playerIndex].Count, forestInit?1:0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN).PlayerUnits[playerIndex].Count, mountainInit?1:0);
                    // Check tiles
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.PLAINS).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, plainsInit ? 1 : 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.FOREST).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, forestInit ? 1 : 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, mountainInit ? 1 : 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.PLAINS).GetTileRelative(0, playerIndex).AllUnits.Count, plainsInit ? 1 : 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.FOREST).GetTileRelative(0, playerIndex).AllUnits.Count, forestInit ? 1 : 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN).GetTileRelative(0, playerIndex).AllUnits.Count, mountainInit ? 1 : 0);
                }
                // And check all lanes and tiles for both players
                verifyLaneStates(playerIndex, plainsInit, forestInit, mountainInit);
                // Change lane and play and verify
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.BOARD) ? chosenTarget : CardTargets.MOUNTAIN;
                plainsInit |= chosenTarget == CardTargets.PLAINS;
                forestInit |= chosenTarget == CardTargets.FOREST;
                mountainInit |= chosenTarget == CardTargets.MOUNTAIN;
                res = sm.PlayFromHand(1, chosenTarget);
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                verifyLaneStates(playerIndex, plainsInit, forestInit, mountainInit);
                // Change lane and play and verify again!
                chosenTarget = (CardTargets)((int)chosenTarget >> 1); // Change target to a different (valid) lane
                chosenTarget = (chosenTarget != CardTargets.BOARD) ? chosenTarget : CardTargets.MOUNTAIN;
                plainsInit |= chosenTarget == CardTargets.PLAINS;
                forestInit |= chosenTarget == CardTargets.FOREST;
                mountainInit |= chosenTarget == CardTargets.MOUNTAIN;
                res = sm.PlayFromHand(1, chosenTarget);
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
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 1, 1, 1));
                for (int i = 0; i < 5; i++)
                {
                    // Insert token cards, 1 in all stats, summonable in any lane 
                    // Insert to both players hands and decks
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, false); // Also ensure hashes are unique for board here
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play card in some lane
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Check also for the lane, unit is in the correct place
                Lane lane = sm.DetailedState.BoardState.GetLane(chosenTarget);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1); // Lane has 1 unit of player and 0 units of other one
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTileRelative(0, otherPlayerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileRelative(-1, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTileRelative(-1, otherPlayerIndex).PlayerUnits[playerIndex].Count, 1);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, false); // Also ensure hashes are unique for board here
                // Ok! Now I end turn, do the draw phase
                sm.EndTurn();
                Assert.AreEqual(sm.DetailedState.CurrentState, States.DRAW_PHASE);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true); // Hash should be here as board didn't change!
                sm.Step();
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true); // Hash should be here as board didn't change!
                // Ok now other player plays card...
                res = sm.PlayFromHand(1, chosenTarget); // Play card in exactly the same lane
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Check also for the lane, unit is in the correct place for the other player
                lane = sm.DetailedState.BoardState.GetLane(chosenTarget);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1); // Lane has 1 unit of each now
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTileRelative(0, otherPlayerIndex).PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(-1, playerIndex).PlayerUnits[otherPlayerIndex].Count, 1); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTileRelative(-1, otherPlayerIndex).PlayerUnits[playerIndex].Count, 1);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, false); // Also ensure hashes are unique for board here
                // Finally do reversions
                // Revert playing and draw
                sm.UndoPreviousStep(); // Unplay p2
                lane = sm.DetailedState.BoardState.GetLane(chosenTarget);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1); // Lane has 1 unit of player and 0 units of other one
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTileRelative(0, otherPlayerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileRelative(-1, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTileRelative(-1, otherPlayerIndex).PlayerUnits[playerIndex].Count, 1);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true); // Hash should be present
                sm.UndoPreviousStep(); // Undraw
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true); // Hash should be present
                sm.UndoPreviousStep(); // Un-end p1 turn
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true); // Hash should be present
                sm.UndoPreviousStep(); // Unplay p1
                lane = sm.DetailedState.BoardState.GetLane(chosenTarget);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 0); // Lane has 1 unit of player and 0 units of other one
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0); // Same with tiles, first and last check
                Assert.AreEqual(lane.GetTileRelative(0, otherPlayerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileRelative(-1, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0); // Also check if first-last is coherent
                Assert.AreEqual(lane.GetTileRelative(-1, otherPlayerIndex).PlayerUnits[playerIndex].Count, 0);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true); // Hash should be present
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
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 0, 1, 1, 1));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert token cards, 1 in all stats but 0 HP, summonable in any lane 
                }
                state.PlayerStates[playerIndex].Gold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                boardHash = sm.DetailedState.BoardState.GetGameStateHash(); // Store hash
                // Will play one of them
                CardTargets chosenTarget = (CardTargets)(1 << _rng.Next(3)); // Choose a random lane as target
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure unit has insta-died (nothing in field, 1 card in GY
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 0); // Player has no units
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 0); // Field has no units
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(chosenTarget).PlayerUnits[playerIndex].Count, 0); // Lane doesn't have the unit
                // Check board hash has not changed (as there's no GY)
                Assert.AreEqual(boardHash, sm.DetailedState.BoardState.GetGameStateHash());
                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 0); // Player still has no units
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 0); // And field has no units
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(chosenTarget).PlayerUnits[playerIndex].Count, 0); // Lane doesn't have the unit
                // Check board hash is still same
                Assert.AreEqual(boardHash, sm.DetailedState.BoardState.GetGameStateHash());
            }
        }
        [TestMethod]
        public void HashTest()
        {
            Unit u1, u2;
            u1 = new Unit()
            {
                UniqueId = 1,
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
            Assert.AreEqual(u1.GetGameStateHash(), u2.GetGameStateHash());
            // Now change a few things
            u2.Attack = 0;
            Assert.AreNotEqual(u1.GetGameStateHash(), u2.GetGameStateHash());
            // Revert
            u2.Attack = u1.Attack;
            Assert.AreEqual(u1.GetGameStateHash(), u2.GetGameStateHash());
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

        // UNITS ADVANCE TESTS
        [TestMethod]
        public void UnitAdvanceTest()
        {
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                int movement = _rng.Next(1, GameConstants.MOUNTAIN_TILES_NUMBER); // Random movement unit, but want to make it so it never reaches castle
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 1, movement, 1));
                // Will try this in mountain!
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, CardTargets.MOUNTAIN); // Play card in mountain (longest lane)!
                // Make sure card was played ok
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Ok! Now i need to do the sequence to advance...
                sm.EndTurn(); // End player's
                sm.Step(); // Finish draw phase of other player
                sm.EndTurn(); // End players turn
                // Now I'm about to do the advance. Before the advance:
                Lane lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                int UnitHash = sm.GetBoardEntity(0).GetGameStateHash(); // Get Hash of unit
                int preAdvanceHash = sm.DetailedState.GetGameStateHash(); // Hash of game overall
                // Now advance! Ensure result of basic advance
                sm.Step();
                lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                // Ensures the unit moved the right number of tiles in the right direction
                Assert.AreEqual(lane.GetTileRelative(movement, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreNotEqual(preAdvanceHash, sm.DetailedState.GetGameStateHash()); // Hash of board has changed
                Assert.AreEqual(sm.GetBoardEntity(0).GetGameStateHash(), UnitHash); // However, the unit should be the same!
                // Finally revert the advance
                sm.UndoPreviousStep();
                lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(movement, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(preAdvanceHash, sm.DetailedState.GetGameStateHash());
                Assert.AreEqual(sm.GetBoardEntity(0).GetGameStateHash(), UnitHash);
            }
        }
        [TestMethod]
        public void UnitAdvanceStoppedByEnemy()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct // This time the opponent starts, so their unit is in the middle of lane
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = (CurrentPlayer)otherPlayerIndex
                };
                // Will try this in mountain!
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 0, 9, 1)); // Max movement bc it's stopped by enemy anyway
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, CardTargets.ALL_LANES, 1, 0, 3, 1)); // 3 movement so it's in the mid-ish of lane
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both 0 attack so they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Max movement bc it's stopped by enemy anyway
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(2); // 3 movement so it's in the mid-ish of lane
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(2);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(2, CardTargets.MOUNTAIN); // Opp plays in mountain and skips turn
                // Ok! Now i need to do the sequence to advance...
                sm.EndTurn(); // End opponent's
                sm.Step(); // Finish draw phase of main player
                // Now I summon unit:
                sm.PlayFromHand(1, CardTargets.MOUNTAIN);
                // Now I end turn and opponent will advance!
                sm.EndTurn();
                sm.Step();
                sm.EndTurn();
                // Finally, player's unit will advance! Pre advance:
                int preAdvanceHash = sm.DetailedState.GetGameStateHash();
                Lane lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                // Both players have a unit
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                int intersectionCoordinate = lane.GetAbsoluteTileCoord(0,otherPlayerIndex) + Lane.GetAdvanceDirection(otherPlayerIndex) * 3;
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                // Advance...
                sm.Step();
                // Verify
                lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                // Now unit is not in advance anymore and instead moved to intersect
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreNotEqual(preAdvanceHash, sm.DetailedState.GetGameStateHash()); // Hash verif
                // Undo advance and verify again
                sm.UndoPreviousStep();
                lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                // Now unit is not in advance anymore and instead moved to intersect
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(preAdvanceHash, sm.DetailedState.GetGameStateHash()); // Hash verif
            }
        }
        [TestMethod]
        public void UnitAdvanceStoppedByEndOfLane()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 0, 9, 1));
                // Will try this in all lanes!
                state.PlayerStates[playerIndex].Hp = 30;
                state.PlayerStates[otherPlayerIndex].Hp = 30;
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. All 0 attack so they can't damage stuff
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Max movement bc it's stopped by lane anyway
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(1, CardTargets.PLAINS);
                sm.PlayFromHand(1, CardTargets.FOREST);
                sm.PlayFromHand(1, CardTargets.MOUNTAIN);
                // Verify...
                Lane plains = sm.DetailedState.BoardState.GetLane(LaneID.PLAINS);
                Lane forest = sm.DetailedState.BoardState.GetLane(LaneID.FOREST);
                Lane mountain = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                // Verify position of units, so that units are in first
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                sm.EndTurn(); // End turn
                sm.Step(); // Finish draw phase of opp
                sm.EndTurn(); // Finish opp turn
                // Now about to start my advance, no hash verif needed
                sm.Step();
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                // After advance, units should be on opposite end of lane!
                Assert.AreEqual(plains.GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                // After undo...
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                // After advance, units should be on opposite end of lane!
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
            }
        }
        [TestMethod]
        public void AdvanceDenominatorTest()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                HashSet<int> boardHashes = new HashSet<int>();
                HashSet<int> unitHashes = new HashSet<int>();
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, 0, 1, 9));
                // Will try this in all lanes!
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. All 0 attack so they can't damage stuff
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Play units and also store them for evil illegal modifications
                sm.PlayFromHand(1, CardTargets.PLAINS);
                Unit plainsUnit = (Unit)sm.DetailedState.BoardState.Entities.Last().Value;
                sm.PlayFromHand(1, CardTargets.FOREST);
                Unit forestUnit = (Unit)sm.DetailedState.BoardState.Entities.Last().Value;
                sm.PlayFromHand(1, CardTargets.MOUNTAIN);
                Unit mountainsUnit = (Unit)sm.DetailedState.BoardState.Entities.Last().Value;
                // Verify...
                Lane plains = sm.DetailedState.BoardState.GetLane(LaneID.PLAINS);
                Lane forest = sm.DetailedState.BoardState.GetLane(LaneID.FOREST);
                Lane mountain = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                // Verify position of units, so that units are in first
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                sm.EndTurn(); // End turn
                sm.Step(); // Finish draw phase of opp
                sm.EndTurn(); // Finish opp turn
                // Now, I quickly modify the denominators
                plainsUnit.MvtCooldownTimer = 0; // Will advance now
                forestUnit.MvtCooldownTimer = 8; // Won't advance but will do so in the following one
                mountainsUnit.MvtCooldownTimer = 1; // Not advancing in this test
                // Regarding hashes, hashes will change constantly because of the denominator, only repeat after 9 turns...
                TestHelperFunctions.HashSetVerification(plainsUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(forestUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(mountainsUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, false);
                sm.Step(); // Advance
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                // Only plains should've moved
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(plains.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                // Rest should still be in the first place
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 0);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, false);
                TestHelperFunctions.HashSetVerification(plainsUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(forestUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(mountainsUnit, unitHashes, false);
                // Another advance sequence, only forest moves in this case
                sm.EndTurn();
                sm.Step();
                sm.EndTurn();
                sm.Step(); // Advance
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(plains.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(forest.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 0);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, false);
                TestHelperFunctions.HashSetVerification(plainsUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(forestUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(mountainsUnit, unitHashes, false);
                // And the last one should remain the same...
                sm.EndTurn();
                sm.Step();
                sm.EndTurn();
                sm.Step(); // Advance
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(plains.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(forest.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 0);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, false); // BUT HASH SHOULD CHANGE AS IT IS A DIFFERENT STATE!
                TestHelperFunctions.HashSetVerification(plainsUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(forestUnit, unitHashes, false);
                TestHelperFunctions.HashSetVerification(mountainsUnit, unitHashes, false);
                // Undo!
                sm.UndoPreviousStep(); sm.UndoPreviousStep(); sm.UndoPreviousStep(); sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(plains.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(forest.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 0);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true);
                TestHelperFunctions.HashSetVerification(plainsUnit, unitHashes, true);
                TestHelperFunctions.HashSetVerification(forestUnit, unitHashes, true);
                TestHelperFunctions.HashSetVerification(mountainsUnit, unitHashes, true);
                sm.UndoPreviousStep(); sm.UndoPreviousStep(); sm.UndoPreviousStep(); sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(plains.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 0);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true);
                TestHelperFunctions.HashSetVerification(plainsUnit, unitHashes, true);
                TestHelperFunctions.HashSetVerification(forestUnit, unitHashes, true);
                TestHelperFunctions.HashSetVerification(mountainsUnit, unitHashes, true); 
                sm.UndoPreviousStep(); sm.UndoPreviousStep(); sm.UndoPreviousStep(); sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                Assert.AreEqual(plains.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(plains.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(forest.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(mountain.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                TestHelperFunctions.HashSetVerification(sm.DetailedState.BoardState, boardHashes, true);
                TestHelperFunctions.HashSetVerification(plainsUnit, unitHashes, true);
                TestHelperFunctions.HashSetVerification(forestUnit, unitHashes, true);
                TestHelperFunctions.HashSetVerification(mountainsUnit, unitHashes, true);
            }
        }
        // Unit battle test
        [TestMethod]
        public void TestUnitCombatNoDeath()
        {
            // Units clash, receive damage but both stay in lane. Damages are different to ensure token calculations are also ok
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int attack = _rng.Next(1,8); // Attack between 1-7
                int hp = attack + 2; // So that both units survive
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct // This time the opponent starts, so their unit is in the middle of lane
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = (CurrentPlayer)otherPlayerIndex
                };
                // Will try this in mountain as there's space!
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, hp, attack, 9, 1)); // Gets to the end so it clashes
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, CardTargets.ALL_LANES, hp, attack+1, 3, 1)); // Gets to the middle and waits there
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(2);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(2);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(2, CardTargets.MOUNTAIN); // Opp plays in mountain and skips turn
                // Ok! Now i need to do the sequence to advance...
                sm.EndTurn(); // End opponent's
                sm.Step(); // Finish draw phase of main player
                // Now I summon unit:
                sm.PlayFromHand(1, CardTargets.MOUNTAIN);
                // Now I end turn and opponent will advance!
                sm.EndTurn();
                sm.Step();
                sm.EndTurn();
                // Finally, player's unit will advance! Pre advance, I check both units HP and count (2)
                Lane lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                // Both players have a unit
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 2); // 2 units total
                Assert.AreEqual(sm.GetBoardEntity(0).DamageTokens, 0); // No damage
                Assert.AreEqual(sm.GetBoardEntity(1).DamageTokens, 0); // No damage
                // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                int intersectionCoordinate = lane.GetAbsoluteTileCoord(0,otherPlayerIndex) + Lane.GetAdvanceDirection(otherPlayerIndex) * 3;
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).AllUnits.Count, 1);
                // Advance...
                sm.Step();
                // Verify
                lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1); // Players still have their dudes
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 2); // 2 units total
                Assert.AreEqual(sm.GetBoardEntity(0).DamageTokens, attack); // They have damage!
                Assert.AreEqual(sm.GetBoardEntity(1).DamageTokens, attack + 1);
                // Now unit is not in advance anymore and instead moved to intersect
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).AllUnits.Count, 2); // 2 Now
                // Undo advance and verify again
                sm.UndoPreviousStep();
                lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                // Both players have a unit
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 2); // 2 units total
                Assert.AreEqual(sm.GetBoardEntity(0).DamageTokens, 0); // No damage
                Assert.AreEqual(sm.GetBoardEntity(1).DamageTokens, 0); // No damage
                // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).AllUnits.Count, 1);
            }
        }
        [TestMethod]
        public void TestUnitCombatWithDeath()
        {
            Random _rng = new Random();
            List<int> attackerExtraDmg = [-1, -1, -1, 0, 0, 0, 1, 1, 1]; // All combos of mutual kill, mutual death, mutual overkill
            List<int> defenderExtraDmg = [-1, 0, 1, -1, 0, 1, -1, 0, 1];
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            for (int i = 0; i < attackerExtraDmg.Count; i++)
            {
                int hp = _rng.Next(1, 9); // Hp between 1-8 so units can under and overkill
                foreach (CurrentPlayer player in players)
                {
                    int attackerStat = hp + attackerExtraDmg[i];
                    int defenderStat = hp + defenderExtraDmg[i];
                    int playerIndex = (int)player;
                    int otherPlayerIndex = 1 - playerIndex;
                    GameStateStruct state = new GameStateStruct // This time the opponent starts, so their unit is in the middle of lane
                    {
                        CurrentState = States.ACTION_PHASE,
                        CurrentPlayer = (CurrentPlayer)otherPlayerIndex
                    };
                    // Will try this in mountain as there's space!
                    CardFinder cardDb = new CardFinder();
                    cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, hp, attackerStat, 9, 1)); // Gets to the end so it clashes
                    cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, CardTargets.ALL_LANES, hp, defenderStat, 3, 1)); // Gets to the middle and waits there
                    for (int j = 0; j < 5; j++)
                    {
                        // Insert to both players hands and decks. Both have attack but they can't kill themselves
                        state.PlayerStates[playerIndex].Hand.InsertCard(1);
                        state.PlayerStates[playerIndex].Deck.InsertCard(1);
                        state.PlayerStates[otherPlayerIndex].Hand.InsertCard(2);
                        state.PlayerStates[otherPlayerIndex].Deck.InsertCard(2);
                    }
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    sm.PlayFromHand(2, CardTargets.MOUNTAIN); // Opp plays in mountain and skips turn
                    // Ok! Now i need to do the sequence to advance...
                    sm.EndTurn(); // End opponent's
                    sm.Step(); // Finish draw phase of main player
                    // Now I summon unit:
                    sm.PlayFromHand(1, CardTargets.MOUNTAIN);
                    // Now I end turn and opponent will advance!
                    sm.EndTurn();
                    sm.Step();
                    sm.EndTurn();
                    // Finally, player's unit will advance! Pre advance, I check both units HP and count (2)
                    Lane lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                    // Both players have a unit
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 2); // 2 units total
                    Assert.AreEqual(sm.GetBoardEntity(0).DamageTokens, 0); // No damage
                    Assert.AreEqual(sm.GetBoardEntity(1).DamageTokens, 0); // No damage
                    // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                    int intersectionCoordinate = lane.GetAbsoluteTileCoord(0,otherPlayerIndex) + Lane.GetAdvanceDirection(otherPlayerIndex) * 3;
                    Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(lane.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).AllUnits.Count, 1);
                    // Advance...
                    sm.Step();
                    // Verify
                    int attackerCount = (defenderExtraDmg[i] < 0) ? 1 : 0; // Attacker is there only if defender's weak
                    int defenderCount = (attackerExtraDmg[i] < 0) ? 1 : 0; // Defender is there only if attacker's weak
                    lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, attackerCount); // Players still have their dudes
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, defenderCount);
                    Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, attackerCount);
                    Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, defenderCount);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, attackerCount + defenderCount); // 2 units total
                    if (defenderCount > 0)
                    {
                        Assert.AreEqual(sm.GetBoardEntity(0).DamageTokens, attackerStat); // Unit alive but damaged
                    }
                    else
                    {
                        Assert.IsFalse(sm.DetailedState.BoardState.Entities.ContainsKey(0)); // unit ded
                    }
                    if (attackerCount > 0)
                    {
                        Assert.AreEqual(sm.GetBoardEntity(1).DamageTokens, defenderStat); // Unit alive but damaged
                    }
                    else
                    {
                        Assert.IsFalse(sm.DetailedState.BoardState.Entities.ContainsKey(1)); // unit ded
                    }
                    // Now unit is not in advance anymore and instead moved to intersect
                    Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                    Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[playerIndex].Count, attackerCount);
                    Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, defenderCount);
                    Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).AllUnits.Count, attackerCount + defenderCount); // 2 Now
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    lane = sm.DetailedState.BoardState.GetLane(CardTargets.MOUNTAIN);
                    // Both players have a unit
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(lane.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(lane.PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 2); // 2 units total
                    Assert.AreEqual(sm.GetBoardEntity(0).DamageTokens, 0); // No damage
                    Assert.AreEqual(sm.GetBoardEntity(1).DamageTokens, 0); // No damage
                    // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                    Assert.AreEqual(lane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(lane.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(lane.GetTileAbsolute(intersectionCoordinate).AllUnits.Count, 1);
                }
            }
        }
        [TestMethod]
        public void TestMultipleWithoutBreakingAdvance()
        {
            Random _rng = new Random();
            List<CardTargets> lanes = [CardTargets.PLAINS, CardTargets.FOREST, CardTargets.MOUNTAIN]; // Test interaction in all orders
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CardTargets lane in lanes)
            {
                int stat = _rng.Next(1, 10); // any Hp between 1-9
                var movt = lane switch
                {
                    CardTargets.PLAINS => GameConstants.PLAINS_TILES_NUMBER - 1,
                    CardTargets.FOREST => GameConstants.FOREST_TILES_NUMBER - 1,
                    CardTargets.MOUNTAIN => GameConstants.MOUNTAIN_TILES_NUMBER - 1,
                    _ => throw new Exception("Wrong lane!"),
                };
                foreach (CurrentPlayer player in players)
                {
                    int playerIndex = (int)player;
                    int otherPlayerIndex = 1 - playerIndex;
                    GameStateStruct state = new GameStateStruct // This time the opponent starts, so their unit is in the middle of lane
                    {
                        CurrentState = States.ACTION_PHASE,
                        CurrentPlayer = (CurrentPlayer)otherPlayerIndex
                    };
                    // Will try this in mountain as there's space!
                    CardFinder cardDb = new CardFinder();
                    cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, stat, stat, 1, 1)); // Gets to the end so it clashes
                    cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, CardTargets.ALL_LANES, stat, stat, movt, 1)); // Gets to the middle and waits there
                    for (int j = 0; j < 5; j++)
                    {
                        // Insert to both players hands and decks. Both have attack but they can't kill themselves
                        state.PlayerStates[playerIndex].Hand.InsertCard(1);
                        state.PlayerStates[playerIndex].Deck.InsertCard(1);
                        state.PlayerStates[otherPlayerIndex].Hand.InsertCard(2);
                        state.PlayerStates[otherPlayerIndex].Deck.InsertCard(2);
                    }
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    sm.PlayFromHand(2, lane); // Opp plays in lane and skips turn
                    // Ok! Now i need to do the sequence to advance...
                    sm.EndTurn(); // End opponent's
                    sm.Step(); // Finish draw phase of main player
                    // Now I summon all units in all lanes:
                    sm.PlayFromHand(1, CardTargets.PLAINS);
                    sm.PlayFromHand(1, CardTargets.FOREST);
                    sm.PlayFromHand(1, CardTargets.MOUNTAIN);
                    // Now I end turn and opponent will advance!
                    sm.EndTurn();
                    sm.Step();
                    sm.EndTurn();
                    // Finally, player's unit will advance! Pre advance, I check both units HP and count (2)
                    Lane theLane = sm.DetailedState.BoardState.GetLane(lane);
                    CardTargets nextTgt = (CardTargets)((((int)lane << 1) > 4) ? 1: ((int)lane << 1));
                    Lane other1 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    nextTgt = (CardTargets)((((int)nextTgt << 1) > 4) ? 1 : ((int)nextTgt << 1));
                    Lane other2 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(theLane.PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(other1.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other2.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 4); // 4 units total
                    // Verify position of units. One tile has 2 things and the others 1
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).AllUnits.Count, 2);
                    Assert.AreEqual(other1.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other1.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(other1.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(other2.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other2.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(other2.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    // Advance...
                    sm.Step();
                    // Verify
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    nextTgt = (CardTargets)((((int)lane << 1) > 4) ? 1 : ((int)lane << 1));
                    other1 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    nextTgt = (CardTargets)((((int)nextTgt << 1) > 4) ? 1 : ((int)nextTgt << 1));
                    other2 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    // Check unit counts, similar but both players lost the unit in "the lane", the others advanced
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 2);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(theLane.PlayerUnits[playerIndex].Count, 0);
                    Assert.AreEqual(theLane.PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(other1.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other2.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 2);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).AllUnits.Count, 0);
                    Assert.AreEqual(other1.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1); // Make sure the others displaced properly
                    Assert.AreEqual(other1.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                    Assert.AreEqual(other1.GetTileRelative(1, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(other2.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other2.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                    Assert.AreEqual(other2.GetTileRelative(1, playerIndex).AllUnits.Count, 1);
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    nextTgt = (CardTargets)((((int)lane << 1) > 4) ? 1 : ((int)lane << 1));
                    other1 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    nextTgt = (CardTargets)((((int)nextTgt << 1) > 4) ? 1 : ((int)nextTgt << 1));
                    other2 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 1);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(theLane.PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(other1.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other2.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 4); // 4 units total
                    // Verify position of units. One tile has 2 things and the others 1
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 1);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).AllUnits.Count, 2);
                    Assert.AreEqual(other1.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other1.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(other1.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(other2.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(other2.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(other2.GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                }
            }
        }
        [TestMethod]
        public void TestMultipleTradesInTile()
        {
            Random _rng = new Random();
            List<CardTargets> lanes = [CardTargets.PLAINS, CardTargets.FOREST, CardTargets.MOUNTAIN]; // Test interaction in all orders
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CardTargets lane in lanes)
            {
                int stat = _rng.Next(1, 10); // any Hp between 1-9
                var movt = lane switch
                {
                    CardTargets.PLAINS => GameConstants.PLAINS_TILES_NUMBER - 1,
                    CardTargets.FOREST => GameConstants.FOREST_TILES_NUMBER - 1,
                    CardTargets.MOUNTAIN => GameConstants.MOUNTAIN_TILES_NUMBER - 1,
                    _ => throw new Exception("Wrong lane!"),
                };
                foreach (CurrentPlayer player in players)
                {
                    int playerIndex = (int)player;
                    int otherPlayerIndex = 1 - playerIndex;
                    GameStateStruct state = new GameStateStruct // This time the opponent starts, so their unit is in the middle of lane
                    {
                        CurrentState = States.ACTION_PHASE,
                        CurrentPlayer = (CurrentPlayer)otherPlayerIndex
                    };
                    // Will try this in mountain as there's space!
                    CardFinder cardDb = new CardFinder();
                    cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, stat, stat, 1, 1)); // Gets to the end so it clashes
                    cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, CardTargets.ALL_LANES, stat, stat, movt, 1)); // Gets to the middle and waits there
                    for (int j = 0; j < 5; j++)
                    {
                        // Insert to both players hands and decks. Both have attack but they can't kill themselves
                        state.PlayerStates[playerIndex].Hand.InsertCard(1);
                        state.PlayerStates[playerIndex].Deck.InsertCard(1);
                        state.PlayerStates[otherPlayerIndex].Hand.InsertCard(2);
                        state.PlayerStates[otherPlayerIndex].Deck.InsertCard(2);
                    }
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    sm.PlayFromHand(2, lane); // Opp plays 2 of same, 0 and 1
                    sm.PlayFromHand(2, lane);
                    // Ok! Now i need to do the sequence to advance...
                    sm.EndTurn(); // End opponent's
                    sm.Step(); // Finish draw phase of main player
                    // Now I summon 3 units: 2,3,4
                    sm.PlayFromHand(1, lane);
                    sm.PlayFromHand(1, lane);
                    sm.PlayFromHand(1, lane);
                    // Now I end turn and opponent will advance!
                    sm.EndTurn();
                    sm.Step();
                    sm.EndTurn();
                    // Finally, player's unit will advance! Pre advance, I check both units HP and count (2)
                    Lane theLane = sm.DetailedState.BoardState.GetLane(lane);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 2);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(theLane.PlayerUnits[otherPlayerIndex].Count, 2);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 5); // 5 units total
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 2);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).AllUnits.Count, 5);
                    // Only for this, also check ids
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(0));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(1));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(2));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(3));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(4));
                    // Advance...
                    sm.Step();
                    // Verify
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 0);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(theLane.PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 1); // 1 unit total
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                    Assert.AreEqual(theLane.GetTileRelative(1, playerIndex).PlayerUnits[playerIndex].Count, 1); // in the next tile!
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 0);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).AllUnits.Count, 0);
                    Assert.AreEqual(theLane.GetTileRelative(1, playerIndex).AllUnits.Count, 1);
                    Assert.IsFalse(sm.DetailedState.BoardState.Entities.ContainsKey(0));
                    Assert.IsFalse(sm.DetailedState.BoardState.Entities.ContainsKey(1));
                    Assert.IsFalse(sm.DetailedState.BoardState.Entities.ContainsKey(2));
                    Assert.IsFalse(sm.DetailedState.BoardState.Entities.ContainsKey(3));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(4)); // Only one that remains
                    Assert.AreEqual(sm.GetBoardEntity(4).DamageTokens, 0); // undamaged too
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.PlayerUnits[otherPlayerIndex].Count, 2);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(theLane.PlayerUnits[otherPlayerIndex].Count, 2);
                    Assert.AreEqual(sm.DetailedState.BoardState.AllUnits.Count, 5); // 5 units total
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 3);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).PlayerUnits[otherPlayerIndex].Count, 2);
                    Assert.AreEqual(theLane.GetTileRelative(0, playerIndex).AllUnits.Count, 5);
                    // Only for this, also check ids
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(0));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(1));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(2));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(3));
                    Assert.IsTrue(sm.DetailedState.BoardState.Entities.ContainsKey(4));
                }
            }
        }
        // Direct damage tests
        [TestMethod]
        public void TestNonlethalDirectDamage()
        {
            // A unit advances, and will damage enemy
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int attack = _rng.Next(1, 10); // Attack between 1-9
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = (CurrentPlayer)playerIndex
                };
                state.PlayerStates[playerIndex].Hp = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp = GameConstants.STARTING_HP;
                // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, attack, 9, 1));
                CardTargets target = (CardTargets)(1 << _rng.Next(0, 3)); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(1, target); // Play the unit
                // Ok! Now i need to do the sequence to advance...
                sm.EndTurn(); // End turn
                sm.Step(); // Finish draw phase of opp
                sm.EndTurn(); // End opp turn
                // Now the unit is ready to advance, check before and after
                PlayerState ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetGameStateHash();
                PlayerState ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetGameStateHash();
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1); // 1 unit in tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1); // From correct player
                // Player's unit will advance!
                sm.Step();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.DamageTokens, attack); // Now player 2 has less Hp
                Assert.AreNotEqual(ps1Hash, ps1.GetGameStateHash()); // Because they drew card
                Assert.AreNotEqual(ps2Hash, ps2.GetGameStateHash());
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE); // Still in action phase, not EOG
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 1); // 1 unit in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 1); // From correct player
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps1Hash, ps1.GetGameStateHash());
                Assert.AreEqual(ps2Hash, ps2.GetGameStateHash());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1); // Unit back at beginning
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 0);
            }
        }
        [TestMethod]
        public void UnitBlockedFromDirectDamage()
        {
            // A unit advances, and will damage enemy
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = (CurrentPlayer)playerIndex
                };
                state.PlayerStates[playerIndex].Hp = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp = GameConstants.STARTING_HP;
                // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 2, 1, 9, 1));
                CardTargets target = (CardTargets)(1 << _rng.Next(0, 3)); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(1, target); // Play the unit
                // Ok! Now i need to do the sequence to advance...
                sm.EndTurn(); // End turn
                sm.Step(); // Finish draw phase of opp
                sm.PlayFromHand(1, target); // Play the unit for opp
                sm.EndTurn(); // End opp turn
                // Now the unit is ready to advance, will collide with enemy tho
                PlayerState ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetGameStateHash();
                PlayerState ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetGameStateHash();
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP); // Because they drew card
                Assert.AreEqual(ps2.Hp, GameConstants.STARTING_HP); // However, this one didn't get any change
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1); // Check both units
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 1); // Ensure enemy blocks last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[otherPlayerIndex].Count, 1);
                // Player's unit will advance!
                sm.Step();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, GameConstants.STARTING_HP); // This time the opp is undamaged
                Assert.AreNotEqual(ps1Hash, ps1.GetGameStateHash()); // Players should have the same hash as their situation hasn't changed? (No draw cards etc)
                Assert.AreEqual(ps2Hash, ps2.GetGameStateHash());
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE); // Still in action phase, not EOG
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 2); // 2 now in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 1); // From both players
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[otherPlayerIndex].Count, 1); // From both players
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps1Hash, ps1.GetGameStateHash());
                Assert.AreEqual(ps2Hash, ps2.GetGameStateHash());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[otherPlayerIndex].Count, 1);
            }
        }
        [TestMethod]
        public void ExactDirectDamageKill()
        {
            // A unit advances, and will kill enemy exactly
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int attack = _rng.Next(1, 10); // Attack between 1-9
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = (CurrentPlayer)playerIndex
                };
                state.PlayerStates[playerIndex].Hp = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp = attack; // Opp has less HP this time
                // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, attack, 9, 1));
                CardTargets target = (CardTargets)(1 << _rng.Next(0, 3)); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(1, target); // Play the unit
                // Ok! Now i need to do the sequence to advance...
                sm.EndTurn(); // End turn
                sm.Step(); // Finish draw phase of opp
                sm.EndTurn(); // End opp turn
                // Now the unit is ready to advance, check before and after
                PlayerState ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetGameStateHash();
                PlayerState ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetGameStateHash();
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, attack);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1); // 1 unit in tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1); // From correct player
                // Player's unit will advance!
                sm.Step();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, ps2.DamageTokens); // Now player 2 is dead
                Assert.AreEqual(ps1Hash, ps1.GetGameStateHash()); // Because the game ends during advance without drawing card!!!!
                Assert.AreNotEqual(ps2Hash, ps2.GetGameStateHash()); // Because they are dead
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, player); // Indicates player who won (player for now can only win in their turn so this is pointless)
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 1); // 1 unit in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 1); // From correct player
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, attack);
                Assert.AreEqual(ps1Hash, ps1.GetGameStateHash());
                Assert.AreEqual(ps2Hash, ps2.GetGameStateHash());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1); // Unit back at beginning
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 0);
            }
        }
        [TestMethod]
        public void DirectDamageOverkill()
        {
            // A unit advances, and will kill enemy with extra damage which doesnt matter
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int attack = _rng.Next(2, 10); // Attack between 1-9
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = (CurrentPlayer)playerIndex
                };
                state.PlayerStates[playerIndex].Hp = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp = attack - 1; // Opp has less HP this time
                                                                      // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, attack, 9, 1));
                CardTargets target = (CardTargets)(1 << _rng.Next(0, 3)); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(1, target); // Play the unit
                // Ok! Now i need to do the sequence to advance...
                sm.EndTurn(); // End turn
                sm.Step(); // Finish draw phase of opp
                sm.EndTurn(); // End opp turn
                // Now the unit is ready to advance, check before and after
                PlayerState ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetGameStateHash();
                PlayerState ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetGameStateHash();
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, attack - 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1); // 1 unit in tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1); // From correct player
                // Player's unit will advance!
                sm.Step();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, ps2.DamageTokens); // Now player 2 is dead
                Assert.AreEqual(ps1Hash, ps1.GetGameStateHash()); // Because the game ends during advance without drawing card!!!!
                Assert.AreNotEqual(ps2Hash, ps2.GetGameStateHash()); // Because they are dead
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, player); // Indicates player who won (player for now can only win in their turn so this is pointless)
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 1); // 1 unit in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 1); // From correct player
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp, attack - 1);
                Assert.AreEqual(ps1Hash, ps1.GetGameStateHash());
                Assert.AreEqual(ps2Hash, ps2.GetGameStateHash());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).AllUnits.Count, 1); // Unit back at beginning
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).AllUnits.Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileRelative(-1, playerIndex).PlayerUnits[playerIndex].Count, 0);
            }
        }
        [TestMethod]
        public void MultiUnitTest()
        {
            // Tests coherence, where game will stop as soon as player dies mid-advance, so units fill be frozen
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            for (int unitThatKills = 1; unitThatKills <= 3; unitThatKills++)
            {
                foreach (CurrentPlayer player in players)
                {
                    int attack = 6 / unitThatKills; // That way the unit that kills, kills
                    int playerIndex = (int)player;
                    int otherPlayerIndex = 1 - playerIndex;
                    GameStateStruct state = new GameStateStruct
                    {
                        CurrentState = States.ACTION_PHASE,
                        CurrentPlayer = (CurrentPlayer)playerIndex
                    };
                    state.PlayerStates[playerIndex].Hp = GameConstants.STARTING_HP; // It's important to set this
                    state.PlayerStates[otherPlayerIndex].Hp = 6; // Opp has 6HP which is pretty handy for this test
                    CardFinder cardDb = new CardFinder();
                    cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, attack, 9, 1));
                    for (int i = 0; i < 5; i++)
                    {
                        // Insert to both players hands and decks. Both have attack but they can't kill themselves
                        state.PlayerStates[playerIndex].Hand.InsertCard(1);
                        state.PlayerStates[playerIndex].Deck.InsertCard(1);
                        state.PlayerStates[otherPlayerIndex].Hand.InsertCard(1);
                        state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                    }
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    sm.PlayFromHand(1, CardTargets.PLAINS); // Play all units, one per lane
                    sm.PlayFromHand(1, CardTargets.FOREST);
                    sm.PlayFromHand(1, CardTargets.MOUNTAIN);
                    // Ok! Now i need to do the sequence to advance...
                    sm.EndTurn(); // End turn
                    sm.Step(); // Finish draw phase of opp
                    sm.EndTurn(); // End opp turn
                    // Now untis are ready to advance, check before and after
                    PlayerState ps1 = sm.DetailedState.PlayerStates[playerIndex];
                    int ps1Hash = ps1.GetGameStateHash();
                    PlayerState ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                    int ps2Hash = ps2.GetGameStateHash();
                    Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                    Assert.AreEqual(ps2.Hp, 6);
                    // Get units in all lanes
                    int plainsHash = sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetGameStateHash();
                    int forestHash = sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetGameStateHash();
                    int mountainHash = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetGameStateHash();
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    // Player's unit will advance!
                    sm.Step();
                    // Post advance check of same things, this will end game and units will be frozen in time
                    ps1 = sm.DetailedState.PlayerStates[playerIndex];
                    ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                    Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                    Assert.AreEqual(ps2.Hp, ps2.DamageTokens); // Now player 2 is dead
                    Assert.AreEqual(ps1Hash, ps1.GetGameStateHash()); // Because the game ends during advance without drawing card!!!!
                    Assert.AreNotEqual(ps2Hash, ps2.GetGameStateHash()); // Because they are dead
                    Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                    Assert.AreEqual(sm.DetailedState.CurrentPlayer, player); // Indicates player who won (player for now can only win in their turn so this is pointless)
                    // Unit positioning is coherent and hashes are. This depends on the unit that kills
                    Assert.AreEqual(unitThatKills < 1, plainsHash == sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetGameStateHash()); // Hash unchanged if unit doesn't advance
                    Assert.AreEqual(unitThatKills < 2, forestHash == sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetGameStateHash());
                    Assert.AreEqual(unitThatKills < 3, mountainHash == sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetGameStateHash());
                    // Unit will be in first tile or in last depending advance
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileRelative((unitThatKills >= 1)?-1:0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileRelative((unitThatKills >= 1) ? -1 : 0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileRelative((unitThatKills >= 2) ? -1 : 0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileRelative((unitThatKills >= 2) ? -1 : 0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileRelative((unitThatKills >= 3) ? -1 : 0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileRelative((unitThatKills >= 3) ? -1 : 0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    ps1 = sm.DetailedState.PlayerStates[playerIndex];
                    ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                    Assert.AreEqual(ps1.Hp, GameConstants.STARTING_HP);
                    Assert.AreEqual(ps2.Hp, 6);
                    // Get units in all lanes
                    Assert.AreEqual(plainsHash, sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetGameStateHash());
                    Assert.AreEqual(forestHash, sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetGameStateHash());
                    Assert.AreEqual(mountainHash, sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetGameStateHash());
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileRelative(0, playerIndex).AllUnits.Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileRelative(0, playerIndex).PlayerUnits[playerIndex].Count, 1);
                }
            }
        }
        [TestMethod]
        public void BuildingDamagedOnAdvance()
        {
            // A unit advances, damages building
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int attack = _rng.Next(1, 9); // Attack between 1-8
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.DRAW_PHASE,
                    CurrentPlayer = (CurrentPlayer)playerIndex
                };
                // Will try this in any lane
                CardTargets target = (CardTargets)(1 << _rng.Next(0, 3)); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(0);
                    state.PlayerStates[playerIndex].Deck.InsertCard(0);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(0);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(0);
                }
                GameStateMachine stageSm = new GameStateMachine();
                stageSm.LoadGame(state); // Start from here
                Unit testUnit = TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, attack, 2, 1);
                testUnit.Owner = playerIndex;
                Building testBldg = TestCardGenerator.CreateBuilding(2, "BUILDING", 0, CardTargets.ALL_LANES, attack + 1, [], [], []);
                testBldg.Owner = otherPlayerIndex;
                Lane lane = stageSm.DetailedState.BoardState.GetLane(target);
                // Initialize unit into board in correct place, skip all playability stuff which is done elsewhere
                stageSm.BOARDENTITY_InitializeEntity(testUnit);
                stageSm.BOARDENTITY_InsertInLane(testUnit, lane.Id);
                int tileCoord = lane.GetAbsoluteTileCoord(0, playerIndex); // Put it in first tile
                stageSm.BOARDENTITY_InsertInTile(testUnit, tileCoord);
                // Same as the building in second tile
                stageSm.BOARDENTITY_InitializeEntity(testBldg);
                stageSm.BOARDENTITY_InsertInLane(testBldg, lane.Id);
                tileCoord = lane.GetAbsoluteTileCoord(1, playerIndex); // Put it in second tile
                stageSm.BOARDENTITY_InsertInTile(testBldg, tileCoord);
                // Create a new blank SM from load game
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(stageSm.DetailedState);
                // Pre advance
                state = sm.DetailedState;
                int hash = state.GetGameStateHash();
                Assert.AreEqual(state.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileRelative(1, playerIndex).AllBuildings.First(), testBldg.UniqueId);
                // Advance
                sm.Step();
                Assert.AreNotEqual(hash, sm.DetailedState.GetGameStateHash()); // Hash changed
                Assert.AreEqual(state.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(testBldg.DamageTokens, attack); // Bldg has now damage tokens
                Assert.AreEqual(state.BoardState.GetLane(target).PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileRelative(1, playerIndex).AllBuildings.First(), testBldg.UniqueId);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(hash, sm.DetailedState.GetGameStateHash()); // Hash changed
                Assert.AreEqual(state.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(testBldg.DamageTokens, 0);
                Assert.AreEqual(state.BoardState.GetLane(target).PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileRelative(1, playerIndex).AllBuildings.First(), testBldg.UniqueId);
            }
        }
        [TestMethod]
        public void BuildingKilledOnAdvance()
        {
            // A unit advances, destorys building
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int attack = _rng.Next(1, 10); // Attack between 1-9
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.DRAW_PHASE,
                    CurrentPlayer = (CurrentPlayer)playerIndex
                };
                // Will try this in any lane
                CardTargets target = (CardTargets)(1 << _rng.Next(0, 3)); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.InsertCard(0);
                    state.PlayerStates[playerIndex].Deck.InsertCard(0);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertCard(0);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(0);
                }
                GameStateMachine stageSm = new GameStateMachine();
                stageSm.LoadGame(state); // Start from here
                Unit testUnit = TestCardGenerator.CreateUnit(1, "UNIT", 0, CardTargets.ALL_LANES, 1, attack, 2, 1);
                testUnit.Owner = playerIndex;
                Building testBldg = TestCardGenerator.CreateBuilding(2, "BUILDING", 0, CardTargets.ALL_LANES, attack, [], [], []);
                testBldg.Owner = otherPlayerIndex;
                Lane lane = stageSm.DetailedState.BoardState.GetLane(target);
                // Initialize unit into board in correct place, skip all playability stuff which is done elsewhere
                stageSm.BOARDENTITY_InitializeEntity(testUnit);
                stageSm.BOARDENTITY_InsertInLane(testUnit, lane.Id);
                int tileCoord = lane.GetAbsoluteTileCoord(0, playerIndex); // Put it in first tile
                stageSm.BOARDENTITY_InsertInTile(testUnit, tileCoord);
                // Same as the building in second tile
                stageSm.BOARDENTITY_InitializeEntity(testBldg);
                stageSm.BOARDENTITY_InsertInLane(testBldg, lane.Id);
                tileCoord = lane.GetAbsoluteTileCoord(1, playerIndex); // Put it in second tile
                stageSm.BOARDENTITY_InsertInTile(testBldg, tileCoord);
                // Create blank SM
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(stageSm.DetailedState);
                // Pre advance
                state = sm.DetailedState;
                int hash = state.GetGameStateHash();
                Assert.AreEqual(state.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileRelative(1,playerIndex).AllBuildings.First(), testBldg.UniqueId);
                // Advance
                sm.Step();
                Assert.AreNotEqual(hash, sm.DetailedState.GetGameStateHash()); // Hash changed
                Assert.AreEqual(state.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.PlayerBuildings[otherPlayerIndex].Count, 0);
                Assert.AreEqual(state.BoardState.GetLane(target).PlayerBuildings[otherPlayerIndex].Count, 0);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(hash, sm.DetailedState.GetGameStateHash()); // Hash changed
                Assert.AreEqual(state.BoardState.PlayerUnits[playerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).PlayerBuildings[otherPlayerIndex].Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileRelative(1, playerIndex).AllBuildings.First(), testBldg.UniqueId);
            }
        }
    }
}
