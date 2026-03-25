using Memtly.Core.Enums;

namespace Memtly.Core.Models.Database
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = "Unknown";
        public string? Email { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Password { get; set; }
        public string? CPassword { get; set; }
        public DateTimeOffset? PaidUntil { get; set; }
        public int FailedLogins { get; set; }
        public DateTimeOffset? LockoutUntil { get; set; }
        public string? MultiFactorToken { get; set; }
        public AccountState State { get; set; } = AccountState.Active;
        public UserLevel Level { get; set; } = UserLevel.Basic;
        public PaidTier Tier { get; set; } = PaidTier.None;

        public SubscriptionState SubscriptionState
        {
            get
            {
                return this.PaidUntil != null && this.PaidUntil.Value.DateTime >= DateTime.UtcNow ? SubscriptionState.Active : SubscriptionState.Inactive;
            }
        }

        public bool IsLockedOut
        {
            get 
            {
                return this.LockoutUntil != null && this.LockoutUntil.Value.DateTime >= DateTime.UtcNow;
            }
        }
    }
}