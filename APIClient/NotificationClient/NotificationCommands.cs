using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarAPIClient.NotificationClient
{
    public static class NotificationCommands
    {
        public const string NewLogEntry = "newLogEntry";
        public const string UpdateLogEntry = "updateLogEntry";
        public const string LogEntryAttachmentUploadComplete = "logEntryAttachmentUploadComplete";
        public const string SendCommand = "sendCommand";
        /// <summary>
        /// Use sparingly
        /// </summary>
        public const string RefreshAllLogEntries = "refreshAllLogEntries";
    }
}
