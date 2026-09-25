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
data/                                   功能查找表（EmbeddedResource 源文件）
  trueitems/                            G2 True Items 六张表
  antigravArmbands/armbands.json        G4 反重力臂章表（22 款）
  backpacks/backpacks.json              G5 背包扩容表（43 条）
src/InescapableTarkovsSoftcore/         主工程（net10.0，库，SPT 服务端 mod）
  Config/                               配置模型与加载器
  Features/                             变换层接缝与编排器
    TrueItems/                          G2 True Items 查找表模型 / 加载器 / 应用器 / 模块
tests/InescapableTarkovsSoftcore.Tests/ xUnit 测试工程
scripts/build.ps1                       构建 + overlay 组装脚本
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

`scripts/build.ps1` 幂等产出（每次运行重建 `build/overlay/`）：

```
build/overlay/
└─ SPT_Runtime/
   └─ user/
      └─ mods/
         └─ com.sammeow.inescapable-softcore/
            ├─ InescapableTarkovsSoftcore.dll
            ├─ config.json      # config/default-config.json 的逐字节拷贝
            └─ Resources/       # 非嵌入式数据资产占位（G2 查找表已内嵌进 DLL，不在此）
```

### 部署映射

| overlay 根 | 游戏实例 |
|---|---|
| `build/overlay/` | 游戏根 `E:\Game\EFT_Offline\SPT_5xx` |
| `build/overlay/SPT_Runtime/user/mods/com.sammeow.inescapable-softcore/` | `SPT_Runtime\user\mods\com.sammeow.inescapable-softcore\` |

部署经 MO2 overlay 目录 `[5]核心-Inescapable-Tarkovs-Softcore-<版本>` 向目标实例投影；不直接写入游戏目录。

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
- 字典型键（如 `trueItems.overrides`）须为 JSON 对象；写成标量同样告警并回落默认值。
- 改动配置后需重启 SPT 服务器生效。

### 结构

```jsonc
{
  "general": { "enabled": true, "debug": false },   // 总开关；general.enabled=false 跳过全部功能组
  "samuelTweaks": { "enabled": true },              // G1
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
  "backpacks": { "enabled": true },                 // G5
  "softcore": { "enabled": true },                  // G6
  "raidDuration": {                                 // G7
    "enabled": true,
    "multiplier": 1.0                               // 全局战局时长倍率；1.0 = 原版，2.0 = 翻倍
  }
}
```

各组开关独立；`raidDuration.multiplier` 调整战局时长倍率（例如 `2.0` 使地图时限翻倍）。

### G2 True Items 查找表

`trueItems` 组的数值来自源 mod（IMM 覆盖层）的六张查找表，已作为内嵌资源随 DLL 分发，
运行时无需外部文件，也不投影到 overlay：

| 资源 | 条目 | 语义 |
|---|---|---|
| barter | List 175 | 杂物/以物易物物品堆叠 |
| clothing | List 22 | 衣物堆叠 |
| keycards | ParentList 1 | 门卡父类堆叠 = 1（不堆叠） |
| medicals | List 43，StackMult 2 | 仅对空医疗容器生效；注射器实得 8 |
| partsnmods | List 104 + ParentList 10 | 源 IMM 层 `Active=false`，不生效 |
| provisions | List 19 | 食品/饮料堆叠 |

应用规则：`StackMaxSize = 条目值 × StackMult`，并置 `StackMinRandom = 1`；未命中 `_id` 输出告警并跳过；
表 `Active=false` 时该文件零变更。`overrides` 在所有查找表之后应用：key 先按物品 `_id` 精确匹配，
匹配不到再按父类 `_parent` 批量匹配。

## 状态

### 已实现模块

- **G3 nofirhideout**（`NoFirHideoutModule`，Order=600）：遍历藏身处区域阶段的建造/升级需求
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

设计访谈（grill-with-docs）已收敛；规格书与工单进行中。版本自 `0.1.0` 起步。

