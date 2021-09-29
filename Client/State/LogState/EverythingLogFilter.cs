using Data.Log;

namespace Client.State.LogState
{
    public class EverythingLogFilter : ILogFilter
    {
        public bool Matches(LogEntry entry)
        {
            return true;
        }
    }
}
