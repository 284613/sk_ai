# SkAiRouteAdvisor C# Mod Skeleton

最小可编译的 Slay the Spire 2 C# mod 骨架。

当前用途：

- 验证仓库已经切到真实运行时：`sts2.dll` + Godot + `ModInitializer`
- 提供后续接入地图读取和游戏内展示的实际落点
- 暂不硬接未验证的地图/UI Hook

## 本地构建

方式一：环境变量

```powershell
$env:STS2_GAME_DIR='E:\SteamLibrary\steamapps\common\Slay the Spire 2'
dotnet build mod\csharp\SkAiRouteAdvisor\SkAiRouteAdvisor.csproj -c Release
```

方式二：显式传参

```powershell
dotnet build mod\csharp\SkAiRouteAdvisor\SkAiRouteAdvisor.csproj -c Release /p:Sts2GameDir='E:\SteamLibrary\steamapps\common\Slay the Spire 2'
```

## 产物

- DLL: `bin\Release\net9.0\SkAiRouteAdvisor.dll`
- Manifest: `dist\SkAiRouteAdvisor.json`

## 当前下一步

1. 在 `sts2.dll` 中确认路线图界面和当前节点的真实类型与访问路径。
2. 在本项目中新增状态提供层，映射到仓库现有的路线推荐核心结构。
3. 实现一个最小 in-game presenter，把推荐文本挂到 map screen 或 debug panel。
