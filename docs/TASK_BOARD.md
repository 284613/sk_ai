# 任务看板

最后更新：2026-04-19

| 优先级 | 任务 | 状态 | 说明 |
| --- | --- | --- | --- |
| P0 | 文档与仓库骨架 | done | 已完成初始文档骨架与最小目录结构初始化 |
| P0 | 真实 mod 运行时核查 | done | 已确认 StS2 为 Godot + C# 运行时，mod 使用独立 json manifest 和 ModInitializer 入口 |
| P0 | C# mod 骨架 | done | 已创建并成功编译 `mod/csharp/SkAiRouteAdvisor`，可作为真实游戏接入入口 |
| P0 | 路线状态模型 | done | 已定义地图节点、运行状态、候选路径、推荐结果和结构化理由模型 |
| P0 | 接入边界与统一导出 | done | 已提供状态提供接口、展示接口、控制器配置和 JSON 导出器，便于后续接地图/UI/日志 |
| P0 | 地图状态读取 | in_progress | 已在 C# mod 中从真实 `RunState` / `MapPoint` 读取地图与当前位置，待 live 验证字段和边界情况 |
| P0 | 路线候选生成 | done | 已实现固定深度 DFS 路径枚举、基于节点序列去重，并明确当前限制与扩展点 |
| P0 | 路线评分 baseline | done | 已实现 `safe` / `balanced` / `aggressive` 三种模式的规则评分和结构化理由 |
| P0 | 游戏内推荐 UI | in_progress | 已在 `NMapScreen` 上实现最小文本面板，待 live 验证显示位置和刷新时机 |
| P0 | 路线节点分数标签 | in_progress | 已实现 Top-1 / Top-2 节点分数标签代码，待关闭游戏后部署并做 live 验证 |
| P0 | 房间内状态刷新 | in_progress | 已补 `GoldChanged` / `CurrentHpChanged` / `MaxHpChanged` 和 map-open 重算，待 live 验证火堆与商店场景 |
| P0 | 主路线高亮 | in_progress | 已实现 Top-1 路线段高亮和 UI 收敛，待 live 验证可读性与兼容性 |
| P0 | 游戏内模式切换 | in_progress | 已实现 `F6` 循环切换 `safe / balanced / aggressive`，待 live 验证手感与可发现性 |
| P0 | 本地调试闭环 | done | 已提供样例地图、debug main 和分项分数输出，可本地手动验证 |
| P0 | 游戏目录安装包 | done | 已将当前 DLL-only 版本部署到 `mods/SkAiRouteAdvisor/`，可直接手动测试 |
| P1 | 决策日志 schema | done | 已完成统一 Decision Log schema、三类 payload 和版本字段设计 |
| P1 | 日志落盘机制 | done | 已实现 JSONL 写盘、目录自动创建、每 run 一个文件和错误日志 |
| P1 | 路线日志闭环 | in_progress | 已接通 recommendation 与 actual_choice 记录，待 live 验证输出文件内容 |
| P1 | 选卡日志入口 | in_progress | 已接通真实战后卡牌屏幕 recommendation / actual choice 入口，待 live 验证 |
| P1 | 遗物日志入口 | in_progress | 已接通真实遗物选择屏幕 recommendation / actual choice 入口，待 live 验证 |
| P1 | 选卡评分接口 | todo | 定义选卡请求/响应协议，并接入评分服务 |
| P2 | 遗物评分接口 | todo | 定义遗物请求/响应协议，并接入评分服务 |
| P2 | 离线评估工具 | todo | 基于日志回放评估推荐质量，支持版本对比 |

## 执行顺序建议

1. 先完成 P1：验证路线、选卡、遗物三类日志真实落盘。
2. 在三类决策事件都能稳定记日志后，再进入选卡/遗物推荐实现。
3. 最后补离线评估和训练消费脚本，把日志系统接到数据闭环。
