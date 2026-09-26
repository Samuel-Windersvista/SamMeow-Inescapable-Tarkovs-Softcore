# data/ 数据出处（Provenance）

本目录为 mod 随包发货的查找表（**磁盘优先、内嵌兜底**：运行时读 mod 目录 `data/**`，缺失/解析失败回落 DLL 内嵌资源）。玩家可直接编辑磁盘副本。软核子集另有 `data/softcore/MANIFEST.md`。

## 数据文件与来源

| 文件 | 条目 | 来源 |
|---|---|---|
| `antigravArmbands/armbands.json` | 37 | 旧包 IMM 22 款 + SPT 5.0 新增 15 款（R1-D） |
| `backpacks/backpacks.json` | 38 | 旧包 IMM 43 条 − 9 条 5.x 失效 + 4 条 SPT 5.0 背包（R1-D） |
| `trueitems/barter.json` | 184 | 旧包 IMM 175 − 1 失效 + 10 新增（R1-D） |
| `trueitems/clothing.json` | 28 | 旧包 IMM 22 + 6 新增（R1-D） |
| `trueitems/keycards.json` | 1（父类） | 旧包 IMM（不变） |
| `trueitems/medicals.json` | 43（StackMult 2） | 旧包 IMM（不变；22 条自定义 stim 占位 `_id` 按 O1 不修正） |
| `trueitems/partsnmods.json` | 104 + 10 父类（`Active:false`） | 旧包 IMM（不变） |
| `trueitems/provisions.json` | 25 | 旧包 IMM 19 + 6 新增（R1-D） |
| `softcore/*.json` | — | 见 `data/softcore/MANIFEST.md` |

## R1-D 记录（T13 后扩展批）

依据：R1-A 覆盖审计（对照 SPT 5.0 `items.json`，5848 条）与 R1-B 容器诊断；用户批准「袖章按价格与获取难度原则 / 背包按建议执行 / TrueItems 全量（含杂物 10）」。

### 袖章（22 → 37，全 stack 5）

- 档位：-6×5 · -8×3 · -10×6 · -15×17 · -20×1 · -25×5。
- 新增 15：Beta（-15，同 Alpha 容器档）、剧情/成就 9 条（-15）、Purple discord（**-10，存疑档**：单价 2 万、单色促销名，取中档；如需可改 -6）、Prestige 3/4/5/6（-25，延续 P1/P2 链、封顶 -25）。
- 审计口径更正：SPT5 臂章父类 = `5b3f15d486f77432d0509248`（`ArmBand`）；`5447e1d04bdc2dff2f8b4567` 实为 `Knife`。
- 详见 `docs/specs/delta-table.md` §7.1。

### 背包（43 → 38）

- 删 9 条 5.x 失效 id：`668bc5cd834c88e06b08b928`、`6673b1ac5cae0610f1079d71`、`672e2e75b9082dbf88dd1dbd`、`672e2e75c076d2093b05c764`、`6621b28d9411498998d408c3`、`672e2e7563b1a22a3c7b3895`、`672e2e754544ab54214fd56c`、`672e2e75d276dfa8dd76770c`、`672e2e75592eb3c91e248717`。
- 新增 4（现状 H×V → 目标 H×V）：`68947a8ce4bf255d1b0ca759` TT Modular Pack 45 Plus 5×8→7×9；`68947ab5a733b1602007e2fe` MR 2 Day Assault Pack 5×6→6×7；`68947ad3e4bf255d1b0ca75c` MR NICE Frame Load Sling 5×7→5×6；`656ddcf0f02d7bcea90bf395` Tehinkom RK-PT-25 1×2→4×3（**存疑**）。
- **跳过**：MR Terraplane `56e294cdd2720b603a8b4575` —— SPT 5.0 网格 `6×50` 疑脏数据（异常超大），不纳入；待以游戏内实测复核。
- 详见 `docs/specs/delta-table.md` §7.2。

### True Items（全量）

- barter：删失效 `e44b40d309fa123643258996`（Vinyl record，5.0 无此 id）；新增 10（值 2–3，高值小众避免经济失衡）——见 delta-table §7.3。
- provisions：新增 6（值 3–5）。
- clothing：新增 6（堆叠 2）。
- medicals：维持 43；22 条自定义 stim 占位 `_id` 按 O1「匹配不到 = 不生效」，不做修正。

### 安全容器变体（反馈 4）

- family 父类 `5448bf274bdc2dfc2f8b456a`（`Port. container`），15 个子项。
- 变体映射：`665ee77c…`→4×5（Unheard 起步）、`676008db…`→5×5（SPT Developer 起步）、`68f8e04e…`/`68f117b8…`→4×5、`68d55968…`→2×4（腰包档）、`64f6f4c5…`→5×5（Kappa 档）。
- 显式跳过：Boss `5c0a7945…`（4×90）、Developer `5c0a5a59…`（10×60）、Theta/Tetta `664a55d8…`（赛事档）。
- 未知变体 → 告警不崩。
- 详见 `docs/specs/delta-table.md` §7.4。

### SamuelTweaks 父类 ID 错标修正

源 `LootTweak.ts:9-10` 臂章/近战父类对调错标；已按 SPT5 纠正（ArmBand=5b3f15d4…、Knife=5447e1d0…）。默认双开行为不变。见 delta-table §7.5。
