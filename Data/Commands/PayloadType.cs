using System.Text.Json.Serialization;

namespace Data.Commands
{
    /// <summary>
    /// Enum values CAN NOT CHANGE
    /// </summary>
    public enum PayloadType
    {
        Plaintext = 0,
        BinaryReference = 1,
        Empty = 2
    }
}
