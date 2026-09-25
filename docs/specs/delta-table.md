# 差异表：六组功能默认值（基础 vs 沉浸感 vs 生存）

> 基线：新 mod 默认值 = 旧包「最终游玩态」。按本项目工具链约定（`mo2-assets-engine`：modlist.txt 顶部 = 最高优先级），生效顺序（低→高）：基础（BASE）→ 沉浸感增强（IMM）→ 难度：生存（SURV）。
> 来源：旧包 `E:\Game\EFT_Offline\Life_in_Norvinsk_v0.3.2\mods\`（只读快照，2026-09-26 提取）。数值以源文件为准；本表供规格阶段微调。
> 目录简称：**BASE** = 主目标目录；**IMM** = `《生活在诺文斯克》…沉浸感增强设置`；**SURV** = `《生活在诺文斯克》难度：生存…`。

---

## 1. 反重力臂章（AntigravArmbands）

生效链：BASE `[7]反重力臂章-AntigravArmbands` → IMM `《生活在诺文斯克》反重力袖章-AntigravArmbands沉浸感增强设置`（最高）。SURV 不含该项。
逻辑一致：`items[band]._props.Weight / StackMaxSize`。IMM 仅 `.ts`（无编译产物、无 package.json）。

| _id | 名称/色 | BASE weight | BASE stack | IMM weight | IMM stack |
|---|---|---|---|---|---|
| 5b3f16c486f7747c327f55f7 | White | -20 | 10 | -6 | 5 |
| 5b3f3af486f774679e752c1f | Blue | -40 | 10 | -6 | 5 |
| 5b3f3b0186f774021a2afef7 | Green | -60 | 10 | -6 | 5 |
| 5b3f3ade86f7746b6b790d8e | Red | -80 | 10 | -6 | 5 |
| 5b3f3b0e86f7746752107cda | Yellow | -100 | 10 | -6 | 5 |
| 619bdeb986e01e16f839a99e | RFARMY | — | — | -8 | 5 |
| 619bdf9cc9546643a67df6f8 | UNTAR | — | — | -8 | 5 |
| 619bddffc9546643a67df6f0 | Train Hard | — | — | -8 | 5 |
| 619bdd8886e01e16f839a99c | BEAR | — | — | -10 | 5 |
| 619bdfd4c9546643a67df6fa | USEC | — | — | -10 | 5 |
| 660312cc4d6cdfa6f500c703 | The Unheard | — | — | -10 | 5 |
| 619bde3dc9546643a67df6f2 | Kiba Arms | — | — | -10 | 5 |
| 619bddc6c9546643a67df6ee | DEADSKUL | — | — | -10 | 5 |
| 60b0f988c4449e4cb624c1da | Evasion | — | — | -15 | 5 |
| 619bc61e86e01e16f839a999 | Alpha | — | — | -15 | 5 |
| 619bde7fc9546643a67df6f4 | Labs | — | — | -15 | 5 |
| 619bdef8c9546643a67df6f6 | TerraGroup | — | — | -15 | 5 |
| 664a5480bfcc521bad3192ca | ARENA | — | — | -15 | 5 |
| 67614b3ab8c060ebb204b106 | Khorovod | — | — | -15 | 5 |
| 5f9949d869e2777a0e779ba5 | Rivals 2020 | — | — | -15 | 5 |
| 67614b542eb91250020f2b86 | Prestige 1 | — | — | -20 | 5 |
| 67614b6b47c71ea3d40256d7 | Prestige 2 | — | — | -25 | 5 |

**最终态**：22 款，-6 ~ -25 kg（色阶分级），堆叠 5。（BASE：5 款 / -20 ~ -100 / 堆叠 10。）

---

## 2. 更大的背包（BetterBackpacks）

生效链：BASE `[7]更大的背包-BetterBackpacks` → IMM `《生活在诺文斯克》背包容量扩展-BetterBackpacks沉浸感增强设置`（最高）。两版均改 `items[id]._props.Grids[0]._props.cellsH/cellsV`。
条目数：BASE 44（含 1 条重复）→ IMM 43；共同项 35。

### 2a 两版共有项（BASE → IMM：h×v / removeFilters）

| itemID | 名称 | BASE | IMM |
|---|---|---|---|
| 5df8a4d786f77412672a1e3b | 6Sh118 raid backpack | 6×13 / false | 8×9 / true |
| 5b44c6ae86f7742d1627baea | ANA Beta 2 | 6×7 / false | 7×7 / true |
| 545cdae64bdc2d39198b4568 | Camelbak TriZip (Foliage) | 6×7 / false | 7×8 / true |
| 66b5f22b78bbc0200425f904 | Camelbak TriZip (Multicam) | 6×7 / false | 7×8 / true |
| 5f5e467b0bc58666c37e7821 | Eberlestock F5 Switchblade | 6×7 / false | 7×8 / true |
| 544a5cde4bdc2d39388b456b | Flyye MBSS | 5×6 / false | 6×6 / true |
| 618bb76513f5097c8d5aa2d5 | Gruppa 99 T20 Black | 6×6 / false | 7×7 / true |
| 619cf0335771dd3c390269ae | Gruppa 99 T20 MC | 6×6 / false | 7×7 / true |
| 628e1ffc83ec92260c0f437f | Gruppa 99 T30 | 6×7 / false | 7×8 / true |
| 62a1b7fbc30cfa1d366af586 | Gruppa 99 T30 MC | 6×7 / false | 7×8 / true |
| 60a272cc93ef783291411d8e | Hazard 4 Drawbridge | 6×6 / false | 7×8 / true |
| 60a2828e8689911a226117f9 | Hazard 4 Pillbox | 6×6 / false | 6×6 / true |
| 6034d103ca006d2dca39b3f0 | Hazard 4 Takedown (Black) | 5×9 / false | 5×9 / true |
| 6038d614d10cbf667352dd44 | Hazard 4 Takedown (MC) | 5×9 / false | 5×9 / true |
| 618cfae774bb2d036a049e7c | LBT-1476A (Woodland) | 6×6 / false | 6×7 / true |
| 67458794e21e5d724e066976 | LBT-1476A (Alpine) | 6×6 / false | 6×7 / true |
| 5e4abc6786f77406812bd572 | LBT-2670 Med | 6×10 / true | 6×7 / true |
| 5e9dcf5986f7746c417435b3 | LBT-8005A Day | 6×6 / false | 6×7 / true |
| 5f5e45cc5021ce62144be7aa | LolKek 3F Transfer | 4×6 / false | 5×6 / true |
| 5c0e774286f77468413cc5b2 | MR Blackjack 50 | 6×11 / false | 7×9 / true |
| 628bc7fb408e2b2e9c0801b1 | MR NICE COMM 3 BVS | 6×12 / true | 6×7 / true |
| 66a9f98f3bd5a41b162030f4 | Partisan's Bag | 6×8 / false | 6×5 / true |
| 59e763f286f7742ee57895da | Pilgrim tourist | 6×10 / false | 7×9 / true |
| 61b9e1aaef9a1b5d6a79899a | Santa | 6×10 / false | 7×9 / true |
| 5ab8ebf186f7742d8b372e80 | SSO Attack 2 | 6×10 / false | 7×9 / true |
| 5e997f0b86f7741ac73993e2 | Sanitars bag | 6×7 / false | 6×6 / true |
| 56e335e4d2720b6c058b456d | Scav backpack | 6×6 / false | 6×7 / true |
| 639346cc1c8f182ad90c8972 | Tasmanian Tiger 35 | 6×10 / false | 7×9 / true |
| 5ab8ee7786f7742d8f33f0b9 | VKBO army bag | 4×5 / false | 5×6 / true |
| 66b5f247af44ca0014063c02 | Vertx Ready (Red) | 6×6 / false | 6×6 / true |
| 5ca20d5986f774331e7c9602 | WARTECH Berkut BB102 | 6×6 / false | 6×7 / true |
| 5ab8f04f86f774585f4237d8 | Tactical sling bag | 4×4 / false | 5×5 / true |
| 56e33634d2720bd8058b456b | Duffle bag | 6×4 / false | 6×6 / true |
| 56e33680d2720be2748b4576 | Transformer Bag | 5×4 / false | 5×5 / true |

### 2b 仅基础项（最终不生效，IMM 已删除）

| itemID | 名称 | BASE h×v |
|---|---|---|
| 656f198fb27298d6fd005466 | Direct Action Dragon Egg Mark II | 2×2 |
| 674da9cf0cb4bcde7103c07b | MR Terraframe (Christmas) | 2×5 |
| 674da107c512807d1a0e7436 | MR Terraframe (Olive) | 2×5 |
| 656e0436d44a1bb4220303a0 | MR SATL Bridger (Foliage) | 6×2 |
| 67458730df3c1da90b0b052b | 5.11 RUSH 100 (Black) | 2×2 |
| 5f5e46b96bdad616ad46d613 | Eberlestock F4 Terminator | 5×4 |
| 5c0e805e86f774683f3dd637 | 3V Gear Paratus 3-Day | 5×5 |
| 6034d2d697633951dc245ea6 | Eberlestock G2 Gunslinger II | 3×5 |
| 5d5d940f86f7742797262046 | Oakley Mechanism (Black) | 4×4 |

### 2c 仅覆盖项（IMM 新增）

| itemID | 名称 | IMM h×v |
|---|---|---|
| 668bc5cd834c88e06b08b928 | 6Sh118 (Alpine) | 8×9 |
| 6673b1ac5cae0610f1079d71 | 6Sh118 (BLK) | 8×9 |
| 672e2e75b9082dbf88dd1dbd | MR NICE COMM 3 BVS | 6×7 |
| 672e2e75c076d2093b05c764 | MR NICE COMM 3 BVS | 6×7 |
| 6621b28d9411498998d408c3 | MR NICE COMM 3 BVS | 6×7 |
| 672e2e7563b1a22a3c7b3895 | Duffle bag | 6×6 |
| 672e2e754544ab54214fd56c | Duffle bag | 6×6 |
| 672e2e75d276dfa8dd76770c | Duffle bag | 6×6 |
| 672e2e75592eb3c91e248717 | Duffle bag | 6×6 |

### 2d 行为差异

- IMM 全部 43 条 `removeFilters: true`；BASE 仅 2 条（`628bc7fb…`、`5e4abc67…`）。
- IMM 新增 item 缺失保护（`!item` → 告警并跳过）；BASE 缺失 id 直接抛错。
- BASE 逐条重复警告注释 `WARNING MAKING THIS ANY BIGGER WILL CAUSE GRID OVERLAPS`；IMM 无。

**最终态**：IMM 版 43 条 —— 主流大背包上调（6Sh118 8×9、TriZip 7×8、Blackjack 7×9 等），**全部清空容器过滤**；MR NICE COMM 3（6×12→6×7）、Partisan（6×8→6×5）等被改为更保守尺寸。

---

## 3. True Items Redux（物品真实堆叠）

生效链：BASE `[10]物品真实堆叠-True Items Redux\config\` → IMM `《生活在诺文斯克》物品堆叠-True Items沉浸感增强设置\config\`（最高）。`StackMult` 语义：最终堆叠 = 条目值 × StackMult。

| 文件 | BASE 条目 | IMM 条目 | 状态 |
|---|---|---|---|
| barter.json | 134 | 175 | 值普遍下调 + 新增 48 / 删 7 |
| clothing.json | 22 | 22 | **完全一致** |
| keycards.json | 1 | 1 | **完全一致**（Keycard 堆叠 = 1，即不堆叠） |
| medicals.json | 31 | 43 | `StackMult` 1→**2**；删 11 / 增 23 |
| partsnmods.json | 104 (+10 父类) | 104 (+10 父类) | 内容一致，**`Active` true→false** |
| provisions.json | 13 | 19 | 改 1 / 删 2 / 增 8 |

### 3a barter.json 代表值（BASE→IMM）

AA Battery 15→5 · AG guitar pick 60→5 · Physical Bitcoin 20→5 · GP coin 20→15 · #FireKlean 10→3 · Cordura 10→3 · Bolts 15→8 · Pack of nails 15→8 · Screws 15→3 · Capacitors 15→5 · CPU fan 15→3 · LEDX 5→3 · Gas analyzer 5→2 · Crickent 12→12（不变）
新增 48 条含：Topographic survey maps(2) · Video cassette Cyborg Killer(2) · Portable Powerbank(3) · Microcontroller board(2) · KEKTAPE(4) · Intelligence folder(5) · Military flash drive(4) · Virtex(3) · Lega Medal(5) · Tamatthi kunai(4) · UHF RFID Reader(3) · Military circuit board(3) · Iridium thermal module(3) · Viibiin sneaker(2) 等
删除 7 条：NIXXOR lens · Electric drill · Horse figurine · Gas mask air filter · Hand drill · BakeEzy cook book · Advanced Electronic Materials textbook

### 3b medicals.json

IMM `StackMult: 2`、43 条：保留 20 个注射器；删 Morphine + 全部 10 个非注射药（AI-2、Analgin、Army/Aseptic bandage、CALOK-B、CAT、Esmarch、Golden Star、Salewa、Vaseline）；新增 23 条（22 个自定义 stim + `2A2-(b-TG)`）。
**待核**：22 条自定义 stim（`cheeta_stim`、`boar_stim`… `radioactiveblood_stim`）的 `_id` 为占位字符串（非 24 位 hex）→ 按 `_id` 精确匹配不生效。迁移按"匹配不到 = 不生效"复刻，或修正为正确 `_id`（待定）。

### 3c provisions.json

`Emergency Water Ration 3→4`；删 `Iskra ration pack`、`MRE ration pack`；新增 8：Tarker dried meat(5)、instant noodles(5)、RatCola(4)、Hot Rod(4)、TarCola(4)、Max Energy(4)、Ice Green tea(4)、Pevko Light(2)。

**最终态**：食品/杂物/药品堆叠被大幅压低并精细化（多数 2–5）；注射器实得堆叠 = 8（条目值 ×2）；**配件堆叠关闭**（partsnmods `Active:false`）；衣物、Keycard 不变。

---

## 4. Softcore（三层 config + 源文件覆盖）

生效链：BASE config → IMM config → **SURV config**（最高）。源码覆盖：SURV 目录含 3 个 `.ts` 覆盖（`SecureContainerOptionsChanger.ts` / `OtherTweaksChanger.ts` / `HideoutContainersChanger.ts`），与 IMM 对应文件逐行一致 → SURV ≡ IMM。**修正**：早前判断「IMM/SURV 无 HideoutContainersChanger 覆盖」有误——该文件确实存在（mtime 晚于 BASE），故藏身处容器尺寸取 SURV 覆盖值（见 §4b/§4c）。

### 4a 三列 config 差异表（仅差异 key）

| key 路径 | BASE | IMM | SURV（最终） |
|---|---|---|---|
| hideout.stashOptions.easierLoyalty | true | false | false |
| hideout.fasterBitcoinFarming.setBitcoinPriceTo100k | true | false | false |
| hideout.fasterBitcoinFarming.baseBitcoinTimeMultiplier | 2.0 | 1.5 | **1.3** |
| hideout.fasterBitcoinFarming.gpuEfficiency | 0.5 | 1 | 1 |
| hideout.fasterCraftingTime.baseCraftingTimeMultiplier | 100 | 3 | 3 |
| hideout.fasterCraftingTime.fasterMoonshineProduction.baseCraftingTimeMultiplier | 10 | 0.3 | 0.3 |
| hideout.fasterCraftingTime.fasterPurifiedWaterProduction.baseCraftingTimeMultiplier | 10 | 0.3 | 0.3 |
| hideout.fasterCraftingTime.fasterCultistCircle.baseCraftingTimeMultiplier | 10 | 0.5 | 0.5 |
| hideout.fasterHideoutConstruction.hideoutConstructionTimeMultiplier | 100 | 50 | 50 |
| hideout.fuelConsumption.fuelConsumptionMultiplier | 10 | 4 | 4 |
| hideout.scavCaseOptions.fasterScavcase.speedMultiplier | 10 | 0.5 | 0.5 |
| economy.priceRebalance.enabled | true | false | false |
| economy.pacifistFleaMarket.enabled | true | false | **true** |
| economy.pacifistFleaMarket.questKeys.priceMultiplier | 2 | 3 | 3 |
| economy.pacifistFleaMarket.markedKeys.priceMultiplier | 2 | 5 | 5 |
| economy.barterEconomy.cashOffersPercentage | 8 | 10 | **5** |
| economy.barterEconomy.barterPriceVariance | 40 | 30 | 30 |
| economy.barterEconomy.offerItemCount.min/max | 10/20 | 5/13 | 5/13 |
| economy.barterEconomy.nonStackableCount.min/max | 1/2 | 1/4 | 1/4 |
| economy.barterEconomy.itemCountMax | 2 | 4 | 4 |
| economy.otherFleaMarketChanges.fleaMarketOpenAtLevel | 5 | 10 | **1** |
| economy.otherFleaMarketChanges.fleaPricesIncreased | 1.3 | 1.5 | 1.5 |
| economy.otherFleaMarketChanges.fleaPristineItems | true | false | **true** |
| economy.otherFleaMarketChanges.onlyFoundInRaidItemsAllowedForBarters | true | false | false |
| traderChanges.pacifistFence.numberOfFenceOffers | 30 | 15 | 15 |
| traderChanges.skierUsesEuros | false | true | true |
| insuranceChanges.prapor.returnChance | 80 | 70 | 70 |
| insuranceChanges.prapor.returnTime.min/max | 0/0 | 240/360 | 240/360 |
| insuranceChanges.prapor.insuranceCostPercentage | 25 | 80 | 80 |
| insuranceChanges.therapist.returnTime.min/max | 0/0 | 120/240 | 120/240 |
| insuranceChanges.therapist.insuranceCostPercentage | 5 | 50 | 50 |
| otherTweaks.skillExpBuffs | true | false | false |
| otherTweaks.biggerAmmoStacks.stackMultiplier | 10 | 5 | 5 |
| otherTweaks.biggerAmmoStacks.botAmmoStackFix | true | （键不存在） | （键不存在） |
| otherTweaks.vestsBlockArmor | （键不存在） | false | false |
| otherTweaks.biggerCurrencyStacks | true | false | false |

三版一致项（不列出即无差异）：`secureContainersOptions.*`（渐进容器/收藏家重做/biggerContainers 全 true）· `stashOptions{progressiveStash,biggerStash,lessCurrencyForConstruction}` · `hideoutContainers.*` · `scavCaseOptions{betterRewards,rebalance}` · `allowGymTrainingWithMusclePain` · `traderChanges{betterSalesToTraders,alternativeCategories,reasonablyPricedCases,biggerLimits}` · `craftingChanges.*` · `therapist.returnChance(=60)` · `disableFleaMarketCompletely=false` · `sellingOnFlea=false` · `otherTweaks{signalPistolInSpecialSlots=true, fasterExamineTime=true, removeBackpackRestrictions=true, removeDiscardLimit=true, reshalaAlwaysHasGoldenTT=true, questChanges=true, removeRaidItemLimits=true, unexaminedItemsAreBack=false, smallContainersInSpecialSlots=false}`。

> SURV vs IMM 仅 5 处不同：`baseBitcoinTimeMultiplier 1.5→1.3`、`pacifistFleaMarket.enabled false→true`、`cashOffersPercentage 10→5`、`fleaMarketOpenAtLevel 10→1`、`fleaPristineItems false→true`。

### 4b 源码覆盖差异（BASE vs IMM/SURV）

**`SecureContainerOptionsChanger.ts`**（唯一差异在 `doCollectorQuestRedone()`）：
- BASE：需交 **2** 个 Gamma；任务等级 10。
- IMM/SURV：需交 **1** 个 Gamma（注释「诺文斯克预设」）；等级 **20**。

**`OtherTweaksChanger.ts`**：
- 移除 `botConfig` 导入与 Boss 弹药重量补偿（`doBiggerAmmoStacks` 只保留 `stackMultiplier`）。
- 新增 `vestsBlockArmor`：含 `RigLayoutName` 者 `BlocksArmorVest=false`（弹挂甲与护甲不冲突）。
- `doQuestChanges`：4 项 → 仅 Crisis(=30) + Drip-Out。
- 移除 `skipUnexaminedIDs` 集合。

**`HideoutContainersChanger.ts`**（SURV 覆盖；BASE 对照）：
- `doBiggerHideoutContainers()` 尺寸修正（tuple = cellsH × cellsV，源 TS 原始记法）：
  - 药品箱 10×10、Holodilnick 10×10、弹匣箱 H10·V7、钥匙工具 5×5（两层一致）
  - 物品箱 10×10 → **6×6**；武器箱 H6·V15（BASE）→ **H7·V6**（SURV）
  - **新增** THICC 武器箱 **H14·V6**、THICC 物品箱 **H14·V6**（SURV 新增条目）
- `doSiccCaseBuff()` 与 BASE 一致（Docs 允许清单 ∪ SICC 允许清单 ∪ 钥匙工具）。

### 4c 最终态（SURV）结论摘要

- 藏身处：制造 3× · 建设 50× · 燃料 4× · ScavCase 速度 ×0.5 · 比特币 GPU×1 / 基础 1.3 · 渐进式仓库（50/100/150/200 行；The Unheard 250 行源自 BASE `StashOptionsChanger.ts:108`，SURV 未覆盖，保留）
- G6-B 制造/建设/燃料/比特币/ScavCase/健身（T09）：制造全局 3×（配方时间 = ceil(原时间/3)，排除比特币/月光酒/纯净水）· 月光酒/纯净水 0.3、邪教圈 0.5 · 藏身处技能经验修复 hoursForSkillCrafting ÷10 · 建设 50× · 燃料 ×4 · 比特币时间 ÷1.3、GPU 效率 1.0、setBitcoinPriceTo100k 关 · ScavCase 父类黑名单 + 物品黑名单、价值区间与配方重做 · 健身效率 0.75。
  - 注意：`fasterScavcase.speedMultiplier=0.5` 在源 TS 中实现为 `round(时间 / 0.5)` = 时间 ×2（语义与「更快」相反）。本项目按「以实现为准」忠实复刻该行为，待 Overseer 裁决是否按 ×0.5 时间修正。
  - 同类方向语义（T09 审查确认保留，属源真实行为）：`fasterMoonshineProduction` / `fasterPurifiedWaterProduction` ÷0.3 ≈ 时间 ×3.33；`fasterCultistCircle` ÷0.5 = 时间 ×2。名称 `faster*` 但时间变长。
  - ScavCase 缺陷状态（T13 核销用）：**P1**（`ScavCaseOptionsChanger.ts:109` `doBetterRewards` 未完全优化）——本行已按 T09 F7 补齐买断规则 + 弹药箱 StackSlots 定价补丁（确定性部分）；**P3**（`ScavCaseOptionsChanger.ts:117-132` 弹药箱价按 `StackSlots` 逐项读取）——已于 F7 落地（`ScavCaseChanger.ResolveHandbookPrice`）；**P4**（数据硬编码）——已外置 `data/softcore/scavcase.json`（EmbeddedResource）。仍未复刻项：`ItemFilterService`（boss/reward 黑名单）与 `SeasonalEventService`（季节性）——5.0 core `ScavCaseRewardGenerator` 已代做，故未注入。
- G6-C 经济/商人/保险（T10）：跳蚤 1 级开放 · 仅全新品 · 价格 ×1.5 · 和平主义白名单（价 ×2/3/5）· 以物易物（现金 5%、价差 30、报价 5–13、非堆叠 1–4、最多 4 换 1）· 商人收价上调（忠诚 +5%/级，Peacekeeper 基准 35+7）· Therapist 收窄 / Ragman 贵重 / Skier 信息 · Fence 15 报价 · Skier 欧元化 · 购买上限 ×2 · 保险 Prapor 70%/80%/240–360、Therapist 60%/50%/120–240（另设 runInterval 10s、储存 30 天、不留附件 50%）。
  - 数据外置：`assets/fleamarket.ts` + `keys.ts` + `itemBaseClasses.ts` → `data/softcore/fleamarket.json`（EmbeddedResource）；计数 whitelist 29 / actualBaseClasses 111 / fleaBarterRequestWhitelist 22 / requestWhitelist 16 / fleaListingsWhitelistHandBook 20 / pacifistFenceItemBaseWhitelist 17 / bsgBlacklist 353 / itemBaseClasses 93 / questKeys 57 / markedKeys 7。出处与再生成命令见 `data/softcore/MANIFEST.md`（工具 `scripts/tools/`）。
  - `markedKeys` 的 `KEY_SHARED_BEDROOM_MARKED` 在 SPT5 改名为 `KEY_SUBSTATION_MARKED`（`62987dfc402c7f69bf010923`），已补入 → 7 项全量落地。
  - ragfair 报价数改为「Clear + `default` + 弹药箱父类键」同范围（5–13），保证所有物品类一致（SPT 先查父类键、再回退 `default`）。
  - Skier 任务奖励欧元化已实现（遍历 Skier 任务 `Rewards.Success`，RUB→EUR，`Math.ceil` 同步 `StackObjectsCount` 与 `reward.Value`）。
   - 未迁移项（记录）：`priceRebalance`（SURV 关闭，changer 未迁移，启用时告警）。
- G6-D 制造/杂项（T11，链道最后一票）：
  - **制造**：配方重平衡 47 条（源 `productionAdjustments.ts` → `data/softcore/crafting-rebalance.json`，闭包录制为声明式 ops）+ 新增 12 条配方（源 `recipes.ts` 的 `additionalCraftingRecipes` → `data/softcore/crafting-recipes.json`：3-b-TG / 肾上腺素 / L1 / AHF1 / CALOK / 检眼镜 / Zagustin / Obdolbos / OLOLO / Perfotran / Trimadol / Meldonin）。
  - **顺序约束**：`CraftingChangesChanger` 注册在 `FasterCraftingTimeChanger` 之后（源 `Softcore.ts` 顺序），新增配方 `productionTime` 保持源值（不被全局 ÷3）；测试钉住（肾上腺素 = 23，非 8）。
  - **配方定位**：按 `endProduct` + 排除 `ChristmasIllumination`（源 3.11 `CHRISTMAS_TREE` 在 SPT5 的对应枚举）；`find` 语义仅命中首条。
  - **唯一性兜底（B3）**：仅校验本 mod 自带配方资源的重复 `endProduct`（告警 + 去重）；不对 SPT5 原版表做全局去重——实测原版（排除 area21）存在 **13 处**合法重复 `endProduct`（如 `590a3b04…` 同站两条、`5448fee0…` 厨房/营养站各一条）。
  - **附加配方与源差异（记录）**：12 条新增配方中有 4 条 `endProduct` 与原版 MedStation 配方同名（`5ed515f6915ec335206e4152` AHF1 / `5e8488fa988a8701445df1e4` CALOK-B / `5c0e533786f7747fa23f4d47` Zagustin / `637b6251104668754b72f8f9` Perfotran）——源 mod 行为即「追加」而非替换，本实现忠实追加（SPT 允许同 `endProduct` 多配方）。
  - **杂项（SURV 终值）**：弹药堆叠 ×5（父类 Ammo 且 `StackMaxSize≠0`；无重量/Boss 补偿）· `vestsBlockArmor=false` ⇒ 含 `RigLayoutName` 弹挂甲 `BlocksArmorVest=false`（弹挂与护甲不冲突）· 移除背包过滤/丢弃限制/战局物品限制 · Reshala 金 TT · 任务仅 Crisis(`+30`)/Drip-Out(`10`/`20`) · 信号枪特殊槽 · 检视 0.2s · `unexaminedItemsAreBack=false`/`biggerCurrencyStacks=false`/`smallContainersInSpecialSlots=false`/`skillExpBuffs=false`。
  - **`vestsBlockArmor` 语义裁定**：源 TS 守卫为 `if (config.vestsBlockArmor)`（与注释/意图相反，属源码反转缺陷）；SURV 终值 `false` 且项目口径为「弹挂与护甲不冲突（与 G1 修复同向）」，故本实现取 `false ⇒ 应用修复`。置 `true` 不改动。
  - **C1 核对结论（5.x 物品 ID 有效性，SPT 5.0.0 build 47242 数据库）**：`fleamarket.json` 内 `bsgBlacklist` **353/353 全有效**；同批核对 `whitelist 29` / `questKeys 57` / `markedKeys 7` / `requestWhitelist 16`（物品 id）全有效，`actualBaseClasses 111` / `itemBaseClasses 93` / `pacifistFenceItemBaseWhitelist 17`（父类 id）全有效，`fleaListingsWhitelistHandBook 20`（手册类目 id）全有效；制造数据全部 `templateId` 与 2 个 `questId` 引用有效。**失效项：0**。
  - **旁证（非 C1 范围）**：`scavcase.json` 的 `parentBlacklist` 含 1 个 SPT5 已不存在的父类 id（`5d52cc5ba4b9367408500062`）——作为黑名单条目不匹配任何物品，惰性无害（T09 迁移遗留，保留）。
  - **数据外置**：`assets/productionAdjustments.ts` + `assets/recipes.ts` → `data/softcore/crafting-rebalance.json` / `crafting-recipes.json`（EmbeddedResource）；出处与再生成命令见 `data/softcore/MANIFEST.md`（工具 `scripts/tools/gen-crafting.mjs`）。
- 藏身处容器（SURV 覆盖终态，cellsV × cellsH）：药品 10×10 · Holodilnick 10×10 · 弹匣 7×10 · 物品 6×6 · 武器 6×7 · 钥匙工具 5×5 · THICC 武器 6×14 · THICC 物品 6×14
- 安全容器（SURV）：腰包 2×4（源 TS 注释「腰包是 2x4」）· Alpha 3×3 · Beta 3×4 · Epsilon 3×5 · Gamma 4×5 · Kappa 5×5
- 经济：和平主义跳蚤（1 级开放、仅全新品、价 ×1.5）· 以物易物（现金 5%、价差 30%、报价 5–13、最多 4 换 1）· priceRebalance 关
- 商人：收价上调 · Fence 15 报价 · Skier 欧元开 · 购买上限 ×2
- 保险：Prapor 70% 返还 / 240–360min / **保费 80%**（存疑）；Therapist 60% / 120–240 / 50%
- 杂项：弹药堆叠 ×5 · 技能经验 buff 关 · 货币堆叠关 · 弹挂甲冲突修复 · 收藏家 20 级 + 1 张 Gamma

---

## 5. 无覆盖层项

- **Samuel's Tweaks**：无覆盖层；默认 = 自身（护甲弹挂冲突修复 / 可掠夺臂章与近战 / 三格弹匣缩格 / 启动器背景）。
- **noFirHideout**：无覆盖层、无配置（纯逻辑：藏身处需求 `isSpawnedInSession=false`）。
- **战局时长控制**：新增功能，默认 1.0。

## 6. 不确定项

1. MO2 精确优先级语义以 UI 为准（本表按 modlist 行序 + 本项目工具链约定推断）。
2. 旧包 5/6 目标无编译 `.js`（仅 Antigrav 有）；运行时行为以源码为准。
3. True Items IMM 自定义 stim `_id` 为占位字符串（见 3b 待核）。
4. Softcore SURV 保险 Prapor `insuranceCostPercentage=80`（BASE 25）数值存疑；按最终态保留，若属笔误在规格阶段修正。
