using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class ResponsibleMenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
