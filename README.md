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
src/InescapableTarkovsSoftcore/         主工程（net10.0，库，SPT 服务端 mod）
  Config/                               配置模型与加载器
  Features/                             变换层接缝与编排器
tests/InescapableTarkovsSoftcore.Tests/ xUnit 测试工程
assets/launcher/bg.png                  启动器背景静态件（随 overlay 部署）
scripts/build.ps1                       构建 + overlay 组装脚本
build/overlay/                          构建产物（git 忽略）
release/                                发行归档约定目录
```

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

`scripts/build.ps1` 幂等产出（每次运行重建 `build/overlay/`）：

```
build/overlay/
└─ SPT_Runtime/
   ├─ SPT_Data/
   │  └─ images/
   │     └─ launcher/
   │        └─ bg.png    # G1 启动器背景（assets/launcher/bg.png 的拷贝；customBackground 关闭时省略）
   └─ user/
      └─ mods/
         └─ com.sammeow.inescapable-softcore/
            ├─ InescapableTarkovsSoftcore.dll
            ├─ config.json      # config/default-config.json 的逐字节拷贝
            └─ Resources/       # 数据资产占位（后续工单填充）
```

### 部署映射

| overlay 根 | 游戏实例 |
|---|---|
| `build/overlay/` | 游戏根 `E:\Game\EFT_Offline\SPT_5xx` |
| `build/overlay/SPT_Runtime/user/mods/com.sammeow.inescapable-softcore/` | `SPT_Runtime\user\mods\com.sammeow.inescapable-softcore\` |
| `build/overlay/SPT_Runtime/SPT_Data/images/launcher/bg.png` | `SPT_Runtime\SPT_Data\images\launcher\bg.png` |

部署经 MO2 overlay 目录 `[5]核心-Inescapable-Tarkovs-Softcore-<版本>` 向目标实例投影；不直接写入游戏目录。

#### 启动器背景路径核对（T07）

- SPT5 实例中启动器背景的真实路径为 `E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime\SPT_Data\images\launcher\bg.png`（同目录另有 `side_bear.png` / `side_scav.png` / `side_usec.png` 启动器 UI 资源）。
- 服务端 `SPTarkov.Server.Core.Utils.ImageRouteImporter`（`IOnLoad`）扫描 `SPT_Data/images/` 目录并按相对路径生成 `/files/<相对路径>` 路由，故该文件即 `/files/launcher/bg`——与旧 mod `ImageRouter.addRoute("/files/launcher/bg", …)` 的语义一致。
- 因此本工单采用**静态件覆盖**：`scripts/build.ps1` 在 `samuelTweaks.customBackground`（且 `samuelTweaks.enabled`）为真时，把 `assets/launcher/bg.png` 拷入 overlay 的上述路径；不注册服务端路由、不引入任何客户端 DLL（PerformanceTweaks 不在本工单范围）。
- 背景改动需重启 SPT 服务器（以及清理启动器缓存）后可见。

### 发行归档

`release/` 存放版本化发行包（`[5]核心-Inescapable-Tarkovs-Softcore-<版本>.zip`）。除 `.gitkeep` 与 `release/README.md` 外，目录内容被 git 忽略；每个发行以 git tag + 构建产物为准。

## 配置

### 文件位置

- 模板（仓库内受控源）：`config/default-config.json`
- 运行时实例（随 overlay 分发）：`SPT_Runtime\user\mods\com.sammeow.inescapable-softcore\config.json`（`scripts/build.ps1` 从模板拷贝）

若运行时缺失 `config.json`，mod 会从内嵌默认模板重建该文件并输出告警（不崩溃）。

### 格式与容错

- 支持 `//` 行注释与尾随逗号（JSONC）。
- 未知键：输出告警并忽略该键。
- 缺键：使用内置默认值。
- 值类型非法（如 `enabled` 写成字符串）：输出告警并回落默认值。
- 改动配置后需重启 SPT 服务器生效。

### 结构

```jsonc
{
  "general": { "enabled": true, "debug": false },   // 总开关；general.enabled=false 跳过全部功能组
  "samuelTweaks": {                                 // G1 Samuel's Tweaks
    "enabled": true,                                // 组开关；false 时整组跳过（零变更）
    "armorConflictFix": true,                       // 护甲弹挂冲突修复
    "lootableItems": {                              // 可掠夺开关（可分别控制）
      "armband": true,                              // 臂章（父类 5447e1d04bdc2dff2f8b4567）
      "meleeWeapons": true                          // 近战武器（类目 5b3f15d486f77432d0509248）
    },
    "magazineResize": {                             // 扩展弹匣缩格
      "enabled": true,
      "minCapacity": 10,                            // 容量下界（含）
      "maxCapacity": 50                             // 容量上界（含）
    },
    "customBackground": true                        // 构建期静态件（重启 + 重新打包生效）
  },
  "trueItems": { "enabled": true },                 // G2
  "noFirHideout": { "enabled": true },              // G3
  "antigravArmbands": { "enabled": true },          // G4
  "backpacks": { "enabled": true },                 // G5
  "softcore": { "enabled": true },                  // G6
  "raidDuration": {                                 // G7
    "enabled": true,
    "multiplier": 1.0                               // 全局战局时长倍率；1.0 = 原版，2.0 = 翻倍
  }
}
```

各组开关独立；`raidDuration.multiplier` 调整战局时长倍率（例如 `2.0` 使地图时限翻倍）。

G1 语义：

- `armorConflictFix`：含 `_props.RigLayoutName` 的弹挂甲 → `_props.BlocksArmorVest=false`（弹挂与护甲不再互斥）。
- `lootableItems.armband` / `meleeWeapons`：对应父类物品 → `Unlootable=false` 且 `UnlootableFromSide=[]`。
- `magazineResize`：弹匣（父类 `5448bc234bdc2d3c308b4569`）宽 1、高 >2、容量在 `[minCapacity, maxCapacity]` 时 → 高置 2、`ExtraSizeDown` 减 1（不跌破 0）。
- `customBackground`：仅构建期生效，控制是否把 `assets/launcher/bg.png` 拷入 overlay；运行时无操作。

## 状态

设计访谈（grill-with-docs）已收敛；规格书与工单进行中。版本自 `0.1.0` 起步。

