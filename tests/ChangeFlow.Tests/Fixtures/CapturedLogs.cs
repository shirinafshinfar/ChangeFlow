using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
namespace ChangeFlow.Tests.Fixtures;
public sealed class CapturedLogs:ILoggerProvider
{
    public ConcurrentQueue<string> Entries {get;}=new();
    public ConcurrentQueue<IReadOnlyList<KeyValuePair<string,object?>>> Structured {get;}=new();
    public ILogger CreateLogger(string categoryName)=>new Logger(Entries,Structured);
    public void Dispose(){}
    private sealed class Logger(ConcurrentQueue<string> entries,ConcurrentQueue<IReadOnlyList<KeyValuePair<string,object?>>> structured):ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState:notnull=>null;
        public bool IsEnabled(LogLevel logLevel)=>true;
        public void Log<TState>(LogLevel level,EventId id,TState state,Exception? exception,Func<TState,Exception?,string> formatter)
        {
            entries.Enqueue(formatter(state,exception));
            if(state is IEnumerable<KeyValuePair<string,object?>> fields)structured.Enqueue(fields.ToArray());
        }
    }
}
