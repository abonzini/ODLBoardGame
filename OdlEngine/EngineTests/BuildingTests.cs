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
        public void VerifyBuildingNonTargetabilityEmptyBlueprints()
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
                int testBuilding = -1010000007;
                for (int i = 0; i < 10; i++)
                {
                    // Insert useless building in hand. Building wouldn't have valid targets
                    state.PlayerStates[playerIndex].Hand.InsertCard(testBuilding);
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, CardTargets> optionRes = sm.GetPlayableOptions(testBuilding, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane because there's no target
                Assert.AreEqual(optionRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(optionRes.Item2, CardTargets.INVALID); // Bc invalid...
                CardTargets[] targetTest = [CardTargets.PLAINS, CardTargets.FOREST, CardTargets.MOUNTAIN];
                foreach(CardTargets target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(testBuilding, target);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
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
                int testBuilding = -1012621437; // Useless building that can be used absolutely anywhere
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(testBuilding);
                }
                GameStateMachine sm = new GameStateMachine
                {
                    CardDb = TestCardGenerator.GenerateTestCardGenerator() // Add test cardDb
                };
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, CardTargets> optionRes = sm.GetPlayableOptions(testBuilding, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(optionRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(optionRes.Item2, CardTargets.INVALID); // Bc invalid...
                CardTargets[] targetTest = [CardTargets.PLAINS, CardTargets.FOREST, CardTargets.MOUNTAIN];
                foreach (CardTargets target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(testBuilding, target);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
            }
        }
        [TestMethod]
        public void VerifyPlayabilityOnceTheresUnit()
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
                CardTargets[] targetTest = [CardTargets.PLAINS, CardTargets.FOREST, CardTargets.MOUNTAIN];
                foreach (CardTargets target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, target);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
                // Ok but now I play unit in plains, and building should be playable in plains only
                sm.PlayFromHand(unitId, CardTargets.PLAINS);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                void TryBuild(CardTargets wouldBeValidTarget)
                {
                    CardTargets[] targetTest = [CardTargets.PLAINS, CardTargets.FOREST, CardTargets.MOUNTAIN];
                    foreach (CardTargets target in targetTest)
                    {
                        int prePlayHash = sm.GetDetailedState().GetGameStateHash();
                        Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, target);
                        if (wouldBeValidTarget.HasFlag(target)) // if target is correct
                        {
                            // Building should've played ok
                            Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                            Assert.IsNotNull(playRes.Item2);
                            Assert.AreNotEqual(prePlayHash, sm.GetDetailedState().GetGameStateHash()); // Hash should've changed
                            Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 1); // Player has building
                            Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(target).PlayerBuildingCount[playerIndex], 1); // Lane has building
                            Assert.AreNotEqual(sm.GetDetailedState().BoardState.GetLane(target).GetTileRelative(0, playerIndex).BuildingInTile, -1); // Tile has building
                            Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(target).GetTileRelative(0, playerIndex).BuildingInTileOwner, playerIndex); // Tile has correct buiding owner
                            // Revert this, assert reversion
                            sm.UndoPreviousStep();
                            Assert.AreEqual(prePlayHash, sm.GetDetailedState().GetGameStateHash());
                            Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 0);
                            Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(target).PlayerBuildingCount[playerIndex], 0);
                            Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(target).GetTileRelative(0, playerIndex).BuildingInTile, -1);
                            Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(target).GetTileRelative(0, playerIndex).BuildingInTileOwner, -1);
                        }
                        else
                        {
                            Assert.AreEqual(playRes.Item1, PlayOutcome.INVALID_TARGET); // Would be an error!
                            Assert.IsNull(playRes.Item2); // Bc invalid...
                        }
                    }
                }
                TryBuild(CardTargets.PLAINS);
                // Then in forest
                sm.PlayFromHand(unitId, CardTargets.FOREST);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                TryBuild(CardTargets.ALL_BUT_MOUNTAIN);
                // Finally in mountain
                sm.PlayFromHand(unitId, CardTargets.MOUNTAIN);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                TryBuild(CardTargets.ANY_LANE);
                // And due reversions...
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                TryBuild(CardTargets.ALL_BUT_MOUNTAIN);
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(CardTargets.PLAINS));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(CardTargets.MOUNTAIN));
                TryBuild(CardTargets.PLAINS);
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(res.Item2, CardTargets.INVALID);
                foreach (CardTargets target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, target);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
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
                CardTargets[] targetTest = [CardTargets.PLAINS, CardTargets.FOREST, CardTargets.MOUNTAIN];
                foreach (CardTargets playTarget in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, playTarget);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
                // Ok but now I play unit in plains... and should still be invalid
                sm.PlayFromHand(unitId, target);
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Still fails...
                Assert.AreEqual(res.Item2, CardTargets.INVALID);
                foreach (CardTargets playTarget in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, playTarget);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
                // End turn shuffle
                sm.EndTurn(); // End p1
                sm.Step(); // Draw p2
                sm.EndTurn(); // End p2
                sm.Step(); // Draw (and advance) p1
                // So now the unit advanced, this should pass
                res = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.AreEqual(res.Item2, target);
                // I finally attempt to actually play the building which would only succeed in the right lane
                foreach (CardTargets playTarget in targetTest)
                {
                    int prePlayHash = sm.GetDetailedState().GetGameStateHash();
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, playTarget);
                    if (target.HasFlag(playTarget)) // if target is correct
                    {
                        // Building should've played ok
                        Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                        Assert.IsNotNull(playRes.Item2);
                        Assert.AreNotEqual(prePlayHash, sm.GetDetailedState().GetGameStateHash()); // Hash should've changed
                        Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 1); // Player has building
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(playTarget).PlayerBuildingCount[playerIndex], 1); // Lane has building
                        Assert.AreNotEqual(sm.GetDetailedState().BoardState.GetLane(playTarget).GetTileRelative(1, playerIndex).BuildingInTile, -1); // Tile has building
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(playTarget).GetTileRelative(1, playerIndex).BuildingInTileOwner, playerIndex); // Tile has correct buiding owner
                        // Revert this, assert reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.GetDetailedState().GetGameStateHash());
                        Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 0);
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(playTarget).PlayerBuildingCount[playerIndex], 0);
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(playTarget).GetTileRelative(1, playerIndex).BuildingInTile, -1);
                        Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(playTarget).GetTileRelative(1, playerIndex).BuildingInTileOwner, -1);
                    }
                    else
                    {
                        Assert.AreEqual(playRes.Item1, PlayOutcome.INVALID_TARGET); // Would be an error!
                        Assert.IsNull(playRes.Item2); // Bc invalid...
                    }
                }
            }
        }
        [TestMethod]
        public void BuildingDiesIfBuiltAt0Hp()
        {
            // Summons dead building and verifies that it died
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
                int buildingId = -1002621437; // Building has 0 HP but buildable anywhere
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
                CardTargets laneTarget = (CardTargets)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                sm.PlayFromHand(unitId, laneTarget);
                // Check my building will be buildable
                Tuple<PlayOutcome, CardTargets>  optionRes = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.OK);
                Assert.AreEqual(optionRes.Item2, laneTarget);
                // Pre play, ensure building's not there
                int prePlayBoardHash = sm.GetDetailedState().BoardState.GetGameStateHash();
                int prePlayStateHash = sm.GetDetailedState().GetGameStateHash();
                int nextEntityIndex = sm.GetDetailedState().NextUniqueIndex;
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).PlayerBuildingCount[playerIndex], 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTile, -1);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTileOwner, -1);
                // Now I play the building
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, laneTarget);
                // Post play, building should STILL not be there because it insta-died
                Assert.AreEqual(prePlayBoardHash, sm.GetDetailedState().BoardState.GetGameStateHash()); // Board shouldn't have changed at all
                Assert.AreNotEqual(prePlayStateHash, sm.GetDetailedState().GetGameStateHash()); // Gamestate definitely changed because hands changed, unit, etc
                Assert.AreNotEqual(nextEntityIndex, sm.GetDetailedState().NextUniqueIndex); // Also ensure building was at some point instantiated
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).PlayerBuildingCount[playerIndex], 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTile, -1);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTileOwner, -1);
                // Finally, revert
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayBoardHash, sm.GetDetailedState().BoardState.GetGameStateHash()); // Board shouldn't have changed at all
                Assert.AreEqual(prePlayStateHash, sm.GetDetailedState().GetGameStateHash()); // Gamestate definitely changed because hands changed, unit, etc
                Assert.AreEqual(nextEntityIndex, sm.GetDetailedState().NextUniqueIndex); // Also ensure building was at some point instantiated
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).PlayerBuildingCount[playerIndex], 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTile, -1);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTileOwner, -1);            }
        }
        [TestMethod]
        public void CantBuildOnTopOfBuiding()
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
                int buildingId = -1012621437; // Building buildable anywhere
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
                CardTargets laneTarget = (CardTargets)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                sm.PlayFromHand(unitId, laneTarget);
                // Check my building will be buildable
                Tuple<PlayOutcome, CardTargets> optionRes = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.OK);
                Assert.AreEqual(optionRes.Item2, laneTarget);
                // Now I play the building
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, laneTarget);
                // Check if same building is buildable (shouldn't be, no available target)
                optionRes = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(optionRes.Item2, CardTargets.INVALID);
                // Try build anyway
                playRes = sm.PlayFromHand(buildingId, laneTarget);
                Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.IsNull(playRes.Item2);
            }
        }
        [TestMethod]
        public void CorrectOptionOrderAndBuildingSequence()
        {
            // Units in tiles 1, 2. uilding will be built first in 1 then in 2
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
                int buildingId = -1012621437; // Building buildable anywhere
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
                CardTargets laneTarget = (CardTargets)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                sm.PlayFromHand(unitId, laneTarget);
                // HACK, add the same unit in 2 different tiles to avoid needing to advance
                state = sm.GetDetailedState();
                Tile secondTile = state.BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex);
                secondTile.UnitsInTile.Add(state.BoardState.Units.First().Key); // Add unit also here, this is a weird invalid state but should work for this test
                secondTile.PlayerUnitCount[playerIndex]++;
                // Check my building will be buildable, prepare for playing
                Tuple<PlayOutcome, CardTargets> optionRes = sm.GetPlayableOptions(buildingId, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.OK);
                Assert.AreEqual(optionRes.Item2, laneTarget);
                int prePlayHash1 = sm.GetDetailedState().GetGameStateHash();
                // Now I play the building
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(buildingId, laneTarget);
                Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                Assert.IsNotNull(playRes.Item2);
                int prePlayHash2 = sm.GetDetailedState().GetGameStateHash();
                Assert.AreNotEqual(prePlayHash1, prePlayHash2); // Hash should've changed
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 1); // Player has building
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).PlayerBuildingCount[playerIndex], 1); // Lane has building
                Assert.AreNotEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTile, -1); // First Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTileOwner, playerIndex);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTile, -1); // Second Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTileOwner, -1);
                // Play second building
                playRes = sm.PlayFromHand(buildingId, laneTarget);
                Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                Assert.IsNotNull(playRes.Item2);
                Assert.AreNotEqual(prePlayHash1, sm.GetDetailedState().GetGameStateHash()); // Hash should've changed
                Assert.AreNotEqual(prePlayHash2, sm.GetDetailedState().GetGameStateHash()); // Hash should've changed
                Assert.AreNotEqual(prePlayHash1, prePlayHash2); // Hash should've changed
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 2); // Player has 2 buildings
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).PlayerBuildingCount[playerIndex], 2); // Lane has 2 buildings
                Assert.AreNotEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTile, -1); // First Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTileOwner, playerIndex);
                Assert.AreNotEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTile, -1); // Second Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTileOwner, playerIndex);
                // Revert 2nd Building
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash2, sm.GetDetailedState().GetGameStateHash());
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 1); // Player has 1 building
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).PlayerBuildingCount[playerIndex], 1); // Lane has building
                Assert.AreNotEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTile, -1); // First Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTileOwner, playerIndex);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTile, -1); // Second Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTileOwner, -1);
                // Revert first building
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash1, sm.GetDetailedState().GetGameStateHash());
                Assert.AreEqual(sm.GetDetailedState().PlayerStates[playerIndex].NBuildings, 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).PlayerBuildingCount[playerIndex], 0);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTile, -1); // First Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(0, playerIndex).BuildingInTileOwner, -1);
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTile, -1); // Second Tile
                Assert.AreEqual(sm.GetDetailedState().BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex).BuildingInTileOwner, -1);
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
