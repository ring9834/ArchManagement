using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;

namespace ArchiveFileManagementNs.Controllers
{
    public class WConfigShowListController : WBaseController
    {
        public IActionResult Index(string id)
        {
            ViewData["table"] = id;
            return View();
        }

        public IActionResult GetFieldsByTableName(string id)
        {
            string sql = "SELECT Unique_code,col_name,show_name FROM t_config_col_dict WHERE code='" + id + "' AND field_type = 0";
            DataTable table = SqlHelper.GetDataTable(sql);
            return Json(table);
        }

        public IActionResult GetSelectedFields(string id)
        {
            string sql = "SELECT t2.Unique_code,t2.col_name,t2.show_name FROM t_config_field_show_list AS t1 INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code WHERE t2.code='" + id + "' ORDER BY CAST(t1.order_number AS INT) ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult SaveConfigInfo(string table, List<string> list)
        {
            string sql = string.Empty;
            string codeStr = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                string selectedcode = list[i];
                sql = "DECLARE @colName nvarchar(50) \r\n";
                sql += "SELECT @colName = (SELECT col_name FROM t_config_col_dict WHERE Unique_code IN(SELECT selected_code FROM t_config_field_show_list WHERE selected_code=" + selectedcode + "))\r\n";
                sql += "IF @colName IS NULL\r\n";
                sql += "INSERT t_config_field_show_list(selected_code,order_number) VALUES(" + selectedcode + ",'" + i.ToString() + "')\r\n";
                sql += "ELSE \r\n";
                sql += "UPDATE t_config_field_show_list SET order_number='" + i.ToString() + "' WHERE selected_code=" + selectedcode + "\r\n";
                SqlHelper.ExecNonQuery(sql, null);
                if (i == 0)
                    codeStr += selectedcode;
                else
                    codeStr += "," + selectedcode;
            }
            if (string.IsNullOrEmpty(codeStr))
                sql = "DELETE FROM t_config_field_show_list WHERE selected_code IN (SELECT Unique_code FROM t_config_col_dict WHERE code='" + table + "')";
            else
                sql = "DELETE FROM t_config_field_show_list WHERE selected_code NOT IN(" + codeStr + ") AND selected_code IN (SELECT Unique_code FROM t_config_col_dict WHERE code='" + table + "')";
            SqlHelper.ExecNonQuery(sql, null);
            int result = 1;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }
    }
}
