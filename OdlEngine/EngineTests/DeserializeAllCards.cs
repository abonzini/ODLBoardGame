using DeserializationVerifier;

namespace EngineTests
{
    [TestClass]
    public class DeserializeAllCards
    {
        [TestMethod]
        public void DeserializeAllCreatedCards()
        {
            const int MIN_INDEX = -1; // Need to edit as I load cards
            const int MAX_INDEX = -1;
            DeserializationHelper helper = new DeserializationHelper();
            for(int i = MIN_INDEX; i <= MAX_INDEX; i++)
            {
                Assert.IsTrue(helper.IsJsonValid(i));
            }
        }
    }
}
