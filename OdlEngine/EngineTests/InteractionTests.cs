using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, PlayTargetLocation.BOARD);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG_STORE };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, PlayTargetLocation.BOARD); // Play the card
                Assert.AreEqual(PlayOutcome.OK, playRes.Item1);
                Assert.IsNotNull(playRes.Item2);
                bool debugFlagFound = false; // Check if the debug check was added when card was played
                foreach (GameEngineEvent gameEngineEvent in playRes.Item2.events)
                {
                    if (gameEngineEvent.eventType == EventType.DEBUG_CHECK)
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 9, 1);
                // Card 2, a building, just casually placed on first tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, PlayTargetLocation.ALL_LANES, 1, [], [], []);
                // Now add the building into the board
                TestHelperFunctions.ManualInitEntity(state, PlayTargetLocation.PLAINS, 0, 2, playerIndex, building); // Now building is in place
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
                    Tuple<PlayOutcome, StepResult> res = sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play unit anywhere (PLAINS)
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
                    Assert.AreEqual(entityType, ((EntityEvent<CpuState>)debugEvent).entity.CurrentSpecificContext.ActivatedEntity.EntityType);
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TESTUNIT", 0, PlayTargetLocation.ALL_LANES, 1, 1, 9, 1);
                // Card 2, a building, just casually placed on second tile
                Building building = TestCardGenerator.CreateBuilding(2, "TESTBLDG", 0, PlayTargetLocation.ALL_LANES, 1, [], [], []);
                // Now add the building into the board
                TestHelperFunctions.ManualInitEntity(state, PlayTargetLocation.PLAINS, 1, 2, playerIndex, building); // Now building is in place
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
                    sm.PlayFromHand(1, PlayTargetLocation.PLAINS); // Play unit anywhere (PLAINS)
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
                    Assert.AreEqual(entityType, ((EntityEvent<CpuState>)debugEvent).entity.CurrentSpecificContext.ActivatedEntity.EntityType);
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
    }
}
