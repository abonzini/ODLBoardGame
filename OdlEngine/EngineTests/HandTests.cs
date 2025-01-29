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
            Assert.AreEqual(hand.HandSize, 0); // Verify empty hand
        }
        [TestMethod]
        public void HandInsertion()
        {
            Hand hand = new Hand();
            // Add 3 cards
            hand.InsertCard(1, hand.HandSize);
            hand.InsertCard(2, hand.HandSize);
            hand.InsertCard(3, hand.HandSize);
            Assert.AreEqual(hand.HandSize, 3); // Verify correct number
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
            hand.InsertCard(1, hand.HandSize);
            hand.InsertCard(2, hand.HandSize);
            hand.InsertCard(3, hand.HandSize);
            for (int i = 1; i <= 3; i++)
            {
                Assert.AreEqual(hand.RemoveCardAt(0), i); // Remove 1 by 1 and verify
            }
        }
    }
}