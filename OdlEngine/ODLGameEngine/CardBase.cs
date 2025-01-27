using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ODLGameEngine
{
    public enum CardType
    {
        UNKNOWN,
        UNIT,
        BUILDING,
        SKILL
    }

    /// <summary>
    /// Which expansion the card belongs to
    /// </summary>
    public enum ExpansionId
    {
        BASE
    }

    /// <summary>
    /// Defines how/where card can be targeted, useful for giving options at a first glance to a player
    /// </summary>
    public enum TargetMode
    {
        NO_TARGET,
        ANY_LANE,
        PLAINS_ONLY,
        FOREST_ONLY,
        MOUNTAIN_ONLY,
        PLAINS_FORBIDDEN,
        FOREST_FORBIDDEN,
        MOUNTAIN_FORBIDDEN
    }

    /// <summary>
    /// The condition that makes the card say "yes, this (lane?) is a valid target"
    /// </summary>
    public enum TargetCondition
    {
        NONE, /// Can be played always
        BLUEPRINT, /// Subject to blueprint
        LANE_HAS_ENEMY_UNIT,
        LANE_HAS_ENEMY_BUILDING,
        LANE_HAS_FRIENDLY_UNIT,
        LANE_HAS_FRIENDLY_BUILDING
    }

    /// <summary>
    /// Defines how a card looks (in the hand) and contains all info that system needs to visualize and decide how card can be played
    /// </summary>
    public class Card
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Text { get; set; } = "";
        public CardType CardType { get; set; } = CardType.UNKNOWN;
        public TargetMode TargetMode { get; set; } = TargetMode.NO_TARGET;
        public List<TargetCondition> TargetConditions { get; set; } = new List<TargetCondition>();
        // Which tiles would be available to build in each
        public int PlainsBpCondition { get; set; } = 0b0;
        public int ForestBpCondition { get; set; } = 0b0;
        public int MountainsBpCondition { get; set; } = 0b0;
        // Playable info (will be in card)
        public int Cost { get; set; } = 0;
        public int Hp { get; set; } = 0;
        public int Movement { get; set; } = 0;
        public int MovementDenominator { get; set; } = 1;
        public int Attack { get; set; } = 0;
        public int Rarity { get; set; } = 0;
        public ExpansionId Expansion { get; set; } = ExpansionId.BASE;
        public PlayerClassType ClassType { get; set; } = PlayerClassType.BASE;
        public bool StealthPlay { get; set; } = false; // Whether card triggers a stealth case
    }
}
