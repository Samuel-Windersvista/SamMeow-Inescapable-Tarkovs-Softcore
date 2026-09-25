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

> 状态（T13）：7 项记录仍准确（源只读快照，代码现状未变；抽查 B1/B2/B5 与原文一致）→ 无需重做。B3 另见 §4。

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

> 状态（T13）：10 项全部核销（0 待办）→ 逐项证据见 §7a。

## 3. 数值侧开放项（与差异表一致）

- `insuranceChanges.prapor.insuranceCostPercentage = 80`（BASE 25）存疑——按最终态保留，若属笔误在规格阶段修正。
- True Items IMM 自定义 stim `_id` 占位符问题（详见 `delta-table.md` §3b）。

> 状态（T13）：2 项均为已记录的开放项（属用户决策 O1 / 规格存疑），未做代码修正；见 §8。

## 4. T11 结项记录（G6-D）

- **C1 已核对（0 失效）**：`fleamarket.json` 的 `bsgBlacklist` 353/353 全有效；同批 whitelist / questKeys / markedKeys / requestWhitelist（物品 id）与 actualBaseClasses / itemBaseClasses / pacifistFenceItemBaseWhitelist（父类 id）及 fleaListingsWhitelistHandBook（手册类目 id）全有效。详见 `delta-table.md` §4c。
- **B3 兜底口径**：唯一性校验仅作用于本 mod 自带的新增配方资源；SPT5 原版存在 13 处合法重复 `endProduct`，不做全局去重。
- 源码反转缺陷（记录并裁定）：`OtherTweaksChanger.vestsBlockArmor` 守卫写法与语义相反，按 SURV 终值语义落地（`false` ⇒ 弹挂与护甲不冲突）。

> 状态（T13）：C1 / B3 / `vestsBlockArmor` 三项与 `delta-table.md` §4c 一致，无矛盾 → §10。

## 5. T11 审查修复（G1–G6）

- **G1 任务按 id 匹配**：5.0 `quests.name` 为本地化键，按名匹配静默失效 → `OtherTweaksChanger` Drip-Out 改按 4 个 id、`CollectorQuestChanger` 改按 `5c51aac186f77432ea65c552`。测试夹具改用生产形态（locale-key 名）。
- **G2 rebalance 首条漂移**：3 条配方按旧包目标 id 钉定（`60098b17`→`61c77cc6…`、`5448fee0`→`5dc1f4d9…`、`5d6fc873`→`5dd3c5a6…`）；漂移清单见 `delta-table.md` §4c。
- **G3 Crisis 记录纠正**：5.0 A4S 仅 1 条（Level 条件已移除），源 `+30` 不可复现 → 告警跳过为正确行为。
- **G4 ExamineTime**：守卫 `not 0` → `> 0`（语义正确）。
- **G5 Standards**：模块级注册序断言（经真实 `SoftcoreModule.Apply`）；ops 引擎缺值改为告警+跳过（区分「未知 op」/「字段缺失」）；`CraftingRecipeGuard` → `internal`。
- **G6 再生成路径**：`dump-spt-symbols.cs` 前置条件（`FrameworkReference Microsoft.AspNetCore.App`，否则解析 `Microsoft.Extensions.Hosting.Abstractions` 失败）与符号表 SHA-256 记入 `data/softcore/MANIFEST.md`。

> 状态（T13）：G1–G6 全部落地并入库（commit `9f406fe` / `2051ce4` / `826150c`）；与 `delta-table.md` §4c 一致 → §10。

## 6. 遗留债（T13 评估）

- **ContainerRecipes 未外置（D12 例外）**：T08 `src/.../Changers/ContainerRecipes.cs` 以 C# 代码定义 `alpha/beta/epsilon/gamma` 四条安全容器配方（源 `assets/recipes.ts` 的 `containerRecipes`）。因源本身以代码常量定义、且 T08 已按 `Id` 幂等注册，T11 未将其并入 `data/softcore/crafting-recipes.json`。
  - **T13 评估结论：保留为遗留债**（源以代码定义 + 已幂等注册；外置的收益低于回归风险）。转为 §11-5 候选（低优先级）。

> 状态（T13）：评估完成 → 保留遗留债，见 §11-5。

---

## 7. 最终核销总表（T13）

### 7a §2 实际残留 10 项

| 编号 | 结论 | 证据（文件:行 / commit） |
|---|---|---|
| B4 | 已核销 — C# 重写消除拼写 | `TraderChangesChanger.cs:20` `StaticTraders`（旧 `stacticTraderList` 已无） |
| Q5 | 已核销 — 死代码不迁移 | 全库 `src/**/*.cs` 无 `if (false)`；源 `TraderChangesChanger.ts:159` 死块未迁移 |
| Q6 | 已核销 — 基类/接口收口样板 | `FeatureModule.cs:12`（泛型基类）；`ISoftcoreChanger.cs:8`；`SoftcoreModule.cs:28-44`（15 个 changer 统一注册）；`SoftcoreContext.cs:23,39`（依赖分组）；`SoftcoreChangeLog.cs` |
| P1 | 已核销 — 预建买断集合 + 短路 | `ScavCaseChanger.cs:71,144`（`BuildBuyableItems` 单次建集，保留式短路） |
| P3 | 已核销 — 手册索引 + 弹药箱定价补丁 | `ScavCaseChanger.cs:167,185,202-224`（`BuildHandbookIndex` + `ResolveHandbookPrice`） |
| P4 | 已核销 — 大数组外置内嵌 JSON | `InescapableTarkovsSoftcore.csproj:19-30`；`data/softcore/{scavcase,fleamarket,crafting-rebalance,crafting-recipes}.json`；**唯一例外 ContainerRecipes（§6）** |
| Q1 | 已核销 — 可空引用 + 显式检查；无守卫 `!` = 0 | 残留 5 处编译器必需 `!` 均有显式守卫：`FasterCraftingTimeChanger.cs:60,75`；`OtherTweaksChanger.cs:234,244`；`ScavCaseChanger.cs:221` → §9 / §11-1 |
| Q2 | 已核销 — 统一 `ISptLogger` + 组前缀 | `SoftcoreChangeLog.cs:20`（`[ITS] softcore.{changer}:`）；`ModuleOrchestrator.cs:79-88`（汇总转发）；`ModuleWarnings.cs`；`src` 无 `Console.`/`console.warn` |
| C1 | 已核销 — 5.x 数据核对 0 失效 | `delta-table.md:256`（§4c）；本表 §4 |
| C2 | 已核销 — SPT5 内部配置逐面测试对齐 | `SoftcoreTraderInsuranceTests.cs:12-307`（InsuranceConfig/TraderConfig）、`SoftcoreEconomyTests.cs:12-130`（RagfairConfig/GlobalTable）、`SoftcoreHideoutSpeedTests.cs`（HideoutSettings）、`SoftcoreCraftingTimeTests.cs:34-103`（HideoutConfig）、`SoftcoreScavCaseTests.cs`（ScavCaseConfig）、`SoftcoreGymTrainingTests.cs`（GlobalTable） |

### 7b §1 plan 过期项（确认仍准确）

| 编号 | T13 状态 |
|---|---|
| B1 | 仍准确（源 `Softcore.ts:24,32` 已用反引号） |
| B2 | 仍准确（源 `OtherTweaksChanger.ts:197-199` 除法在循环外） |
| B3 | 仍准确（`recipes.ts` 16 个 endProduct 无重复）；重写已加唯一性兜底 → §4 |
| B5 | 仍准确（源 `TraderChangesChanger.ts` `buyPriceCoef` 在 for 内） |
| B6 | 仍准确（源 `InsuranceChangesChanger.ts:19` `configServer`） |
| Q3 | 仍准确（无 `node:console` 导入） |
| Q4 | 仍准确（`push(a,b)`） |
| P2 | 仍准确（源 `OtherTweaksChanger.ts:154-166` 单循环） |

## 8. 全库清扫结果（T13）

| 命中 | 位置 | 结论 / 去向 |
|---|---|---|
| `priceRebalance 未迁移（SURV 关闭），告警` | `EconomyOptionsChanger.cs:45` | 已记录（`delta-table.md:237`）；SURV 关闭 → 非缺陷；`SoftcoreEconomyTests.cs:139` 覆盖告警不抛 |
| True Items 自定义 stim 占位 `_id`（22 条） | `delta-table.md:147,278`；`spec-v0.1.0.md`(O1)；`TrueItemsApplier.cs:49`；`TrueItemsModuleTests.cs:238` | 已记录（用户决策 O1）：按「匹配不到=不生效」+ 告警，测试覆盖 |
| `Resources/ placeholder` | `scripts/build.ps1:10,131`；`README.md:94` | 已记录：数据已内嵌 DLL，`Resources/` 确为占位 → §11-2（清理议题） |
| 测试夹具 `null!` 占位 | `SoftcoreTestData.cs:18,255,431` | 测试实现细节，非缺陷 |
| 未复刻 `ItemFilterService`/`SeasonalEventService` | `delta-table.md:231` | 已记录：5.0 core `ScavCaseRewardGenerator` 已代做 → 非缺陷 |
| SURV 关闭项（`skillExpBuffs` 等 4 项） | `README.md:296` | 已记录（SURV 终态默认关闭） |
| Crisis `+30` 不可复现 | `delta-table.md:254`；本表 §5 | 已记录（G3：Level 条件已移除 → 告警跳过） |
| `TODO` / `FIXME` / `HACK` / `NotImplementedException` | — | **0 处**（全库 `*.cs/*.md/*.mjs/*.ps1/*.json` 扫描） |

补充：`setBitcoinPriceTo100k` **已实现**（`FasterBitcoinFarmingChanger.cs:42-66`；SURV 默认 `false`）；Skier 任务奖励欧元化 **已实现**（`TraderChangesChanger.ApplySkierQuestRewards`；`delta-table.md:236`；测试 `SkierQuestRewards_ConvertRublesToEuros`）。

## 9. Q1/Q2 基础模式抽查（T13）

- **Q1（可空断言）**：核心 changer 无「无守卫 `!`」。残留 5 处 `!` 全部落在已显式判空的路径上：
  - `FasterCraftingTimeChanger.cs:60,75` — `TryGetRecipes(...) == true` 时 `recipes` 必非空（`production?.recipes is not null`）；
  - `OtherTweaksChanger.cs:234,244` — 由 `Where(item => item.Properties is {...})` / `Properties?.RigLayoutName is not null` 谓词保证；
  - `ScavCaseChanger.cs:221` — 由 `handbookIndex is not null`（`BuildHandbookIndex` 仅在 `Handbook?.Items` 非空时返回非空）保证。
  - 结论：**已核销**（可空引用 + 显式 null 检查）；5 处为编译器对 out/谓词窄化的限制，非逻辑风险 → §11-1。
- **Q2（统一日志）**：`src` 内无 `Console.Write*` / `console.warn`。changer 告警经 `SoftcoreChangeLog.Warn`（`[ITS] softcore.{changer}: ...`）累加，`SoftcoreModule` 汇总为 `ModuleReport`，`ModuleOrchestrator.Run:79-88` 逐条转发并输出合计；模块级日志经 `ISptLogger` + `ModuleWarnings`。结论：**已核销**。

## 10. 交叉核对（§4/§5/§6 与 `delta-table.md` §4c）

| 记录项 | 状态 |
|---|---|
| `vestsBlockArmor=false` 语义裁定（源守卫反转） | 一致：本表 §4 ↔ `delta-table.md:255` |
| Drip-Out `10/20`（5.0 原版 50/100） | 一致：本表 §5 ↔ `delta-table.md:253` ↔ `README.md:295` |
| **`ModifyBarter` 有意偏离**（源 `modifyTraderBarters` 仅改 `barter_scheme[id][0]` 内全部需求 → 会串扰；C# 精确定位同模板需求并覆盖全部 scheme） | **不一致（缺口）**：`delta-table.md` §4c 未单列；仅代码注释 `TraderChangesChanger.cs:241` + 测试 `SoftcoreTraderInsuranceTests.cs:184` → §11-3 |
| ScavCase 速度方向（`round(time/0.5)` = ×2，名称与方向相反） | 一致：`delta-table.md:229-230` ↔ `ScavCaseChanger.cs:135-138` ↔ `README.md:286` |
| ScavCase P1/P3/P4 缺陷状态 | 一致：`delta-table.md:231` ↔ 本表 §7a |
| Crisis 记录纠正（G3） | 一致：本表 §5 ↔ `delta-table.md:254` |
| ContainerRecipes 遗留债 | 一致：本表 §6 ↔ `data/softcore/MANIFEST.md` |

**矛盾**：未发现。**缺口**：1 处（`ModifyBarter` 偏离未在 delta-table 单列）→ §11-3。

## 11. 新 issue 候选（T13，不擅自扩大修复面）

1. **Q1 整洁（低）**：`FasterCraftingTimeChanger.TryGetRecipes` 的 `out List<...>?` 迫使调用方 `recipes!`；可改为返回可空列表并用局部非空变量，消除 2 处 `!`。影响：可读性；无功能风险。
2. **`Resources/` 占位清理（低）**：`build.ps1:131` 创建 `Resources/.gitkeep` 与 `README.md:94` 的说明，随数据全部内嵌 DLL 已冗余；可移除或改说明。影响：打包产物整洁；无功能风险。
3. **`ModifyBarter` 偏离补记（低）**：将「精确定位同模板需求 + 覆盖全部 scheme（源会串扰且只改首个 scheme）」补入 `delta-table.md` §4c（本审计 §10 已记录，待文档正式补行）。影响：文档完整性。
4. **`scavcase.json` 惰性黑名单项清理（很低）**：`parentBlacklist` 含 1 个 SPT5 已不存在的父类 id（`5d52cc5ba4b9367408500062`）——黑名单不匹配任何物品，惰性无害。影响：数据整洁。
5. **ContainerRecipes 统一外置（低，遗留债）**：§6 评估保留；如需与其它配方同源管理，再将其并入 `data/softcore/crafting-recipes.json`。影响：数据一致性；有回归风险（需重跑 T08 测试）。
