using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /// <summary>
    /// Classes types. Number same as the card number
    /// </summary>
    public enum PlayerClassType
    {
        BASE = -1
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Player : LivingEntity
    {
        [JsonProperty]
        public int CurrentGold { get; set; } = 0;
        [JsonProperty]
        public bool PowerAvailable { get; set; } = true;
        [JsonProperty]
        public AssortedCardCollection Hand { get; set; } = new AssortedCardCollection();
        [JsonProperty]
        public Deck Deck { get; set; } = new Deck();
        [JsonProperty]
        public AssortedCardCollection DiscardPile { get; set; } = new AssortedCardCollection();
        [JsonProperty]
        public int ActivePowerId { get; set; } = GameConstants.RUSH_CARD_ID;

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(CurrentGold);
            hash.Add(PowerAvailable);
            hash.Add(Hand.GetHashCode());
            hash.Add(Deck.GetHashCode());
            hash.Add(DiscardPile.GetHashCode());
            hash.Add(ActivePowerId);
            return hash.ToHashCode();
        }
        public override object Clone()
        {
            Player newEntity = (Player)base.Clone();
            newEntity.CurrentGold = CurrentGold;
            newEntity.PowerAvailable = PowerAvailable;
            newEntity.Deck = (Deck)Deck.Clone();
            newEntity.Hand = (AssortedCardCollection)Hand.Clone();
            newEntity.DiscardPile = (AssortedCardCollection)DiscardPile.Clone();
            newEntity.ActivePowerId = ActivePowerId;
            return newEntity;
        }
    }
}
