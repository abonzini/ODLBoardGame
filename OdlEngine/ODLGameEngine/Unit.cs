using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ODLGameEngine
{
    /// <summary>
    /// Defines a unit that was placed on the board
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Unit : PlacedEntity
    {
        // Stats
        [JsonProperty]
        [JsonConverter(typeof(StatJsonConverter))]
        public Min0Stat Movement { get; set; } = new Min0Stat();
        [JsonProperty]
        [JsonConverter(typeof(StatJsonConverter))]
        public Min1Stat MovementDenominator { get; set; } = new Min1Stat();
        [JsonProperty]
        [JsonConverter(typeof(StatJsonConverter))]
        public Min0Stat Attack { get; set; } = new Min0Stat();
        [JsonProperty]
        public int MvtCooldownTimer { get; set; } = 0;
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetGameStateHash());
            hash.Add(Movement.GetGameStateHash());
            hash.Add(MovementDenominator.GetGameStateHash());
            hash.Add(Attack.GetGameStateHash());
            hash.Add(MvtCooldownTimer);
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            return Name;
        }
        public override object Clone()
        {
            Unit newUnit = (Unit)base.Clone();
            newUnit.Movement = (Min0Stat)Movement.Clone();
            newUnit.MovementDenominator = (Min1Stat)MovementDenominator.Clone();
            newUnit.Attack = (Min0Stat)Attack.Clone();
            newUnit.MvtCooldownTimer = MvtCooldownTimer;
            return newUnit;
        }
    }
}
