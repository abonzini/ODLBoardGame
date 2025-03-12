using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /// <summary>
    /// Loads and finds data of card to play it or get additional info
    /// </summary>
    public class CardFinder
    {
        protected Dictionary<int, EntityBase> cardData = new Dictionary<int, EntityBase>();
        protected readonly string _baseDir;
        public CardFinder(string baseDir)
        {
            _baseDir = baseDir;
        }
        public virtual EntityBase GetCard(int id)
        {
            if (cardData.TryGetValue(id, out EntityBase cardEntity)) // Need to only parse data I yet didn't parse
            {
                return cardEntity;
            }

            string[] allLines = File.ReadAllLines(Path.Combine(_baseDir, "index.csv")); // Open index file
            foreach (string line in allLines)
            {
                string[] splitLines = line.Split(',');
                if(id == int.Parse(splitLines[0])) // Found the desired ID
                {
                    _ = Enum.TryParse(splitLines[1], out CardType cardtype);
                    string expa = splitLines[2];
                    string cardClass = splitLines[3];
                    // Found all I need from card dir, now I import the card json
                    string cardInfoFile = Path.Combine(_baseDir, "CardData", expa, cardClass + $"{id}.json");
                    // Load the specific card data
                    switch (cardtype)
                    {
                        case CardType.UNIT:
                            cardEntity = JsonSerializer.Deserialize<EntityBase>(File.ReadAllText(cardInfoFile)); ;
                            break;
                        case CardType.BUILDING:
                        case CardType.SKILL:
                        case CardType.UNKNOWN:
                        default:
                            throw new Exception("Unrecognised card type when deserializing");
                    }
                    cardData[id] = cardEntity;
                    return cardEntity;
                }
            }
            throw new Exception("Card not found!");
        }
    }
}
