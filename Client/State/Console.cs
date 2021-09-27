using Data.Log;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client.State
{
    public class Console : StateObject
    {
        private readonly ObservableCollection<LogEntry> _logEntries = new();

        public Console()
        {
            _logEntries.CollectionChanged += Messages_CollectionChanged;
        }
        private void Messages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyStateChanged();
        }

        public IList<LogEntry> LogEntries => _logEntries;
        
        private string _currentEntryText = "";
        public string CurrentEntryText
        {
            get => _currentEntryText;
            set
            {
                if (_currentEntryText == value)
                    return;

                _currentEntryText = value;
                NotifyStateChanged();
            }
        }
    }
}
