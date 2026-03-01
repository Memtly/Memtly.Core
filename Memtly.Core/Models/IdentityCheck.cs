namespace Memtly.Core.Models
{
    public class IdentityCheck
    {
        public IdentityCheck()
            : this(false)
        {
        }

        public IdentityCheck(bool enabled)
        {
            this.Enabled = enabled;
        }

        public bool Enabled { get; set; }
        public bool IsNameRequired { get; set; } = true;
        public bool IsEmailRequired { get; set; } = false;
    }
}