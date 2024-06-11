using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System.Data;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WDepartMentController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddDepartView()
        {
            return View("AddDepart");
        }

        public IActionResult ModiDepartView(string id)
        {
            string sql = "SELECT Unique_code,parent_id,department_name,order_number FROM t_config_department WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            DataRow dr = table.Rows[0];
            ViewData["name"] = dr["department_name"];
            ViewData["order"] = dr["order_number"];
            return View("ModifyDepart");
        }

        public IActionResult GetDepartments()
        {
            string sql = "SELECT Unique_code,parent_id,department_name as name,order_number FROM t_config_department ORDER BY order_number ASC";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult AddDepartInfo(string parent, string departName, string orderId)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("parent_id", parent);
            SqlParameter para2 = SqlHelper.MakeInParam("department_name", departName);
            SqlParameter para3 = SqlHelper.MakeInParam("order_number", orderId);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3 };
            string sql = "IF(SELECT COUNT(*) FROM t_config_department WHERE parent_id=@parent_id AND department_name=@department_name) = 0 \r\n";
            sql += " BEGIN \r\n";
            sql += "    INSERT INTO t_config_department(parent_id,department_name,order_number) VALUES(@parent_id,@department_name,@order_number) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法插入重复记录
            sql += " END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult ModiDepartInfo(string parent, string departName, string orderId, int unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("parent_id", parent);
            SqlParameter para2 = SqlHelper.MakeInParam("department_name", departName);
            SqlParameter para3 = SqlHelper.MakeInParam("order_number", orderId);
            SqlParameter para4 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4 };
            string sql = "IF (SELECT COUNT(*) FROM t_config_department WHERE department_name=@department_name AND Unique_code != @Unique_code AND parent_id=@parent_id) = 0 \r\n";//同一父功能下不能有同名的，不同父功能下可能同名
            sql += "BEGIN \r\n";
            sql += "    UPDATE t_config_department SET department_name=@department_name,order_number=@order_number WHERE Unique_code=@Unique_code";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法插入重复记录
            sql += " END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult DeleteDepartInfo(int unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1 };
            string sql = "DELETE FROM t_config_department WHERE Unique_code=@Unique_code";
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }
    }
}
