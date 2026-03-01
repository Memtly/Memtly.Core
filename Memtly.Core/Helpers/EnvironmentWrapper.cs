namespace Memtly.Core.Helpers
{
    public interface IEnvironmentWrapper
    {
        string? GetEnvironmentVariable(string variable);
    }

    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public string? GetEnvironmentVariable(string variable) 
        { 
            return Environment.GetEnvironmentVariable(variable);
        }
    }
}