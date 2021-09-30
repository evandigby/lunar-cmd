using Data.Commands;

namespace Client.Components.Log
{
    public interface IEditableLogEntryView 
    {
        public UpdateLogEntryCommand UpdateCommand { get; }
    }
}
