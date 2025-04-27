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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG };
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
        // Triggers
        [TestMethod]
        public void UnitTriggers()
        {
            // Testing of a debug trigger
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that has a debug trigger effect
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                int stateHash = sm.DetailedState.GetGameStateHash();
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
                // Play
                sm.PlayFromHand(1, TargetLocation.PLAINS); // Play the unit in any lane
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetGameStateHash()); // State hash has changed
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
                Assert.AreEqual(stateHash, sm.DetailedState.GetGameStateHash());
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that has a debug trigger effect. However will die the moment it's summoned so trigger would be deleted immediately
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, TargetLocation.ALL_LANES, 0, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG };
                unit.Triggers = new Dictionary<TriggerType, List<Effect>>();
                unit.Triggers.Add(TriggerType.DEBUG_TRIGGER, [debugEffect]);
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Pre play assert ( specific state and no triggers )
                int stateHash = sm.DetailedState.GetGameStateHash();
                Assert.IsFalse(sm.DetailedState.Triggers.ContainsKey(TriggerType.DEBUG_TRIGGER));
                // Play
                sm.PlayFromHand(1, TargetLocation.PLAINS); // Play the unit in any lane
                Assert.AreNotEqual(stateHash, sm.DetailedState.GetGameStateHash()); // State hash has changed because hand and placeable counter changed
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
                Assert.AreEqual(stateHash, sm.DetailedState.GetGameStateHash());
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that has a debug trigger effect
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG };
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that does nothing
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, TargetLocation.BOARD);
                Effect summonEffect = new Effect()
                {
                    EffectType = EffectType.SUMMON_UNIT, // Summons unit
                    CardNumber = 2, // Always card 2
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
                int prePlayHash = state.GetGameStateHash();
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
                        int newHash = state.GetGameStateHash();
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
                        Assert.AreEqual(prePlayHash, state.GetGameStateHash());
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
        /// <summary>
        /// Adds entity to board, no checks
        /// </summary>
        /// <param name="state">GameState</param>
        /// <param name="lane">Which lane</param>
        /// <param name="tileCoord">Which tile</param>
        /// <param name="uniqueId">Desired ID</param>
        /// <param name="owner">Entity owner index</param>
        /// <param name="entity">Entity to add</param>
        static public void ManualInitEntity(GameStateStruct state, TargetLocation lane, int tileCoord, int uniqueId, int owner, PlacedEntity entity)
        {
            entity.Owner = owner;
            entity.UniqueId = uniqueId;
            // Add to board and sm
            state.EntityData.Add(uniqueId, entity);
            state.BoardState.EntityListOperation(entity, EntityListOperation.ADD);
            // Add to lane
            entity.LaneCoordinate = (LaneID)lane;
            state.BoardState.GetLane(lane).EntityListOperation(entity, EntityListOperation.ADD);
            // Add to tile
            entity.TileCoordinate = state.BoardState.GetLane(lane).GetAbsoluteTileCoord(tileCoord, entity.Owner);
            state.BoardState.GetLane(lane).GetTileAbsolute(entity.TileCoordinate).EntityListOperation(entity, EntityListOperation.ADD);
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
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
                    EffectType = EffectType.DEBUG, // Pops debug results
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
                ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
                });
                ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
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
                        searchEffect.Value = dir;
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
                                int prePlayHash = sm.DetailedState.GetGameStateHash(); // Check hash beforehand
                                int prePlayBoardHash = sm.DetailedState.BoardState.GetGameStateHash(); // Check hash beforehand
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
                                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetGameStateHash()); // Hash rchanged because discard pile changed
                                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash()); // Hash remains the same as search shouldnt modify board or entities at all
                                List<int> searchResultList = ((EntityEvent<OngoingEffectContext>)debugEvent).entity.EffectTargets;
                                Assert.AreEqual(expectedEntityNumber, searchResultList.Count);
                                // Special cases
                                if (searchEffect.TargetType.HasFlag(EntityType.PLAYER) && searchEffect.TargetPlayer.HasFlag(EntityOwner.OWNER))
                                { // Look for myself
                                    if (searchEffect.Value >= 0) Assert.AreEqual(playerIndex, searchResultList.First());
                                    else Assert.AreEqual(playerIndex, searchResultList.Last());
                                }
                                if (searchEffect.TargetType.HasFlag(EntityType.PLAYER) && searchEffect.TargetPlayer.HasFlag(EntityOwner.OPPONENT))
                                { // Look for opp
                                    if (searchEffect.Value >= 0) Assert.AreEqual(opponentIndex, searchResultList.Last());
                                    else Assert.AreEqual(opponentIndex, searchResultList.First());
                                }
                                if(searchResultList.Count == 6) // In the case everything was found, there's two options
                                {
                                    List<int> expectedResult = (searchEffect.Value >= 0) ? [playerIndex,2,3,4,5,opponentIndex] : [opponentIndex,2,3,4,5,playerIndex];
                                    for(int k =0; k<6; k++)
                                    {
                                        Assert.AreEqual(searchResultList[k], expectedResult[k]);
                                    }
                                }
                                // Revert and hash check
                                sm.UndoPreviousStep();
                                Assert.AreEqual(prePlayHash, sm.DetailedState.GetGameStateHash());
                                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash());
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
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
                    EffectType = EffectType.DEBUG, // Pops debug results
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
                ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
                });
                ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
                });
                state.NextUniqueIndex = 6;
                // Finally load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Set targeting effects to see if the played lane is checked properly
                searchEffect.SearchCriterion = SearchCriterion.ALL; // Search for all units in board, no weird lane situations yet
                searchEffect.TargetLocation = TargetLocation.PLAY_TARGET; // Skill will search where played
                searchEffect.Value = 0; // Forward
                searchEffect.TargetPlayer = EntityOwner.BOTH;
                searchEffect.TargetType = EntityType.UNIT|EntityType.PLAYER|EntityType.BUILDING; // Search for all
                List<TargetLocation> playLocations = [targetLocation, otherLane1, otherLane2];
                foreach (TargetLocation loc in playLocations)
                {
                    // Pre-play prep
                    int expectedEntityNumber = (targetLocation == loc) ? 6 : 2; // 6 things if correct lane, otherwise only players
                    int prePlayHash = sm.DetailedState.GetGameStateHash(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetGameStateHash(); // Check hash beforehand
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
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetGameStateHash()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<OngoingEffectContext>)debugEvent).entity.EffectTargets;
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
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetGameStateHash());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash());
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
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
                    EffectType = EffectType.DEBUG, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
                });
                ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
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
                    searchEffect.Value = dir;
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetGameStateHash(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetGameStateHash(); // Check hash beforehand
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
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetGameStateHash()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<OngoingEffectContext>)debugEvent).entity.EffectTargets;
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
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetGameStateHash());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash());
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
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
                    EffectType = EffectType.DEBUG, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
                });
                ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
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
                    searchEffect.Value = ord;
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetGameStateHash(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetGameStateHash(); // Check hash beforehand
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
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetGameStateHash()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<OngoingEffectContext>)debugEvent).entity.EffectTargets;
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
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetGameStateHash());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash());
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
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
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
                    EffectType = EffectType.DEBUG, // Pops debug results
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [searchEffect, debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Add stuff to board. In a random lane, add a unit for each player (1 and 2), in relative tiles 1, and building in relative tile 0
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card
                int lane = _rng.Next(0, 3);
                TargetLocation targetLocation = (TargetLocation)(1 << lane); // Random lane
                ManualInitEntity(state, targetLocation, 1, 2, playerIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 1, 3, opponentIndex, new Unit()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.UNIT },
                });
                ManualInitEntity(state, targetLocation, 0, 4, playerIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
                });
                ManualInitEntity(state, targetLocation, 0, 5, opponentIndex, new Building()
                {
                    EntityPlayInfo = new EntityPlayInfo() { EntityType = EntityType.BUILDING },
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
                    searchEffect.Value = num;
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetGameStateHash(); // Check hash beforehand
                    int prePlayBoardHash = sm.DetailedState.BoardState.GetGameStateHash(); // Check hash beforehand
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
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetGameStateHash()); // Hash rchanged because discard pile changed
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash()); // Hash remains the same as search shouldnt modify board or entities at all
                    List<int> searchResultList = ((EntityEvent<OngoingEffectContext>)debugEvent).entity.EffectTargets;
                    Assert.AreEqual(Math.Abs(num), searchResultList.Count); // Ordinals return a single value regardless
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
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetGameStateHash());
                    Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetGameStateHash());
                }
            }
        }
    }
}
