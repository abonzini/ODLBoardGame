using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class HypotheticalTests
    {
        [TestMethod]
        public void HandReplacedByWildcards()
        {
            // Starts hypothetical mode for each player, the opposing player's hand will remain the same size but replaced with wildcards
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Fill hands and decks with BS, don't care about the actual card as won't be played, for easy validation and checking, cards from 1-N
                int numberOfCardsInHandAndDeck = _rng.Next(5, 20);
                for (int i = 1; i <= numberOfCardsInHandAndDeck; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertToCollection(i);
                    state.PlayerStates[playerIndex].Deck.InsertToCollection(i);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertToCollection(i);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertToCollection(i);
                }
                // Start now...
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                int[] bothPlayers = [playerIndex, otherPlayerIndex];
                foreach (int whichPlayer in bothPlayers)
                {
                    // Verify initial hash and hands
                    int startHash = sm.DetailedState.GetHashCode();
                    state = sm.DetailedState;
                    for (int i = 1; i <= numberOfCardsInHandAndDeck; i++) // Verify all's there
                    {
                        Assert.AreNotEqual(0, state.PlayerStates[playerIndex].Hand.CheckAmountInCollection(i));
                        Assert.AreNotEqual(0, state.PlayerStates[playerIndex].Deck.CheckAmountInCollection(i));
                        Assert.AreNotEqual(0, state.PlayerStates[otherPlayerIndex].Hand.CheckAmountInCollection(i));
                        Assert.AreNotEqual(0, state.PlayerStates[otherPlayerIndex].Deck.CheckAmountInCollection(i));
                    }
                    // Start Hypothetical mode
                    sm.StartHypotheticalMode(whichPlayer); // Starts hypothetical with a specific POV
                    Assert.AreNotEqual(startHash, sm.DetailedState.GetHashCode()); // Hash changed because one hand changed
                    for (int i = 1; i <= numberOfCardsInHandAndDeck; i++) // Verify all's there
                    {
                        // Ref player keeps hand and deck
                        Assert.AreNotEqual(0, state.PlayerStates[whichPlayer].Hand.CheckAmountInCollection(i));
                        Assert.AreNotEqual(0, state.PlayerStates[whichPlayer].Deck.CheckAmountInCollection(i));
                        // Other player loses hand, don't care about deck
                        Assert.AreEqual(0, state.PlayerStates[1 - whichPlayer].Hand.CheckAmountInCollection(i));
                    }
                    Assert.AreEqual(numberOfCardsInHandAndDeck, state.PlayerStates[1 - whichPlayer].Hand.CheckAmountInCollection(0)); // Player should have a ton of wildcards now...
                    // End hypothetical mode
                    sm.EndHypotheticalMode();
                    Assert.AreEqual(startHash, sm.DetailedState.GetHashCode()); // Re-assert
                    for (int i = 1; i <= numberOfCardsInHandAndDeck; i++) // Verify all's still there, hand restored
                    {
                        Assert.AreNotEqual(0, state.PlayerStates[playerIndex].Hand.CheckAmountInCollection(i));
                        Assert.AreNotEqual(0, state.PlayerStates[playerIndex].Deck.CheckAmountInCollection(i));
                        Assert.AreNotEqual(0, state.PlayerStates[otherPlayerIndex].Hand.CheckAmountInCollection(i));
                        Assert.AreNotEqual(0, state.PlayerStates[otherPlayerIndex].Deck.CheckAmountInCollection(i));
                    }
                }
            }
        }
        [TestMethod]
        public void DrawWildcards()
        {
            // Skill that draws for owner and opponent, they'll draw N cards and they'll now have N more wildcards and deck will be N times smaller
            // Real decks unchanged
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Fill hands and decks with BS, don't care about the actual card as won't be played, for easy validation and checking, cards from 1-N
                int numberOfCardsInDeck = _rng.Next(10, 20);
                for (int i = 1; i <= numberOfCardsInDeck; i++)
                {
                    state.PlayerStates[playerIndex].Deck.InsertToCollection(i);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertToCollection(i);
                }
                // Card 1: Skill that draws cards to both players
                int cardsToDraw = _rng.Next(1, 10);
                CardFinder cardDb = new CardFinder();
                Skill skill = TestCardGenerator.CreateSkill(1, 0, [0], CardTargetingType.BOARD);
                Effect drawEffect = new Effect()
                {
                    EffectType = EffectType.CARD_DRAW,
                    Input = Variable.TEMP_VARIABLE,
                    TargetPlayer = EntityOwner.BOTH,
                    TempVariable = cardsToDraw
                };
                skill.Interactions = new Dictionary<InteractionType, List<Effect>>();
                skill.Interactions.Add(InteractionType.WHEN_PLAYED, [drawEffect]); // Add interaction to card
                cardDb.InjectCard(1, skill); // Add to cardDb
                state.PlayerStates[playerIndex].Hand.InsertToCollection(1); // Add card
                // Start now...
                GameStateMachine sm = new GameStateMachine(cardDb);
                sm.LoadGame(state); // Start from here
                // Ok now will go to hypothetical mode (init of hypothetical in other tests)
                sm.StartHypotheticalMode(playerIndex);
                int prePlayHash = sm.DetailedState.GetHashCode();
                int prePlayDeckHash0 = sm.DetailedState.PlayerStates[playerIndex].Deck.GetHashCode();
                int prePlayDeckHash1 = sm.DetailedState.PlayerStates[otherPlayerIndex].Deck.GetHashCode();
                Assert.AreEqual(1, sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount);
                Assert.AreEqual(0, sm.DetailedState.PlayerStates[otherPlayerIndex].Hand.CardCount);
                Assert.AreEqual(numberOfCardsInDeck, sm.DetailedState.PlayerStates[playerIndex].Deck.CardCount);
                Assert.AreEqual(numberOfCardsInDeck, sm.DetailedState.PlayerStates[otherPlayerIndex].Deck.CardCount);
                // Both players will draw now, ensure many things
                sm.PlayFromHand(1, 0);
                Assert.AreNotEqual(prePlayHash, sm.DetailedState.GetHashCode());
                Assert.AreNotEqual(prePlayDeckHash0, sm.DetailedState.PlayerStates[playerIndex].Deck.GetHashCode());
                Assert.AreNotEqual(prePlayDeckHash1, sm.DetailedState.PlayerStates[otherPlayerIndex].Deck.GetHashCode());
                Assert.AreEqual(cardsToDraw, sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount);
                Assert.AreEqual(cardsToDraw, sm.DetailedState.PlayerStates[otherPlayerIndex].Hand.CardCount);
                Assert.AreEqual(cardsToDraw, sm.DetailedState.PlayerStates[playerIndex].Hand.CheckAmountInCollection(0)); // All drawn cards are wildcards
                Assert.AreEqual(cardsToDraw, sm.DetailedState.PlayerStates[otherPlayerIndex].Hand.CheckAmountInCollection(0));
                Assert.AreEqual(numberOfCardsInDeck - cardsToDraw, sm.DetailedState.PlayerStates[playerIndex].Deck.CardCount);
                Assert.AreEqual(numberOfCardsInDeck - cardsToDraw, sm.DetailedState.PlayerStates[otherPlayerIndex].Deck.CardCount);
                // End hypothetical mode
                sm.EndHypotheticalMode();
                Assert.AreEqual(prePlayHash, sm.DetailedState.GetHashCode());
                Assert.AreEqual(prePlayDeckHash0, sm.DetailedState.PlayerStates[playerIndex].Deck.GetHashCode());
                Assert.AreEqual(prePlayDeckHash1, sm.DetailedState.PlayerStates[otherPlayerIndex].Deck.GetHashCode());
                Assert.AreEqual(1, sm.DetailedState.PlayerStates[playerIndex].Hand.CardCount);
                Assert.AreEqual(0, sm.DetailedState.PlayerStates[otherPlayerIndex].Hand.CardCount);
                Assert.AreEqual(numberOfCardsInDeck, sm.DetailedState.PlayerStates[playerIndex].Deck.CardCount);
                Assert.AreEqual(numberOfCardsInDeck, sm.DetailedState.PlayerStates[otherPlayerIndex].Deck.CardCount);
            }
        }
        [TestMethod]
        public void EndHypotheticalMode()
        {
            // Start in non-hypothetical, do draw phase,
            // Then start hypothetical and do sequences of EOT->draw->EOT, then end the mode, hash should revert to original,
            // And then reversion again should show that the game state wasn't messed up
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.ACTION_PHASE;
                state.CurrentPlayer = player;
                // Fill hands and decks with BS, don't care about the actual card as won't be played, for easy validation and checking, cards from 1-N
                for (int i = 1; i <= 30; i++) // Add 30 just for funsies
                {
                    state.PlayerStates[playerIndex].Hand.InsertToCollection(i);
                    state.PlayerStates[playerIndex].Deck.InsertToCollection(i);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertToCollection(i);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertToCollection(i);
                }
                // Start now...
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                int[] bothPlayers = [playerIndex, otherPlayerIndex];
                foreach (int whichPlayer in bothPlayers)
                {
                    // Verify pre-hyp hash
                    int startHash = sm.DetailedState.GetHashCode();
                    int turnsToSkip = _rng.Next(3, 10); // Just skip a bunch of turns nothing crazy
                    // Start Hypothetical mode
                    sm.StartHypotheticalMode(whichPlayer); // Starts hypothetical with a specific POV
                    Assert.AreNotEqual(startHash, sm.DetailedState.GetHashCode()); // Hash changed because one hand changed
                    // Skip lot of turns
                    for (int i = 0; i < turnsToSkip; i++)
                    {
                        sm.EndTurn(); // EOT
                        sm.Step(); // Skip BOT
                    }
                    Assert.AreNotEqual(startHash, sm.DetailedState.GetHashCode());
                    // End hypothetical mode and ensure complete restoration of game state
                    sm.EndHypotheticalMode();
                    Assert.AreEqual(startHash, sm.DetailedState.GetHashCode());
                }
            }
        }
        [TestMethod]
        public void WildcardDiscovery()
        {
            // When a player has wildcards, can discover the wildcard but can also reverse this discovery
            Random _rng = new Random();
            CurrentPlayer[] players = [CurrentPlayer.PLAYER_1, CurrentPlayer.PLAYER_2]; // Will test both
            foreach (CurrentPlayer player in players)
            {
                int playerIndex = (int)player;
                int otherPlayerIndex = 1 - playerIndex;
                GameStateStruct state = TestHelperFunctions.GetBlankGameState();
                state.CurrentState = States.DRAW_PHASE;
                state.CurrentPlayer = player;
                // Fill hands and decks with BS, don't care about the actual card as won't be played, for easy validation and checking, cards from 1-N
                int numberOfCardsInHandAndDeck = _rng.Next(5, 20);
                for (int i = 1; i <= numberOfCardsInHandAndDeck; i++)
                {
                    state.PlayerStates[playerIndex].Hand.InsertToCollection(i);
                    state.PlayerStates[playerIndex].Deck.InsertToCollection(i);
                    state.PlayerStates[otherPlayerIndex].Hand.InsertToCollection(i);
                    state.PlayerStates[otherPlayerIndex].Deck.InsertToCollection(i);
                }
                // Start now...
                GameStateMachine sm = new GameStateMachine();
                sm.LoadGame(state); // Start from here
                int[] bothPlayers = [playerIndex, otherPlayerIndex];
                sm.StartHypotheticalMode(playerIndex); // Starts hypothetical for current player
                sm.Step(); // Implements draw phase (that way both players will have atleast one wildcard
                int hypothethicalStateHash = sm.DetailedState.GetHashCode();
                foreach (int whichPlayer in bothPlayers)
                {
                    int discoveredCard = _rng.Next(21, 100);
                    Assert.AreEqual(0, sm.DetailedState.PlayerStates[whichPlayer].Hand.CheckAmountInCollection(discoveredCard));
                    int numberOfCards = sm.DetailedState.PlayerStates[whichPlayer].Hand.CardCount;
                    int numberOfWildcards = sm.DetailedState.PlayerStates[whichPlayer].Hand.CheckAmountInCollection(0);
                    Assert.AreNotEqual(0, numberOfWildcards); // Player has atleast one wildcard
                    // Discovery and checks
                    sm.DiscoverHypotheticalWildcard(whichPlayer, discoveredCard); // Player now has a wildcard replaced by the desired
                    Assert.AreEqual(numberOfCards, sm.DetailedState.PlayerStates[whichPlayer].Hand.CardCount); // Ensure this remains the same
                    Assert.AreEqual(1, sm.DetailedState.PlayerStates[whichPlayer].Hand.CheckAmountInCollection(discoveredCard)); // Player got the card
                    Assert.AreEqual(numberOfWildcards - 1, sm.DetailedState.PlayerStates[whichPlayer].Hand.CheckAmountInCollection(0)); // But also player lost a wildcard
                    Assert.AreNotEqual(hypothethicalStateHash, sm.DetailedState.GetHashCode()); // Hash changed because hand contents also changed
                    // Reversion of discovery
                    sm.UndoPreviousStep();
                    Assert.AreEqual(numberOfCards, sm.DetailedState.PlayerStates[whichPlayer].Hand.CardCount);
                    Assert.AreEqual(0, sm.DetailedState.PlayerStates[whichPlayer].Hand.CheckAmountInCollection(discoveredCard));
                    Assert.AreEqual(numberOfWildcards, sm.DetailedState.PlayerStates[whichPlayer].Hand.CheckAmountInCollection(0));
                    Assert.AreEqual(hypothethicalStateHash, sm.DetailedState.GetHashCode());
                }
            }
        }
    }
}
