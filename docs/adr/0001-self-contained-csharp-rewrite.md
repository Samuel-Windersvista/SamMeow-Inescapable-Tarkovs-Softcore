# ADR-0001：以单一自包含 C# 服务端 mod 重写替代六个 3.11 源 mod

- 状态：已接受（2026-09-26）
- 决策人：Overseer（用户）

## 背景

旧整合包 Life in Norvinsk v0.3.2 基于 SPT 3.11；六组功能均为 TypeScript 服务端 mod。目标实例为 SPT 5.0.0（build 47242，EFT 1.1.5，net10.0，IL2CPP + BepInEx 6）。3.x 的 TS 服务端 mod 在 5.x 已不可用；SPT 5 服务端 mod 为 C#（IModMetadata + IOnLoad、user/mods 布局、无 package.json）。

## 决策

将六组功能 + 战局时长控制在单一 C# 服务端 mod「Inescapable Tarkov's Softcore」中自包含重写：

- 源 mod 目录保持只读，不参与运行时；
- 目标实例不再安装源六件套（避免双重生效）；
- 仅适配 SPT 5，以目标实例版本（build 47242）为准，不做 4.x 兼容层。

## 备选方案

1. 逐个移植六个 mod 并共存 —— 否决：SPT5 下 TS 不可用、多 mod 冲突面大、调整入口分散。
2. 薄包装调度 —— 否决：无可行包装对象（源实现不可运行）。

## 后果

- 正面：单一调整入口；统一配置；可修复源实现已知缺陷。
- 负面：重写工作量集中在 Softcore（20 个 changer）；行为一致性依赖差异表与验收清单。
