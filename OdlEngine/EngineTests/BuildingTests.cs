using ODLGameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineTests
{
    [TestClass]
    public class BuildingTests
    {
        // Blueprint targetability
        /* - Given a BP, playability for empty lane (unplayable, verif)
		- Playability OK for first tile
		- Playability OK if building already present
		- Playability OK, building only in 1, unit advances and then becomes ok
		- Playability OK by skipping 1st one and allowing in 2nd*/
        [TestMethod]
        public void HashTest()
        {
            Building b1, b2;
            b1 = new Building()
            {
                UniqueId = 1,
                Owner = 0,
                LaneCoordinate = LaneID.PLAINS,
                TileCoordinate = 2,
                Hp = 10
            };
            b2 = (Building)b1.Clone();
            Assert.AreEqual(b1.GetGameStateHash(), b2.GetGameStateHash());
            // Now change a few things
            b2.Hp = 1;
            Assert.AreNotEqual(b1.GetGameStateHash(), b2.GetGameStateHash());
            // Revert
            b2.Hp = b1.Hp;
            Assert.AreEqual(b1.GetGameStateHash(), b2.GetGameStateHash());
        }
    }
}
