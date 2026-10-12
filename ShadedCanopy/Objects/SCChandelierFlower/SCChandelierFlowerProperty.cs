using SCUtils.SCDevTools.NodeTreeManager;
using UnityEngine;

namespace ShadedCanopy.Objects.SCChandelierFlower
{
    internal static class SCChandelierFlowerProperty
    {
        const string DevToolPath = "Root.ChandelierFlower"; // DevTools 参数树路径
        // Fruit
        [SCDevToolsInspectType(DevToolPath, "FruitPhysical")]
        internal class FruitPhysicalProperty
        {
            [SCDevToolsInspectValue]
            public float airFriction = 0.999f; // 空气阻力
            [SCDevToolsInspectValue]
            public float gravity = 0.9f; // 重力
            [SCDevToolsInspectValue]
            public float bounce = 0.2f; // 反弹系数
            [SCDevToolsInspectValue]
            public float surfaceFriction = 0.7f; // 地面摩擦
            [SCDevToolsInspectValue]
            public int collisionLayer = 1; // 碰撞层
            [SCDevToolsInspectValue]
            public float waterFriction = 0.95f; // 水中阻力
            [SCDevToolsInspectValue]
            public float buoyancy = 1.1f; // 浮力
            [SCDevToolsInspectValue]
            public float rad = 9f; // 碰撞半径
            [SCDevToolsInspectValue]
            public float mass_on_stalk = 0.5f; // 在藤上的质量（影响被拉扯时的加速度/力反馈）
            [SCDevToolsInspectValue]
            public float mass_off_stalk = 0.2f; // 被摘下来的质量
            [SCDevToolsInspectValue]
            public float rollingFriction = 0.8f; // 滚动摩擦
            [SCDevToolsInspectValue]
            public float rollingRotationSpeedRatio = 0.1f; // 滚动带来的自旋速度比例
        }
        public static FruitPhysicalProperty fruitPhysicalProperty = new(); // 果实物理参数集合

        [SCDevToolsInspectType(DevToolPath, "FruitObject")]
        internal class FruitObjectProperty
        {
            [SCDevToolsInspectValue]
            public string fruitContainerBeforePick = "Foreground0"; // 未摘取时所在容器
            [SCDevToolsInspectValue]
            public string fruitContainerAfterPick = "Foreground"; // 摘取后所在容器
            [SCDevToolsInspectValue]
            public float fruitSpriteScale = 1f; // 果实精灵缩放
            [SCDevToolsInspectValue]
            public Color fruitColor = Color.green; // 果实颜色（用于着色/透镜底色）
            [SCDevToolsInspectValue]
            public float fruitLensAlpha = 0.7f; // 透镜主体透明度
            [SCDevToolsInspectValue]
            public float fruitRimAlpha = 0f; // 果实边缘/阴影透明度
            [SCDevToolsInspectValue]
            public float fruitRimScale = 1.02f; // 果实边缘/阴影缩放倍率
            [SCDevToolsInspectValue]
            public float fruitLightAlpha = 0.3f; // 果实发光透明度（基础）
            [SCDevToolsInspectValue]
            public float fruitLightRad = 80f; // 果实发光半径（基础）
            [SCDevToolsInspectValue]
            public float fruitLightAlphaGrabbed = 0.5f; // 被抓住时发光透明度
            [SCDevToolsInspectValue]
            public float fruitLightRadGrabbed = 100f; // 被抓住时发光半径
            [SCDevToolsInspectValue]
            public float fruitLightAlphaMaxSlope = 0.03f; // 发光透明度随速度变化的最大斜率
            [SCDevToolsInspectValue]
            public float fruitLightRadMaxSlope = 1f; // 发光半径随速度变化的最大斜率

            [SCDevToolsInspectValue]
            public float fruitGrabVisualOffsetY = 4f; // 玩家抓住时的视觉 Y 偏移（像素）

            [SCDevToolsInspectValue]
            public int fruitPickRequireTicks = 30; // 摘取所需帧数
            [SCDevToolsInspectValue]
            public int fruitBites = 4; // 可被咬的次数
            [SCDevToolsInspectValue]
            public int fruitFoodPoints = 3; // 提供的食物点数
            public Player.ObjectGrabability fruitGrability = Player.ObjectGrabability.OneHand; // 可抓取方式（单手/双手等）
        }
        public static FruitObjectProperty fruitObjectProperty = new(); // 果实显示与交互参数集合

        // Stalk
        [SCDevToolsInspectType(DevToolPath, "TorusPhysical")]
        public class TorusPhysicalProperty
        {
            [SCDevToolsInspectValue]
            public float airFriction = 0.95f; // 空气阻力
            [SCDevToolsInspectValue]
            public float gravity = 0.9f; // 重力
            [SCDevToolsInspectValue]
            public float bounce = 0.2f; // 反弹系数
            [SCDevToolsInspectValue]
            public float surfaceFriction = 0.7f; // 地面摩擦
            [SCDevToolsInspectValue]
            public int collisionLayer = 1; // 碰撞层
            [SCDevToolsInspectValue]
            public float waterFriction = 0.95f; // 水中阻力
            [SCDevToolsInspectValue]
            public float buoyancy = 1.1f; // 浮力
            [SCDevToolsInspectValue]
            public float rad = 1f; // 茎节点碰撞半径
            [SCDevToolsInspectValue]
            public float mass = 1.2f; // 茎节点质量
            [SCDevToolsInspectValue]
            public float rollingFriction = 0.8f; // 滚动摩擦
            [SCDevToolsInspectValue]
            public float rollingRotationSpeedRatio = 0.1f; // 滚动带来的自旋速度比例
        }
        public static TorusPhysicalProperty torusPhysicalProperty = new(); // 茎（主物体）物理参数集合

        [SCDevToolsInspectType(DevToolPath, "StalkObject")]
        public class StalkObjectProperty
        {
            [SCDevToolsInspectValue]
            public float stalkSegmentMaxLength = 25f; // 单段最大长度（用于决定段数）
            [SCDevToolsInspectValue]
            public int stalkSegmentMinCount = 3; // 最小段数
            [SCDevToolsInspectValue]
            public float stalkNodeMass = 0.15f; // 茎节点质量
            [SCDevToolsInspectValue]
            public float stalkFlexibility = 1.1f; // 茎连接柔性系数（越大越“软/弹”）
            [SCDevToolsInspectValue]
            public float stalkFruitFlexibility = 1.2f; // 茎果之间的连接弹性系数（越大越“软/弹”）
            [SCDevToolsInspectValue]
            public float stalkFruitDistance = 20f; // 果实与花托节点的连接距离
            [SCDevToolsInspectValue]
            public float stalkSegmentShrink = 0.6f; // 段长缩放（用于整体更短/更长）

            [SCDevToolsInspectValue]
            public float stalkExtraPlayerElastic = 0.4f; // 玩家抓住时额外弹性/助力系数
            [SCDevToolsInspectValue]
            public float stalkMaxJumpBoost = 8f; // 松手时跳跃加成
            [SCDevToolsInspectValue]
            public float stalkMaxJumpBoostLenghPct = 0.3f; // 达到最大跳跃加成所需的拉伸比例

            [SCDevToolsInspectValue]
            public float stalkNoFruitShirink = 0.5f; // 无果实时的段长缩放

            [SCDevToolsInspectValue]
            public float stalkAntiInvertMinSeparation = 3f; // 防倒挂：花托节点与上一节点的最小垂直间距
            [SCDevToolsInspectValue]
            public float fruitAntiInvertMinSeparation = 5f; // 防穿模：果实与花托节点的最小垂直间距

            public string stalkContainer = "Background"; // 茎渲染容器名
            public string torusContainer = "Midground"; // 预留：花托/主节点容器名
        }
        public static StalkObjectProperty stalkObjectProperty = new(); // 茎结构/交互参数集合


        [SCDevToolsInspectType(DevToolPath, "Outfit")]
        public class OutfitProperty
        {
            [SCDevToolsInspectValue]
            public Color fruitBaseColor = Color.green; // 果实基础颜色
            // 颜色战地过大 最好放最后
            public Color stalkColorA = RWCustom.Custom.hexToColor("018C52"); // 茎颜色 A（用于随机插值）
            public Color stalkColorB = RWCustom.Custom.hexToColor("013E32"); // 茎颜色 B（用于随机插值）
            [SCDevToolsInspectValue]
            public Color stalkNoFruitColor = RWCustom.Custom.hexToColor("58561E"); // 无果实时茎的偏色
            [SCDevToolsInspectValue]
            public float depthFogColorLerpMultiplier = 0.5f; // 深度雾颜色混合倍率

            public Color C1Color = new Color(166 / 255f, 73 / 255f, 35 / 255f); // Cover 分段颜色 1
            public Color C2Color = new Color(193 / 255f, 120 / 255f, 90 / 255f); // Cover 分段颜色 2
            public Color C3Color = new Color(216 / 255f, 204 / 255f, 183 / 255f); // Cover 分段颜色 3
            public Color BaseColor = new Color(230 / 255f, 230 / 255f, 230 / 255f); // Cover 基础颜色
        }
        public static OutfitProperty outfitProperty = new(); // 配色参数集合


        [SCDevToolsInspectType(DevToolPath, "CoverPhysical")]
        internal class CoverPhysicalProperty
        {
            public float coverDistrubFlipMultipler = 2f; // 翻转扰动强度倍率
            //[SCDevToolsInspectValue]
            public float coverDistrubSpinMultipler = 6f; // 旋转扰动强度倍率
            //[SCDevToolsInspectValue]
            public float coverAutoDisturbProb = 0.1f; // 自动扰动概率（预留/未启用）
            [SCDevToolsInspectValue]
            public float coverDistrubMaxForce = 2f; // 扰动最大强度
            [SCDevToolsInspectValue]
            public float coverNoFruitDisturbMultiplier = 2.5f; // 无果实状态下扰动倍率

            [SCDevToolsInspectValue]
            public float coverSpinGravity = 0.02f; // Spin 回正强度（弹簧系数）
            [SCDevToolsInspectValue]
            public float coverSpinAirFriction = 0.97f; // Spin 空气阻尼（越小衰减越快）
            [SCDevToolsInspectValue]
            public float coverSpinMaxAngle = 180f; // Spin 最大角度限制
            [SCDevToolsInspectValue]
            public float coverFlipGravity = 0.02f; // Flip 回正强度（弹簧系数）
            [SCDevToolsInspectValue]
            public float coverFlipAirFriction = 0.96f; // Flip 空气阻尼（越小衰减越快）
            [SCDevToolsInspectValue]
            public float coverFlipMaxAngle = 75f; // Flip 最大角度限制

            [SCDevToolsInspectValue]
            public float coverNoiseTimeBaseStep = 0.015f; // 噪声时间基础步进
            [SCDevToolsInspectValue]
            public float coverNoiseTimeVelStep = 0.0025f; // 噪声时间随速度的额外步进
            [SCDevToolsInspectValue]
            public float coverAutoDisturbIntensityMultiplier = 2f; // 自动扰动强度倍率

            [SCDevToolsInspectValue]
            public float coverDetachCooldownInit = 1f; // 摘果后冷却初值（影响扰动强度）
            [SCDevToolsInspectValue]
            public float coverDetachCooldownDecay = 0.1f; // 冷却衰减速度（每帧）
            [SCDevToolsInspectValue]
            public float coverDetachNoiseIntensityMin = 0.35f; // 摘果冷却期噪声强度下限
            [SCDevToolsInspectValue]
            public float coverNoFruitLerpSpeed = 0.08f; // 无果实状态权重插值速度
            [SCDevToolsInspectValue]
            public float coverDetachImpulseSpinMultiplier = 0.18f; // 摘果瞬间 Spin 冲击倍率
            [SCDevToolsInspectValue]
            public float coverDetachImpulseFlipMultiplier = 0.22f; // 摘果瞬间 Flip 冲击倍率

            [SCDevToolsInspectValue]
            public int coverWitherTicks = 80; // 枯萎过渡帧数
        }
        public static CoverPhysicalProperty coverPhysicalProperty = new(); // Cover 物理扰动参数集合

        [SCDevToolsInspectType(DevToolPath, "CoverMesh")]
        internal class CoverMeshProperty
        {
            [SCDevToolsInspectValue]
            public int nLeaves = 4; // 外层叶片数量

            [SCDevToolsInspectValue]
            public int outerNLeaves = 3; // 外部叶片数量
            [SCDevToolsInspectValue]
            public int outerNDivTriangles = 4; // 外部叶片沿长度方向细分次数(不包括起始点）
            [SCDevToolsInspectValue]
            public Color outerColor = Color.black; // 外部颜色（单色）

            [SCDevToolsInspectValue]
            public float C1Pos = 0.3f; // 分段色 1 的位置（0-1）
            [SCDevToolsInspectValue]
            public float C2Pos = 0.5f; // 分段色 2 的位置（0-1）
            [SCDevToolsInspectValue]
            public float C3Pos = 0.7f; // 分段色 3 的位置（0-1）
            [SCDevToolsInspectValue]
            public int nSubColorCount = 3; // 分段色数量
            [SCDevToolsInspectValue]
            public int nDivTriangles = 10; // 叶片沿长度方向的细分次数
            [SCDevToolsInspectValue]
            public float smallEps = 0.01f; // 数值微小偏移（用于避免重合分割）

            [SCDevToolsInspectValue]
            public float InnerAngleOffset = 45; // 内层起始角偏移（度）
            [SCDevToolsInspectValue]
            public float InnerAnglePerLeaf = 50f; // 单个叶片内层扇形角度（度）
            [SCDevToolsInspectValue]
            public int InnerNRidges = 3; // 内层扇形分片数量（沿角度）
            [SCDevToolsInspectValue]
            public int InnerNVerticesPerRidge = 6; // 每个分片沿长度方向顶点数

            [SCDevToolsInspectValue]
            public int targetFaces = 600; // 目标三角面数（用于重采样/拆分）
        }

        [SCDevToolsInspectType(DevToolPath, "NormalCoverMesh")]
        internal class NormalCoverMeshProperty
        {
            public CoverMeshProperty meshProperty = new(); // Cover 网格公共参数

            [SCDevToolsInspectValue]
            public Vector3 CenterBezierA0 = new Vector3(0, 0, 0); // 中线 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 CenterBezierA1Offset = new Vector3(0, -0.4f, 0); // 中线 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 CenterBezierB0 = new Vector3(0, -1f, 1.2f); // 中线 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 CenterBezierB1Offset = new Vector3(0, 0, -0.4f); // 中线 Bezier 控制点 2 偏移

            [SCDevToolsInspectValue]
            public Vector3 RightBezierA0 = new Vector3(0, 0, 0); // 右边界 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 RightBezierA1Offset = new Vector3(0.4f, -0.3f, 0); // 右边界 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 RightBezierB0 = new Vector3(0.27f, -0.8f, 0.5f); // 右边界 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 RightBezierB1Offset = new Vector3(0, 0, -0.2f); // 右边界 Bezier 控制点 2 偏移

            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierA0 = new Vector3(0, 0, 0); // 外部中线 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierA1Offset = new Vector3(0, -0.1f, 0); // 外部中线 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierB0 = new Vector3(0, 0.1f, 0.6f); // 外部中线 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierB1Offset = new Vector3(0, -0.7f, -0.2f); // 外部中线 Bezier 控制点 2 偏移
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierA0 = new Vector3(0.02f, 0, 0.02f); // 外部右边界 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierA1Offset = new Vector3(0.55f, -0.4f, 0); // 外部右边界 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierB0 = new Vector3(0.25f, -0.25f, 0.2f); // 外部右边界 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierB1Offset = new Vector3(-0.12f, 0, -0.16f); // 外部右边界 Bezier 控制点 2 偏移

            [SCDevToolsInspectValue]
            public float VanishingZ = 1366; // 透视消失点 Z（越大越接近正交）
            [SCDevToolsInspectValue]
            public float coverScale = 24f; // Cover 模型缩放

            [SCDevToolsInspectValue]
            public Vector2 InnerBezierA0YZ = new Vector2(-0.05f, 0); // 内层曲线 Bezier 起点（Y,Z）
            [SCDevToolsInspectValue]
            public Vector2 InnerBezierA1OffsetYZ = new Vector2(-0.3f, 0); // 内层曲线 Bezier 控制点 1 偏移（Y,Z）
            [SCDevToolsInspectValue]
            public Vector2 InnerBezierB0YZ = new Vector2(-0.65f, 0.43f); // 内层曲线 Bezier 终点（Y,Z）
            [SCDevToolsInspectValue]
            public Vector2 InnerBezierB1OffsetYZ = new Vector2(0, -0.2f); // 内层曲线 Bezier 控制点 2 偏移（Y,Z）
        }
        public static NormalCoverMeshProperty normalCoverMeshProperty = new(); // Cover 正常形态网格参数集合

        [SCDevToolsInspectType(DevToolPath, "CoverWitheredMesh")]
        internal class CoverWitheredMeshProperty
        {
            [SCDevToolsInspectValue]
            public float witheredScaleXZ = 0.9f; // 枯萎形态 X/Z 缩放
            [SCDevToolsInspectValue]
            public float witheredScaleY = 1.05f; // 枯萎形态 Y 缩放

            public CoverMeshProperty meshProperty = new(); // Cover 网格公共参数

            [SCDevToolsInspectValue]
            public Vector3 CenterBezierA0 = new Vector3(0, 0, 0); // 中线 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 CenterBezierA1Offset = new Vector3(0, -0.55f, 0); // 中线 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 CenterBezierB0 = new Vector3(0, -1.2f, 0.9f); // 中线 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 CenterBezierB1Offset = new Vector3(0, 0, -0.45f); // 中线 Bezier 控制点 2 偏移

            [SCDevToolsInspectValue]
            public Vector3 RightBezierA0 = new Vector3(0, 0, 0); // 右边界 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 RightBezierA1Offset = new Vector3(0.35f, -0.33f, 0); // 右边界 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 RightBezierB0 = new Vector3(0.3f, -1.0f, 0.52f); // 右边界 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 RightBezierB1Offset = new Vector3(0, 0, -0.22f); // 右边界 Bezier 控制点 2 偏移

            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierA0 = new Vector3(0, 0, 0); // 外部中线 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierA1Offset = new Vector3(0, -0.2f, 0); // 外部中线 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierB0 = new Vector3(0, -0.1f, 0.5f); // 外部中线 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 outerCenterBezierB1Offset = new Vector3(0, -0.3f, -0.2f); // 外部中线 Bezier 控制点 2 偏移
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierA0 = new Vector3(0.02f, 0, 0.02f); // 外部右边界 Bezier 起点
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierA1Offset = new Vector3(0.55f, -0.45f, 0); // 外部右边界 Bezier 控制点 1 偏移
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierB0 = new Vector3(0.2f, -0.35f, 0.15f); // 外部右边界 Bezier 终点
            [SCDevToolsInspectValue]
            public Vector3 outerRightBezierB1Offset = new Vector3(-0.12f, 0, -0.16f); // 外部右边界 Bezier 控制点 2 偏移
            [SCDevToolsInspectValue]
            public float VanishingZ = 1366; // 透视消失点 Z（越大越接近正交）
            [SCDevToolsInspectValue]
            public float coverScale = 24f; // Cover 模型缩放

            [SCDevToolsInspectValue]
            public Vector2 InnerBezierA0YZ = new Vector2(-0.08f, 0); // 内层曲线 Bezier 起点（Y,Z）
            [SCDevToolsInspectValue]
            public Vector2 InnerBezierA1OffsetYZ = new Vector2(-0.28f, 0); // 内层曲线 Bezier 控制点 1 偏移（Y,Z）
            [SCDevToolsInspectValue]
            public Vector2 InnerBezierB0YZ = new Vector2(-0.7f, 0.33f); // 内层曲线 Bezier 终点（Y,Z）
            [SCDevToolsInspectValue]
            public Vector2 InnerBezierB1OffsetYZ = new Vector2(0, -0.22f); // 内层曲线 Bezier 控制点 2 偏移（Y,Z）
        }
        public static CoverWitheredMeshProperty coverWitheredMeshProperty = new(); // Cover 枯萎形态网格参数集合

    }

}
