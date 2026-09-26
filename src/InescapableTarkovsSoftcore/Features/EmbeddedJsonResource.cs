using System.Text.Json;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace InescapableTarkovsSoftcore.Features;

/// <summary>
/// JSON 数据表加载助手：**磁盘优先、内嵌兜底**。
/// <para>
/// 查找顺序：<c>&lt;mod 目录&gt;/&lt;relativeDiskPath&gt;</c>（玩家可编辑）→ 内嵌资源。
/// 磁盘文件缺失或解析失败 → 追加告警并回落内嵌（不抛异常）。首次调用后缓存。
/// </para>
/// <para>
/// 注册 SPT 的 <see cref="StringToMongoIdConverter"/>，使含 <c>MongoId</c> 字段的模型
/// （如 HideoutProduction / Requirement）可直接反序列化。
/// </para>
/// </summary>
internal static class EmbeddedJsonResource<T> where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new StringToMongoIdConverter() }
    };

    private static readonly Dictionary<string, Lazy<T>> Cache = new(StringComparer.Ordinal);

    /// <summary>mod 数据目录默认根：程序集所在目录（运行时即 mod 文件夹）。</summary>
    public static string ModDirectory =>
        Path.GetDirectoryName(typeof(EmbeddedJsonResource<T>).Assembly.Location) ?? AppContext.BaseDirectory;

    public static T Load(
        string resourceName,
        string? relativeDiskPath = null,
        string? baseDirectory = null,
        List<string>? warnings = null)
    {
        var key = $"{resourceName}|{relativeDiskPath}|{baseDirectory}";
        lock (Cache)
        {
            if (!Cache.TryGetValue(key, out var lazy))
            {
                lazy = new Lazy<T>(() => Read(resourceName, relativeDiskPath, baseDirectory, warnings));
                Cache[key] = lazy;
            }

            return lazy.Value;
        }
    }

    private static T Read(
        string resourceName,
        string? relativeDiskPath,
        string? baseDirectory,
        List<string>? warnings)
    {
        if (relativeDiskPath is not null)
        {
            var root = baseDirectory ?? ModDirectory;
            var path = Path.Combine(root, relativeDiskPath);

            if (File.Exists(path))
            {
                try
                {
                    using var file = File.OpenRead(path);
                    var fromDisk = JsonSerializer.Deserialize<T>(file, Options);
                    if (fromDisk is not null)
                    {
                        return fromDisk;
                    }

                    warnings?.Add($"[ITS] 数据文件解析为空，回落内嵌资源：{path}");
                }
                catch (Exception ex)
                {
                    warnings?.Add($"[ITS] 数据文件解析失败，回落内嵌资源：{path}（{ex.Message}）");
                }
            }
            else
            {
                warnings?.Add($"[ITS] 数据文件缺失，使用内嵌资源：{path}");
            }
        }

        var assembly = typeof(EmbeddedJsonResource<T>).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"缺少内嵌资源：{resourceName}");

        return JsonSerializer.Deserialize<T>(stream, Options)
            ?? throw new InvalidOperationException($"内嵌资源解析为空：{resourceName}");
    }
}
