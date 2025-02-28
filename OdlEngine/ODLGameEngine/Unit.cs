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
        protected int _movement = 0;
        /// <summary>
        /// The movement stat of the unit
        /// </summary>
        public int Movement { get { return _movement; } set { _dirtyHash = true; _movement = value; } }
        [JsonProperty]
        protected int _movementDenominator = 1;
        /// <summary>
        /// The movement denominator stat
        /// </summary>
        public int MovementDenominator { get { return _movementDenominator; } set { _dirtyHash = true; _movementDenominator = value; } }
        [JsonProperty]
        protected int _attack = 0;
        /// <summary>
        /// Attack stat
        /// </summary>
        public int Attack { get { return _attack; } set { _dirtyHash = true; _attack = value; } }
        [JsonProperty]
        protected int _mvtCooldownTimer = 0;
        /// <summary>
        /// Count to check when unit can move again
        /// </summary>
        public int MvtCooldownTimer { get { return _mvtCooldownTimer; } set { _dirtyHash = true; _mvtCooldownTimer= value; } }
        /// <summary>
        /// Gets the hash of the entity
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHash()
        {
            if (_dirtyHash) // Recalculates only when dirty
            {
                HashCode hash = new HashCode();
                hash.Add(base.GetHash());
                hash.Add(_movement);
                hash.Add(_movementDenominator);
                hash.Add(_attack);
                hash.Add(_mvtCooldownTimer);
                _hash = hash.ToHashCode();
                _dirtyHash = false; // Currently updated hash
            }
            return _hash;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
