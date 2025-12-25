
using Newtonsoft.Json;
using SCUtils.SCSaveManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.SaveDatas
{
    public record SCDeathPersistentData : JsonSaveData<SCDeathPersistentData> //回头手写il生成
    {
        public int meets;                      //见面次数
        public bool hasPermission;             //是否被迭代器给予许可
        public bool shelterUnlocked;           //是否解锁避难所
        public bool foodPrintUnlocked;         //是否解锁每循环食物打印

        public List<DataPearl.AbstractDataPearl> stashPearls = new();  //暂存的珍珠

        [JsonIgnore]
        public bool meetThisCycle;              //本周期是否见过面
    }


    internal class SCDeathPersistentManager : DeathPersistentSaveManager<SCDeathPersistentManager, SCDeathPersistentData>
    {
        public override string SaveKey => "ShadedCanopy.Shimmer.Death";

        public override bool IsAvaiableForThisSession(StoryGameSession session) => session.characterStats.name == SCEnums.SlugStateName.Shimmer;

        protected override void SaveToData(bool isDied, bool isQuit)
        {
            if(isDied || isQuit)
                _data = _oldData;
        }

        public override void RainCycleTick(RainWorldGame game)
        {
            base.RainCycleTick(game);
        }
    }
}
