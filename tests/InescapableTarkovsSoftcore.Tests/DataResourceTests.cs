using InescapableTarkovsSoftcore.Features;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>
/// 反馈 6：数据文件「磁盘优先、内嵌兜底」。经 <see cref="EmbeddedJsonResource{T}"/> 的
/// baseDirectory 注入口在临时目录验证；磁盘缺失或解析失败 → 告警并回落（不崩）。
/// </summary>
public class DataResourceTests
{
    private const string RelativePath = "data/antigravArmbands/armbands.json";

    [Fact]
    public void DiskFile_TakesPrecedenceOverEmbedded()
    {
        var dir = NewTempDir();
        Write(dir, RelativePath, """[{"id":"000000000000000000000001","name":"disk","weight":1.5}]""");

        var warnings = new List<string>();
        var result = EmbeddedJsonResource<List<ArmbandEntry>>.Load(
            FeatureTables.ArmbandsResourceName, RelativePath, dir, warnings);

        Assert.Single(result);
        Assert.Equal("disk", result[0].Name);
        Assert.Empty(warnings);
    }

    [Fact]
    public void MissingDiskFile_FallsBackToEmbedded_AndWarns()
    {
        var dir = NewTempDir();
        var warnings = new List<string>();

        var result = EmbeddedJsonResource<List<ArmbandEntry>>.Load(
            FeatureTables.ArmbandsResourceName, RelativePath, dir, warnings);

        Assert.Equal(22, result.Count);
        Assert.Contains(warnings, warning => warning.Contains("数据文件缺失", StringComparison.Ordinal));
    }

    [Fact]
    public void BrokenDiskFile_FallsBackToEmbedded_AndWarns()
    {
        var dir = NewTempDir();
        Write(dir, RelativePath, "{ not-json");
        var warnings = new List<string>();

        var result = EmbeddedJsonResource<List<ArmbandEntry>>.Load(
            FeatureTables.ArmbandsResourceName, RelativePath, dir, warnings);

        Assert.Equal(22, result.Count);
        Assert.Contains(warnings, warning => warning.Contains("解析失败", StringComparison.Ordinal));
    }

    private static void Write(string baseDir, string relativePath, string content)
    {
        var path = Path.Combine(baseDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "its-data-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
