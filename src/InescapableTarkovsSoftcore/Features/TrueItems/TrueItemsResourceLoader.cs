namespace InescapableTarkovsSoftcore.Features.TrueItems;

/// <summary>
/// True Items 六张查找表加载器：磁盘优先（mod 目录 <c>data/trueitems/*.json</c>，玩家可编辑），
/// 缺失/解析失败回落内嵌资源。首次调用后缓存。
/// </summary>
public static class TrueItemsResourceLoader
{
    private const string ResourcePrefix = "InescapableTarkovsSoftcore.trueitems.";

    /// <summary>读取并缓存六张表；可选告警收集（磁盘缺失 / 解析失败 / 回落内嵌）。</summary>
    public static TrueItemsTables Load(List<string>? warnings = null) => new()
    {
        Barter = Read("barter", warnings),
        Clothing = Read("clothing", warnings),
        Keycards = Read("keycards", warnings),
        Medicals = Read("medicals", warnings),
        PartsnMods = Read("partsnmods", warnings),
        Provisions = Read("provisions", warnings)
    };

    private static TrueItemsTable Read(string name, List<string>? warnings) =>
        EmbeddedJsonResource<TrueItemsTable>.Load(
            $"{ResourcePrefix}{name}.json",
            $"data/trueitems/{name}.json",
            warnings: warnings);
}
