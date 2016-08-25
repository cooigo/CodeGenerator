using Newtonsoft.Json;
using System.IO;

namespace Cooigo.CodeGenerator
{
    public static class JsonConfigHelper
    {
        public static TConfig LoadConfig<TConfig>(string path)
            where TConfig : new()
        {
            if (File.Exists(path))
            {
                using (var sr = new StreamReader(path))
                {
                    string json = sr.ReadToEnd().TrimToNull() ?? "{}";
                    return JsonConvert.DeserializeObject<TConfig>(json, JsonSettings.Tolerant);
                }
            }

            return new TConfig();
        }
    }
}