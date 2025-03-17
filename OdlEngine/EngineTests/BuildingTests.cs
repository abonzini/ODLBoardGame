using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    [TestClass]
    public class BuildingTests
    {
        // Blueprint targetability
        [TestMethod]
        public void VerifyBuildingNonTargetability()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    // Insert useless building in hand. Building wouldn't have valid targets
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1010000007);
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(-1010000007, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(res.Item2, CardTargets.INVALID); // Bc invalid...
            }
        }
        [TestMethod]
        public void VerifyNonPlayabilityBecauseNoUnit()
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = new GameStateStruct
                {
                    CurrentState = States.ACTION_PHASE,
                    CurrentPlayer = player
                };
                for (int i = 0; i < 10; i++)
                {
                    // Insert useless building that can be used absolutely anywhere
                    state.PlayerStates[playerIndex].Hand.InsertCard(-1012621437);
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(-1012621437, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(res.Item2, CardTargets.INVALID); // Bc invalid...
            }
        }
        [TestMethod]
        public void VerifyPlayabilityOnceUnit()
        {
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
                int buildingId = -1012621437;
                int unitId = -1011117;
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(buildingId);
                state.PlayerStates[playerIndex].Hand.InsertCard(buildingId);
                state.PlayerStates[playerIndex].Hand.InsertCard(buildingId);
                // And 3 cheap units 1-G-HP-ATK-MOV-DENOM-TGT
                state.PlayerStates[playerIndex].Hand.InsertCard(unitId);
                state.PlayerStates[playerIndex].Hand.InsertCard(unitId);
                state.PlayerStates[playerIndex].Hand.InsertCard(unitId);
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(res.Item2, CardTargets.INVALID); // Bc invalid...
                // Ok but now I play unit in plains, and building should be playable in plains only
                sm.PlayFromHand(unitId, CardTargets.PLAINS);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                // Then in forest
                sm.PlayFromHand(unitId, CardTargets.FOREST);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                // Finally in mountain
                sm.PlayFromHand(unitId, CardTargets.MOUNTAIN);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                // And due reversions...
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(res.Item2, CardTargets.INVALID);
            }
        }
        [TestMethod]
        public void VerifyPlayabilityOnceAdvanced()
        {
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
                CardTargets target = (CardTargets)(1 << _rng.Next(3)); // Random target
                int buildingId = -1010020827; // Only playable in second tile of lanes
                int unitId = -1011117;
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(buildingId);
                // And 3 cheap units 1-G-HP-ATK-MOV-DENOM-TGT
                state.PlayerStates[playerIndex].Hand.InsertCard(unitId);
                // Finally add one card to decks to avoid crash
                state.PlayerStates[playerIndex].Deck.InitializeDeck("-107,-107"); // Whatever
                state.PlayerStates[1-playerIndex].Deck.InitializeDeck("-107,-107"); // Whatever
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, CardTargets> res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(res.Item2, CardTargets.INVALID);
                // Ok but now I play unit in plains... and should still be invalid
                sm.PlayFromHand(unitId, target);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Still fails...
                Assert.AreEqual(res.Item2, CardTargets.INVALID);
                // End turn shuffle
                sm.EndTurn(); // End p1
                sm.Step(); // Draw p2
                sm.EndTurn(); // End p2
                sm.Step(); // Draw (and advance) p1
                // So now the unit advanced, this should pass
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.AreEqual(res.Item2, target);
            }
        }
        [TestMethod]
        public void HashTest()
        {
            Building b1, b2;
            b1 = new Building()
            {
                UniqueId = 1,
                Owner = 0,
                LaneCoordinate = LaneID.PLAINS,
                TileCoordinate = 2,
                Hp = 10
            };
            b2 = (Building)b1.Clone();
            Assert.AreEqual(b1.GetGameStateHash(), b2.GetGameStateHash());
            // Now change a few things
            b2.Hp = 1;
            Assert.AreNotEqual(b1.GetGameStateHash(), b2.GetGameStateHash());
            // Revert
            b2.Hp = b1.Hp;
            Assert.AreEqual(b1.GetGameStateHash(), b2.GetGameStateHash());
        }
    }
}
