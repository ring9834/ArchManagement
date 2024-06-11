using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArchiveFileManagementNs.Controllers
{
    public class WFirstPageController : Controller
    {
        public IActionResult Index(string id,string userid)
        {
            ViewData["LoggedUser"] = userid;//登录名
            ViewData["UserNickName"] = id;//登录用户名的昵称
            return View();
        }

    }
}
