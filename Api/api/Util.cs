using Data.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace api
{
    public class Util
    {
        private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

        public static string PartitionKey()
        {
            return jsonOptions.PropertyNamingPolicy?.ConvertName(nameof(Command.UserId)) ?? nameof(Command.UserId);
        }

        public static string Serialize<T>(T item)
        {
            return JsonSerializer.Serialize(item, jsonOptions);
        }

        public static Stream SerializeToStream<T>(T item)
        {
            var memStream = new MemoryStream();

            JsonSerializer.Serialize(memStream, item, jsonOptions);

            memStream.Position = 0;
            return memStream;
        }

        public static T Deserialize<T>(string message)
        {
            return JsonSerializer.Deserialize<T>(message, jsonOptions);
        }

        public static T Deserialize<T>(Stream message)
        {
            return JsonSerializer.Deserialize<T>(message, jsonOptions);
        }

        public static ValueTask<T> DeserializeAsync<T>(Stream message, CancellationToken cancellationToken)
        {
            return JsonSerializer.DeserializeAsync<T>(message, jsonOptions, cancellationToken);
        }
    }
}
