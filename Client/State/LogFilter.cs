using Data.Log;
using System.Text.RegularExpressions;

namespace Client.State
{
    public class LogFilter
    {
        public Regex? UserIDMatch { get; set; }

        public bool Matches(LogEntry entry)
        {
            if (UserIDMatch != null)
            {
                if (!UserIDMatch.IsMatch(entry.User.Id))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
