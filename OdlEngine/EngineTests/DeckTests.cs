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
            Assert.AreEqual(newDeck.DeckSize, 0);
        }
        [TestMethod]
        public void DeckInitialization() // Deck is initialized properly with 5 cards, can pop cards
        {
            Deck newDeck = new Deck();
            newDeck.InitializeDeck("1,2,3,4,5"); // Adds cards 1 2 3 4 
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(newDeck.CheckAmount(i), 1); // Verify 1 of each
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
                Assert.AreEqual(newDeck.CheckAmount(i), 1); // Verify 1 of each
                newDeck.InsertCard(i, 0); // But also sneakily add another one
            }
            // Check again
            for (int i = 1; i <= 5; i++) // Check existance of each card
            {
                Assert.AreEqual(newDeck.CheckAmount(i), 2); // Verify 2 of each
            }
        }
        [TestMethod]
        public void HashTest()
        {
            Deck deck1 = new Deck();
            Deck deck2 = new Deck();
            Random _rng = new Random();
            for (int i = 0; i < 30; i++) // Add 30 random hands between 0-99
            {
                int rn = _rng.Next(100);
                deck1.InsertCard(rn);
                deck2.InsertCard(rn);
            }
            Assert.AreEqual(deck1.GetHash(), deck2.GetHash());
            // Now swap 2 random cards
            int swap1 = _rng.Next(30);
            // Ensure it's another random card
            int swap2 = swap1 + _rng.Next(1,30);
            swap2 %= 30;
            deck2.SwapCards(swap1, swap2);
            Assert.AreNotEqual(deck1.GetHash(), deck2.GetHash());
            // Revert this, should be back to equal
            deck2.SwapCards(swap1, swap2);
            Assert.AreEqual(deck1.GetHash(), deck2.GetHash());
        }
        //[TestMethod]
        //public void HashStressTest()
        //{
        //    HashSet<int> hashes = new HashSet<int>();
        //    float total = 0;
        //    float collisions = 0;
        //    Random _rng = new Random();
        //    for (int i = 0; i < 1000000; i++) // Test 1000000 times, create unique cols and verify few collisions
        //    {
        //        Deck deck = new Deck();
        //        int repeat = _rng.Next(5, 31);
        //        for (int j = 0; j < repeat; j++) // Add cards 0-99 between 5-30 times (test that unique deck are unique, not ones with few cards)
        //        {
        //            deck.InsertCard(_rng.Next(100));
        //        }
        //        total++;
        //        if (hashes.Contains(deck.GetHash()))
        //        {
        //            collisions++;
        //        }
        //        else
        //        {
        //            hashes.Add(deck.GetHash());
        //        }
        //    }
        //    Assert.IsTrue(collisions / total < 0.01); // Try for 1% or less of collisions
        //}
    }
}
