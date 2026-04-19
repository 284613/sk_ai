# 项目状态

最后更新：2026-04-19

## 状态概览

- 当前阶段：阶段二 / 决策日志系统
- 已完成：方案设计、仓库骨架、路线推荐 MVP、统一决策日志 schema 与写盘服务
- 进行中：路线日志闭环验证与选卡/遗物日志入口验证
- 下一阶段：选卡与遗物推荐入口接通后的日志实装

## 本轮产出

- 明确项目定位为“建议系统”，不是自动代打系统。
- 明确阶段一先使用本地 baseline scorer，Python 服务留到后续阶段。
- 建立项目核心文档骨架与最小目录结构。
- 固定初始任务优先级，先围绕路线推荐 MVP 推进。
- 在 `mod/src` 下建立路线推荐领域模型，覆盖地图节点、运行状态、候选路径、推荐结果。
- 实现固定深度路径枚举器，支持从当前节点出发枚举可达路径并去重。
- 实现三种模式的 baseline 路径评分器：`safe`、`balanced`、`aggressive`。
- 实现 Top-2 推荐结果组装、文本面板模型和调试输出格式化。
- 增加接入边界：状态提供接口、展示接口、控制器配置和统一 JSON 导出器。
- 在 `shared/schemas/route_advisor_v1.md` 中固定路线推荐 MVP 的统一数据结构说明。
- 提供本地 debug 入口，并通过 `javac -encoding UTF-8` + `java` 验证可运行。
- 外部事实核查：确认 StS2 已在 2026-03-05 进入 Early Access，mod support 已存在但在 Early Access 中仍不稳定，因此当前未绑定未经核实的具体 SDK 接口。
- 基于真实游戏根目录核查，确认运行时为 Godot + C#，并新增可编译的 C# mod 骨架 `mod/csharp/SkAiRouteAdvisor/`。
- 已使用本机游戏目录成功编译 `SkAiRouteAdvisor.csproj`，验证 `ModInitializer` 入口和 `sts2.dll` 引用链成立。
- 在 C# mod 中接入 `RunManager.Instance` 事件，使用真实 `RunState` / `MapPoint` 枚举到 Boss 的路径。
- 在 C# mod 中实现本地 baseline 路径评分与 Top-2 推荐计算，当前模式固定为 `balanced`。
- 在 C# mod 中实现最小 map screen 文本面板，通过 `NMapScreen.Instance` 显示 Top-1、Top-2 和一句理由。
- 已将当前 DLL-only 版本安装到 `E:\\SteamLibrary\\steamapps\\common\\Slay the Spire 2\\mods\\SkAiRouteAdvisor\\` 以便手动验证。
- 基于首次游戏内截图反馈，已修正 HP/Gold 读取来源为 `Player.Gold` 与 `Player.Creature.CurrentHp/MaxHp`。
- 已压缩 overlay 路径展示，去掉当前起点与冗长 `MapCoord(...)` 文本，提升地图界面可读性。
- 已实现 Top-1 / Top-2 路线节点分数标签，当前设计为在节点旁显示增量分数。
- 已修复房间内状态变化不刷新的问题：当前会在 `GoldChanged`、`CurrentHpChanged`、`MaxHpChanged` 以及地图打开时重新计算推荐。
- 已将 UI 收敛为“主路线优先”：左上面板突出 Best 路线，并对 Top-1 路线段做高亮显示。
- 已实现游戏内模式切换：按 `F6` 可循环切换 `safe / balanced / aggressive`，并即时重算推荐。
- 已设计统一 `DecisionLogEntry` schema，覆盖 metadata、run_state、decision_event、candidate_options、recommendation、actual_choice、outcome_snapshot。
- 已实现 C# 侧 JSONL 日志服务，目录为 `user://sk_ai_route_advisor/decision_logs/`，策略为“一次 run 一个文件”。
- 已实现路线推荐 recommendation 日志与 actual_choice 补写闭环。
- 已为战后选卡和遗物三选一提供日志服务入口与 payload 结构预留。
- 已新增 [DATA_SCHEMA.md](</E:/sk_ai/docs/DATA_SCHEMA.md>) 和最小样例 [route_decision.sample.json](</E:/sk_ai/shared/schemas/examples/route_decision.sample.json>)。
- 已将选卡与遗物日志入口接到真实运行时屏幕监控与玩家状态事件。
- 已修正遗物选择入口，当前同时监控 `NChooseARelicSelection` 与 `NTreasureRoomRelicCollection`。

## 当前重点

- 在真实游戏中验证路线 recommendation 与 actual choice 日志是否按预期写入。
- 确认日志文件真实输出路径和每 run 一个 JSONL 文件的组织方式。
- 验证选卡和遗物屏幕 recommendation / actual choice 日志是否按预期落盘。
- 在不破坏兼容性的前提下校准 schema 字段与 payload 内容。

## 当前未决事项

- 《杀戮尖塔2》mod 接入方式、Hook 点和运行时限制尚未确认。
- 当前仓库仍没有 StS2 mod 构建脚本、资源描述或游戏 API 封装层。
- 当前展示仅为本地文本 renderer，还未接入真实游戏 UI。
- 当前路径枚举固定向前看 5 个节点，尚未根据真实地图规模做剪枝调优。
- 当前 JSON 导出已切到 C# `System.Text.Json`，Java 原型里的手写导出器不再是主路径。
- 当前 C# 运行时虽已接入真实类型，但尚未完成 live in-game 验证。
- 当前 overlay 是最小文本面板，位置和样式还未针对真实地图 UI 调整。
- 当前模式已支持热键切换，但还没有更正式的配置界面或持久化。
- 当前新 DLL 已构建成功，但游戏运行中会锁定旧 DLL，无法热覆盖部署。
- 当前主路线高亮使用反射访问 `NMapScreen` 的 `_paths` 字段，后续版本变动时需要留意兼容性。
- 当前路线 actual choice 只能稳定解析到“下一跳节点”，无法唯一映射到完整路径时会保留 `chosen_option_id = null`。
- 选卡与遗物日志已接到真实屏幕监控，但 recommendation/skip 判定仍需 live 验证。
