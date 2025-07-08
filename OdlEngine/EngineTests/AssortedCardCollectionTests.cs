using ODLGameEngine;

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
            col.AddToCollection(1);
            col.AddToCollection(2);
            col.AddToCollection(2);
            col.AddToCollection(3);
            col.AddToCollection(3);
            col.AddToCollection(3);
            Assert.AreEqual(col.CardCount, 6); // Verify correct number
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(col.HasCardInCollection(i)); // In dict
                Assert.AreEqual(col.CheckAmountInCollection(i), i); // Verify correct card ammount
            }
        }
        [TestMethod]
        public void Removal()
        {
            AssortedCardCollection col = new AssortedCardCollection();
            // Add 6 cards as before
            col.AddToCollection(1);
            col.AddToCollection(2);
            col.AddToCollection(2);
            col.AddToCollection(3);
            col.AddToCollection(3);
            col.AddToCollection(3);
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(col.HasCardInCollection(i));
                col.RemoveFromCollection(i); // Remove the card
                if (i == 1)
                {
                    Assert.IsFalse(col.HasCardInCollection(i));
                }
                else
                {
                    Assert.AreEqual(col.CheckAmountInCollection(i), i - 1);
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
                col1.AddToCollection(rn);
                col2.AddToCollection(rn);
            }
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode());
            // Now add an extra random card
            int rCard = _rng.Next(100);
            col2.AddToCollection(rCard);
            Assert.AreNotEqual(col1.GetHashCode(), col2.GetHashCode());
            // Revert this, should be back to equal
            col2.RemoveFromCollection(rCard);
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode());
        }
        [TestMethod]
        public void HashOrderTest()
        {
            AssortedCardCollection col1 = new AssortedCardCollection();
            AssortedCardCollection col2 = new AssortedCardCollection();
            for (int i = 0; i < 10; i++) // Add cards 0-9 but in different insertion order
            {
                col1.AddToCollection(i);
                col2.AddToCollection(9 - i);
            }
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode()); // Still should have same hash
        }
        [TestMethod]
        public void AddMany()
        {
            AssortedCardCollection col = new AssortedCardCollection();
            // Add 6 cards, 1, 2, 2, 3, 3, 3
            col.AddToCollection(1);
            col.AddToCollection(2, 2);
            col.AddToCollection(3, 3);
            Assert.AreEqual(col.CardCount, 6); // Verify correct number
            for (int i = 1; i <= 3; i++)
            {
                Assert.IsTrue(col.HasCardInCollection(i)); // In dict
                Assert.AreEqual(col.CheckAmountInCollection(i), i); // Verify correct card ammount
            }
        }
        [TestMethod]
        public void RemoveMany()
        {
            AssortedCardCollection col = new AssortedCardCollection();
            col.AddToCollection(1, 10);
            Assert.AreEqual(col.CardCount, 10); // Verify correct number
            Assert.AreEqual(col.CheckAmountInCollection(1), 10); // Verify correct card ammount
            Random _rng = new Random();
            int removed = _rng.Next(2, 10);
            col.RemoveFromCollection(1, removed);
            Assert.AreEqual(col.CardCount, 10 - removed); // Verify correct number
            Assert.AreEqual(col.CheckAmountInCollection(1), 10 - removed); // Verify correct card ammount
        }
        [TestMethod]
        public void AmountsHistogramAddition()
        {
            Random _rng = new Random();
            // Adds random cards in 1-3s count how many
            AssortedCardCollection col = new AssortedCardCollection();
            int[] counts = [0, 0, 0];
            for (int i = 1; i <= 10; i++) // Add cards 1-10
            {
                int amount = _rng.Next(1, 4); // Between 1-3
                counts[amount - 1]++;
                col.AddToCollection(i, amount);
            }
            // Now I check
            for (int i = 0; i < 3; i++)
            {
                if (counts[i] > 0)
                {
                    Assert.AreEqual(counts[i], col.CountHistogram[i + 1].Count);
                }
                else
                {
                    Assert.IsFalse(col.CountHistogram.ContainsKey(i + 1));
                }
            }
        }
        [TestMethod]
        public void AmountsHistogramRemoval()
        {
            Random _rng = new Random();
            // Remove from collection and see if amounts histogram remains ok
            AssortedCardCollection col = new AssortedCardCollection();
            for (int i = 1; i <= 10; i++) // Add cards 1-10, 3 copies each
            {
                col.AddToCollection(i, 3);
            }
            int[] counts = [0, 0, 10];
            for (int i = 1; i <= 10; i++) // Remove randomly
            {
                int amountRemoved = _rng.Next(0, 3); // Between 1-3 copies now
                counts[2]--;
                counts[2 - amountRemoved]++;
                col.RemoveFromCollection(i, amountRemoved);
            }
            // Now I check
            for (int i = 0; i < 3; i++)
            {
                if (counts[i] > 0)
                {
                    Assert.AreEqual(counts[i], col.CountHistogram[i + 1].Count);
                }
                else
                {
                    Assert.IsFalse(col.CountHistogram.ContainsKey(i + 1));
                }
            }
        }
        [TestMethod]
        public void AmountsHistogramCompleteRemoval()
        {
            // Remove from collection and see if amounts histogram remains ok
            AssortedCardCollection col = new AssortedCardCollection();
            col.AddToCollection(1, 2);
            // Check everyhting about card and count histogram is ok
            Assert.AreEqual(2, col.CardCount);
            Assert.AreEqual(2, col.CheckAmountInCollection(1));
            Assert.AreEqual(1, col.CountHistogram.Count);
            Assert.IsTrue(col.CountHistogram[2].Contains(1)); // Key is here
            // Now I remove an excessive amount, should empty the collection
            col.RemoveFromCollection(1, 200);
            Assert.AreEqual(0, col.CardCount);
            Assert.AreEqual(0, col.CheckAmountInCollection(1));
            Assert.AreEqual(0, col.CountHistogram.Count);
        }
    }
}