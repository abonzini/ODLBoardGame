using Microsoft.VisualBasic;
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
        public static Skill CreateSkill(int id, string name, int cost, CardTargets target)
        {
            EntityPrintInfo printInfo = new EntityPrintInfo()
            {
                Id = id,
                Title = name,
                Cost = cost.ToString()
            };
            EntityPlayInfo playInfo = new EntityPlayInfo()
            {
                EntityType = EntityType.SKILL,
                TargetOptions = target,
            };
            return new Skill() // Returns "brick" card
            {
                EntityPlayInfo = playInfo,
                EntityPrintInfo = printInfo
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
        public static Unit CreateUnit(int id, string name, int cost, CardTargets target, int hp, int attack, int movement, int denominator)
        {
            EntityPrintInfo printInfo = new EntityPrintInfo()
            {
                Id = id,
                Title = name,
                Cost = cost.ToString(),
            };
            EntityPlayInfo playInfo = new EntityPlayInfo()
            {
                EntityType = EntityType.UNIT,
                TargetOptions = target
            };
            return new Unit()
            {
                EntityPlayInfo = playInfo,
                EntityPrintInfo = printInfo,
                Hp = hp,
                Attack = attack,
                Movement = movement,
                MovementDenominator = denominator
            };
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
        public static Building CreateBuilding(int id, string name, int cost, CardTargets target, int hp, int[] plainBp, int[] forestBp, int[] mountainBp)
        {
            EntityPrintInfo printInfo = new EntityPrintInfo()
            {
                Id = id,
                Title = name,
                Cost = cost.ToString()
            };
            EntityPlayInfo playInfo = new EntityPlayInfo()
            {
                EntityType = EntityType.BUILDING,
                TargetOptions = target,
                TargetConditions = TargetCondition.BLUEPRINT,
            };
            return new Building() // Returns "TOKEN_BUILDING" card
            {
                EntityPlayInfo = playInfo,
                EntityPrintInfo = printInfo,
                Hp = hp,
                PlainsBp = plainBp,
                ForestBp = forestBp,
                MountainBp = mountainBp,
            };
        }
    }
}
