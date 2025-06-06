using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class DeserializeAllCards
    {
        public static bool IsJsonValid(int cardId)
        {
            CardFinder cardFinderToTest = new CardFinder(".\\..\\..\\..\\..\\..\\CardResources\\CardData");
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
            const int MIN_INDEX = -1; // Need to edit as I load cards
            const int MAX_INDEX = 3;
            for (int i = MIN_INDEX; i <= MAX_INDEX; i++)
            {
                if (i == 0) continue; // No 0 card
                Assert.IsTrue(IsJsonValid(i));
            }
        }
    }
}
