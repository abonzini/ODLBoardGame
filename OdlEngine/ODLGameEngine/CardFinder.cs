using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ODLGameEngine
{
    /// <summary>
    /// Loads and finds data of card to play it or get additional info
    /// </summary>
    public class CardFinder
    {
        protected Dictionary<int, EntityBase> cardData = new Dictionary<int, EntityBase>();
        protected readonly string _baseDir;
        public CardFinder()
        {
            _baseDir = "";
        }
        public CardFinder(string baseDir)
        {
            _baseDir = baseDir;
        }
        public void InjectCard(int id, EntityBase entity)
        {
            cardData[id] = entity;
        }
        public EntityBase GetCard(int id)
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
                    _ = Enum.TryParse(splitLines[1].ToUpper(), out EntityType cardtype);
                    string expa = splitLines[2];
                    string cardClass = splitLines[3];
                    // Found all I need from card dir, now I import the card json
                    string cardInfoFile = Path.Combine(_baseDir, "CardData", expa, cardClass, $"{id}.json");
                    // Load the specific card data
                    switch (cardtype)
                    {
                        case EntityType.UNIT:
                            cardEntity = JsonConvert.DeserializeObject<Unit>(File.ReadAllText(cardInfoFile));
                            break;
                        case EntityType.SKILL:
                            cardEntity = JsonConvert.DeserializeObject<Skill>(File.ReadAllText(cardInfoFile));
                            break;
                        case EntityType.BUILDING:
                            cardEntity = JsonConvert.DeserializeObject<Building>(File.ReadAllText(cardInfoFile));
                            break;
                        case EntityType.PLAYER:
                            cardEntity = JsonConvert.DeserializeObject<Player>(File.ReadAllText(cardInfoFile));
                            break;
                        case EntityType.NONE:
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
