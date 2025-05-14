using ODLGameEngine;
using System.Linq.Expressions;

namespace EngineTests
{
    [TestClass]
    public class TrigInterTests // Class to test card effects (i.e. triggers and interactions)
    {
        // Interactions
        [TestMethod]
        public void WhenPlayedInteractionSkill()
        {
            // Testing interactions that do something when played
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG_STORE };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, TargetLocation.BOARD); // Play the card
                Assert.AreEqual(PlayOutcome.OK,playRes.Item1);
                Assert.IsNotNull(playRes.Item2);
                bool debugFlagFound = false; // Check if the debug check was added when card was played
                foreach(GameEngineEvent gameEngineEvent in playRes.Item2.events)
                {
                    if(gameEngineEvent.eventType == EventType.DEBUG_CHECK)
                    {
                        debugFlagFound = true;
                        break;
                    }
                }
                Assert.IsTrue(debugFlagFound);
            }
        }
        [TestMethod]
        public void UnitEnterBuildingOnSummonInteraction()
        {
            // Building is on the board, unit is summoned, both the unit and the building should be triggered accordingly when stepping in
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats (movt 9)
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, TargetLocation.ALL_LANES, 1, 1, 9, 1);
                // Card 2, a building, just casually placed on first tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, TargetLocation.ALL_LANES, 1, [], [], []);
                // Now add the building into the board
                TestHelperFunctions.ManualInitEntity(state, TargetLocation.PLAINS, 0, 2, playerIndex, building); // Now building is in place
                state.NextUniqueIndex = 3;
                // Create the "on step" effect
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                Dictionary<InteractionType, List<Effect>>  stepInteraction = new Dictionary<InteractionType, List<Effect>>();
                stepInteraction.Add(InteractionType.UNIT_ENTERS_BUILDING, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                List<EntityType> effectEntities = [EntityType.UNIT, EntityType.BUILDING]; // Who I want to test
                foreach (EntityType entityType in effectEntities)
                {
                    switch(entityType) // Will add interaction to correct entity
                    {
                        case EntityType.UNIT:
                            unit.Interactions = stepInteraction;
                            building.Interactions = null;
                            break;
                        case EntityType.BUILDING:
                            building.Interactions = stepInteraction;
                            unit.Interactions = null;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    // I'll load the game
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.PLAINS); // Play unit anywhere (PLAINS)
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                    // Want to make sure the entity activated is speicfically the building OR the unit (whatever im tracking)
                    Assert.AreEqual(entityType, ((EntityEvent<CpuState>)debugEvent).entity.DebugEffectReference.ActivatedEntity.PrePlayInfo.EntityType);
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void UnitEnterBuildingOnAdvanceInteraction()
        {
            // Building is on the board, unit is summoned, both the unit and the building should be triggered accordingly when stepping in
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats (movt 9)
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, TargetLocation.ALL_LANES, 1, 1, 9, 1);
                // Card 2, a building, just casually placed on second tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, TargetLocation.ALL_LANES, 1, [], [], []);
                // Now add the building into the board
                TestHelperFunctions.ManualInitEntity(state, TargetLocation.PLAINS, 1, 2, playerIndex, building); // Now building is in place
                state.NextUniqueIndex = 3;
                // Create the "on step" effect
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                Dictionary<InteractionType, List<Effect>> stepInteraction = new Dictionary<InteractionType, List<Effect>>();
                stepInteraction.Add(InteractionType.UNIT_ENTERS_BUILDING, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                List<EntityType> effectEntities = [EntityType.UNIT, EntityType.BUILDING]; // Who I want to test
                foreach (EntityType entityType in effectEntities)
                {
                    switch (entityType) // Will add interaction to correct entity
                    {
                        case EntityType.UNIT:
                            unit.Interactions = stepInteraction;
                            building.Interactions = null;
                            break;
                        case EntityType.BUILDING:
                            building.Interactions = stepInteraction;
                            unit.Interactions = null;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    // I'll load the game
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    // Play
                    sm.PlayFromHand(1, TargetLocation.PLAINS); // Play unit anywhere (PLAINS)
                    sm.EndTurn(); // Ends turn
                    sm.Step(); // Opp draw phase
                    sm.EndTurn(); // End opp turn
                    StepResult res = sm.Step(); // Do my draw phase, trigger advance now
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                    // Want to make sure the entity activated is speicfically the building OR the unit (whatever im tracking)
                    Assert.AreEqual(entityType, ((EntityEvent<CpuState>)debugEvent).entity.DebugEffectReference.ActivatedEntity.PrePlayInfo.EntityType);
                    // Revert EVERYTHING and hash check
                    sm.UndoPreviousStep();
                    sm.UndoPreviousStep();
                    sm.UndoPreviousStep();
                    sm.UndoPreviousStep();
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        // Triggers
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG_STORE };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                int stateHash = sm.DetailedState.GetHashCode();
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
                // Play
                sm.PlayFromHand(1, TargetLocation.PLAINS); // Play the unit in any lane
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetHashCode()); // State hash has changed
                Assert.IsTrue(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER)); // And the state is there!
                // Now to trigger
                bool debugFlagFound = false; // Check if the debug check was added when card was played
                StepResult res = sm.TriggerDebugStep();
                foreach (GameEngineEvent gameEngineEvent in res.events)
                {
                    if (gameEngineEvent.eventType == EventType.DEBUG_CHECK)
                    {
                        debugFlagFound = true;
                        break;
                    }
                }
                Assert.IsTrue(debugFlagFound);
                // Reversion
                sm.UndoPreviousStep(); // Undoes the debug trigger
                sm.UndoPreviousStep(); // Undoes card play
                Assert.AreEqual(stateHash, sm.DetailedState.GetHashCode());
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, TargetLocation.ALL_LANES, 0, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG_STORE };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                int stateHash = sm.DetailedState.GetHashCode();
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
                // Play
                sm.PlayFromHand(1, TargetLocation.PLAINS); // Play the unit in any lane
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetHashCode()); // State hash has changed because hand and placeable counter changed
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER)); // However this is the same
                // Now to trigger
                bool debugFlagFound = false; // Check if the debug check was added when card was played (should not be!)
                StepResult res = sm.TriggerDebugStep();
                foreach (GameEngineEvent gameEngineEvent in res.events)
                {
                    if (gameEngineEvent.eventType == EventType.DEBUG_CHECK)
                    {
                        debugFlagFound = true;
                        break;
                    }
                }
                Assert.IsFalse(debugFlagFound);
                // Reversion
                sm.UndoPreviousStep(); // Undoes the debug trigger
                sm.UndoPreviousStep(); // Undoes card play
                Assert.AreEqual(stateHash, sm.DetailedState.GetHashCode());
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG_STORE };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                int numberOfUnits = _rng.Next(2, 11); // Any random number of events (max 10)
                for(int i = 0; i < numberOfUnits; i++) // Add X of the card
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
                // Play
                for (int i = 0; i < numberOfUnits; i++) // Play X times
                {
                    sm.PlayFromHand(1, TargetLocation.PLAINS); // Play the unit in any lane
                }
                Assert.IsTrue(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
                Assert.AreEqual(sm.DetailedState.Triggers[TriggerType.DEBUG_TRIGGER].Count, numberOfUnits); // One trigger per unit
                // Now to trigger
                int debugFlagNumber = 0; // Check if the debug check was added when card was played (should not be!)
                StepResult res = sm.TriggerDebugStep();
                foreach (GameEngineEvent gameEngineEvent in res.events)
                {
                    if (gameEngineEvent.eventType == EventType.DEBUG_CHECK)
                    {
                        debugFlagNumber++;
                    }
                }
                Assert.AreEqual(debugFlagNumber, numberOfUnits); // One trigger per unit
            }
        }
        // Effects
        [TestMethod]
        public void SummonUnitEffect()
        {
            // Testing effect where unit(s) is(are) summoned
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect summonEffect = new Effect()
                {
                    EffectType = EffectType.SUMMON_UNIT, // Summons unit
                    TempVariable = 2, // Always card 2
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [summonEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Card 2: placeholder simple unit
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1));
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test loop:
                // interaction will be modified for target player and target lane(s)
                // Resulting hash board will be compared with pre-play hash and non-repeated
                // Will check: Player count, lane count, tile count, init unit count
                state = sm.DetailedState;
                int prePlayHash = state.GetHashCode();
                HashSet<int> hashes = new HashSet<int>();
                hashes.Add(prePlayHash);
                List<EntityOwner> playerTargets = [EntityOwner.OWNER, EntityOwner.OPPONENT];
                foreach (EntityOwner playerTarget in playerTargets)
                {
                    int ownerPlayer = (playerTarget == EntityOwner.OWNER) ? playerIndex : 1 - playerIndex;
                    for (int i = 1; i <= 7;  i++)
                    {
                        var numberOfSummons = i switch
                        {
                            1 or 2 or 4 => 1,
                            3 or 5 or 6 => 2,
                            7 => 3,
                            _ => throw new Exception("Invalid lane what happened here?"),
                        };
                        TargetLocation laneTarget = (TargetLocation)i;
                        summonEffect.TargetLocation = laneTarget;
                        summonEffect.TargetPlayer = playerTarget;
                        // Pre play tests
                        Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 0);
                        Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        // Play
                        Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, TargetLocation.BOARD); // Play the card
                        Assert.AreEqual(PlayOutcome.OK, playRes.Item1);
                        Assert.IsNotNull(playRes.Item2);
                        // Hash assert
                        int newHash = state.GetHashCode();
                        Assert.IsFalse(hashes.Contains(newHash));
                        hashes.Add(newHash);
                        // Location assert
                        Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT).Count, numberOfSummons);
                        Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, numberOfSummons);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, laneTarget.HasFlag(TargetLocation.PLAINS)?1:0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, laneTarget.HasFlag(TargetLocation.FOREST) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, laneTarget.HasFlag(TargetLocation.MOUNTAIN) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, laneTarget.HasFlag(TargetLocation.PLAINS) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, laneTarget.HasFlag(TargetLocation.FOREST) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, laneTarget.HasFlag(TargetLocation.MOUNTAIN) ? 1 : 0);
                        // Revert
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, state.GetHashCode());
                        Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT).Count, 0);
                        Assert.AreEqual(state.BoardState.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).GetPlacedEntities(EntityType.UNIT, ownerPlayer).Count, 0);
                    }
                }
            }
        }
        [TestMethod]
        public void TestTargetingFilters()
        {
            Random _rng = new Random();
            // Testing effect where unit(s) is(are) summoned
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing but performs a search
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect searchEffect = new Effect()
                {
                    EffectType = EffectType.FIND_ENTITIES, // Search
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation) (1 << lane); // Random lane
                lane++; lane %= 3;
                TargetLocation otherLane1 = (TargetLocation)(1 << lane); // Get the other lanes for extra testing
                lane++; lane %= 3;
                TargetLocation otherLane2 = (TargetLocation)(1 << lane);
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                state.NextUniqueIndex = 6;
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set of targeting tests
                searchEffect.SearchCriterion = SearchCriterion.ALL; // Search for all units in board, no weird lane situations yet
                List<TargetLocation> targetLocations = [TargetLocation.BOARD, targetLocation, otherLane1, otherLane2];
                List<int> directionOptions = [0, -1];
                List<EntityOwner> ownerOptions = [EntityOwner.OWNER, EntityOwner.OPPONENT];
                List<EntityType> entityOptions = [EntityType.UNIT, EntityType.BUILDING, EntityType.PLAYER];
                foreach(TargetLocation loc in targetLocations)
                {
                    searchEffect.TargetLocation = loc;
                    foreach(int dir in directionOptions)
                    {
                        searchEffect.TempVariable = dir;
                        for (int i = 0; i < 1 << ownerOptions.Count; i++) // Loop for all owners
                        {
                            // Assemble target player
                            searchEffect.TargetPlayer = EntityOwner.NONE;
                            for (int bit = 0; bit < ownerOptions.Count; bit++)
                            {
                                if (((1<<bit) & i) != 0)
                                {
                                    searchEffect.TargetPlayer |= ownerOptions[bit];
                                }
                            }
                            for (int j = 0; j < 1 << entityOptions.Count; j++)
                            {
                                // Assemble target type
                                searchEffect.TargetType = EntityType.NONE;
                                for (int bit = 0; bit < entityOptions.Count; bit++)
                                {
                                    if (((1<<bit) & j) != 0)
                                    {
                                        searchEffect.TargetType |= entityOptions[bit];
                                    }
                                }
                                // Pre-play prep
                                int expectedEntityNumber = 0;
                                // Units and buildings are found only in a specific lane
                                if (searchEffect.TargetType.HasFlag(EntityType.UNIT) && !(loc == otherLane1 || loc == otherLane2)) expectedEntityNumber++;
                                if (searchEffect.TargetType.HasFlag(EntityType.BUILDING) && !(loc == otherLane1 || loc == otherLane2)) expectedEntityNumber++;
                                if (searchEffect.TargetType.HasFlag(EntityType.PLAYER)) expectedEntityNumber++;
                                expectedEntityNumber *= searchEffect.TargetPlayer switch // Duplicated if both, 0 if none
                                {
                                    EntityOwner.NONE => 0,
                                    EntityOwner.BOTH => 2,
                                    _ => 1,
                                };
                                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                                int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode(); // Check hash beforehand
                                // Play
                                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play search card
                                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                                GameEngineEvent debugEvent = null;
                                foreach (GameEngineEvent ev in res.Item2.events)
                                {
                                    if (ev.eventType == EventType.DEBUG_CHECK)
                                    {
                                        debugEvent = ev;
                                        break;
                                    }
                                }
                                Assert.IsNotNull(debugEvent); // Found it!
                                // Check returned targets
                                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash rchanged because discard pile changed
                                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Hash remains the same as search shouldnt modify board or entities at all
                                List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                                Assert.AreEqual(expectedEntityNumber, searchResultList.Count);
                                // Special cases
                                if (searchEffect.TargetType.HasFlag(EntityType.PLAYER) && searchEffect.TargetPlayer.HasFlag(EntityOwner.OWNER))
                                { // Look for myself
                                    if (searchEffect.TempVariable >= 0) Assert.AreEqual(playerIndex, searchResultList.First());
                                    else Assert.AreEqual(playerIndex, searchResultList.Last());
                                }
                                if (searchEffect.TargetType.HasFlag(EntityType.PLAYER) && searchEffect.TargetPlayer.HasFlag(EntityOwner.OPPONENT))
                                { // Look for opp
                                    if (searchEffect.TempVariable >= 0) Assert.AreEqual(opponentIndex, searchResultList.Last());
                                    else Assert.AreEqual(opponentIndex, searchResultList.First());
                                }
                                if(searchResultList.Count == 6) // In the case everything was found, there's two options
                                {
                                    List<int> expectedResult = (searchEffect.TempVariable >= 0) ? [playerIndex,2,3,4,5,opponentIndex] : [opponentIndex,2,3,4,5,playerIndex];
                                    for(int k =0; k<6; k++)
                                    {
                                        Assert.AreEqual(searchResultList[k], expectedResult[k]);
                                    }
                                }
                                // Revert and hash check
                                sm.UndoPreviousStep();
                                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode());
                            }
                        }
                    }
                }
            }
        }
        [TestMethod]
        public void TargetInPlayedLane()
        {
            Random _rng = new Random();
            // Testing effect where unit(s) is(are) summoned
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing but performs a search
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.ALL_LANES);
                Effect searchEffect = new Effect()
                {
                    EffectType = EffectType.FIND_ENTITIES, // Search
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                lane++; lane %= 3;
                TargetLocation otherLane1 = (TargetLocation)(1 << lane); // Get the other lanes for extra testing
                lane++; lane %= 3;
                TargetLocation otherLane2 = (TargetLocation)(1 << lane);
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                state.NextUniqueIndex = 6;
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set targeting effects to see if the played lane is checked properly
                searchEffect.SearchCriterion = SearchCriterion.ALL; // Search for all units in board, no weird lane situations yet
                searchEffect.TargetLocation = TargetLocation.PLAY_TARGET; // Skill will search where played
                searchEffect.TempVariable = 0; // Forward
                searchEffect.TargetPlayer = EntityOwner.BOTH;
                searchEffect.TargetType = EntityType.UNIT|EntityType.PLAYER|EntityType.BUILDING; // Search for all
                List<TargetLocation> playLocations = [targetLocation, otherLane1, otherLane2];
                foreach (TargetLocation loc in playLocations)
                {
                    // Pre-play prep
                    int expectedEntityNumber = (targetLocation == loc) ? 6 : 2; // 6 things if correct lane, otherwise only players
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, loc); // Play search card
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                    Assert.AreEqual(expectedEntityNumber, searchResultList.Count);
                    // Special cases
                    List<int> expectedResult;
                    if(targetLocation == loc)
                    {
                        expectedResult = [playerIndex, 2, 3, 4, 5, opponentIndex];
                    }
                    else
                    {
                        expectedResult = [playerIndex, opponentIndex];
                    }
                    for (int k = 0; k < expectedResult.Count; k++)
                    {
                        Assert.AreEqual(searchResultList[k], expectedResult[k]);
                    }
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void TileByTileExploration()
        {
            Random _rng = new Random();
            // Testing effect where unit(s) is(are) summoned
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing but performs a search
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect searchEffect = new Effect()
                {
                    EffectType = EffectType.FIND_ENTITIES, // Search
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                state.NextUniqueIndex = 6;
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set of targeting tests
                searchEffect.SearchCriterion = SearchCriterion.QUANTITY; // Search for first n elements (all ordered!)
                searchEffect.TargetLocation = targetLocation;
                searchEffect.TargetPlayer = EntityOwner.BOTH;
                searchEffect.TargetType = EntityType.UNIT | EntityType.BUILDING | EntityType.PLAYER;
                List<int> directionOptions = [6, -6]; // Will try forward and in the reverse, get 6 max (all elems)
                foreach (int dir in directionOptions)
                {
                    searchEffect.TempVariable = dir;
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play search card
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                    Assert.AreEqual(6, searchResultList.Count);
                    // Check correct results
                    List<int> expectedResult = (player == CurrentPlayer.PLAYER_1) ? [0,4,2,3,5,1] : [0,5,3,2,4,1]; // The 2 options of how board looks in absolute
                    bool forwardOrder = true; // Is it a fw style response?
                    if (player != CurrentPlayer.PLAYER_1) // Being p2 flips
                        forwardOrder = !forwardOrder;
                    if (dir < 0) // ...but being in reverse direction also flips
                        forwardOrder = !forwardOrder;
                    if (!forwardOrder) expectedResult.Reverse(); // If reversed order, just flip the list
                    for (int k = 0; k < 6; k++)
                    {
                        Assert.AreEqual(searchResultList[k], expectedResult[k]);
                    }
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void OrdinalTargeting()
        {
            Random _rng = new Random();
            // Testing effect where unit(s) is(are) summoned
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing but performs a search
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect searchEffect = new Effect()
                {
                    EffectType = EffectType.FIND_ENTITIES, // Search
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                state.NextUniqueIndex = 6;
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set of targeting tests
                searchEffect.SearchCriterion = SearchCriterion.ORDINAL; // Search for first n elements (all ordered!)
                searchEffect.TargetLocation = targetLocation;
                searchEffect.TargetPlayer = EntityOwner.BOTH;
                searchEffect.TargetType = EntityType.UNIT | EntityType.BUILDING | EntityType.PLAYER;
                for(int ord = -6; ord < 6; ord++) // Will look for all units, one by one
                {
                    searchEffect.TempVariable = ord;
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play search card
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                    Assert.AreEqual(1, searchResultList.Count); // Ordinals return a single value regardless
                    // Check correct results
                    List<int> expectedResult = (player == CurrentPlayer.PLAYER_1) ? [0, 4, 2, 3, 5, 1] : [0, 5, 3, 2, 4, 1]; // The 2 options of how board looks in absolute
                    int idx = ord;
                    bool forwardOrder = true; // Is it a fw style response?
                    if (player != CurrentPlayer.PLAYER_1) // Being p2 flips
                        forwardOrder = !forwardOrder;
                    if (ord < 0) // ...but being in reverse direction will flip index
                    {
                        idx += 6;
                    }
                    if (!forwardOrder) expectedResult.Reverse(); // If reversed order, just flip the list
                    Assert.AreEqual(searchResultList[0], expectedResult[idx]);
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void NumericalTargeting()
        {
            Random _rng = new Random();
            // Testing effect where unit(s) is(are) summoned
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing but performs a search
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect searchEffect = new Effect()
                {
                    EffectType = EffectType.FIND_ENTITIES, // Search
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.BUILDING },
                });
                state.NextUniqueIndex = 6;
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set of targeting tests
                searchEffect.SearchCriterion = SearchCriterion.QUANTITY; // Search for first n elements (all ordered!)
                searchEffect.TargetLocation = targetLocation;
                searchEffect.TargetPlayer = EntityOwner.BOTH;
                searchEffect.TargetType = EntityType.UNIT | EntityType.BUILDING | EntityType.PLAYER;
                for (int num = -6; num <= 6; num++) // Will look for all units, one by one
                {
                    searchEffect.TempVariable = num;
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play search card
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                    Assert.AreEqual(Math.Abs(num), searchResultList.Count);
                    // Check correct results
                    if(num != 0) // Nothing to assert if searching for 0
                    {
                        List<int> expectedResult = (player == CurrentPlayer.PLAYER_1) ? [0, 4, 2, 3, 5, 1] : [0, 5, 3, 2, 4, 1]; // The 2 options of how board looks in absolute
                        bool forwardOrder = true; // Is it a fw style response?
                        if (player != CurrentPlayer.PLAYER_1) // Being p2 flips
                            forwardOrder = !forwardOrder;
                        if (num < 0) // ...but being in reverse direction will revert again
                            forwardOrder = !forwardOrder;
                        if (!forwardOrder) expectedResult.Reverse(); // If reversed order, just flip the list
                        for(int idx = 0; idx < Math.Abs(num); idx++) // Iterate
                        {
                            Assert.AreEqual(searchResultList[idx], expectedResult[idx]);
                        }
                    }
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void BuffingDifferentStats()
        {
            Random _rng = new Random();
            // Will buff (SET) each stat
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Values
                int statValue = _rng.Next(1, 100); // Initial value will be a specific number
                int newValue;
                do
                {
                    newValue = _rng.Next(1, 100);
                } while (newValue == statValue); // Will set to a new but different value
                // Card 1: Skill that performs a search and buffs the specific stat of a unit
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect searchEffect = new Effect() // First, search for entities
                {
                    EffectType = EffectType.FIND_ENTITIES,
                    SearchCriterion = SearchCriterion.ALL, // All of them,
                    TargetLocation = TargetLocation.BOARD, // Everywhere
                    TargetPlayer = EntityOwner.OWNER, // Whoever played the card
                    TargetType = EntityType.UNIT // Get unit (as this has all stats)
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                Effect buffEffect = new Effect() // Operation that'll replace the stat for the new value
                {
                    EffectType = EffectType.MODIFIER,
                    TempVariable = newValue,
                    ModifierOperation = ModifierOperation.SET
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect, buffEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Add stuff to board. In a random lane, add a few units for player
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                Unit theUnit = new Unit() // This is the unit that'll be created
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT }, 
                };
                theUnit.Attack.BaseValue = statValue;
                theUnit.Hp.BaseValue = statValue;
                theUnit.Movement.BaseValue = statValue;
                theUnit.MovementDenominator.BaseValue = statValue;
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 2, playerIndex, (PlacedEntity)theUnit.Clone());
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 3, playerIndex, (PlacedEntity)theUnit.Clone());
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set of stat targeting
                List<ModifierTarget> modifierTargets = [ModifierTarget.TARGET_HP, ModifierTarget.TARGET_ATTACK, ModifierTarget.TARGET_MOVEMENT, ModifierTarget.TARGET_MOVEMENT_DENOMINATOR];
                foreach (ModifierTarget modifierTarget in modifierTargets) // Will buff all things, one by one
                {
                    buffEffect.ModifierTarget = modifierTarget; // Buff will now target this stat
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play search card
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                    List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                    foreach (int entityId in searchResultList)
                    { // Check if the buff did it's job
                        Unit unitToCheck = (Unit)sm.DetailedState.EntityData[entityId];
                        Stat statToChech = modifierTarget switch
                        {
                            ModifierTarget.TARGET_HP => unitToCheck.Hp,
                            ModifierTarget.TARGET_ATTACK => unitToCheck.Attack,
                            ModifierTarget.TARGET_MOVEMENT => unitToCheck.Movement,
                            ModifierTarget.TARGET_MOVEMENT_DENOMINATOR => unitToCheck.MovementDenominator,
                            _ => throw new NotImplementedException("Modifier type not implemented yet")
                        };
                        Assert.AreEqual(statToChech.Total, newValue);
                    }
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void BuffingModes()
        {
            Random _rng = new Random();
            // Will buff (SET) each stat
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Values
                int statValue = _rng.Next(2, 100); // Initial value will be a specific number
                int buffValue;
                do
                {
                    buffValue = _rng.Next(2, 100);
                } while (buffValue == statValue); // Will set to a new but different value
                // Card 1: Skill that performs a search and buffs the specific stat of a unit
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect searchEffect = new Effect() // First, search for entities
                {
                    EffectType = EffectType.FIND_ENTITIES,
                    SearchCriterion = SearchCriterion.ALL, // All of them,
                    TargetLocation = TargetLocation.BOARD, // Everywhere
                    TargetPlayer = EntityOwner.OWNER, // Whoever played the card
                    TargetType = EntityType.UNIT // Get unit (as this has all stats)
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                Effect buffEffect = new Effect() // Operation that'll replace the stat for the new value
                {
                    EffectType = EffectType.MODIFIER,
                    TempVariable = buffValue,
                    ModifierTarget = ModifierTarget.TARGET_HP
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect, buffEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Add stuff to board. In a random lane, add a few units for player
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                Unit theUnit = new Unit() // This is the unit that'll be created
                {
                    PrePlayInfo = new PrePlayInfo() { EntityType = EntityType.UNIT },
                };
                theUnit.Attack.BaseValue = statValue;
                theUnit.Hp.BaseValue = statValue;
                theUnit.Movement.BaseValue = statValue;
                theUnit.MovementDenominator.BaseValue = statValue;
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 0, 2, playerIndex, (PlacedEntity)theUnit.Clone());
                TestHelperFunctions.ManualInitEntity(state, targetLocation, 1, 3, playerIndex, (PlacedEntity)theUnit.Clone());
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set of stat targeting
                List<ModifierOperation> modifierOperations = [ModifierOperation.ADD, ModifierOperation.ABSOLUTE_SET, ModifierOperation.SET, ModifierOperation.MULTIPLY];
                foreach (ModifierOperation modifierOperation in modifierOperations) // Will buff in all ways, one by one
                {
                    buffEffect.ModifierOperation = modifierOperation; // Buff will do this
                    int desiredValue = modifierOperation switch
                    {
                        ModifierOperation.ADD => statValue + buffValue,
                        ModifierOperation.MULTIPLY => statValue * buffValue,
                        ModifierOperation.SET => buffValue,
                        ModifierOperation.ABSOLUTE_SET => buffValue,
                        _ => throw new NotImplementedException("Modifier op not implemented yet")
                    };
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play search card
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                    List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                    foreach (int entityId in searchResultList)
                    { // Check if the buff did it's job
                        Unit unitToCheck = (Unit)sm.DetailedState.EntityData[entityId];
                        Stat statToChech = unitToCheck.Hp;
                        Assert.AreEqual(statToChech.Total, desiredValue);
                    }
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void SelectActor()
        {
            // Will play a card and then make sure this is a valid actor of a "when played" effect
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect selectEffect = new Effect() // First, search for entities
                {
                    EffectType = EffectType.SELECT_ENTITY,
                    SearchCriterion = SearchCriterion.ACTOR_ENTITY, // Selects actor entity, in this case, myself when played
                    TargetPlayer = EntityOwner.OWNER,
                    TargetType = EntityType.UNIT,
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                unit.Interactions = new Dictionary<InteractionType, List<Effect>>();
                unit.Interactions.Add(InteractionType.WHEN_PLAYED, [selectEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Do the selection
                // Pre-play prep
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                // Play
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.PLAINS); // Play search card anywhere (PLAINS)
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                GameEngineEvent debugEvent = null;
                foreach (GameEngineEvent ev in res.Item2.events)
                {
                    if (ev.eventType == EventType.DEBUG_CHECK)
                    {
                        debugEvent = ev;
                        break;
                    }
                }
                Assert.IsNotNull(debugEvent); // Found it!
                // Check returned targets
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                Assert.AreEqual(searchResultList.Count, 1);
                Assert.AreEqual(searchResultList[0], sm.DetailedState.NextUniqueIndex - 1); // Unit shoudl've been initialized as id = 2
                // Revert and hash check
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void SelectAffected()
        {
            // Will step on building and make sure I can obtain affected (building)
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect selectEffect = new Effect() // First, search for entities
                {
                    EffectType = EffectType.SELECT_ENTITY,
                    SearchCriterion = SearchCriterion.AFFECTED_ENTITY, // Selects affected entity, in this case, myself when played
                    TargetPlayer = EntityOwner.OWNER,
                    TargetType = EntityType.BUILDING,
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                unit.Interactions = new Dictionary<InteractionType, List<Effect>>();
                unit.Interactions.Add(InteractionType.UNIT_ENTERS_BUILDING, [selectEffect, debugEffect]); // Add interaction to card, will obtain the bldg i step into
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Now I init a building in first tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, TargetLocation.ALL_LANES, 1, [], [], []);
                TestHelperFunctions.ManualInitEntity(state, TargetLocation.PLAINS, 0, 2, playerIndex, building); // Now building is in place
                state.NextUniqueIndex = 3;
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Do the selection
                // Pre-play prep
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                // Play
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.PLAINS); // Play unit anywhere (PLAINS)
                GameEngineEvent debugEvent = null;
                foreach (GameEngineEvent ev in res.Item2.events)
                {
                    if (ev.eventType == EventType.DEBUG_CHECK)
                    {
                        debugEvent = ev;
                        break;
                    }
                }
                Assert.IsNotNull(debugEvent); // Found it!
                // Check returned targets
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                Assert.AreEqual(searchResultList.Count, 1);
                Assert.AreEqual(searchResultList[0], sm.DetailedState.NextUniqueIndex - 2); // Building shoudl've been initialized as id = 2 (and unit = 3)
                // Revert and hash check
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void TriggeredUnitSelection()
        {
            // Testing of a debug trigger and selection of unit
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect selectEffect = new Effect() // First, search for entities
                {
                    EffectType = EffectType.SELECT_ENTITY,
                    SearchCriterion = SearchCriterion.EFFECT_OWNING_ENTITY, // Selects triggered entity, in this case, the unit
                    TargetPlayer = EntityOwner.OWNER,
                    TargetType = EntityType.UNIT,
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [selectEffect, debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                int stateHash = sm.DetailedState.GetHashCode();
                // Play
                sm.PlayFromHand(1, TargetLocation.PLAINS); // Play the unit in any lane
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetHashCode()); // State hash has changed
                // Now check trigger
                StepResult res = sm.TriggerDebugStep();
                GameEngineEvent debugEvent = null;
                foreach (GameEngineEvent ev in res.events)
                {
                    if (ev.eventType == EventType.DEBUG_CHECK)
                    {
                        debugEvent = ev;
                        break;
                    }
                }
                Assert.IsNotNull(debugEvent); // Found it!
                // Check returned targets
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                Assert.AreEqual(searchResultList.Count, 1);
                Assert.AreEqual(searchResultList[0], sm.DetailedState.NextUniqueIndex - 1); // Unit shoudl've been initialized as id = 2
                // Reversion
                sm.UndoPreviousStep(); // Undoes the debug trigger
                sm.UndoPreviousStep(); // Undoes card play
                Assert.AreEqual(stateHash, sm.DetailedState.GetHashCode());
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
            }
        }
        [TestMethod]
        public void SelectEntityTypeFilters()
        {
            // Like the building enter test, but I trick the building into being weird thigns to make sure entitty type filter works
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect selectEffect = new Effect() // First, search for entities
                {
                    EffectType = EffectType.SELECT_ENTITY,
                    SearchCriterion = SearchCriterion.AFFECTED_ENTITY, // Selects affected entity, in this case, myself when played
                    TargetPlayer = EntityOwner.OWNER,
                    TargetType = EntityType.BUILDING,
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                unit.Interactions = new Dictionary<InteractionType, List<Effect>>();
                unit.Interactions.Add(InteractionType.UNIT_ENTERS_BUILDING, [selectEffect, debugEffect]); // Add interaction to card, will obtain the bldg i step into
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Now I init a building in first tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, TargetLocation.ALL_LANES, 1, [], [], []);
                TestHelperFunctions.ManualInitEntity(state, TargetLocation.PLAINS, 0, 2, playerIndex, building); // Now building is in place
                state.NextUniqueIndex = 3; 
                List<EntityType> buildingEntityTypes = [EntityType.UNIT, EntityType.BUILDING];
                List<EntityType> filterEntityTypes = [EntityType.UNIT, EntityType.BUILDING, EntityType.BUILDING|EntityType.UNIT];
                foreach (EntityType buildingEntityType in buildingEntityTypes)
                {
                    foreach (EntityType filterEntityType in filterEntityTypes)
                    {
                        selectEffect.TargetType = filterEntityType;
                        building.PrePlayInfo.EntityType = buildingEntityType;
                        // Finally load the game
                        GameStateMachine sm = new GameStateMachine(cardDb);
                        sm.LoadGame(state); // Start from here
                        // Do the selection
                        // Pre-play prep
                        int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                        // Play
                        Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.PLAINS); // Play unit anywhere (PLAINS)
                        GameEngineEvent debugEvent = null;
                        foreach (GameEngineEvent ev in res.Item2.events)
                        {
                            if (ev.eventType == EventType.DEBUG_CHECK)
                            {
                                debugEvent = ev;
                                break;
                            }
                        }
                        Assert.IsNotNull(debugEvent); // Found it!
                        // Check returned targets
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                        List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                        if(filterEntityType.HasFlag(buildingEntityType)) // Then, if types match, id get sth as target, otherwise no
                        {
                            Assert.AreEqual(searchResultList.Count, 1);
                            Assert.AreEqual(searchResultList[0], sm.DetailedState.NextUniqueIndex - 2); // Building shoudl've been initialized as id = 2 (and unit = 3)
                        }
                        else
                        {
                            Assert.AreEqual(searchResultList.Count, 0);
                        }
                        // Revert and hash check
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                }
            }
        }
        [TestMethod]
        public void SelectEntityOwnerFilters()
        {
            // Like the building enter test, but I trick the building into being weird thigns to make sure entitty type filter works
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect selectEffect = new Effect() // First, search for entities
                {
                    EffectType = EffectType.SELECT_ENTITY,
                    SearchCriterion = SearchCriterion.AFFECTED_ENTITY, // Selects affected entity, in this case, myself when played
                    TargetPlayer = EntityOwner.OWNER,
                    TargetType = EntityType.BUILDING,
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                unit.Interactions = new Dictionary<InteractionType, List<Effect>>();
                unit.Interactions.Add(InteractionType.UNIT_ENTERS_BUILDING, [selectEffect, debugEffect]); // Add interaction to card, will obtain the bldg i step into
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Now I init a building in first tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, TargetLocation.ALL_LANES, 1, [], [], []);
                TestHelperFunctions.ManualInitEntity(state, TargetLocation.PLAINS, 0, 2, playerIndex, building); // Now building is in place
                state.NextUniqueIndex = 3; 
                List<EntityOwner> buildingEntityOwners = [EntityOwner.OWNER, EntityOwner.OPPONENT]; // This is weird and never should happen in real code
                List<EntityOwner> filterEntityOwners = [EntityOwner.OWNER, EntityOwner.OPPONENT, EntityOwner.BOTH];
                foreach (EntityOwner buildingEntityOwner in buildingEntityOwners)
                {
                    foreach (EntityOwner filterEntityOwner in filterEntityOwners)
                    {
                        selectEffect.TargetPlayer = filterEntityOwner;
                        building.Owner = (buildingEntityOwner == EntityOwner.OWNER) ? playerIndex : opponentIndex;
                        // Finally load the game
                        GameStateMachine sm = new GameStateMachine(cardDb);
                        sm.LoadGame(state); // Start from here
                        // Do the selection
                        // Pre-play prep
                        int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                        // Play
                        Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.PLAINS); // Play unit anywhere (PLAINS)
                        GameEngineEvent debugEvent = null;
                        foreach (GameEngineEvent ev in res.Item2.events)
                        {
                            if (ev.eventType == EventType.DEBUG_CHECK)
                            {
                                debugEvent = ev;
                                break;
                            }
                        }
                        Assert.IsNotNull(debugEvent); // Found it!
                        // Check returned targets
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                        List<int> searchResultList = ((EntityEvent<CpuState>)debugEvent).entity.EffectTargets;
                        if (filterEntityOwner.HasFlag(buildingEntityOwner)) // Then, if types match, id get sth as target, otherwise no
                        {
                            Assert.AreEqual(searchResultList.Count, 1);
                            Assert.AreEqual(searchResultList[0], sm.DetailedState.NextUniqueIndex - 2); // Building shoudl've been initialized as id = 2 (and unit = 3)
                        }
                        else
                        {
                            Assert.AreEqual(searchResultList.Count, 0);
                        }
                        // Revert and hash check
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                }
            }
        }
        [TestMethod]
        public void RegisterModifierArithmetic()
        {
            Random _rng = new Random();
            // Performs operations on ACC to check register targetting
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Values
                int firstValue = _rng.Next(2, 100); // First will set the ACC
                int secondValue = _rng.Next(2, 100); // Then ACC (op)= secondValue
                // Card 1: Calculator skill, will set the value of ACC and return it as a debug trigger
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect setFirstValueEffect = new Effect()
                {
                    EffectType = EffectType.MODIFIER,
                    ModifierOperation = ModifierOperation.SET,
                    InputRegister = Register.TEMP_VARIABLE,
                    TempVariable = firstValue,
                    OutputRegister = Register.ACC, // Stores into ACC
                };
                Effect operationEffect = new Effect()
                {
                    EffectType = EffectType.MODIFIER,
                    InputRegister = Register.TEMP_VARIABLE,
                    TempVariable = secondValue,
                    OutputRegister = Register.ACC, // Stores into ACC
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [setFirstValueEffect, operationEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set of stat targeting
                List<ModifierOperation> modifierOperations = [ModifierOperation.ADD, ModifierOperation.ABSOLUTE_SET, ModifierOperation.SET, ModifierOperation.MULTIPLY];
                foreach (ModifierOperation modifierOperation in modifierOperations) // Will buff in all ways, one by one
                {
                    operationEffect.ModifierOperation = modifierOperation; // Select calculator op
                    int desiredValue = modifierOperation switch
                    {
                        ModifierOperation.ADD => firstValue + secondValue,
                        ModifierOperation.MULTIPLY => firstValue * secondValue,
                        ModifierOperation.SET => secondValue,
                        ModifierOperation.ABSOLUTE_SET => secondValue,
                        _ => throw new NotImplementedException("Modifier op not implemented yet")
                    };
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    // Play
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play search card
                    Assert.AreEqual(res.Item1, PlayOutcome.OK);
                    GameEngineEvent debugEvent = null;
                    foreach (GameEngineEvent ev in res.Item2.events)
                    {
                        if (ev.eventType == EventType.DEBUG_CHECK)
                        {
                            debugEvent = ev;
                            break;
                        }
                    }
                    Assert.IsNotNull(debugEvent); // Found it!
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                    Assert.AreEqual(((EntityEvent<CpuState>)debugEvent).entity.Acc, desiredValue); // Check if ACC loaded properly
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void EffectChainContextSharing()
        {
            // Triggers mid-interaction to ensure CPU context of a secific entity persists
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Values
                int firstValue = 5; // First will set the ACC
                int secondValue = 2; // Then ACC *= secondValue
                // Card 1: Calculator skill, will set the value of ACC and return it as a debug trigger
                Unit unit = TestCardGenerator.CreateUnit(1, "WHENPLAYED", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect setFirstValueEffect = new Effect()
                {
                    EffectType = EffectType.MODIFIER,
                    ModifierOperation = ModifierOperation.SET,
                    InputRegister = Register.TEMP_VARIABLE,
                    TempVariable = firstValue,
                    OutputRegister = Register.ACC, // Stores into ACC
                };
                Effect secondOperationEffect = new Effect()
                {
                    EffectType = EffectType.MODIFIER,
                    ModifierOperation = ModifierOperation.MULTIPLY,
                    InputRegister = Register.TEMP_VARIABLE,
                    TempVariable = secondValue,
                    OutputRegister = Register.ACC, // Stores into ACC
                };
                Effect debugPushEffect = new Effect()
                {
                    EffectType = EffectType.TRIGGER_DEBUG,
                };
                Effect debugPopEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                // Add when played inter
                unit.Interactions = new Dictionary<InteractionType, List<Effect>>();
                unit.Interactions.Add(InteractionType.WHEN_PLAYED, [setFirstValueEffect, debugPushEffect, debugPopEffect]); // Add interaction to card
                // Add debug trigger!
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [secondOperationEffect]);
                // Rest of test
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Test
                int desiredValue = firstValue * secondValue; 
                // Pre-play prep
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                // Play
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.PLAINS); // Play unit in any lane idc
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                GameEngineEvent debugEvent = null;
                foreach (GameEngineEvent ev in res.Item2.events)
                {
                    if (ev.eventType == EventType.DEBUG_CHECK)
                    {
                        debugEvent = ev;
                        break;
                    }
                }
                Assert.IsNotNull(debugEvent); // Found it!
                // Check returned targets
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                Assert.AreEqual(((EntityEvent<CpuState>)debugEvent).entity.Acc, desiredValue); // Check if ACC loaded properly
                // Revert and hash check
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void EffectChainContextIndependence()
        {
            // Triggers mid-interaction to ensure CPU contexts of different entities are independend
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Values
                int firstValue = 5; // First will set the ACC
                int secondValue = 2; // Then ACC *= secondValue
                // Card 1: Calculator skill, will set the value of ACC and return it as a debug trigger
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Unit unit = TestCardGenerator.CreateUnit(2, "ONTRIGGER", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect setFirstValueEffect = new Effect()
                {
                    EffectType = EffectType.MODIFIER,
                    ModifierOperation = ModifierOperation.SET,
                    InputRegister = Register.TEMP_VARIABLE,
                    TempVariable = firstValue,
                    OutputRegister = Register.ACC, // Stores into ACC
                };
                Effect secondOperationEffect = new Effect()
                {
                    EffectType = EffectType.MODIFIER,
                    ModifierOperation = ModifierOperation.MULTIPLY,
                    InputRegister = Register.TEMP_VARIABLE,
                    TempVariable = secondValue,
                    OutputRegister = Register.ACC, // Stores into ACC
                };
                Effect debugPushEffect = new Effect()
                {
                    EffectType = EffectType.TRIGGER_DEBUG,
                };
                Effect debugPopEffect = new Effect()
                {
                    EffectType = EffectType.DEBUG_STORE, // Pops debug results, useful
                };
                // Add when played inter
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [setFirstValueEffect, debugPushEffect, debugPopEffect]); // Add interaction to card
                // Add debug trigger!
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [secondOperationEffect]);
                // Rest of test
                cardDb.InjectCard(1, skill); // Add to cardDb
                cardDb.InjectCard(2, unit);
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Test
                int desiredValue = firstValue;
                // Pre-play prep
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                // Play
                sm.PlayFromHand(2, TargetLocation.PLAINS); // Play unit in any lane idc
                Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play the skill
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                GameEngineEvent debugEvent = null;
                foreach (GameEngineEvent ev in res.Item2.events)
                {
                    if (ev.eventType == EventType.DEBUG_CHECK)
                    {
                        debugEvent = ev;
                        break;
                    }
                }
                Assert.IsNotNull(debugEvent); // Found it!
                // Check returned targets
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                Assert.AreEqual(((EntityEvent<CpuState>)debugEvent).entity.Acc, desiredValue); // Check if ACC loaded properly, and no interference from trigger
                // Revert and hash check
                sm.UndoPreviousStep();
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void GoldBuffEffect()
        {
            Random _rng = new Random();
            // Play a spell that will affect a player's gold, given all possible operations and targets
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Gold
                int startingGold = _rng.Next(2, 100);
                int goldModifier = _rng.Next(2, 100);
                state.PlayerStates[0].CurrentGold = startingGold;
                state.PlayerStates[1].CurrentGold = startingGold;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that will modify  a player's gold
                Skill skill = TestCardGenerator.CreateSkill(1, "GOLDBUFF", 0, TargetLocation.BOARD);
                Effect goldModifyEffect = new Effect()
                {
                    EffectType = EffectType.MODIFIER,
                    InputRegister = Register.TEMP_VARIABLE,
                    TempVariable = goldModifier,
                    ModifierTarget = ModifierTarget.PLAYERS_GOLD,
                };
                // Add when played inter
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [goldModifyEffect]); // Add interaction to card
                // Rest of test
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                List<EntityOwner> ownersToCheck = [EntityOwner.OWNER, EntityOwner.OPPONENT, EntityOwner.BOTH];
                List<ModifierOperation> operationsToCheck = [ModifierOperation.SET, ModifierOperation.ABSOLUTE_SET, ModifierOperation.ADD, ModifierOperation.MULTIPLY];
                foreach (ModifierOperation operation in operationsToCheck)
                {
                    goldModifyEffect.ModifierOperation = operation;
                    int desiredValue = operation switch
                    {
                        ModifierOperation.SET => goldModifier,
                        ModifierOperation.ABSOLUTE_SET => goldModifier,
                        ModifierOperation.ADD => startingGold + goldModifier,
                        ModifierOperation.MULTIPLY => startingGold * goldModifier,
                        _ => throw new NotImplementedException("Not a valid op")
                    };
                    foreach (EntityOwner owner in ownersToCheck)
                    {
                        goldModifyEffect.TargetPlayer = owner;
                        // Pre-play prep
                        int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                        // Play
                        Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, TargetLocation.BOARD); // Play the skill
                        Assert.AreEqual(res.Item1, PlayOutcome.OK);
                        // Check gold then
                        if (owner.HasFlag(EntityOwner.OWNER)) // Check if this player's gold had to be modified or remains as starting
                            Assert.AreEqual(sm.DetailedState.PlayerStates[playerIndex].CurrentGold, desiredValue);
                        else
                            Assert.AreEqual(sm.DetailedState.PlayerStates[playerIndex].CurrentGold, startingGold);
                        if (owner.HasFlag(EntityOwner.OPPONENT)) // Check if this player's gold had to be modified or remains as starting
                            Assert.AreEqual(sm.DetailedState.PlayerStates[opponentIndex].CurrentGold, desiredValue);
                        else
                            Assert.AreEqual(sm.DetailedState.PlayerStates[opponentIndex].CurrentGold, startingGold);
                        // Revert and hash check
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                }
            }
        }
    }
}
