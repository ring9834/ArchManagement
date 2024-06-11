using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class ModalWinController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ToModalView()
        {
            ViewData["closeModal"] = true;
            return Json(new { success = true, message = "mine" });
        }
    }
}
