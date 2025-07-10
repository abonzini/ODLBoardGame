using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class GameEngineTests // For debugging and control, verify that rulebook and backend works properly
    {
        // Also testing hashes at the same time to verify uniqueness and repeatability
        [TestMethod]
        public void GameStatesInit() // To make sure step by step, player 1, player 2 until first draw without issues, proper init
        {
            // Kinda deprecated as 1/2 init and draw phase don't exist anymore. Goes straight to P1 turn 1
            HashSet<int> playerHashes = new HashSet<int>(); // Stores all player hashes
            HashSet<int> stateHashes = new HashSet<int>(); // Stores all states
            CardFinder cardFinder = new CardFinder();
            TestHelperFunctions.InjectBasePlayerToDb(cardFinder);
            GameStateMachine sm = new GameStateMachine(cardFinder);
            Assert.AreEqual(sm.DetailedState.CurrentState, States.START); // Ensure start in start state
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer("p1");
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer("p2");
            sm.StartNewGame(dummyPlayer1, dummyPlayer2);
            // Initial hashes of players and whole game
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[0], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.DetailedState.PlayerStates[1], playerHashes, false);
            TestHelperFunctions.HashSetVerification(sm.DetailedState, stateHashes, false);
            Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE); // Now should be about to init P1
            Assert.AreEqual(1, sm.DetailedState.TurnCounter); // Start at turn 1
            Assert.AreEqual(CurrentPlayer.PLAYER_1, sm.DetailedState.CurrentPlayer); // Start at turn 1
        }
        [TestMethod]
        public void TestDeterminismInit()
        {
            // Inits game states, first with a seed and then with another one, checks same state results in a same seed
            // Not much more tests as no extra randomness for now
            Random _rng = new Random(); // Will do a random seed
            int randomSeed = _rng.Next();
            // Start of game, SM1
            CardFinder cardFinder = new CardFinder();
            TestHelperFunctions.InjectBasePlayerToDb(cardFinder);
            GameStateMachine sm1 = new GameStateMachine(cardFinder, randomSeed);
            PlayerInitialData dummyPlayer1 = InitialStatesGenerator.GetDummyPlayer("p1");
            PlayerInitialData dummyPlayer2 = InitialStatesGenerator.GetDummyPlayer("p2");
            sm1.StartNewGame(dummyPlayer1, dummyPlayer2);
            // SM2
            GameStateMachine sm2 = new GameStateMachine(cardFinder, randomSeed);
            sm2.StartNewGame(dummyPlayer1, dummyPlayer2);
            Assert.AreEqual(sm1.DetailedState.Seed, sm2.DetailedState.Seed); // Assert seed remained equal
            Assert.AreNotEqual(sm1.DetailedState.Seed, randomSeed); // Assert that seed advanced

        }
        [TestMethod]
        public void EndOfTurnTest()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                int playerIndex = (int)id;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = id;

                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                // Ensure all in order before EOT
                GameStateStruct preEotState = sm.DetailedState;
                int preEotHash = preEotState.GetHashCode(); // Keep hash
                Assert.AreEqual(preEotState.CurrentState, States.ACTION_PHASE); // in action phase
                Assert.AreEqual(preEotState.CurrentPlayer, id); // Current player
                // Now I activate end of turn!
                sm.EndTurn();
                GameStateStruct postEotState = sm.DetailedState;
                int postEotHash = preEotState.GetHashCode(); // Keep hash
                Assert.AreEqual(postEotState.CurrentState, States.ACTION_PHASE); // in draw phase now!
                Assert.AreEqual(postEotState.CurrentPlayer, (CurrentPlayer)otherPlayerIndex); // Current player has changed and will soon initialize drawing (draw phase already tested)
                Assert.AreNotEqual(preEotHash, postEotHash); // Hope hash are different (only because new current player)
                // Reversion should also apply
                sm.UndoPreviousStep();
                postEotState = sm.DetailedState;
                postEotHash = preEotState.GetHashCode();
                Assert.AreEqual(postEotState.CurrentState, States.ACTION_PHASE); // Back to action
                Assert.AreEqual(postEotState.CurrentPlayer, id); // Current player back to original
                Assert.AreEqual(preEotHash, postEotHash); // Hash repeatability check
            }
        }
        // Deck out tests
        [TestMethod]
        public void NoDeckoutDamage()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            Random _rng = new Random();
            foreach (CurrentPlayer id in ids)
            {
                int currentPlayer = (int)id;
                int playerHp = _rng.Next(GameConstants.DECKOUT_DAMAGE + 1, 31);
                Player pl1 = new Player();
                pl1.Hp.BaseValue = playerHp;
                Player pl2 = new Player();
                pl2.Hp.BaseValue = playerHp;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - id;
                state.PlayerStates = [pl1, pl2];
                state.PlayerStates[currentPlayer].Deck.InsertCard(-1); // Adds useless card to player deck
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 1);
                // Draw
                sm.EndTurn();
                Assert.AreNotEqual(deckHash, plState.Deck.GetHashCode()); // Changed because card was drawn
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 1);
            }
        }
        [TestMethod]
        public void DeckOutDamage()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            Random _rng = new Random();
            foreach (CurrentPlayer id in ids)
            {
                int playerHp = _rng.Next(GameConstants.DECKOUT_DAMAGE + 1, 31);
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - id;
                state.PlayerStates[0].Hp.BaseValue = playerHp;
                state.PlayerStates[1].Hp.BaseValue = playerHp;
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                // Draw
                sm.EndTurn();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode()); // No change in deck contents
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, playerHp);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
            }
        }
        [TestMethod]
        public void DeckOutKill()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - id;
                state.PlayerStates[0].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE;
                state.PlayerStates[1].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE;
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE);
                // Draw
                sm.EndTurn();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode()); // No change in deck contents
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.DamageTokens, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, (CurrentPlayer)(1 - (int)id)); // other player won
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE);
            }
        }
        [TestMethod]
        public void DeckOutOverkill()
        {
            CurrentPlayer[] ids = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2];
            foreach (CurrentPlayer id in ids)
            {
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = 1 - id;
                state.PlayerStates[0].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE - 1;
                state.PlayerStates[1].Hp.BaseValue = GameConstants.DECKOUT_DAMAGE - 1;
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                Player plState = sm.DetailedState.PlayerStates[(int)id];
                // Pre draw
                int deckHash = plState.Deck.GetHashCode();
                int playerHash = plState.GetHashCode();
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE);
                // Draw
                sm.EndTurn();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode()); // No change in deck contents
                Assert.AreNotEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.DamageTokens, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.EOG); // Game ends here
                Assert.AreEqual(sm.DetailedState.CurrentPlayer, (CurrentPlayer)(1 - (int)id)); // other player won
                // Revert
                sm.UndoPreviousStep();
                Assert.AreEqual(deckHash, plState.Deck.GetHashCode());
                Assert.AreEqual(playerHash, plState.GetHashCode());
                Assert.AreEqual(plState.Hp.Total, GameConstants.DECKOUT_DAMAGE - 1);
                Assert.AreEqual(plState.DamageTokens, 0);
                Assert.AreEqual(plState.Deck.DeckSize, 0);
                Assert.AreEqual(sm.DetailedState.CurrentState, States.ACTION_PHASE);
            }
        }
        // Hash tests
        [TestMethod]
        public void BoardHashVerify() // Verify that an unchanged board has an unchanged hash
        {
            int playerIndex = 0;
            GameStateStruct state = TestHelperFunctions.GetBlankGameState();
            state.CurrentState = States.ACTION_PHASE;
            state.CurrentPlayer = CurrentPlayer.PLAYER_1;
            CardFinder cardDb = new CardFinder();
            // Card 1: basic unit
            cardDb.InjectCard(1, TestCardGenerator.CreateUnit(1, "UNIT", 0, [0, 4, 10], 1, 1, 1, 1));
            state.PlayerStates[playerIndex].Hand.AddToCollection(1); // Insert token card
            state.PlayerStates[playerIndex].CurrentGold = 4; // Set gold to 4
            GameStateMachine sm = new GameStateMachine(cardDb);
            sm.LoadGame(state); // Start from here
            // HASH CHECK
            int emptyBoardHash = sm.DetailedState.BoardState.GetHashCode();
            int emptyBoardStateHash = sm.DetailedState.GetHashCode();
            Assert.AreEqual(emptyBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Hash would be recalculated but still the same
            Assert.AreEqual(emptyBoardStateHash, sm.DetailedState.GetHashCode()); // Hash would be recalculated but still the same
            // Will play card now
            Tuple<PlayContext, StepResult> res = sm.PlayFromHand(1, 0); // Play it
            // Make sure card was played ok
            Assert.AreEqual(res.Item1.PlayOutcome, PlayOutcome.OK);
            Assert.IsNotNull(res.Item2);
            // And check hash again
            int boardWUnitHash = sm.DetailedState.BoardState.GetHashCode();
            int stateWUnitHash = sm.DetailedState.GetHashCode();
            Assert.AreNotEqual(emptyBoardHash, boardWUnitHash);
            Assert.AreNotEqual(emptyBoardStateHash, stateWUnitHash);
            Assert.AreEqual(boardWUnitHash, sm.DetailedState.BoardState.GetHashCode()); // Hash would be recalculated but still the same
            Assert.AreEqual(stateWUnitHash, sm.DetailedState.GetHashCode()); // Hash would be recalculated but still the same
            // Modify unit (shady)
            int unitIndex = sm.DetailedState.BoardState.GetPlacedEntities(EntityType.UNIT).First();
            ((Unit)sm.DetailedState.EntityData[unitIndex]).Attack.BaseValue += 5; // Add 5 to attack, whatever
            Assert.AreEqual(boardWUnitHash, sm.DetailedState.BoardState.GetHashCode()); // Board is 100% positional so this hash should remain the same
            Assert.AreNotEqual(stateWUnitHash, sm.DetailedState.GetHashCode()); // But now the state changed because unit data is different
            sm.UndoPreviousStep();
            Assert.AreEqual(emptyBoardHash, sm.DetailedState.BoardState.GetHashCode()); // Finally hash should've reverted and known
            Assert.AreEqual(emptyBoardStateHash, sm.DetailedState.GetHashCode()); // Finally hash should've reverted and known
        }
    }
    public static class InitialStatesGenerator // Generates a game state for test
    {
        /// <summary>
        /// Creates a brand new dummy player with a 30-card test deck
        /// </summary>
        /// <returns></returns>
        public static PlayerInitialData GetDummyPlayer(string name)
        {
            PlayerInitialData ret = new PlayerInitialData()
            {
                Name = name
            };
            for (int i = 1; i <= GameConstants.DECK_SIZE; i++)
            {
                ret.InitialDecklist.AddToCollection(i);
            }
            return ret;
        }
    }
}
