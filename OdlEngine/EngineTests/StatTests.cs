using Newtonsoft.Json;
using ODLGameEngine;

namespace EngineTests
{
    [TestClass]
    public class StatTest
    {
        [TestMethod]
        public void StatDeserializeTest()
        {
            Random _rng = new Random();
            // Create 3 random numbers
            int _v1 = _rng.Next(1, 100);
            int _v2 = _rng.Next(1, 100);
            int _v3 = _rng.Next(1, 100);
            string _singleIntJson = $"{_v1}";
            string _fullObjectJson = $"{{\"BaseValue\": {_v2}, \"Modifier\": {_v3}}}";

            Stat _stat1 = JsonConvert.DeserializeObject<Stat>(_singleIntJson);
            Stat _stat2 = JsonConvert.DeserializeObject<Stat>(_fullObjectJson);

            Assert.AreEqual(_stat1.BaseValue, _v1);
            Assert.AreEqual(_stat1.Modifier, 0);
            Assert.AreEqual(_stat2.BaseValue, _v2);
            Assert.AreEqual(_stat2.Modifier, _v3);
        }
        [TestMethod]
        public void StatHashTest()
        {
            Random _rng = new Random();
            // Create 3 random numbers
            int v1 = _rng.Next(1, 100);
            int v2 = _rng.Next(1, 100);
            Stat _stat = new Stat
            {
                BaseValue = v1,
                Modifier = v2
            };
            int _statHash = _stat.GetHashCode(); // Gets Hash
            // Is hash dependent on base value only?
            _stat.BaseValue++;
            Assert.AreNotEqual(_statHash, _stat.GetHashCode());
            // How about modifier?
            _stat.BaseValue--;
            _stat.Modifier++;
            Assert.AreNotEqual(_statHash, _stat.GetHashCode());
            // Is it deterministic and reversible?
            _stat.Modifier--;
            Assert.AreEqual(_statHash, _stat.GetHashCode());
        }
        [TestMethod]
        public void Min1StatCheck()
        {
            // Create stat, add modifiers, verify the min possible total is 1 with multiple actions
            Min1Stat _stat = new Min1Stat
            {
                BaseValue = 10,
                Modifier = -5
            };
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -5);
            Assert.AreEqual(_stat.Total, 5);
            _stat.Modifier = -9;
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -9);
            Assert.AreEqual(_stat.Total, 1);
            _stat.Modifier = -10; // Wouldnt be possible
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -9);
            Assert.AreEqual(_stat.Total, 1);
            _stat.Modifier = -11; // Wouldnt be possible
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -9);
            Assert.AreEqual(_stat.Total, 1);
        }
        [TestMethod]
        public void Min0StatCheck()
        {
            // Create stat, add modifiers, verify the min possible total is 1 with multiple actions
            Min0Stat _stat = new Min0Stat
            {
                BaseValue = 10,
                Modifier = -5
            };
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -5);
            Assert.AreEqual(_stat.Total, 5);
            _stat.Modifier = -9;
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -9);
            Assert.AreEqual(_stat.Total, 1);
            _stat.Modifier = -10;
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -10);
            Assert.AreEqual(_stat.Total, 0);
            _stat.Modifier = -11; // Wouldnt be possible
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -10);
            Assert.AreEqual(_stat.Total, 0);
        }
        [TestMethod]
        public void StatCloningConservesProperty()
        {
            // Create stat, add modifiers, verify the min possible total is 1 with multiple actions
            Min1Stat _stat = new Min1Stat
            {
                BaseValue = 10,
                Modifier = -9
            };
            // Stat has a min of 1 so this is a sanity check
            _stat.Modifier = -10;
            Assert.AreEqual(_stat.BaseValue, 10);
            Assert.AreEqual(_stat.Modifier, -9);
            Assert.AreEqual(_stat.Total, 1);
            // Clone
            Min1Stat _clonedStat = (Min1Stat)_stat.Clone();
            // Does it conserve property?
            _clonedStat.Modifier = -10;
            Assert.AreEqual(_clonedStat.BaseValue, 10);
            Assert.AreEqual(_clonedStat.Modifier, -9);
            Assert.AreEqual(_clonedStat.Total, 1);
        }
        [TestMethod]
        public void StatInEntityDeserializing()
        {
            // Deserializing stat but in entity to make sure all's good (try HP)
            Random _rng = new Random();
            // Create 3 random numbers
            int _v1 = _rng.Next(1, 100);
            int _v2 = _rng.Next(1, 100);
            int _v3 = _rng.Next(1, 100);
            string _singleStatJson = $"{_v1}";
            string _fullStatJson = $"{{\"BaseValue\": {_v2}, \"Modifier\": {_v3}}}";
            _singleStatJson = "{\"Name\": \"TEST\", \"Hp\": " + _singleStatJson + "}";
            _fullStatJson = "{\"Name\": \"TEST\", \"Hp\": " + _fullStatJson + "}";

            LivingEntity _entity1 = JsonConvert.DeserializeObject<LivingEntity>(_singleStatJson);
            Assert.AreEqual(_entity1.Name, "TEST");
            Assert.AreEqual(_entity1.Hp.Total, _v1);
            Assert.AreEqual(_entity1.Hp.BaseValue, _v1);
            Assert.AreEqual(_entity1.Hp.Modifier, 0);

            LivingEntity _entity2 = JsonConvert.DeserializeObject<LivingEntity>(_fullStatJson);
            Assert.AreEqual(_entity2.Name, "TEST");
            Assert.AreEqual(_entity2.Hp.Total, _v2 + _v3);
            Assert.AreEqual(_entity2.Hp.BaseValue, _v2);
            Assert.AreEqual(_entity2.Hp.Modifier, _v3);
        }
    }
}
