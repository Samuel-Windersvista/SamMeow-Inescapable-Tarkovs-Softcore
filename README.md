# Inescapable Tarkov's Softcore

SPT 5.0 服务端整合 mod。把 Life in Norvinsk v0.3.2 的六组功能 + 战局时长控制，自包含重写为单一 C# 服务端 mod。

## 目标

- 游戏线：SPT 5.0.0（build 47242 / EFT 1.1.5 / net10.0 / IL2CPP + BepInEx 6）
- 部署：MO2 overlay `[5]核心-Inescapable-Tarkovs-Softcore-<版本>` → 目标实例 `E:\Game\EFT_Offline\Inescapable Tarkov`

## 功能组

1. **Samuel's Tweaks**：护甲弹挂冲突修复 / 可掠夺臂章与近战 / 三格弹匣缩格 / 启动器背景
2. **True Items Redux**：物品真实堆叠上限
3. **noFirHideout**：藏身处建造移除 FIR 要求
4. **反重力臂章**：臂章减重 + 堆叠
5. **更大的背包**：背包网格扩容
6. **Softcore 经济与制造系统大修**：20 个 changer（安全容器 / 藏身处 / 经济 / 商人 / 制造 / 保险 / 杂项）
7. **战局时长控制**：全局倍率

## 文档

- `CONTEXT.md` — 术语表
- `docs/adr/` — 架构决策记录
- `docs/specs/` — 规格书快照（以 GitHub issue 为准）
- 规格书与工单：to-spec / to-tickets 产出（后续提交）

## 开发

### 环境要求

- .NET SDK 10.0.300+
- SPT 5.0.0 服务端运行时（提供引用程序集），默认路径 `E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime`

### 工程布局

```
InescapableTarkovsSoftcore.sln
config/default-config.json              受控默认配置模板（JSONC）
data/                                   数据表（随包发货到 mod 目录 data/**，并内嵌为兜底）
  trueitems/                            G2 True Items 六张表（见 data/MANIFEST.md）
  antigravArmbands/armbands.json        G4 反重力臂章表（37 款）
  backpacks/backpacks.json              G5 背包扩容表（38 条）
  softcore/scavcase.json                G6-B ScavCase 数据表（配方/区间/黑名单）
  softcore/fleamarket.json              G6-C 跳蚤市场数据表（白/黑名单等；见 softcore/MANIFEST.md）
  softcore/crafting-rebalance.json      G6-D 配方重平衡表（47 条 ops；源 productionAdjustments.ts）
  softcore/crafting-recipes.json        G6-D 新增配方表（12 条；源 recipes.ts additionalCraftingRecipes）
src/InescapableTarkovsSoftcore/         主工程（net10.0，库，SPT 服务端 mod）
  Config/                               配置模型与加载器
  Features/                             变换层接缝与编排器
    TrueItems/                          G2 True Items 查找表模型 / 加载器 / 应用器 / 模块
tests/InescapableTarkovsSoftcore.Tests/ xUnit 测试工程
scripts/build.ps1                       构建 + overlay 组装脚本
scripts/tools/                          数据再生成工具（gen-fleamarket.ps1 / gen-crafting.mjs / dump-spt-symbols.cs）
build/overlay/                          构建产物（git 忽略）
release/                                发行归档约定目录
```

查找表经 `EmbeddedResource` 嵌入主程序集（逻辑名见 `InescapableTarkovsSoftcore.csproj`），运行时模块与测试经 `FeatureTables` 读取同一资源，无需随 overlay 拷贝。

SPT 引用程序集目录由 MSBuild 属性 `SptRuntimeDir` 控制（默认值见 `Directory.Build.props`）。若 SPT 安装在别处：

```powershell
dotnet build -c Release -p:SptRuntimeDir="D:\Other\SPT_Runtime"
powershell -File scripts/build.ps1 -SptRuntimeDir "D:\Other\SPT_Runtime"
```

### 构建与测试

```powershell
dotnet build -c Release          # Release 构建
dotnet test                      # 运行单测
powershell -File scripts/build.ps1     # 组装 overlay 产物（拷贝配置模板为 config.json）
```

`scripts/build.ps1` 不再内联生成配置，而是把受控模板 `config/default-config.json` 逐字节拷贝为产物 `config.json`。

### 产物布局

`scripts/build.ps1` 幂等产出（不整树重建，采用升级安全的拷贝策略）：

```
build/overlay/
└─ SPT_Runtime/
   └─ user/
      └─ mods/
         └─ com.sammeow.inescapable-softcore/
            ├─ InescapableTarkovsSoftcore.dll   # 始终覆盖
            ├─ config.json                      # copy-if-missing（保留玩家编辑）
            └─ data/**                          # copy-if-missing，逐文件（保留玩家编辑）
               ├─ trueitems/*.json
               ├─ antigravArmbands/armbands.json
               ├─ backpacks/backpacks.json
               └─ softcore/{scavcase,fleamarket,crafting-*.json}
```

### 部署映射

| overlay 根 | 游戏实例 |
|---|---|
| `build/overlay/` | 游戏根 `E:\Game\EFT_Offline\SPT_5xx` |
| `build/overlay/SPT_Runtime/user/mods/com.sammeow.inescapable-softcore/` | `SPT_Runtime\user\mods\com.sammeow.inescapable-softcore\` |

部署经 MO2 overlay 目录 `[5]核心-Inescapable-Tarkovs-Softcore-<版本>` 向目标实例投影；不直接写入游戏目录。

#### 升级更新流程（保留玩家 config 与 data）

1. `scripts/build.ps1` 只覆盖 DLL，并在目标缺失时补 `config.json` / `data/**`（已存在则不覆盖）。
2. 若玩家改动过 `config.json` 或 `data/**`：升级后这些文件保持原样；如需采用新版默认值，手动删除对应文件后重跑 build.ps1（或从 `config/default-config.json` / 仓库 `data/` 手动取新值合并）。
3. 部署到 MO2 时以「更新现有 mod 目录」方式同步；不要先删除 mod 目录（否则玩家配置/数据一并丢失）。
4. 重启 SPT 服务器生效。

### 发行归档

`release/` 存放版本化发行包（`[5]核心-Inescapable-Tarkovs-Softcore-<版本>.zip`）。除 `.gitkeep` 与 `release/README.md` 外，目录内容被 git 忽略；每个发行以 git tag + 构建产物为准。

## 配置

### 文件位置

- 模板（仓库内受控源）：`config/default-config.json`
- 运行时实例（随 overlay 分发）：`SPT_Runtime\user\mods\com.sammeow.inescapable-softcore\config.json`（`scripts/build.ps1` 首次部署拷贝，之后不覆盖）

查找顺序：`config.json` → `config.jsonc`（替代文件名）。两者都不存在时，mod 会从内嵌默认模板重建 `config.json` 并输出告警（不崩溃）。

### 格式与容错

- 文件名：`config.json`（默认）或 `config.jsonc`（显式 JSONC 后缀）；两者同时存在时以 `config.json` 优先。
- 支持 `//` 行注释与尾随逗号（JSONC）。示例：
  ```jsonc
  {
    // 总开关
    "general": { "enabled": true, },
  }
  ```
- 未知键：输出告警并忽略该键。
- 缺键：使用内置默认值。
- 值类型非法（如 `enabled` 写成字符串）：输出告警并回落默认值。
- 字典型键（如 `trueItems.overrides`）须为 JSON 对象；写成标量同样告警并回落默认值。
- 改动配置后需重启 SPT 服务器生效。

### 结构

```jsonc
{
  "general": { "enabled": true, "debug": false },   // 总开关；general.enabled=false 跳过全部功能组
  "samuelTweaks": {                                 // G1 Samuel's Tweaks
    "enabled": true,                                // 组开关；false 时整组跳过（零变更）
    "armorConflictFix": true,                       // 护甲弹挂冲突修复
    "lootableItems": {                              // 可掠夺开关（可分别控制）
      "armband": true,                              // 臂章（SPT5 父类 ArmBand = 5b3f15d486f77432d0509248）
      "meleeWeapons": true                          // 近战武器（SPT5 类目 Knife = 5447e1d04bdc2dff2f8b4567）
    },
    "magazineResize": {                             // 扩展弹匣缩格
      "enabled": true,
      "minCapacity": 10,                            // 容量下界（含）
      "maxCapacity": 50                             // 容量上界（含）
    }
  },
  "trueItems": {                                   // G2
    "enabled": true,
    "overrides": {}                                 // id → 目标堆叠值；最后应用，覆盖内嵌查找表
  },
  "noFirHideout": { "enabled": true },              // G3
  "antigravArmbands": {                              // G4
    "enabled": true,
    "stackSize": 5,                                  // 全部臂章堆叠上限
    "overrides": {}                                  // 单品覆盖：臂章 id → 重量（最后应用）
  },
  "backpacks": {                                    // G5
    "enabled": true,
    "overrides": {}                                 // 逐背包覆盖：id → { cellsH/cellsV 绝对值, colsDelta/rowsDelta 增量 }
  },                                                // 例：{ "5df8a4d786f77412672a1e3b": { "rowsDelta": 2, "colsDelta": 1 } }（在原基础上加 2 行 1 列）
  "softcore": {                                     // G6（T08 起子结构对齐源 config.json5）
    "enabled": true,
    "secureContainersOptions": {                    // 安全容器：2×2 腰包起步 → Kappa
      "enabled": true,
      "progressiveContainers": { "enabled": true, "collectorQuestRedone": true },
      "biggerContainers": true
    },
    "stashOptions": {                               // 仓库：LVL1 起步，50/100/150/200 行，建造现金 ÷10
      "enabled": true,
      "progressiveStash": true,
      "biggerStash": true,
      "lessCurrencyForConstruction": true,
      "easierLoyalty": false
    },
    "hideoutContainers": {                          // 藏身处容器扩容 + SICC 增强（SURV 终态，cellsV×cellsH）
      "enabled": true,                              // 药品 10×10 / Holo 10×10 / 弹匣 7×10 / 物品 6×6 /
      "biggerHideoutContainers": true,              // 武器 6×7 / 钥匙工具 5×5 / THICC 武器 6×14 / THICC 物品 6×14
      "siccCaseBuff": true
    },
    "fasterCraftingTime": {                         // 制造加速（T09；配方时间 = ceil(原时间 / 倍率)）
      "enabled": true,
      "baseCraftingTimeMultiplier": 3,              // 全局基础倍率
      "hideoutSkillExpFix": { "enabled": true, "hideoutSkillExpMultiplier": 10 },
      "fasterMoonshineProduction": { "enabled": true, "baseCraftingTimeMultiplier": 0.3 },
      "fasterPurifiedWaterProduction": { "enabled": true, "baseCraftingTimeMultiplier": 0.3 },
      "fasterCultistCircle": { "enabled": true, "baseCraftingTimeMultiplier": 0.5 }
    },
    "fasterHideoutConstruction": {                  // 建设加速（阶段时间 = round(原时间 / 倍率)）
      "enabled": true,
      "hideoutConstructionTimeMultiplier": 50
    },
    "fuelConsumption": { "enabled": true, "fuelConsumptionMultiplier": 4 },   // 燃料流量倍率
    "fasterBitcoinFarming": {                       // 比特币农场
      "enabled": true,
      "setBitcoinPriceTo100k": false,
      "baseBitcoinTimeMultiplier": 1.3,
      "gpuEfficiency": 1.0
    },
    "scavCaseOptions": {                            // ScavCase（奖励池过滤 + 区间/配方重做 + 速度）
      "enabled": true,
      "betterRewards": true,
      "fasterScavcase": { "enabled": true, "speedMultiplier": 0.5 },
      "rebalance": true
    },
    "allowGymTrainingWithMusclePain": true,          // 严重肌肉疼痛下健身效率 75%
    "economyOptions": {                             // G6-C 经济（SURV 终态）
      "enabled": true,
      "disableFleaMarketCompletely": false,
      "priceRebalance": { "enabled": false, "itemFixes": true },
      "pacifistFleaMarket": {                       // 白名单/任务钥匙/标记钥匙 价格倍率 2/3/5
        "enabled": true,
        "whitelist": { "enabled": true, "priceMultiplier": 2 },
        "questKeys": { "enabled": true, "priceMultiplier": 3 },
        "markedKeys": { "enabled": true, "priceMultiplier": 5 }
      },
      "barterEconomy": {                            // 现金 5% / 价差 30 / 报价 5–13 / 非堆叠 1–4 / 最多 4 换 1
        "enabled": true,
        "cashOffersPercentage": 5,
        "barterPriceVariance": 30,
        "offerItemCount": { "min": 5, "max": 13 },
        "nonStackableCount": { "min": 1, "max": 4 },
        "itemCountMax": 4
      },
      "otherFleaMarketChanges": {                   // 1 级开放 / 仅全新品 / 价格 ×1.5
        "enabled": true,
        "sellingOnFlea": false,
        "fleaMarketOpenAtLevel": 1,
        "fleaPricesIncreased": 1.5,
        "fleaPristineItems": true,
        "onlyFoundInRaidItemsAllowedForBarters": false
      }
    },
    "traderChanges": {                              // G6-C 商人
      "enabled": true,
      "betterSalesToTraders": true,                 // 收价上调（忠诚 +5%/级）
      "alternativeCategories": true,                // Therapist 收窄 / Ragman 贵重 / Skier 信息
      "pacifistFence": { "enabled": true, "numberOfFenceOffers": 15 },
      "reasonablyPricedCases": true,
      "skierUsesEuros": true,
      "biggerLimits": { "enabled": true, "multiplier": 2.0 }
    },
    "insuranceChanges": {                           // G6-C 保险
      "enabled": true,
      "praporInsuranceChanges": { "enabled": true, "returnChance": 70, "returnTime": { "min": 240, "max": 360 }, "insuranceCostPercentage": 80 },
      "therapistInsuranceChanges": { "enabled": true, "returnChance": 60, "returnTime": { "min": 120, "max": 240 }, "insuranceCostPercentage": 50 }
    },
    "craftingChanges": {                            // G6-D 制造（数据内嵌 crafting-rebalance.json / crafting-recipes.json）
      "enabled": true,
      "craftingRebalance": true,                    // 30+ 条配方重平衡（按 endProduct 定位，仅命中首条）
      "additionalCraftingRecipes": true             // 12 条新增配方（3-b-TG / 肾上腺素 / L1 / AHF1-M / CALOK-B 等）
    },
    "otherTweaks": {                                // G6-D 杂项（SURV 终态）
      "enabled": true,
      "skillExpBuffs": false,                       // 技能经验增益（关闭）
      "signalPistolInSpecialSlots": true,           // 信号手枪可放入特殊槽
      "unexaminedItemsAreBack": false,              // 撤销「默认已检视」（关闭）
      "fasterExamineTime": true,                    // 检视时间固定 0.2s
      "removeBackpackRestrictions": true,           // 移除背包/容器过滤限制
      "removeDiscardLimit": true,                   // 移除丢弃限制
      "reshalaAlwaysHasGoldenTT": true,             // Reshala 必带金色 TT
      "biggerAmmoStacks": { "enabled": true, "stackMultiplier": 5 },  // 弹药堆叠 ×5（无重量/Boss 补偿）
      "vestsBlockArmor": false,                     // false = 弹挂与护甲不冲突（与 G1 修复同向）
      "questChanges": true,                         // 任务变更（仅 Crisis + Drip-Out）
      "removeRaidItemLimits": true,                 // 移除战局内物品限制
      "biggerCurrencyStacks": false,                // 货币堆叠（关闭）
      "smallContainersInSpecialSlots": false        // 小型容器特殊槽（关闭）
    }
  },
  "raidDuration": {                                 // G7
    "enabled": true,
    "multiplier": 1.0                               // 全局战局时长倍率；1.0 = 原版，2.0 = 翻倍
  }
}
```

各组开关独立；`raidDuration.multiplier` 调整战局时长倍率（例如 `2.0` 使地图时限翻倍）。`softcore` 段随 G6 串行链（T08–T11）逐步扩展，子开关默认值 = 旧包 SURV 终态。

> 倍率方向语义（源真实行为，非笔误）：`faster*` 时间参数实现为 `时间 / 倍率`，故 `0.3` → ×3.33、`0.5` → ×2（时间变长）。涉及 `fasterMoonshineProduction` / `fasterPurifiedWaterProduction` / `fasterCultistCircle` / `scavCaseOptions.fasterScavcase`。这是旧包终态行为，T09 审查确认保留。

### G6-D 制造与杂项语义

- **顺序约束（制造）**：`CraftingChangesChanger` 注册在 `FasterCraftingTimeChanger` 之后，故新增配方的 `productionTime` 保持源值，不被全局 ÷3（与源 `Softcore.ts` 应用顺序等效，测试已钉住）。
- **配方重平衡**：数据 = 内嵌 `crafting-rebalance.json`（47 条，源 `productionAdjustments.ts` 的闭包录制为声明式 ops：`count` / `setAllCounts` / `setCount` / `replaceTemplate` / `setAreaLevel` / `replaceRequirements` / `pushRequirement`）。按 `endProduct` 定位，排除 `ChristmasIllumination`（源 3.11 的 `CHRISTMAS_TREE`）；`find` 语义 = 仅命中首条。
- **唯一性兜底（B3）**：仅校验本 mod 自带的新增配方资源（重复 `endProduct` 告警并去重）。SPT5 原版同一 `endProduct` 存在合法的多配方（例如同一物品在厨房/营养站各一条、同站不同耗时两条），故不对原版表做全局去重。
- **`vestsBlockArmor=false`**：源 TS 的守卫写法为 `if (config.vestsBlockArmor)`（语义反转缺陷）；本实现按 SURV 终值语义落地——`false` 表示含 `RigLayoutName` 的弹挂甲 `BlocksArmorVest=false`（弹挂与护甲不冲突，与 G1 修复同向）。置 `true` 则不改动。
- **弹药堆叠**：父类为 Ammo 且 `StackMaxSize≠0` → `×stackMultiplier`；SURV 覆盖已移除 BASE 的 Boss 弹药重量补偿（无 botConfig 依赖）。
- **任务变更（按 id 匹配）**：SPT 5.0 生产 DB 的 `quests.name` 是本地化键（`<id> name`），按名匹配会静默失效，故按 id：Drip-Out 4 个任务（`6613f300…` / `6613f307…` / `66151401…` / `6615141b…`）设 `HandoverItem=10` / `CounterCreator=20`（源终值；5.0 原版 50/100）；收藏家任务按 id `5c51aac186f77432ea65c552` 重做。Crisis 在 5.0 仅 1 条 `AvailableForStart`（Level 条件已被 BSG 移除），源 `+30` 不可复现 → 告警跳过。SURV 覆盖已移除 circulate 与 colleagues3。
- **未迁移/关闭项**：`skillExpBuffs`、`unexaminedItemsAreBack`、`biggerCurrencyStacks`、`smallContainersInSpecialSlots` 默认关闭（SURV）

### G2 True Items 查找表

`trueItems` 组的数值来自源 mod（IMM 覆盖层）的六张查找表，随包发货到 mod 目录 `data/trueitems/`
（玩家可编辑）并内嵌于 DLL 作为兜底：磁盘优先、缺失或解析失败告警并回落（不崩溃）。

| 资源 | 条目 | 语义 |
|---|---|---|
| barter | List 184 | 杂物/以物易物物品堆叠 |
| clothing | List 28 | 衣物堆叠 |
| keycards | ParentList 1 | 门卡父类堆叠 = 1（不堆叠） |
| medicals | List 43，StackMult 2 | 仅对空医疗容器生效；注射器实得 8 |
| partsnmods | List 104 + ParentList 10 | 源 IMM 层 `Active=false`，不生效 |
| provisions | List 25 | 食品/饮料堆叠 |

应用规则：`StackMaxSize = 条目值 × StackMult`，并置 `StackMinRandom = 1`；未命中 `_id` 输出告警并跳过；
表 `Active=false` 时该文件零变更。`overrides` 在所有查找表之后应用：key 先按物品 `_id` 精确匹配，
匹配不到再按父类 `_parent` 批量匹配。

- **G1 语义**：
  - `armorConflictFix`：含 `_props.RigLayoutName` 的弹挂甲 → `_props.BlocksArmorVest=false`（弹挂与护甲不再互斥）。
  - `lootableItems.armband` / `meleeWeapons`：对应父类物品 → `Unlootable=false` 且 `UnlootableFromSide=[]`。
  - `magazineResize`：弹匣（父类 `5448bc234bdc2d3c308b4569`）宽 1、高 >2、容量在 `[minCapacity, maxCapacity]` 时 → 高置 2、`ExtraSizeDown` 减 1（不跌破 0）。

## 数据文件（玩家可编辑）

### 位置

- 运行时：`SPT_Runtime\user\mods\com.sammeow.inescapable-softcore\data\<相对路径>`（如 `data\softcore\fleamarket.json`）
- 随包源：仓库 `data/**`（`scripts/build.ps1` copy-if-missing 拷贝）

### 加载与容错

- **磁盘优先、内嵌兜底**：先读 mod 目录 `data/<相对路径>`；文件缺失或解析失败 → 输出告警并回落到 DLL 内嵌资源（不崩溃）。
- 数据文件同样支持 JSONC（`//` 注释、尾随逗号）。
- 改动后需重启 SPT 服务器生效；升级不会覆盖玩家已改动的数据文件。

### 可改内容示例（制造配方）

在 `data\softcore\crafting-recipes.json` 的 `additionalRecipes` 数组中增删条目（每条为一条 `HideoutProduction`）：

```jsonc
{
  "additionalRecipes": [
    // 在末尾追加一条（示例：自己造的配方）
    // 需保证 "_id" 与 "endProduct" 为 24 位 hex；重复 endProduct 会被告警并去重
    { "_id": "63da4dbee8fa73e225000099", "areaType": 7, "count": 1, "productionTime": 60, "endProduct": "<24-hex 物品 id>", "requirements": [] }
  ]
}
```

`data\softcore\crafting-rebalance.json` 的 `recipeAdjustments[].ops` 支持 `count` / `setAllCounts` / `setCount` / `replaceTemplate` / `setAreaLevel` / `replaceRequirements` / `pushRequirement`（语义见 `CraftingResourceLoader.cs`）。其他数据表（`trueitems/*`、`backpacks/backpacks.json`、`antigravArmbands/armbands.json`、`softcore/{scavcase,fleamarket}.json`）亦可按同结构编辑；出处与再生成见 `data/softcore/MANIFEST.md` 与 `scripts/tools/`。

## 状态

### 已实现模块

- **G3 noFirHideout**（`NoFirHideoutModule`，Order=600）：遍历藏身处区域阶段的建造/升级需求
  （`stages[].requirements` 与 `stages[].improvements[].requirements`），凡 `isSpawnedInSession`
  为 `true` 者置 `false`；无该键的需求不变。开关 `noFirHideout.enabled` 关闭时零变更。
- **G7 raidDuration**（`RaidDurationModule`，Order=700）：`raidDuration.multiplier`（默认 1.0）
  乘以各图时限 `LocationTable.<map>.Base.EscapeTimeLimit`；`1.0` 不变、非法（≤0）告警且零变更；
  开关关闭时零变更。

#### G7 字段核对结论（SPT 5.0.0 build 47242 运行时程序集）

- 时限字段位置：`SPTarkov.Server.Core.Models.Eft.Common.LocationBase.EscapeTimeLimit`
  （经 `LocationTable.<map>.Base` 访问）；服务器自身 `RaidTimeAdjustmentService.MakeAdjustmentsToMap`
  亦以该字段作为战局时长写入点。
- 类型：非空 `double`（`required`）。3.11 旧包字段名同为 `EscapeTimeLimit`，语义可平移。
- `EscapeTimeLimitCoop` / `EscapeTimeLimitPVE`：服务器未用于战局时长，本模块不改动。
- 注意：本仓库引用的 SP-Tushonka 源码快照与真实运行时的模型修饰符存在差异（快照为可空
  `double?`，运行时为 `required double`，且运行时模型广泛使用 `required` 成员）。实现以运行时程序集为准。
- 组合语义（G7）：服务器自身 `RaidTimeAdjustmentService.GetRaidAdjustments` 以
  `LocationTable.<map>.Base.EscapeTimeLimit` 为基准读取（此时该值已被本模块按倍率放大），计算后经
  `MakeAdjustmentsToMap` 回写同一字段。故本模块的倍率参与其计算而不会被抹除，两者可组合存活。
- 变更计数说明（G7）：计数遍历位置表全部条目，含非战局地图（如 `hideout` / `develop` / `terminal`）；
  这些条目无玩法影响，计数偏大属预期，仅作汇报用途。

设计访谈（grill-with-docs）已收敛；规格书与工单进行中。版本自 `0.1.0` 起步。

## 参考源（工作基准）

本项目的实现、审计与修复以以下来源为准（优先级从高到低；路径为本机示例）：

1. **SPT 5.0 服务端源码**：`E:\云文件\GitHub\SamMeow_SP-Tushonka_5xx_source_code` —— API / 表结构 / 生命周期语义参考。
2. **SPT5 运行时数据库**：`E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime\SPT_Data\database\templates\items.json` 等 —— 物品 id / 字段 / 数值的**唯一数据真值**；任何物品清单必须对照它全扫（失效 + 新增）。
3. **运行时程序集**：`E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime\*.dll` —— 编译引用与反射核对的真值（优先于源码快照；快照存在修饰符/可空性差异）。
4. **知识库笔记**：`knowledge/spt-kb/curated/api-notes-5.0/`（toolkit 仓库）—— 5.0 源码实读笔记（config / database / DI / 路由 / 存档 / mod 加载）。
5. **反编译参考**：3.11 版 `Assembly-CSharp`（历史行为对照，如排序/堆叠逻辑）；`SPT_5xx\EscapeFromTarkov_Data\il2cpp_data`（1.1.5 元数据；深挖客户端行为时经 Il2CppDumper）。
6. **运行日志位置**：经 MO2 运行时在实例 `overwrite\SPT_Runtime\user\logs\`（VFS 重定向）；不经 MO2 直跑时在游戏根 `SPT_Runtime\user\logs\`。

> 规则：物品清单 / 数值一律以 SPT5 数据库为准；行为语义以**实际执行文件**（代码 / DLL）为准，注释与源码快照仅供参考（已多次证实注释漂移）。
> 开发回望与教训：「`docs/retro-2026-09-26.md`」。

