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
                    // Cards 0-9: test spells with various costs
                    cardDb.InjectCard(i, TestCardGenerator.CreateSkill(i, "BRICK", i, PlayTargetLocation.BOARD));
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
        public void VerifyTargetability() // Generate targets 0-9, verify targetability is same
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
                    // generate various bricks with different targetability
                    cardDb.InjectCard(i, TestCardGenerator.CreateSkill(i, "BRICK", 0, (PlayTargetLocation)i));
                    state.PlayerStates[playerIndex].Hand.InsertCard(i);
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount; i++) // Check for each card
                {
                    PlayContext res = sm.GetPlayabilityOptions(i, PlayType.PLAY_FROM_HAND); // Ok in all cases with valid target
                    if (i <= 7)
                    {
                        Assert.AreEqual(res.PlayOutcome, PlayOutcome.OK); // OK
                        Assert.AreEqual(res.PlayedTarget, (PlayTargetLocation)i); // All them valid targets
                    }
                    else
                    {
                        Assert.AreEqual(res.PlayOutcome, PlayOutcome.NO_TARGET_AVAILABLE); // Would be an error!
                        Assert.AreEqual(res.PlayedTarget, PlayTargetLocation.INVALID); // Bc invalid...
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
                        Assert.AreEqual(res.PlayedTarget, PlayTargetLocation.INVALID);
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
                cardDb.InjectCard(1, TestCardGenerator.CreateSkill(1, "BRICK", 0, PlayTargetLocation.BOARD));

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
                        Assert.AreEqual(res.PlayedTarget, PlayTargetLocation.INVALID); // Also this invalid...
                    }
                }
            }
        }
        // --------------------------------------------------------------------------------------
        // ---------------------------  ACTUALLY PLAYING THE CARD -------------------------------
        // --------------------------------------------------------------------------------------

        [TestMethod]
        public void PlayInIncorrectState() // Tries to play a card in incorrect phase
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
                    if (st != States.ACTION_PHASE) // Only check invalid states as valid state is used elsewhere during tests
                    {
                        Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, PlayTargetLocation.BOARD); // Should break every single time and nothing should happen (TODO hash verify?)
                        Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.INVALID_GAME_STATE);
                        Assert.IsNull(res.Item2);
                    }
                }
            }
        }
        [TestMethod]
        public void PlayInvalidIndex() // Generate targets 0-4, verify i can only play valid cards
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                cardDb.InjectCard(1, TestCardGenerator.CreateSkill(1, "BRICK", 0, PlayTargetLocation.BOARD));
                for (int i = 0; i < 5; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertCard(1); // Insert test card (brick) in hand all targets 5 times
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                List<int> possibleCards = [1, 1, 1, 1, 1, 2, 2, 2, 2, 2]; // First 5 are ok, last 5 are not
                foreach (int card in possibleCards) // Check for each card
                {
                    if (card == 2) // Just test wrong case as other cases are tested elsewhere in detail
                    {
                        Tuple<PlayContext, StepResult> res = sm.PlayFromHand(card, PlayTargetLocation.BOARD);
                        Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.INVALID_CARD); // Would be an error!
                        Assert.IsNull(res.Item2); // Also this invalid...
                    }
                }
            }
        }
        [TestMethod]
        public void PlayAllTargets() // Play absolutely all targets for all cards!
        {
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                CardFinder cardDb = new CardFinder();
                for (int i = 0; i <= 7; i++)
                {
                    // Insert test cards (brick) in hand all targets, but only valid ones because invalids test already performed
                    cardDb.InjectCard(i, TestCardGenerator.CreateSkill(i, "BRICK", 0, (PlayTargetLocation)i));
                }
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount; i++) // Check for each card
                {
                    for (int j = 0; j < 10; j++) // Try and target absolutely all ways (many should break!)
                    {
                        Tuple<PlayContext, StepResult> res = sm.PlayFromHand(i, (PlayTargetLocation)j); // Try to target card
                        if ((j > 7) || (j & (j - 1)) != 0) // Implying invalid targets no matter what! (Either non power of 2 or high target enum)
                        {
                            Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.INVALID_TARGET); // Invalid target and no state change
                            Assert.AreEqual(res.Item2, null);
                        }
                        else if (i == 0) // If global targeting...
                        {
                            if (j == 0) // Global is only valid option, then should succeed
                            {
                                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                                Assert.IsNotNull(res.Item2);
                                sm.UndoPreviousStep(); // Undo to try next one
                            }
                            else
                            {
                                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.INVALID_TARGET); // Invalid target and no state change
                                Assert.IsNull(res.Item2);
                            }
                        }
                        else // Lane specific bricks, only playable if lane matches
                        {
                            if ((i & j) != 0) // Targets match!
                            {
                                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
                                Assert.IsNotNull(res.Item2);
                                sm.UndoPreviousStep(); // Undo to try next one
                            }
                            else
                            {
                                Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.INVALID_TARGET); // Invalid target and no state change
                                Assert.IsNull(res.Item2);
                            }
                        }
                    }
                }
            }
        }
        [TestMethod]
        public void PlayAffordability() // Generate costs 0-9, half can be afforable, half can't
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
                    // Insert test cards (brick) in hand costs 0-9
                    cardDb.InjectCard(i, TestCardGenerator.CreateSkill(i, "BRICK", i, PlayTargetLocation.BOARD));
                }
                state.PlayerStates[playerIndex].CurrentGold = 4; // Set gold to 4
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount; i++) // Check for each card
                {
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(i, PlayTargetLocation.BOARD);
                    if (i <= 4)
                    {
                        Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK); // Could be played
                        Assert.IsNotNull(res.Item2);
                        sm.UndoPreviousStep();
                    }
                    else
                    {
                        Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.CANT_AFFORD); // Could not
                        Assert.IsNull(res.Item2);
                    }
                }
            }
        }
        [TestMethod]
        public void PlayCosts() // Verifies card cost is paid and card is discarded
        {
            Random rng = new Random(); // Random costs just for fun
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                List<int> possibleCards = new List<int>();
                CardFinder cardDb = new CardFinder();
                for (int i = 0; i < 10; i++)
                {
                    int randomCard = rng.Next(10);
                    cardDb.InjectCard(randomCard, TestCardGenerator.CreateSkill(randomCard, "BRICK", randomCard, PlayTargetLocation.BOARD));
                    state.PlayerStates[playerIndex].Hand.InsertCard(randomCard); // Insert test cards (brick) in hand with random cost 0-9
                    possibleCards.Add(randomCard); // Add it also to one of my choices
                }
                state.PlayerStates[playerIndex].CurrentGold = rng.Next(1000, 10000); // Set gold to random but high value
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                for (int i = 0; i < 5; i++) // Will play 5 random cards
                {
                    // Know everything I'll do beforehand
                    int handSize = sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount;
                    int cardIndexToPlay = rng.Next(handSize);
                    int cardIdToPlay = possibleCards[cardIndexToPlay]; // Get random card of the ones I generated
                    EntityBase cardToPlay = sm.CardDb.GetCard(cardIdToPlay);
                    int currentGold = sm.DetailedState.PlayerStates[playerIndex].CurrentGold;
                    Tuple<PlayContext, StepResult> res = sm.PlayFromHand(cardIdToPlay, PlayTargetLocation.BOARD);
                    possibleCards.RemoveAt(cardIndexToPlay); // Remove this one
                    Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK); // Could be played
                    Assert.IsNotNull(res.Item2); // Sth happened
                    Assert.IsTrue(sm.DetailedState.PlayerStates[playerIndex].DiscardPile.HasCard(cardIdToPlay)); // Card was discarded
                    Assert.AreEqual(sm.DetailedState.PlayerStates[playerIndex].DiscardPile.CardCount, i + 1); // Discard pile has correct number of cards
                    Assert.AreEqual(sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount, handSize - 1); // One less card in hand
                    Assert.AreEqual(sm.DetailedState.PlayerStates[playerIndex].CurrentGold, currentGold - cardToPlay.Cost); // Spent the money
                    Assert.AreEqual(sm.DetailedState.CurrentPlayer, player); // Player still in command
                    Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE); // Still in action phase
                }
            }
        }
    }
}
