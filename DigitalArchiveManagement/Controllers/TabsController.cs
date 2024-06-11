using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class TabsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
