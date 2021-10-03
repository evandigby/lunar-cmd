using Data.Log;

namespace LunarAPIClient
{
    public class LogEntryAttachmentData
    {
        public Guid AttachmentId { get; set; } = Guid.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string DataURI { get; set; } = string.Empty;
    }
}
