using Data.Log;

namespace Client.State.LogState
{
    public interface ILogFilter
    {
        public bool Matches(LogEntry entry);
    }
}
