using ODLGameEngine;

namespace EngineTests
{
    /// <summary>
    /// The TestCardGenerator returns as normal, but if ID is negative, instead will generate a card depending on the code
    /// </summary>
    static class TestCardGenerator
    {
        /// <summary>
        /// Creates a blank spell
        /// </summary>
        /// <param name="id">Id#</param>
        /// <param name="name">Name</param>
        /// <param name="cost">Cost</param>
        /// <param name="target">Targets</param>
        /// <returns></returns>
        public static Skill CreateSkill(int id, int cost, HashSet<int> targetOptions, CardTargetingType skillTarget)
        {
            return new Skill() // Returns "brick" card
            {
                Id = id,
                Cost = cost,
                EntityType = EntityType.SKILL,
                TargetOptions = targetOptions,
                TargetType = skillTarget
            };
        }
        /// <summary>
        /// Creates a basic unit
        /// </summary>
        /// <param name="id">1</param>
        /// <param name="name">Name</param>
        /// <param name="cost">Cost</param>
        /// <param name="target">Targets</param>
        /// <param name="hp">Hp</param>
        /// <param name="attack">Attack</param>
        /// <param name="movement">Movement</param>
        /// <param name="denominator">Movement Denominator</param>
        /// <returns></returns>
        public static Unit CreateUnit(int id, string name, int cost, HashSet<int> target, int hp, int attack, int movement, int denominator)
        {
            Unit unit = new Unit()
            {
                Id = id,
                Name = name,
                EntityType = EntityType.UNIT,
                TargetOptions = target,
                Cost = cost,
            };
            unit.Hp.BaseValue = hp;
            unit.Attack.BaseValue = attack;
            unit.Movement.BaseValue = movement;
            unit.MovementDenominator.BaseValue = denominator;
            return unit;
        }
        /// <summary>
        /// Creates basic building
        /// </summary>
        /// <param name="id">Id#</param>
        /// <param name="name">Name</param>
        /// <param name="cost">Cost</param>
        /// <param name="target">Target</param>
        /// <param name="hp">Hp</param>
        /// <param name="plainBp">Array with plain Bp options</param>
        /// <param name="forestBp">Array with forest Bp options</param>
        /// <param name="mountainBp">Array with mountain Bp options</param>
        /// <returns></returns>
        public static Building CreateBuilding(int id, string name, int cost, HashSet<int> target, int hp)
        {
            Building building = new Building() // Returns "TOKEN_BUILDING" card
            {
                Id = id,
                Name = name,
                Cost = cost,
                EntityType = EntityType.BUILDING,
                TargetOptions = target
            };
            building.Hp.BaseValue = hp;
            return building;
        }
    }
}
