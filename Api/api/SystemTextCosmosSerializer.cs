using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api
{
    public class SystemTextCosmosSerializer : CosmosSerializer
    {
        public override T FromStream<T>(Stream stream)
        {
            using (stream) return Util.Deserialize<T>(stream);
        }

        public override Stream ToStream<T>(T input)
        {
            return Util.SerializeToStream(input);
        }
    }
}
