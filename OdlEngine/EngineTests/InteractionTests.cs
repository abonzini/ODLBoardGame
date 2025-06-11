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
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
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
                state.CurrentState = States.DRAW_PHASE;
                state.CurrentPlayer = player;
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
                    // Before the advance
                    int prePlayHash = sm.DetailedState.GetHashCode(); // Check hash beforehand
                    StepResult res = sm.Step(); // Do my draw phase, trigger advance now
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
    }
}
