using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class TriggerTests
    {
        /*
        [TestMethod]
        public void UnitTriggers()
        {
            // Testing of a debug trigger
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that has a debug trigger effect
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                int stateHash = sm.DetailedState.GetHashCode();
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED));
                // Play
                sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play the unit in any lane
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetHashCode()); // State hash has changed
                Assert.IsTrue(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED)); // And the state is there!
                // Now to trigger
                StepResult res = sm.TriggerDebugStep();
                // Check if debug event is there
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(res);
                Assert.IsNotNull(cpu);
                // Reversion
                sm.UndoPreviousStep(); // Undoes the debug trigger
                sm.UndoPreviousStep(); // Undoes card play
                Assert.AreEqual(stateHash, sm.DetailedState.GetHashCode());
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED));
            }
        }
        [TestMethod]
        public void DeadUnitDoesntTrigger()
        {
            // Testing of a debug trigger
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that has a debug trigger effect. However will die the moment it's summoned so trigger would be deleted immediately
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, PlayTargetLocation.ALL_LANES, 0, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                int stateHash = sm.DetailedState.GetHashCode();
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED));
                // Play
                sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play the unit in any lane
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetHashCode()); // State hash has changed because hand and placeable counter changed
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED)); // However this is the same
                // Now to trigger
                StepResult res = sm.TriggerDebugStep();
                // Check if debug event is there
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(res);
                Assert.IsNull(cpu);
                // Reversion
                sm.UndoPreviousStep(); // Undoes the debug trigger
                sm.UndoPreviousStep(); // Undoes card play
                Assert.AreEqual(stateHash, sm.DetailedState.GetHashCode());
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED));
            }
        }
        [TestMethod]
        public void MultipleUnitsMultipleTriggers()
        {
            // Testing of a debug trigger
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
                // Card 1: Unit that has a debug trigger effect
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.ON_DEBUG_TRIGGERED, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                int numberOfUnits = _rng.Next(2, 11); // Any random number of events (max 10)
                for (int i = 0; i < numberOfUnits; i++) // Add X of the card
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED));
                // Play
                for (int i = 0; i < numberOfUnits; i++) // Play X times
                {
                    sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play the unit in any lane
                }
                Assert.IsTrue(sm.DetailedState.Triggers.ContainsKey(TriggerType.ON_DEBUG_TRIGGERED));
                Assert.AreEqual(sm.DetailedState.Triggers[TriggerType.ON_DEBUG_TRIGGERED].Count, numberOfUnits); // One trigger per unit
                // Now to trigger
                int debugFlagNumber = 0; // Check if the debug check was added when card was played (should not be!)
                StepResult res = sm.TriggerDebugStep();
                foreach (GameEngineEvent gameEngineEvent in res.events)
                {
                    if (gameEngineEvent.eventType == EventType.DEBUG_EVENT)
                    {
                        debugFlagNumber++;
                    }
                }
                Assert.AreEqual(debugFlagNumber, numberOfUnits); // One trigger per unit
            }
        }
        */
    }
}
