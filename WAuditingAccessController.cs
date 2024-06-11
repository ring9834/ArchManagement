using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;

namespace ArchiveFileManagementNs.Controllers
{
    public class WAuditingAccessController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetUsers()
        {
            string sql = "SELECT A.Unique_code,A.user_name,A.nick_name,B.role_name FROM t_user AS A \r\n";
            sql += "INNER JOIN t_config_role AS B ON A.role_id=B.Unique_code ";
            DataTable table = SqlHelper.GetDataTable(sql);
            return Json(table);
        }

        public IActionResult GetSelectedUsers()
        {
            string sql = "SELECT A.Unique_code,A.user_name,A.nick_name,B.role_name FROM t_user AS A \r\n";
            sql += "INNER JOIN t_config_role AS B ON A.role_id=B.Unique_code \r\n";
            sql += "INNER JOIN t_config_auditing_access AS C ON C.selected_code=A.Unique_code";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult SaveConfigInfo(string table, List<string> list)
        {
            string sql;
            string codeStr = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                string selectedcode = list[i];
                sql = "DECLARE @colName nvarchar(50) \r\n";
                sql += "SELECT @colName = (SELECT Unique_code FROM t_config_auditing_access WHERE selected_code=" + selectedcode + ") \r\n";
                sql += "IF @colName IS NULL\r\n";
                sql += "    INSERT t_config_auditing_access(selected_code,order_number) VALUES(" + selectedcode + ",'" + i.ToString() + "')\r\n";
                sql += "ELSE \r\n";
                sql += "    UPDATE t_config_auditing_access SET order_number='" + i.ToString() + "' WHERE selected_code=" + selectedcode + "\r\n";
                SqlHelper.ExecNonQuery(sql, null);
                if (i == 0)
                    codeStr += selectedcode;
                else
                    codeStr += "," + selectedcode;
            }
            if (string.IsNullOrEmpty(codeStr))
                sql = "DELETE FROM t_config_auditing_access";
            else
                sql = "DELETE FROM t_config_auditing_access WHERE selected_code NOT IN(" + codeStr + ")";
            SqlHelper.ExecNonQuery(sql, null);
            int result = 1;
            return Json(new { rst = result });
        }
    }
}
