# 更新记录

##  - 2025-12-08
-  整体改动
	1.  修改了.csproj，现在生成时会自动替换mod目录里的旧版本dll
	2.  移除并且对.dll取消跟踪
	3.  删除了模板的portrait
-  代码改动
	1. SCUtils.SCHelperUtils.cs
		- 新增uad CreatureFollowingLabel，显示跟随生物的自定义文本标签来帮助debug
	2. ShadedCanopy.ShimmerSlugcat.PGraphicHooks.cs
		- 修复了在sandbox中体色不正常的bug
	3. ShadedCanopy.ShimmerSlugcat.PlayerHooks.cs
		- 新增在玩家周围打印能量值的debug功能，使用宏控制开关（参考TestingDefines.txt）
		- 新增在受到爆闪生物周围打印恐慌值的debug功能，使用宏控制开关（参考TestingDefines.txt）
		- 修复了吃回能量食物的时候没正常回复的bug
		- 修复了爆闪会闪自己的bug
		- 放宽了爆闪的按键触发条件，只要在松开spec键之前0.25s spec和pckp有同时摁下即可触发