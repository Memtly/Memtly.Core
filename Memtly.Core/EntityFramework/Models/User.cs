using Memtly.Core.Enums;

namespace Memtly.Core.EntityFramework.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string MultiFactorAuthToken { get; set; } = string.Empty;
        public string ActionAuthCode { get; set; } = string.Empty;
        public UserLevel? Level { get; set; } = UserLevel.Basic;
        public PaidTier? Tier { get; set; } = PaidTier.None;
        public AccountState? State { get; set; } = AccountState.PendingActivation;
        public DateTimeOffset? PaidUntil { get; set; }
        public int FailedLoginCount { get; set; } = 0;
        public DateTimeOffset? LockoutUntil { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}