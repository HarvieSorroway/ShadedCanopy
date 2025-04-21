using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

namespace SCUtils.CreatureUtils
{
    public static partial class CreatureStateProxy
    {
        /// <summary>
        /// 自动反序列化<paramref name="state"/>中拥有<see cref="CreatureStateProxy.AutoProxyField"/>属性的字段，需要和<see cref="CreatureStateProxy.ExrtraValToString{T}(T)"/>配合使用。
        /// </summary>
        public static void FromString<T>(T state, string[] s)
        {
            var t = typeof(T);
            TryInitForStateType(typeof(T));
            for (int i = 0; i < s.Length; i++)
            {
                var splited = Regex.Split(s[i], "<cC>");
                foreach (var data in fieldToProxy[t])
                {
                    if(data.name == splited[0])
                    {
                        data.field.SetValue(state, JsonConvert.DeserializeObject(splited[1], data.fieldType));
                    }
                }
            }
        }

        /// <summary>
        /// 自动序列化<paramref name="state"/>中拥有<see cref="CreatureStateProxy.AutoProxyField"/>属性的字段，需要和<see cref="CreatureStateProxy.FromString{T}(T, string[])"/>配合使用。
        /// </summary>
        public static string ExrtraValToString<T>(T state)
        {
            var t = typeof(T);
            StringBuilder stringBuilder = new StringBuilder();
            TryInitForStateType(t);
            foreach(var data in fieldToProxy[t])
            {
                stringBuilder.Append($"<cB>{data.name}<cC>{JsonConvert.SerializeObject(data.field.GetValue(state), data.fieldType, null)}");
            }
            return stringBuilder.ToString();
        }
    }

    public static partial class CreatureStateProxy
    {
        static Dictionary<Type, List<ProxiedFieldData>> fieldToProxy = new Dictionary<Type, List<ProxiedFieldData>>();

        internal static void TryInitForStateType(Type type)
        {
            if (fieldToProxy.ContainsKey(type))
                return;

            var proxyFields = new List<ProxiedFieldData>();
            fieldToProxy.Add(type, proxyFields);

            foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<AutoProxyField>() != null)
                {
                    proxyFields.Add(new ProxiedFieldData(field.Name.ToUpperInvariant(), field, field.FieldType));
                }
            }
        }

        internal class ProxiedFieldData
        {
            public string name;
            public FieldInfo field;
            public Type fieldType;

            public ProxiedFieldData(string name, FieldInfo field, Type fieldType)
            {
                this.name = name;
                this.field = field;
                this.fieldType = fieldType;
            }
        }

        public class AutoProxyField : Attribute { }
    }
}
