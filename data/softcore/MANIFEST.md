# data/softcore 数据出处（Provenance）

本目录的 JSON 数据由源 mod（只读）的 TypeScript 资产生成，生成脚本随仓库归档，可复现。

## 数据文件

| 文件 | 内嵌资源名 | 源文件 |
|---|---|---|
| `fleamarket.json` | `InescapableTarkovsSoftcore.softcore.fleamarket.json` | 源 mod `src/assets/fleamarket.ts`、`src/assets/keys.ts`、`src/assets/itemBaseClasses.ts` |
| `scavcase.json` | `InescapableTarkovsSoftcore.softcore.scavcase.json` | 源 mod `src/assets/scavcase.ts` |

源 mod 路径（只读快照，v0.3.2）：
`E:\Game\EFT_Offline\Life_in_Norvinsk_v0.3.2\mods\[5]经济与制造系统大修-Softcore - 已AI优化\user\mods\odt-softcore\`

SURV 覆盖层（数值终态）：`...\mods\《生活在诺文斯克》难度：生存-重生地：中心区-主家：立交桥安全屋\user\mods\odt-softcore\`

SPT5 枚举来源：`E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime\SPTarkov.Server.Core.dll`（`ItemTpl` / `BaseClasses` 静态字段，类型 `MongoId`）。

## 生成命令

```powershell
# 1) 导出 SPT5 符号表（见 scripts/tools/dump-spt-symbols.cs 头部说明）
#    产出例如 D:\Temp\spt-symbols.json：{ "ItemTpl": {...}, "BaseClasses": {...} }

# 2) 生成 fleamarket.json（脚本自定位仓库根与源 assets 目录）
powershell -NoProfile -ExecutionPolicy Bypass `
  -File scripts/tools/gen-fleamarket.ps1 `
  -SymbolsJson D:\Temp\spt-symbols.json
```

`scavcase.json` 为一次性迁移（T09 F1），其数据直接取自 `scavcase.ts`（无需符号解析）。

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
