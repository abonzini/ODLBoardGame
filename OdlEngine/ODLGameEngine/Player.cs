using Newtonsoft.Json;
using System.ComponentModel;

namespace ODLGameEngine
{
    /// <summary>
    /// Classes types. Number will be same as the card number, most likely negative
    /// </summary>
    public enum PlayerTribe
    {
        BASE = -1
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Player : LivingEntity
    {
        [JsonProperty]
        public int CurrentGold { get; set; } = 0;
        [JsonProperty]
        [DefaultValue(true)]
        public bool PowerAvailable { get; set; } = true;
        [JsonProperty]
        public AssortedCardCollection Hand { get; set; } = new AssortedCardCollection();
        [JsonProperty]
        public Deck Deck { get; set; } = new Deck();
        [JsonProperty]
        public AssortedCardCollection DiscardPile { get; set; } = new AssortedCardCollection();
        [JsonProperty]
        public int ActivePowerId { get; set; } = GameConstants.DEFAULT_ACTIVE_POWER_ID;

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
