using Memtly.Core.Attributes;
using Memtly.Core.Constants;
using Memtly.Core.Enums;
using Memtly.Core.Helpers;
using Memtly.Core.Helpers.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TwoFactorAuthNet;

namespace Memtly.Core.Controllers
{
    [Authorize]
    public class MultiFactorController : BaseController
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISettingsHelper _settings;
        private readonly IDatabaseHelper _database;
        private readonly IAuditHelper _audit;
        private readonly IIdentityHelper _identity;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public MultiFactorController(IWebHostEnvironment hostingEnvironment, ISettingsHelper settings, IDatabaseHelper database, IAuditHelper audit, IIdentityHelper identity, ILoggerFactory loggerFactory, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _hostingEnvironment = hostingEnvironment;
            _settings = settings;
            _database = database;
            _audit = audit;
            _identity = identity;
            _logger = loggerFactory.CreateLogger<MultiFactorController>();
            _localizer = localizer;
        }

        [HttpGet]
        [RequiresRole(UserPermission = UserPermissions.Login)]
        public async Task<IActionResult> GenerateToken()
        {
            var title = await _settings.GetOrDefault(MemtlyConfiguration.Basic.Title, "Memtly");
            var tfa = new TwoFactorAuth(title);

            var secret = tfa.CreateSecret(160);
            var qrCode = tfa.GetQrCodeImageAsDataUri(title, secret);
            
            return Json(new { secret = secret, qr_code = qrCode });
        }

        [HttpPost]
        [RequiresRole(UserPermission = UserPermissions.Login)]
        public async Task<IActionResult> Register(string secret, string code)
        {
            if (!string.IsNullOrWhiteSpace(secret) && !string.IsNullOrWhiteSpace(code))
            {
                if (_identity.IsValid(User))
                {
                    try
                    {
                        if (await _settings.GetOrDefault(MemtlyConfiguration.IsDemoMode, false))
                        {
                            return Json(new { success = false, message = _localizer["Feature_Unavailable_Demo_Mode"].Value });
                        }

                        var tfa = new TwoFactorAuth(await _settings.GetOrDefault(MemtlyConfiguration.Basic.Title, "Memtly"));

                        var valid = tfa.VerifyCode(secret, code);
                        if (valid)
                        {
                            var userId = _identity.GetUserId(User);
                            if (userId > 0)
                            {
                                var set = await _database.SetMultiFactorToken(userId, secret);
                                if (set)
                                {
                                    await _audit.LogAction(userId, _localizer["Audit_MultiFactorAdded"].Value, AuditSeverity.Verbose);

                                    HttpContext.Session.SetString(SessionKey.MultiFactor.TokenSet, "true");
                                    return Json(new { success = true });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{_localizer["MultiFactor_Token_Set_Failed"].Value} - {ex?.Message}");
                    }
                }
            }

            return Json(new { success = false });
        }

        [HttpDelete]
        [RequiresRole(UserPermission = UserPermissions.Reset_MFA)]
        public async Task<IActionResult> Reset()
        {
            if (_identity.IsValid(User))
            {
                try
                {
                    var userId = _identity.GetUserId(User);

                    await _audit.LogAction(userId, _localizer["Audit_MultiFactorReset"].Value, AuditSeverity.Information);

                    return await ResetForUser(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_localizer["MultiFactor_Token_Set_Failed"].Value} - {ex?.Message}");
                }
            }

            return Json(new { success = false });
        }

        [HttpDelete]
        [RequiresRole(UserPermission = UserPermissions.Reset_MFA)]
        public async Task<IActionResult> ResetForUser(int userId)
        {
            if (_identity.IsValid(User))
            {
                try
                {
                    if (userId > 0)
                    {
                        var user = await _database.GetUser(userId);
                        if (user != null && _identity.CanEdit(User, UserPermissions.Reset_MFA, user.Id))
                        { 
                            var cleared = await _database.SetMultiFactorToken(userId, string.Empty);
                            if (cleared)
                            {
                                var currentUserId = _identity.GetUserId(User);
                                if (userId == currentUserId)
                                { 
                                    HttpContext.Session.SetString(SessionKey.MultiFactor.TokenSet, "false");
                                }

                                await _audit.LogAction(currentUserId, $"{_localizer["Audit_MultiFactorResetUser"].Value} '{user?.Username}'", AuditSeverity.Information);

                                return Json(new { success = true });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_localizer["MultiFactor_Token_Set_Failed"].Value} - {ex?.Message}");
                }
            }

            return Json(new { success = false });
        }
    }
}