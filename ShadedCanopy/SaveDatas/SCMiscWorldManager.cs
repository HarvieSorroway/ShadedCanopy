using SCUtils.SCSaveManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.SaveDatas
{
    internal record SCMiscWorldData : JsonSaveData<SCMiscWorldData>
    {

    }

    internal class SCMiscWorldManager : MiscWorldSaveManager<SCMiscWorldManager, SCMiscWorldData>
    {
        public override string SaveKey => "ShadedCanopy.Shimmer.Misc";

        public override bool IsAvaiableForThisSession(StoryGameSession session) => session.characterStats.name == SCEnums.SlugStateName.Shimmer;

        protected override void SaveToData()
        {
        }
    }
}
