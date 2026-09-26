# CONTEXT — Inescapable Tarkov's Softcore

> 术语表（glossary）。只记录项目语言，不记录实现细节与规格。

## 核心术语

- **整合 mod**：本项目交付物——把 Life in Norvinsk v0.3.2 的六组功能自包含重写进一个 SPT 5 服务端 mod。
- **源 mod**：旧整合包中提供需求行为的 SPT 3.11 时代 TypeScript mod；只读参考素材，不再运行。
- **六组功能**：① Samuel's Tweaks ② True Items Redux 物品真实堆叠 ③ noFirHideout 藏身处免打勾 ④ 反重力臂章 ⑤ 更大的背包 ⑥ Softcore 经济与制造系统大修。
- **战局时长控制**：v1 唯一超出六组的新增功能；形态 = 全局倍率（默认 1.0 = 原版）。
- **组开关**：单一配置 JSON 中每组的 enabled 开关。
- **覆盖层**：旧包中「…沉浸感增强设置」与「《难度：生存》」等后置覆盖目录；与基础 mod 叠加后决定最终数值。
- **最终游玩态**：基础六件套 + 沉浸感增强 + 难度：生存预设 的叠加结果——新 mod 默认值基准。
- **差异表**：基础 vs 沉浸感 vs 生存 的逐项数值对照；写入规格书供微调。
- **目标实例**：MO2 便携实例 `E:\Game\EFT_Offline\Inescapable Tarkov`（profile: Default），游戏根 `E:\Game\EFT_Offline\SPT_5xx`（SPT 5.0.0，build 47242，EFT 1.1.5）。
- **部署单元**：MO2 overlay 目录 `[5]核心-Inescapable-Tarkovs-Softcore-<版本>`。
- **客户端件**：需要 BepInEx 6 客户端插件的部分；v1 无客户端件（启动器背景已于反馈 8 移除，客户端优化插件属延后项）。
- **延后项**：PerformanceTweaks 客户端优化插件——源码在 toolkit 仓库 `mods\PerformanceTweaks`（net472 时代产物），需针对 EFT 1.1.5 客户端重新审查评估，不属 v1。
