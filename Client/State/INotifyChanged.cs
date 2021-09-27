using System;

namespace Client.State
{
    public interface INotifyChanged
    {
        public event Action? OnChanged;
    }
}
