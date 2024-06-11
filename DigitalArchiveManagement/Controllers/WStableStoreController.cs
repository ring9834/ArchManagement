using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;
using NetCoreDbUtility;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WStableStoreController : WBaseController
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
            string fields = "Unique_code,yw,yw_xml," + fieldsStr;
            string where = " 1=1 And store_type='2' AND is_deleted <> '1' ";//管理库中的数据
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table, conditon = where, pms = "", searchmode = 0 });
        }

        public IActionResult CreateSearchControls(string id)
        {
            string sql = "SELECT t2.col_name,t2.show_name,t3.code_value,t3.code_name FROM t_config_primary_search AS t1 INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code\r\n ";
            sql += "INNER JOIN t_config_codes AS t3 ON t1.search_code=t3.Unique_code \r\n";
            sql += "WHERE t2.code='" + id + "' ORDER BY t1.order_number ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return View("PrimSearch", dt);
        }

        public IActionResult GetSearchResultByCon(string tableName, string fieldsStr, int pageSize, int pageIndex, List<SearchCondtionModel> pList)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName;
            string fields = "Unique_code,yw,yw_xml," + fieldsStr;
            string where = " 1=1 And store_type='2' AND is_deleted <> '1' ";//资源总库中的数据
            List<SqlParameter> ps = new List<SqlParameter>();
            for (int i = 0; i < pList.Count; i++)
            {
                SearchCondtionModel md = pList[i];
                where += " AND " + md.ColName + " " + md.Oprator + " @" + md.ColName;
                if (md.Oprator.ToLower().Contains("like"))
                {
                    SqlParameter p = SqlHelper.MakeInParam(md.ColName, "%" + md.InputValue + "%");
                    ps.Add(p);
                }
                else
                {
                    SqlParameter p = SqlHelper.MakeInParam(md.ColName, md.InputValue);
                    ps.Add(p);
                }
            }

            string sort = SortService.GetSortStringByTableName(tableName);
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ps.ToArray(), ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table, conditon = where, pms = pList, searchmode = 1 });//searchmode = 0 表示初始检索，1表示基本检索，2表示高级检索
        }

        public IActionResult GetSuperSearchResultByCon(string tableName, string fieldsStr, int pageSize, int pageIndex, List<List<SuperSchCondtion>> pList)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName;
            string fields = "Unique_code,yw,yw_xml," + fieldsStr;
            string where = " 1=1 And store_type='2' AND is_deleted <> '1' ";//资源总库中的数据
            List<SqlParameter> al = new List<SqlParameter>();
            for (int i = 0; i < pList.Count; i++)
            {
                string orop = " AND (";
                string andop = "";
                var SuperSchList = pList[i];
                //IEnumerable<SuperSchCondtion> orlist = SuperSchList.Where(s => s.AndOr.ToLower().Equals("or"));
                if (SuperSchList.Count > 1)
                {
                    for (int j = 0; j < SuperSchList.Count; j++)
                    {
                        SuperSchCondtion ss = SuperSchList[j];
                        if (orop.Equals(" AND ("))
                            orop += ss.Field + " " + ss.Condition + " @" + ss.Field + "_" + j + " ";
                        else
                            orop += ss.AndOr + " " + ss.Field + " " + ss.Condition + " @" + ss.Field + "_" + j + " ";

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
                    orop += ")";
                    where += orop;
                }
                else
                { //SuperSchList.Count == 1
                    for (int j = 0; j < SuperSchList.Count; j++)
                    {
                        SuperSchCondtion ss = SuperSchList[j];
                        andop += " " + ss.AndOr + " " + ss.Field + " " + ss.Condition + " @" + ss.Field + "_" + j + " ";

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
                    where += andop;
                }
            }
            string sort = SortService.GetSortStringByTableName(tableName);
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, al.ToArray(), ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table, conditon = where, pms = pList, searchmode = 2 });
        }

        public IActionResult SuperSearchView(string id, string userid)
        {
            ViewData["table"] = id;
            return View("SuperSearch");
        }

        public IActionResult ToManageStore(string table, string[] ids)
        {
            string idstr = string.Empty;
            for (int i = 0; i < ids.Length; i++)
            {
                if (i == 0)
                    idstr += ids[i];
                else
                    idstr += "," + ids[i];
            }
            string sql = "UPDATE " + table + " SET store_type='1' WHERE Unique_code IN(" + idstr + ")";
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult AllToManageStore(string table, string where, string pms, string searchmode)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            { }
            else if (int.Parse(searchmode) == 1)//基本搜索的传参
            {
                Newtonsoft.Json.Linq.JArray prms = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(pms);
                List<SearchCondtionModel> pList = prms.ToObject<List<SearchCondtionModel>>();
                for (int i = 0; i < pList.Count; i++)
                {
                    SearchCondtionModel md = pList[i];
                    //where += " AND " + md.ColName + " " + md.Oprator + " @" + md.ColName;
                    if (md.Oprator.ToLower().Contains("like"))
                    {
                        SqlParameter p = SqlHelper.MakeInParam(md.ColName, "%" + md.InputValue + "%");
                        list.Add(p);
                    }
                    else
                    {
                        SqlParameter p = SqlHelper.MakeInParam(md.ColName, md.InputValue);
                        list.Add(p);
                    }
                }
            }
            else if (int.Parse(searchmode) == 2)//高级搜索的传参
            {
                Newtonsoft.Json.Linq.JArray prms = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(pms);
                List<List<SuperSchCondtion>> pList = prms.ToObject<List<List<SuperSchCondtion>>>();
                for (int i = 0; i < pList.Count; i++)
                {
                    var SuperSchList = pList[i];
                    for (int j = 0; j < SuperSchList.Count; j++)
                    {
                        SuperSchCondtion ss = SuperSchList[j];
                        //where += " " + ss.AndOr + " " + ss.Field + " " + ss.Condition + " @" + ss.Field + "_" + j + " ";
                        if (ss.Condition.ToLower().Contains("like"))
                        {
                            SqlParameter p = SqlHelper.MakeInParam(ss.Field + "_" + j, "%" + ss.Value + "%");
                            list.Add(p);
                        }
                        else
                        {
                            SqlParameter p = SqlHelper.MakeInParam(ss.Field + "_" + j, ss.Value);
                            list.Add(p);
                        }
                    }
                }
            }
            string sql = "UPDATE " + table + " SET store_type='1' WHERE " + where;
            int result = SqlHelper.ExecNonQuery(sql, list.ToArray());
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult ExportToExcel(string table, string where, string pms, string searchmode)//从前端的XmlHttpRequest传递FormData参数而来 2020年3月14日
        {
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            { }
            else if (int.Parse(searchmode) == 1)//基本搜索的传参
            {
                Newtonsoft.Json.Linq.JArray prms = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(pms);
                List<SearchCondtionModel> pList = prms.ToObject<List<SearchCondtionModel>>();
                for (int i = 0; i < pList.Count; i++)
                {
                    SearchCondtionModel md = pList[i];
                    //where += " AND " + md.ColName + " " + md.Oprator + " @" + md.ColName;
                    if (md.Oprator.ToLower().Contains("like"))
                    {
                        SqlParameter p = SqlHelper.MakeInParam(md.ColName, "%" + md.InputValue + "%");
                        list.Add(p);
                    }
                    else
                    {
                        SqlParameter p = SqlHelper.MakeInParam(md.ColName, md.InputValue);
                        list.Add(p);
                    }
                }
            }
            else if (int.Parse(searchmode) == 2)//高级搜索的传参
            {
                Newtonsoft.Json.Linq.JArray prms = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(pms);
                List<List<SuperSchCondtion>> pList = prms.ToObject<List<List<SuperSchCondtion>>>();
                for (int i = 0; i < pList.Count; i++)
                {
                    var SuperSchList = pList[i];
                    for (int j = 0; j < SuperSchList.Count; j++)
                    {
                        SuperSchCondtion ss = SuperSchList[j];
                        //where += " " + ss.AndOr + " " + ss.Field + " " + ss.Condition + " @" + ss.Field + "_" + j + " ";
                        if (ss.Condition.ToLower().Contains("like"))
                        {
                            SqlParameter p = SqlHelper.MakeInParam(ss.Field + "_" + j, "%" + ss.Value + "%");
                            list.Add(p);
                        }
                        else
                        {
                            SqlParameter p = SqlHelper.MakeInParam(ss.Field + "_" + j, ss.Value);
                            list.Add(p);
                        }
                    }
                }
            }

            string sql = "SELECT t1.col_name,t1.show_name FROM t_config_col_dict t1 \r\n";
            sql += "INNER JOIN  t_config_field_show_list t2 ON t1.Unique_code=t2.selected_code \r\n";
            sql += "WHERE t1.code='" + table + "'\r\n";
            sql += " ORDER BY CAST(t2.order_number AS INT) ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            string fieldStr = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i == 0)
                    fieldStr += dt.Rows[i]["col_name"].ToString() + " AS " + dt.Rows[i]["show_name"].ToString();
                else
                    fieldStr += "," + dt.Rows[i]["col_name"].ToString() + " AS " + dt.Rows[i]["show_name"].ToString();
            }

            sql = "SELECT " + fieldStr + " FROM " + table + " WHERE " + where;
            dt = SqlHelper.GetDataTable(sql, list.ToArray());
            byte[] bts = ExcelHelper.GetExcelByDataTable(dt);
            FileContentResult fResult = File(bts, "application/vnd.ms-excel", DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx");
            dt.Dispose();
            return fResult;
        }
    }
}
