using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data.Converters
{
    public static class ConverterOptions
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; } = new (JsonSerializerDefaults.Web);
    }
}
