using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class TreeGridController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
