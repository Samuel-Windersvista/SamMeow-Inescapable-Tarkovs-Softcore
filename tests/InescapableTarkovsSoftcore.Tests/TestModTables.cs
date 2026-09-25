using InescapableTarkovsSoftcore.Features;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>测试用空表载体：不含真实 SPT 表；fake 模块不得触碰表。</summary>
internal static class TestModTables
{
    public static ModTables Empty { get; } = new(null!, null!, null!, null!, null!);
}
