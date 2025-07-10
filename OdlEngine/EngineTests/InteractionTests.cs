using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class InteractionTests
    {
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
                Skill skill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD);
                Effect debugEffect = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayContext, StepResult> playRes = sm.PlayFromHand(1, 0); // Play the card
                Assert.AreEqual(PlayOutcome.OK, playRes.Item1.PlayOutcome);
                Assert.IsNotNull(playRes.Item2);
                // Check if debug event is there
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(playRes.Item2);
                Assert.IsNotNull(cpu);
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, [0, 4, 10], 1, 1, 9, 1);
                // Card 2, a building, just casually placed on first tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, [], 1);
                // Now add the building into the board
                int tileCoord = (playerIndex == 0) ? 0 : 3; // Puts in the first plains lane (RELATIVE TO PLAYER!)
                TestHelperFunctions.ManualInitEntity(state, tileCoord, 2, playerIndex, building); // Now building is in place
                state.NextUniqueIndex = 3;
                // Create the "on step" effect
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE, // Pops debug results, useful
                };
                Dictionary<InteractionType, List<Effect>> stepInteraction = new Dictionary<InteractionType, List<Effect>>();
                stepInteraction.Add(InteractionType.UNIT_ENTERS_BUILDING, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Add card
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
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, tileCoord); // Play unit anywhere (PLAINS)
                    Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                    // Check if debug event is there
                    CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2);
                    Assert.IsNotNull(cpu);
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                    // Want to make sure the entity activated is speicfically the building OR the unit (whatever im tracking)
                    Assert.AreEqual(entityType, cpu.CurrentSpecificContext.ActivatedEntity.EntityType);
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
                state.CurrentPlayer = (CurrentPlayer)opponentIndex; // So I can end turn
                state.PlayerStates[0].Hp.BaseValue = 30; // Just in case
                state.PlayerStates[1].Hp.BaseValue = 30;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats (movt 9)
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, [0, 4, 10], 1, 1, 9, 1);
                // Card 2, a building, just casually placed on second tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, [], 1);
                // Now add the building into the board
                int tileCoord = (playerIndex == 0) ? 1 : 2; // Puts in the second plains lane (RELATIVE TO PLAYER!)
                TestHelperFunctions.ManualInitEntity(state, tileCoord, 2, playerIndex, building); // Now building is in place
                tileCoord = (playerIndex == 0) ? 0 : 3; // Puts in the first plains lane (RELATIVE TO PLAYER!)
                TestHelperFunctions.ManualInitEntity(state, tileCoord, 3, playerIndex, unit); // Now unit is there too
                // Create the "on step" effect
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE, // Pops debug results, useful
                };
                Dictionary<InteractionType, List<Effect>> stepInteraction = new Dictionary<InteractionType, List<Effect>>();
                stepInteraction.Add(InteractionType.UNIT_ENTERS_BUILDING, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, unit); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Add card
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
                    // Before the advance
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    StepResult res = sm.EndTurn(); // Do my draw phase, trigger advance now
                    // Check if debug event is there
                    CpuState cpu = TestHelperFunctions.FetchDebugEvent(res);
                    Assert.IsNotNull(cpu);
                    // Check returned targets
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                    // Want to make sure the entity activated is speicfically the building OR the unit (whatever im tracking)
                    Assert.AreEqual(entityType, cpu.CurrentSpecificContext.ActivatedEntity.EntityType);
                    // Revert EVERYTHING and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void PostDamageInteractionCombatUnitVUnit()
        {
            // Checks if combat between 2 units causes a post-damage effect
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = (CurrentPlayer)opponentIndex; // So I can end turn
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that pushes debug effect when damaging something
                Unit unit = TestCardGenerator.CreateUnit(1, "DAMAGE_TRIGGER_UNITS", 0, [0, 4, 10], 2, 1, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE,
                };
                unit.Interactions = new Dictionary<InteractionType, List<Effect>>();
                unit.Interactions.Add(InteractionType.POST_DAMAGE, [debugEffect]);
                // I'll load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Now add the units in the board
                int tileCoord = 1; // Wherever
                sm.UNIT_PlayUnit(playerIndex, new PlayContext() { Actor = unit, PlayedTarget = tileCoord }); // For p1
                sm.UNIT_PlayUnit(opponentIndex, new PlayContext() { Actor = unit, PlayedTarget = tileCoord }); // For p2
                sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext()); // Trigger debug event to safely close the step result
                // Before the advance
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                StepResult res = sm.EndTurn(); // Do my draw phase, trigger advance now
                // Check if debug event is there
                List<CpuState> cpus = TestHelperFunctions.FetchDebugEvents(res);
                Assert.AreEqual(2, cpus.Count);
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                // Want to make sure the entity activated order is speicfically first me and then the opp unit
                Assert.AreEqual(playerIndex, cpus[0].CurrentSpecificContext.ActivatedEntity.Owner);
                Assert.AreEqual(opponentIndex, cpus[1].CurrentSpecificContext.ActivatedEntity.Owner);
                // Revert EVERYTHING and hash check
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void PostDamageInteractionCombatUnitVPlayer()
        {
            // Checks if combat between unit and player results in the proper damage step
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = (CurrentPlayer)opponentIndex; // So I can end turn
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit that pushes debug effect when damaging something
                Unit unit = TestCardGenerator.CreateUnit(1, "DAMAGE_TRIGGER_UNITS", 0, [0, 4, 10], 2, 1, 1, 1);
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE,
                };
                unit.Interactions = new Dictionary<InteractionType, List<Effect>>();
                unit.Interactions.Add(InteractionType.POST_DAMAGE, [debugEffect]);
                // I'll load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Now add the units in the board
                int tileCoord = sm.DetailedState.BoardState.PlainsLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, -1, playerIndex); // Get this player's end tile (wanna damage)
                sm.UNIT_PlayUnit(playerIndex, new PlayContext() { Actor = unit, PlayedTarget = tileCoord }); // For p1
                sm.TestActivateTrigger(TriggerType.ON_DEBUG_TRIGGERED, EffectLocation.BOARD, new EffectContext()); // Trigger debug event to safely close the step result
                // Before the advance
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                StepResult res = sm.EndTurn(); // Do my draw phase, trigger advance now
                // Check if debug event is there
                List<CpuState> cpus = TestHelperFunctions.FetchDebugEvents(res);
                Assert.AreEqual(1, cpus.Count);
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                // Want to make sure the entity damaged specifically the opponent player
                Assert.AreEqual(playerIndex, cpus[0].CurrentSpecificContext.ActivatedEntity.Owner);
                Assert.AreEqual(EntityType.PLAYER, ((DamageContext)cpus[0].CurrentSpecificContext).Affected.EntityType);
                Assert.AreEqual(opponentIndex, ((DamageContext)cpus[0].CurrentSpecificContext).Affected.Owner);
                // Revert EVERYTHING and hash check
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void PostEffectDamageInteraction()
        {
            // Checks if a skill doing effect damage also triggers PostDamage step
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that deals 1 damage and then triggers itself POST damage to push debug
                Skill skill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD);
                Effect chooseBoard = new Effect()
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.BOARD
                };
                Effect chooseEnemyPlayer = new Effect()
                {
                    EffectType = EffectType.FIND_ENTITIES,
                    SearchCriterion = SearchCriterion.ALL, // All of them,
                    TargetPlayer = EntityOwner.OPPONENT, // Enemy Player
                    TargetType = EntityType.PLAYER,
                };
                Effect hitEnemy = new Effect()
                {
                    EffectType = EffectType.EFFECT_DAMAGE,
                    TempVariable = 1,
                    Input = Variable.TEMP_VARIABLE,
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE,
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [chooseBoard, chooseEnemyPlayer, hitEnemy]);
                skill.Interactions.Add(InteractionType.POST_DAMAGE, [debugEffect]);
                cardDb.InjectCard(1, skill);
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                // I'll load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Before the advance
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, 0);
                // Check if debug event is there
                List<CpuState> cpus = TestHelperFunctions.FetchDebugEvents(res.Item2);
                Assert.AreEqual(1, cpus.Count);
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                // Want to make sure the entity damaged specifically the opponent player
                Assert.AreEqual(playerIndex, cpus[0].CurrentSpecificContext.ActivatedEntity.Owner);
                Assert.AreEqual(EntityType.SKILL, cpus[0].CurrentSpecificContext.ActivatedEntity.EntityType);
                Assert.AreEqual(EntityType.PLAYER, ((DamageContext)cpus[0].CurrentSpecificContext).Affected.EntityType);
                Assert.AreEqual(opponentIndex, ((DamageContext)cpus[0].CurrentSpecificContext).Affected.Owner);
                // Revert EVERYTHING and hash check
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void PreDamageInteraction()
        {
            // A skill that is about to deal damage but interrupts itself and changes the damage last moment
            // Proves pre-damage and also that it can modify a damage outcome
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int opponentIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Skill that deals 1 damage and then triggers itself PRE damage to push debug
                Skill skill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD);
                Effect chooseBoard = new Effect()
                {
                    EffectType = EffectType.ADD_LOCATION_REFERENCE,
                    EffectLocation = EffectLocation.BOARD
                };
                Effect chooseEnemyPlayer = new Effect()
                {
                    EffectType = EffectType.FIND_ENTITIES,
                    SearchCriterion = SearchCriterion.ALL, // All of them,
                    TargetPlayer = EntityOwner.OPPONENT, // Enemy Player
                    TargetType = EntityType.PLAYER,
                };
                int damageAmountInitial = _rng.Next(2, GameConstants.STARTING_HP); // Will try to do a damage between 2-19
                Effect hitEnemy = new Effect()
                {
                    EffectType = EffectType.EFFECT_DAMAGE,
                    TempVariable = damageAmountInitial,
                    Input = Variable.TEMP_VARIABLE,
                };
                Effect overrideDamage = new Effect() // Overrides damage to 1
                {
                    EffectType = EffectType.MODIFIER,
                    Input = Variable.TEMP_VARIABLE,
                    Output = Variable.DAMAGE_AMOUNT,
                    ModifierOperation = ModifierOperation.SET,
                    TempVariable = 1
                };
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE,
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [chooseBoard, chooseEnemyPlayer, hitEnemy]);
                skill.Interactions.Add(InteractionType.PRE_DAMAGE, [overrideDamage, debugEffect]);
                cardDb.InjectCard(1, skill);
                state.PlayerStates[playerIndex].Hand.AddToCollection(1);
                // I'll load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Before the advance
                int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, 0);
                // Check if debug event is there
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2);
                Assert.IsNotNull(cpu);
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash obviously changed
                // Want to make sure the entity damaged has a single damage counter and not the initial damage in mind
                Assert.AreEqual(1, sm.DetailedState.PlayerStates[opponentIndex].DamageTokens);
                Assert.AreNotEqual(damageAmountInitial, sm.DetailedState.PlayerStates[opponentIndex].DamageTokens);
                // Revert EVERYTHING and hash check
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void BuildingConstructionInteraction()
        {
            Random _rng = new Random();
            // Interaction when a unit constructs a building
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Cards
                CardFinder cardDb = new CardFinder();
                // Card 1: Unit with very basic stats (movt 9)
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, [], 1, 0, 1, 1);
                // Card 2, Building, can be built anywhere at all
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17], 1);
                // Now add unit anywhere in the board
                int tileCoord = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                TestHelperFunctions.ManualInitEntity(state, tileCoord, 2, playerIndex, unit); // Now building is in place
                state.NextUniqueIndex = 3;
                // Create the "on step" effect
                Effect debugEffect = new Effect()
                {
                    EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE, // Pops debug results, useful
                };
                Dictionary<InteractionType, List<Effect>> constructInteraction = new Dictionary<InteractionType, List<Effect>>();
                constructInteraction.Add(InteractionType.UNIT_CONSTRUCTS_BUILDING, [debugEffect]); // Add interaction
                // Will add building to card to be buildable
                cardDb.InjectCard(2, building); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.AddToCollection(2); // Add card
                // I'll load the game
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                unit = (Unit)sm.DetailedState.EntityData[2]; // Retureve the actual unit
                for (int i = 0; i < 4; i++) // Will test all combinations, unit and/or building
                {
                    int expectedInteractions = 0;
                    if ((i & 0b01) != 0)
                    {
                        expectedInteractions++;
                        unit.Interactions = constructInteraction;
                    }
                    else
                    {
                        unit.Interactions = null;
                    }
                    if ((i & 0b10) != 0)
                    {
                        expectedInteractions++;
                        building.Interactions = constructInteraction;
                    }
                    else
                    {
                        building.Interactions = null;
                    }
                    // Pre-play prep
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    PlayContext ctxOptions = sm.GetPlayabilityOptions(2, PlayType.PLAY_FROM_HAND);
                    Assert.IsTrue(ctxOptions.ValidTargets.Contains(2)); // Can play on unit 2
                    // Play
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(2, 2); // Play building, built by unit 2
                    Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                    Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    // Check if debug event(s) is(are) there
                    List<CpuState> cpus = TestHelperFunctions.FetchDebugEvents(res.Item2);
                    Assert.AreEqual(expectedInteractions, cpus.Count); // Triggered as many times as it had to
                    // Revert and hash check
                    sm.UndoPreviousStep();
                    Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                }
            }
        }
    }
}
