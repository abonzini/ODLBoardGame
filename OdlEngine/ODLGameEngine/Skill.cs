using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Linq;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Skill : IngameEntity
    {
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardTargetingType TargetType { get; set; } = CardTargetingType.BOARD;
        public override object Clone()
        {
            Skill newSkill = (Skill)base.Clone();
            newSkill.TargetType = TargetType;
            return newSkill;
        }
    }
}
