using System.Text.Json;
using System.Text.Json.Serialization;

namespace InescapableTarkovsSoftcore.Features;

/// <summary>
/// 内嵌功能查找表访问入口。表文件以 EmbeddedResource 嵌入主工程，
/// 运行时模块与测试读同一资源，避免数值在代码与文档间漂移。
/// </summary>
public static class FeatureTables
{
    internal const string ArmbandsResourceName = "InescapableTarkovsSoftcore.antigravArmbands.armbands.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>G4：22 款反重力臂章（id → 减重后的重量）。</summary>
    public static IReadOnlyList<ArmbandEntry> Armbands { get; } = ReadList<ArmbandEntry>(ArmbandsResourceName);

    private static IReadOnlyList<T> ReadList<T>(string resourceName)
    {
        var assembly = typeof(FeatureTables).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"缺少内嵌资源：{resourceName}");
        return JsonSerializer.Deserialize<List<T>>(stream, Options) ?? [];
    }
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
