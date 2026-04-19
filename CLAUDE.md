# CLAUDE.md

## 项目定位

这是一个《杀戮尖塔2》建议型 AI mod 项目。

它不是自动代打 AI，不做点击模拟，不接管整局操作。  
它的目标是在游戏内给玩家提供结构化建议：

- 路线推荐
- 战后选卡推荐
- 遗物三选一推荐

## 当前阶段

当前已进入：

- 阶段二 / 决策日志系统

阶段一的路线推荐 MVP 已完成真实游戏接入。  
阶段二的三类决策日志闭环也已经打通：

- 路线
- 选卡
- 遗物

## 当前技术栈

### 游戏侧运行时

- Godot 4.5.1
- .NET / C#
- 游戏主程序集：`sts2.dll`
- mod 入口：`ModInitializer`

### 仓库结构

- `mod/csharp/SkAiRouteAdvisor/`
  - 当前真实运行时代码
- `mod/src/`
  - 早期 Java 原型，主要作为结构与评分参考
- `docs/`
  - 项目与 schema 文档
- `shared/schemas/`
  - schema 示例和说明

## 当前已完成能力

### 路线推荐

- 真实 `RunState` / `Player` / `MapPoint` 接入
- 路径枚举
- baseline 规则评分
- 地图界面展示
- 主路线高亮
- `F6` 切换 `safe / balanced / aggressive`

### 决策日志

已完成统一日志 schema 与 JSONL 落盘：

- `route_decision`
- `card_reward_decision`
- `relic_choice_decision`

当前日志文件按“每个 run 一个文件”落盘。

## 关键目录

- 真实运行时代码：
  - `mod/csharp/SkAiRouteAdvisor/src/`
- 决策日志系统：
  - `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/`
- 文档：
  - `docs/PROJECT_STATUS.md`
  - `docs/TASK_BOARD.md`
  - `docs/SESSION_HANDOFF.md`
  - `docs/DATA_SCHEMA.md`

## 构建命令

```powershell
dotnet build mod\csharp\SkAiRouteAdvisor\SkAiRouteAdvisor.csproj -c Release /p:Sts2GameDir='E:\SteamLibrary\steamapps\common\Slay the Spire 2'
```

## 日志输出

逻辑路径：

```text
user://sk_ai_route_advisor/decision_logs/run_<run_id>.jsonl
```

当前机器上的真实路径已确认是：

```text
C:\Users\28443\AppData\Roaming\SlayTheSpire2\sk_ai_route_advisor\decision_logs\
```

## 当前已知问题

- `relic_name` / `relic_summary[].relic_name` 之前曾输出成 `LocString` 对象字符串，当前代码已收尾修正为可读文本，需要再实际验证一次。
- `route actual choice` 当前主要是基于“下一跳节点”推断，不能总是唯一映射到完整路径。
- UI 仍偏工程调试风格，不是最终产品化视觉。
- 推荐逻辑当前主要是规则型 baseline，不是模型推理。

## 后续 agent 接手建议

进入仓库后优先阅读：

1. `CLAUDE.md`
2. `docs/PROJECT_STATUS.md`
3. `docs/TASK_BOARD.md`
4. `docs/SESSION_HANDOFF.md`
5. `docs/DATA_SCHEMA.md`

然后重点看：

- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/`
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/`

## 当前最适合继续推进的方向

如果继续收尾阶段二：

- 校准日志字段可读性
- 统一 `character_id` / `card_id` / `relic_id` 风格
- 优化 recommendation 与 candidate_options 的数据质量

如果进入阶段三：

- 开始 Python 服务骨架
- 直接复用当前 JSONL schema
- 不要另起一套日志格式

## 明确边界

这个项目不是：

- 自动代打
- 输入代理
- 整局 autonomous agent

这个项目是：

- 游戏内建议系统
- 结构化数据采集与分析管线
- 后续训练和离线评估的数据基础
