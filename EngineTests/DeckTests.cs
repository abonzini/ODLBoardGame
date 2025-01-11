using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EngineTests
{
    [TestClass]
    public class DeckTests
    {
        [TestMethod]
        public void DeckCreation() // Deck construction and empty initial deck
        {
            Deck newDeck = new Deck();
            Assert.AreEqual(newDeck.GetCardNumber(), 0);
        }
        [TestMethod]
        public void DeckInitialization() // Deck is initialized properly with 5 cards, can pop cards
        {
            Deck newDeck = new Deck();
            newDeck.InitializeDeck("1,2,3,4,5"); // Adds cards 1 2 3 4 5
            string deckHistogram = newDeck.GetDeckHistogramString();
            Dictionary<int, int>? histogram = JsonSerializer.Deserialize<Dictionary<int, int>>(deckHistogram);
            if (histogram == null) throw new Exception("Deserialization of deck broke");
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(histogram[i], 1); // Verify 1 of each
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
        public void DeckInsertion()
        {
            Deck newDeck = new Deck();
            // Will add 1 ones, 2 twos, etc, for a total of 15 cards
            for (int i = 1; i <= 5; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    newDeck.InsertCard(i);
                }
            }
            Assert.AreEqual(newDeck.GetCardNumber(), 15);
            // Make sure of individual cards
            string deckHistogram = newDeck.GetDeckHistogramString();
            Dictionary<int, int>? histogram = JsonSerializer.Deserialize<Dictionary<int, int>>(deckHistogram);
            if (histogram == null) throw new Exception("Deserialization of deck broke");
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(histogram[i], i); // Verify i of each
            }
        }
        [TestMethod]
        public void DeckRestoration()
        {
            Deck newDeck = new Deck();
            newDeck.InitializeDeck("1,2,3,4,5"); // Adds cards 1 2 3 4 5
            string deckHistogram = newDeck.GetDeckHistogramString();
            Dictionary<int, int>? histogram = JsonSerializer.Deserialize<Dictionary<int, int>>(deckHistogram);
            if (histogram == null) throw new Exception("Deserialization of deck broke");
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(histogram[i], 1); // Verify 1 of each
                newDeck.InsertCard(i); // But also sneakily add another one
            }
            // Check again
            deckHistogram = newDeck.GetDeckHistogramString();
            histogram = JsonSerializer.Deserialize<Dictionary<int, int>>(deckHistogram);
            if (histogram == null) throw new Exception("Deserialization of deck broke");
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(histogram[i], 2); // Verify 1 of each
            }
        }
        [TestMethod]
        public void DeckShuffling()
        {
            Deck newDeck = new Deck();
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                newDeck.InsertCard(i); // Add cards 1,2,3,4,5 and will be shuffled (ensures no exception on shuffle)
            }
            newDeck.ShuffleDeck(0);
            for (int i = 0; i < 5; i++)
            {
                int card = newDeck.PopCard();
                bool isBetween = card > 0 && card <= 5;
                Assert.IsTrue(isBetween); // Ensures card is between!
            }
            Assert.AreEqual(newDeck.GetCardNumber(), 0); // By now there should be 0 cards remaining   
        }
    }
}
