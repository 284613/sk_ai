# 会话交接

最后更新：2026-04-19

## 当前阶段

- 阶段二 / 决策日志系统

## 本次会话完成内容

- 完成项目上下文恢复，核对文档与代码现状。
- 修正文档中的阶段偏差：阶段一先使用本地 baseline scorer，不接入 Python 服务。
- 在 `mod/src/com/skai/sts2advisor/route/` 下建立路线推荐核心。
- 完成地图节点、运行状态、候选路径、结构化理由、推荐结果等核心数据结构。
- 完成固定深度路径枚举器，默认向前看 5 个节点，并按节点序列去重。
- 完成 baseline 路径评分器，支持 `safe`、`balanced`、`aggressive` 三种模式。
- 完成 Top-2 推荐结果组装与文本面板 renderer。
- 完成接入边界：`RouteStateProvider`、`RouteRecommendationPresenter`、`RouteAdvisorController`、`RouteAdvisorConfig`。
- 完成统一 JSON 导出器，便于后续日志系统和外部评分器复用。
- 在 `shared/schemas/route_advisor_v1.md` 中补充当前统一数据结构说明。
- 提供本地 debug 样例和 `main` 入口，可输出地图摘要、路径数量、分项分数与推荐理由。
- 使用 `javac -encoding UTF-8` 和 `java` 完成本地编译运行验证。
- 核查外部事实后确认：StS2 已有 mod support，但 Early Access 阶段更新频繁、mod 容易失效，因此当前代码保持 SDK 无关的薄适配层。
- 基于真实游戏根目录核查，确认 StS2 使用 Godot + .NET/C# 运行时，mod 入口为 `ModInitializer` 风格。
- 新增 `mod/csharp/SkAiRouteAdvisor/`，并成功在本机游戏目录引用下编译生成 DLL。
- 在 C# mod 中新增真实路线运行时，已订阅 `RunManager.Instance` 的 `RunStarted` / `ActEntered` / `RoomEntered` / `RoomExited` 事件。
- 已使用真实 `RunState.CurrentMapPoint ?? RunState.Map?.StartingMapPoint` 和 `MapPoint.Children` 进行路径枚举。
- 已在 C# mod 中实现 `balanced` 模式的 Top-2 路线评分和最小 map screen 文本面板。
- 已把构建产物复制到 `E:\\SteamLibrary\\steamapps\\common\\Slay the Spire 2\\mods\\SkAiRouteAdvisor\\`。
- 基于首次游戏内截图，已修正 overlay 中 `hp=0/0 | gold=0` 的错误读取。
- 已把路径展示从完整 `PointType@MapCoord(...)` 序列压缩为短标签路径，避免面板过宽。
- 已实现 Top-1 / Top-2 路线节点分数标签，格式为 `1:+x.x` / `2:+x.x`。
- 已修复火堆回血、商店消费这类房间内状态变化不刷新的问题。
- 已将 UI 收敛为“主路线优先”：左上面板缩减为 Best 路线信息，并高亮 Top-1 路线段。
- 已实现 `F6` 游戏内切模式，当前会在 `safe / balanced / aggressive` 之间循环。
- 已新增统一决策日志 schema，覆盖路线 / 选卡 / 遗物三类决策。
- 已在 C# mod 中实现日志服务层：
  - schema builder
  - event factory
  - JSON serializer
  - JSONL file writer
  - log service
- 已实现路线 recommendation 事件写入。
- 已实现路线 actual choice 事件补写，当前基于“进入下一跳节点”推断。
- 已为选卡和遗物建立日志入口方法，并接入真实屏幕事件监控与玩家状态事件。
- 已新增日志文档 [DATA_SCHEMA.md](</E:/sk_ai/docs/DATA_SCHEMA.md>) 和最小样例 [route_decision.sample.json](</E:/sk_ai/shared/schemas/examples/route_decision.sample.json>)。
- 已修正遗物日志 recommendation 入口，当前优先读取 `NChooseARelicSelection._relics`，并兼容 `NTreasureRoomRelicCollection`。

## 修改过的关键文件

- `docs/PROJECT_BRIEF.md`
  - 将阶段一表述收敛为“本地 baseline scorer”，避免与当前范围冲突。
- `docs/ARCHITECTURE.md`
  - 将架构改为“阶段一单进程本地评分，后续再拆 Python 服务”。
- `docs/PROJECT_STATUS.md`
  - 更新为阶段一已开始，并记录本次代码产出与当前阻塞点。
- `docs/TASK_BOARD.md`
  - 更新任务状态，标记已完成的模型、枚举、评分和本地调试闭环。
- `mod/src/com/skai/sts2advisor/route/model/*`
  - 路线推荐核心数据模型。
- `mod/src/com/skai/sts2advisor/route/RoutePathEnumerator.java`
  - 路径枚举逻辑。
- `mod/src/com/skai/sts2advisor/route/BaselineRouteScorer.java`
  - baseline 规则评分逻辑。
- `mod/src/com/skai/sts2advisor/route/RouteRecommendationEngine.java`
  - 枚举与评分编排层。
- `mod/src/com/skai/sts2advisor/route/RouteAdvisorController.java`
  - 接入边界控制器，负责状态 -> 推荐 -> 展示。
- `mod/src/com/skai/sts2advisor/route/serialization/RouteRecommendationJsonSerializer.java`
  - 统一 JSON 导出器。
- `mod/csharp/SkAiRouteAdvisor/SkAiRouteAdvisor.csproj`
  - 真实 StS2 C# mod 项目文件，引用本机 `sts2.dll` / `0Harmony.dll` / `GodotSharp.dll`。
- `mod/csharp/SkAiRouteAdvisor/src/Plugin.cs`
  - 真实 mod 入口，使用 `ModInitializer(nameof(Initialize))`。
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisorBootstrap.cs`
  - 初始化与关闭真实路线运行时。
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RouteAdvisorRuntime.cs`
  - 订阅运行事件、`GoldChanged` / `CurrentHpChanged` / `MaxHpChanged`，支持 `F6` 模式切换，并监控选卡/遗物选择屏幕。
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RouteRecommendationService.cs`
  - 基于真实 `MapPoint` 的路径枚举与 baseline 评分，并为每个节点输出增量分数。
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RouteOverlayPresenter.cs`
  - 在 `NMapScreen` 上显示更短、更可读的最小文本面板，并高亮 Top-1 路线段。
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RouteRecommendationModels.cs`
  - 当前也保留 `AllRoutes`，供日志系统记录完整候选项。
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RunStateScalarReader.cs`
  - 通过反射读取 `RunState` 标量字段，降低版本变动风险。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogModels.cs`
  - 顶层日志 schema 与统一快照结构。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionPayloads.cs`
  - 路线 / 选卡 / 异物三类 payload 以及 actual choice payload。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogFactory.cs`
  - 负责从运行时对象构建统一日志事件。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogService.cs`
  - 负责 run/session 管理、写盘调度与路线 / 选卡 / 遗物日志闭环。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogPathProvider.cs`
  - 负责日志目录和文件命名规则。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogJsonSerializer.cs`
  - 负责 `System.Text.Json` 序列化。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogFileWriter.cs`
  - 负责 JSONL append 和错误处理。
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogValueFormatter.cs`
  - 负责稳定的 `model_id` / `node_id` / 坐标格式化。
- `mod/csharp/SkAiRouteAdvisor/dist/SkAiRouteAdvisor.json`
  - 当前 manifest 模板。
- `shared/schemas/route_advisor_v1.md`
  - 路线推荐 MVP 的统一结构说明。
- `docs/DATA_SCHEMA.md`
  - 决策日志 schema、文件组织、兼容策略和样例说明。
- `shared/schemas/examples/route_decision.sample.json`
  - 最小日志样例。
- `mod/src/com/skai/sts2advisor/route/ui/*`
  - 文本面板模型与 renderer。
- `mod/src/com/skai/sts2advisor/route/debug/*`
  - 本地调试样例、debug 输出和可执行入口。

## 当前可运行状态

- C# mod 已可编译，并已安装到游戏 `mods/SkAiRouteAdvisor/` 目录。
- 已接通的日志事件：
  - `route_decision / recommendation_generated`
  - `route_decision / actual_choice_recorded`
- 已接通但待 live 验证的日志事件：
  - `card_reward_decision / recommendation_generated`
  - `card_reward_decision / actual_choice_recorded`
  - `relic_choice_decision / recommendation_generated`
  - `relic_choice_decision / actual_choice_recorded`
- 日志输出目录：
  - `user://sk_ai_route_advisor/decision_logs/`
  - 文件规则：`run_<run_id>.jsonl`
- 手动验证方式：
  1. 启动《杀戮尖塔2》。
  2. 开启一局新 run。
  3. 打开地图界面，触发路线推荐。
  4. 选择下一跳并进入房间。
  5. 检查日志目录中是否出现对应 `run_*.jsonl` 文件，以及是否同时包含 recommendation 和 actual_choice 两类 route 事件。

## 已知问题

- 具体 mod API、Loader、Hook 机制和打包方式尚未确认。
- 当前仓库已经有真实 C# mod 结构，但尚未完成 live in-game 验证。
- 当前路径枚举是固定深度 DFS，只适合作为 MVP baseline。
- 当前评分权重写在 Java 代码里，尚未配置化。
- Windows 环境编译需要显式指定 `javac -encoding UTF-8`。
- 当前仓库仍未确定实际 mod manifest、DLL/PCK 结构和加载方式。
- 当前已确认 manifest 和 DLL 结构，但尚未决定是否需要 PCK；当前 skeleton 先走 DLL-only。
- `NMapScreen.Instance` 可能晚于 mod 初始化出现，因此当前代码使用懒订阅；仍需进游戏验证是否稳定。
- 当前模式已支持 `F6` 热键切换，但还没有配置持久化。
- 路线 actual choice 当前只能稳定解析到“下一跳节点”，无法唯一映射到完整候选路径时会保留 `chosen_option_id = null`。
- 选卡和遗物日志入口已接通运行时，但候选抓取与 skip/choice 判定仍需 live 验证。
- 2026-04-19 23:12:15 版 DLL 已部署到游戏目录，包含遗物选择入口修正。

## 下次会话建议起点

1. 先验证路线日志是否真实落盘：
   - 打开地图触发 recommendation
   - 进入下一跳触发 actual choice
   - 检查同一 `decision_id` 的两类事件是否都存在
2. 重点验证遗物三选一屏幕的 recommendation / actual choice 事件是否真实落盘。
3. 如果要做训练/分析脚本，直接按 [DATA_SCHEMA.md](</E:/sk_ai/docs/DATA_SCHEMA.md>) 读取 JSONL，不要另起 schema。
4. 路线 actual choice 如需更精确，可考虑记录“下一跳节点 + 匹配候选路径集合”的后续聚合逻辑。

## 下次启动时优先检查的文件和模块

- `docs/PROJECT_STATUS.md`
- `docs/TASK_BOARD.md`
- `docs/ARCHITECTURE.md`
- `docs/DATA_SCHEMA.md`
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogService.cs`
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogFactory.cs`
- `mod/csharp/SkAiRouteAdvisor/src/DecisionLogging/DecisionLogModels.cs`
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RouteAdvisorRuntime.cs`
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RouteRecommendationService.cs`
- `mod/csharp/SkAiRouteAdvisor/src/RouteAdvisor/RouteOverlayPresenter.cs`
