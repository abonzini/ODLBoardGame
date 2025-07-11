﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace ODLGameEngine
{
    /// <summary>
    /// Loads and finds data of card to play it or get additional info
    /// </summary>
    public class CardFinder
    {
        protected ConcurrentDictionary<int, EntityBase> cardData = new ConcurrentDictionary<int, EntityBase>();
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
            cardData.TryAdd(id, entity);
        }
        public EntityBase GetCard(int id)
        {
            return cardData.GetOrAdd(id, FindCard);
        }
        private EntityBase FindCard(int id)
        {
            EntityBase cardEntity;
            // Otherwise fetch the card, assuming I'm in the correct folder
            string cardJsonFile = Path.Combine(_baseDir, $"{id}.json");
            if (Path.Exists(cardJsonFile))
            {
                // Attempt to find what is this card
                EntityType cardType;
                using (StreamReader reader = new StreamReader(cardJsonFile))
                {
                    JObject json = JObject.Parse(reader.ReadToEnd());
                    cardType = json["EntityType"].ToObject<EntityType>();
                }
                // Load the specific card data
                cardEntity = cardType switch
                {
                    EntityType.UNIT => JsonConvert.DeserializeObject<Unit>(File.ReadAllText(cardJsonFile)),
                    EntityType.SKILL => JsonConvert.DeserializeObject<Skill>(File.ReadAllText(cardJsonFile)),
                    EntityType.BUILDING => JsonConvert.DeserializeObject<Building>(File.ReadAllText(cardJsonFile)),
                    EntityType.PLAYER => JsonConvert.DeserializeObject<Player>(File.ReadAllText(cardJsonFile)),
                    _ => throw new Exception("Unrecognised card type when deserializing"),
                };
                cardData[id] = cardEntity;
                return cardEntity;
            }
            throw new Exception("Card not found!");
        }
    }
}
