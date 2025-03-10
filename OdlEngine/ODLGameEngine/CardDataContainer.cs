using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /// <summary>
    /// Container for de-serializing card data
    /// </summary>
    public class CardDataContainer
    {
        public Card CardData { get; set; }
        public Unit UnitData { get; set; } = null;
        public Skill SkillData { get; set; } = null;
        public Building BuildingData { get; set; } = null;
    }

    /// <summary>
    /// Loads and finds data of card to play it or get additional info
    /// </summary>
    public class CardFinder
    {
        protected Dictionary<int, Card> cardBasicData = new Dictionary<int, Card>();
        protected Dictionary<int, Unit> unitData = new Dictionary<int, Unit>();
        protected Dictionary<int, Building> buildingData = new Dictionary<int, Building>();
        protected Dictionary<int, Skill> skillData = new Dictionary<int, Skill>();
        protected readonly string _baseDir;
        public CardFinder(string baseDir)
        {
            _baseDir = baseDir;
        }
        protected void LoadCard(int id)
        {
            if (cardBasicData.ContainsKey(id)) return; // Need to only parse data I yet didn't parse

            string[] allLines = File.ReadAllLines(Path.Combine(_baseDir, "index.csv")); // Open index file
            foreach (string line in allLines)
            {
                string[] splitLines = line.Split(',');
                if(id == int.Parse(splitLines[0])) // Found the desired ID
                {
                    string expa = splitLines[1];
                    string cardClass = splitLines[2];
                    // Found all I need from card dir, now I import the card json
                    string cardInfoFile = Path.Combine(_baseDir, "CardData", expa, cardClass + $"{id.ToString()}.json");
                    CardDataContainer cardInfo = JsonSerializer.Deserialize<CardDataContainer>(File.ReadAllText(cardInfoFile));
                    // Loaded data, add one card to right places
                    Card card = cardInfo.CardData;
                    cardBasicData[id] = card;
                    switch (card.CardType)
                    {
                        case CardType.UNIT:
                            unitData[id] = cardInfo.UnitData;
                            break;
                        case CardType.SKILL:
                            skillData[id] = cardInfo.SkillData;
                            break;
                        case CardType.BUILDING:
                            buildingData[id] = cardInfo.BuildingData;
                            break;
                        default:
                            throw new InvalidDataException($"Card doesn't have proper info, in card document {cardInfoFile} id {card.Id}");
                    }
                }
            }
        }
        /// <summary>
        /// Gets the card data from loaded collection
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Card data</returns>
        public virtual Card GetCardData(int id)
        {
            LoadCard(id);
            return cardBasicData[id];
        }
        /// <summary>
        /// Gets skill data from card (to play skill)
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Skill data</returns>
        /// <exception cref="InvalidDataException">If card id doesn't correspond to a skill</exception>
        public virtual Skill GetSkillData(int id)
        {
            LoadCard(id);
            return (cardBasicData[id].CardType == CardType.SKILL)? skillData[id] : throw new InvalidDataException($"Chosen card is not a skill!");
        }
        /// <summary>
        /// Gets unit data from card (to play unit)
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Unit data</returns>
        /// <exception cref="InvalidDataException">If card id doesn't correspond to an unit</exception>
        public virtual Unit GetUnitData(int id)
        {
            LoadCard(id);
            return (cardBasicData[id].CardType == CardType.UNIT) ? unitData[id] : throw new InvalidDataException($"Chosen card is not an unit!");
        }
        /// <summary>
        /// Gets building data from card (to play building)
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Building data</returns>
        /// <exception cref="InvalidDataException">If card id doesn't correspond to a building</exception>
        public virtual Building GetBuildingData(int id)
        {
            LoadCard(id);
            return (cardBasicData[id].CardType == CardType.BUILDING) ? buildingData[id] : throw new InvalidDataException($"Chosen card is not an building!");
        }
    }
}
