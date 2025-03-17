using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class Building : PlacedEntity
    {
        public int[] PlainsBp { get; set; } = null;
        public int[] ForestBp { get; set; } = null;
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
    }
}
