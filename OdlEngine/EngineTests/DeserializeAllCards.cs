using DeserializationVerifier;

namespace EngineTests
{
    [TestClass]
    public class DeserializeAllCards
    {
        [TestMethod]
        public void DeserializeAllCreatedCards()
        {
            const int NUMBER_OF_CREATED_CARDS = 0;
            DeserializationHelper helper = new DeserializationHelper();
            for(int i = 0; i <= NUMBER_OF_CREATED_CARDS; i++)
            {
                Assert.IsTrue(helper.IsJsonValid(i));
            }
        }
    }
}
