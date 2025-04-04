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
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, CardTargets.GLOBAL);
                Effect debugEffect = new Effect() { EffectType = EffectType.DEBUG };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [debugEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, CardTargets.GLOBAL); // Play the card
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, CardTargets.ALL_LANES, 1, 1, 1, 1);
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
                sm.PlayFromHand(1, CardTargets.PLAINS); // Play the unit in any lane
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, CardTargets.ALL_LANES, 0, 1, 1, 1);
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
                sm.PlayFromHand(1, CardTargets.PLAINS); // Play the unit in any lane
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
                Unit unit = TestCardGenerator.CreateUnit(1, "TRIG_UNIT", 0, CardTargets.ALL_LANES, 1, 1, 1, 1);
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
                    sm.PlayFromHand(1, CardTargets.PLAINS); // Play the unit in any lane
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
                Skill skill = TestCardGenerator.CreateSkill(1, "WHENPLAYED", 0, CardTargets.GLOBAL);
                Effect summonEffect = new Effect()
                {
                    EffectType = EffectType.SUMMON_UNIT, // Summons unit
                    CardNumber = 2, // Always card 2
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [summonEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                // Card 2: placeholder simple unit
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, CardTargets.ALL_LANES, 1, 1, 1, 1));
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
                List<PlayerTarget> playerTargets = [PlayerTarget.CARD_OWNER, PlayerTarget.CARD_OWNER_OPPONENT];
                foreach (PlayerTarget playerTarget in playerTargets)
                {
                    int ownerPlayer = (playerTarget == PlayerTarget.CARD_OWNER) ? playerIndex : 1 - playerIndex;
                    for (int i = 1; i <= 7;  i++)
                    {
                        var numberOfSummons = i switch
                        {
                            1 or 2 or 4 => 1,
                            3 or 5 or 6 => 2,
                            7 => 3,
                            _ => throw new Exception("Invalid lane what happened here?"),
                        };
                        CardTargets laneTarget = (CardTargets)i;
                        summonEffect.LaneTargets = laneTarget;
                        summonEffect.TargetPlayer = playerTarget;
                        // Pre play tests
                        Assert.AreEqual(state.BoardState.AllUnits.Count, 0);
                        Assert.AreEqual(state.BoardState.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, 0);
                        // Play
                        Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, CardTargets.GLOBAL); // Play the card
                        Assert.AreEqual(PlayOutcome.OK, playRes.Item1);
                        Assert.IsNotNull(playRes.Item2);
                        // Hash assert
                        int newHash = state.GetGameStateHash();
                        Assert.IsFalse(hashes.Contains(newHash));
                        hashes.Add(newHash);
                        // Location assert
                        Assert.AreEqual(state.BoardState.AllUnits.Count, numberOfSummons);
                        Assert.AreEqual(state.BoardState.PlayerUnits[ownerPlayer].Count, numberOfSummons);
                        Assert.AreEqual(state.BoardState.PlainsLane.PlayerUnits[ownerPlayer].Count, laneTarget.HasFlag(CardTargets.PLAINS)?1:0);
                        Assert.AreEqual(state.BoardState.ForestLane.PlayerUnits[ownerPlayer].Count, laneTarget.HasFlag(CardTargets.FOREST) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.MountainLane.PlayerUnits[ownerPlayer].Count, laneTarget.HasFlag(CardTargets.MOUNTAIN) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, laneTarget.HasFlag(CardTargets.PLAINS) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, laneTarget.HasFlag(CardTargets.FOREST) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, laneTarget.HasFlag(CardTargets.MOUNTAIN) ? 1 : 0);
                        // Revert
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, state.GetGameStateHash());
                        Assert.AreEqual(state.BoardState.AllUnits.Count, 0);
                        Assert.AreEqual(state.BoardState.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).PlayerUnits[ownerPlayer].Count, 0);
                    }
                }
            }
        }
    }
}
