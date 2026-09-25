namespace InescapableTarkovsSoftcore.Features.Softcore;

/// <summary>Softcore 子变换器的变更/告警/错误累加器。</summary>
public sealed class SoftcoreChangeLog
{
    public int ChangedCount { get; private set; }

    public List<string> Warnings { get; } = [];

    public List<Exception> Errors { get; } = [];

    public void Changed(int count = 1) => ChangedCount += count;

    public void Warn(string message) => Warnings.Add(message);

    public void Error(Exception exception) => Errors.Add(exception);
}
