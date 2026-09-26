using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>
/// 反馈 3 / 8：构建脚本的 copy-if-missing 策略与启动器背景移除（脚本级证据）。
/// </summary>
public class BuildScriptTests
{
    [Fact]
    public void Script_UsesCopyIfMissing_ForConfigAndData()
    {
        var script = File.ReadAllText(ScriptPath());

        // config.json：仅当目标不存在时拷贝
        Assert.Contains("$configDest", script, StringComparison.Ordinal);
        Assert.Contains("if (-not (Test-Path -LiteralPath $configDest))", script, StringComparison.Ordinal);
        // data/**：逐文件 copy-if-missing
        Assert.Contains("function Copy-DataTree", script, StringComparison.Ordinal);
        // DLL 始终覆盖
        Assert.Contains("Copy-Item -LiteralPath $dllPath -Destination $modDir -Force", script, StringComparison.Ordinal);
        // 不再整树删除 overlay（保留玩家编辑）
        Assert.DoesNotContain("Remove-Item -LiteralPath $overlayRoot -Recurse", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Script_NoLongerHandlesLauncherBackground()
    {
        var script = File.ReadAllText(ScriptPath());

        Assert.DoesNotContain("customBackground", script, StringComparison.Ordinal);
        Assert.DoesNotContain(@"assets\launcher", script, StringComparison.Ordinal);
        Assert.DoesNotContain(@"SPT_Data\images\launcher", script, StringComparison.Ordinal);
    }

    private static string ScriptPath() => Path.Combine(RepoRoot(), "scripts", "build.ps1");

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "InescapableTarkovsSoftcore.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }
}
