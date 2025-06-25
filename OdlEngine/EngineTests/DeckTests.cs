using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class DeckTests
    {
        [TestMethod]
        public void DeckCreation() // Deck construction and empty initial deck
        {
            Deck newDeck = new Deck();
            Assert.AreEqual(newDeck.DeckSize, 0);
        }
        [TestMethod]
        public void DeckInitialization() // Deck is initialized properly with 5 cards, can pop cards
        {
            Deck newDeck = new Deck();
            newDeck.InitializeDeck("1,2,3,4,5"); // Adds cards 1 2 3 4 
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(newDeck.CheckAmountInCollection(i), 1); // Verify 1 of each
            }
        }
        [TestMethod]
        public void DeckPop() // Deck is initialized properly with 5 cards, can pop cards
        {
            Deck newDeck = new Deck();
            newDeck.InitializeDeck("1,2,3,4,5"); // Adds cards 1 2 3 4 5
            for (int i = 0; i < 5; i++)
            {
                int card = newDeck.PopCard();
                Assert.AreEqual(card, 5 - i); // Cards obtained but in reverse order
            }
        }
        [TestMethod]
        public void DeckAddAndPop()
        {
            Deck newDeck = new Deck();
            // Will add 1 ones, 2 twos, etc, for a total of 15 cards
            for (int i = 1; i <= 5; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    newDeck.InsertCard(i, 0); // Added in reverse order for some reason
                }
            }
            // Now I pop one by one, should get them in the order as I added (due to inserting at 0!)
            for (int i = 1; i <= 5; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    int card = newDeck.PopCard();
                    Assert.AreEqual(card, i);
                }
            }
        }
        [TestMethod]
        public void DeckInsertion()
        {
            Deck newDeck = new Deck();
            newDeck.InitializeDeck("1,2,3,4,5"); // Adds cards 1 2 3 4 5
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(newDeck.CheckAmountInCollection(i), 1); // Verify 1 of each
                newDeck.InsertCard(i, 0); // But also sneakily add another one
            }
            // Check again
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(newDeck.CheckAmountInCollection(i), 2); // Verify 2 of each
            }
        }
        [TestMethod]
        public void HashTest()
        {
            Deck deck1 = new Deck();
            Deck deck2 = new Deck();
            Random _rng = new Random();
            for (int i = 0; i < 30; i++) // Add 30 random cards between 0-99
            {
                int rn = _rng.Next(100);
                deck1.InsertCard(rn);
                deck2.InsertCard(rn);
            }
            Assert.AreEqual(deck1.GetHashCode(), deck2.GetHashCode());
            // Now swap 2 random cards
            int swap1 = _rng.Next(30);
            // Ensure it's another random card
            int swap2;
            do
            {
                swap2 = swap1 + _rng.Next(1, 30);
                swap2 %= 30;
            } while (deck2.PeepAt(swap1) == deck2.PeepAt(swap2)); // But ensure cards are actually distinct otherwise this won't work
            deck2.SwapCards(swap1, swap2);
            Assert.AreNotEqual(deck1.GetHashCode(), deck2.GetHashCode());
            // Revert this, should be back to equal
            deck2.SwapCards(swap1, swap2);
            Assert.AreEqual(deck1.GetHashCode(), deck2.GetHashCode());
        }
    }
}
