using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    //[TestClass]
    public class CardSerializationHelper
    {
        //[TestMethod]
        public void HelpDeserializing()
        {
            Dictionary<InteractionType, List<Effect>> dict = new Dictionary<InteractionType, List<Effect>>();
            Effect effect = new Effect()
            {
                EffectType = EffectType.SUMMON_UNIT,
                TargetPlayer = PlayerTarget.OWNER,
                CardNumber = 1,
                LaneTargets = CardTargets.ALL_LANES
            };
            dict.Add(InteractionType.WHEN_PLAYED, new List<Effect>() { effect });

            Skill skill = new Skill()
            {
                EntityPlayInfo = new EntityPlayInfo()
                {
                    EntityType = EntityType.SKILL,
                    TargetOptions = CardTargets.BOARD
                },
                EntityPrintInfo = new EntityPrintInfo()
                {
                    Id = 1,
                    Title = "RUSH",
                    Cost = "5",
                    Expansion = ExpansionId.VANILLA,
                    ClassType = PlayerClassType.BASE
                },
                Interactions = dict
            };

            // Serialize to JSON string
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented, // For pretty-printing with indents
                Converters = { new StringEnumConverter() } // To serialize enums as strings
            };
            string jsonOutput = JsonConvert.SerializeObject(skill, settings);
            Console.WriteLine(jsonOutput);
        }
    }
}
