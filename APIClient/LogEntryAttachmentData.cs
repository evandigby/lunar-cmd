using Data.Log;

namespace LunarAPIClient
{
    public class LogEntryAttachmentData
    {
        public LogEntryAttachment Attachment { get; set; } = new LogEntryAttachment();
        public string DataURI { get; set; } = string.Empty;
    }
}
