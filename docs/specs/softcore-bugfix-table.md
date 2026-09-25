# Softcore 缺陷/优化清单（以代码核对为准）

> **权威声明**：fork 的 `docs/optimization-plan.md`（写于 2026-05-24）与当前代码严重脱节。经 MD5 核对：`SamMeow--Softcore\src` 与旧包 `[5]经济与制造系统大修-Softcore - 已AI优化\user\mods\odt-softcore\src` **9 个关键文件逐字节相同**（两处均 version 3.3.0）。本表以实际代码状态为准，替换 plan 原始结论。
> 用户决策：迁移时修复缺陷并记录。执行口径 = 本表「实际残留项」。

## 1. Plan 已过期（代码已修，无需处理）

| 编号 | plan 描述 | 代码现状证据 |
|---|---|---|
| B1 | 模板字面量失效（`Softcore.ts:24,32`） | 已用反引号 |
| B2 | Boss 弹药除法在循环内 | `OtherTweaksChanger.ts:197-199` 除法已在循环外 |
| B3 | 5 处重复 3-b-TG 配方 | 无法复现：`recipes.ts` 仅 650 行，`5ed515c8…` 仅 1 次作 `endProduct`，16 个 `endProduct` 无重复（重写时以配方唯一性校验兜底） |
| B5 | `buyPriceCoef` 跨 trader 累积 | `TraderChangesChanger.ts:122` 已移入 for 循环 |
| B6 | 局部变量遮蔽 `ConfigServer` 类名 | `InsuranceChangesChanger.ts:19` 已改 `configServer` |
| Q3 | 未使用的 `node:console` import | 已无该导入 |
| Q4 | `push(...[a,b])` 多余展开 | 已改 `push(a,b)` |
| P2 | 四个物品级修改各自全表遍历 | `OtherTweaksChanger.ts:154-166` 已合并单循环 |

## 2. 实际残留项（新 mod 处理口径）

| 编号 | 文件（旧 TS） | 问题 | 新 mod 处理 |
|---|---|---|---|
| B4 | `changers/TraderChangesChanger.ts`（`:22,:33,:118,:354`） | `stacticTraderList` 拼写错误 | C# 重写自然消除；记录中注明对应旧代码拼写 |
| Q5 | `changers/TraderChangesChanger.ts:159-172` | `if (false)` 死代码块 | 不迁移（删除） |
| Q6 | 各 changer 构造样板 | 重复 logger/tables 样板（`BaseChanger` 已存在，10 个已继承；`TraderChangesChanger`、`InsuranceChangesChanger` 等未继承） | C# 基类统一；异常/日志/表访问收口 |
| P1 | `changers/ScavCaseOptionsChanger.ts:109` | `doBetterRewards` 遍历未完全优化（弹药箱分支仍在循环内） | 迁移时补齐缓存/短路 |
| P3 | `changers/ScavCaseOptionsChanger.ts:117-132` | 弹药箱价格每次启动重算 `StackSlots` | 静态化/缓存 |
| P4 | `assets/fleamarket.ts`(785)、`assets/scavcase.ts`(310) | 大数组硬编码（内存/线性查找） | 迁为 JSON 资源 + `Set` 查找 |
| Q1 | 全库 changer | `!` 非空断言（`OtherTweaksChanger` 4 处、`SecureContainerOptionsChanger` 3 处、`StashOptionsChanger` 1 处） | C# 可空引用类型 + 显式 null 检查 |
| Q2 | 各 changer（18 个 .ts 含 `console.warn`） | 错误输出用 `console.warn` 而非 logger | 统一 `ISptLogger`；带组前缀 |
| C1 | `assets/fleamarket.ts:425-426` | BSG 黑名单硬编码自 3.10.2（源码注释 NEED TO UPDATE） | 迁移时升级到 5.x 数据源并核对 |
| C2 | 全库 | 直接依赖 SPT 内部 config（`IRagfairConfig`/`IHideoutConfig`/`IInsuranceConfig`/`ITraderConfig` 等） | 迁移时逐项对齐 SPT5 语义（重点验证项） |

## 3. 数值侧开放项（与差异表一致）

- `insuranceChanges.prapor.insuranceCostPercentage = 80`（BASE 25）存疑——按最终态保留，若属笔误在规格阶段修正。
- True Items IMM 自定义 stim `_id` 占位符问题（详见 `delta-table.md` §3b）。
