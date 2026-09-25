# Inescapable Tarkov's Softcore

SPT 5.0 服务端整合 mod。把 Life in Norvinsk v0.3.2 的六组功能 + 战局时长控制，自包含重写为单一 C# 服务端 mod。

## 目标

- 游戏线：SPT 5.0.0（build 47242 / EFT 1.1.5 / net10.0 / IL2CPP + BepInEx 6）
- 部署：MO2 overlay `[5]核心-Inescapable-Tarkovs-Softcore-<版本>` → 目标实例 `E:\Game\EFT_Offline\Inescapable Tarkov`

## 功能组

1. **Samuel's Tweaks**：护甲弹挂冲突修复 / 可掠夺臂章与近战 / 三格弹匣缩格 / 启动器背景
2. **True Items Redux**：物品真实堆叠上限
3. **nofirhideout**：藏身处建造移除 FIR 要求
4. **反重力臂章**：臂章减重 + 堆叠
5. **更大的背包**：背包网格扩容
6. **Softcore 经济与制造系统大修**：20 个 changer（安全容器 / 藏身处 / 经济 / 商人 / 制造 / 保险 / 杂项）
7. **战局时长控制**：全局倍率

## 文档

- `CONTEXT.md` — 术语表
- `docs/adr/` — 架构决策记录
- 规格书与工单：to-spec / to-tickets 产出（后续提交）

## 状态

设计访谈（grill-with-docs）已收敛；规格书待产出。版本自 `0.1.0` 起步。
