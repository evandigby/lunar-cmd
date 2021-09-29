using Data.Log;

namespace Client.State.LogState
{
    public class UserIDLogFilter : RegexLogFilter
    {
        public UserIDLogFilter(string userId) 
        {
            UpdateUserID(userId);
        }

        public void UpdateUserID(string userId)
        {
            Exact(userId);
        }

        public override string MatchValue(LogEntry entry)
        {
            return entry.User?.Id ?? "";
        }
    }
}
