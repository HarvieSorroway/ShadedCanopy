using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static SCUtils.SCDevTools.NodeTreeManager.NodeTreeTypeInfo;

namespace SCUtils.SCDevTools.NodeTreeManager
{
    internal static class NodeTreeTypeInfo
    {
        static List<TypeInfo> typeInfos = new List<TypeInfo>();
        static Dictionary<Type, TypeInfo> typeInfoMap = new Dictionary<Type, TypeInfo>();
        static List<TypeReferenceInfo> typeReferenceInfos = new List<TypeReferenceInfo>();

        public static void BuildTypeInfos()
        {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var tp in assembly.SafeGetTypes())//创建typeinfo
                {
                  
                    if (!IsTrackType(tp))
                        continue;
                    SCUtils.Log($"BuildTypeInfos - Checking type {tp.Name} - {IsTrackType(tp)}");
                    var typeInfo = new TypeInfo();
                    typeInfo.type = tp;
                    typeInfo.inspectInfos = tp.GetCustomAttribute<SCDevToolsInspectType>();

                    SCDevNodeTreeManager.rootNode.GetChild(typeInfo.inspectInfos.branchPatch, true);

                    foreach (var field in tp.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (field.GetCustomAttribute<SCDevToolsInspectValue>() != null)
                        {
                            typeInfo.valueFields.Add(field);
                            var specialFieldAttr = field.GetCustomAttribute<SCDevToolsSpecialFieldType>();
                            if (specialFieldAttr != null)
                            {
                                typeInfo.specialFieldTypes.Add(field, specialFieldAttr);
                            }
                        }
                    }
                    typeInfos.Add(typeInfo);
                    typeInfoMap.Add(tp, typeInfo);
                }
            }
            

            foreach(var typeInfo in typeInfos)//创建typereferenceinfo
            {
                SCUtils.Log($"BuildTypeInfos - Processing type {typeInfo.type.Name}");
                foreach (var field in typeInfo.type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    
                    var fieldType = field.FieldType;
                    SCUtils.Log($"   check field {fieldType}-{field.Name}, {IsTrackType(fieldType)}");
                    if (IsTrackType(fieldType))
                    {
                        var typeRefInfo = typeReferenceInfos.Find(ti => ti.IsTypeBelongs(typeInfo.type, fieldType));
                        if (typeRefInfo == null)
                        {
                            typeRefInfo = new TypeReferenceInfo(typeInfo.type, fieldType);
                            typeReferenceInfos.Add(typeRefInfo);
                            SCUtils.Log($"   new TypeReferenceInfo between A:{typeInfo.type}-B:{fieldType}, A2B:{typeRefInfo.A2BRef}-B2A:{typeRefInfo.B2ARef}");
                        }
                        typeRefInfo.SetRef(typeInfo.type, fieldType, field);
                    }
                }
            }
        }

        static bool IsTrackType(Type type)
        {
            return type.GetCustomAttribute<SCDevToolsInspectType>() != null;
                 
        }

        internal static TypeInfo GetTypeInfo(Type type)
        {
            if (typeInfoMap.TryGetValue(type, out var typeInfo))
                return typeInfo;
            return null;
        }

        internal static IEnumerable<TypeReferenceInfo> GetReferenceInfo(Type type)
        {
            foreach(var typeRefInfo in typeReferenceInfos)
            {
                if(typeRefInfo.IsTypeBelongs(type))
                    yield return typeRefInfo;
            }
        }

        internal static IEnumerable<TypeInfo> GetAllTypeInfos()
        {
            foreach(var typeInfo in typeInfos)
            {
                yield return typeInfo;
            }
        }

        internal class TypeReferenceInfo
        {
            internal Type typeA, typeB;
            internal FieldInfo A2BRef, B2ARef;

            internal TypeReferenceInfo(Type a, Type b)
            {
                typeA = a;
                typeB = b;
            }

            internal bool IsTypeBelongs(Type a, Type b)
            {
                return (typeA == a && typeB == b) || (typeA == b && typeB == a);
            }

            internal bool IsTypeBelongs(Type type)
            {
                return typeA == type || typeB == type;
            }

            internal void SetRef(Type type, Type fieldType, FieldInfo field)
            {
                if (type == typeA && fieldType == typeB)
                    A2BRef = field;
                else if (type == typeB && fieldType == typeA)
                    B2ARef = field;
            }

            internal Type GetReferencedType(Type type)
            {
                if (type == typeA)
                    return typeB;
                else if (type == typeB)
                    return typeA;
                return null;
            }

            internal bool IsObjsReferenced(object A, object B)
            {
                var tpA = A.GetType();
                var tpB = B.GetType();

                if(!IsTypeBelongs(tpA, tpB)) return false;

                if (typeA != tpA)//确保正确的比较顺序
                {
                    var tmp = A;
                    A = B;
                    B = tmp;
                }
                if (A2BRef != null && A2BRef.GetValue(A) == B)
                    return true;
                if (B2ARef != null && B2ARef.GetValue(B) == A)
                    return true;
                return false;
            }
        }

        internal class TypeInfo
        {
            internal Type type;
            internal SCDevToolsInspectType inspectInfos;

            internal List<FieldInfo> valueFields = new List<FieldInfo>();
            internal Dictionary<FieldInfo, SCDevToolsSpecialFieldType> specialFieldTypes = new Dictionary<FieldInfo, SCDevToolsSpecialFieldType>();
        }
    }
}
