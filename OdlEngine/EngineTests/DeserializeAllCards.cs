using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class DeserializeAllCards
    {
        CardFinder cardFinderToTest = new CardFinder(".\\..\\..\\..\\..\\..\\CardDatabase\\CardData");
        public bool IsJsonValid(int cardId)
        {
            bool valid = false;
            try
            {
                EntityBase card = cardFinderToTest.GetCard(cardId);
                valid = card != null;
            }
            catch (Exception ex)
            {
                valid = false;
            }
            return valid;
        }
        [TestMethod]
        public void DeserializeAllCreatedCards()
        {
            const int MIN_INDEX = -1; // Need to edit as I load cards
            const int MAX_INDEX = 2;
            for(int i = MIN_INDEX; i <= MAX_INDEX; i++)
            {
                if (i == 0) continue; // No 0 card
                Assert.IsTrue(IsJsonValid(i));
            }
        }
    }
}
