using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class DeserializeAllCards
    {
        public static bool IsJsonValid(int cardId, CardFinder cardFinderToTest)
        {
            bool valid;
            try
            {
                EntityBase card = cardFinderToTest.GetCard(cardId);
                valid = card != null;
            }
            catch
            {
                valid = false;
            }
            return valid;
        }
        [TestMethod]
        public void DeserializeAllCreatedCards()
        {
            string cardDataPath = ".\\..\\..\\..\\..\\..\\CardResources\\CardData";
            CardFinder cardFinderToTest = new CardFinder(cardDataPath);
            string cardIndexFile = Path.Combine(cardDataPath, "index.csv");
            Assert.IsTrue(File.Exists(cardIndexFile));
            string[] indices = File.ReadAllLines(cardIndexFile)[0].Split(',');
            int min, max;
            min = int.Parse(indices[0]);
            max = int.Parse(indices[1]);
            for (int i = min; i <= max; i++)
            {
                if (i == 0) continue; // No 0 card
                Assert.IsTrue(IsJsonValid(i, cardFinderToTest));
            }
        }
    }
}
