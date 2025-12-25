using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.SCSaveManager
{
    public interface ISaveData
    {
        public ISaveData DeepClone();
    }

    public abstract class JsonSaveData<TSelf> : ISaveData where TSelf : JsonSaveData<TSelf>
    {
        public ISaveData DeepClone()
        {
            return JsonConvert.DeserializeObject<TSelf>(JsonConvert.SerializeObject(this)); //一个使用Json序列化的低效率拷贝，适用于少量数据
        }
    }

}
