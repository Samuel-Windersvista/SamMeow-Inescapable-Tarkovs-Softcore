using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;

namespace InescapableTarkovsSoftcore.Config;

/// <summary>
/// 配置加载器：定位 mod 目录 → 读取（缺失则从内嵌默认模板重建）→ 两遍解析。
/// 第一遍做键集与类型校验（未知键 / 类型非法 → 告警并从待绑定节点移除），
/// 第二遍用 System.Text.Json 绑定到 <see cref="SoftcoreConfig"/>（缺键回落默认）。
/// 解析容忍 <c>//</c> 注释与尾随逗号。
/// </summary>
[Injectable(InjectionType.Singleton)]
public sealed class ConfigLoader(ModHelper modHelper, ISptLogger<ConfigLoader> logger)
{
    public const string ConfigFileName = "config.json";

    private const string EmbeddedResourceName = "InescapableTarkovsSoftcore.default-config.json";

    private static readonly string[] RootKeys =
    [
        "general",
        "samuelTweaks",
        "trueItems",
        "noFirHideout",
        "antigravArmbands",
        "backpacks",
        "softcore",
        "raidDuration"
    ];

    private static readonly string[] ToggleSections =
    [
        "samuelTweaks",
        "trueItems",
        "noFirHideout",
        "antigravArmbands",
        "backpacks",
        "softcore"
    ];

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = false
    };

    /// <summary>定位 mod 目录并加载配置（生产入口）。</summary>
    public ConfigLoadResult Load() => LoadFromDirectory(ResolveModDirectory());

    /// <summary>从指定目录加载 <c>config.json</c>；文件缺失时从内嵌模板重建并告警。</summary>
    public static ConfigLoadResult LoadFromDirectory(string modDirectory)
    {
        var warnings = new List<string>();
        var path = Path.Combine(modDirectory, ConfigFileName);
        string json;

        if (File.Exists(path))
        {
            json = File.ReadAllText(path);
        }
        else
        {
            warnings.Add($"[ITS] 未找到 {ConfigFileName}，已从内嵌默认模板重建：{path}");
            json = ReadDefaultConfigJson();

            try
            {
                Directory.CreateDirectory(modDirectory);
                File.WriteAllText(path, json, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                warnings.Add($"[ITS] 重建 {ConfigFileName} 失败：{ex.Message}");
            }
        }

        var result = Parse(json);
        return new ConfigLoadResult(result.Config, warnings.Concat(result.Warnings).ToList());
    }

    /// <summary>解析 JSONC 文本；纯函数，便于测试（不触碰文件系统或 SPT 运行时）。</summary>
    public static ConfigLoadResult Parse(string json)
    {
        var warnings = new List<string>();

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json, nodeOptions: null, documentOptions: DocumentOptions);
        }
        catch (JsonException ex)
        {
            warnings.Add($"[ITS] 配置 JSON 解析失败，使用全部默认值：{ex.Message}");
            return new ConfigLoadResult(new SoftcoreConfig(), warnings);
        }

        if (root is not JsonObject rootObject)
        {
            warnings.Add("[ITS] 配置根节点不是对象，使用全部默认值");
            return new ConfigLoadResult(new SoftcoreConfig(), warnings);
        }

        // 在副本上清洗，避免污染调用方传入的节点。
        var sanitized = (JsonObject)rootObject.DeepClone();
        ValidateRoot(sanitized, warnings);

        SoftcoreConfig config;
        try
        {
            config = sanitized.Deserialize<SoftcoreConfig>(SerializerOptions) ?? new SoftcoreConfig();
        }
        catch (JsonException ex)
        {
            warnings.Add($"[ITS] 配置绑定失败，使用默认值：{ex.Message}");
            config = new SoftcoreConfig();
        }

        return new ConfigLoadResult(config, warnings);
    }

    /// <summary>读取主工程内嵌的默认配置模板原文（JSONC）。</summary>
    public static string ReadDefaultConfigJson()
    {
        var assembly = typeof(ConfigLoader).Assembly;
        using var stream = assembly.GetManifestResourceStream(EmbeddedResourceName)
            ?? throw new InvalidOperationException($"缺少内嵌资源：{EmbeddedResourceName}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>优先用 ModHelper 定位 mod 目录；失败则回退程序集所在目录。</summary>
    private string ResolveModDirectory()
    {
        var assembly = typeof(ConfigLoader).Assembly;

        try
        {
            var resolved = modHelper.GetAbsolutePathToModFolder(assembly);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved;
            }
        }
        catch (Exception ex)
        {
            logger.Warning($"[ITS] ModHelper 定位 mod 目录失败，回退程序集目录：{ex.Message}");
        }

        var directory = Path.GetDirectoryName(assembly.Location);
        return string.IsNullOrWhiteSpace(directory) ? AppContext.BaseDirectory : directory;
    }

    private static void ValidateRoot(JsonObject root, List<string> warnings)
    {
        foreach (var (key, _) in root.ToList())
        {
            if (!RootKeys.Contains(key))
            {
                warnings.Add($"[ITS] 未知配置键 \"{key}\"，已忽略");
                root.Remove(key);
            }
        }

        ValidateSection(root, "general", warnings, ["enabled", "debug"]);
        foreach (var section in ToggleSections)
        {
            ValidateSection(root, section, warnings, ["enabled"]);
        }

        ValidateSection(root, "raidDuration", warnings, ["enabled"], ["multiplier"]);
    }

    /// <summary>
    /// 校验单个配置节：缺键保留默认；整节非法则移除整节（回落默认）；
    /// 未知叶键 / 叶键类型非法则移除该键（回落默认）并告警。
    /// </summary>
    private static void ValidateSection(
        JsonObject root,
        string sectionName,
        List<string> warnings,
        string[] boolKeys,
        string[]? numberKeys = null)
    {
        if (!root.TryGetPropertyValue(sectionName, out var node) || node is null)
        {
            return;
        }

        if (node is not JsonObject section)
        {
            warnings.Add($"[ITS] 配置节 \"{sectionName}\" 类型非法（应为对象），使用默认值");
            root.Remove(sectionName);
            return;
        }

        var allowed = new HashSet<string>(boolKeys, StringComparer.Ordinal);
        if (numberKeys is not null)
        {
            foreach (var key in numberKeys)
            {
                allowed.Add(key);
            }
        }

        foreach (var (key, value) in section.ToList())
        {
            if (!allowed.Contains(key))
            {
                warnings.Add($"[ITS] 未知配置键 \"{sectionName}.{key}\"，已忽略");
                section.Remove(key);
                continue;
            }

            var valid = boolKeys.Contains(key)
                ? value is JsonValue boolValue && boolValue.TryGetValue<bool>(out _)
                : value is JsonValue numberValue && numberValue.TryGetValue<double>(out _);

            if (!valid)
            {
                warnings.Add($"[ITS] 配置键 \"{sectionName}.{key}\" 类型非法，使用默认值");
                section.Remove(key);
            }
        }
    }
}
