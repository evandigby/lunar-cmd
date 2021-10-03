using Client.State.LogState;
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
            Name = MainLogName,
            Filters = new List<ILogFilter>() { new EverythingLogFilter() }
        };

        private readonly Log currentUserLog = new()
        {
            Name = CurrentUserLogName,
            Filters = new List<ILogFilter>() { new UserIDLogFilter("") }
        };

        public StateContainer(Uri baseAddress, string apiVersion, string hubName)
        {

            Api = new(baseAddress, apiVersion, hubName);
            Mission = new()
            {
                Logs = new List<Log> { mainLog, currentUserLog }
            };

            User = new();
            Attachments = new();
        }

        public void UpdateUser(Data.Users.User newUser)
        {
            User = new()
            {
                Id = newUser.Id,
                Name = newUser.Name,
                PreferredUserName = newUser.PreferredUserName
            };

            if (currentUserLog.Filters.Single() is UserIDLogFilter userIDLogFilter)
            {
                userIDLogFilter.UpdateUserID(newUser.Id);
            }
        }
        
        public Api Api { get; set; }
        public Mission Mission { get; set; }
        public User User { get; set; }
        public Attachments Attachments { get; set; }
    }
}
