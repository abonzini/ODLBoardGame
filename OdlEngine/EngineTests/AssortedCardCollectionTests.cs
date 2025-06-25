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
            col.InsertToCollection(1);
            col.InsertToCollection(2);
            col.InsertToCollection(2);
            col.InsertToCollection(3);
            col.InsertToCollection(3);
            col.InsertToCollection(3);
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
            col.InsertToCollection(1);
            col.InsertToCollection(2);
            col.InsertToCollection(2);
            col.InsertToCollection(3);
            col.InsertToCollection(3);
            col.InsertToCollection(3);
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
                col1.InsertToCollection(rn);
                col2.InsertToCollection(rn);
            }
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode());
            // Now add an extra random card
            int rCard = _rng.Next(100);
            col2.InsertToCollection(rCard);
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
                col1.InsertToCollection(i);
                col2.InsertToCollection(9 - i);
            }
            Assert.AreEqual(col1.GetHashCode(), col2.GetHashCode()); // Still should have same hash
        }
    }
}