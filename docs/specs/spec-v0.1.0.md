# Spec: Inescapable Tarkov's Softcore v0.1.0

> 状态：ready-for-agent ｜ 目标版本：v0.1.0 ｜ 日期：2026-09-26
> 发布：GitHub issue #1（`Samuel-Windersvista/SamMeow-Inescapable-Tarkovs-Softcore`）；本文件为随仓库快照，以 issue 为准。
> 来源：grill-with-docs 设计收敛（两轮 12 项决策 + 测试接缝确认）与 6 轮只读侦察
> 术语：以 `CONTEXT.md` 为准
> 附件：`docs/specs/delta-table.md`（默认值差异表）· `docs/specs/softcore-bugfix-table.md`（缺陷清单，替换过期 optimization-plan）
> 锚点：SPT 5.0.0（build 47242 / EFT 1.1.5 / net10.0 / IL2CPP + BepInEx 6）｜目标实例：MO2 `Inescapable Tarkov`（profile Default）

## Problem Statement

玩家在 SPT 5.0.0 的「Inescapable Tarkov」整合包中游玩离线塔科夫。旧整合包 Life in Norvinsk v0.3.2 提供了六组让单机体验更顺手的服务端行为（Samuel's Tweaks、True Items Redux、藏身处免 FIR、反重力臂章、更大的背包、Softcore 经济与制造系统大修），但它们全部是 SPT 3.11 时代的 TypeScript 服务端 mod，无法在 SPT 5 上运行；且它们的最终数值分散在「基础 mod + 沉浸感增强覆盖 + 难度：生存预设」三层目录中，逐项调整需要跨多个目录找配置。玩家需要一个单一、自包含、可一处调整的服务端 mod：在新整合包里复现旧包的最终游玩态，并能随时调整个中数值。

## Solution

交付单一 C# 服务端 mod「Inescapable Tarkov's Softcore」：

- 自包含实现七组功能（六组 + 战局时长倍率），不依赖任何源 mod、源 mod 目录保持只读；
- 单一 `config.json`（每组独立开关 + 细项参数，中文注释），改后重启 SPT 服务器生效；
- 开箱默认值 = 旧包最终游玩态（以差异表为唯一数值真相）；
- 部署为 MO2 overlay `[5]核心-Inescapable-Tarkovs-Softcore-<版本>`，向目标实例投影（服务端 mod 至 `SPT_Runtime\user\mods\`）；
- 启动日志按组汇报变更计数与警告；缺失物品 ID 安全跳过；
- 单测（变换层接缝）+ 服务器启动日志 + 玩家进游戏逐组验收；
- 迁移时修复缺陷清单中的 10 项实际残留缺陷并记录。

## User Stories

1. 作为玩家，我希望新 mod 开箱即得旧包「最终游玩态」数值，以便新整合包可直接开始游玩。
2. 作为玩家，我希望所有数值集中在一个 JSON 配置文件中，以便不需要跨多个 mod 目录找配置。
3. 作为玩家，我希望每组功能有独立开关，以便逐组对比或排除故障。
4. 作为玩家，我希望关闭任意一组时其余组不受影响，以便稳定做 A/B 验证。
5. 作为玩家，我希望 mod 只作用于服务端、不写入游戏本体文件，以便卸载后游戏回到原版行为。
6. 作为玩家，我希望有一个全局战局时长倍率，以便自主控制单局长度。
7. 作为玩家，我希望护甲与弹挂甲冲突修复生效，以便配装不受互斥限制。
8. 作为玩家，我希望臂章与近战武器可被掠夺，以便击杀者可搜刮。
9. 作为玩家，我希望三格弹匣（容量 10–50）缩为两格，以便节省背包格位。
10. 作为玩家，我希望自定义启动器背景保留，以便视觉风格一致。
11. 作为玩家，我希望堆叠上限为「真实」数值（多数 2–5、注射器 8、配件不堆叠、钥匙卡不堆叠），以便经济与收纳体验与旧包一致。
12. 作为玩家，我希望堆叠可按物品/父类粒度调整，以便微调个别物品。
13. 作为玩家，我希望藏身处建造/升级不再要求 FIR，以便建造不被战局绑定。
14. 作为玩家，我希望 22 款臂章按色阶减重 6–25kg 且可堆叠 5，以便轻装潜行与收藏。
15. 作为玩家，我希望主流背包扩容（如 6Sh118 8×9）且容器过滤被清空，以便收纳自由。
16. 作为玩家，我希望背包数据中缺失的物品 ID 被安全跳过并告警，以便服务器不因数据差异而崩溃。
17. 作为玩家，我希望 Softcore 的渐进式安全容器与仓库（新档从 2×2 腰包起步）生效，以便获得渐进成长曲线。
18. 作为玩家，我希望藏身处加速按最终态生效（制造 3×、建设 50×、燃料 4×、ScavCase 速度 ×0.5、比特币按其终值），以便节奏合适。
19. 作为玩家，我希望跳蚤市场为和平主义形态且 1 级开放、仅全新品、价格 ×1.5，以便经济符合旧体验。
20. 作为玩家，我希望以物易物经济按最终态生效（现金 5%、价差 30%、报价 5–13、最多 4 换 1），以便交易有趣且可行。
21. 作为玩家，我希望保险数值按最终态生效（Prapor 70% 返还 / 240–360 分钟、Therapist 60% / 120–240 分钟），以便风险与回报符合旧体验。
22. 作为玩家，我希望弹药堆叠 ×5、货币堆叠关闭、技能经验增益关闭，以便数值与生存版一致。
23. 作为玩家，我希望收藏家任务要求为 20 级 + 1 张 Gamma，以便任务线符合旧体验。
24. 作为玩家，我希望弹挂甲与护甲不冲突（与护甲修复协同），以便配装自由。
25. 作为玩家，我希望服务器启动日志按组汇报变更计数与警告，以便确认生效并快速排查。
26. 作为维护者，我希望单测断言差异表关键值，以便对数值回归有自动化防护。
27. 作为维护者，我希望缺陷清单（10 项实际残留）逐项落实并记录，以便质量闭环。
28. 作为维护者，我希望构建脚本一键产出 overlay 内容（DLL + 配置 + 数据），以便快速迭代与部署。
29. 作为维护者，我希望部署经 MO2 overlay 且先做 profile 备份，以便随时回滚。
30. 作为维护者，我希望版本锚定 SPT 5.0.0 build 47242，且升级时以差异表 + 单测回归，以便长期可维护。
31. 作为玩家，我希望配置注释为中文，以便直接读懂每一项。
32. 作为玩家，我希望未知配置键或非法值只产生警告而不崩溃，以便配置试验安全。
33. 作为维护者，我希望未来新增大组 = 增加一节配置 + 一个模块，以便低摩擦扩展。
34. 作为玩家，我希望所有组可同时开启且互相兼容（内部应用顺序确定），以便默认全开即可玩。

## Implementation Decisions

**D1 交付形态**：单程序集 C# 服务端 mod（net10.0，`user/mods` 布局，恰一个 `IModMetadata` 实现，无 package.json）。标识：GUID `com.sammeow.inescapable-softcore`，显示名 Inescapable Tarkov's Softcore，作者 SamMeow，版本 0.1.0，`SptVersion ~5.0.0`，许可 MIT。部署目录名采用反向域名风格。

**D2 加载阶段**：`IOnLoad` @ `OnLoadOrder.Preload + 1`（数据库已导入、SPT post-DB 处理之前）。例外：若实现期发现某变更被 SPT post-DB 覆盖，该组改到 `PostLoad` 并记录于 ADR/注释。

**D3 模块结构**：Core（配置加载与校验、日志、应用编排器）+ 七组变换模块：G1 SamuelTweaks / G2 TrueItems / G3 NoFirHideout / G4 Antigrav / G5 Backpacks / G6 Softcore（含安全容器、藏身处、仓库、经济、商人、制造、保险、杂项等子模块）/ G7 RaidDuration；数据层 = 外置 JSON 资源（堆叠表、背包表、臂章表、跳蚤黑名单、配方、ScavCase、钥匙等）。

**D4 变换层接缝**（已确认）：每组模块暴露纯逻辑入口「（表集合, 组配置）→ 组报告（变更计数 + 警告列表）」；生产环境由编排器以注入的表调用；测试直接构造 POCO。

**D5 内部应用顺序**（决定重叠项胜负，固定）：G6 → G1 → G4 → G5 → G2 → G3 → G7。理由：G2 靠后应用使「逐物品显式值」优先于 G6 的通用倍率；G1 与 G6 的弹挂甲修复同向、无冲突；G3/G7 独立。

**D6 配置设计**：单一 `config.json`（随包分发默认文件、中文注释；解析容忍注释与尾逗号）。结构：`general{enabled, debug}` + 每模块 `{enabled, ...参数}`。未知键 → 警告；缺键 → 内置默认；默认值完整性由测试保证。默认值 = 差异表最终态。不注册 SPT 服务端配置编辑器（v0.2 候选）。config 缺失时重建默认并告警。

**D7 G1 要点**：护甲弹挂冲突修复（含 `RigLayoutName` 的弹挂 → `BlocksArmorVest=false`）；可掠夺（臂章父类 + 近战类目 → `Unlootable=false`、`UnlootableFromSide=[]`）；弹匣缩格（宽 1 且高 >2 且容量 10–50 → 高 2、ExtraSizeDown−1）；启动器背景（bg.png 静态替换，随 overlay 提供）。

**D8 G2 要点**：六张数据表按 List/ParentList 语义写 `StackMaxSize = 条目值 × StackMult`，并设 `StackMinRandom=1`；medicals 仅对空医疗容器（MaxHpResource ≤ 0）生效；最终态 StackMult: barter/clothing/keycards/partsnmods/provisions = 1、medicals = 2；`partsnmods.Active=false`（配件堆叠保持关闭）；keycards = 1（不堆叠）。

**D9 G3 要点**：遍历藏身处区域/阶段需求，含 `isSpawnedInSession` 键者置 `false`；无该键的需求不变。

**D10 G4 要点**：22 款臂章；减重 −6 ~ −25（色阶分级）、`StackMaxSize=5`（全部配置化）。

**D11 G5 要点**：43 条背包（cellsH/cellsV 扩容 + 清空 filters）；缺失 ID 跳过并告警（对应缺陷清单 Q1 精神）。

**D12 G6 要点**：子模块与最终态数值以差异表 §4 为准；数据资产（跳蚤黑名单 360+、配方、ScavCase、钥匙、生产调整）从旧 TS 转写为 JSON 资源（对应缺陷清单 P4）；迁移时核对 5.x 物品 ID 有效性，失效项移除并记录（对应缺陷清单 C1）；SPT5 内部配置依赖逐项对齐（对应缺陷清单 C2）。

**D13 G7 要点**：全局倍率（默认 1.0）× 各地图战局时限（字段以 SPT5 位置表模型为准，实现期核对）；仅服务端。

**D14 缺陷修复口径**：以 `softcore-bugfix-table.md` 的「实际残留项」10 项为准（B4、Q5、Q6、P1、P3、P4、Q1、Q2、C1、C2）；plan 已过期项（B1/B2/B3/B5/B6/Q3/Q4/P2）不重复劳动，仅记录「已修/无需处理」。

**D15 日志与错误隔离**：统一日志接口、组前缀；每模块一行 summary；缺物品/未知键/非法值 → 警告；单组异常捕获并报错，不阻断其他组。

**D16 构建与部署**：Release 构建产出（DLL + 配置 + 资源）→ 部署经 MO2 MCP：profile 备份 → 创建 overlay → 填充 → 启用 → 优先级定位；发行物归档至 `release/`。

**D17 许可与来源**：新代码 MIT；数据表源于 MIT/NCSA 许可的源 mod，来源记录于差异表与 README「来源与许可」节。

## Testing Decisions

- **好测试的定义**：只测外部行为——给定（配置 + 合成内存表）→ 断言表变化与组报告；不测私有实现细节。
- **主接缝**（已与用户确认）：变换层。测试项目独立于服务器运行，直接构造 SPT5 模型 POCO。
- **覆盖样例**（引用差异表数值）：
  - 配置层：默认文件加载 = 全组启用；每个键存在默认值；未知键 → 警告；缺键 → 默认。
  - G1：带 RigLayoutName 弹挂 → BlocksArmorVest=false；臂章 → Unlootable=false；宽 1/高>2/容量 10–50 弹匣 → 高 2 且 ExtraSizeDown−1；容量 51 → 不变。
  - G2：AA Battery → 5；Physical Bitcoin → 5；LEDX → 3；Crickent 保持 12；medicals StackMult=2（注射器实得 8）；keycards 不堆叠；partsnmods Active=false → 无变化；未命中 ID → 跳过 + 告警。
  - G3：合成需求 `isSpawnedInSession` → false；无该键 → 不变。
  - G4：White → (−6, 5)；Prestige 2 → (−25, 5)；22 条全命中；单品覆盖配置生效。
  - G5：6Sh118 → 8×9 且 filters 清空；仅基础项（9 条）不生效；缺失 ID 跳过。
  - G6：跳蚤等级 → 1；制造 ×3；弹药堆叠 ×5（含边界）；保险 70%/80%；收藏家 20 级 + 需求数 1；开关关闭 → 零变更。
  - G7：倍率 2.0 → 地图时限翻倍；1.0 → 不变。
- **验收（不进 CI）**：服务器启动日志出现各组 summary；玩家进游戏按核对清单逐组验证（臂章重量与堆叠、背包尺寸与过滤、堆叠上限、藏身处建造、跳蚤入口与等级、商人/保险、战局时长、启动器背景）。
- **先例**：新仓库无既有测试；结构参考 SPT 5 服务端源码仓库 `Testing/UnitTests` 的组织方式。

## Out of Scope

- SPT 4.x / 3.11 兼容（仅 SPT 5，锚定 build 47242）。
- 客户端插件 PerformanceTweaks（延后项；需针对 EFT 1.1.5 重新审查，另案处理）。
- 六组以外功能：旧包其余 mod（Realism、barter_economy、PathToTarkov、HomeComforts、战局大修等）；通用战局旋钮（玩家明确：不是这个 mod 该管的）。
- 热重载（重启生效已确认）；SPT 服务端配置编辑器 UI 集成（v0.2 候选）；F12 客户端配置；自动更新/源 mod 同步。
- 反编译或反推旧客户端 DLL。

## Further Notes

- **开放项**：
  - O1 True Items 自定义 stim 占位 `_id`（22 条）：按「复刻不生效」处理 + 文档标注（可选手工修正，默认不做）。
  - O2 保险 Prapor 保费 80%：按最终态保留；若确认为笔误则修正（单配置项）。
  - O3 配置编辑器 UI：v0.2 候选。
  - O4 PerformanceTweaks 客户端评审：延后项，目标 EFT 1.1.5 + BepInEx 6（源码在 toolkit 仓库 mods/PerformanceTweaks，net472 时代产物）。
- **性能与实现注意**：单次遍历合并、Set 查找（缺陷清单 P1/P3/P4）；错误隔离（D15）。
- **兼容性注意**：SPT5 内部配置依赖（C2）逐项对齐；加载期从 Preload+1 起步，若被 SPT post-DB 覆盖则改 PostLoad 并记录。
- **部署映射参考**：MO2 overlay 根 → 游戏根 `SPT_5xx`；服务端 mod → `SPT_Runtime\user\mods\`；客户端静态件 → `BepInEx\plugins\`（本版仅启动器背景）。
