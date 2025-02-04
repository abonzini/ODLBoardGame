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
    [Flags]
    public enum CardTargets
    {
        GLOBAL = 0,
        PLAINS = 1,
        FOREST = 2,
        MOUNTAIN = 4,
        ALL_BUT_MOUNTAIN = 3,
        ALL_BUT_FOREST = 5,
        ALL_BUT_PLAINS = 6,
        ANY_LANE = 7,
        INVALID = 99
    }

    /// <summary>
    /// The condition that makes the card say "yes, this (lane?) is a valid target"
    /// </summary>
    public enum TargetCondition
    {
        NONE, /// Can be played always
        // Could be, but only implement as needed
        //BLUEPRINT, /// Subject to blueprint
        //LANE_HAS_ENEMY_UNIT,
        //LANE_HAS_ENEMY_BUILDING,
        //LANE_HAS_FRIENDLY_UNIT,
        //LANE_HAS_FRIENDLY_BUILDING
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
        public CardTargets TargetOptions { get; set; } = CardTargets.GLOBAL; // Which lane(s) if any the card could work on
        public List<TargetCondition> TargetConditions { get; set; } = new List<TargetCondition>(); // What needs to happen for a card to be "playable" in a lane
        // Which tiles would be available to build in each
        public int PlainsBpCondition { get; set; } = 0b0;
        public int ForestBpCondition { get; set; } = 0b0;
        public int MountainsBpCondition { get; set; } = 0b0;
        // Playable info (will be in card)
        public string Cost { get; set; } = "";
        public string Hp { get; set; } = "";
        public string Movement { get; set; } = "";
        public string Attack { get; set; } = "";
        public int Rarity { get; set; } = 0;
        public ExpansionId Expansion { get; set; } = ExpansionId.BASE;
        public PlayerClassType ClassType { get; set; } = PlayerClassType.BASE;
        public bool StealthPlay { get; set; } = false; // Whether card triggers a stealth case
    }
}
