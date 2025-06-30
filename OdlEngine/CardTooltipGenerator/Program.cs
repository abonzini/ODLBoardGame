using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ODLGameEngine;

namespace CardTooltipGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string destinationPath = args.Length > 0 ? args[0] : "";
            string resourcesPath = args.Length > 1 ? args[1] : "";

            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                Console.Write("Enter the destination path (parent of Generated): ");
                destinationPath = Console.ReadLine();
            }
            // Ensure output directories exist
            string generatedPath = Path.Combine(destinationPath, "Generated");
            string tooltipsPath = Path.Combine(generatedPath, "CardTooltips");
            if (!Directory.Exists(tooltipsPath))
            {
                Directory.CreateDirectory(tooltipsPath);
            }
            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                Console.Write("Enter the resources path (Container of all resources): ");
                resourcesPath = Console.ReadLine();
            }
            string keywordsPath = Path.Combine(resourcesPath, "Keywords");
            if (!Directory.Exists(keywordsPath))
            {
                Console.WriteLine($"no keywords folder");
                return;
            }
            // Get all possible keywords first
            List<Keyword> keywords = new List<Keyword>();
            foreach(string keywordFile in Directory.GetFiles(keywordsPath))
            {
                string jsonText = File.ReadAllText(keywordFile);
                keywords.Add(JsonConvert.DeserializeObject<Keyword>(jsonText));
            }
            // Find cards now
            string cardDataPath = Path.Combine(resourcesPath, "CardData");
            string indexFile = Path.Combine(cardDataPath, "index.csv");
            if (!File.Exists(indexFile))
            {
                Console.WriteLine($"index.csv not found at {indexFile}");
                return;
            }
            string[] indices = File.ReadAllLines(indexFile)[0].Split(',');
            int min = int.Parse(indices[0]);
            int max = int.Parse(indices[1]);
            Dictionary<int, CardIllustrationInfo> allCardsById = new Dictionary<int, CardIllustrationInfo>();
            Dictionary<string, CardIllustrationInfo> allCardsByName = new Dictionary<string, CardIllustrationInfo>();
            for (int i = min; i <= max; i++)
            {
                if (i == 0) continue;
                string cardInfoPath = Path.Combine(resourcesPath, "CardData", $"{i}-illustration.json");
                string jsonText = File.ReadAllText(cardInfoPath);
                CardIllustrationInfo cardInfo = JsonConvert.DeserializeObject<CardIllustrationInfo>(jsonText); // Get card illustration info
                allCardsById.Add(cardInfo.Id, cardInfo);
                allCardsByName.Add(cardInfo.Name.ToLower(), cardInfo);
            }
            // Got all cards, now to find what card references which card
            Dictionary<int, List<int>> interCardReferences = new Dictionary<int, List<int>>();
            foreach (KeyValuePair<int, CardIllustrationInfo> kvp in allCardsById)
            {
                List<int> thisCardReferences = new List<int>();
                string[] references = kvp.Value.Text.Split("#"); // Mentions are split by #
                for(int i  = 0; i < references.Length; i++)
                {
                    if (i % 2 == 0) continue; // Need the odd numbers (values between #s)
                    string reference = references[i].ToLower();
                    if(reference != kvp.Value.Name.ToLower()) // Can't reference itself!
                    {
                        thisCardReferences.Add(allCardsByName[reference].Id);
                    }
                }
                interCardReferences.Add(kvp.Key, thisCardReferences);
            }
            // Got all cards, now to find what card references which keyword
            Dictionary<int, List<string>> keywordReference = new Dictionary<int, List<string>>();
            foreach (KeyValuePair<int, CardIllustrationInfo> kvp in allCardsById)
            {
                List<string> thisCardKeywords = new List<string>();
                string[] potentialKeywords = kvp.Value.Text.Split("*"); // Tooltips are split by *
                for (int i = 0; i < potentialKeywords.Length; i++)
                {
                    if (i % 2 == 0) continue; // Need the odd numbers (values between #s)
                    string keyword = potentialKeywords[i].ToLower();
                    keyword = keyword.Trim();
                    switch(keyword.Last()) // Some trailing chars need to be dealt with
                    {
                        case '.':
                        case ',':
                        case ':':
                        case ';':
                            keyword = keyword.Remove(keyword.Length - 1); // Delete last
                            break;
                        default:
                            break;
                    }
                    foreach(Keyword kword in keywords) // Find the corresponding keyword
                    {
                        if(kword.Name.ToLower() == keyword || kword.Synonyms.Contains(keyword))
                        {
                            // Found it! Add if not there before
                            if(!thisCardKeywords.Contains(kword.Name.ToLower()))
                            {
                                thisCardKeywords.Add(kword.Name.ToLower());
                            }
                        }
                    }
                }
                keywordReference.Add(kvp.Key, thisCardKeywords);
            }
            // Finally, all cards processed, time to create the card's tooltip
            foreach (KeyValuePair<int, CardIllustrationInfo> kvp in allCardsById)
            {
                CardTooltip tooltip = new CardTooltip();
                tooltip.CardId = kvp.Key;
                tooltip.HasBlueprint = (kvp.Value.EntityType == EntityType.BUILDING);
                // Find cards
                tooltip.RelatedCards = [.. interCardReferences[kvp.Key]]; // Start by copying list
                for(int i = 0; i < tooltip.RelatedCards.Count;i++) // Iterate through these to find recursively (foundRefs.Count may increase!)
                {
                    foreach(int extraRef in interCardReferences[tooltip.RelatedCards[i]]) // Get references of next ref
                    {
                        if ((kvp.Key!= extraRef) && !tooltip.RelatedCards.Contains(extraRef))
                        {
                            tooltip.RelatedCards.Add(extraRef); // Add to end only if not already there and it's not myself!
                        }
                    }
                }
                // Finally, find keywords
                tooltip.Keywords = [.. keywordReference[kvp.Key]]; // Copy this card's keywords
                foreach(int card in tooltip.RelatedCards) // Also the keywords for related ones
                {
                    foreach(string keyword in keywordReference[card])
                    {
                        if(!tooltip.Keywords.Contains(keyword))
                        {
                            tooltip.Keywords.Add(keyword);
                        }
                    }
                }
                // Got all I need finally
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented, // Add indentation for better readability
                };
                // Serialize result
                string tooltipJson = JsonConvert.SerializeObject(tooltip, settings);
                File.WriteAllText(Path.Combine(tooltipsPath, $"{kvp.Key}.json"), tooltipJson);
            }
        }
    }
}
