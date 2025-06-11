using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Skill : IngameEntity
    {
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardTargetingType TargetType { get; set; } = CardTargetingType.BOARD;
        [JsonProperty]
        [JsonConverter(typeof(FlagEnumJsonConverter))]
        public EntityOwner TargetOwner { get; set; } = EntityOwner.NONE;
        public override object Clone()
        {
            Skill newSkill = (Skill)base.Clone();
            newSkill.TargetType = TargetType;
            newSkill.TargetOwner = TargetOwner;
            return newSkill;
        }
    }
}
