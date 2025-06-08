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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, PlayTargetLocation.ALL_LANES, 1, 0, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Means that when triggered, it'll push the debug effect
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
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
                    sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play in plains because the unit location itself doesn't matter really
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, PlayTargetLocation.ALL_LANES, 1, 0, 1, 1);
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
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test loop plays unit in different locations
                PlayTargetLocation[] playLocations = [PlayTargetLocation.PLAINS, PlayTargetLocation.FOREST, PlayTargetLocation.MOUNTAIN];
                foreach (PlayTargetLocation playLocation in playLocations)
                {
                    // Play the unit in the specific location
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    sm.PlayFromHand(1, playLocation);
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
                        if (location == EffectLocation.PLAINS && playLocation == PlayTargetLocation.PLAINS) shouldHaveTriggered = true;
                        else if (location == EffectLocation.FOREST && playLocation == PlayTargetLocation.FOREST) shouldHaveTriggered = true;
                        else if (location == EffectLocation.MOUNTAIN && playLocation == PlayTargetLocation.MOUNTAIN) shouldHaveTriggered = true;
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, PlayTargetLocation.ALL_LANES, 0, 0, 1, 1);
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
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test
                int prePlayHash = sm.DetailedState.GetHashCode();
                int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode();
                sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play in plains because the unit location itself doesn't matter really
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
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
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add cards to hand
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test
                int prePlayHash = sm.DetailedState.GetHashCode();
                sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play both units
                sm.PlayFromHand(1, PlayTargetLocation.PLAINS);
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIGGER_TEST", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
                Dictionary<TriggerType, List<Effect>> triggerEffect = new Dictionary<TriggerType, List<Effect>>();
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE
                };
                triggerEffect.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]); // Trigger has no effect as nothing should happen, so there should never be a debug context in the pile as a result of this
                unit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                unit.Triggers.Add(EffectLocation.BOARD, triggerEffect); // Adds to board (location also not important)
                // Card 2: Same but unit has 0 hp
                Unit deadUnit = TestCardGenerator.CreateUnit(2, "TRIGGER_TEST", 0, PlayTargetLocation.ALL_LANES, 0, 0, 1, 1);
                deadUnit.Triggers = new Dictionary<EffectLocation, Dictionary<TriggerType, List<Effect>>>();
                deadUnit.Triggers.Add(EffectLocation.BOARD, triggerEffect);
                // Setup
                cardDb.InjectCard(1, unit); // Add to cardDb
                cardDb.InjectCard(2, deadUnit);
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add cards to hand
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test
                int prePlayHash = sm.DetailedState.GetHashCode();
                sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play both units
                sm.PlayFromHand(2, PlayTargetLocation.PLAINS);
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
    }
}
