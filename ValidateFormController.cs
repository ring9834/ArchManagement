using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class ValidateForm : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ReturnDataToFront()
        {
            //return RedirectToAction("ToModalView", "ModalWin");
            return Json(new { success = true,message="ok"});
        }
    }
}
