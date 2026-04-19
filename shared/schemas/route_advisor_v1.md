# route-advisor-v1

最小统一数据结构，用于路线推荐 MVP 的日志记录、离线评估和未来外部评分器替换。

当前由 `mod/src/com/skai/sts2advisor/route/serialization/RouteRecommendationJsonSerializer.java` 导出。

## 顶层字段

- `schema_version`
- `run_state`
- `map_state`
- `recommendation_result`

## run_state

- `run_id`
- `act_index`
- `current_floor`
- `current_hp`
- `max_hp`
- `gold`
- `potion_count`
- `potion_slots`
- `character_id`
- `ascension_or_difficulty`
- `current_node_id`

## map_state.nodes[]

- `node_id`
- `floor_index`
- `node_type`
- `x`
- `y`
- `outgoing_edges[]`
- `incoming_edges[]`
- `reachable_from_current`

## recommendation_result

- `top_path`
- `second_path`
- `all_path_scores[]`
- `summary_reason`
- `scoring_version`

## scored_path

- `path_id`
- `node_sequence[]`
- `total_score`
- `risk_score`
- `reward_score`
- `recovery_score`
- `structure_score`
- `style_tag`
- `reasons[]`

## reason

- `category`
- `code`
- `message`
- `impact`

## 设计约束

- 不混入 UI 坐标、颜色、字体等展示字段。
- 保持命名稳定，未来外部服务应优先兼容该结构，而不是重新定义一套返回体。
- `scoring_version` 必须可追踪评分逻辑版本。
- 后续如果增加置信度，应作为新增字段追加，不修改现有语义。
