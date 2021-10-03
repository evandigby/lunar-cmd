using Data.Commands;
using Data.Converters;
using Data.Log;
using Data.Notifications;
using LunarAPIClient.LogEntryRepository;
using LunarAPIClient.NotificationClient;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunarAPIClient.CommandProcessors
{
    internal class AppendLogEntryCommandProcessor : LogEntryProducingCommandProcessor
    {
        private readonly ILogEntryAttachmentContentTypeRepository logEntryAttachmentContentTypeRepository;
        private readonly ILogEntryRepository logEntryRepository;

        public AppendLogEntryCommandProcessor(
            ILogEntryAttachmentContentTypeRepository logEntryAttachmentContentTypeRepository, 
            ILogEntryRepository logEntryRepository)
        {
            this.logEntryAttachmentContentTypeRepository = logEntryAttachmentContentTypeRepository;
            this.logEntryRepository = logEntryRepository;
        }

        public async Task ProcessCommand(AppendLogEntryCommand cmd, CancellationToken cancellationToken)
        {
            LogEntry entry;

            if (cmd.LogEntryId == default)
            {
                throw new Exception("invalid log entry id");
            }

            LogEntry existingEntry;
            try
            {
                existingEntry = await logEntryRepository.GetById(cmd.LogEntryId, cmd.MissionId, cancellationToken);
            }
            catch (Exception)
            {
                // TODO: Doesn't already exist. This is expected in most cases... but we need to catch more specific exceptions
                existingEntry = new PlaceholderLogEntry();
            }

            if (existingEntry.EntryType != LogEntryType.Placeholder)
            {
                throw new Exception("only placeholders should exist here");
            }

            if (cmd.Payload is PlaintextPayload plaintextPayloadValue)
            {
                entry = new PlaintextLogEntry
                {
                    Value = plaintextPayloadValue.Value
                };
            }
            else
            {
                throw new Exception("Unknown log entry type");
            }

            var fileNameProvider = new FileExtensionContentTypeProvider();
            
            entry.Id = cmd.LogEntryId;
            entry.MissionId = cmd.MissionId;
            entry.User = cmd.User;
            entry.Attachments = cmd.Attachments.Select(a =>
            {
                var existingAttachment = existingEntry?.Attachments?.FirstOrDefault(ea => ea.Id == a.Id);
                if (existingAttachment != null)
                {
                    existingAttachment.Alt = a.Alt;
                    return existingAttachment;
                }

                return a;
            }).ToList();

            entry.LoggedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.EditHistory = Enumerable.Empty<LogEntry>().ToList();

            ProduceLogEntry(entry);
            ProduceNotification(new Notification
            {
                CommandTarget = NotificationCommands.NewLogEntry,
                Audience = Audience.Everyone,
                Message = entry
            });

            return;
        }

        public override Task ProcessCommand(Command cmd, CancellationToken cancellationToken) => 
            ProcessCommand((AppendLogEntryCommand)cmd, cancellationToken);
    }
}
