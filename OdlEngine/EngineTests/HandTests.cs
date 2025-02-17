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
            // Add 6 cards, 1, 2, 2, 3, 3, 3
            hand.InsertCard(1);
            hand.InsertCard(2);
            hand.InsertCard(2);
            hand.InsertCard(3);
            hand.InsertCard(3);
            hand.InsertCard(3);
            Assert.AreEqual(hand.HandSize, 6); // Verify correct number
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(hand.CardsInHand.ContainsKey(i)); // In dict
                Assert.AreEqual(hand.CardsInHand[i], i); // Verify correct card ammount
            }
        }
        [TestMethod]
        public void HandRemoval()
        {
            Hand hand = new Hand();
            // Add 6 cards as before
            hand.InsertCard(1);
            hand.InsertCard(2);
            hand.InsertCard(2);
            hand.InsertCard(3);
            hand.InsertCard(3);
            hand.InsertCard(3);
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(hand.CardsInHand.ContainsKey(i));
                hand.RemoveCard(i); // Remove the card
                if(i == 1)
                {
                    Assert.IsFalse(hand.CardsInHand.ContainsKey(i));
                }
                else
                {
                    Assert.AreEqual(hand.CardsInHand[i], i - 1);
                }
            }
        }
    }
}