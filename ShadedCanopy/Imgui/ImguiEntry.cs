using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using JetBrains.Annotations;
using RWIMGUI.API;

namespace ShadedCanopy.Imgui
{
    /// <summary>
    /// ImGui 调试系统的入口点与上下文管理器。
    /// <para>
    /// 可以通过点击 "Debug Arguments" 按钮
    /// 进入由 源生成器 自动生成的参数调试面板，实现数据的显示、实时调整与保存。
    /// </para>
    /// </summary>
    internal static class ImguiRegister
    {
        private static float testInput = 6.0f;
        public static void TryInit()
        {
            if (ModManager.ActiveMods.Any(i => i.id == "rwimgui"))
            {
                ImguiEntry.Init();
            }
            // 将值赋给 DebugArguments
            // 生成器会生成只读属性
            DebugArguments.Title = "Debug Arguments"; // 实例 只读显示
            DebugArguments.Test = new List<string>{"A", "B"};// 实例 只读显示

            // 直接写这行代码，IDE 可能会暂时报错说 TestInput 不存在
            // 生成器运行后，会自动生成 public static float TestInput { get; set; }
            // 并在 UI 中生成一个 InputFloat 控件
            testInput = DebugArguments.TestInput; // 实例 可写显示
        }
    }
    internal static class ImguiEntry
    {
        [CanBeNull] public static IMGUIContext LastContext;
        
        
        public static unsafe void Init()
        {
            typeof(ImGUIAPI).Module.GetType("RWIMGUI.Core.InitializationManager", true)
                .GetMethod("InitializeAll", BindingFlags.Static | BindingFlags.Public)!.Invoke(null, Array.Empty<object>());
            ImGUIAPI.AddMenuCallback(&DebugEditorPresent);
        }
    
        public static void DebugEditorPresent(ref IntPtr swapChain, ref uint sync, ref uint flags)
        {
            if (ImGui.BeginTabItem("Stupid Mouse"))
            {
                if (ImGui.Button("Debug Arguments"))
                {
                    LastContext = ImGUIAPI.CurrentContext;
                    ImGUIAPI.SwitchContext(new DebugArgumentContext());
                }
                ImGui.EndTabItem(); 
            }
        }
    }
}