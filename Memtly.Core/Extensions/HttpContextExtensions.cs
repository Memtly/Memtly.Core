namespace Memtly.Core.Extensions
{
    public static class HttpContextExtensions
    {
        public static string TryGetIpAddress(this HttpContext ctx)
        {
            try
            {
                var ipAddress = TryGetHeaderValue(ctx, ["CF-Connecting-IP", "CF-Connecting-IPv6", "X-Forwarded-For", "HTTP_X_FORWARDED_FOR", "REMOTE_ADDR"]);
                if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                {
                    return ctx.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
                }

                return ipAddress;
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string TryGetCountry(this HttpContext ctx)
        {
            return ctx.TryGetHeaderValue(["CF-IPCountry"]);
        }

        public static string TryGetHeaderValue(this HttpContext ctx, string[] headers)
        {
            foreach (var header in headers)
            {
                try
                {
                    string? val = ctx.Request.Headers[header];
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        var vals = val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (vals.Length != 0)
                        {
                            return vals[0];
                        }
                    }
                }
                catch { }
            }

            return "Unknown";
        }
    }
}