using Microsoft.Extensions.Logging;
using Spectre.Console;
using SPTarkov.Common.Models.Logging;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>通用记录型日志桩：按级别收集消息文本，供模块与编排器测试断言。</summary>
internal sealed class RecordingLogger<T> : ISptLogger<T>
{
    public List<string> InfoMessages { get; } = [];

    public List<string> ErrorMessages { get; } = [];

    public List<string> WarningMessages { get; } = [];

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
