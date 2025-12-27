
using Newtonsoft.Json;
using SCUtils.SCSaveManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Iterators.MechIterator
{
    public record MechIteratorSaveData : JsonSaveData<MechIteratorSaveData> //回头手写il生成
    {
        public int meets;                      //见面次数
        public bool hasPermission;             //是否被迭代器给予许可
        public bool shelterUnlocked;           //是否解锁避难所
        public bool foodPrintUnlocked;         //是否解锁每循环食物打印

        public List<DataPearl.AbstractDataPearl> stashPearls = new();  //暂存的珍珠

        public string playerNamePrefix = "User#1BF52";
        public bool playerNameChangNeedCheck;

        [JsonIgnore]
        public bool meetThisCycle;              //本周期是否见过面

        public void SetPlayerName(string newName)
        {
            playerNamePrefix = newName;
            playerNameChangNeedCheck = true;
        }
    }


    internal class MechIteratorMiscSaveManager : MiscWorldSaveManager<MechIteratorMiscSaveManager, MechIteratorSaveData>
    {
        public override string SaveKey => "ShadedCanopy.MechIterator.Misc";

        public override bool IsAvaiableForThisSession(StoryGameSession session) => session.characterStats.name == SCEnums.SlugStateName.Shimmer;


        public override void RainCycleTick(RainWorldGame game)
        {
            if (Data.meetThisCycle)
                Data.meets++;
        }

        protected override void SaveToData()
        {
            //不用写这一块
        }
    }
}
