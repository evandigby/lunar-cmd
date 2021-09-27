using System;

namespace Client.State
{
    public class StateContainer : StateObject
    {
        public StateContainer(Uri baseAddress)
        {
            BaseAddress = baseAddress;
            Mission = new Mission();
            Mission.OnChanged += Mission_OnChanged;
            User = new User();
            User.OnChanged += User_OnChanged;
            Console = new Console();
            Console.OnChanged += Console_OnChanged;
        }

        private void Console_OnChanged()
        {
            NotifyStateChanged();
        }

        private void User_OnChanged()
        {
            NotifyStateChanged();
        }

        private void Mission_OnChanged()
        {
            NotifyStateChanged();
        }

        public Uri BaseAddress {  get; set; }
        public Mission Mission { get; } 
        public User User { get; }
        public Console Console { get; }
    }
}
