using System.Text.Json.Serialization;

namespace InescapableTarkovsSoftcore.Features;

/// <summary>
/// 功能查找表访问入口。表文件**随包发货**（mod 目录 <c>data/**</c>，玩家可编辑）并以
/// EmbeddedResource 兜底：磁盘优先、缺失/解析失败回落内嵌。运行时模块与测试读同一来源。
/// </summary>
public static class FeatureTables
{
    internal const string ArmbandsResourceName = "InescapableTarkovsSoftcore.antigravArmbands.armbands.json";

    internal const string BackpacksResourceName = "InescapableTarkovsSoftcore.backpacks.backpacks.json";

    internal const string ArmbandsRelativePath = "data/antigravArmbands/armbands.json";

    internal const string BackpacksRelativePath = "data/backpacks/backpacks.json";

    /// <summary>G4：22 款反重力臂章（id → 减重后的重量）。</summary>
    public static IReadOnlyList<ArmbandEntry> LoadArmbands(List<string>? warnings = null) =>
        ReadList<ArmbandEntry>(ArmbandsResourceName, ArmbandsRelativePath, warnings);

    /// <summary>G5：43 条背包扩容目标（id → 网格宽高）。</summary>
    public static IReadOnlyList<BackpackEntry> LoadBackpacks(List<string>? warnings = null) =>
        ReadList<BackpackEntry>(BackpacksResourceName, BackpacksRelativePath, warnings);

    /// <summary>G4 便捷属性（无告警收集）。</summary>
    public static IReadOnlyList<ArmbandEntry> Armbands => LoadArmbands();

    /// <summary>G5 便捷属性（无告警收集）。</summary>
    public static IReadOnlyList<BackpackEntry> Backpacks => LoadBackpacks();

    private static IReadOnlyList<T> ReadList<T>(string resourceName, string relativePath, List<string>? warnings)
        where T : class =>
        EmbeddedJsonResource<List<T>>.Load(resourceName, relativePath, warnings: warnings);
}

/// <summary>反重力臂章条目。</summary>
public sealed class ArmbandEntry
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("weight")]
    public double Weight { get; init; }
}

/// <summary>背包扩容条目。</summary>
public sealed class BackpackEntry
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("cellsH")]
    public int CellsH { get; init; }

    [JsonPropertyName("cellsV")]
    public int CellsV { get; init; }
}
