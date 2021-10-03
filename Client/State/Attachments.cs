using LunarAPIClient;
using LunarAPIClient.NotificationClient;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Client.State
{
    public class Attachments
    {
        private readonly ConcurrentDictionary<Guid, LogEntryAttachmentData> attachments = new();

        public async Task UpdateLogEntryProgress(LogEntryAttachmentUploadComplete progress, ILogEntryClient client, CancellationToken cancellationToken)
        {
            if (attachments.ContainsKey(progress.AttachmentId))
                return;

            await AddAttachment(client, progress.MissionId, progress.LogEntryId, progress.AttachmentId, cancellationToken);
        }


        public async Task<LogEntryAttachmentData?> AddAttachment(ILogEntryClient client, Guid missionId, Guid logEntryId, Guid attachmentId, CancellationToken cancellationToken)
        {
            var data = await client.GetLogEntryAttachment(missionId, logEntryId, attachmentId, cancellationToken);
            if (data == null)
                return null;

            attachments.TryAdd(attachmentId, data);

            return data;
        }

        public LogEntryAttachmentData? Get(Guid attachmentId)
        {
            if (attachments.TryGetValue(attachmentId, out LogEntryAttachmentData? logEntryAttachmentData))
                return logEntryAttachmentData;


            return null;
        }
    }
}
