using Newtonsoft.Json;
using ODLGameEngine;

namespace DeserializationVerifier
{
    public class DeserializationHelper
    {
        CardFinder cardFinderToTest = new CardFinder("C:\\Users\\augus\\Documents\\Boardgame\\ODLBoardGame\\CardDatabase");
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
        public string GetJsonBack(int cardId)
        {
            EntityBase card = cardFinderToTest.GetCard(cardId);
            // Success!
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented, // Add indentation for better readability
                DefaultValueHandling = DefaultValueHandling.Ignore // Exclude default values
            };
            string res = JsonConvert.SerializeObject(card, settings);
            return res;
        }
    }
}
