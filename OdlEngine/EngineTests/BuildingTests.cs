using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class BuildingTests
    {
        // Blueprint targetability
        [TestMethod]
        public void VerifyBuildingNonTargetabilityEmptyBlueprints()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that cant be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, PlayTargetLocation.ALL_LANES, 1, [], [], []));
                for (int i = 0; i < 10; i++)
                {
                    // Insert useless building in hand. Building wouldn't have valid targets
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane because there's no target
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(optionRes.PlayTarget, PlayTargetLocation.INVALID); // Bc invalid...
                PlayTargetLocation[] targetTest = [PlayTargetLocation.PLAINS, PlayTargetLocation.FOREST, PlayTargetLocation.MOUNTAIN];
                foreach (PlayTargetLocation target in targetTest)
                {
                    Tuple<PlayContext, StepResult> playRes = sm.PlayFromHand(1, target);
                    Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
            }
        }
        [TestMethod]
        public void VerifyNonPlayabilityBecauseNoUnit()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be built anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, PlayTargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(optionRes.PlayTarget, PlayTargetLocation.INVALID); // Bc invalid...
                PlayTargetLocation[] targetTest = [PlayTargetLocation.PLAINS, PlayTargetLocation.FOREST, PlayTargetLocation.MOUNTAIN];
                foreach (PlayTargetLocation target in targetTest)
                {
                    Tuple<PlayContext, StepResult> playRes = sm.PlayFromHand(1, target);
                    Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
            }
        }
        [TestMethod]
        public void VerifyPlayabilityOnceTheresUnit()
        {
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, PlayTargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Basic unit
                Unit unit = TestCardGenerator.CreateUnit(2, "UNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Card should not be playable in any lane, but because it's missing the unit. This is proven in another test...
                // ...but now I inject unit in plains, and building should be playable in plains only
                int plainsCoord = sm.DetailedState.BoardState.PlainsLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_LANE, _rng.Next(GameConstants.PLAINS_NUMBER_OF_TILES)); // Get lane's random tile. Buildings BP should make it buildable anywhere so this should never fail
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, plainsCoord, -1, playerIndex, unit); // Add unit (will use negative ids not to interfere with the building id)
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND); // What happens if I attempt to play building from hand?
                Assert.AreEqual(res.PlayOutcome, PlayOutcome.OK);
                Assert.IsTrue(res.PlayTarget.HasFlag(PlayTargetLocation.PLAINS)); // Would be playable in lane and none other
                Assert.IsFalse(res.PlayTarget.HasFlag(PlayTargetLocation.FOREST));
                Assert.IsFalse(res.PlayTarget.HasFlag(PlayTargetLocation.MOUNTAIN));
                /// Helper function, attempts to build a building in any lane, and asserts it builds only in wouldBeValidTarget 
                void TryBuild(PlayTargetLocation wouldBeValidTarget)
                {
                    PlayTargetLocation[] targetTest = [PlayTargetLocation.PLAINS, PlayTargetLocation.FOREST, PlayTargetLocation.MOUNTAIN];
                    foreach (PlayTargetLocation target in targetTest)
                    {
                        int prePlayHash = sm.DetailedState.GetHashCode();
                        Tuple<PlayContext, StepResult> playRes = sm.PlayFromHand(1, target);
                        if (wouldBeValidTarget.HasFlag(target)) // if target is correct
                        {
                            // Building should've played ok
                            Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.OK);
                            Assert.AreEqual(playRes.Item1.PlayTarget, target);
                            Assert.IsNotNull(playRes.Item2);
                            Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash should've changed
                            Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Player has building
                            Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Lane has building
                            // Revert this, assert reversion
                            sm.UndoPreviousStep();
                            Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                            Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                            Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                        }
                        else
                        {
                            Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                            Assert.AreEqual(playRes.Item1.PlayTarget, PlayTargetLocation.INVALID);
                            Assert.IsNull(playRes.Item2); // Bc invalid...
                        }
                    }
                }
                TryBuild(PlayTargetLocation.PLAINS);
                // Same in forest
                int forestCoord = sm.DetailedState.BoardState.ForestLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_LANE, _rng.Next(GameConstants.FOREST_NUMBER_OF_TILES));
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, forestCoord, -2, playerIndex, unit);
                res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.PlayOutcome, PlayOutcome.OK);
                Assert.IsTrue(res.PlayTarget.HasFlag(PlayTargetLocation.PLAINS));
                Assert.IsTrue(res.PlayTarget.HasFlag(PlayTargetLocation.FOREST));
                Assert.IsFalse(res.PlayTarget.HasFlag(PlayTargetLocation.MOUNTAIN));
                TryBuild(PlayTargetLocation.ALL_BUT_MOUNTAIN);
                // Finally in mountain
                int mountainCoord = sm.DetailedState.BoardState.MountainLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_LANE, _rng.Next(GameConstants.MOUNTAIN_NUMBER_OF_TILES));
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, mountainCoord, -3, playerIndex, unit);
                res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.PlayOutcome, PlayOutcome.OK);
                Assert.IsTrue(res.PlayTarget.HasFlag(PlayTargetLocation.PLAINS));
                Assert.IsTrue(res.PlayTarget.HasFlag(PlayTargetLocation.FOREST));
                Assert.IsTrue(res.PlayTarget.HasFlag(PlayTargetLocation.MOUNTAIN));
                TryBuild(PlayTargetLocation.ALL_LANES);
                // Old reversions are ignored because they involved reversions of unit playing, which are done elsewhere
            }
        }
        [TestMethod]
        public void BuildingDiesIfBuiltAt0Hp()
        {
            // Summons dead building and verifies that it died
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                // Init game state
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere but has 0 hp
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, PlayTargetLocation.ALL_LANES, 0, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Basic unit
                Unit unit = TestCardGenerator.CreateUnit(2, "UNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayTargetLocation laneTarget = (PlayTargetLocation)(1 << _rng.Next(3)); // Random lane target
                // Play unit in lane
                int tileCoord = sm.DetailedState.BoardState.GetLane(laneTarget).FirstTileIndexOffset; // Get a coord from this lane idc
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, tileCoord, -1, playerIndex, unit); // Add unit (will use negative ids not to interfere with the building id)
                // Check my building will be buildable
                PlayContext optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.OK);
                Assert.AreEqual(optionRes.PlayTarget, laneTarget);
                // Pre play, ensure building's not there
                int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode();
                int prePlayStateHash = sm.DetailedState.GetHashCode();
                int nextEntityIndex = sm.DetailedState.NextUniqueIndex;
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                // Now I play the building
                sm.PlayFromHand(1, laneTarget);
                // Post play, building should STILL not be there because it insta-died
                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Board shouldn't have changed at all
                Assert.AreNotEqual(prePlayStateHash, sm.DetailedState.GetHashCode()); // Gamestate definitely changed because hands changed, unit, etc
                Assert.AreNotEqual(nextEntityIndex, sm.DetailedState.NextUniqueIndex); // Also ensure building was at some point instantiated
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                // Finally, revert
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Board shouldn't have changed at all
                Assert.AreEqual(prePlayStateHash, sm.DetailedState.GetHashCode()); // Gamestate definitely changed because hands changed, unit, etc
                Assert.AreEqual(nextEntityIndex, sm.DetailedState.NextUniqueIndex); // Also ensure building was at some point instantiated
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
            }
        }
        [TestMethod]
        public void CantBuildOnTopOfBuiding()
        {
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, PlayTargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Basic unit
                Unit unit = TestCardGenerator.CreateUnit(2, "UNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayTargetLocation laneTarget = (PlayTargetLocation)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                int tileCoord = sm.DetailedState.BoardState.GetLane(laneTarget).FirstTileIndexOffset; // Get a coord from this lane idc
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, tileCoord, -1, playerIndex, unit); // Add unit (will use negative ids not to interfere with the building id)
                // Check my building will be buildable
                PlayContext optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.OK);
                Assert.AreEqual(optionRes.PlayTarget, laneTarget);
                // Now I play the building
                sm.PlayFromHand(1, laneTarget);
                // Check if same building is buildable (shouldn't be, no available target)
                optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(optionRes.PlayTarget, PlayTargetLocation.INVALID);
                // Try build anyway
                Tuple<PlayContext, StepResult> playRes = sm.PlayFromHand(1, laneTarget);
                Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.IsNull(playRes.Item2);
            }
        }
        [TestMethod]
        public void CorrectOptionOrderAndBuildingSequence()
        {
            // Units in tiles 1, 2. uilding will be built first in 1 then in 2
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, PlayTargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Basic unit
                Unit unit = TestCardGenerator.CreateUnit(2, "UNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayTargetLocation laneTarget = (PlayTargetLocation)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                int firstTileCoord = sm.DetailedState.BoardState.GetLane(laneTarget).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex); // Get 1st coord
                int secondTileCoord = sm.DetailedState.BoardState.GetLane(laneTarget).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 1, playerIndex); // Get 1st coord
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, firstTileCoord, -1, playerIndex, unit); // Add unit (will use negative ids not to interfere with the building id)
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, secondTileCoord, -2, playerIndex, unit);
                int prePlayHash1 = sm.DetailedState.GetHashCode();
                // Now I play the building
                Tuple<PlayContext, StepResult> playRes = sm.PlayFromHand(1, laneTarget);
                Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(playRes.Item2);
                Assert.AreEqual(((ConstructionContext)(playRes.Item1.LastAuxContext)).AbsoluteConstructionTile, firstTileCoord); // Verify building was first built in the first coord
                int prePlayHash2 = sm.DetailedState.GetHashCode();
                Assert.AreNotEqual(prePlayHash1, prePlayHash2); // Hash should've changed
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Player has building
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Lane has building
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Tile has building
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[secondTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0); // But not the second tile yet
                // Play second building
                playRes = sm.PlayFromHand(1, laneTarget);
                Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.OK);
                Assert.IsNotNull(playRes.Item2);
                Assert.AreEqual(((ConstructionContext)(playRes.Item1.LastAuxContext)).AbsoluteConstructionTile, secondTileCoord); // Verify building was built in the second coord
                Assert.AreNotEqual(prePlayHash1, sm.DetailedState.GetHashCode()); // Hash should've changed
                Assert.AreNotEqual(prePlayHash2, sm.DetailedState.GetHashCode()); // Hash should've changed
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 2); // Player has 2 buildings
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 2); // Lane has 2 buildings
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Both tiles have
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[secondTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1);
                // Revert 2nd Building
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash2, sm.DetailedState.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Player has 1 building
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Lane has building
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Tile has building
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[secondTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0); // But not the second tile yet
                // Revert first building
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash1, sm.DetailedState.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[firstTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.Tiles[secondTileCoord].GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
            }
        }
        [TestMethod]
        public void HashTest()
        {
            Building b1, b2;
            b1 = new Building()
            {
                UniqueId = 1,
                Owner = 0,
                TileCoordinate = 2
            };
            b1.Hp.BaseValue = 10;
            b2 = (Building)b1.Clone();
            Assert.AreEqual(b1.GetHashCode(), b2.GetHashCode());
            // Now change a few things
            b2.Hp.BaseValue = 1;
            Assert.AreNotEqual(b1.GetHashCode(), b2.GetHashCode());
            // Revert
            b2.Hp.BaseValue = b1.Hp.BaseValue;
            Assert.AreEqual(b1.GetHashCode(), b2.GetHashCode());
        }
    }
}
