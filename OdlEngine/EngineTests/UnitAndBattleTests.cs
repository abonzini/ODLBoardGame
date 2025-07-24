using ODLGameEngine;

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
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                // Card 1: test unit with 1 in all stats, summonable anywhere
                HashSet<int> allTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    allTiles.Add(i);
                }
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, allTiles, 1, 1, 1, 1));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].CurrentGold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                boardHash = sm.DetailedState.BoardState.GetBoardElementHashCode(sm.DetailedState.EntityData); // Store hash
                // Will play one of them
                int chosenTarget = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES); // Choose a random tile in the board
                Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure now there's a unit in the list
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // Player now has 1 unit summoned
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 1); // Check also back end for now
                Assert.AreEqual(sm.DetailedState.BoardState.GetLaneContainingTile(chosenTarget).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // Lane also contains 1 unit
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[chosenTarget].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // Tile also contains 1 unit
                // Check board hash has changed
                Assert.AreNotEqual(boardHash, sm.DetailedState.BoardState.GetBoardElementHashCode(sm.DetailedState.EntityData));
                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Reverted
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLaneContainingTile(chosenTarget).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[chosenTarget].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Tile also contains 1 unit
                // Check board hash has been properly reverted
                Assert.AreEqual(boardHash, sm.DetailedState.BoardState.GetBoardElementHashCode(sm.DetailedState.EntityData));
            }
        }

        [TestMethod]
        public void UnitsAreIndependent() // Manually modify a unit, and check if next unit s independent of modified
        // THIS IS A SANITY TEST not actual gameplay that would happen like this
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 1, 1, 1));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Insert token cards, 1 in all stats, summonable in any lane 
                }
                state.PlayerStates[playerIndex].CurrentGold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Will play one of them
                PlayContext options = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                int chosenTarget = TestHelperFunctions.GetRandomChoice(options.ValidTargets.ToList()); // Choose a random starting tile as target
                int unitCounter1 = sm.DetailedState.NextUniqueIndex;
                Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play it
                int unitCounter2 = sm.DetailedState.NextUniqueIndex;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Modify unit (shady)
                ((Unit)sm.DetailedState.EntityData[unitCounter1]).Attack.BaseValue += 5; // Add 5 to attack, whatever
                chosenTarget = TestHelperFunctions.GetRandomChoice(options.ValidTargets.ToList()); // Change target to a possibly different (valid) tile
                // Play new one
                res = sm.PlayFromHand(1, chosenTarget); // Play it
                int futureUnitCounter = sm.DetailedState.NextUniqueIndex;
                // Make sure card was played ok
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Check they're different
                Assert.AreNotEqual(unitCounter1, unitCounter2);
                Assert.AreEqual(futureUnitCounter - unitCounter2, 1); // have a diff of 1 too
                Assert.AreEqual(unitCounter2 - unitCounter1, 1); // have a diff of 1 too
                // Some stats should be similar, some should be different
                Assert.AreEqual(sm.DetailedState.EntityData[unitCounter2].Hp.Total, sm.DetailedState.EntityData[unitCounter1].Hp.Total);
                Assert.AreNotEqual(((Unit)sm.DetailedState.EntityData[unitCounter2]).Attack, ((Unit)sm.DetailedState.EntityData[unitCounter1]).Attack);
                // Finally, roll back!
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter2, sm.DetailedState.NextUniqueIndex); // And reverts properly
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 1);
                sm.UndoPreviousStep();
                Assert.AreEqual(unitCounter1, sm.DetailedState.NextUniqueIndex); // And reverts properly
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 0);
            }
        }

        [TestMethod]
        public void SummonedUnitDiesIf0Hp()
        {
            // Summons dead unit and verifies that the unit properly died
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int boardHash;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 0, 1, 1, 1));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Insert token cards, 1 in all stats but 0 HP, summonable in any lane 
                }
                state.PlayerStates[playerIndex].CurrentGold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                boardHash = sm.DetailedState.BoardState.GetBoardElementHashCode(sm.DetailedState.EntityData); // Store hash
                // Will play one of them
                PlayContext options = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                int chosenTarget = TestHelperFunctions.GetRandomChoice(options.ValidTargets.ToList());
                Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, chosenTarget); // Play it
                // Make sure card was played ok
                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(res.Item2);
                // Make sure unit has insta-died (nothing in field, 1 card in GY
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Player has no units
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 0); // Field has no units
                Assert.AreEqual(sm.DetailedState.BoardState.GetLaneContainingTile(chosenTarget).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Lane doesn't have the unit
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[chosenTarget].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Lane doesn't have the unit
                // Check board hash has not changed (as there's no GY)
                Assert.AreEqual(boardHash, sm.DetailedState.BoardState.GetBoardElementHashCode(sm.DetailedState.EntityData));
                // Now I revert!
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Player still has no units
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 0); // And field has no units
                Assert.AreEqual(sm.DetailedState.BoardState.GetLaneContainingTile(chosenTarget).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Lane doesn't have the unit
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[chosenTarget].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // Lane doesn't have the unit
                // Check board hash is still same
                Assert.AreEqual(boardHash, sm.DetailedState.BoardState.GetBoardElementHashCode(sm.DetailedState.EntityData));
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
                TileCoordinate = 2
            };
            u1.Hp.BaseValue = 10;
            u1.Movement.BaseValue = 10;
            u1.MovementDenominator.BaseValue = 10;
            u1.Attack.BaseValue = 10;
            u1.MvtCooldownTimer = 10;
            u2 = (Unit)u1.Clone();
            Assert.AreEqual(u1.GetHashCode(), u2.GetHashCode());
            // Now change a few things
            u2.Attack.BaseValue = 0;
            Assert.AreNotEqual(u1.GetHashCode(), u2.GetHashCode());
            // Revert
            u2.Attack = u1.Attack;
            Assert.AreEqual(u1.GetHashCode(), u2.GetHashCode());
        }

        //// UNITS ADVANCE TESTS
        [TestMethod]
        public void UnitAdvanceTest()
        {
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                int movement = _rng.Next(1, GameConstants.MOUNTAIN_NUMBER_OF_TILES); // Random movement unit, but want to make it so it never reaches castle
                CardFinder cardDb = new CardFinder();
                // Place unit in place, will try it in mountain (p0 in 10, p1 in 17)
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 1, movement, 1);
                TestHelperFunctions.ManualInitEntity(state, (playerIndex == 0) ? 10 : 17, 2, playerIndex, unit);
                // Start simulation
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                // Now I'm about to do the advance. Before the advance:
                Lane lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                int UnitHash = sm.DetailedState.EntityData[2].GetHashCode(); // Get Hash of unit
                int preAdvanceHash = sm.DetailedState.GetHashCode(); // Hash of game overall
                // Now advance! Ensure result of basic advance
                sm.EndTurn();
                lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                // Ensures the unit moved the right number of tiles in the right direction
                Assert.AreEqual(lane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, movement, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreNotEqual(preAdvanceHash, sm.DetailedState.GetHashCode()); // Hash of board has changed
                Assert.AreEqual(sm.DetailedState.EntityData[2].GetHashCode(), UnitHash); // However, the unit should be the same!
                // Finally revert the advance
                sm.UndoPreviousStep();
                lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, movement, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(preAdvanceHash, sm.DetailedState.GetHashCode());
                Assert.AreEqual(sm.DetailedState.EntityData[2].GetHashCode(), UnitHash);
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
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                // Place units in place, will try it in mountain (p0 in 10, p1 in 17)
                CardFinder cardDb = new CardFinder();
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 0, 9, 1); // Max movement bc it's stopped by enemy anyway
                int beginningCoord = (playerIndex == 0) ? 10 : 17; // Where a player's unit begins
                int intersectionCoord = 15; // Always the same intersection coord regardless
                TestHelperFunctions.ManualInitEntity(state, beginningCoord, 2, playerIndex, (Unit)unit.Clone());
                TestHelperFunctions.ManualInitEntity(state, intersectionCoord, 3, otherPlayerIndex, (Unit)unit.Clone());
                // Ok now I can begin simulation
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                int preAdvanceHash = sm.DetailedState.GetHashCode();
                Lane lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                // Both players have a unit
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[beginningCoord].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionCoord].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                // Advance...
                sm.EndTurn();
                // Verify
                lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                // Now unit is not in advance anymore and instead moved to intersect
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[beginningCoord].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionCoord].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionCoord].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreNotEqual(preAdvanceHash, sm.DetailedState.GetHashCode()); // Hash verif
                // Undo advance and verify again
                sm.UndoPreviousStep();
                lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                // Now unit is not in advance anymore and instead moved to intersect
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[beginningCoord].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionCoord].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionCoord].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(preAdvanceHash, sm.DetailedState.GetHashCode()); // Hash verif
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
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                CardFinder cardDb = new CardFinder();
                // Will try this in all lanes!
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 0, 9, 1);
                int firstPlains = state.BoardState.GetLane(LaneID.PLAINS).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int lastPlains = state.BoardState.GetLane(LaneID.PLAINS).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex);
                int firstForest = state.BoardState.GetLane(LaneID.FOREST).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int lastForest = state.BoardState.GetLane(LaneID.FOREST).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex);
                int firstMountains = state.BoardState.GetLane(LaneID.MOUNTAIN).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int lastMountains = state.BoardState.GetLane(LaneID.MOUNTAIN).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, firstPlains, 2, playerIndex, (Unit)unit.Clone());
                TestHelperFunctions.ManualInitEntity(state, firstForest, 3, playerIndex, (Unit)unit.Clone());
                TestHelperFunctions.ManualInitEntity(state, firstMountains, 4, playerIndex, (Unit)unit.Clone());
                // Rest of init
                state.PlayerStates[playerIndex].Hp.BaseValue = 30;
                state.PlayerStates[otherPlayerIndex].Hp.BaseValue = 30;
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Verify...
                Lane plains = sm.DetailedState.BoardState.GetLane(LaneID.PLAINS);
                Lane forest = sm.DetailedState.BoardState.GetLane(LaneID.FOREST);
                Lane mountain = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // Verify position of units, so that units are in first
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstPlains].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstForest].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstMountains].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // Now about to start my advance, no hash verif needed
                sm.EndTurn();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // After advance, units should be on opposite end of lane!
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[lastPlains].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[lastForest].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[lastMountains].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // After undo...
                sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstPlains].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstForest].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstMountains].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
            }
        }
        [TestMethod]
        public void AdvanceDenominatorTest()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                CardFinder cardDb = new CardFinder();
                // One unit in each lane
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 0, 1, 9);
                Unit unit1 = (Unit)unit.Clone();
                Unit unit2 = (Unit)unit.Clone();
                Unit unit3 = (Unit)unit.Clone();
                int firstPlains = state.BoardState.GetLane(LaneID.PLAINS).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int firstForest = state.BoardState.GetLane(LaneID.FOREST).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int firstMountains = state.BoardState.GetLane(LaneID.MOUNTAIN).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, firstPlains, 2, playerIndex, unit1);
                TestHelperFunctions.ManualInitEntity(state, firstForest, 3, playerIndex, unit2);
                TestHelperFunctions.ManualInitEntity(state, firstMountains, 4, playerIndex, unit3);
                // Start simulation
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Verify...
                Lane plains = sm.DetailedState.BoardState.GetLane(LaneID.PLAINS);
                Lane forest = sm.DetailedState.BoardState.GetLane(LaneID.FOREST);
                Lane mountain = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // Verify position of units, so that units are in first
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // Now, I modify the denominators
                unit1.MvtCooldownTimer = 0; // Will advance now
                unit2.MvtCooldownTimer = 8; // Won't advance but will do so in the following one
                unit3.MvtCooldownTimer = 1; // Not advancing in this test
                sm.EndTurn(); // Advance
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // Only plains should've moved
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                // Rest should still be in the first place
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                // Another advance sequence, only forest moves in this case
                sm.EndTurn();
                sm.EndTurn();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                // And the last one should remain the same...
                sm.EndTurn();
                sm.EndTurn();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                // Undo!
                sm.UndoPreviousStep(); sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                sm.UndoPreviousStep(); sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                sm.UndoPreviousStep(); sm.UndoPreviousStep();
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                Assert.AreEqual(plains.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(plains.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(forest.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(mountain.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
            }
        }
        //// Unit battle tests
        [TestMethod]
        public void TestUnitCombatNoDeath()
        {
            // Units clash, receive damage but both stay in lane. Damages are different to ensure token calculations are also ok
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int attack = _rng.Next(1, 8); // Attack between 1-7
                int hp = attack + 2; // So that both units survive
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                // Will try this in mountain as there's space!
                CardFinder cardDb = new CardFinder();
                Unit unit1 = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], hp, attack, 9, 1); // Gets to the end so it clashes
                Unit unit2 = TestCardGenerator.CreateUnit(2, "UNIT", 0, [0, 4, 10], hp, attack + 1, 3, 1); // Gets to the middle and waits there
                int firstTile = (playerIndex == 0) ? 10 : 17;
                int intersectionTile = 15;
                TestHelperFunctions.ManualInitEntity(state, firstTile, 2, playerIndex, unit1); // Player unit advances
                TestHelperFunctions.ManualInitEntity(state, intersectionTile, 3, otherPlayerIndex, unit2); // Opponen'ts is standing, intersects
                // Beginning of simulation
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Now, player's unit will advance! Pre advance, I check both units HP and count (2)
                Lane lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                // Both players have a unit
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 2); // 2 units total
                Assert.AreEqual(unit1.DamageTokens, 0); // No damage
                Assert.AreEqual(unit2.DamageTokens, 0); // No damage
                // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTile].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTile].GetPlacedEntities(EntityType.UNIT).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT).Count, 1);
                // Advance...
                sm.EndTurn();
                // Verify
                lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // Players still have their dudes
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 2); // 2 units total
                Assert.AreEqual(unit1.DamageTokens, attack + 1); // They have damage!
                Assert.AreEqual(unit2.DamageTokens, attack);
                // Now unit is not in advance anymore and instead moved to intersect
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTile].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT).Count, 2); // 2 Now
                // Undo advance and verify again
                sm.UndoPreviousStep();
                lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                // Both players have a unit
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 2); // 2 units total
                Assert.AreEqual(sm.DetailedState.EntityData[2].DamageTokens, 0); // No damage
                Assert.AreEqual(sm.DetailedState.EntityData[3].DamageTokens, 0); // No damage
                // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTile].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTile].GetPlacedEntities(EntityType.UNIT).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT).Count, 1);
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
                    GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                    state.CurrentState = States.ACTION_PHASE;
                    state.CurrentPlayer = 1 - player;
                    CardFinder cardDb = new CardFinder();
                    // Will try this in mountain as there's space!
                    Unit unit1 = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], hp, attackerStat, 9, 1); // Gets to the end so it clashes
                    Unit unit2 = TestCardGenerator.CreateUnit(2, "UNIT", 0, [0, 4, 10], hp, defenderStat, 3, 1); // Gets to the middle and waits there
                    int firstTile = (playerIndex == 0) ? 10 : 17;
                    int intersectionTile = 15;
                    TestHelperFunctions.ManualInitEntity(state, firstTile, 2, playerIndex, unit1); // Player unit advances
                    TestHelperFunctions.ManualInitEntity(state, intersectionTile, 3, otherPlayerIndex, unit2); // Opponen'ts is standing, intersects
                    // Begin simulation
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    // Now, player's unit will advance! Pre advance, I check both units HP (count has been checked in prev. tests)
                    Assert.AreEqual(unit1.DamageTokens, 0); // No damage
                    Assert.AreEqual(unit2.DamageTokens, 0); // No damage
                    // Advance...
                    sm.EndTurn();
                    // Verify
                    int attackerCount = (defenderExtraDmg[i] < 0) ? 1 : 0; // Attacker is there only if defender's weak
                    int defenderCount = (attackerExtraDmg[i] < 0) ? 1 : 0; // Defender is there only if attacker's weak
                    Lane lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, attackerCount); // Players still have their dudes
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, defenderCount);
                    Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, attackerCount);
                    Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, defenderCount);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, attackerCount + defenderCount); // 2 units total
                    if (defenderCount > 0)
                    {
                        Assert.AreEqual(unit2.DamageTokens, attackerStat); // Unit alive but damaged
                    }
                    else
                    {
                        Assert.IsFalse(sm.DetailedState.EntityData.ContainsKey(3)); // unit ded
                    }
                    if (attackerCount > 0)
                    {
                        Assert.AreEqual(unit1.DamageTokens, defenderStat); // Unit alive but damaged
                    }
                    else
                    {
                        Assert.IsFalse(sm.DetailedState.EntityData.ContainsKey(2)); // unit ded
                    }
                    // Now unit is not in advance anymore and instead moved to intersect
                    Assert.AreEqual(lane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, attackerCount);
                    Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, defenderCount);
                    Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT).Count, attackerCount + defenderCount); // 2 Now
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    lane = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN);
                    // Both players have a unit
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(lane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 2); // 2 units total
                    Assert.AreEqual(unit1.DamageTokens, 0); // No damage
                    Assert.AreEqual(unit2.DamageTokens, 0); // No damage
                    // Verify position of units. Player's is in the first, and opp's is in the intersection tile as they advanced already
                    Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTile].GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTile].GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.Tiles[intersectionTile].GetPlacedEntities(EntityType.UNIT).Count, 1);
                }
            }
        }

        [TestMethod]
        public void TestMultipleWithoutBreakingAdvance()
        {
            Random _rng = new Random();
            List<LaneID> lanes = [LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]; // Test interaction in all orders
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (LaneID lane in lanes)
            {
                int stat = _rng.Next(1, 10); // any Hp between 1-9
                foreach (CurrentPlayer player in players)
                {
                    int playerIndex = (int)player;
                    int otherPlayerIndex = 1 - playerIndex;
                    GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                    state.CurrentState = States.ACTION_PHASE;
                    state.CurrentPlayer = 1 - player;
                    CardFinder cardDb = new CardFinder();
                    // Inits the whole set
                    Unit playerUnit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], stat, stat, 1, 1); // Gets to the end so it clashes
                    Unit oppUnit = TestCardGenerator.CreateUnit(2, "UNIT", 0, [0, 4, 10], stat, stat, 0, 1); // Gets to the middle and waits there
                    Unit unit1 = (Unit)playerUnit.Clone();
                    Unit unit2 = (Unit)playerUnit.Clone();
                    Unit unit3 = (Unit)playerUnit.Clone();
                    int firstPlains = state.BoardState.GetLane(LaneID.PLAINS).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                    int firstForest = state.BoardState.GetLane(LaneID.FOREST).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                    int firstMountains = state.BoardState.GetLane(LaneID.MOUNTAIN).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                    TestHelperFunctions.ManualInitEntity(state, firstPlains, 2, playerIndex, unit1);
                    TestHelperFunctions.ManualInitEntity(state, firstForest, 3, playerIndex, unit2);
                    TestHelperFunctions.ManualInitEntity(state, firstMountains, 4, playerIndex, unit3);
                    int firstTileCoord = lane switch
                    {
                        LaneID.PLAINS => firstPlains,
                        LaneID.FOREST => firstForest,
                        LaneID.MOUNTAIN => firstMountains,
                        _ => throw new Exception("not valid for this test")
                    };
                    TestHelperFunctions.ManualInitEntity(state, firstTileCoord, 5, otherPlayerIndex, oppUnit);
                    // Begin test
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    // Now, player's unit will advance!
                    Lane theLane = sm.DetailedState.BoardState.GetLane(lane);
                    LaneID nextTgt = lane switch
                    {
                        LaneID.PLAINS => LaneID.FOREST,
                        LaneID.FOREST => LaneID.MOUNTAIN,
                        LaneID.MOUNTAIN => LaneID.PLAINS,
                        _ => throw new Exception("No more lanes")
                    };
                    Lane other1 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    nextTgt = nextTgt switch
                    {
                        LaneID.PLAINS => LaneID.FOREST,
                        LaneID.FOREST => LaneID.MOUNTAIN,
                        LaneID.MOUNTAIN => LaneID.PLAINS,
                        _ => throw new Exception("No more lanes")
                    };
                    Lane other2 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    Assert.AreEqual(other1.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other2.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 4); // 4 units total
                    // Verify position of units. One tile has 2 things and the others 1
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 2);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    // Advance...
                    sm.EndTurn();
                    // Verify
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    nextTgt = lane switch
                    {
                        LaneID.PLAINS => LaneID.FOREST,
                        LaneID.FOREST => LaneID.MOUNTAIN,
                        LaneID.MOUNTAIN => LaneID.PLAINS,
                        _ => throw new Exception("No more lanes")
                    };
                    other1 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    nextTgt = nextTgt switch
                    {
                        LaneID.PLAINS => LaneID.FOREST,
                        LaneID.FOREST => LaneID.MOUNTAIN,
                        LaneID.MOUNTAIN => LaneID.PLAINS,
                        _ => throw new Exception("No more lanes")
                    };
                    other2 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    // Check unit counts, similar but both players lost the unit in "the lane", the others advanced
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 2);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(other1.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other2.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 2);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // Make sure the others displaced properly
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    nextTgt = lane switch
                    {
                        LaneID.PLAINS => LaneID.FOREST,
                        LaneID.FOREST => LaneID.MOUNTAIN,
                        LaneID.MOUNTAIN => LaneID.PLAINS,
                        _ => throw new Exception("No more lanes")
                    };
                    other1 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    nextTgt = nextTgt switch
                    {
                        LaneID.PLAINS => LaneID.FOREST,
                        LaneID.FOREST => LaneID.MOUNTAIN,
                        LaneID.MOUNTAIN => LaneID.PLAINS,
                        _ => throw new Exception("No more lanes")
                    };
                    other2 = sm.DetailedState.BoardState.GetLane(nextTgt);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    Assert.AreEqual(other1.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other2.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 4); // 4 units total
                    // Verify position of units. One tile has 2 things and the others 1
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 2);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(other1.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(other2.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                }
            }
        }

        [TestMethod]
        public void TestMultipleTradesInTile()
        {
            Random _rng = new Random();
            List<LaneID> lanes = [LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]; // Test interaction in all orders
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (LaneID lane in lanes)
            {
                int stat = _rng.Next(1, 10); // any Hp between 1-9
                foreach (CurrentPlayer player in players)
                {
                    int playerIndex = (int)player;
                    int otherPlayerIndex = 1 - playerIndex;
                    GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                    state.CurrentState = States.ACTION_PHASE;
                    state.CurrentPlayer = 1 - player;
                    CardFinder cardDb = new CardFinder();
                    // Inits all
                    Unit oppUnit = TestCardGenerator.CreateUnit(2, "UNIT", 0, [0, 4, 10], stat, stat, 0, 1); // Gets to the middle and waits there
                    Unit playerUnit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], stat, stat, 1, 1); // Gets to the end so it clashes
                    Unit unit1 = (Unit)playerUnit.Clone();
                    Unit unit2 = (Unit)playerUnit.Clone();
                    Unit unit3 = (Unit)playerUnit.Clone();
                    Unit unit4 = (Unit)oppUnit.Clone();
                    Unit unit5 = (Unit)oppUnit.Clone();
                    int theTile = lane switch
                    {
                        LaneID.PLAINS => state.BoardState.GetLane(LaneID.PLAINS).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex),
                        LaneID.FOREST => state.BoardState.GetLane(LaneID.FOREST).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex),
                        LaneID.MOUNTAIN => state.BoardState.GetLane(LaneID.MOUNTAIN).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex),
                        _ => throw new Exception("not valid for this test")
                    };
                    TestHelperFunctions.ManualInitEntity(state, theTile, 2, otherPlayerIndex, unit4);
                    TestHelperFunctions.ManualInitEntity(state, theTile, 3, otherPlayerIndex, unit5);
                    TestHelperFunctions.ManualInitEntity(state, theTile, 4, playerIndex, unit1);
                    TestHelperFunctions.ManualInitEntity(state, theTile, 5, playerIndex, unit2);
                    TestHelperFunctions.ManualInitEntity(state, theTile, 6, playerIndex, unit3);
                    // Start sim
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    // Now, player's unit will advance!
                    Lane theLane = sm.DetailedState.BoardState.GetLane(lane);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 2);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 2);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 5); // 5 units total
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 2);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 5);
                    // Only for this, also check ids
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(2)); // hmm I mean I know these are the indices but maybe I shouldn't
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(3));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(4));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(5));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(6));
                    // Advance...
                    sm.EndTurn();
                    // Verify
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 1); // 1 unit total
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // in the next tile!
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 0);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.IsFalse(sm.DetailedState.EntityData.ContainsKey(2));
                    Assert.IsFalse(sm.DetailedState.EntityData.ContainsKey(3));
                    Assert.IsFalse(sm.DetailedState.EntityData.ContainsKey(4));
                    Assert.IsFalse(sm.DetailedState.EntityData.ContainsKey(5));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(6)); // Only one that remains
                    Assert.AreEqual(sm.DetailedState.EntityData[6].DamageTokens, 0); // undamaged too
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    theLane = sm.DetailedState.BoardState.GetLane(lane);
                    // Check unit counts
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 2);
                    // Check the right amount in all lanes
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(theLane.GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 2);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 5); // 5 units total
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 3);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 2);
                    Assert.AreEqual(theLane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 5);
                    // Only for this, also check ids
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(0));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(1));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(2));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(3));
                    Assert.IsTrue(sm.DetailedState.EntityData.ContainsKey(4));
                }
            }
        }
        //// Direct damage tests
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
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                state.PlayerStates[playerIndex].Hp.BaseValue = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp.BaseValue = GameConstants.STARTING_HP;
                // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, attack, 9, 1);
                LaneID target = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target, it doesn't really matter
                int tileInitial = state.BoardState.GetLane(target).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, tileInitial, 2, playerIndex, unit); // Unit in place
                // Begin sim
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                sm.PlayFromHand(1, tileInitial); // Play the unit
                // Now the unit is ready to advance, check before and after
                Player ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetHashCode();
                Player ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetHashCode();
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // 1 unit in tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // From correct player
                // Player's unit will advance!
                sm.EndTurn();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.DamageTokens, attack); // Now player 2 has less Hp
                Assert.AreNotEqual(ps1Hash, ps1.GetHashCode()); // Because they drew card
                Assert.AreNotEqual(ps2Hash, ps2.GetHashCode());
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE); // Still in action phase, not EOG
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // 1 unit in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // From correct player
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps1Hash, ps1.GetHashCode());
                Assert.AreEqual(ps2Hash, ps2.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // Unit back at beginning
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
            }
        }
        [TestMethod]
        public void UnitBlockedFromDirectDamage()
        {
            // A unit advances, and will damage enemy
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                state.PlayerStates[playerIndex].Hp.BaseValue = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp.BaseValue = GameConstants.STARTING_HP;
                // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 2, 1, 9, 1);
                LaneID target = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target, it doesn't really matter
                int tileInitial = state.BoardState.GetLane(target).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int tileFinal = state.BoardState.GetLane(target).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, tileInitial, 2, playerIndex, (Unit)unit.Clone()); // Unit in place
                TestHelperFunctions.ManualInitEntity(state, tileFinal, 3, otherPlayerIndex, (Unit)unit.Clone()); // Opp unit in place
                // Add some cards to players to avoid deckout
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                //Begin sim
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Now the unit is ready to advance, will collide with enemy tho
                Player ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetHashCode();
                Player ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetHashCode();
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // Check both units
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // Ensure enemy blocks last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
                // Player's unit will advance!
                sm.EndTurn();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, GameConstants.STARTING_HP); // This time the opp is undamaged
                Assert.AreNotEqual(ps1Hash, ps1.GetHashCode()); // Players should have the same hash as their situation hasn't changed? (No draw cards etc)
                Assert.AreEqual(ps2Hash, ps2.GetHashCode());
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE); // Still in action phase, not EOG
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 2); // 2 now in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // From both players
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1); // From both players
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps1Hash, ps1.GetHashCode());
                Assert.AreEqual(ps2Hash, ps2.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, otherPlayerIndex).Count, 1);
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
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                state.PlayerStates[playerIndex].Hp.BaseValue = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp.BaseValue = attack; // Opp has less HP this time
                // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, attack, 9, 1);
                LaneID target = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target, it doesn't really matter
                int tileInitial = state.BoardState.GetLane(target).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, tileInitial, 2, playerIndex, (Unit)unit.Clone()); // Unit in place
                // Add some cards to avoid deckout damage
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                // Begin simulation
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Now the unit is ready to advance, check before and after
                Player ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetHashCode();
                Player ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetHashCode();
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, attack);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // 1 unit in tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // From correct player
                // Player's unit will advance!
                sm.EndTurn();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, ps2.DamageTokens); // Now player 2 is dead
                Assert.AreEqual(ps1Hash, ps1.GetHashCode()); // Because the game ends during advance without drawing card!!!!
                Assert.AreNotEqual(ps2Hash, ps2.GetHashCode()); // Because they are dead
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, player); // Indicates player who won (player for now can only win in their turn so this is pointless)
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // 1 unit in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // From correct player
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, attack);
                Assert.AreEqual(ps1Hash, ps1.GetHashCode());
                Assert.AreEqual(ps2Hash, ps2.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // Unit back at beginning
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
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
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                state.PlayerStates[playerIndex].Hp.BaseValue = GameConstants.STARTING_HP; // It's important to set this
                state.PlayerStates[otherPlayerIndex].Hp.BaseValue = attack - 1; // Opp has less HP this time
                // Will try this in any lane
                CardFinder cardDb = new CardFinder();
                Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, attack, 9, 1);
                LaneID target = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target, it doesn't really matter
                int tileInitial = state.BoardState.GetLane(target).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, tileInitial, 2, playerIndex, (Unit)unit.Clone()); // Unit in place
                // Add some cards to avoid deckout damage
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                    state.PlayerStates[playerIndex].Deck.InsertCard(1);
                    state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(1);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                }
                // Begin sim
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Now the unit is ready to advance, check before and after
                Player ps1 = sm.DetailedState.PlayerStates[playerIndex];
                int ps1Hash = ps1.GetHashCode();
                Player ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                int ps2Hash = ps2.GetHashCode();
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, attack - 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // 1 unit in tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // From correct player
                // Player's unit will advance!
                sm.EndTurn();
                // Post advance check of same things
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, ps2.DamageTokens); // Now player 2 is dead
                Assert.AreEqual(ps1Hash, ps1.GetHashCode()); // Because the game ends during advance without drawing card!!!!
                Assert.AreNotEqual(ps2Hash, ps2.GetHashCode()); // Because they are dead
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, player); // Indicates player who won (player for now can only win in their turn so this is pointless)
                // Unit positioning is coherent
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0); // No more unit in first tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // 1 unit in last tile
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // From correct player
                // Undo advance and verify again
                sm.UndoPreviousStep();
                ps1 = sm.DetailedState.PlayerStates[playerIndex];
                ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                Assert.AreEqual(ps2.Hp.Total, attack - 1);
                Assert.AreEqual(ps1Hash, ps1.GetHashCode());
                Assert.AreEqual(ps2Hash, ps2.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1); // Unit back at beginning
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0);
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
                    GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                    state.CurrentState = States.ACTION_PHASE;
                    state.CurrentPlayer = 1 - player;
                    state.PlayerStates[playerIndex].Hp.BaseValue = GameConstants.STARTING_HP; // It's important to set this
                    state.PlayerStates[otherPlayerIndex].Hp.BaseValue = 6; // Opp has 6HP which is pretty handy for this test
                    // In all lanes, I summon units
                    CardFinder cardDb = new CardFinder();
                    Unit unit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, attack, 9, 1);
                    int plainsInitial = state.BoardState.GetLane(LaneID.PLAINS).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                    int forestInitial = state.BoardState.GetLane(LaneID.FOREST).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                    int mountainInitial = state.BoardState.GetLane(LaneID.MOUNTAIN).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                    TestHelperFunctions.ManualInitEntity(state, plainsInitial, 2, playerIndex, (Unit)unit.Clone()); // Unit in place
                    TestHelperFunctions.ManualInitEntity(state, forestInitial, 3, playerIndex, (Unit)unit.Clone()); // Unit in place
                    TestHelperFunctions.ManualInitEntity(state, mountainInitial, 4, playerIndex, (Unit)unit.Clone()); // Unit in place
                    // Add some cards to avoid deckout damage
                    for (int i = 0; i < 5; i++)
                    {
                        // Insert to both players hands and decks. Both have attack but they can't kill themselves
                        state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                        state.PlayerStates[playerIndex].Deck.InsertCard(1);
                        state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(1);
                        state.PlayerStates[otherPlayerIndex].Deck.InsertCard(1);
                    }
                    // Begin sim
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    // Now untis are ready to advance, check before and after
                    Player ps1 = sm.DetailedState.PlayerStates[playerIndex];
                    int ps1Hash = ps1.GetHashCode();
                    Player ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                    int ps2Hash = ps2.GetHashCode();
                    Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                    Assert.AreEqual(ps2.Hp.Total, 6);
                    // Get units in all lanes
                    int plainsHash = sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetBoardElementHashCode(sm.DetailedState.EntityData);
                    int forestHash = sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetBoardElementHashCode(sm.DetailedState.EntityData);
                    int mountainHash = sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetBoardElementHashCode(sm.DetailedState.EntityData);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    // Player's unit will advance!
                    sm.EndTurn();
                    // Post advance check of same things, this will end game and units will be frozen in time
                    ps1 = sm.DetailedState.PlayerStates[playerIndex];
                    ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                    Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                    Assert.AreEqual(ps2.Hp.Total, ps2.DamageTokens); // Now player 2 is dead
                    Assert.AreEqual(ps1Hash, ps1.GetHashCode()); // Because the game ends during advance without drawing card!!!!
                    Assert.AreNotEqual(ps2Hash, ps2.GetHashCode()); // Because they are dead
                    Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                    Assert.AreEqual(sm.DetailedState.CurrentPlayer, player); // Indicates player who won (player for now can only win in their turn so this is pointless)
                    // Unit positioning is coherent and hashes are. This depends on the unit that kills
                    Assert.AreEqual(unitThatKills < 1, plainsHash == sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetBoardElementHashCode(sm.DetailedState.EntityData)); // Hash unchanged if unit doesn't advance
                    Assert.AreEqual(unitThatKills < 2, forestHash == sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetBoardElementHashCode(sm.DetailedState.EntityData));
                    Assert.AreEqual(unitThatKills < 3, mountainHash == sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetBoardElementHashCode(sm.DetailedState.EntityData));
                    // Unit will be in first tile or in last depending advance
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, (unitThatKills >= 1) ? -1 : 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, (unitThatKills >= 1) ? -1 : 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, (unitThatKills >= 2) ? -1 : 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, (unitThatKills >= 2) ? -1 : 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, (unitThatKills >= 3) ? -1 : 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, (unitThatKills >= 3) ? -1 : 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    // Undo advance and verify again
                    sm.UndoPreviousStep();
                    ps1 = sm.DetailedState.PlayerStates[playerIndex];
                    ps2 = sm.DetailedState.PlayerStates[otherPlayerIndex];
                    Assert.AreEqual(ps1.Hp.Total, GameConstants.STARTING_HP);
                    Assert.AreEqual(ps2.Hp.Total, 6);
                    // Get units in all lanes
                    Assert.AreEqual(plainsHash, sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetBoardElementHashCode(sm.DetailedState.EntityData));
                    Assert.AreEqual(forestHash, sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetBoardElementHashCode(sm.DetailedState.EntityData));
                    Assert.AreEqual(mountainHash, sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetBoardElementHashCode(sm.DetailedState.EntityData));
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.PLAINS).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.FOREST).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT).Count, 1);
                    Assert.AreEqual(sm.DetailedState.BoardState.GetLane(LaneID.MOUNTAIN).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex).GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                }
            }
        }
        [TestMethod]
        public void BuildingDamagedOnSummon()
        {
            // Unit summoned on top of building, damages building
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Will try this in any lane
                LaneID target = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.AddToCollection(0);
                    state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Add unit too
                    state.PlayerStates[playerIndex].Deck.InsertCard(0);
                    state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(0);
                    state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(1); // Add unit too
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(0);
                }
                Unit testUnit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 0, 2, 1);
                CardFinder cardDb = new CardFinder(); // Card holder
                cardDb.InjectCard(1, testUnit); // Add to cardDb
                Building testBldg = TestCardGenerator.CreateBuilding(2, "BUILDING", 0, [0, 4, 10], 2);
                testBldg.Owner = otherPlayerIndex;
                // Initialize building in first tile
                int firstTileCoord = state.BoardState.GetLane(target).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, firstTileCoord, 2, otherPlayerIndex, testBldg); // Insert building in field, in beginning of player
                state.NextUniqueIndex = 3;
                // Create a new blank SM from load game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state);
                // Pre summon
                state = sm.DetailedState;
                int hash = state.GetHashCode();
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // No units
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1); // A building
                // Summon
                sm.PlayFromHand(1, firstTileCoord); // Player will summon the unit
                Assert.AreNotEqual(hash, sm.DetailedState.GetHashCode()); // Hash changed
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1); // Now unit
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1); // Now building
                Assert.AreEqual(testBldg.DamageTokens, 1); // Bldg has now damage tokens
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(hash, sm.DetailedState.GetHashCode()); // Hash reverted
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 0); // No unit
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(testBldg.DamageTokens, 0);
            }
        }
        [TestMethod]
        public void BuildingDamagedOnAdvance()
        {
            // A unit advances, damages building
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                // Will try this in any lane
                LaneID target = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.AddToCollection(0);
                    state.PlayerStates[playerIndex].Deck.InsertCard(0);
                    state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(0);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(0);
                }
                Unit testUnit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 0, 2, 1);
                Building testBldg = TestCardGenerator.CreateBuilding(2, "BUILDING", 0, [0, 4, 10], 2);
                Lane lane = state.BoardState.GetLane(target);
                int firstTile = lane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int secondTile = lane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, firstTile, 2, playerIndex, testUnit);
                TestHelperFunctions.ManualInitEntity(state, secondTile, 3, otherPlayerIndex, testBldg);
                // Begin sim
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state);
                // Pre advance
                state = sm.DetailedState;
                int hash = state.GetHashCode();
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.BUILDING).First(), testBldg.UniqueId);
                // Advance
                sm.EndTurn();
                Assert.AreNotEqual(hash, sm.DetailedState.GetHashCode()); // Hash changed
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(testBldg.DamageTokens, 1); // Bldg has now damage tokens
                Assert.AreEqual(state.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.BUILDING).First(), testBldg.UniqueId);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(hash, sm.DetailedState.GetHashCode()); // Hash changed
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(testBldg.DamageTokens, 0);
                Assert.AreEqual(state.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.BUILDING).First(), testBldg.UniqueId);
            }
        }
        [TestMethod]
        public void BuildingKilledOnAdvance()
        {
            // A unit advances, destorys building
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - player;
                // Will try this in any lane
                LaneID target = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target, it doesn't really matter
                for (int i = 0; i < 5; i++)
                {
                    // Insert to both players hands and decks. Both have attack but they can't kill themselves
                    state.PlayerStates[playerIndex].Hand.AddToCollection(0);
                    state.PlayerStates[playerIndex].Deck.InsertCard(0);
                    state.PlayerStates[otherPlayerIndex].Hand.AddToCollection(0);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertCard(0);
                }
                Unit testUnit = TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 0, 2, 1);
                Building testBldg = TestCardGenerator.CreateBuilding(2, "BUILDING", 0, [0, 4, 10], 1);
                Lane lane = state.BoardState.GetLane(target);
                int firstTile = lane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int secondTile = lane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex);
                TestHelperFunctions.ManualInitEntity(state, firstTile, 2, playerIndex, testUnit);
                TestHelperFunctions.ManualInitEntity(state, secondTile, 3, otherPlayerIndex, testBldg);
                // Create blank SM
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state);
                // Pre advance
                state = sm.DetailedState;
                int hash = state.GetHashCode();
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                // Advance
                sm.EndTurn();
                Assert.AreNotEqual(hash, sm.DetailedState.GetHashCode()); // Hash changed
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 0);
                Assert.AreEqual(state.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 0);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(hash, sm.DetailedState.GetHashCode()); // Hash changed
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, playerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, otherPlayerIndex).Count, 1);
                Assert.AreEqual(state.BoardState.GetLane(target).GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex).GetPlacedEntities(EntityType.BUILDING).First(), testBldg.UniqueId);
            }
        }
    }
}
