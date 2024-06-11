using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WRoleController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddRoleView()
        {
            return View("AddRole");
        }

        public IActionResult GetRoles()
        {
            string sql = "SELECT role_name,Unique_code FROM t_config_role";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetRoleInfo(string id)
        {
            string sql = "SELECT Unique_code,role_name FROM t_config_role WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            ViewData["roleID"] = table.Rows[0]["Unique_code"].ToString();
            ViewData["roleName"] = table.Rows[0]["role_name"].ToString();
            return View("UpdateRole");
        }

        public IActionResult AddRole(string roleName)
        {
            string sql = "IF(SELECT COUNT(*) FROM t_config_role WHERE role_name=@role_name) = 0 \r\n";
            sql += " BEGIN \r\n";
            sql += "    INSERT INTO t_config_role(role_name) VALUES(@role_name) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法插入重复记录
            sql += " END ";
            SqlParameter para1 = SqlHelper.MakeInParam("role_name", roleName);
            SqlParameter[] param = new SqlParameter[] { para1 };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult UpdateRole(string roleName, string roleId)
        {
            string sql = "IF (SELECT COUNT(*) FROM t_config_role WHERE role_name=@role_name AND Unique_code != @Unique_code) = 0 \r\n";
            sql += " BEGIN \r\n";
            sql += "     UPDATE t_config_role SET role_name=@role_name WHERE Unique_code=@Unique_code \r\n";
            sql += "     SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//相同的角色名已存在，无法修改
            sql += " END ";
            SqlParameter para1 = SqlHelper.MakeInParam("role_name", roleName);
            SqlParameter para2 = SqlHelper.MakeInParam("Unique_code", roleId);
            SqlParameter[] param = new SqlParameter[] { para1, para2 };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult DeleteRoleInfo(string roleId)
        {
            string sql = "DELETE FROM t_config_role WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", roleId);
            SqlParameter[] param = new SqlParameter[] { para1 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }
    }
}
