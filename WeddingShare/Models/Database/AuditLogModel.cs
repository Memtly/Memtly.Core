using WeddingShare.Enums;

namespace WeddingShare.Models.Database
{
    public class AuditLogModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public AuditSeverity Severity { get; set; } = AuditSeverity.Information;
        public DateTime Timestamp { get; set; }
    }
}