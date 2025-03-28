﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /// <summary>
    /// Classes types
    /// </summary>
    public enum PlayerClassType
    {
        BASE
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PlayerState : BoardEntity
    {
        public PlayerState() // Default values of base cards are different
        {
            EntityPlayInfo.EntityType = EntityType.PLAYER;
            Hp = GameConstants.STARTING_HP;
        }
        [JsonProperty]
        public PlayerClassType PlayerClass { get; set; } = PlayerClassType.BASE;
        [JsonProperty]
        public int Gold { get; set; } = 0;
        [JsonProperty]
        public bool PowerAvailable { get; set; } = true;
        [JsonProperty]
        public AssortedCardCollection Hand { get; set; } = new AssortedCardCollection();
        [JsonProperty]
        public Deck Deck { get; set; } = new Deck();
        [JsonProperty]
        public AssortedCardCollection DiscardPile { get; set; } = new AssortedCardCollection();
        [JsonProperty]
        public int ActivePowerCast { get; set; } = GameConstants.RUSH_CARD_ID;

        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetGameStateHash());
            hash.Add(PlayerClass);
            hash.Add(Gold);
            hash.Add(PowerAvailable);
            hash.Add(Hand.GetGameStateHash());
            hash.Add(Deck.GetGameStateHash());
            hash.Add(DiscardPile.GetGameStateHash());
            hash.Add(ActivePowerCast);
            return hash.ToHashCode();
        }
    }
}
