using Data.Commands;
using Data.Converters;
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
        public static string Serialize<T>(T item)
        {
            return JsonSerializer.Serialize(item, ConverterOptions.JsonSerializerOptions);
        }

        public static Stream SerializeToStream<T>(T item)
        {
            var memStream = new MemoryStream();

            JsonSerializer.Serialize(memStream, item, ConverterOptions.JsonSerializerOptions);

            memStream.Position = 0;
            return memStream;
        }

        public static T Deserialize<T>(string message)
        {
            return JsonSerializer.Deserialize<T>(message, ConverterOptions.JsonSerializerOptions);
        }

        public static T Deserialize<T>(Stream message)
        {
            return JsonSerializer.Deserialize<T>(message, ConverterOptions.JsonSerializerOptions);
        }

        public static ValueTask<T> DeserializeAsync<T>(Stream message, CancellationToken cancellationToken)
        {
            return JsonSerializer.DeserializeAsync<T>(message, ConverterOptions.JsonSerializerOptions, cancellationToken);
        }
    }
}
