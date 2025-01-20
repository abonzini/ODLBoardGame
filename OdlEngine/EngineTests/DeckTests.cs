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
            Dictionary<int, int>? histogram = JsonSerializer.Deserialize<Dictionary<int, int>>(deckHistogram) ?? throw new Exception("Deserialization of deck broke");
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
        public void DeckAddAndPop()
        {
            Deck newDeck = new Deck();
            // Will add 1 ones, 2 twos, etc, for a total of 15 cards
            for (int i = 1; i <= 5; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    newDeck.InsertCard(0,i); // Added in reverse order for some reason
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
            string deckHistogram = newDeck.GetDeckHistogramString();
            Dictionary<int, int>? histogram = JsonSerializer.Deserialize<Dictionary<int, int>>(deckHistogram) ?? throw new Exception("Deserialization of deck broke");
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(histogram[i], 1); // Verify 1 of each
                newDeck.InsertCard(0,i); // But also sneakily add another one
            }
            // Check again
            deckHistogram = newDeck.GetDeckHistogramString();
            histogram = JsonSerializer.Deserialize<Dictionary<int, int>>(deckHistogram);
            if (histogram == null) throw new Exception("Deserialization of deck broke");
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(histogram[i], 2); // Verify 2 of each
            }
        }
        
    }
}
