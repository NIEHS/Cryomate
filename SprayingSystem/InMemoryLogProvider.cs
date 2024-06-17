using System.Collections.Concurrent;

public class InMemoryLogProvider : ILoggerProvider
{
    private ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();

    public ILogger CreateLogger(string categoryName) => new InMemoryLogger(this);

    public void Dispose() { _logs = null; }

    internal void AddLog(string log)
    {
        _logs.Enqueue(log);
        // Optionally limit the queue size
    }

    public IEnumerable<string> GetLogs() => _logs.ToList();
}

public class InMemoryLogger : ILogger
{
    private readonly InMemoryLogProvider _provider;

    public InMemoryLogger(InMemoryLogProvider provider)
    {
        _provider = provider;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _provider.AddLog(formatter(state, exception));
    }
}