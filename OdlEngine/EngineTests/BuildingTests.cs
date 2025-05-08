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
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that cant be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1,"TEST", 0, TargetLocation.ALL_LANES, 1, [], [], []));
                for (int i = 0; i < 10; i++)
                {
                    // Insert useless building in hand. Building wouldn't have valid targets
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, TargetLocation> optionRes = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane because there's no target
                Assert.AreEqual(optionRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(optionRes.Item2, TargetLocation.INVALID); // Bc invalid...
                TargetLocation[] targetTest = [TargetLocation.PLAINS, TargetLocation.FOREST, TargetLocation.MOUNTAIN];
                foreach(TargetLocation target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, target);
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
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be built anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, TargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                for (int i = 0; i < 10; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, TargetLocation> optionRes = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(optionRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(optionRes.Item2, TargetLocation.INVALID); // Bc invalid...
                TargetLocation[] targetTest = [TargetLocation.PLAINS, TargetLocation.FOREST, TargetLocation.MOUNTAIN];
                foreach (TargetLocation target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, target);
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
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, TargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Card 2: Basic unit
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1));
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                // And 3 cheap units 1-G-HP-ATK-MOV-DENOM-TGT
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, TargetLocation> res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                Assert.AreEqual(res.Item2, TargetLocation.INVALID); // Bc invalid...
                TargetLocation[] targetTest = [TargetLocation.PLAINS, TargetLocation.FOREST, TargetLocation.MOUNTAIN];
                foreach (TargetLocation target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, target);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
                // Ok but now I play unit in plains, and building should be playable in plains only
                sm.PlayFromHand(2, TargetLocation.PLAINS);
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.PLAINS));
                Assert.IsFalse(res.Item2.HasFlag(TargetLocation.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(TargetLocation.MOUNTAIN));
                void TryBuild(TargetLocation wouldBeValidTarget)
                {
                    TargetLocation[] targetTest = [TargetLocation.PLAINS, TargetLocation.FOREST, TargetLocation.MOUNTAIN];
                    foreach (TargetLocation target in targetTest)
                    {
                        int prePlayHash = sm.DetailedState.GetHashCode();
                        Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, target);
                        if (wouldBeValidTarget.HasFlag(target)) // if target is correct
                        {
                            // Building should've played ok
                            Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                            Assert.IsNotNull(playRes.Item2);
                            Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash should've changed
                            Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Player has building
                            Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Lane has building
                            // Revert this, assert reversion
                            sm.UndoPreviousStep();
                            Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                            Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                            Assert.AreEqual(sm.DetailedState.BoardState.GetLane(target).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                        }
                        else
                        {
                            Assert.AreEqual(playRes.Item1, PlayOutcome.INVALID_TARGET); // Would be an error!
                            Assert.IsNull(playRes.Item2); // Bc invalid...
                        }
                    }
                }
                TryBuild(TargetLocation.PLAINS);
                // Then in forest
                sm.PlayFromHand(2, TargetLocation.FOREST);
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(TargetLocation.MOUNTAIN));
                TryBuild(TargetLocation.ALL_BUT_MOUNTAIN);
                // Finally in mountain
                sm.PlayFromHand(2, TargetLocation.MOUNTAIN);
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.FOREST));
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.MOUNTAIN));
                TryBuild(TargetLocation.ALL_LANES);
                // And due reversions...
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.PLAINS));
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(TargetLocation.MOUNTAIN));
                TryBuild(TargetLocation.ALL_BUT_MOUNTAIN);
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.IsTrue(res.Item2.HasFlag(TargetLocation.PLAINS));
                Assert.IsFalse(res.Item2.HasFlag(TargetLocation.FOREST));
                Assert.IsFalse(res.Item2.HasFlag(TargetLocation.MOUNTAIN));
                TryBuild(TargetLocation.PLAINS);
                sm.UndoPreviousStep();
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(res.Item2, TargetLocation.INVALID);
                foreach (TargetLocation target in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, target);
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
                TargetLocation target = (TargetLocation)(1 << _rng.Next(3)); // Random target
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted only in tile 2
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, TargetLocation.ALL_LANES, 1, [1], [1], [1]));
                // Card 2: Basic unit
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1));
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                // And 3 cheap units 1-G-HP-ATK-MOV-DENOM-TGT
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                // Finally add one card to decks to avoid crash
                state.PlayerStates[playerIndex].Deck.InitializeDeck("-107,-107"); // Whatever
                state.PlayerStates[1-playerIndex].Deck.InitializeDeck("-107,-107"); // Whatever
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                Tuple<PlayOutcome, TargetLocation> res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                // Card should not be playable in any lane, but because it's missing the unit!
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(res.Item2, TargetLocation.INVALID);
                TargetLocation[] targetTest = [TargetLocation.PLAINS, TargetLocation.FOREST, TargetLocation.MOUNTAIN];
                foreach (TargetLocation playTarget in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, playTarget);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
                // Ok but now I play unit in plains... and should still be invalid
                sm.PlayFromHand(2, target);
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Still fails...
                Assert.AreEqual(res.Item2, TargetLocation.INVALID);
                foreach (TargetLocation playTarget in targetTest)
                {
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, playTarget);
                    Assert.AreEqual(playRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                    Assert.IsNull(playRes.Item2); // Bc invalid...
                }
                // End turn shuffle
                sm.EndTurn(); // End p1
                sm.Step(); // Draw p2
                sm.EndTurn(); // End p2
                sm.Step(); // Draw (and advance) p1
                // So now the unit advanced, this should pass
                res = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(res.Item1, PlayOutcome.OK);
                Assert.AreEqual(res.Item2, target);
                // I finally attempt to actually play the building which would only succeed in the right lane
                foreach (TargetLocation playTarget in targetTest)
                {
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, playTarget);
                    if (target.HasFlag(playTarget)) // if target is correct
                    {
                        // Building should've played ok
                        Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                        Assert.IsNotNull(playRes.Item2);
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode()); // Hash should've changed
                        Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Player has building
                        Assert.AreEqual(sm.DetailedState.BoardState.GetLane(playTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Lane has building
                        // Revert this, assert reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                        Assert.AreEqual(sm.DetailedState.BoardState.GetLane(playTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
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
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere but has 0 hp
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, TargetLocation.ALL_LANES, 0, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Card 2: Basic unit
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1));
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                // And 3 cheap units 1-G-HP-ATK-MOV-DENOM-TGT
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                TargetLocation laneTarget = (TargetLocation)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                sm.PlayFromHand(2, laneTarget);
                // Check my building will be buildable
                Tuple<PlayOutcome, TargetLocation> optionRes = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.OK);
                Assert.AreEqual(optionRes.Item2, laneTarget);
                // Pre play, ensure building's not there
                int prePlayBoardHash = sm.DetailedState.BoardState.GetHashCode();
                int prePlayStateHash = sm.DetailedState.GetHashCode();
                int nextEntityIndex = sm.DetailedState.NextUniqueIndex;
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                // Now I play the building
                sm.PlayFromHand(1, laneTarget);
                // Post play, building should STILL not be there because it insta-died
                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Board shouldn't have changed at all
                Assert.AreNotEqual(prePlayStateHash, sm.DetailedState.GetHashCode()); // Gamestate definitely changed because hands changed, unit, etc
                Assert.AreNotEqual(nextEntityIndex, sm.DetailedState.NextUniqueIndex); // Also ensure building was at some point instantiated
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                // Finally, revert
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Board shouldn't have changed at all
                Assert.AreEqual(prePlayStateHash, sm.DetailedState.GetHashCode()); // Gamestate definitely changed because hands changed, unit, etc
                Assert.AreEqual(nextEntityIndex, sm.DetailedState.NextUniqueIndex); // Also ensure building was at some point instantiated
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
            }
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
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, TargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Card 2: Basic unit
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1));
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                // And 3 cheap units 1-G-HP-ATK-MOV-DENOM-TGT
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                TargetLocation laneTarget = (TargetLocation)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                sm.PlayFromHand(2, laneTarget);
                // Check my building will be buildable
                Tuple<PlayOutcome, TargetLocation> optionRes = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.OK);
                Assert.AreEqual(optionRes.Item2, laneTarget);
                // Now I play the building
                sm.PlayFromHand(1, laneTarget);
                // Check if same building is buildable (shouldn't be, no available target)
                optionRes = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.NO_TARGET_AVAILABLE);
                Assert.AreEqual(optionRes.Item2, TargetLocation.INVALID);
                // Try build anyway
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, laneTarget);
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
                CardFinder cardDb = new CardFinder();
                // Card 1: test building that can be targeted anywhere
                cardDb.InjectCard(1, TestCardGenerator.CreateBuilding(1, "TEST", 0, TargetLocation.ALL_LANES, 1, [0, 1, 2, 3], [0, 1, 2, 3, 4, 5], [0, 1, 2, 3, 4, 5, 6, 7]));
                // Card 2: Basic unit
                cardDb.InjectCard(2, TestCardGenerator.CreateUnit(2, "UNIT", 0, TargetLocation.ALL_LANES, 1, 1, 1, 1));
                // Insert 3 buildings
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                // And 3 cheap units 1-G-HP-ATK-MOV-DENOM-TGT
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                state.PlayerStates[playerIndex].Hand.InsertCard(2);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                TargetLocation laneTarget = (TargetLocation)(1 << _rng.Next(3)); // Random target
                // Play unit in lane
                sm.PlayFromHand(2, laneTarget);
                // HACK, add the same unit in 2 different tiles to avoid needing to advance
                state = sm.DetailedState;
                Tile secondTile = state.BoardState.GetLane(laneTarget).GetTileRelative(1, playerIndex);
                secondTile.EntityListOperation((PlacedEntity)state.EntityData[state.BoardState.GetPlacedEntities(EntityType.UNIT).First()], EntityListOperation.ADD); // Add unit also here, this is a weird invalid state but should work for this test
                // Check my building will be buildable, prepare for playing
                Tuple<PlayOutcome, TargetLocation> optionRes = sm.GetPlayableOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(optionRes.Item1, PlayOutcome.OK);
                Assert.AreEqual(optionRes.Item2, laneTarget);
                int prePlayHash1 = sm.DetailedState.GetHashCode();
                // Now I play the building
                Tuple<PlayOutcome, StepResult> playRes = sm.PlayFromHand(1, laneTarget);
                Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                Assert.IsNotNull(playRes.Item2);
                int prePlayHash2 = sm.DetailedState.GetHashCode();
                Assert.AreNotEqual(prePlayHash1, prePlayHash2); // Hash should've changed
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Player has building
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Lane has building
                // Play second building
                playRes = sm.PlayFromHand(1, laneTarget);
                Assert.AreEqual(playRes.Item1, PlayOutcome.OK);
                Assert.IsNotNull(playRes.Item2);
                Assert.AreNotEqual(prePlayHash1, sm.DetailedState.GetHashCode()); // Hash should've changed
                Assert.AreNotEqual(prePlayHash2, sm.DetailedState.GetHashCode()); // Hash should've changed
                Assert.AreNotEqual(prePlayHash1, prePlayHash2); // Hash should've changed
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 2); // Player has 2 buildings
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 2); // Lane has 2 buildings
                // Revert 2nd Building
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash2, sm.DetailedState.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Player has 1 building
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 1); // Lane has building
                // Revert first building
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash1, sm.DetailedState.GetHashCode());
                Assert.AreEqual(sm.DetailedState.BoardState.GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
                Assert.AreEqual(sm.DetailedState.BoardState.GetLane(laneTarget).GetPlacedEntities(EntityType.BUILDING, playerIndex).Count, 0);
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
                TileCoordinate = 2
            };
            b1.Hp.BaseValue = 10;
            b2 = (Building)b1.Clone();
            Assert.AreEqual(b1.GetHashCode(), b2.GetHashCode());
            // Now change a few things
            b2.Hp.BaseValue = 1;
            Assert.AreNotEqual(b1.GetHashCode(), b2.GetHashCode());
            // Revert
            b2.Hp.BaseValue = b1.Hp.BaseValue;
            Assert.AreEqual(b1.GetHashCode(), b2.GetHashCode());
        }
    }
}
