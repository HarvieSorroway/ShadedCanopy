using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager;

namespace SCUtils.SCDevTools.NodeTreeManager
{
    internal static class SCDevNodeInspector
    {
        static List<List<FieldDrawer>> fieldDrawers = new List<List<FieldDrawer>>();
        static List<List<List<FieldDrawer>>> specialFieldDrawers = new List<List<List<FieldDrawer>>>();

        public static void SetUpInspector(VirtualObj virtualObj)
        {
            fieldDrawers.Clear();
            specialFieldDrawers.Clear();

            foreach (var weakRef in virtualObj.refs)
            {
                var typeInfo = NodeTreeTypeInfo.GetTypeInfo(weakRef.Target.GetType());
                var obj = weakRef.Target;
                List<FieldDrawer> drawers = new List<FieldDrawer>();
                List<List<FieldDrawer>> specialDrawersList = new List<List<FieldDrawer>>();
                foreach (var field in typeInfo.valueFields)
                {
                    //主要控件
                    if (field.FieldType == typeof(string))
                    {
                        if (field.GetCustomAttribute<SCDevToolsListBoxStringField>() != null)
                        {
                            drawers.Add(new DropBoxStringFieldDrawer(field, field.GetCustomAttribute<SCDevToolsListBoxStringField>().options));
                        }
                        else
                        {
                            drawers.Add(new StringFieldDrawer(field));
                        }
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        drawers.Add(new IntFieldDrawer(field));
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        if (field.GetCustomAttribute<SCDevToolsRangeField>() != null)
                        {
                            var attr = field.GetCustomAttribute<SCDevToolsRangeField>();
                            drawers.Add(new FloatBarFieldDrawer(field, attr.min, attr.max));
                        }
                        else
                        {
                            drawers.Add(new FloatFieldDrawer(field));
                        }
                    }
                    else if (field.FieldType == typeof(Color))
                    {
                        drawers.Add(new ColorFieldDrawer(field));
                    }
                    else if (field.FieldType == typeof(Vector2))
                    {
                        drawers.Add(new Vector2FieldDrawer(field));
                    }
                    else if (field.FieldType.IsExtEnum())
                    {
                        drawers.Add(new EnumFieldDrawer(field, field.FieldType));
                    }

                    List <FieldDrawer> specialDrawers = new List<FieldDrawer>();
                    //额哇控件
                    if (field.FieldType == typeof(float) && field.GetCustomAttribute<SCDevToolsDrawGraph>() != null)
                    {
                        specialDrawers.Add(new FloatGraphViewer(field, obj));
                    }

                    specialDrawersList.Add(specialDrawers);
                }
                fieldDrawers.Add(drawers);
                specialFieldDrawers.Add(specialDrawersList);
            }
        }

        public static void DrawVirtualObj(VirtualObj virtualObj)
        {
            System.Numerics.Vector2 windowSize = ImGui.GetWindowSize();
            float cursorX = ImGui.GetCursorPosX();

            foreach (var weakRef in virtualObj.refs)
            {
                if (!weakRef.IsAlive)
                {
                    ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "[Dead Reference]");
                    continue;
                }


                var typeInfo = NodeTreeTypeInfo.GetTypeInfo(weakRef.Target.GetType());
                var obj = weakRef.Target;

                System.Numerics.Vector2 textSize = ImGui.CalcTextSize(typeInfo.type.Name); // 计算文本尺寸

                ImGui.SetCursorPosX((windowSize.X - textSize.X) * 0.5f);
                ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0, 1), typeInfo.type.Name);
                ImGui.SetCursorPosX(cursorX);

                for (int i = 0; i < typeInfo.valueFields.Count; i++)
                {
                    int vIndex = virtualObj.refs.IndexOf(weakRef);
                    var field = typeInfo.valueFields[i];
                    var drawer = fieldDrawers[vIndex][i];
                    drawer.Draw(field, obj);
                    foreach(var sDrawer in specialFieldDrawers[vIndex][i])
                    {
                        sDrawer.Draw(field, obj);
                    }   
                }
                //foreach (var field in typeInfo.valueFields)
                //{
                //    if (field.FieldType == typeof(string))
                //    {
                //        string val = field.GetValue(obj) as string;
                //        if (ImGui.InputText(field.Name, ref val, 256))
                //        {
                //            field.SetValue(obj, val);
                //        }
                //    }
                //    else if (field.FieldType == typeof(int))
                //    {
                //        int val = (int)field.GetValue(obj);
                //        if (ImGui.InputInt(field.Name, ref val))
                //        {
                //            field.SetValue(obj, val);
                //        }
                //    }
                //    else if (field.FieldType == typeof(float))
                //    {
                //        float val = (float)field.GetValue(obj);
                //        if (ImGui.InputFloat(field.Name, ref val))
                //        {
                //            field.SetValue(obj, val);
                //        }
                //    }
                //    else if (field.FieldType == typeof(Color))
                //    {
                //        Color val = (Color)field.GetValue(obj);
                //        System.Numerics.Vector4 vecVal = new System.Numerics.Vector4(val.r, val.g, val.b, val.a);

                //        if (ImGui.ColorPicker4(field.Name, ref vecVal))
                //        {
                //            val = new Color(vecVal.X, vecVal.Y, vecVal.Z, vecVal.W);
                //            field.SetValue(obj, val);
                //        }
                //    }
                //    else if (field.FieldType == typeof(Vector2))
                //    {
                //        Vector2 val = (Vector2)field.GetValue(obj);
                //        System.Numerics.Vector2 vecVal = new System.Numerics.Vector2(val.x, val.y);

                //        if (ImGui.InputFloat2(field.Name, ref vecVal))
                //        {
                //            val = new Vector2(vecVal.X, vecVal.Y);
                //            field.SetValue(obj, val);
                //        }
                //    }
                //}
            }

        }

        internal abstract class FieldDrawer
        {
            string name;
            float spacing;
            internal FieldDrawer(FieldInfo fieldInfo)
            {
                name = fieldInfo.Name + " : ";
                spacing = 20f + ImGui.CalcTextSize(name).X;
            }
            internal abstract void Draw(FieldInfo fieldInfo, object obj);

            internal virtual void DrawLabel(FieldInfo fieldInfo, bool sameLine = true)
            {
                ImGui.Text(fieldInfo.Name);
                if(sameLine) ImGui.SameLine(spacing);
            }
        }

        internal class StringFieldDrawer : FieldDrawer
        {
            public StringFieldDrawer(FieldInfo fieldInfo) : base(fieldInfo)
            {
            }

            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                string val = fieldInfo.GetValue(obj) as string;
                DrawLabel(fieldInfo);
                if (ImGui.InputText($"##{fieldInfo.Name}", ref val, 256))
                {
                    fieldInfo.SetValue(obj, val);
                }
            }
        }

        internal sealed class DropBoxStringFieldDrawer : FieldDrawer
        {
            string[] options;
            public DropBoxStringFieldDrawer(FieldInfo fieldInfo, string[] options) : base(fieldInfo)
            {
                this.options = options;
            }
            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                string val = fieldInfo.GetValue(obj) as string;
                int currentIndex = options.IndexOf(val);
                if (currentIndex < 0) currentIndex = 0;

                DrawLabel(fieldInfo);
                if (ImGui.Combo($"##{fieldInfo.Name}", ref currentIndex, options, options.Length))
                {
                    fieldInfo.SetValue(obj, options[currentIndex]);
                }
            }
        }

        internal sealed class IntFieldDrawer : FieldDrawer
        {
            public IntFieldDrawer(FieldInfo fieldInfo) : base(fieldInfo)
            {
            }

            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                int val = (int)fieldInfo.GetValue(obj);

                DrawLabel(fieldInfo);
                if (ImGui.InputInt($"##{fieldInfo.Name}", ref val))
                {
                    fieldInfo.SetValue(obj, val);
                }
            }
        }

        internal sealed class FloatFieldDrawer : FieldDrawer
        {
            public FloatFieldDrawer(FieldInfo fieldInfo) : base(fieldInfo)
            {
            }

            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                float val = (float)fieldInfo.GetValue(obj);

                DrawLabel(fieldInfo);
                if (ImGui.InputFloat($"##{fieldInfo.Name}", ref val))
                {
                    fieldInfo.SetValue(obj, val);
                }
            }
        }

        internal sealed class FloatBarFieldDrawer : FieldDrawer
        {
            float min;
            float max;
            public FloatBarFieldDrawer(FieldInfo fieldInfo, float min, float max) : base(fieldInfo)
            {
                this.min = min;
                this.max = max;
            }
            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                float val = (float)fieldInfo.GetValue(obj);

                DrawLabel(fieldInfo);
                if (ImGui.SliderFloat($"##{fieldInfo.Name}", ref val, min, max))
                {
                    fieldInfo.SetValue(obj, val);
                }
            }
        }

        internal sealed class ColorFieldDrawer : FieldDrawer
        {
            public ColorFieldDrawer(FieldInfo fieldInfo) : base(fieldInfo)
            {
            }

            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                Color val = (Color)fieldInfo.GetValue(obj);
                System.Numerics.Vector4 vecVal = new System.Numerics.Vector4(val.r, val.g, val.b, val.a);

                DrawLabel(fieldInfo, false);
                if (ImGui.ColorPicker4($"##{fieldInfo.Name}", ref vecVal))
                {
                    val = new Color(vecVal.X, vecVal.Y, vecVal.Z, vecVal.W);
                    fieldInfo.SetValue(obj, val);
                }
            }
        }

        internal sealed class Vector2FieldDrawer : FieldDrawer
        {
            public Vector2FieldDrawer(FieldInfo fieldInfo) : base(fieldInfo)
            {
            }

            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                Vector2 val = (Vector2)fieldInfo.GetValue(obj);
                System.Numerics.Vector2 vecVal = new System.Numerics.Vector2(val.x, val.y);

                DrawLabel(fieldInfo);
                if (ImGui.InputFloat2($"##{fieldInfo.Name}", ref vecVal))
                {
                    val = new Vector2(vecVal.X, vecVal.Y);
                    fieldInfo.SetValue(obj, val);
                }
            }
        }

        internal sealed class EnumFieldDrawer : FieldDrawer
        {
            string[] options;
            Type type;
            ConstructorInfo createInstanceMethod;

            string currOption = string.Empty;

            public EnumFieldDrawer(FieldInfo fieldInfo, Type type) : base(fieldInfo)
            {
                this.type = type;
                createInstanceMethod = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string), typeof(bool) }, null);

                var boo = ExtEnumBase.valueDictionary.TryGetValue(type, out var extEnumType);

                SCUtils.Log($"Try get extenumbase for {type.Name} {boo}");
                if (boo)
                {
                    SCUtils.Log($"{type.Name} entries count : {extEnumType.entries.Count}");
                    options = extEnumType.entries.ToArray();
                }
                //var test = new CreatureTemplate.Type("Scavenger");
            }

            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                if(string.IsNullOrEmpty(currOption))
                {
                    var extEnum = (fieldInfo.GetValue(obj) as ExtEnumBase);
                    if (extEnum == null)
                        currOption = options[0];
                    else
                        currOption = extEnum.value;
                }

                int currentIndex = options.IndexOf(currOption);
                if (currentIndex < 0) currentIndex = 0;

                DrawLabel(fieldInfo);
                if (ImGui.Combo($"##{fieldInfo.Name}", ref currentIndex, options, options.Length))
                {
                    currOption = options[currentIndex];
                    fieldInfo.SetValue(obj, createInstanceMethod.Invoke(new object[] {currOption, false}));
                }
            }
        }

        internal sealed class FloatGraphViewer : FieldDrawer
        {
            string name;
            float[] samples = new float[100];
            public FloatGraphViewer(FieldInfo fieldInfo, object obj) : base(fieldInfo)
            {
                name = $"Sample Graph - {fieldInfo.Name}";
                float initVal = (float)fieldInfo.GetValue(obj);
                for (int i = 0;i < samples.Length; i++)
                {
                    samples[i] = initVal;
                }
            }
            internal override void Draw(FieldInfo fieldInfo, object obj)
            {
                for(int i = 0; i < samples.Length - 1; i++)
                {
                    samples[i] = samples[i + 1];
                }
                samples[samples.Length - 1] = (float)fieldInfo.GetValue(obj);
                ImGui.PlotLines(name, ref samples[0], samples.Length);
            }
        }
    }
}
