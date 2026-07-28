using Memtly.Core.Extensions;
using Memtly.Core.Helpers.Database;
using Memtly.Core.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Memtly.Core.Controllers
{
    [AllowAnonymous]
    public class UserController : BaseController
    {
        private readonly IDatabaseHelper _database;
        private readonly ILogger<ThemesController> _logger;
        private readonly IStringLocalizer<Localization.Translations> _localizer;

        public UserController(IDatabaseHelper database, ILogger<ThemesController> logger, IStringLocalizer<Localization.Translations> localizer)
            : base()
        {
            _database = database;
            _logger = logger;
            _localizer = localizer;
        }

        [HttpPost]
        [Route("User/Search")]
        public async Task<IActionResult> Search(string term, int limit = 5, int[]? excludeUserIds = null)
        {
            List<UserModel>? result = null;

            try
            {
                result = await _database.GetUsers(term, page: 1, limit: limit, exclude: excludeUserIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to search users by term '{term}' and limit '{limit}'");
            }

            return Json(new { items = result?.Select(u => new { Id = u.Id, Username = u.Username })?.ToList() });
        }
    }
}