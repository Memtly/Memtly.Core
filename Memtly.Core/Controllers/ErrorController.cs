using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Memtly.Core.Controllers
{
    [AllowAnonymous]
    public class ErrorController : BaseController
    {
        public ErrorController()
            : base()
        {
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View();
        }
    }
}