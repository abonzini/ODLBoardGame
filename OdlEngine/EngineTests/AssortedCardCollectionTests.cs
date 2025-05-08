using ODLGameEngine;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace EngineTests
{
    [TestClass]
    public class AssortedCardCollectionTests
    {
        [TestMethod]
        public void Empty()
        {
            AssortedCardCollection col = new AssortedCardCollection();
            Assert.AreEqual(col.CardCount, 0); // Verify empty hand
        }
        [TestMethod]
        public void Insertion()
        {
            AssortedCardCollection col = new AssortedCardCollection();
            // Add 6 cards, 1, 2, 2, 3, 3, 3
            col.InsertCard(1);
            col.InsertCard(2);
            col.InsertCard(2);
            col.InsertCard(3);
            col.InsertCard(3);
            col.InsertCard(3);
            Assert.AreEqual(col.CardCount, 6); // Verify correct number
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(col.HasCard(i)); // In dict
                Assert.AreEqual(col.CheckAmount(i), i); // Verify correct card ammount
            }
        }
        [TestMethod]
        public void Removal()
        {
            AssortedCardCollection col = new AssortedCardCollection();
            // Add 6 cards as before
            col.InsertCard(1);
            col.InsertCard(2);
            col.InsertCard(2);
            col.InsertCard(3);
            col.InsertCard(3);
            col.InsertCard(3);
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(col.HasCard(i));
                col.RemoveCard(i); // Remove the card
                if(i == 1)
                {
                    Assert.IsFalse(col.HasCard(i));
                }
                else
                {
                    Assert.AreEqual(col.CheckAmount(i), i - 1);
                }
            }
        }
        [TestMethod]
        public void HashTest()
        {
            AssortedCardCollection col1 = new AssortedCardCollection();
            AssortedCardCollection col2 = new AssortedCardCollection();
            Random _rng = new Random();
            for (int i = 0; i < 10; i++) // Add 10 random hands between 0-99
            {
                int rn = _rng.Next(100);
                col1.InsertCard(rn);
                col2.InsertCard(rn);
            }
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode());
            // Now add an extra random card
            int rCard = _rng.Next(100);
            col2.InsertCard(rCard);
            Assert.AreNotEqual(col1.GetHashCode(), col2.GetHashCode());
            // Revert this, should be back to equal
            col2.RemoveCard(rCard);
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode());
        }
        [TestMethod]
        public void HashOrderTest()
        {
            AssortedCardCollection col1 = new AssortedCardCollection();
            AssortedCardCollection col2 = new AssortedCardCollection();
            for (int i = 0; i < 10; i++) // Add cards 0-9 but in different insertion order
            {
                col1.InsertCard(i);
                col2.InsertCard(9-i);
            }
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode()); // Still should have same hash
        }
        //[TestMethod]
        //public void HashStressTest()
        //{
        //    HashSet<int> hashes = new HashSet<int>();
        //    float total = 0;
        //    float collisions = 0;
        //    Random _rng = new Random();
        //    for (int i=0; i<1000000; i++) // Test 1000000 times, create unique cols and verify few collisions
        //    {
        //        AssortedCardCollection hand = new AssortedCardCollection();
        //        int repeat = _rng.Next(5,11);
        //        for (int j = 0; j < repeat; j++) // Add cards 0-99 between 5-10 times (test that unique cols are unique, not ones with few cards)
        //        {
        //            hand.InsertCard(_rng.Next(100));
        //        }
        //        total++;
        //        if(hashes.Contains(hand.GetHash()))
        //        {
        //            collisions++;
        //        }
        //        else
        //        {
        //            hashes.Add(hand.GetHash());
        //        }
        //    }
        //    Assert.IsTrue(collisions / total < 0.01); // Try for 1% or less of collisions
        //}
    }
}