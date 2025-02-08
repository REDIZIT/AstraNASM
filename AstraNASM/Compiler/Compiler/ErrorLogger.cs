namespace Astra.Compilation;

using System.Collections.Generic;

public class ErrorLogger
{
    public List<LogEntry> entries = new();

    public void Error(LogEntry entry)
    {
        entries.Add(entry);
    }
}

public class LogEntry
{
    public Token token;
    public string message;
}
public class LogEntries
{
    public LogEntry[] markers;
}