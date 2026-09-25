# data/softcore 数据出处（Provenance）

本目录的 JSON 数据由源 mod（只读）的 TypeScript 资产生成，生成脚本随仓库归档，可复现。

## 数据文件

| 文件 | 内嵌资源名 | 源文件 |
|---|---|---|
| `fleamarket.json` | `InescapableTarkovsSoftcore.softcore.fleamarket.json` | 源 mod `src/assets/fleamarket.ts`、`src/assets/keys.ts`、`src/assets/itemBaseClasses.ts` |
| `scavcase.json` | `InescapableTarkovsSoftcore.softcore.scavcase.json` | 源 mod `src/assets/scavcase.ts` |
| `crafting-rebalance.json` | `InescapableTarkovsSoftcore.softcore.crafting-rebalance.json` | 源 mod `src/assets/productionAdjustments.ts` |
| `crafting-recipes.json` | `InescapableTarkovsSoftcore.softcore.crafting-recipes.json` | 源 mod `src/assets/recipes.ts`（`additionalCraftingRecipes`；`containerRecipes` 由 T08 `ContainerRecipes.cs` 承载，不重复外置） |

源 mod 路径（只读快照，v0.3.2）：
`E:\Game\EFT_Offline\Life_in_Norvinsk_v0.3.2\mods\[5]经济与制造系统大修-Softcore - 已AI优化\user\mods\odt-softcore\`

SURV 覆盖层（数值终态）：`...\mods\《生活在诺文斯克》难度：生存-重生地：中心区-主家：立交桥安全屋\user\mods\odt-softcore\`

SPT5 枚举来源：`E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime\SPTarkov.Server.Core.dll`（`ItemTpl` / `BaseClasses` 静态字段，类型 `MongoId`）。

## 生成命令

```powershell
# 1) 导出 SPT5 符号表（见 scripts/tools/dump-spt-symbols.cs 头部说明）
#    产出例如 D:\Temp\spt-symbols.json：{ "ItemTpl": {...}, "BaseClasses": {...} }
#    前置条件：临时控制台工程需 <FrameworkReference Include="Microsoft.AspNetCore.App" />
#    （否则枚举 GetExportedTypes() 会因缺失 Microsoft.Extensions.Hosting.Abstractions 抛 FileNotFoundException）。
#    本仓库生成所用符号表 SHA-256（386294 字节）：
#      7664EB279F71744B49D5AB71F25E48228C8AB48DA0DC151271E89A3A0687E616

# 2) 生成 fleamarket.json（脚本自定位仓库根与源 assets 目录）
powershell -NoProfile -ExecutionPolicy Bypass `
  -File scripts/tools/gen-fleamarket.ps1 `
  -SymbolsJson D:\Temp\spt-symbols.json
```

`scavcase.json` 为一次性迁移（T09 F1），其数据直接取自 `scavcase.ts`（无需符号解析）。

`crafting-rebalance.json` / `crafting-recipes.json` 由 `scripts/tools/gen-crafting.mjs`（Node）从源 TS 资产生成：

```powershell
# 依赖符号表 D:\Temp\spt-symbols.json（见 scripts/tools/dump-spt-symbols.cs；前置条件同上）
node scripts/tools/gen-crafting.mjs `
  --symbols D:\Temp\spt-symbols.json `
  --source "E:\Game\EFT_Offline\Life_in_Norvinsk_v0.3.2\mods\[5]经济与制造系统大修-Softcore - 已AI优化\user\mods\odt-softcore\src"
```

生成器把 `productionAdjustments.ts` 的每条 `adjust` 闭包以「录制代理」执行为声明式 ops
（`count` / `setAllCounts` / `setCount` / `replaceTemplate` / `setAreaLevel` / `replaceRequirements` / `pushRequirement`）；
`recipes.ts` 的 `additionalCraftingRecipes` 为纯数据，直接序列化。脚本自检 `ItemTpl` 符号缺失（输出 `MISSING SYMBOLS`）。

生成计数：`crafting-rebalance.json` 47 条调整 · `crafting-recipes.json` 12 条新增配方。

### 旧包目标配方 id 钉定（recipeId）

SPT 5.0 生产 DB 中下列 `endProduct` 各有 2 条配方，且 3 条的首条与源 3.11 的 `find` 命中不同，
故在生成器 `RECIPE_ID_PINS` 中钉定源目标配方 id（消除首条漂移，见 `docs/specs/delta-table.md` §4c）：

| endProduct | 钉定 recipeId | 5.0 首条（漂移后） |
|---|---|---|
| `60098b1705871270cd5352a1` | `61c77cc6fcc1673f08540e9b` | `67f4ec82690e0a541a021d3d` |
| `5448fee04bdc2dbc018b4567` | `5dc1f4d9e078d303d91b44c7` | `67f4ebb7d0fb51b8c705e80e` |
| `5d6fc87386f77449db3db94e` | `5dd3c5a67da3785e63275437` | `5dd3c9c8449c0c31795b0f0b` |
| `590a3b0486f7743954552bdb` | 无需钉定 | `5ffcac4e1285295b7441ee01`（与源一致） |

## 符号解析与重命名映射

源 TS 使用 SPT 3.11 时代的枚举符号名，SPT5 有少量改名。脚本按两级解析：

1. 精确名匹配；
2. **去下划线归一化**匹配（唯一命中时采用），已处理：
   - `BaseClasses.MEDKIT` → `MED_KIT`
3. **显式别名**（源符号在 SPT5 已不存在时的正确替换）：
   - `ItemTpl.KEY_SHARED_BEDROOM_MARKED` → `62987dfc402c7f69bf010923`（SPT5 改名为 `KEY_SUBSTATION_MARKED`）

未命中符号会输出 `MISSING SYMBOLS: ...`；当前应无输出。

## 计数（生成后自动打印）

whitelist 29 · actualBaseClasses 111 · fleaBarterRequestWhitelist 22 · requestWhitelist 16 ·
fleaListingsWhitelistHandBook 20 · pacifistFenceItemBaseWhitelist 17 · bsgBlacklist 353 ·
itemBaseClasses 93 · questKeys 57 · markedKeys 7

> 注：`scavcase.json` 由 T09 迁移；`fleamarket.json` 由 T10 迁移，重命名修复见 G4。
