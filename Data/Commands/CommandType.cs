namespace Data.Commands
{
    /// <summary>
    /// Enum values CAN NOT CHANGE
    /// </summary>
    public enum CommandType
    {
        AppendLogItem = 0,
        UpdateLogItem = 1,
        UploadAttachment = 2,
        FinalizeUserLogEntries = 3,
    }
}
