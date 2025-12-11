namespace ShadedCanopy.Imgui
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
    internal sealed class ImguiAutoPropertyAttribute : System.Attribute { }

    internal static class DebugMeta
    {
        public static T With<T>(T arg, bool showOnly = false, (T min, T max)? range = null)
        {
            return arg;
        }
    }
    [ImguiAutoProperty]
    public static partial class DebugArguments
    {
        public static partial void Draw();
    }
}