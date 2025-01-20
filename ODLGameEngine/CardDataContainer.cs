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
        public Dictionary<int, Card> cardBasicData = new Dictionary<int, Card>();
        public Dictionary<int, Unit> unitData = new Dictionary<int, Unit>();
        public Dictionary<int, Building> buildingData = new Dictionary<int, Building>();
        public Dictionary<int, Skill> skillData = new Dictionary<int, Skill>();
        string _baseDir;
        public CardFinder(string baseDir)
        {
            _baseDir = baseDir;
        }
        public void LoadCard(int id)
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
                    // Found all I need from card dir, now I import absolutely everything
                    string cardInfoFile = Path.Combine(_baseDir, "CardData", expa, cardClass + ".json");
                    List<CardDataContainer> allTheseCards = JsonSerializer.Deserialize<List<CardDataContainer>>(File.ReadAllText(cardInfoFile));
                    foreach(CardDataContainer cardInfo in allTheseCards)
                    {
                        // Loaded data, add one by one to right places
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
        }
    }
}
