using System.Text.Json.Serialization;

namespace InescapableTarkovsSoftcore.Config;

/// <summary>功能组配置段基类：当前仅含启用开关，具体参数由各自工单扩展。</summary>
public class ModuleConfig
{
    /// <summary>该功能组开关。</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>G1 Samuel's Tweaks。</summary>
public sealed class SamuelTweaksConfig : ModuleConfig;

/// <summary>G2 True Items Redux 物品真实堆叠。</summary>
public sealed class TrueItemsConfig : ModuleConfig;

/// <summary>G3 藏身处建造免 FIR。</summary>
public sealed class NoFirHideoutConfig : ModuleConfig;

/// <summary>G4 反重力臂章。</summary>
public sealed class AntigravArmbandsConfig : ModuleConfig;

/// <summary>G5 更大的背包。</summary>
public sealed class BackpacksConfig : ModuleConfig;

/// <summary>G6 Softcore 经济与制造系统大修（子结构对齐源 config.json5）。</summary>
public sealed class SoftcoreModuleConfig : ModuleConfig
{
    /// <summary>安全容器选项（G6-A）。</summary>
    [JsonPropertyName("secureContainersOptions")]
    public SecureContainersOptions SecureContainersOptions { get; set; } = new();

    /// <summary>仓库选项（G6-A）。</summary>
    [JsonPropertyName("stashOptions")]
    public StashOptions StashOptions { get; set; } = new();

    /// <summary>藏身处容器选项（G6-A）。</summary>
    [JsonPropertyName("hideoutContainers")]
    public HideoutContainersOptions HideoutContainers { get; set; } = new();
}

/// <summary>安全容器选项（默认值 = SURV 终态）。</summary>
public sealed class SecureContainersOptions
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("progressiveContainers")]
    public ProgressiveContainersOptions ProgressiveContainers { get; set; } = new();

    /// <summary>放宽容器尺寸（Alpha 3×3 … Kappa 5×5）。</summary>
    [JsonPropertyName("biggerContainers")]
    public bool BiggerContainers { get; set; } = true;
}

/// <summary>渐进式安全容器选项。</summary>
public sealed class ProgressiveContainersOptions
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>收藏家任务重做（20 级可接 + 仅需 1 张 Gamma）。</summary>
    [JsonPropertyName("collectorQuestRedone")]
    public bool CollectorQuestRedone { get; set; } = true;
}

/// <summary>仓库选项（默认值 = SURV 终态）。</summary>
public sealed class StashOptions
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>新档从 LVL1 仓库起步。</summary>
    [JsonPropertyName("progressiveStash")]
    public bool ProgressiveStash { get; set; } = true;

    /// <summary>扩容至 50/100/150/200 行。</summary>
    [JsonPropertyName("biggerStash")]
    public bool BiggerStash { get; set; } = true;

    /// <summary>仓库建造现金需求 ÷10。</summary>
    [JsonPropertyName("lessCurrencyForConstruction")]
    public bool LessCurrencyForConstruction { get; set; } = true;

    /// <summary>降低阶段建造忠诚度要求（SURV 关闭）。</summary>
    [JsonPropertyName("easierLoyalty")]
    public bool EasierLoyalty { get; set; }
}

/// <summary>藏身处容器选项（默认值 = SURV 终态）。</summary>
public sealed class HideoutContainersOptions
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("biggerHideoutContainers")]
    public bool BiggerHideoutContainers { get; set; } = true;

    /// <summary>SICC 增强（合并 Docs 允许清单并允许钥匙工具）。</summary>
    [JsonPropertyName("siccCaseBuff")]
    public bool SiccCaseBuff { get; set; } = true;
}

/// <summary>G7 战局时长控制。</summary>
public sealed class RaidDurationConfig : ModuleConfig
{
    /// <summary>全局战局时长倍率；1.0 = 原版，2.0 = 翻倍。</summary>
    [JsonPropertyName("multiplier")]
    public double Multiplier { get; set; } = 1.0;
}
