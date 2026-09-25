using Microsoft.Extensions.Logging;
using Spectre.Console;
using SPTarkov.Common.Models.Logging;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>
/// 通用日志桩：捕获 Info / Warning / Error 文本，供模块与编排器测试断言旁路日志。
/// </summary>
internal sealed class RecordingLogger<T> : ISptLogger<T>
{
    public List<string> InfoMessages { get; } = [];

    public List<string> WarningMessages { get; } = [];

    public List<string> ErrorMessages { get; } = [];

    public void Info(string data, Exception? ex = null) => InfoMessages.Add(data);

    public void Error(string data, Exception? ex = null) => ErrorMessages.Add(data);

    public void Warning(string data, Exception? ex = null) => WarningMessages.Add(data);

    public void Debug(string data, Exception? ex = null) { }

    public void Success(string data, Exception? ex = null) { }

    public void Critical(string data, Exception? ex = null) { }

    public void LogWithColor(string data, Color? textColor = null, Color? backgroundColor = null, Exception? ex = null) { }

    public void Log(
        LogLevel level,
        string data,
        Color? textColor = null,
        Color? backgroundColor = null,
        Exception? ex = null) { }

    public bool IsLogEnabled(LogLevel level) => true;
}
