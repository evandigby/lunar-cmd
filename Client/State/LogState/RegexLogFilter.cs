using Data.Log;
using System.Text.RegularExpressions;

namespace Client.State.LogState
{
    public abstract class RegexLogFilter : ILogFilter
    {
        private Regex? Matcher { get; set; }

        public void Exact(string match)
        {
            Matcher = new Regex($"^{match}$");
        }

        public abstract string MatchValue(LogEntry entry);

        public bool Matches(LogEntry entry)
        {
            return Matcher?.IsMatch(MatchValue(entry)) ?? false;
        }
    }
}
