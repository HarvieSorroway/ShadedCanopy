using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

namespace ShadedCanopy.Creatures.Scavengers
{
    public partial class SCScavExtra
    {
        static ConditionalWeakTable<Scavenger, SCScavExtra> _scavengerTable = new ConditionalWeakTable<Scavenger, SCScavExtra>();



        public static SCScavExtra TryGetSCScav(Scavenger scavenger, bool addIfMissing = false)
        {
            if(_scavengerTable.TryGetValue(scavenger ,out var res))
                return res;

            if (addIfMissing)
            {
                res = new SCScavExtra();
                _scavengerTable.Add(scavenger, res);
            }
            else
                res = null;
            return res;
        }
    }

    public partial class SCScavExtra
    {
        public float decorationColoredHands;

        public SCScavExtra()
        {
            
        }

        public void InitGraphicsIndividualParam()
        {
            if (Random.value < 0.2f)
                decorationColoredHands = Random.value * 0.5f + 0.3f;
        }
    }
}
