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
    public class Unit : PlaceableEntity
    {
        // Stats
        [JsonProperty]
        public int Movement { get; set; } = 0;
        [JsonProperty]
        public int MovementDenominator { get; set; } = 1;
        [JsonProperty]
        public int Attack { get; set; } = 0;
        [JsonProperty]
        public int MvtCooldownTimer { get; set; } = 0;
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHash()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHash());
            hash.Add(Movement);
            hash.Add(MovementDenominator);
            hash.Add(Attack);
            hash.Add(MvtCooldownTimer);
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
