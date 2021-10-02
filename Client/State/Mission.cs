using Data.Log;
using Client.State.LogState;
using System;
using System.Collections.Generic;
using System.Linq;
using LunarAPIClient.NotificationClient;

namespace Client.State
{
    public class Mission
    {
        private List<LogEntry> _logEntries = new();

        public IEnumerable<Log> Logs { get; set; } = Enumerable.Empty<Log>();
        public Guid Id { get; set; } = Guid.Empty;
        public string CurrentEntryText { get; set; } = string.Empty;

        public void AddLogEntry(LogEntry entry)
        {
            _logEntries.Add(entry);
            UpdateLogs();
        }
        public void UpdateLogEntry(LogEntry entry)
        {
            _logEntries = _logEntries.Where(e => e.Id != entry.Id).Concat(new[] { entry }).ToList();
            UpdateLogs();
        }

        public void UpdateLogEntryProgress(LogEntryAttachmentPartUploadProgress progress)
        {
            _logEntries = _logEntries.Select(entry =>
            {
                if (entry.Id != progress.LogEntryId)
                    return entry;

                entry.Attachments = entry.Attachments.Select(attachment =>
                {
                    if (attachment.Id != progress.AttachmentId)
                        return attachment;

                    attachment.PartsUploaded = progress.NumUploaded;

                    return attachment;
                }).ToList();

                return entry;
            }).ToList();
        }

        public void AddLogEntries(IEnumerable<LogEntry> entries)
        {
            _logEntries.AddRange(entries);
            UpdateLogs();
        }

        private void UpdateLogs()
        {
            foreach (var log in Logs)
            {
                log.UpdateLogEntries(_logEntries.AsQueryable());
            }
        }
    }
}
