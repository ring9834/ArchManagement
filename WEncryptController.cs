using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WEncryptController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdatePwd()
        {
            return View("UpdatePwd");
        }

        public IActionResult DecryptAccessV()
        {
            return View("PermitDecrypt");
        }

        public IActionResult DecryptGAccessV()
        {
            return View("PermitDecrypt2");
        }

        public IActionResult GetPmssions()
        {
            string sql = "SELECT pwd_type,name,pwd,Unique_code FROM t_config_yw_permit";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult IfOldRight(string oldPwd, string pwdType)
        {
            string sql = "SELECT Unique_code FROM t_config_yw_permit WHERE pwd_type='" + pwdType + "' AND pwd='" + oldPwd + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            int result = 0;
            if (dt.Rows.Count > 0)
                result = 1;
            return Json(new { rst = result });
        }

        public IActionResult UpdatePwdFunc(string newPwd, string pwdType)
        {
            string sql = "UPDATE t_config_yw_permit SET pwd=@pwd WHERE pwd_type=@pwd_type";
            SqlParameter para1 = SqlHelper.MakeInParam("pwd", newPwd);
            SqlParameter para2 = SqlHelper.MakeInParam("pwd_type", pwdType);
            SqlParameter[] param = new SqlParameter[] { para1, para2 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            return Json(new { rst = result });
        }

        public IActionResult IfAccessed(string ppwd)
        {
            string sql = "SELECT Unique_code FROM t_config_yw_permit WHERE pwd_type='forpermmit' AND pwd='" + ppwd + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            int result = 0;
            if (dt.Rows.Count > 0)
                result = 1;
            return Json(new { rst = result });
        }

    }
}
