using Newtonsoft.Json;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Building : PlacedEntity
    {
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            return Name;
        }
        public override object Clone()
        {
            Building newBuilding = (Building)base.Clone();
            return newBuilding;
        }
    }
}
