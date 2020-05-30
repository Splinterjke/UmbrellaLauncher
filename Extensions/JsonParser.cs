using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbrellaProject.Extensions
{
    public static class JsonParser
    {
        public static bool TryParseJson<T>(this string obj, out T result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(obj);
                return true;
            }
            catch (JsonSerializationException ex)
            {
                result = default(T);
                return false;
            }
        }
    }
}
