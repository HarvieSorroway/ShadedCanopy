using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager;

namespace SCUtils.SCDevTools.NodeTreeManager
{
    internal static class SCDevNodeTreeManager
    {
        internal static Dictionary<TreeNode, List<VirtualObj>> virtualObjMap = new Dictionary<TreeNode, List<VirtualObj>>();
        internal static TreeNode rootNode = new TreeNode("Root");

        /// <summary>
        /// 对象弱引用反向映射到虚拟对象，用于通过弱引用找到对应的虚拟对象，以及比较多个追踪类型是否属于同一个虚拟对象
        /// </summary>
        internal static ConditionalWeakTable<WeakHandle, VirtualObj> weakRef2VirtualObjMap = new ConditionalWeakTable<WeakHandle, VirtualObj>();

        internal static Dictionary<Type, List<WeakHandle>> type2WeakRefsMap = new Dictionary<Type, List<WeakHandle>>();

        public static void Init()
        {
            foreach(var typeInfo in NodeTreeTypeInfo.GetAllTypeInfos())
            {
                type2WeakRefsMap.Add(typeInfo.type, new List<WeakHandle>());
            }
            SCDevNodeTreeManager.rootNode.GetChild("Root.RainWorld.Game.World.Room", true);
        }

        public static void Track(object obj)
        {
            var type = obj.GetType();
            var attr = type.GetCustomAttribute<SCDevToolsInspectType>();

            if (attr == null)
                return;

            var handle = new WeakHandle(obj);
            SetVirtualObjForObj(type, obj, handle, attr);
            SCUtils.Log(" ");

            type2WeakRefsMap[type].Add(handle);
        }

        /// <summary>
        /// 根据现有的对象和类型信息，建立正确的虚拟对象，如果没有则创建一个新的虚拟对象
        /// </summary>
        static VirtualObj SetVirtualObjForObj(Type type, object obj, WeakHandle objHandle, SCDevToolsInspectType attr)
        {
            SCUtils.Log($"[SCUtils] SetVirtualObjForObj - {type.Name}, {obj.GetHashCode()}");
            foreach(var typeRefInfo in NodeTreeTypeInfo.GetReferenceInfo(type))
            {
                var refedType = typeRefInfo.GetReferencedType(type);
                SCUtils.Log($"[SCUtils] referenced type - {refedType.Name}");

                if (type2WeakRefsMap.TryGetValue(refedType, out var weakRefs))
                {
                    for(int i = weakRefs.Count - 1; i >= 0; i--)
                    {
                        if (!weakRefs[i].IsAlive)
                        {
                            weakRefs.RemoveAt(i);
                            continue;
                        }

                        SCUtils.Log($"[SCUtils]   referenced obj - {weakRefs[i].Target.GetHashCode()} - {typeRefInfo.IsObjsReferenced(obj, weakRefs[i].Target)}");
                        if (typeRefInfo.IsObjsReferenced(obj, weakRefs[i].Target))
                        {
                            if (weakRef2VirtualObjMap.TryGetValue(weakRefs[i], out var vObj))
                            {
                                weakRef2VirtualObjMap.Add(objHandle, vObj);
                                vObj.refs.Add(objHandle);
                                return vObj;
                            }
                        }
                    }
                }
            }

            var newObj = new VirtualObj()
            {
                name = attr.leafName,
                refs = new List<WeakHandle>() { objHandle }
            };

            weakRef2VirtualObjMap.Add(objHandle, newObj);
            rootNode.GetChild(attr.branchPatch, true).virtualObjs.Add(newObj);

            return newObj;
        }


        public static void DrawVirtualObj(VirtualObj virtualObj)
        {
            foreach(var weakRef in virtualObj.refs)
            {
                if (!weakRef.IsAlive)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "[Dead Reference]");
                    continue;
                }

                
                var typeInfo = NodeTreeTypeInfo.GetTypeInfo(weakRef.Target.GetType());
                var obj = weakRef.Target;

                ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0, 1), $"---{typeInfo.type.Name}---");

                foreach (var field in typeInfo.valueFields)
                {
                    if (field.FieldType == typeof(string))
                    {
                        string val = field.GetValue(obj) as string;
                        if (ImGui.InputText(field.Name, ref val, 256))
                        {
                            field.SetValue(obj, val);
                        }
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        int val = (int)field.GetValue(obj);
                        if (ImGui.InputInt(field.Name, ref val))
                        {
                            field.SetValue(obj, val);
                        }
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        float val = (float)field.GetValue(obj);
                        if (ImGui.InputFloat(field.Name, ref val))
                        {
                            field.SetValue(obj, val);
                        }
                    }
                    else if (field.FieldType == typeof(Color))
                    {
                        Color val = (Color)field.GetValue(obj);
                        System.Numerics.Vector4 vecVal = new System.Numerics.Vector4(val.r, val.g, val.b, val.a);

                        if (ImGui.ColorPicker4(field.Name, ref vecVal))
                        {
                            val = new Color(vecVal.X, vecVal.Y, vecVal.Z, vecVal.W);
                            field.SetValue(obj, val);
                        }
                    }
                    else if (field.FieldType == typeof(Vector2))
                    {
                        Vector2 val = (Vector2)field.GetValue(obj);
                        System.Numerics.Vector2 vecVal = new System.Numerics.Vector2(val.x, val.y);

                        if (ImGui.InputFloat2(field.Name, ref vecVal))
                        {
                            val = new Vector2(vecVal.X, vecVal.Y);
                            field.SetValue(obj, val);
                        }
                    }
                }
            }
            
        }

        public static void UpdateTreeObjects()
        {

        }

        internal class VirtualObj//持有虚引用的对象
        {
            internal string name;
            internal List<WeakHandle> refs = new List<WeakHandle>();
        }
    }

    internal class TreeNode
    {
        internal string name;
        internal List<TreeNode> children = new List<TreeNode>();
        internal List<SCDevNodeTreeManager.VirtualObj> virtualObjs = new List<SCDevNodeTreeManager.VirtualObj>();

        public TreeNode(string name)
        {
            this.name = name;
            SCDevNodeTreeManager.virtualObjMap.Add(this, virtualObjs);
        }

        public TreeNode AddChild(string newNode)
        {
            var res = new TreeNode(newNode);
            children.Add(res);
            return res;
        }

        public TreeNode GetChild(string path, bool addIfMissing )
        {
            string[] nodes = path.Split('.');
            if (nodes.Length == 1)
            {
                if (nodes[0] == name)
                    return this;
                else
                    return null;
            }  
            else if (nodes.Length == 2)
            {
                foreach(var child in children)
                {
                    if(child.name == nodes[1])
                        return child;
                }
                if (addIfMissing)
                    return AddChild(nodes[1]);
                else
                    return null;
            }
            else
            {
                string subPath = string.Join(".", nodes.Skip(1));
                foreach (var child in children)
                {
                    if (child.name == nodes[1])
                        return child.GetChild(subPath, addIfMissing);
                }
                if (addIfMissing)
                {
                    var newChild = AddChild(nodes[1]);
                    return newChild.GetChild(subPath, addIfMissing);
                }
                else
                    return null;
            }
        }

    }

}
