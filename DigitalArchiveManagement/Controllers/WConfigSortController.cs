using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;
using NetCoreDbUtility;

namespace ArchiveFileManagementNs.Controllers
{
    public class WConfigSortController : WBaseController
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
            string sql = "SELECT t2.Unique_code,t2.col_name,t2.show_name,t1.sort_code FROM t_config_sorted AS t1 INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code WHERE t2.code='" + id + "' ORDER BY order_number ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetSConditions()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='pxgz')";
            DataTable dtcondtion = SqlHelper.GetDataTable(sql, null);
            return Json(dtcondtion);
        }

        public IActionResult SaveConfigInfo(string table, List<ConfigSearchEntity> list)
        {
            string sql = string.Empty;
            string codeStr = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                ConfigSearchEntity item = list[i];
                sql = "DECLARE @colName nvarchar(50) \r\n";
                sql += "SELECT @colName = (SELECT col_name FROM t_config_col_dict WHERE Unique_code IN(SELECT selected_code FROM t_config_sorted WHERE selected_code=" + item.SelectedCode + "))\r\n";
                sql += "IF @colName IS NULL\r\n";
                sql += "INSERT t_config_sorted(selected_code,order_number,sort_code) VALUES(" + item.SelectedCode + ",'" + item.OrderNumber + "'," + item.SearchCode + ")\r\n";
                sql += "ELSE \r\n";
                sql += "UPDATE t_config_sorted SET order_number='" + item.OrderNumber + "',sort_code=" + item.SearchCode + " WHERE selected_code=" + item.SelectedCode + "\r\n";
                SqlHelper.ExecNonQuery(sql, null);
                if (i == 0)
                    codeStr += item.SelectedCode;
                else
                    codeStr += "," + item.SelectedCode;
            }
            sql = "DELETE FROM t_config_sorted WHERE selected_code NOT IN(" + codeStr + ") AND selected_code IN (SELECT Unique_code FROM t_config_col_dict WHERE code='" + table + "')";
            SqlHelper.ExecNonQuery(sql, null);
            int result = 1;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }
    }
}
