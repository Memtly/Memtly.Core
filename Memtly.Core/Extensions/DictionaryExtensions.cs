namespace Memtly.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static string GetValue(this IDictionary<string, string> value, string key, string defaultValue = "")
        {
            try
            {
                if (value.ContainsKey(key))
                {
                    return value[key];
                }
            }
            catch { }

            return defaultValue;
        }
    }
}