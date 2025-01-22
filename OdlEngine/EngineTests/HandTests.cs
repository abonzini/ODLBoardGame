using ODLGameEngine;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace EngineTests
{
    [TestClass]
    public class HandTests
    {
        [TestMethod]
        public void EmptyHand()
        {
            Hand hand = new Hand();
            Assert.AreEqual(hand.GetHandSize(), 0); // Verify empty hand
        }
        [TestMethod]
        public void HandInsertion()
        {
            Hand hand = new Hand();
            // Add 3 cards
            hand.InsertCard(1, hand.GetHandSize());
            hand.InsertCard(2, hand.GetHandSize());
            hand.InsertCard(3, hand.GetHandSize());
            Assert.AreEqual(hand.GetHandSize(), 3); // Verify correct number
            List<int> cards = hand.CardsInHand;
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(cards[i], i+1); // Verify correct cards in correct order
            }
        }
        [TestMethod]
        public void HandRemoval()
        {
            Hand hand = new Hand();
            // Add 3 cards
            hand.InsertCard(1, hand.GetHandSize());
            hand.InsertCard(2, hand.GetHandSize());
            hand.InsertCard(3, hand.GetHandSize());
            for (int i = 1; i <= 3; i++)
            {
                Assert.AreEqual(hand.RemoveCardAt(0), i); // Remove 1 by 1 and verify
            }
        }
    }
}