namespace Astra.Compilation;

using System.Collections.Generic;

public class ErrorLogger
{
    public List<LogEntry> entries = new();

    public void Error(LogEntry entry)
    {
        entries.Add(entry);
        throw new Exception();
    }
}

public class LogEntry
{
    public int tokenBeginIndex, tokenEndIndex;
    public string message;
}
public class LogEntries
{
    public LogEntry[] markers;
}