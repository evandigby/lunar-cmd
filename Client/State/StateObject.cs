using System;

namespace Client.State
{
    public abstract class StateObject : INotifyChanged
    {
        public event Action? OnChanged;
        protected void NotifyStateChanged() => OnChanged?.Invoke();
    }
}
