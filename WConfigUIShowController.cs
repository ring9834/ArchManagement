using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WConfigUIShowController : WBaseController
    {
        public IActionResult Index(string id)
        {
            ViewData["table"] = id;
            return View();
        }

        public IActionResult GetControlTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='XSFS')";
            DataTable table = SqlHelper.GetDataTable(sql);
            return Json(table);
        }

        public IActionResult GetHlpCodes()
        {
            string sql = "SELECT Unique_code,base_name,code_key FROM t_config_codes_base";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        /// <summary>
        /// field_type='0'表示非管理字段，即业务字段
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetFldsByCode(string id)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("codeName", id);
            SqlParameter[] param = new SqlParameter[] { para1 };
            string sql = "SELECT col_name,show_name,col_show_type,col_show_value,col_order,Unique_code FROM t_config_col_dict WHERE code=@codeName and field_type='0' ORDER BY CAST(col_order AS INT) ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, param);
            return Json(dt);
        }

        public IActionResult SaveShowCtrlType(string table, string id,string showType,string showValue, string orderValue)
        {
            object sv = DBNull.Value;
            if (showValue != null)
                sv = showValue;
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", id);
            SqlParameter para2 = SqlHelper.MakeInParam("code", table);
            SqlParameter para3 = SqlHelper.MakeInParam("col_show_type", showType);
            SqlParameter para4 = SqlHelper.MakeInParam("col_show_value", sv);
            SqlParameter para5 = SqlHelper.MakeInParam("col_order", orderValue);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5 };
            string sql = "UPDATE t_config_col_dict SET code=@code,col_show_type=@col_show_type,col_show_value=@col_show_value,col_order=@col_order WHERE Unique_code=@Unique_code";
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        /// <summary>
        /// 集中（批量）保存列的排序
        /// </summary>
        /// <param name="table"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public IActionResult SaveColOrders(string table,List<ColOrderModel> cols)
        {
            int result = 0;
            for (int i = 0; i < cols.Count; i++)
            {
                object rd = DBNull.Value;
                if (cols[i].Order != null)
                    rd = cols[i].Order;

                object hlp = DBNull.Value;
                if (cols[i].ShowValue != null)
                    hlp = cols[i].ShowValue;

                object tp = DBNull.Value;
                if (cols[i].ShowType != null)
                    tp = cols[i].ShowType;

                SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", cols[i].ID);
                SqlParameter para2 = SqlHelper.MakeInParam("code", table);
                SqlParameter para3 = SqlHelper.MakeInParam("col_order", rd);
                SqlParameter para4 = SqlHelper.MakeInParam("col_show_type", tp);
                SqlParameter para5 = SqlHelper.MakeInParam("col_show_value", hlp);
                SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5 };
                string sql = "UPDATE t_config_col_dict SET col_order=@col_order,col_show_type=@col_show_type,col_show_value=@col_show_value WHERE code=@code AND Unique_code=@Unique_code";
                result = SqlHelper.ExecNonQuery(sql, param);

            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }
    }
}
