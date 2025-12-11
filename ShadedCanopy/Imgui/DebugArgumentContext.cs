using System;
using ImGuiNET;
using RWIMGUI.API;

namespace ShadedCanopy.Imgui
{
    public class DebugArgumentContext : IMGUIContext
    {
        public override bool BlockWMEvent()
        {
            return false; 
        }
    
        public override void Render(ref IntPtr swap, ref uint sync, ref uint flags)
        {
            ImGui.BeginTabBar("Debug");
            if (ImGui.BeginTabItem("Debug Arguments"))
            {
                DebugArguments.Draw();
                if (ImGui.Button("Exit"))
                {
                    ImGUIAPI.SwitchContext(ImguiEntry.LastContext);
                    ImguiEntry.LastContext = null;
                }
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }
}