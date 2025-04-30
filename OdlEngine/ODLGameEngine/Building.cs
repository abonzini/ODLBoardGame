using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Building : PlacedEntity
    {
        [JsonProperty]
        public int[] PlainsBp { get; set; } = null;
        [JsonProperty]
        public int[] ForestBp { get; set; } = null;
        [JsonProperty]
        public int[] MountainBp { get; set; } = null;
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetGameStateHash());
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            return Name;
        }
        public override object Clone()
        {
            Building newBuilding = (Building) base.Clone();
            newBuilding.ForestBp = ForestBp;
            newBuilding.PlainsBp = PlainsBp;
            newBuilding.MountainBp = MountainBp;
            return newBuilding;
        }
    }
}
