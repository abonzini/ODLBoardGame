using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardTooltipGenerator
{
    public class CardTooltip
    {
        public int CardId;
        public bool HasBlueprint;
        public List<string> Keywords;
        public List<int> RelatedCards;
    }
}
