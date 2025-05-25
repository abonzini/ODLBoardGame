using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

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
            // Otherwise fetch the card, assuming I'm in the correct folder
            string cardJsonFile = Path.Combine(_baseDir, $"{id}.json");
            if(Path.Exists(cardJsonFile))
            {
                // Attempt to find what is this card
                EntityType cardType;
                using (StreamReader reader = new StreamReader(cardJsonFile))
                {
                    JObject json = JObject.Parse(reader.ReadToEnd());
                    cardType = json["EntityType"].ToObject<EntityType>();
                }
                // Load the specific card data
                switch (cardType)
                {
                    case EntityType.UNIT:
                        cardEntity = JsonConvert.DeserializeObject<Unit>(File.ReadAllText(cardJsonFile));
                        break;
                    case EntityType.SKILL:
                        cardEntity = JsonConvert.DeserializeObject<Skill>(File.ReadAllText(cardJsonFile));
                        break;
                    case EntityType.BUILDING:
                        cardEntity = JsonConvert.DeserializeObject<Building>(File.ReadAllText(cardJsonFile));
                        break;
                    case EntityType.PLAYER:
                        cardEntity = JsonConvert.DeserializeObject<Player>(File.ReadAllText(cardJsonFile));
                        break;
                    case EntityType.NONE:
                    default:
                        throw new Exception("Unrecognised card type when deserializing");
                }
                cardData[id] = cardEntity;
                return cardEntity;
            }
            throw new Exception("Card not found!");
        }
    }
}
