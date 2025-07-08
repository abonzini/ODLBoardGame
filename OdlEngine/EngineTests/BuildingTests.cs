using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class BuildingTests
    {
        [TestMethod]
        public void BuildingDiesIfBuiltAt0Hp()
        {
            // Summons dead building and verifies that it died
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
                HashSet<int> allTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    allTiles.Add(i);
                }
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, allTiles, 0));
                // Basic unit
                Unit unit = TestCardGenerator.CreateUnit(2, "UNIT", 0, allTiles, 1, 1, 1, 1);
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                LaneID laneTarget = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target
                // Play unit in lane
                int tileCoord = sm.DetailedState.BoardState.GetLane(laneTarget).FirstTileIndexOffset; // Get a coord from this lane idc
                int unitId = 300; // Will use high ids not to interfere with the building id
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, tileCoord, unitId, playerIndex, unit); // Add unit
                // Check my building will be buildable
                PlayContext optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.OK);
                Assert.AreEqual(1, optionRes.ValidTargets.Count);
                Assert.AreEqual(unitId, optionRes.ValidTargets.First()); // Target is the unit (negative) id
                // Pre play, ensure building's not there
                int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode();
                int prePlayStateHash = sm.DetailedState.GetHashCode();
                int nextEntityIndex = sm.DetailedState.NextUniqueIndex;
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                // Now I play the building
                sm.PlayFromHand(1, unitId);
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
                HashSet<int> allTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    allTiles.Add(i);
                }
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, allTiles, 1));
                // Basic unit
                Unit unit = TestCardGenerator.CreateUnit(2, "UNIT", 0, allTiles, 1, 1, 1, 1);
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                LaneID laneTarget = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]); // Random lane target
                // Play unit in lane
                int tileCoord = sm.DetailedState.BoardState.GetLane(laneTarget).FirstTileIndexOffset; // Get a coord from this lane idc
                int unitId = 300; // Will use high ids not to interfere with the building id
                TestHelperFunctions.ManualInitEntity(sm.DetailedState, tileCoord, unitId, playerIndex, unit); // Add unit
                // Check my building will be buildable
                PlayContext optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.OK);
                Assert.AreEqual(1, optionRes.ValidTargets.Count);
                Assert.AreEqual(unitId, optionRes.ValidTargets.First());
                // Now I play the building
                sm.PlayFromHand(1, unitId);
                // Check if same building is buildable (shouldn't be, no available target)
                optionRes = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(0, optionRes.ValidTargets.Count);
                // Try build anyway
                Tuple<PlayContext, StepResult> playRes = sm.PlayFromHand(1, unitId);
                Assert.AreEqual(playRes.Item1.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.IsNull(playRes.Item2);
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
