using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void AbsoluteTriggerTesting()
        {
            // Subscribes a unit to all absolute locations combinations and then triggers different places to ensure trigger works as intended
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that can trigger anywhere and places debug event in pile
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, [0, 3, 4, 9, 10, 17], 1, 0, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Means that when triggered, it'll push the debug effect
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test loop, iterate 0b0000-0b1111 to register triggers in up to 32 combinations
                for (int i = 0; i < 0b1111; i++)
                {
                    unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                    // Attach triggers if i need them
                    if ((i & 0b0001) != 0) // Board check
                        unit.Triggers.Add(EffectLocation.BOARD, triggerEffect);
                    if ((i & 0b0010) != 0) // Plains check
                        unit.Triggers.Add(EffectLocation.PLAINS, triggerEffect);
                    if ((i & 0b0100) != 0) // Forest check
                        unit.Triggers.Add(EffectLocation.FOREST, triggerEffect);
                    if ((i & 0b1000) != 0) // Mountain check
                        unit.Triggers.Add(EffectLocation.MOUNTAIN, triggerEffect);
                    // Got the unit!
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    sm.PlayFromHand(1, 0); // Play somewhere (doesn't matter)
                    int postPlayHash = sm.DetailedState.GetHashCode();
                    Assert.AreNotEqual(prePlayHash, postPlayHash);
                    // TRIGGERING LOOP, will trigger absolutely everyhwere!
                    EffectLocation[] locationsToProbe = [EffectLocation.BOARD, EffectLocation.PLAINS, EffectLocation.FOREST, EffectLocation.MOUNTAIN];
                    foreach (EffectLocation location in locationsToProbe) // Next location to test
                    {
                        // Play, test active trigger
                        StepResult res = sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, location, new EffectContext());
                        // Check if should've triggered
                        bool shouldHaveTriggered = false;
                        if (location == EffectLocation.BOARD && (i & 0b0001) != 0) shouldHaveTriggered = true;
                        else if (location == EffectLocation.PLAINS && (i & 0b0010) != 0) shouldHaveTriggered = true;
                        else if (location == EffectLocation.FOREST && (i & 0b0100) != 0) shouldHaveTriggered = true;
                        else if (location == EffectLocation.MOUNTAIN && (i & 0b1000) != 0) shouldHaveTriggered = true;
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                        if (shouldHaveTriggered)
                        {
                            Assert.IsNotNull(cpu);
                        }
                        else
                        {
                            Assert.IsNull(cpu);
                        }
                        sm.UndoPreviousStep(); // Undo this trigger, on to the next one
                    }
                    // Finally revert the unit play
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void WherePlayedTriggerTesting()
        {
            // Subscribes a unit to a semi-absolute "when played" location and then triggers different places to ensure trigger works as intended
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that triggers where played and places debug event in pile
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, [0, 4, 10], 1, 0, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Means that when triggered, it'll push the debug effect
                unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                unit.Triggers.Add(EffectLocation.PLAY_TARGET, triggerEffect); // Adds trigger specifically where the unit was played
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test loop plays unit in different locations
                LaneID[] playLocations = [LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN];
                foreach (LaneID playLocation in playLocations)
                {
                    int firstTileCoord = sm.DetailedState.BoardState.GetLane(playLocation).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                    // Play the unit in the specific location
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    sm.PlayFromHand(1, firstTileCoord);
                    int postPlayHash = sm.DetailedState.GetHashCode();
                    Assert.AreNotEqual(prePlayHash, postPlayHash);
                    // TRIGGERING LOOP, will trigger absolutely everyhwere!
                    EffectLocation[] locationsToProbe = [EffectLocation.PLAINS, EffectLocation.FOREST, EffectLocation.MOUNTAIN];
                    foreach (EffectLocation location in locationsToProbe) // Next location to test
                    {
                        // Play, test active trigger
                        LaneID locationLane = location switch
                        {
                            EffectLocation.PLAINS => LaneID.PLAINS,
                            EffectLocation.FOREST => LaneID.FOREST,
                            EffectLocation.MOUNTAIN => LaneID.MOUNTAIN,
                            _ => throw new Exception("Not a lane")
                        };
                        int locationCoord = sm.DetailedState.BoardState.GetLane(locationLane).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                        StepResult res = sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, sm.DetailedState.BoardState.Tiles[locationCoord], new EffectContext());
                        // Check if should've triggered
                        bool shouldHaveTriggered = false;
                        if (location == EffectLocation.PLAINS && playLocation == LaneID.PLAINS) shouldHaveTriggered = true;
                        else if (location == EffectLocation.FOREST && playLocation == LaneID.FOREST) shouldHaveTriggered = true;
                        else if (location == EffectLocation.MOUNTAIN && playLocation == LaneID.MOUNTAIN) shouldHaveTriggered = true;
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                        if (shouldHaveTriggered)
                        {
                            Assert.IsNotNull(cpu);
                        }
                        else
                        {
                            Assert.IsNull(cpu);
                        }
                        sm.UndoPreviousStep(); // Undo this trigger, on to the next one
                    }
                    // Finally revert the unit play
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void AutomaticDeadTriggerDeregistration()
        {
            // Unit insta dies when summoned, the trigger remains in place until it is auto-deregistered when the event triggers with no unit
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that registers trigger in board (could be anywhere) but insta dies, leaving the zombie trigger
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, [0, 3, 4, 9, 10, 17], 0, 0, 1, 1);
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Trigger has no effect as nothing should happen, so there should never be a debug context in the pile as a result of this
                unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                unit.Triggers.Add(EffectLocation.BOARD, triggerEffect); // Adds to board (location also not important)
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test
                int prePlayHash = sm.DetailedState.GetHashCode();
                int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode();
                sm.PlayFromHand(1, 0); // The unit location itself doesn't matter really
                int postPlayHash = sm.DetailedState.GetHashCode();
                int postPlayBoardHash = sm.DetailedState.BoardState.GetHashCode();
                Assert.AreNotEqual(prePlayHash, postPlayHash); // General hash has changed because of hand size and stuff
                Assert.AreNotEqual(prePlayBoardHash, postPlayBoardHash); // This one is also different because of the subscribed triggers even tho the number of units remains the same
                // Play, test "active" trigger in board
                StepResult res = sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext());
                int postTriggerHash = sm.DetailedState.GetHashCode();
                int postTriggerBoardHash = sm.DetailedState.BoardState.GetHashCode();
                // Now the interesting bit, obviously no effect should've happened but the trigger itself shoudl've cleaned the trigger of the dead unit
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                Assert.IsNull(cpu); // No debug present
                Assert.AreNotEqual(postTriggerHash, postPlayHash); // General hash still different because of board different
                Assert.AreEqual(prePlayBoardHash, postTriggerBoardHash); // But this one would be 100% same as the boards are identical again!
                sm.UndoPreviousStep(); // Undo this trigger
                Assert.AreEqual(sm.DetailedState.BoardState.GetHashCode(), postPlayBoardHash); // Re-added the zombie trigger...
                Assert.AreEqual(sm.DetailedState.GetHashCode(), postPlayHash); // Last check
                // Finally revert the unit play
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode());
            }
        }
        [TestMethod]
        public void MultiUnitTriggerRegistered()
        {
            // 2 units are summoned, now there's two triggers, do they both trigger?
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that registers trigger in board
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, [0, 3, 4, 9, 10, 17], 1, 1, 1, 1);
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Trigger has no effect as nothing should happen, so there should never be a debug context in the pile as a result of this
                unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                unit.Triggers.Add(EffectLocation.BOARD, triggerEffect); // Adds to board (location also not important)
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add cards to hand
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test
                int prePlayHash = sm.DetailedState.GetHashCode();
                sm.PlayFromHand(1, 0); // Play both units
                sm.PlayFromHand(1, 0);
                int postPlayHash = sm.DetailedState.GetHashCode();
                Assert.AreNotEqual(prePlayHash, postPlayHash);
                // Play, test "active" trigger in board
                StepResult res = sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext());
                // Now the interesting bit, obviously no effect should've happened but the trigger itself should've cleaned the trigger of the dead unit
                int debugEventCount = 0;
                foreach (GameEngineEvent ev in res.events)
                {
                    if (ev.eventType == EventType.DEBUG_EVENT)
                    {
                        debugEventCount++;
                    }
                }
                Assert.AreEqual(2, debugEventCount); // Expecting 2 because both triggered
                sm.UndoPreviousStep(); // Undo this trigger
                Assert.AreEqual(sm.DetailedState.GetHashCode(), postPlayHash); // Last check
                // Finally revert the unit play
                sm.UndoPreviousStep();
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void MultiUnitTriggerButOneDied()
        {
            // Summon 2 units but one insta-dies, thing will trigger but only once
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that registers trigger in board
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, [0, 3, 4, 9, 10, 17], 1, 1, 1, 1);
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Trigger has no effect as nothing should happen, so there should never be a debug context in the pile as a result of this
                unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                unit.Triggers.Add(EffectLocation.BOARD, triggerEffect); // Adds to board (location also not important)
                // Card 2: Same but unit has 0 hp
                Unit deadUnit = TestCardGenerator.CreateUnit(2, "TRIGGER_TEST", 0, [0, 3, 4, 9, 10, 17], 0, 0, 1, 1);
                deadUnit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                deadUnit.Triggers.Add(EffectLocation.BOARD, triggerEffect);
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                cardDb.InjectCard(2, deadUnit);
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add cards to hand
                state.PlayerStates[playerIndex].Hand.InsertToCollection(2);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test
                int prePlayHash = sm.DetailedState.GetHashCode();
                sm.PlayFromHand(1, 0); // Play both units
                sm.PlayFromHand(2, 0);
                int postPlayHash = sm.DetailedState.GetHashCode();
                Assert.AreNotEqual(prePlayHash, postPlayHash);
                // Play, test "active" trigger in board
                StepResult res = sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext());
                // Now the interesting bit, obviously no effect should've happened but the trigger itself should've cleaned the trigger of the dead unit
                int debugEventCount = 0;
                foreach (GameEngineEvent ev in res.events)
                {
                    if (ev.eventType == EventType.DEBUG_EVENT)
                    {
                        debugEventCount++;
                    }
                }
                Assert.AreEqual(1, debugEventCount); // Expecting 2 because both triggered
                sm.UndoPreviousStep(); // Undo this trigger
                Assert.AreEqual(sm.DetailedState.GetHashCode(), postPlayHash); // Last check
                // Finally revert the unit play
                sm.UndoPreviousStep();
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void CurrentTileTriggerTesting()
        {
            Random _rng = new Random();
            // Subscribes a unit to a relative "current tile" location. Tests proper subscription and subscription reversion too
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that triggers where played and places debug event in pile
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, [0, 3, 4, 9, 10, 17], 1, 0, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Means that when triggered, it'll push the debug effect
                unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                unit.Triggers.Add(EffectLocation.CURRENT_TILE, triggerEffect); // Adds trigger specifically where the unit is currently located
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test loop will play the unit in original tile or move elsewhere
                bool[] testInOriginalTileCases = [true, false];
                LaneID playLane = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]);
                int firstTileCoord = sm.DetailedState.BoardState.GetLane(playLane).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int randomTileCoord;
                do
                {
                    randomTileCoord = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                } while (randomTileCoord == firstTileCoord); // Get another random tile coord different to original
                foreach (bool testInOriginalTileCase in testInOriginalTileCases)
                {
                    // Play the unit in the specific location
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    sm.PlayFromHand(1, firstTileCoord);
                    if (!testInOriginalTileCase) // Requested to move unit elsewhere
                    {
                        PlacedEntity theEntity = (PlacedEntity)sm.DetailedState.EntityData.Last().Value; // Get the last unit I had summoned
                        Assert.AreEqual(firstTileCoord, theEntity.TileCoordinate); // Ensure I'm not about to mess up
                        sm.LIVINGENTITY_InsertInTile(theEntity, randomTileCoord); // Move entity elsewhere
                        // Harmless trigger in void to properly terminate effect stack, has no effect but to unbreak stuff
                        sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext());
                    }
                    int postPlayHash = sm.DetailedState.GetHashCode();
                    Assert.AreNotEqual(prePlayHash, postPlayHash);
                    Tile playTile = sm.DetailedState.BoardState.Tiles[firstTileCoord];
                    Tile randomTile = sm.DetailedState.BoardState.Tiles[randomTileCoord];
                    // Test trigger in playtile
                    StepResult res = sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, playTile, new EffectContext());
                    CpuState cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                    if (testInOriginalTileCase)
                    {
                        Assert.IsNotNull(cpu);
                    }
                    else
                    {
                        Assert.IsNull(cpu);
                    }
                    sm.UndoPreviousStep(); // Undo debug trigger
                    // Test trigger in other
                    res = sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, randomTile, new EffectContext());
                    cpu = TestHelperFunctions.FetchDebugEvent(res);
                    if (!testInOriginalTileCase)
                    {
                        Assert.IsNotNull(cpu);
                    }
                    else
                    {
                        Assert.IsNull(cpu);
                    }
                    sm.UndoPreviousStep(); // Undo debug trigger
                    // May need to revert the manual movement
                    if (!testInOriginalTileCase)
                    {
                        sm.UndoPreviousStep();
                    }
                    // Finally revert the unit play
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void TriggersInBoardElementHashConsistency()
        {
            Random _rng = new Random();
            // Create unit with "current place" trigger, moves the unit away, and returns (not by reverting) ensures hash also reversed if all else unchanged
            // Create secpond unit with "current place" trigger
            // Want to make sure that subscribed triggers are invariant of addition order
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that triggers where played and places debug event in pile
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, [0, 3, 4, 9, 10, 17], 1, 0, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Means that when triggered, it'll push the debug effect
                unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                unit.Triggers.Add(EffectLocation.CURRENT_TILE, triggerEffect); // Adds trigger specifically where the unit is currently located
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add 2 of these cards to hand
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test
                LaneID playLane = TestHelperFunctions.GetRandomChoice([LaneID.PLAINS, LaneID.FOREST, LaneID.MOUNTAIN]);
                int firstTileCoord = sm.DetailedState.BoardState.GetLane(playLane).GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, playerIndex);
                int randomTileCoord;
                do
                {
                    randomTileCoord = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                } while (randomTileCoord == firstTileCoord); // Get another random tile coord different to original
                // Play 2 units in the specific location
                int prePlayHash = sm.DetailedState.GetHashCode();
                sm.PlayFromHand(1, firstTileCoord);
                PlacedEntity theEntity = (PlacedEntity)sm.DetailedState.EntityData.Last().Value; // Get this summoned unit
                sm.PlayFromHand(1, firstTileCoord);
                int postPlayHash = sm.DetailedState.GetHashCode();
                Assert.AreNotEqual(prePlayHash, postPlayHash);
                // Now move unit 1
                Assert.AreEqual(firstTileCoord, theEntity.TileCoordinate); // Ensure I'm not about to mess up
                sm.LIVINGENTITY_InsertInTile(theEntity, randomTileCoord); // Move entity elsewhere
                int postmoveHash = sm.DetailedState.GetHashCode();
                Assert.AreNotEqual(prePlayHash, postmoveHash);
                Assert.AreNotEqual(postmoveHash, postPlayHash);
                // Now, move unit again to original place
                sm.LIVINGENTITY_InsertInTile(theEntity, firstTileCoord); // Return manually to first tile
                Assert.AreEqual(sm.DetailedState.GetHashCode(), postPlayHash); // Hash is same value as before without reversion, means unit trigger is order invariant
            }
        }
        [TestMethod]
        public void OnMarchTriggerTesting()
        {
            Random _rng = new Random();
            // Subscribes 2 units unit to a relative "current tile" on march trigger
            // Then, first unit (intantiated) will march and we'll see if second unit's trigger marched
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.DRAW_PHASE; // Prepare for march
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that just moves
                Unit marchingUnit = TestCardGenerator.CreateUnit(1, "MARCHING_UNIT", 0, [], 1, 0, 1, 1);
                // Card 2: Building that detects marching that happens on it's tile
                Building marchDetectingBuilding = TestCardGenerator.CreateBuilding(2, "SENSOR_BUILDING", 0, [], 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_MARCH, [debugEffect]); // Means that when triggered, it'll push the debug effect
                marchDetectingBuilding.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                marchDetectingBuilding.Triggers.Add(EffectLocation.CURRENT_TILE, triggerEffect); // Adds trigger specifically where building is currently located
                // Setup
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test loop will play the unit in same place as building or elsewhere
                int buildingCoord = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                int randomCoord;
                do
                {
                    randomCoord = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                } while (randomCoord == buildingCoord); // Get another random tile coord different to original
                bool[] testinSameLocationCases = [true, false];
                foreach (bool testinsameLocationCase in testinSameLocationCases)
                {
                    int unitCoord = testinsameLocationCase ? buildingCoord : randomCoord;
                    sm.UNIT_PlayUnit(playerIndex, new PlayContext() { Actor = marchingUnit, PlayedTarget = unitCoord }); // Manually insert unit
                    sm.BUILDING_ConstructBuilding(playerIndex, new ConstructionContext() { AbsoluteConstructionTile = buildingCoord, Actor = marchingUnit, Affected = marchDetectingBuilding }); // Manually insert building
                    sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext()); // Trigger useless debug event to properly terminate event stack
                    int preMarchHash = sm.DetailedState.GetHashCode();
                    StepResult res = sm.Step(); // This should trigger marching of units and such
                    int postMarchHash = sm.DetailedState.GetHashCode();
                    Assert.AreNotEqual(postMarchHash, preMarchHash);
                    CpuState cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                    if (testinsameLocationCase)
                    {
                        Assert.IsNotNull(cpu);
                    }
                    else
                    {
                        Assert.IsNull(cpu);
                    }
                    // Finally revert march
                    sm.UndoPreviousStep();
                    Assert.AreEqual(preMarchHash, sm.DetailedState.GetHashCode());
                    sm.UndoPreviousStep(); // Also undo unit/building injection
                }
            }
        }
        [TestMethod]
        public void NoSelfTriggering()
        {
            // Verify that a unit can't trigger when it's an action its doing by itself (as it should be resolved by interactions
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.DRAW_PHASE; // Prepare for march
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that just moves
                Unit marchingUnit = TestCardGenerator.CreateUnit(1, "MARCHING_UNIT", 0, [], 1, 0, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_MARCH, [debugEffect]); // Means that when triggered, it'll push the debug effect
                marchingUnit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                marchingUnit.Triggers.Add(EffectLocation.CURRENT_TILE, triggerEffect); // Adds trigger specifically where building is currently located
                // Setup
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test will play the unit and make it march
                sm.UNIT_PlayUnit(playerIndex, new PlayContext() { Actor = marchingUnit, PlayedTarget = 0 }); // Manually insert unit
                sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext()); // Trigger useless debug event to properly terminate event stack
                int preMarchHash = sm.DetailedState.GetHashCode();
                StepResult res = sm.Step(); // This should trigger marching of units and such
                int postMarchHash = sm.DetailedState.GetHashCode();
                Assert.AreNotEqual(postMarchHash, preMarchHash);
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                Assert.IsNull(cpu); // Ensure unit hasn't triggered
                // Finally revert march
                sm.UndoPreviousStep();
                Assert.AreEqual(preMarchHash, sm.DetailedState.GetHashCode());
                sm.UndoPreviousStep(); // Also undo unit/building injection
            }
        }
        [TestMethod]
        public void OnEotTriggerTesting()
        {
            // Testing of end of turn triggering
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that just moves
                Unit eotSensor = TestCardGenerator.CreateUnit(1, "ETO_SENSOR_UNIT", 0, [0, 3, 4, 9, 10, 17], 1, 0, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_END_OF_TURN, [debugEffect]); // Means that when triggered, it'll push the debug effect
                eotSensor.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                eotSensor.Triggers.Add(EffectLocation.BOARD, triggerEffect); // Adds trigger specifically where building is currently located
                cardDb.InjectCard(1, eotSensor); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add card to hand
                // Setup
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // FIRST: EOT and ensure nothing happens
                // Simply end turn and see if thing had been pushed
                int preEotHash = sm.DetailedState.GetHashCode();
                StepResult res = sm.EndTurn();
                Assert.AreNotEqual(preEotHash, sm.DetailedState.GetHashCode());
                Assert.AreEqual(States.DRAW_PHASE, sm.DetailedState.CurrentState); // Ensure EOT happened ok
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                Assert.IsNull(cpu); // Ensure it's not here
                // Finally revert EOT
                sm.UndoPreviousStep();
                Assert.AreEqual(preEotHash, sm.DetailedState.GetHashCode());
                // SECOND: EOT and ensure now it's triggered
                sm.PlayFromHand(1, 0); // Play the unit anywhere idc
                preEotHash = sm.DetailedState.GetHashCode();
                res = sm.EndTurn();
                Assert.AreNotEqual(preEotHash, sm.DetailedState.GetHashCode());
                Assert.AreEqual(States.DRAW_PHASE, sm.DetailedState.CurrentState); // Ensure EOT happened ok
                cpu = TestHelperFunctions.FetchDebugEvent(res); // Cpu in stack only if triggered
                Assert.IsNotNull(cpu); // Ensure I got it now
                // Finally revert EOT
                sm.UndoPreviousStep();
                Assert.AreEqual(preEotHash, sm.DetailedState.GetHashCode());
            }
        }
    }
}
