using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArchiveFileManagementNs.Controllers
{
    public class WBaseController : Controller
    {
        /// <summary>
        /// 请求过滤处理
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            byte[] result;
            filterContext.HttpContext.Session.TryGetValue("User", out result);
            if (result == null)
            {
                filterContext.Result = new RedirectResult("/WLogin/Index");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

    }
}
