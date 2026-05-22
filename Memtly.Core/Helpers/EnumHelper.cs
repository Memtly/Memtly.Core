namespace Memtly.Core.Helpers
{
    public class EnumHelper
    {
        public static T TryParse<T>(string value, T defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(value))
            { 
                try
                {
                    value = value.Trim();
                    
                    foreach (var val in Enum.GetValues(typeof(T)))
                    {
                        if (val.ToString()!.Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            return (T)val;
                        }
                    }
                }
                catch { }
            }
                
            return defaultValue;
        }
    }
}