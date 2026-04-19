# 决策日志数据结构

最后更新：2026-04-19

## 目标

决策日志系统以“决策事件”为中心，统一记录三类建议型决策：

- `route_decision`
- `card_reward_decision`
- `relic_choice_decision`

日志系统服务于：

- 训练数据采集
- 离线回放与分析
- 推荐质量排查
- 版本升级行为对比
- 跨会话开发接续

## 文件组织

当前实现使用：

- 存储格式：`JSONL`
- 策略：`一次 run 一个文件`
- 目录：`user://sk_ai_route_advisor/decision_logs/`
- 文件名：`run_<run_id>.jsonl`

运行时通过 Godot `ProjectSettings.GlobalizePath("user://")` 解析真实路径。

每一行都是一个完整的 `DecisionLogEntry`。  
优点：

- 方便追加写入
- 方便 Python 批量按行读取
- 单条损坏不会破坏整个 run 文件

## 顶层结构

每条日志事件统一为：

```json
{
  "metadata": {},
  "run_state": {},
  "decision_event": {},
  "candidate_options": [],
  "recommendation": {},
  "actual_choice": null,
  "outcome_snapshot": {}
}
```

## 字段定义

### metadata

- `schema_version`
- `game_version`
- `mod_version`
- `timestamp_utc`
- `session_id`
- `run_id`
- `seed`
- `character_id`
- `difficulty_or_ascension`
- `act_index`
- `floor_index`

说明：

- `run_id` 当前由 mod 侧生成并在同一 `RunState` 生命周期内保持稳定。
- `seed` 当前来自 `RunState.Rng.StringSeed`。

### run_state

- `current_hp`
- `max_hp`
- `current_hp_ratio`
- `gold`
- `potion_count`
- `potion_slots`
- `current_node_id`
- `current_map_position`
- `deck_summary`
- `relic_summary`

#### deck_summary

- `total_cards`
- `upgraded_cards`
- `card_ids`
- `card_counts_by_id`

#### relic_summary

数组项结构：

- `relic_id`
- `relic_name`
- `stack_count`

### decision_event

- `decision_id`
- `decision_type`
- `decision_phase`
- `source_screen`
- `mode_tag`
- `model_version_or_scorer_version`

当前约定：

- `decision_type`
  - `route_decision`
  - `card_reward_decision`
  - `relic_choice_decision`
- `decision_phase`
  - `recommendation_generated`
  - `actual_choice_recorded`

### candidate_options

统一外层结构：

- `option_id`
- `payload`

`payload` 为多态对象，当前支持三种：

- `route_payload`
- `card_reward_payload`
- `relic_choice_payload`

### recommendation

- `recommended_option_id`
- `secondary_option_id`
- `recommendation_scores`
- `reasons`
- `confidence`

### actual_choice

- `chosen_option_id`
- `chosen_option_payload`
- `choice_source`
- `choice_timestamp_utc`

`chosen_option_payload` 为多态对象，当前支持：

- `route_choice_payload`
- `card_reward_choice_payload`
- `relic_choice_payload`

### outcome_snapshot

当前全部为预留字段：

- `next_combat_damage_taken`
- `survived_next_elite`
- `floors_survived_after_decision`
- `hp_after_n_floors`
- `gold_after_n_floors`
- `notes`

## 三类 payload

### route_payload

- `path_id`
- `node_sequence`
- `node_types`
- `total_score`
- `risk_score`
- `reward_score`
- `recovery_score`
- `structure_score`
- `reasons`
- `style_tag`

### card_reward_payload

- `card_id`
- `card_name`
- `rarity`
- `cost`
- `upgraded`
- `score`
- `reasons`
- `skip_option`

### relic_choice_payload

- `relic_id`
- `relic_name`
- `relic_category`
- `score`
- `reasons`

## 路线决策闭环

当前已接通的日志闭环是路线推荐：

1. 地图打开且推荐生成时，写入 `recommendation_generated`
2. 玩家进入下一节点时，写入同一 `decision_id` 的 `actual_choice_recorded`

注意：

- 路线推荐候选项是“从当前点到 Boss 的完整路径”
- 玩家实际选择在游戏里通常只是“下一跳节点”
- 因此 `route_choice_payload` 当前记录：
  - `selected_next_node_id`
  - `selected_next_node_type`
  - `matched_candidate_option_ids`
  - `resolved_to_single_candidate`

如果多个完整候选路径共享同一个下一跳，`chosen_option_id` 会保留为 `null`，由 `matched_candidate_option_ids` 表达“可能对应的候选集合”。

## 选卡与遗物接入点

当前已实现的预留入口：

- `DecisionLogService.LogCardRewardRecommendation(...)`
- `DecisionLogService.LogCardRewardChoice(...)`
- `DecisionLogService.LogRelicChoiceRecommendation(...)`
- `DecisionLogService.LogRelicChoice(...)`

当前状态：

- schema 和服务层已就位
- 路线日志已接通 recommendation + actual choice
- 选卡与遗物已接通 recommendation + actual choice 的运行时入口
- 仍需 live 验证屏幕候选抓取与 skip/choice 判定

## 版本兼容策略

- 顶层使用 `schema_version`
- 新字段优先追加，不修改旧字段语义
- `outcome_snapshot` 为预留扩展位
- Python 侧应按字段存在性做兼容，不假设所有字段都非空

## 已知限制

- 当前 `run_id` 是 mod 侧生成，不是游戏官方原生字段
- 路线 actual choice 目前只能稳定解析到“下一跳节点”
- 选卡/遗物 recommendation 与 actual choice 入口已接通，但仍需 live 验证
- 当前文件落盘是本地 JSONL，不包含数据库或联网功能

## 训练脚本消费建议

推荐读取方式：

1. 扫描 `decision_logs/*.jsonl`
2. 逐行解析为 JSON
3. 按 `decision_type` 分流
4. 对 `candidate_options[].payload.payload_type` 做类型分发
5. 对同一 `decision_id` 的 `recommendation_generated` / `actual_choice_recorded` 做 join

## 最小样例

仓库内示例文件：

- [route_decision.sample.json](</E:/sk_ai/shared/schemas/examples/route_decision.sample.json>)
