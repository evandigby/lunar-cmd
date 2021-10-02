using System.Text.Json.Serialization;

namespace Data.Commands
{
    /// <summary>
    /// Enum values CAN NOT CHANGE
    /// </summary>
    public enum PayloadType
    {
        Plaintext = 0,
        Binary = 1,
        None = 2
    }
}
