using System.Text.RegularExpressions;
using Memtly.Core.Enums;

namespace Memtly.Core.Helpers
{
    public interface IDeviceDetector
    {
        Task<DeviceType> ParseDeviceType(string userAgent);
    }

    public class DeviceDetector : IDeviceDetector
    {
        public async Task<DeviceType> ParseDeviceType(string userAgent)
        {
            return await Task.Run(() => 
            {
                if (string.IsNullOrWhiteSpace(userAgent))
                { 
                    return DeviceType.Unknown;
                }

                if (Regex.IsMatch(userAgent, "(tablet|ipad|playbook|silk)|(android(?!.*mobile))", RegexOptions.IgnoreCase))
                { 
                    return DeviceType.Tablet;
                }

                if (Regex.IsMatch(userAgent, "blackberry|iphone|mobile|windows ce|opera mini|htc|sony|palm|symbianos|ipad|ipod|blackberry|bada|kindle|symbian|sonyericsson|android|samsung|nokia|wap|motor", RegexOptions.IgnoreCase))
                { 
                    return DeviceType.Mobile;
                }
                
                return DeviceType.Desktop;
            });
        }
    }
}