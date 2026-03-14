using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Memtly.Core.UnitTests.Helpers
{
    internal class MockSession : ISession
    {
        private IDictionary<string, string> Values = new Dictionary<string, string>();

        public bool IsAvailable => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public IEnumerable<string> Keys => throw new NotImplementedException();

        public void Clear()
        {
            this.Values.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            this.Values.Remove(key);
        }

        public void Set(string key, string value)
        {
            this.Set(key, Encoding.UTF8.GetBytes(value));
        }

        public void Set(string key, byte[] value)
        {
            if (this.Values.ContainsKey(key))
            {
                this.Values.Remove(key);
            }

            this.Values.Add(key, Encoding.UTF8.GetString(value));
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
        {
            if (this.Values.ContainsKey(key))
            {
                value = Encoding.UTF8.GetBytes(this.Values[key]);
                return true;
            }

            value = null;

            return false;
        }
    }
}