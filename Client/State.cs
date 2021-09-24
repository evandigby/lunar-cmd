using Data.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client
{
    public class State
    {
        public State(Uri baseAddress)
        {
            BaseAddress = baseAddress;
            logEntries.CollectionChanged += Messages_CollectionChanged;
        }

        private void Messages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyStateChanged();
        }

        private readonly ObservableCollection<LogEntry> logEntries = new();

        public IList<LogEntry> LogEntries => logEntries;

        public Uri BaseAddress {  get; set; }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
