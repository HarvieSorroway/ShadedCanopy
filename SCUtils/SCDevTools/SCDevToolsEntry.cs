using ImGuiNET;
using RWIMGUI.API;
using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager;

namespace SCUtils.SCDevTools
{
    internal class SCDevToolsEntry
    {
        static TestClass testInstance1, testInstance2;
        static TestClassB testInstanceB1;
        public static void Init()
        {
            try
            {
                ImGUIAPI.SwitchContext(new SCDevToolsGUI());
                NodeTreeTypeInfo.BuildTypeInfos();
                SCDevNodeTreeManager.Init();

                testInstance1 = new TestClass()
                {
                    intValue = 42,
                    floatValue = 3.14f,
                    stringValue = "Hello, SC DevTools!"
                };
                testInstance2 = new TestClass()
                {
                    intValue = 7,
                    floatValue = 1.71f,
                    stringValue = "Another Test Instance"
                };

                testInstanceB1 = new TestClassB()
                {
                    parent = testInstance1,
                    colorValue = UnityEngine.Color.cyan,
                    vec2Value = new Vector2(10.0f, 20.0f)
                };

                SCDevNodeTreeManager.Track(testInstance1);
                SCDevNodeTreeManager.Track(testInstance2);
                SCDevNodeTreeManager.Track(testInstanceB1);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    
        public static void TestUpdate()
        {
            testInstance2.floatValue = 1f + Mathf.Sin(Time.time);
        }
    }

    [SCDevToolsInspectType("Root.TestA","TestClassA")]
    public class TestClass
    {
        [SCDevToolsInspectValue] public int intValue;

        [SCDevToolsInspectValue]
        [SCDevToolsRangeField(0, 2f)]
        [SCDevToolsDrawGraph]
        public float floatValue;

        [SCDevToolsInspectValue] public string stringValue;
    }

    [SCDevToolsInspectType("Root.TestB", "TestClassB")]
    public class TestClassB
    {
        public TestClass parent;
        [SCDevToolsInspectValue] public UnityEngine.Color colorValue;
        [SCDevToolsInspectValue] public Vector2 vec2Value;

        [SCDevToolsInspectValue]
        [SCDevToolsListBoxStringField(new string[]
        {
            "Option 1",
            "Option 2",
            "Option 3",
            "Option 4"
        })]
        public string optionValue;


        [SCDevToolsInspectValue] public CreatureTemplate.Type testExtEnum;
    }

    public class SCDevToolsGUI : IMGUIContext
    {
        bool toolActive = true;
        System.Numerics.Vector4 color;

        DevGUIPage currPage;
        VirtualObj selectedObj;

        public override void Render(ref IntPtr IDXGISwapChain, ref uint SyncInterval, ref uint Flags)
        {
            SCDevToolsEntry.TestUpdate();
            ImGui.Begin("SC DevTools", ref toolActive, ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginMenuBar())//绘制模式选择
            {
                if (ImGui.BeginMenu("Mode"))
                {
                    foreach (var pg in Enum.GetValues(typeof(DevGUIPage)).Cast<DevGUIPage>())
                    {
                        if (ImGui.MenuItem(pg.ToString()))
                        {
                            currPage = pg;
                            selectedObj = null;
                        }
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }


            if (currPage == DevGUIPage.Default)
            {
                RenderDefaultPage();
            }
            else if (currPage == DevGUIPage.TestPage)
            {
                RenderTestPage();
            }
            else if (currPage == DevGUIPage.NodeTree)
            {
                RenderNodeTreePage();
            }
        }

        void RenderDefaultPage()
        {
            ImGui.Text("Welcome to SC DevTools!");
            ImGui.Text("Select a page from the menu to get started.");
            ImGui.End();

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 100));
            ImGui.Begin("SC DevTools - Quick Info", ref toolActive, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

            ImGui.Text("Quick multi-windows test");
            ImGui.SameLine();
            float[] samples = new float[100];
            for (int n = 0; n < 100; n++)
            {
                samples[n] = Mathf.Sin(n * 0.2f + (float)ImGui.GetTime() * 1.5f);
            }

            ImGui.PlotLines("Samples", ref samples[0], samples.Length);
            ImGui.End();
        }

        void RenderTestPage()
        {
            ImGui.ColorEdit4("Color", ref color);

            // Generate samples and plot them
            float[] samples = new float[100];
            for (int n = 0; n < 100; n++)
            {
                samples[n] = Mathf.Sin(n * 0.2f + (float)ImGui.GetTime() * 1.5f);
            }

            ImGui.PlotLines("Samples", ref samples[0], samples.Length);

            // Display contents in a scrolling region
            ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 0f, 1f), "Important Stuff");

            ImGui.BeginChild("Scrolling");
            for (int n = 0; n < 50; n++)
                ImGui.Text($"{n} Some text");
            ImGui.EndChild();
            ImGui.Text($"End Important Stuff");
            ImGui.End();
        }

        void RenderNodeTreePage()
        {
            ImGui.Text("Node Tree View Coming Soon!");
            RenderNodeTree(SCDevNodeTreeManager.rootNode);
            ImGui.End();

            if(selectedObj != null)
            {
                if (selectedObj.refs[0].Target == null)
                {
                    selectedObj = null;
                    return;
                }

                //ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 100));
                ImGui.Begin($"SC DevTools - {selectedObj.name}");
                SCDevNodeInspector.DrawVirtualObj(selectedObj);
                ImGui.End();
            }
        }

        void RenderNodeTree(TreeNode treeNode)
        {
            //ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if(treeNode.children.Count == 0)
            {
                //flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

                if(treeNode.virtualObjs.Count == 0)
                {
                    ImGui.TreeNodeEx(treeNode.name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                }
                else if (ImGui.TreeNodeEx(treeNode.name, ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    int id = 0;
                    
                    foreach (var obj in treeNode.virtualObjs)
                    {
                        ImGui.PushID(id);
                        string nodeLabel = $"{obj.name}";
                        ImGui.TreeNodeEx(nodeLabel, ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                        if (ImGui.IsItemClicked())
                        {
                            selectedObj = obj;
                            SCDevNodeInspector.SetUpInspector(selectedObj);
                        }

                        ImGui.PopID();
                        id++;

                    }
                    ImGui.TreePop();
                }
                
            }
            else
            {
                bool nodeOpen = ImGui.TreeNodeEx(treeNode.name, ImGuiTreeNodeFlags.OpenOnArrow);
                if (nodeOpen)
                {
                    foreach (var child in treeNode.children)
                    {
                        RenderNodeTree(child);
                    }
                    ImGui.TreePop();
                }
            }
        }


        // Leaving this uncommented allows you to interact with the game outside the imgui window
        public override bool BlockWMEvent()
        {
            return base.BlockWMEvent() && (ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) || ImGui.IsAnyItemHovered());
        }

        public enum DevGUIPage
        {
            Default,
            TestPage,
            NodeTree
        }
    }
}
