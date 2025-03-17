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
            return new TestCardGenerator(".\\..\\..\\..\\..\\..\\CardDatabase");
        }
        public override EntityBase GetCard(int id)
        {
            if (id >= 0) // Normal card then
            {
                return base.GetCard(id);
            }
            id *= -1; // Remove negative sign
            if(id >= 100 && id <= 199) // Attempting to generate brick skill (does nothing but can be played) Brick: 1-G-TGT where TGT 0-7 or invalid
            {
                EntityPrintInfo printInfo = new EntityPrintInfo()
                {
                    Id = -id,
                    Title = "BRICK",
                    Cost = ((id / 10) % 10).ToString() // Second digit is gold, 0-9
                };
                EntityPlayInfo playInfo = new EntityPlayInfo()
                {
                    EntityType = EntityType.SKILL,
                    TargetOptions = ((id % 10) <= 7) ? (CardTargets)(id % 10) : CardTargets.INVALID,
                };
                return new Skill() // Returns "brick" card
                {
                    EntityPlayInfo = playInfo,
                    EntityPrintInfo = printInfo
                };
            }
            if(id >= 1000000 && id < 1999999)
            {
                EntityPrintInfo printInfo = new EntityPrintInfo()
                {
                    Id = -id,
                    Title = "TOKEN",
                    Cost = ((id / 100000) % 10).ToString() // First digit is gold, 0-9
                };
                EntityPlayInfo playInfo = new EntityPlayInfo()
                {
                    EntityType = EntityType.UNIT,
                    TargetOptions = ((id % 10) <= 7) ? (CardTargets)(id % 10) : CardTargets.INVALID,
                };
                return new Unit() // Returns "TOKEN" card
                {
                    EntityPlayInfo = playInfo,
                    EntityPrintInfo = printInfo,
                    Hp = (id / 10000) % 10, // Second digit is hp, 0-9
                    Attack = (id / 1000) % 10, // Third, attack
                    Movement = (id / 100) % 10, // 4th, movement
                    MovementDenominator = (id / 10) % 10, // 5th, mov denominator
                };
            }
            if (id >= 1000000000 && id < 1999999999)
            {
                EntityPrintInfo printInfo = new EntityPrintInfo()
                {
                    Id = -id,
                    Title = "TOKEN_BUILDING",
                    Cost = ((id / 100000000) % 10).ToString() // First digit is gold, 0-9
                };
                EntityPlayInfo playInfo = new EntityPlayInfo()
                {
                    EntityType = EntityType.BUILDING,
                    TargetOptions = ((id % 10) <= 7) ? (CardTargets)(id % 10) : CardTargets.INVALID,
                    TargetConditions = TargetCondition.BLUEPRINT,
                };
                int bpRaw = (id / 100) % 1000000;
                List<int> plainsBp = new List<int>();
                List<int> forestBp = new List<int>();
                List<int> mountainBp = new List<int>();
                for (int i = 0; i<18;i++) // Decode the 18-bit of BP
                {
                    bool bitActive = (bpRaw &= 1 << i) != 0; // get bit
                    if(bitActive)
                    {
                        if (i < GameConstants.PLAINS_TILES_NUMBER) // parsing palins
                        {
                            plainsBp.Add(i);
                        }
                        else if (i < GameConstants.PLAINS_TILES_NUMBER + GameConstants.FOREST_TILES_NUMBER) // parsing forest
                        {
                            forestBp.Add(i - GameConstants.PLAINS_TILES_NUMBER);
                        }
                        else if (i < GameConstants.PLAINS_TILES_NUMBER + GameConstants.FOREST_TILES_NUMBER + GameConstants.MOUNTAIN_TILES_NUMBER) // Parsing mountains
                        {
                            mountainBp.Add(i - GameConstants.PLAINS_TILES_NUMBER - GameConstants.FOREST_TILES_NUMBER);
                        }
                        else { throw new Exception("Messed up bitpacking of BP!"); }
                    }
                }
                return new Building() // Returns "TOKEN_BUILDING" card
                {
                    EntityPlayInfo = playInfo,
                    EntityPrintInfo = printInfo,
                    Hp = (id / 10000000) % 10, // Second digit is hp, 0-9
                    PlainsBp = plainsBp.ToArray(),
                    ForestBp = forestBp.ToArray(),
                    MountainBp = mountainBp.ToArray(),
                };
            }
            throw new NotImplementedException("Not implemented yet");
        }
    }
}
