using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    /// <summary>
    /// The TestCardGenerator returns as normal, but if ID is negative, instead will generate a card depending on the code
    /// </summary>
    internal class TestCardGenerator : CardFinder
    {
        public TestCardGenerator(string baseDir) : base(baseDir)
        {
        }
        /// <summary>
        /// Creates a new instanxce of stast generator for well... testing. I assume path doesn't change otherwise just call constructor manually...
        /// </summary>
        /// <returns>Test generator</returns>
        public static TestCardGenerator GenerateTestCardGenerator()
        {
            return new TestCardGenerator(".\\..\\..\\..\\..\\CardDatabase");
        }
        public override Card GetCardData(int id)
        {
            if (id >= 0) // Normal card then
            {
                return base.GetCardData(id);
            }
            id *= -1; // Remove negative sign
            if(id >= 100 && id <= 199) // Attempting to generate brick skill (does nothing but can be played) Brick: 1-G-TGT where TGT 0-7 or invalid
            {
                return new Card() // Returns "brick" card
                {
                    Id = -id,
                    Name = "BRICK",
                    CardType = CardType.SKILL,
                    TargetOptions = ((id % 10)<=7)? (CardTargets)(id % 10) : CardTargets.INVALID,
                    Cost = ((id/10)%10).ToString() // Second digit is gold, 0-9
                };
            }
            throw new NotImplementedException("Not implemented yet");
        }
        public override Skill GetSkillData(int id)
        {
            if (id >= 0) // Normal card then
            {
                return base.GetSkillData(id);
            }
            if (id >= 100 && id <= 199) // Attempting to generate brick skill (does nothing but can be played)
            {
                return new Skill(); // No effect no nothing
            }
            throw new NotImplementedException("Not implemented yet");
        }
        public override Unit GetUnitData(int id)
        {
            if (id >= 0) // Normal card then
            {
                return base.GetUnitData(id);
            }
            throw new NotImplementedException("Not implemented yet");
        }
        public override Building GetBuildingData(int id)
        {
            if (id >= 0) // Normal card then
            {
                return base.GetBuildingData(id);
            }
            throw new NotImplementedException("Not implemented yet");
        }
    }
}
