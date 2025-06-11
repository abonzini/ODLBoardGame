using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class PlayabilityAndTargetingTests
    {
        [TestMethod]
        public void VerifyAffordability() // Generate costs 0-9, half can be afforable, half can't
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                for (int i = 0; i < 10; i++)
                {
                    // Cards 0-9: test skills with various costs
                    cardDb.InjectCard(i, TestCardGenerator.CreateSkill(i, i, [], CardTargetingType.BOARD));
                    state.PlayerStates[playerIndex].Hand.InsertCard(i); // Insert test cards (brick) in hand costs 0-9
                }
                state.PlayerStates[playerIndex].CurrentGold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount; i++) // Check for each card
                {
                    PlayContext res = sm.GetPlayabilityOptions(i, PlayType.PLAY_FROM_HAND);
                    if (i <= 4)
                    {
                        Assert.AreEqual(res.PlayOutcome, PlayOutcome.OK); // Could be played
                    }
                    else
                    {
                        Assert.AreEqual(res.PlayOutcome, PlayOutcome.CANT_AFFORD); // Could not
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyCorrectPlayabilityState() // Verifies if card playable in incorrect phase
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                List<States> ls = [.. Enum.GetValues<States>()];
                foreach (States st in ls)
                {
                    GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                    state.CurrentState = st;
                    state.CurrentPlayer = player;
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert only one card, I don't care
                    GameStateMachine sm = new GameStateMachine();
                    sm.LoadGame(state); // Start from here
                    if (st != States.ACTION_PHASE) // Only check invalid states as valid state is used elsewhere during tests. Card itself shouldnt be checked
                    {
                        PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                        Assert.AreEqual(res.PlayOutcome, PlayOutcome.INVALID_GAME_STATE);
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyCardIndexValid() // Generate targets 0-4, verify i can only access valid cards
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD));

                for (int i = 0; i < 5; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert test card (brick) in hand all targets 5 times
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                List<int> possibleCards = [1, 1, 1, 1, 1, 2, 2, 2, 2, 2]; // First 5 are ok, last 5 are not
                foreach (int card in possibleCards) // Check for each card
                {
                    if (card == 2) // Only test incorrect ones as correct ones are in another test
                    {
                        PlayContext res = sm.GetPlayabilityOptions(card, PlayType.PLAY_FROM_HAND);
                        Assert.AreEqual(res.PlayOutcome, PlayOutcome.INVALID_CARD); // Would be an error!
                    }
                }
            }
        }
        [TestMethod]
        public void IfNonValidTargets()
        {
            // Creates a skill with no valid targets
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, null, CardTargetingType.BOARD);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.PlayOutcome); // OK (playable)
            }
        }

        [TestMethod]
        public void PlayCosts() // Verifies card cost is paid and card is discarded
        {
            // Creates a skill with BOARD target, plays it, check it was paid
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                state.PlayerStates[playerIndex].CurrentGold = 5; // Player has 5 gold
                // Play
                for(int cost = 0; cost < 10; cost++)
                {
                    boardTargetableSkill.Cost = cost; // Card is now a new price
                    GameStateMachine sm = new GameStateMachine(cardDb);
                    sm.LoadGame(state); // Start from here
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    Assert.AreEqual(1, state.PlayerStates[playerIndex].Hand.CardCount);
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, 0); // Plays (board target)
                    if(cost <= 5) // Should've been paid
                    {
                        Assert.AreEqual(0, state.PlayerStates[playerIndex].Hand.CardCount); // Used the card
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2);
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNotNull(cpu);
                        Assert.AreEqual(0, res.Item1.PlayedTarget);
                        Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                        // Reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                    else // No play
                    {
                        Assert.AreEqual(1, state.PlayerStates[playerIndex].Hand.CardCount); // Used the card
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNull(res.Item2);
                        Assert.AreEqual(PlayOutcome.CANT_AFFORD, res.Item1.PlayOutcome);
                    }    
                }
            }
        }

        // --------------------------------------------------------------------------------------
        // -------------------------------  TARGETING CASES -------------------------------------
        // --------------------------------------------------------------------------------------

        // BOARD TARGETING
        [TestMethod]
        public void VerifyValidBoardTargets()
        {
            // Creates a skill with BOARD target, check playability, should obtain playability as board
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(1, res.ValidTargets.Count); // Verify a single valid target
                Assert.IsTrue(res.ValidTargets.Contains(0)); // Only target should be 0
                Assert.AreEqual(PlayOutcome.OK, res.PlayOutcome); // OK (playable)
            }
        }
        [TestMethod]
        public void VerifyPlayBoardTargets()
        {
            // Creates a skill with BOARD target, plays it, checks if play was ok
            // Playability verified by checking debug content
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                // Play
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                int prePlayHash = sm.DetailedState.GetHashCode();
                Tuple<PlayContext, StepResult>  res = sm.PlayFromHand(1, 0); // Plays (board target)
                CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2);
                // Asserts
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                Assert.IsNotNull(cpu);
                Assert.AreEqual(0, res.Item1.PlayedTarget);
                Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                // Reversion
                sm.UndoPreviousStep();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
            }
        }
        [TestMethod]
        public void VerifyInvalidPlayBoardTargets()
        {
            // Creates a skill with BOARD target, plays it in a wrong place
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.BOARD);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                // Play
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                int prePlayHash = sm.DetailedState.GetHashCode();
                Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, 1); // Play (wrong place)
                // Asserts
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                Assert.IsNull(res.Item2);
                Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.Item1.PlayOutcome);
            }
        }

        // LANE TARGETING
        [TestMethod]
        public void VerifyValidLaneTargets()
        {
            // Creates a skill with LANE target, add all combinations of possible lanes and a few invalid ones
            // Ensure playable only in the existing defined lanes
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.LANE);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                for(int i = 0; i < 16; i++) // All combinations of valid lanes and one invalid
                {
                    int realLaneNumber = 0;
                    HashSet<int> targets = new HashSet<int>();
                    if ((i & 0b0001) != 0) { targets.Add(0); realLaneNumber++; };
                    if ((i & 0b0010) != 0) { targets.Add(1); realLaneNumber++; };
                    if ((i & 0b0100) != 0) { targets.Add(2); realLaneNumber++; };
                    if ((i & 0b1000) != 0) targets.Add(3); // Non existing lane 3
                    boardTargetableSkill.TargetOptions = targets;
                    PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                    if(realLaneNumber == 0) // Should be no valid lanes
                    {
                        Assert.AreEqual(0, res.ValidTargets.Count);
                        Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.PlayOutcome);
                    }
                    else // Should be some valid lanes and playable, but filters out the invalid
                    {
                        Assert.AreEqual(realLaneNumber, res.ValidTargets.Count);
                        Assert.AreEqual(PlayOutcome.OK, res.PlayOutcome);
                        foreach(int target in targets)
                        {
                            if(target < GameConstants.BOARD_NUMBER_OF_LANES)
                            {
                                Assert.IsTrue(res.ValidTargets.Contains(target));
                            }
                            else
                            {
                                Assert.IsFalse(res.ValidTargets.Contains(target));
                            }
                        }
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyPlayLaneTargets()
        {
            // Creates a skill with LANE target, add all combinations of possible lanes, play in each
            // Ensure plays or not properly
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, [], CardTargetingType.LANE);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < 8; i++) // All combinations of valid lanes and one invalid
                {
                    HashSet<int> targets = new HashSet<int>();
                    if ((i & 0b0001) != 0) targets.Add(0);
                    if ((i & 0b0010) != 0) targets.Add(1);
                    if ((i & 0b0100) != 0) targets.Add(2);
                    boardTargetableSkill.TargetOptions = targets;
                    for(int j = 0; j < GameConstants.BOARD_NUMBER_OF_LANES; j++) // Play in each
                    {
                        int prePlayHash = sm.DetailedState.GetHashCode();
                        Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, j); // Play in this target
                        if (targets.Contains(j)) // This was a valid target, ensure played correctly
                        {
                            CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2); // See if I got debug event
                            // Asserts
                            Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                            Assert.IsNotNull(cpu);
                            Assert.AreEqual(j, res.Item1.PlayedTarget);
                            Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                            // Reversion
                            sm.UndoPreviousStep();
                            Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        }
                        else // Shouldn't have been able to be played here
                        {
                            Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                            Assert.IsNull(res.Item2);
                            Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.Item1.PlayOutcome);
                        }
                    }
                }
            }
        }

        // TILE TARGETING
        [TestMethod]
        public void VerifyValidTileTargets()
        {
            // Creates a skill with TILE target, add some valid and some invalid tiles, check play
            // Ensure playable only in the valid tiles
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid and invalid tiles
                HashSet<int> validTiles = new HashSet<int>();
                HashSet<int> invalidTiles = new HashSet<int>();
                int tileCountInEachSet = _rng.Next(2, 5);
                while (validTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        validTiles.Add(nextTile);
                    }
                }
                while (invalidTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        invalidTiles.Add(nextTile);
                    }
                }
                HashSet<int> allTiles = [..validTiles, ..invalidTiles];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.TILE);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(tileCountInEachSet, res.ValidTargets.Count);
                Assert.AreEqual(PlayOutcome.OK, res.PlayOutcome);
                foreach (int tile in allTiles)
                {
                    if (validTiles.Contains(tile))
                    {
                        Assert.IsTrue(res.ValidTargets.Contains(tile));
                    }
                    else
                    {
                        Assert.IsFalse(res.ValidTargets.Contains(tile));
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyPlayTileTargets()
        {
            // Creates a skill with TILE target, add some valid and some invalid tiles, check play
            // Ensure plays only in the valid tiles
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid and invalid tiles
                HashSet<int> validTiles = new HashSet<int>();
                HashSet<int> invalidTiles = new HashSet<int>();
                int tileCountInEachSet = _rng.Next(2, 5);
                while (validTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        validTiles.Add(nextTile);
                    }
                }
                while (invalidTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        invalidTiles.Add(nextTile);
                    }
                }
                HashSet<int> allTiles = [.. validTiles, .. invalidTiles];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.TILE);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                foreach (int tile in allTiles) // Check all tiles
                {
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, tile); // Play in this target
                    if (validTiles.Contains(tile)) // This was a valid target, ensure played correctly
                    {
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2); // See if I got debug event
                        // Asserts
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNotNull(cpu);
                        Assert.AreEqual(tile, res.Item1.PlayedTarget);
                        Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                        // Reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                    else // Shouldn't have been able to be played here
                    {
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNull(res.Item2);
                        Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.Item1.PlayOutcome);
                    }
                }
            }
        }

        // TILE TARGETING, RELATIVE
        [TestMethod]
        public void VerifyValidTileRelativeTargets()
        {
            // Creates a skill with TILE target, add some valid and some invalid tiles, check play
            // Ensure playable only in the valid tiles
            // Different players will have different tiles tho
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid and invalid tiles
                HashSet<int> validTiles = new HashSet<int>();
                HashSet<int> invalidTiles = new HashSet<int>();
                int tileCountInEachSet = _rng.Next(2, 5);
                while (validTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        validTiles.Add(nextTile);
                    }
                }
                while (invalidTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        invalidTiles.Add(nextTile);
                    }
                }
                HashSet<int> allTiles = [.. validTiles, .. invalidTiles];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.TILE_RELATIVE);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(tileCountInEachSet, res.ValidTargets.Count);
                Assert.AreEqual(PlayOutcome.OK, res.PlayOutcome);
                foreach (int tile in allTiles)
                {
                    // This is the absolute!
                    Lane refLane = sm.DetailedState.BoardState.GetLaneContainingTile(tile);
                    int coordRelativeToLane = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_LANE, LaneRelativeIndexType.ABSOLUTE, tile); // Get relative to lane
                    int absoluteCoord = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, coordRelativeToLane, playerIndex); // Obtain the equivalent for the player who's checking
                    if (validTiles.Contains(tile))
                    {
                        Assert.IsTrue(res.ValidTargets.Contains(absoluteCoord));
                    }
                    else
                    {
                        Assert.IsFalse(res.ValidTargets.Contains(absoluteCoord));
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyPlayTileRelativeTargets()
        {
            // Creates a skill with TILE target, add some valid and some invalid tiles, check play
            // Ensure plays only in the valid tiles
            // Different players will have different tiles tho
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid and invalid tiles
                HashSet<int> validTiles = new HashSet<int>();
                HashSet<int> invalidTiles = new HashSet<int>();
                int tileCountInEachSet = _rng.Next(2, 5);
                while (validTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        validTiles.Add(nextTile);
                    }
                }
                while (invalidTiles.Count < tileCountInEachSet)
                {
                    int nextTile = _rng.Next(GameConstants.BOARD_NUMBER_OF_TILES);
                    if (!validTiles.Contains(nextTile) && !invalidTiles.Contains(nextTile))
                    {
                        invalidTiles.Add(nextTile);
                    }
                }
                HashSet<int> allTiles = [.. validTiles, .. invalidTiles];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.TILE_RELATIVE);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                foreach (int tile in allTiles) // Check all tiles
                {
                    // This is what I actually play
                    Lane refLane = sm.DetailedState.BoardState.GetLaneContainingTile(tile);
                    int coordRelativeToLane = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_LANE, LaneRelativeIndexType.ABSOLUTE, tile); // Get relative to lane
                    int absoluteCoord = refLane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, coordRelativeToLane, playerIndex); // Obtain the equivalent for the player who's checking

                    int prePlayHash = sm.DetailedState.GetHashCode();
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, absoluteCoord); // Play in this target
                    if (validTiles.Contains(tile)) // This was a valid target (relative-wise), ensure played correctly
                    {
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2); // See if I got debug event
                        // Asserts
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNotNull(cpu);
                        Assert.AreEqual(absoluteCoord, res.Item1.PlayedTarget);
                        Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                        // Reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                    else // Shouldn't have been able to be played here
                    {
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNull(res.Item2);
                        Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.Item1.PlayOutcome);
                    }
                }
            }
        }

        // UNIT TARGETING
        [TestMethod]
        public void VerifyValidUnitTargets()
        {
            // Creates a skill with UNIT target, will be playable in even tiles
            // Then, put a bunch of units in even tiles, and a bunch in odds, check values are ok
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid tiles as the even ones
                HashSet<int> validTiles = new HashSet<int>();
                for(int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    if(i % 2 == 0) validTiles.Add(i); // Add even tiles
                }
                // Let's add random units
                Unit theUnit = TestCardGenerator.CreateUnit(2, "TEST", 0, [], 1, 0, 1, 1);
                int evenUnits = _rng.Next(2, 5);
                int oddUnits = _rng.Next(2, 5);
                HashSet<int> validUnits = new HashSet<int>();
                HashSet<int> invalidUnits = new HashSet<int>();
                int nextUniqueId = 2; // Next entity ID
                for (int i = 0; i < evenUnits; i++)
                {
                    int nextCoord = _rng.Next(9) * 2;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    validUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                for (int i = 0; i < oddUnits; i++)
                {
                    int nextCoord = (_rng.Next(9) * 2) + 1;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    invalidUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                HashSet<int> allUnits = [.. validUnits, .. invalidUnits];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.UNIT);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(evenUnits, res.ValidTargets.Count);
                Assert.AreEqual(PlayOutcome.OK, res.PlayOutcome);
                foreach (int unit in allUnits)
                {
                    if (validUnits.Contains(unit))
                    {
                        Assert.IsTrue(res.ValidTargets.Contains(unit));
                    }
                    else
                    {
                        Assert.IsFalse(res.ValidTargets.Contains(unit));
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyPlayUnitTargets()
        {
            // Creates a skill with UNIT target, will be playable in even tiles
            // Then, put a bunch of units in even tiles, and a bunch in odds, check it only plays in good ones
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid tiles as the even ones
                HashSet<int> validTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    if (i % 2 == 0) validTiles.Add(i); // Add even tiles
                }
                // Let's add random units
                Unit theUnit = TestCardGenerator.CreateUnit(2, "TEST", 0, [], 1, 0, 1, 1);
                int evenUnits = _rng.Next(2, 5);
                int oddUnits = _rng.Next(2, 5);
                HashSet<int> validUnits = new HashSet<int>();
                HashSet<int> invalidUnits = new HashSet<int>();
                int nextUniqueId = 2; // Next entity ID
                for (int i = 0; i < evenUnits; i++)
                {
                    int nextCoord = _rng.Next(9) * 2;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    validUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                for (int i = 0; i < oddUnits; i++)
                {
                    int nextCoord = (_rng.Next(9) * 2) + 1;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    invalidUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                HashSet<int> allUnits = [.. validUnits, .. invalidUnits];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.UNIT);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                foreach (int unit in allUnits) // Check all units
                {
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, unit); // Play in this target
                    if (validUnits.Contains(unit)) // This was a valid target, ensure played correctly
                    {
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2); // See if I got debug event
                        // Asserts
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNotNull(cpu);
                        Assert.AreEqual(unit, res.Item1.PlayedTarget);
                        Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                        // Reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                    else // Shouldn't have been able to be played here
                    {
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNull(res.Item2);
                        Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.Item1.PlayOutcome);
                    }
                }
            }
        }

        // UNIT TARGETING, RELATIVE
        [TestMethod]
        public void VerifyValidUnitRelativeTargets()
        {
            // Creates a skill with UNIT target, will be playable in even tiles
            // Then, put a bunch of units in even tiles, and a bunch in odds, check values are ok
            // In this case, as UNIT tile map is relative, for P2, odd units are actually the playable ones
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid tiles as the even ones
                HashSet<int> validTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    if (i % 2 == 0) validTiles.Add(i); // Add even tiles
                }
                // Let's add random units
                Unit theUnit = TestCardGenerator.CreateUnit(2, "TEST", 0, [], 1, 0, 1, 1);
                int evenUnits = _rng.Next(2, 5);
                int oddUnits = _rng.Next(2, 5);
                HashSet<int> validUnits = new HashSet<int>();
                HashSet<int> invalidUnits = new HashSet<int>();
                int nextUniqueId = 2; // Next entity ID
                for (int i = 0; i < evenUnits; i++)
                {
                    int nextCoord = _rng.Next(9) * 2;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    validUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                for (int i = 0; i < oddUnits; i++)
                {
                    int nextCoord = (_rng.Next(9) * 2) + 1;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    invalidUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                HashSet<int> allUnits = [.. validUnits, .. invalidUnits];
                if(player == CurrentPlayer.PLAYER_2) // Player 2 switches even units and odd units!
                {
                    HashSet<int> auxHashSet = validUnits;
                    validUnits = invalidUnits;
                    invalidUnits = auxHashSet;
                }
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.UNIT_RELATIVE);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(validUnits.Count, res.ValidTargets.Count);
                Assert.AreEqual(PlayOutcome.OK, res.PlayOutcome);
                foreach (int unit in allUnits)
                {
                    if (validUnits.Contains(unit))
                    {
                        Assert.IsTrue(res.ValidTargets.Contains(unit));
                    }
                    else
                    {
                        Assert.IsFalse(res.ValidTargets.Contains(unit));
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyPlayUnitRelativeTargets()
        {
            // Creates a skill with UNIT target, will be playable in even tiles
            // Then, put a bunch of units in even tiles, and a bunch in odds, check it only plays in good ones
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid tiles as the even ones
                HashSet<int> validTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    if (i % 2 == 0) validTiles.Add(i); // Add even tiles
                }
                // Let's add random units
                Unit theUnit = TestCardGenerator.CreateUnit(2, "TEST", 0, [], 1, 0, 1, 1);
                int evenUnits = _rng.Next(2, 5);
                int oddUnits = _rng.Next(2, 5);
                HashSet<int> validUnits = new HashSet<int>();
                HashSet<int> invalidUnits = new HashSet<int>();
                int nextUniqueId = 2; // Next entity ID
                for (int i = 0; i < evenUnits; i++)
                {
                    int nextCoord = _rng.Next(9) * 2;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    validUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                for (int i = 0; i < oddUnits; i++)
                {
                    int nextCoord = (_rng.Next(9) * 2) + 1;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Unit)theUnit.Clone());
                    invalidUnits.Add(nextUniqueId);
                    nextUniqueId++;
                }
                HashSet<int> allUnits = [.. validUnits, .. invalidUnits];
                if (player == CurrentPlayer.PLAYER_2) // Player 2 switches even units and odd units!
                {
                    HashSet<int> auxHashSet = validUnits;
                    validUnits = invalidUnits;
                    invalidUnits = auxHashSet;
                }
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.UNIT_RELATIVE);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                foreach (int unit in allUnits) // Check all units
                {
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, unit); // Play in this target
                    if (validUnits.Contains(unit)) // This was a valid target, ensure played correctly
                    {
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2); // See if I got debug event
                        // Asserts
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNotNull(cpu);
                        Assert.AreEqual(unit, res.Item1.PlayedTarget);
                        Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                        // Reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                    else // Shouldn't have been able to be played here
                    {
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNull(res.Item2);
                        Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.Item1.PlayOutcome);
                    }
                }
            }
        }

        // BULDING TARGETING
        [TestMethod]
        public void VerifyValidBuildingTargets()
        {
            // Creates a skill with BUILDING target, will be playable in even tiles
            // Then, put a bunch of bldgs in even tiles, and a bunch in odds, check values are ok
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid tiles as the even ones
                HashSet<int> validTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    if (i % 2 == 0) validTiles.Add(i); // Add even tiles
                }
                // Let's add random buildings
                Building theBuilding = TestCardGenerator.CreateBuilding(2, "TEST", 0, [], 1);
                int evenBuildings = _rng.Next(2, 5);
                int oddBuildings = _rng.Next(2, 5);
                HashSet<int> validBuildings = new HashSet<int>();
                HashSet<int> invalidBuildings = new HashSet<int>();
                int nextUniqueId = 2; // Next entity ID
                for (int i = 0; i < evenBuildings; i++)
                {
                    int nextCoord = _rng.Next(9) * 2;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Building)theBuilding.Clone());
                    validBuildings.Add(nextUniqueId);
                    nextUniqueId++;
                }
                for (int i = 0; i < oddBuildings; i++)
                {
                    int nextCoord = (_rng.Next(9) * 2) + 1;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Building)theBuilding.Clone());
                    invalidBuildings.Add(nextUniqueId);
                    nextUniqueId++;
                }
                HashSet<int> allBuildings = [.. validBuildings, .. invalidBuildings];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.BUILDING);
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                PlayContext res = sm.GetPlayabilityOptions(1, PlayType.PLAY_FROM_HAND);
                Assert.AreEqual(evenBuildings, res.ValidTargets.Count);
                Assert.AreEqual(PlayOutcome.OK, res.PlayOutcome);
                foreach (int building in allBuildings)
                {
                    if (validBuildings.Contains(building))
                    {
                        Assert.IsTrue(res.ValidTargets.Contains(building));
                    }
                    else
                    {
                        Assert.IsFalse(res.ValidTargets.Contains(building));
                    }
                }
            }
        }
        [TestMethod]
        public void VerifyPlayBuildingTargets()
        {
            // Creates a skill with BUILDING target, will be playable in even tiles
            // Then, put a bunch of bldgs in even tiles, and a bunch in odds, check it only plays in good ones
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Let's define valid tiles as the even ones
                HashSet<int> validTiles = new HashSet<int>();
                for (int i = 0; i < GameConstants.BOARD_NUMBER_OF_TILES; i++)
                {
                    if (i % 2 == 0) validTiles.Add(i); // Add even tiles
                }
                // Let's add random units
                Building theBuilding = TestCardGenerator.CreateBuilding(2, "TEST", 0, [], 1);
                int evenBuildings = _rng.Next(2, 5);
                int oddBuildings = _rng.Next(2, 5);
                HashSet<int> validBuildings = new HashSet<int>();
                HashSet<int> invalidBuildings = new HashSet<int>();
                int nextUniqueId = 2; // Next entity ID
                for (int i = 0; i < evenBuildings; i++)
                {
                    int nextCoord = _rng.Next(9) * 2;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Building)theBuilding.Clone());
                    validBuildings.Add(nextUniqueId);
                    nextUniqueId++;
                }
                for (int i = 0; i < oddBuildings; i++)
                {
                    int nextCoord = (_rng.Next(9) * 2) + 1;
                    TestHelperFunctions.ManualInitEntity(state, nextCoord, nextUniqueId, playerIndex, (Building)theBuilding.Clone());
                    invalidBuildings.Add(nextUniqueId);
                    nextUniqueId++;
                }
                HashSet<int> allBuildings = [.. validBuildings, .. invalidBuildings];
                // Rest of init
                CardFinder cardDb = new CardFinder();
                Skill boardTargetableSkill = TestCardGenerator.CreateSkill(1, 0, validTiles, CardTargetingType.BUILDING);
                Effect debugEvent = new Effect() { EffectType = EffectType.STORE_DEBUG_IN_EVENT_PILE };
                boardTargetableSkill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                boardTargetableSkill.Interactions[InteractionType.WHEN_PLAYED] = [debugEvent];
                cardDb.InjectCard(1, boardTargetableSkill);
                state.PlayerStates[playerIndex].Hand.InsertCard(1);
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                foreach (int building in allBuildings) // Check all buildings
                {
                    int prePlayHash = sm.DetailedState.GetHashCode();
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, building); // Play in this target
                    if (validBuildings.Contains(building)) // This was a valid target, ensure played correctly
                    {
                        CpuState cpu = TestHelperFunctions.FetchDebugEvent(res.Item2); // See if I got debug event
                        // Asserts
                        Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNotNull(cpu);
                        Assert.AreEqual(building, res.Item1.PlayedTarget);
                        Assert.AreEqual(PlayOutcome.OK, res.Item1.PlayOutcome);
                        // Reversion
                        sm.UndoPreviousStep();
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                    }
                    else // Shouldn't have been able to be played here
                    {
                        Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                        Assert.IsNull(res.Item2);
                        Assert.AreEqual(PlayOutcome.NO_TARGET_AVAILABLE, res.Item1.PlayOutcome);
                    }
                }
            }
        }
    }
}
