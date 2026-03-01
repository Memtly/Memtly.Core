using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Memtly.Core.Attributes;
using Memtly.Core.Enums;
using Memtly.Core.Extensions;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Models.Database;

namespace Memtly.Core.Controllers
{
    [Authorize]
    public class AuditController : BaseController
    {
        private readonly IDatabaseHelper _database;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public AuditController(IDatabaseHelper database, ILogger<AuditController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _database = database;
            _logger = logger;
            _localizer = localizer;
        }

        [HttpPost]
        [RequiresRole(AuditPermission = AuditPermissions.View)]
        public async Task<IActionResult> AuditList(string term = "", AuditSeverity severity = AuditSeverity.Information, int limit = 10)
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }

            IEnumerable<AuditLogModel>? result = null;

            try
            {
                var user = await _database.GetUser(User.Identity.GetUserId());
                if (user != null)
                {
                    limit = limit >= 5 ? limit : 5;

                    if (User?.Identity?.IsPrivilegedUser() ?? false)
                    {
                        result = await _database.GetAuditLogs(null, term, severity, limit);
                    }
                    else
                    {
                        result = await _database.GetAuditLogs(user.Id, term, severity, limit);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{_localizer["Audit_List_Failed"].Value} - {ex?.Message}");
            }

            return PartialView("~/Views/Account/Partials/AuditList.cshtml", result ?? new List<AuditLogModel>());
        }
    }
}