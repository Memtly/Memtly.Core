namespace Memtly.Core.UnitTests.Helpers
{
	internal class JsonResponseHelper
	{
		public static T GetPropertyValue<T>(object? obj, string propertyName, T defaultValue)
		{
			if (obj != null)
			{ 
				try
				{
					var val = obj?.GetType()?.GetProperty(propertyName)?.GetValue(obj, null);
					if (val != null)
					{
						return (T)val;
					}
				}
				catch { }
			}

			return defaultValue;
		}
	}
}