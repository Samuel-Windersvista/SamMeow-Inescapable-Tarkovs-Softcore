// dump-spt-symbols.cs — 生成 gen-fleamarket.ps1 所需的符号表 JSON。
//
// 用途：把 SPT 5 运行时的 ItemTpl / BaseClasses 静态枚举常量导出为
//   { "ItemTpl": { "NAME": "hex" }, "BaseClasses": { "NAME": "hex" } }
// 供 scripts/tools/gen-fleamarket.ps1 解析源 TS 中的 ItemTpl.* / BaseClasses.* 符号。
//
// 使用方式（临时控制台项目，勿入库；需 .NET 10 SDK）：
//   1. mkdir D:\Temp\dump-spt-symbols && cd D:\Temp\dump-spt-symbols
//   2. dotnet new console
//   3. 把本文件内容覆盖到 Program.cs
//   4. dotnet run -- "E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime" "D:\Temp\spt-symbols.json"
//
// 说明：SPT 静态枚举字段类型为 MongoId（ToString() 返回 24 位 hex），
// 故此处对字段值调用 ToString() 后再序列化。

using System.Reflection;
using System.Text.Json;

var runtimeDir = args.Length > 0 ? args[0] : @"E:\Game\EFT_Offline\SPT_5xx\SPT_Runtime";
var outPath = args.Length > 1 ? args[1] : @"D:\Temp\spt-symbols.json";

var assembly = Assembly.LoadFrom(Path.Combine(runtimeDir, "SPTarkov.Server.Core.dll"));
var exported = assembly.GetExportedTypes();

Dictionary<string, string> Dump(string typeName)
{
    var type = exported.First(type => type.Name == typeName);
    var map = new Dictionary<string, string>();
    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
    {
        try
        {
            map[field.Name] = field.GetValue(null)?.ToString() ?? string.Empty;
        }
        catch
        {
            // 忽略无法取值的字段
        }
    }

    return map;
}

var symbols = new Dictionary<string, object>
{
    ["ItemTpl"] = Dump("ItemTpl"),
    ["BaseClasses"] = Dump("BaseClasses")
};

File.WriteAllText(outPath, JsonSerializer.Serialize(symbols));
Console.WriteLine($"wrote {outPath}: ItemTpl={((Dictionary<string, string>)symbols["ItemTpl"]).Count} BaseClasses={((Dictionary<string, string>)symbols["BaseClasses"]).Count}");
