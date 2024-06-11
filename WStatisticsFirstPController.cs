using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class WStatisticsFirstPController : WBaseController
    {
        public IActionResult Index(string userid)
        {
            ViewData["userid"] = userid;
            return View();
        } 
    }
}
