using System;
using System.Text.RegularExpressions;

namespace Client.State
{
    public class StateContainer : StateObject
    {
        public StateContainer(Uri baseAddress, string apiVersion, string hubName)
        {

            Api = new(baseAddress, apiVersion, hubName);
            Mission = new();
            User = new();
            User.OnChanged += User_OnChanged;
        }

        private void User_OnChanged()
        {
            var all = new Log
            {
                Name = "Mission Log"
            };

            //var justMe = new Log
            //{
            //    Name = "My Log Entries"
            //};

            //justMe.Filters.Add(new LogFilter
            //{
            //    UserIDMatch = new Regex($"^{User.Id}$")
            //});

            Mission.UpdateLogs(new[] { all/*, justMe */});
        }

        public void UpdateUser(Data.Users.User newUser)
        {
            User.Id = newUser.Id;
            User.Name = newUser.Name;
            User.PreferredUserName = newUser.PreferredUserName;
        }

        public Api Api { get; }
        public Mission Mission { get; }
        public User User { get; }
    }
}
