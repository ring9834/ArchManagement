using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WConditionSearchController : WBaseController
    {
        public IActionResult Index(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            string fieldStr = string.Empty;
            string colFields = GetFieldsStrShowing(id, out fieldStr);
            ViewData["colFields"] = colFields;
            ViewData["fieldStr"] = fieldStr;
            return View();
        }

        public IActionResult SuperSearchView(string id, string userid)
        {
            ViewData["table"] = id;
            return View("SuperSearch");
        }

        public string GetFieldsStrShowing(string id, out string fieldStr)
        {
            string sql = "SELECT t1.col_name,t1.show_name FROM t_config_col_dict t1 \r\n";
            sql += "INNER JOIN  t_config_field_show_list t2 ON t1.Unique_code=t2.selected_code \r\n";
            sql += "WHERE t1.code='" + id + "'\r\n";
            sql += " ORDER BY CAST(t2.order_number AS INT) ASC";
            DataTable fieldDt = SqlHelper.GetDataTable(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();

            fieldStr = string.Empty;
            if (fieldDt.Rows.Count == 0)
                return string.Empty;

            string colFields = string.Empty;
            for (int i = 0; i < fieldDt.Rows.Count; i++)
            {
                if (i == 0)
                {
                    fieldStr = fieldDt.Rows[i]["col_name"].ToString();
                    colFields = "{field:'" + fieldDt.Rows[i]["col_name"].ToString() + "',title:'" + fieldDt.Rows[i]["show_name"].ToString() + "'}";
                }
                else
                {
                    fieldStr += "," + fieldDt.Rows[i]["col_name"].ToString();
                    colFields += ",{field:'" + fieldDt.Rows[i]["col_name"].ToString() + "',title:'" + fieldDt.Rows[i]["show_name"].ToString() + "'}";
                }
            }
            fieldDt.Dispose();
            return colFields;
        }

        public IActionResult ShowInitialSearchRecs(string tableName, string fieldsStr, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName;
            string fields = "Unique_code," + fieldsStr;
            string where = " 1=1 AND is_deleted <> '1' ";
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult CreateSearchControls(string id)
        {
            string sql = "SELECT t2.col_name,t2.show_name,t3.code_value,t3.code_name FROM t_config_primary_search AS t1 INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code\r\n ";
            sql += "INNER JOIN t_config_codes AS t3 ON t1.search_code=t3.Unique_code \r\n";
            sql += "WHERE t2.code='" + id + "' ORDER BY t1.order_number ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return View("PrimSearch", dt);
        }

        public IActionResult CreateSuperSearchControls(string id)
        {
            string sql = "SELECT t2.col_name,t2.show_name,t3.code_value,t3.code_name FROM t_config_primary_search AS t1 INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code\r\n ";
            sql += "INNER JOIN t_config_codes AS t3 ON t1.search_code=t3.Unique_code \r\n";
            sql += "WHERE t2.code='" + id + "' ORDER BY t1.order_number ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetSearchResultByCon(string tableName, string fieldsStr, int pageSize, int pageIndex, List<SearchCondtionModel> pList)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName;
            string fields = "Unique_code," + fieldsStr;
            string where = " 1=1  AND is_deleted <> '1' ";
            SqlParameter[] pms = new SqlParameter[pList.Count];
            for (int i = 0; i < pList.Count; i++)
            {
                SearchCondtionModel md = pList[i];
                where += " AND " + md.ColName + " " + md.Oprator + " @" + md.ColName;
                if (md.Oprator.ToLower().Contains("like"))
                    pms[i] = SqlHelper.MakeInParam(md.ColName, "%" + md.InputValue + "%");
                else
                    pms[i] = SqlHelper.MakeInParam(md.ColName, md.InputValue);
            }

            string sort = SortService.GetSortStringByTableName(tableName);
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, pms, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult GetSuperSearchResultByCon(string tableName, string fieldsStr, int pageSize, int pageIndex, List<List<SuperSchCondtion>> pList)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName;
            string fields = "Unique_code," + fieldsStr;
            string where = " 1=1  AND is_deleted <> '1' ";
            List<SqlParameter> al = new List<SqlParameter>();
            for (int i = 0; i < pList.Count; i++)
            {
                var SuperSchList = pList[i];
                for (int j = 0; j < SuperSchList.Count; j++)
                {
                    SuperSchCondtion ss = SuperSchList[j];
                    where += " " + ss.AndOr + " " + ss.Field + " " + ss.Condition + " @" + ss.Field + "_" + j + " ";
                    if (ss.Condition.ToLower().Contains("like"))
                    {
                        SqlParameter p = SqlHelper.MakeInParam(ss.Field + "_" + j, "%" + ss.Value + "%");
                        al.Add(p);
                    }
                    else
                    {
                        SqlParameter p = SqlHelper.MakeInParam(ss.Field + "_" + j, ss.Value);
                        al.Add(p);
                    }
                }
            }
            string sort = SortService.GetSortStringByTableName(tableName);
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, al.ToArray(), ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult GetOperators()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='jstj')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetAndOrs()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='andorcode')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

    }
}
