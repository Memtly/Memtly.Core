using Memtly.Core.Enums;

namespace Memtly.Core.EntityFramework.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public User? User { get; set; }
        public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
        public DateTimeOffset CreatedAt { get; set; }
    }
}