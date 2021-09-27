using System;

namespace Client.State
{
    public class Mission : StateObject
    {
        private Guid _id;
        public Guid Id 
        { 
            get => _id;
            set
            {
                _id = value;
                NotifyStateChanged();
            }
        }
       
    }
}
