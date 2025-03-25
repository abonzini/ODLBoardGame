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
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, CardTargets.ANY_LANE, 1, 1, 1, 1));
                state.PlayerStates[playerIndex].Hand.InsertCard(1); // Add card to hand
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Beginning of test loop:
                // interaction will be modified for target player and target lane(s)
                // Resulting hash board will be compared with pre-play hash and non-repeated
                // Will check: Player count, lane count, tile count, init unit count
                state = sm.GetDetailedState();
                int prePlayHash = state.GetGameStateHash();
                HashSet<int> hashes = new HashSet<int>();
                hashes.Add(prePlayHash);
                List<PlayerTarget> playerTargets = [PlayerTarget.CARD_PLAYER, PlayerTarget.CARD_PLAYER_OPPONENT];
                foreach (PlayerTarget playerTarget in playerTargets)
                {
                    int ownerPlayer = (playerTarget == PlayerTarget.CARD_PLAYER) ? playerIndex : 1 - playerIndex;
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
                        Assert.AreEqual(state.BoardState.Units.Count, 0);
                        Assert.AreEqual(state.PlayerStates[ownerPlayer].NUnits, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.ForestLane.PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.MountainLane.PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], 0);
                        // Play
                        Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, CardTargets.GLOBAL); // Play the card
                        Assert.AreEqual(PlayOutcome.OK, playRes.Item1);
                        Assert.IsNotNull(playRes.Item2);
                        // Hash assert
                        int newHash = state.GetGameStateHash();
                        Assert.IsFalse(hashes.Contains(newHash));
                        hashes.Add(newHash);
                        // Location assert
                        Assert.AreEqual(state.BoardState.Units.Count, numberOfSummons);
                        Assert.AreEqual(state.PlayerStates[ownerPlayer].NUnits, numberOfSummons);
                        Assert.AreEqual(state.BoardState.PlainsLane.PlayerUnitCount[ownerPlayer], laneTarget.HasFlag(CardTargets.PLAINS)?1:0);
                        Assert.AreEqual(state.BoardState.ForestLane.PlayerUnitCount[ownerPlayer], laneTarget.HasFlag(CardTargets.FOREST) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.MountainLane.PlayerUnitCount[ownerPlayer], laneTarget.HasFlag(CardTargets.MOUNTAIN) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], laneTarget.HasFlag(CardTargets.PLAINS) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], laneTarget.HasFlag(CardTargets.FOREST) ? 1 : 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], laneTarget.HasFlag(CardTargets.MOUNTAIN) ? 1 : 0);
                        // Revert
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, state.GetGameStateHash());
                        Assert.AreEqual(state.BoardState.Units.Count, 0);
                        Assert.AreEqual(state.PlayerStates[ownerPlayer].NUnits, 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.ForestLane.PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.MountainLane.PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.PlainsLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.ForestLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], 0);
                        Assert.AreEqual(state.BoardState.MountainLane.GetTileRelative(0, ownerPlayer).PlayerUnitCount[ownerPlayer], 0);
                    }
                }
            }
        }
    }
}
