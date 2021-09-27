using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data.Converters
{
    public static class Extensions
    {
        public static string Serialize<T>(this T item)
        {
            return JsonSerializer.Serialize(item, ConverterOptions.JsonSerializerOptions);
        }

        public static Stream SerializeToStream<T>(this T item)
        {
            var memStream = new MemoryStream();

            JsonSerializer.Serialize(memStream, item, ConverterOptions.JsonSerializerOptions);

            memStream.Position = 0;
            return memStream;
        }
    }
}
