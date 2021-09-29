using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Client.State
{
    public class StateContainer
    {
        private const string MainLogName = "Mission Log";
        private const string CurrentUserLogName = "My Logs";

        private readonly Log mainLog = new()
        {
            Name = MainLogName
        };

        private readonly Log currentUserLog = new()
        {
            Name = CurrentUserLogName,
            Filters = new List<LogFilter>()
            {
                new LogFilter
                {
                    UserIDMatch = new Regex($"^$")
                }
            }
        };

        public StateContainer(Uri baseAddress, string apiVersion, string hubName)
        {

            Api = new(baseAddress, apiVersion, hubName);
            Mission = new()
            {
                Logs = new List<Log> { mainLog, currentUserLog }
            };

            User = new();
        }

        public void UpdateUser(Data.Users.User newUser)
        {
            User = new()
            {
                Id = newUser.Id,
                Name = newUser.Name,
                PreferredUserName = newUser.PreferredUserName
            };
            currentUserLog.Filters.Single().UserIDMatch = new Regex($"^{newUser.Id}$");
        }
        
        public Api Api { get; set; }
        public Mission Mission { get; set; }
        public User User { get; set; }
    }
}
