using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.SCSaveManager
{
    public interface ISaveManager
    {
        bool IsAvailable { get; }

        string SaveKey { get; }

        bool IsAvaiableForThisSession(StoryGameSession session);

        void Save(bool isDied, bool isQuit);

        void Init(StoryGameSession newSession);

        void RainCycleTick(RainWorldGame game);

        void Clear();
    }

    public abstract class SaveManager<TSelf, TData> : ISaveManager where TSelf : SaveManager<TSelf, TData>, new() where TData : new()
    {

        public static Lazy<TSelf> _instance = new Lazy<TSelf>(() => new TSelf());

        public static TSelf Instance => _instance.Value;

        public static TData Data => Instance.SaveData;

        public abstract TData SaveData { get;}

        public abstract bool IsAvailable { get; }

        public abstract string SaveKey { get; }

        public abstract void Init(StoryGameSession newSession);

        public abstract bool IsAvaiableForThisSession(StoryGameSession session);

        public abstract void Save(bool isDied, bool isQuit);

        public virtual void RainCycleTick(RainWorldGame game) { }

        public abstract void Clear();
    }

    public abstract class DeathPersistentSaveManager<TSelf, TData> : SaveManager<TSelf, TData> where TSelf : DeathPersistentSaveManager<TSelf, TData>, new()
        where TData : ISaveData, new()
    {
        protected TData _data = default;

        protected TData _oldData = default;

        protected bool _isAvaiable = false;

        protected StoryGameSession _session = null;

        public override TData SaveData => IsAvailable ? _data : throw new InvalidOperationException($"Save data:{SaveKey} is not avaiable");

        public override bool IsAvailable => _isAvaiable;


        public override void Init(StoryGameSession newSession)
        {
            if (!(_isAvaiable = IsAvaiableForThisSession(newSession)))
            {
                Clear();
                return;
            }

            var slugbase = newSession.saveState.deathPersistentSaveData.GetSlugBaseData();
            _oldData = slugbase.ForceGet<TData>(SaveKey);
            if (_oldData.DeepClone() is not TData data)
            {
                throw new InvalidCastException($"Failed to cast saved data to type {typeof(TData).FullName} for SaveKey {SaveKey}");
            }
            _data = data;
            _session = newSession;
        }


        public override sealed void Save(bool isDied, bool isQuit)
        {
          
            if (!IsAvailable || _session is null) return;

            //针对饥饿情况同时两种保存状态的特殊处理
            var data = (TData)_data.DeepClone();
            var slugbase = _session.saveState.deathPersistentSaveData.GetSlugBaseData();
            SaveToData(isDied, isQuit);
            slugbase.Set(SaveKey, _data);
            _data = data;
        }

        public override void Clear()
        {
            _session = null;
            _data = default;
            _oldData = default;
        }

        protected abstract void SaveToData(bool isDied, bool isQuit);
    }

    public abstract class MiscWorldSaveManager<TSelf, TData> : SaveManager<TSelf, TData> where TSelf : MiscWorldSaveManager<TSelf, TData>, new()
       where TData : new()
    {
        protected TData _data = default;

        protected bool _isAvaiable = false;

        protected StoryGameSession _session = null;

        public override TData SaveData => IsAvailable ? _data : throw new InvalidOperationException($"Save data:{SaveKey} is not avaiable");

        public override bool IsAvailable => _isAvaiable;

        public override void Init(StoryGameSession newSession)
        {
            if (!(_isAvaiable = IsAvaiableForThisSession(newSession)))
            {
                Clear();
                return;
            }
        

            var slugbase = newSession.saveState.miscWorldSaveData.GetSlugBaseData();
            _data = slugbase.ForceGet<TData>(SaveKey);

            _session = newSession;
        }

        public override sealed void Save(bool isDied, bool isQuit)
        {
            if (!IsAvailable || _session is null) return;
            var slugbase = _session.saveState.miscWorldSaveData.GetSlugBaseData();
            SaveToData();
            slugbase.Set(SaveKey, _data);
        }

        public override void Clear()
        {
            _session = null;
            _data = default;
        }

        protected abstract void SaveToData();
    }
}
