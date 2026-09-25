using InescapableTarkovsSoftcore.Config;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace InescapableTarkovsSoftcore.Features.TrueItems;

/// <summary>
/// True Items Redux 纯逻辑应用器（无 I/O）：把内嵌查找表应用到 SPT 模板表。
/// 语义对齐源 TS（trueStack.ts）：
/// <list type="bullet">
///   <item>List 按 <c>_id</c> 精确匹配；ParentList 按 <c>_parent</c> 批量匹配（源实现仅取首个匹配，此处按工单「批量」语义修正）。</item>
///   <item><c>StackMaxSize = 条目值 × StackMult</c>，同时置 <c>StackMinRandom = 1</c>。</item>
///   <item>medicals 仅对空医疗容器（<c>MaxHpResource</c> 有值且 ≤ 0）生效。</item>
///   <item>表 <c>Active=false</c> → 整表零变更；未命中 id → 跳过 + 告警。</item>
///   <item>源 TS 的 <c>_props === undefined</c> 检查在 C# 中以 <c>Properties is null</c> 对应；StackMaxSize 为 int，无 undefined 语义。</item>
///   <item>overrides 最后应用，覆盖查找表结果；key 先按物品 <c>_id</c> 匹配，否则按父类批量。</item>
/// </list>
/// 应用顺序对齐源 TS：clothing → provisions → medicals → barter → partsnmods → keycards → overrides。
/// </summary>
public static class TrueItemsApplier
{
    public static ModuleReport Apply(TrueItemsTables templates, TemplateTable table, TrueItemsConfig config)
    {
        var warnings = new List<string>();
        var items = table?.Items;
        if (items is null)
        {
            warnings.Add($"[ITS] {TrueItemsModule.ModuleId}: 模板表 Items 为空，未应用任何堆叠变更");
            return ModuleReport.Ok(TrueItemsModule.ModuleId, 0, warnings);
        }

        var (byId, byParent) = BuildIndex(items);

        var changed = 0;
        changed += ApplyList(templates.Clothing, "clothing", false, byId, warnings);
        changed += ApplyList(templates.Provisions, "provisions", false, byId, warnings);
        changed += ApplyList(templates.Medicals, "medicals", true, byId, warnings);
        changed += ApplyList(templates.Barter, "barter", false, byId, warnings);
        changed += ApplyParents(templates.PartsnMods, "partsnmods", byParent, warnings);
        changed += ApplyParents(templates.Keycards, "keycards", byParent, warnings);
        changed += ApplyOverrides(config.Overrides, byId, byParent, warnings);

        return ModuleReport.Ok(TrueItemsModule.ModuleId, changed, warnings);
    }

    /// <summary>按 <c>_id</c> / <c>_parent</c> 建立字符串索引（MongoId.ToString，兼容占位非 hex id 不抛异常）。</summary>
    private static (Dictionary<string, TemplateItem> ById, Dictionary<string, List<TemplateItem>> ByParent) BuildIndex(
        Dictionary<SPTarkov.Server.Core.Models.Common.MongoId, TemplateItem> items)
    {
        var byId = new Dictionary<string, TemplateItem>(StringComparer.Ordinal);
        var byParent = new Dictionary<string, List<TemplateItem>>(StringComparer.Ordinal);

        foreach (var item in items.Values)
        {
            if (item is null)
            {
                continue;
            }

            var id = item.Id.ToString();
            if (!string.IsNullOrEmpty(id))
            {
                byId.TryAdd(id, item);
            }

            var parent = item.Parent.ToString();
            if (string.IsNullOrEmpty(parent))
            {
                continue;
            }

            if (!byParent.TryGetValue(parent, out var children))
            {
                children = [];
                byParent[parent] = children;
            }

            children.Add(item);
        }

        return (byId, byParent);
    }

    private static int ApplyList(
        TrueItemsTable table,
        string label,
        bool medicalOnlyEmptyContainers,
        IReadOnlyDictionary<string, TemplateItem> byId,
        List<string> warnings)
    {
        if (!table.Active)
        {
            return 0;
        }

        var changed = 0;
        foreach (var entry in table.List)
        {
            if (!byId.TryGetValue(entry.Id, out var item))
            {
                warnings.Add($"[ITS] {TrueItemsModule.ModuleId}/{label}: 未命中 _id={entry.Id}（{entry.Name}），已跳过");
                continue;
            }

            var props = item.Properties;
            if (props is null)
            {
                continue;
            }

            // medicals 仅对空医疗容器生效：MaxHpResource 有值且 ≤ 0。
            if (medicalOnlyEmptyContainers && !(props.MaxHpResource is int maxHp && maxHp <= 0))
            {
                continue;
            }

            props.StackMaxSize = entry.Props.StackMaxSize * table.StackMult;
            props.StackMinRandom = 1;
            changed++;
        }

        return changed;
    }

    private static int ApplyParents(
        TrueItemsTable table,
        string label,
        IReadOnlyDictionary<string, List<TemplateItem>> byParent,
        List<string> warnings)
    {
        if (!table.Active)
        {
            return 0;
        }

        var changed = 0;
        foreach (var entry in table.ParentList)
        {
            if (!byParent.TryGetValue(entry.Id, out var children))
            {
                warnings.Add($"[ITS] {TrueItemsModule.ModuleId}/{label}: 未命中父类 _id={entry.Id}（{entry.Name}），已跳过");
                continue;
            }

            foreach (var item in children)
            {
                if (item.Properties is null)
                {
                    continue;
                }

                item.Properties.StackMaxSize = entry.StackMaxSize * table.StackMult;
                item.Properties.StackMinRandom = 1;
                changed++;
            }
        }

        return changed;
    }

    private static int ApplyOverrides(
        IReadOnlyDictionary<string, int>? overrides,
        IReadOnlyDictionary<string, TemplateItem> byId,
        IReadOnlyDictionary<string, List<TemplateItem>> byParent,
        List<string> warnings)
    {
        if (overrides is null || overrides.Count == 0)
        {
            return 0;
        }

        var changed = 0;
        foreach (var (id, target) in overrides)
        {
            if (byId.TryGetValue(id, out var item))
            {
                if (item.Properties is not null)
                {
                    item.Properties.StackMaxSize = target;
                    item.Properties.StackMinRandom = 1;
                    changed++;
                }

                continue;
            }

            if (byParent.TryGetValue(id, out var children))
            {
                foreach (var child in children)
                {
                    if (child.Properties is null)
                    {
                        continue;
                    }

                    child.Properties.StackMaxSize = target;
                    child.Properties.StackMinRandom = 1;
                    changed++;
                }

                continue;
            }

            warnings.Add($"[ITS] {TrueItemsModule.ModuleId}/overrides: 未命中 id={id}，已跳过");
        }

        return changed;
    }
}
