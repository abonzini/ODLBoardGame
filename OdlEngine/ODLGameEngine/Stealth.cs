using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public enum CorrelationType
    {
        NONE,
        MUTUALLY_EXCLUSIVE, // If A then not B
        CORRELATED // If A then B
    }
    public class HiddenCorrelation
    {
        public CorrelationType corrType;
        public int corrEntityId;
    }
}
