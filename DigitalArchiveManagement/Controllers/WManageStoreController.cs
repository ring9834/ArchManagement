using ArchiveFileManagementNs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using Newtonsoft.Json;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WManageStoreController : WBaseController
    {
        private IHttpContextAccessor _accessor;
        public WManageStoreController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public IActionResult Index(string id, string userid, string other, string othertwo)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            ViewData["HasContent"] = other;
            ViewData["ContentType"] = othertwo;
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

        /// <summary>
        /// t1.col_show_type IS NOT NULL 表示 不进行界面设置的字段，在目录著录界面将不显示出来
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CatalogAddView(string id, string userid)
        {
            string selectFieldStr = "Unique_code";//所有搜索的语句，头部都加上Unique_code字段，只不过在搜索结果页不让它显示出来。 
            string sql = "SELECT t1.col_name,t1.show_name,t1.col_null,t1.col_maxlen,t1.col_show_type,t1.col_show_value,t2.code_value,t3.code_value as dataType FROM t_config_col_dict t1 \r\n";
            sql += "LEFT JOIN  t_config_codes t2 ON t2.Unique_code=t1.col_show_type \r\n";
            sql += "LEFT JOIN  t_config_codes t3 ON t3.Unique_code=t1.col_datatype \r\n";
            sql += "WHERE t1.code='" + id + "' AND field_type=0 AND t1.col_show_type IS NOT NULL AND t1.col_show_type <> '-1' \r\n";
            sql += " ORDER BY CAST(t1.col_order AS INT) ASC";
            DataTable fieldDt = SqlHelper.GetDataTable(sql);
            List<ColDictionary> colList = new List<ColDictionary>();
            for (int i = 0; i < fieldDt.Rows.Count; i++)
            {
                DataRow dr = fieldDt.Rows[i];
                selectFieldStr += "," + dr["col_name"].ToString() + " AS " + dr["show_name"].ToString();
                ColDictionary coldict = new ColDictionary(dr["show_name"].ToString(), dr["col_name"].ToString(), false, dr["col_maxlen"].ToString());
                bool isColNull = dr["col_null"] == DBNull.Value ? false : Boolean.Parse(dr["col_null"].ToString());
                coldict.ColNull = isColNull;
                coldict.ColShowValue = dr["code_value"] == DBNull.Value ? string.Empty : dr["code_value"].ToString();//下拉框、文本框等的代码值
                coldict.ColShowType = dr["col_show_type"] == DBNull.Value ? string.Empty : dr["col_show_type"].ToString();//下拉框、文本框等的unique_code
                coldict.BaseCode = dr["col_show_value"] == DBNull.Value ? string.Empty : dr["col_show_value"].ToString();//密级、保管期限等
                coldict.DataType = dr["dataType"] == DBNull.Value ? string.Empty : dr["dataType"].ToString();//数据类型，如，字符串或数字类型 added on 20201110
                colList.Add(coldict);
            }
            fieldDt.Dispose();
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View("CatalogAdd", colList);
        }

        public IActionResult CatalogUpdateView(string id, string userid, string other)
        {
            string selectFieldStr = "Unique_code";//所有搜索的语句，头部都加上Unique_code字段，只不过在搜索结果页不让它显示出来。 
            string sql = "SELECT t1.col_name,t1.show_name,t1.col_null,t1.col_maxlen,t1.col_show_type,t1.col_show_value,t2.code_value FROM t_config_col_dict t1 \r\n";
            sql += "INNER JOIN  t_config_codes t2 ON t2.Unique_code=t1.col_show_type \r\n";
            sql += "WHERE t1.code='" + id + "' AND field_type=0 \r\n";
            sql += " ORDER BY CAST(t1.col_order AS INT) ASC";
            DataTable fieldDt = SqlHelper.GetDataTable(sql);
            List<ColDictionary> colList = new List<ColDictionary>();
            for (int i = 0; i < fieldDt.Rows.Count; i++)
            {
                DataRow dr = fieldDt.Rows[i];
                selectFieldStr += "," + dr["col_name"].ToString() + " AS " + dr["show_name"].ToString();
                ColDictionary coldict = new ColDictionary(dr["show_name"].ToString(), dr["col_name"].ToString(), false, dr["col_maxlen"].ToString());
                bool isColNull = dr["col_null"] == DBNull.Value ? false : Boolean.Parse(dr["col_null"].ToString());
                coldict.ColNull = isColNull;
                coldict.ColShowValue = dr["code_value"] == DBNull.Value ? string.Empty : dr["code_value"].ToString();//下拉框、文本框等的代码值
                coldict.ColShowType = dr["col_show_type"] == DBNull.Value ? string.Empty : dr["col_show_type"].ToString();//下拉框、文本框等的unique_code
                coldict.BaseCode = dr["col_show_value"] == DBNull.Value ? string.Empty : dr["col_show_value"].ToString();//密级、保管期限等
                colList.Add(coldict);
            }
            fieldDt.Dispose();
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            ViewData["ID"] = other;//added on 2020年5月18日
            //ViewData["rowdata"] = other;//2020年3月13日，去掉了从URL接收other参数，因考虑到other字符串太长
            return View("CatalogUpdate", colList);
        }

        public IActionResult ShowInitialSearchRecs(string tableName, string fieldsStr, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName;
            string fields = "Unique_code,yw,yw_xml," + fieldsStr;
            string where = " 1=1 And store_type='1' AND is_deleted <> '1' ";//管理库中的数据
            string sort = SortService.GetSortStringByTableName(tableName);//debugged on 20201113
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table, conditon = where, pms = "", searchmode = 0 });
        }

        public IActionResult GetCodesByBaseCode(string baseCode)
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code='" + baseCode + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult AddSingleCatalog(List<FieldValuePair> list, string table, string userid)
        {
            string fieldStr = string.Empty;
            string valueStr = string.Empty;
            List<SqlParameter> lp = new List<SqlParameter>();
            for (int i = 0; i < list.Count; i++)
            {
                FieldValuePair fv = list[i];
                if (i == 0)
                {
                    fieldStr += fv.Field;
                    valueStr += "@" + fv.Field;
                    SqlParameter p = SqlHelper.MakeInParam(fv.Field, fv.Value == null ? string.Empty : fv.Value);
                    lp.Add(p);
                }
                else
                {
                    fieldStr += "," + fv.Field;
                    valueStr += ",@" + fv.Field;
                    SqlParameter p = SqlHelper.MakeInParam(fv.Field, fv.Value == null ? string.Empty : fv.Value);
                    lp.Add(p);
                }
            }
            string sql = "INSERT INTO " + table + " (" + fieldStr + ",store_type) VALUES(" + valueStr + ",'1') \r\n";
            //sql += "SELECT top 1 Unique_code FROM " + table + " ORDER BY Unique_code DESC ";
            int result = SqlHelper.ExecNonQuery(sql, lp.ToArray());

            sql = "SELECT top 1 Unique_code FROM " + table + " ORDER BY Unique_code DESC ";
            object idObj = SqlHelper.ExecuteScalar(sql, null);
            List<string> ids = new List<string>();
            ids.Add(idObj.ToString());

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "增加一条目录", "预归档", "", ipAddr);
            opInfo.OperTag = "手动录入一条目录";
            OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//手动录入一条目录的用户操作登记;2020年3月13日

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult UpdateSingleCatalog(List<FieldValuePair> list, string table, string uniquecode, string userid)
        {
            string fieldStr = string.Empty;
            List<SqlParameter> lp = new List<SqlParameter>();
            for (int i = 0; i < list.Count; i++)
            {
                FieldValuePair fv = list[i];
                if (i == 0)
                {
                    fieldStr += fv.Field + "=@" + fv.Field;
                    SqlParameter p = SqlHelper.MakeInParam(fv.Field, fv.Value == null ? string.Empty : fv.Value);
                    lp.Add(p);
                }
                else
                {
                    fieldStr += "," + fv.Field + "=@" + fv.Field;
                    SqlParameter p = SqlHelper.MakeInParam(fv.Field, fv.Value == null ? string.Empty : fv.Value);
                    lp.Add(p);
                }
            }
            string sql = "UPDATE " + table + " SET " + fieldStr + " WHERE Unique_code=" + uniquecode;
            int result = SqlHelper.ExecNonQuery(sql, lp.ToArray());

            List<string> ids = new List<string>();
            ids.Add(uniquecode);

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "修改一条目录", "预归档", "", ipAddr);
            opInfo.OperTag = "修改一条记录";
            OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//修改一条记录的用户操作登记;2020年3月13日

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult BunchUpdateView(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["searchMode"] = userid;//借用userid传参
            //ViewData["where"] = other;//2020年3月，去掉了从URL接收other和othertwo
            //ViewData["params"] = othertwo;
            return View("CatalogBunchUpdate");
        }

        public IActionResult BunchUpdateRecs(string table, string where, string pms, string searchmode, string field, string source, string dest, string flag, string userid)
        {
            string searchConditon = "", modifyContent = "";
            string sql = string.Empty;
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            {
                searchConditon = "影响表" + table + "全部记录 ";
            }
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
                searchConditon = "根据基本搜索条件 ";
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
                searchConditon = "根据高级搜索条件 ";
            }

            if (int.Parse(flag) == 0)//内容全部替换
            {
                sql = "UPDATE " + table + " SET " + field + "=@Replacement WHERE " + where;
                SqlParameter p = SqlHelper.MakeInParam("Replacement", dest);
                list.Add(p);
                modifyContent = " " + field + "字段全部内容都被替换为“" + dest + "”";
            }
            if (int.Parse(flag) == 1)//空白内容替换
            {
                sql = "UPDATE " + table + " SET " + field + "=@Replacement \r\n";
                sql += "WHERE (ltrim(rtrim(" + field + "))='' OR " + field + " IS NULL) AND " + where;
                SqlParameter p = SqlHelper.MakeInParam("Replacement", dest);
                list.Add(p);
                modifyContent = " " + field + "字段中的空白内容都被替换为“" + dest + "”";
            }
            if (int.Parse(flag) == 2)//内容内容部分替换
            {
                sql = "UPDATE " + table + " SET " + field + "=REPLACE(" + field + ",@source,@dest) \r\n";
                sql += "WHERE " + where;
                SqlParameter p1 = SqlHelper.MakeInParam("source", source);
                SqlParameter p2 = SqlHelper.MakeInParam("dest", dest);
                list.Add(p1);
                list.Add(p2);
                modifyContent = " " + field + "字段中“" + source + "”空白内容都被替换为“" + dest + "”";
            }
            int result = SqlHelper.ExecNonQuery(sql, list.ToArray());

            sql = "SELECT COUNT(Unique_code) as cnt FROM " + table + " WHERE " + where;
            object cntObj = SqlHelper.ExecuteScalar(sql, list.ToArray());
            int cnt = int.Parse(cntObj.ToString());

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "批量修改目录", "预归档", "", ipAddr);
            opInfo.OperTag = "修改了" + cnt + "条目录记录：" + searchConditon + modifyContent;
            if (cnt <= 500)
            {
                sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
                DataTable dt = SqlHelper.GetDataTable(sql, list.ToArray());
                OperateRecHlp.RcdUserOpration2(opInfo, dt);//修改一批记录，记录到用户操作记录中;2020年3月13日
            }
            else
            {
                List<string> ids = new List<string>();
                OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//修改一批记录，记录到用户操作记录中;2020年3月13日
            }

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult GetFieldsByTableName(string id)
        {
            string sql = "SELECT Unique_code,col_name,show_name FROM t_config_col_dict WHERE code='" + id + "' AND field_type = 0";
            DataTable table = SqlHelper.GetDataTable(sql);
            return Json(table);
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
            string where = " 1=1 And store_type='1' AND is_deleted <> '1' ";//管理库中的数据
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
            string where = " 1=1 And store_type='1' AND is_deleted <> '1' ";//管理库中的数据

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

        public IActionResult DeleteSingleCatalog(string table, string userid, string uniquecode)
        {
            //string sql = "DELETE FROM " + table + " WHERE Unique_code=" + uniquecode;
            //int result = SqlHelper.ExecNonQuery(sql, null);

            List<string> list = new List<string>();
            list.Add(uniquecode);

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "删除一条记录", "预归档", "", ipAddr);
            opInfo.OperTag = "删除一条记录";
            OperateRecHlp.RcdUserOprationDel(opInfo, list);//删除一条记录;2020年3月11日

            int result = 1;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        //2020年3月11日，将string[] ids修改为List<string> ids
        public IActionResult DeleteBunchCatalog(string table, string userid, List<string> ids)
        {
            //string where = "";
            //for (int i = 0; i < ids.Count; i++)
            //{
            //    if (i == 0)
            //        where += " Unique_code=" + ids[i];
            //    else
            //        where += " OR Unique_code=" + ids[i];
            //}
            //string sql = "DELETE FROM " + table + " WHERE " + where;
            //int result = SqlHelper.ExecNonQuery(sql, null);

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "删除多条记录", "预归档", "", ipAddr);
            opInfo.OperTag = "删除一批记录";
            OperateRecHlp.RcdUserOprationDel(opInfo, ids);//删除一批记录，记录到用户操作记录中;2020年3月11日
            int result = 1;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult ToStableStore(string table, string userid, string[] ids)
        {
            string idstr = string.Empty;
            for (int i = 0; i < ids.Length; i++)
            {
                if (i == 0)
                    idstr += ids[i];
                else
                    idstr += "," + ids[i];
            }
            string sql = "UPDATE " + table + " SET store_type='2' WHERE Unique_code IN(" + idstr + ")";
            int result = SqlHelper.ExecNonQuery(sql, null);

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "所选择记录入档案总库", "预归档", "", ipAddr);
            opInfo.OperTag = "预归档库入档案总库";
            OperateRecHlp.RcdUserOprationCommon(opInfo, ids.ToList());//挂接一批原文的用户操作登记;2020年3月13日

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult AllToStableStore(string table, string where, string pms, string searchmode, string userid)
        {
            string searchConditon = "";
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            {
                searchConditon = "把表" + table + "的全部记录 ";
            }
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
                searchConditon = "把表" + table + "基本搜索的全部记录 ";
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
                searchConditon = "把表" + table + "高级搜索的全部记录 ";
            }

            string sql = "SELECT COUNT(Unique_code) as cnt FROM " + table + " WHERE " + where;
            object cntObj = SqlHelper.ExecuteScalar(sql, list.ToArray());

            int cnt = int.Parse(cntObj.ToString());
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "多页（搜索结果）所有记录入档案总库", "预归档", "", ipAddr);
            opInfo.OperTag = searchConditon + "入档案总库，影响记录" + cnt + "条";

            if (cnt <= 500)
            {
                sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
                DataTable dt = SqlHelper.GetDataTable(sql, list.ToArray());
                OperateRecHlp.RcdUserOpration2(opInfo, dt);//把所有页（可能是搜索结果的所有页）入资源总库，记录到用户操作记录中;2020年3月13日
                dt.Dispose();
            }
            else
            {
                List<string> ids = new List<string>();
                OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//记录超过数量时，就不记录影响的记录ID了
            }

            sql = "UPDATE " + table + " SET store_type='2' WHERE " + where;
            int result = SqlHelper.ExecNonQuery(sql, list.ToArray());

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult GenerateArchNumber(string table, string where, string pms, string searchmode, string userid)
        {
            string searchConditon = "";
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            {
                searchConditon = "把表" + table + "的全部记录 ";
            }
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
                searchConditon = "把表" + table + "基本搜索的全部记录 ";
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
                searchConditon = "把表" + table + "高级搜索的全部记录 ";
            }

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT archive_body,archive_num_field,connect_char FROM t_config_archive_num_makeup WHERE code_name='" + table + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                string xmlfile = dt.Rows[0]["archive_body"].ToString();
                string dhfield = dt.Rows[0]["archive_num_field"].ToString();
                //string prefix = dt.Rows[0]["archive_prefix"].ToString();
                string connchar = dt.Rows[0]["connect_char"].ToString();
                dt.Dispose();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlfile.ToString());
                XmlNodeList nodeList = doc.SelectNodes(@"ArchiveNumMakeup/MakeupItem");
                string dhstr = "";
                string dhpartNull = "(";
                for (int i = 0; i < nodeList.Count; i++)
                {
                    string prefix = nodeList[i].Attributes["Prefix"].Value;
                    string pre = "";
                    if (!string.IsNullOrEmpty(prefix))
                        pre = "'" + prefix + "'+";
                    else
                        pre = "''+";

                    string fld = nodeList[i].Attributes["value"].Value;
                    if (i == nodeList.Count - 1)
                        dhstr += pre + fld;
                    else
                        dhstr += pre + fld + "+'" + connchar + "'+";

                    if (i == 0)
                        dhpartNull += fld + " is null ";
                    else
                        dhpartNull += " or " + fld + " is null ";
                }
                dhpartNull += ") ";

                //sql = "UPDATE " + table + " SET " + dhfield + "=" + dhstr + " WHERE " + where;
                //int result = SqlHelper.ExecNonQuery(sql, list.ToArray());

                //modifed on 20201108
                sql = "declare @dhlen int,@dhlenLimit int \r\n";
                sql += "select @dhlen = MAX(len(" + dhstr + ")) from " + table + " WHERE " + where + " \r\n";//MAX，求最大
                sql += "select @dhlenLimit = cast(col_maxlen as int) from t_config_col_dict where col_name='" + dhfield + "' and code='" + table + "' \r\n";
                sql += "if(@dhlen > @dhlenLimit) \r\n";
                sql += "begin \r\n";
                sql += "    select -2 \r\n"; //欲生成的档号长度超过了档号配置所允许的长度
                sql += "end \r\n";
                sql += "else \r\n";
                sql += "begin \r\n";
                sql += "    IF(SELECT COUNT(Unique_code) FROM " + table + " WHERE " + where + " AND " + dhpartNull + ") = 0 \r\n";
                sql += "    begin \r\n";
                sql += "        UPDATE " + table + " SET " + dhfield + "=" + dhstr + " WHERE " + where + " \r\n";
                sql += "        select 1 \r\n";
                sql += "    end \r\n";
                sql += "    else \r\n";
                sql += "    begin \r\n";
                sql += "        select -3 \r\n"; //组成档号的字段中的实际数据，有null的值，则提示前端不能生成档号
                sql += "    end \r\n";
                sql += "end \r\n";
                object rltObj = SqlHelper.ExecuteScalar(sql, list.ToArray());
                int result = int.Parse(rltObj.ToString());

                sql = "SELECT COUNT(Unique_code) as cnt FROM " + table + " WHERE " + where;
                object cntObj = SqlHelper.ExecuteScalar(sql, list.ToArray());

                int cnt = int.Parse(cntObj.ToString());
                string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
                OperationInfo opInfo = MakeOperInfo(userid, table, "批量目录生成档号", "预归档", "", ipAddr);
                opInfo.OperTag = searchConditon + "生成档号，影响记录" + cnt + "条";

                if (cnt <= 500)
                {
                    sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
                    DataTable dtrec = SqlHelper.GetDataTable(sql, list.ToArray());
                    OperateRecHlp.RcdUserOpration2(opInfo, dtrec);//批量生成档号记录到用户操作记录中;2020年3月13日
                    dtrec.Dispose();
                }
                else
                {
                    List<string> ids = new List<string>();
                    OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//记录超过数量时，就不记录影响的记录ID了
                }

                string title = "档号生成成功！";
                dt.Dispose();
                return Json(new { rst = result, info = title });
            }
            int rlt = 0;
            string tle = "档号需要配置后，才能生成档号！请到管理配置的档号配置操作后继续！";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
        }

        public IActionResult GenerateSArchNumber(string table, List<string> pms, string userid)
        {
            string searchConditon = "";
            string where = "";

            for (int i = 0; i < pms.Count; i++)
            {
                if (i == 0)
                {
                    where += " Unique_code=" + pms[i];
                    searchConditon += " ID等于" + pms[i];
                }
                else
                {
                    where += " OR Unique_code=" + pms[i];
                    searchConditon += "," + pms[i];
                }
            }
            searchConditon += "的记录";

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT archive_body,archive_num_field,connect_char FROM t_config_archive_num_makeup WHERE code_name='" + table + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                string xmlfile = dt.Rows[0]["archive_body"].ToString();
                string dhfield = dt.Rows[0]["archive_num_field"].ToString();
                string connchar = dt.Rows[0]["connect_char"].ToString();
                dt.Dispose();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlfile.ToString());
                XmlNodeList nodeList = doc.SelectNodes(@"ArchiveNumMakeup/MakeupItem");
                string dhstr = "";
                string dhpartNull = "(";
                for (int i = 0; i < nodeList.Count; i++)
                {
                    string prefix = nodeList[i].Attributes["Prefix"].Value;
                    string pre = "";
                    if (!string.IsNullOrEmpty(prefix))
                        pre = "'" + prefix + "'+";
                    else
                        pre = "''+";

                    string fld = nodeList[i].Attributes["value"].Value;
                    if (i == nodeList.Count - 1)
                        dhstr += pre + fld;
                    else
                        dhstr += pre + fld + "+'" + connchar + "'+";

                    if (i == 0)
                        dhpartNull += fld + " is null ";
                    else
                        dhpartNull += " or " + fld + " is null ";
                }
                dhpartNull += ") ";

                //sql = "UPDATE " + table + " SET " + dhfield + "=" + dhstr + " WHERE " + where;
                //int result = SqlHelper.ExecNonQuery(sql, null);
                //List<string> ids = new List<string>();

                //modifed on 20201108
                sql = "declare @dhlen int,@dhlenLimit int \r\n";
                sql += "select @dhlen = MAX(len(" + dhstr + ")) from " + table + " WHERE " + where + " \r\n";//MAX，求最大
                sql += "select @dhlenLimit = cast(col_maxlen as int) from t_config_col_dict where col_name='" + dhfield + "' and code='" + table + "' \r\n";
                sql += "if(@dhlen > @dhlenLimit) \r\n";
                sql += "begin \r\n";
                sql += "    select -2 \r\n"; //欲生成的档号长度超过了档号配置所允许的长度
                sql += "end \r\n";
                sql += "else \r\n";
                sql += "begin \r\n";
                sql += "    IF(SELECT COUNT(Unique_code) FROM " + table + " WHERE " + where + " AND " + dhpartNull + ") = 0 \r\n";
                sql += "    begin \r\n";
                sql += "        UPDATE " + table + " SET " + dhfield + "=" + dhstr + " WHERE " + where + " \r\n";
                sql += "        select 1 \r\n";
                sql += "    end \r\n";
                sql += "    else \r\n";
                sql += "    begin \r\n";
                sql += "        select -3 \r\n"; //组成档号的字段中的实际数据，有null的值，则提示前端不能生成档号
                sql += "    end \r\n";
                sql += "end \r\n";

                object rltObj = SqlHelper.ExecuteScalar(sql, null);
                int result = int.Parse(rltObj.ToString());

                string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
                OperationInfo opInfo = MakeOperInfo(userid, table, "单条目录生成档号", "预归档", "", ipAddr);
                opInfo.OperTag = searchConditon + "生成档号，影响记录" + pms.Count + "条";
                OperateRecHlp.RcdUserOprationCommon(opInfo, pms);

                string title = "档号生成成功！";
                dt.Dispose();
                return Json(new { rst = result, info = title });
            }
            int rlt = 0;
            string tle = "档号需要配置后，才能生成档号！请到管理配置的档号配置操作后继续！";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
        }

        public async Task<IActionResult> EncryptSContent(string table, List<string> pms, string userid)
        {
            await Task.Delay(5);
            string searchConditon = "";
            string where = "";
            int rlt = 0;
            string tle = "";

            for (int i = 0; i < pms.Count; i++)
            {
                if (i == 0)
                {
                    where += " Unique_code=" + pms[i];
                    searchConditon += " ID等于" + pms[i];
                }
                else
                {
                    where += " OR Unique_code=" + pms[i];
                    searchConditon += "," + pms[i];
                }
            }
            searchConditon += "的记录";

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            int counter = 0;
            string openPwd = string.Empty;
            string permitPwd = string.Empty;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    string path = ywrootPath + row[0].ToString();
                    path = path.Replace("\\", "/");
                    if (System.IO.File.Exists(path))
                    {
                        if (string.IsNullOrEmpty(permitPwd))//
                        {
                            GetPasswords(out openPwd, out permitPwd);
                            if (string.IsNullOrEmpty(permitPwd))
                            {
                                rlt = 0;
                                tle = "原文打开密码和操作密码均还未配置，请到管理配置中配置后继续！";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }

                        FileStream fs = new FileStream(path, FileMode.Open);//使用FileStream而不是使用LoadFromFile,是为了解决pdf文件被进程占用的问题 20200629
                        long size = fs.Length;
                        byte[] array = new byte[size];
                        fs.Read(array, 0, array.Length);
                        fs.Close();//必须关闭
                        PdfDocument pdf = new PdfDocument(array, permitPwd);//不论pdf是否加密，最好都带上originalPermissionPassword这个参数，因为如果已加密，则会弹出异常

                        if (!pdf.IsEncrypted)//若已加密，则不用再加密
                        {
                            pdf.Security.Encrypt(openPwd, permitPwd, PdfPermissionsFlags.Print | PdfPermissionsFlags.CopyContent, PdfEncryptionKeySize.Key128Bit);

                            //将文档保存到文件
                            pdf.SaveToFile(path, FileFormat.PDF);
                            counter++;
                        }
                    }
                }
            }

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "原文批量加密", "原文加密", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文加密，影响记录" + counter + "条，" + counter + "条加密成功，" + (pms.Count - counter) + "条未成功（可能原因为原文不存在或路径有误或者已被加密。）";
            OperateRecHlp.RcdUserOprationCommon(opInfo, pms);

            rlt = 1;
            tle = counter + "条加密成功，" + (pms.Count - counter) + "条未成功（可能原因为原文不存在或路径有误或者已被加密。）";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
        }

        public async Task<IActionResult> EncryptContent(string table, string where, string pms, string searchmode, string userid)
        {
            await Task.Delay(5);
            string searchConditon = "";
            int rlt = 0;
            string tle = "";

            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            {
                searchConditon = "把表" + table + "的全部记录 ";
            }
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
                searchConditon = "把表" + table + "基本搜索的全部记录 ";
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
                searchConditon = "把表" + table + "高级搜索的全部记录 ";
            }

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            int counter = 0;
            string openPwd = string.Empty;
            string permitPwd = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    string path = ywrootPath + row[0].ToString();
                    path = path.Replace("\\", "/");
                    if (System.IO.File.Exists(path))
                    {
                        if (string.IsNullOrEmpty(permitPwd))//
                        {
                            GetPasswords(out openPwd, out permitPwd);
                            if (string.IsNullOrEmpty(permitPwd))
                            {
                                rlt = 0;
                                tle = "原文打开密码和操作密码均还未配置，请到管理配置中配置后继续！";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }

                        FileStream fs = new FileStream(path, FileMode.Open);//使用FileStream而不是使用LoadFromFile,是为了解决pdf文件被进程占用的问题 20200629
                        long size = fs.Length;
                        byte[] array = new byte[size];
                        fs.Read(array, 0, array.Length);
                        fs.Close();//必须关闭
                        PdfDocument pdf = new PdfDocument(array, permitPwd);//不论pdf是否加密，最好都带上originalPermissionPassword这个参数，因为如果已加密，则会弹出异常
                        if (!pdf.IsEncrypted)
                        {
                            //加密PDF文件
                            pdf.Security.Encrypt(openPwd, permitPwd, PdfPermissionsFlags.Print | PdfPermissionsFlags.CopyContent, PdfEncryptionKeySize.Key128Bit);

                            //将文档保存到文件
                            pdf.SaveToFile(path, FileFormat.PDF);
                            counter++;
                        }
                    }
                }
            }

            int cnt = dt.Rows.Count;
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "原文批量加密", "原文加密", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文批量加密，影响记录" + cnt + "条，" + counter + "条加密成功，" + (dt.Rows.Count - counter) + "条未成功（可能原因为原文不存在或路径有误或者已被加密。）";
            if (cnt <= 500)
            {
                sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
                DataTable dtrec = SqlHelper.GetDataTable(sql, list.ToArray());
                OperateRecHlp.RcdUserOpration2(opInfo, dtrec);//批量生成档号记录到用户操作记录中;2020年3月13日
                dtrec.Dispose();
            }
            else
            {
                List<string> ids = new List<string>();
                OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//记录超过数量时，就不记录影响的记录ID了
            }

            rlt = 1;
            tle = counter + "条加密成功，" + (dt.Rows.Count - counter) + "条未成功（可能原因为原文不存在或路径有误或者已被加密。）";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
        }

        public async Task<IActionResult> DecryptSContent(string table, List<string> pms, string userid)
        {
            await Task.Delay(5);
            string searchConditon = "";
            string where = "";
            int rlt = 0;
            string tle = "";

            for (int i = 0; i < pms.Count; i++)
            {
                if (i == 0)
                {
                    where += " Unique_code=" + pms[i];
                    searchConditon += " ID等于" + pms[i];
                }
                else
                {
                    where += " OR Unique_code=" + pms[i];
                    searchConditon += "," + pms[i];
                }
            }
            searchConditon += "的记录";

            string openPwd = string.Empty;
            string permitPwd = string.Empty;
            int counter = 0;

            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    string path = ywrootPath + row[0].ToString();
                    if (System.IO.File.Exists(path))
                    {
                        if (string.IsNullOrEmpty(permitPwd))//
                        {
                            GetPasswords(out openPwd, out permitPwd);
                            if (string.IsNullOrEmpty(permitPwd))
                            {
                                rlt = 0;
                                tle = "原文打开密码和操作密码均还未配置，请到管理配置中配置后继续！";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }
                        FileStream fs = new FileStream(path, FileMode.Open);//使用FileStream而不是使用LoadFromFile,是为了解决pdf文件被进程占用的问题 20200629
                        long size = fs.Length;
                        byte[] array = new byte[size];
                        fs.Read(array, 0, array.Length);
                        fs.Close();//必须关闭

                        try
                        {
                            PdfDocument pdf = new PdfDocument(array, permitPwd);//不论pdf是否加密，最好都带上originalPermissionPassword这个参数，因为如果已加密，则会弹出异常
                            if (pdf.IsEncrypted)//只有加密过的才能解密
                            {
                                //解密PDF文件
                                pdf.Security.Encrypt(string.Empty, string.Empty, PdfPermissionsFlags.Default, PdfEncryptionKeySize.Key128Bit, permitPwd);

                                //将文档保存到文件
                                pdf.SaveToFile(path, FileFormat.PDF);
                                counter++;
                            }
                            pdf.Dispose();
                        }
                        catch (Exception e)
                        {
                            if (e.Message.ToLower().Contains("password is invalid"))
                            {
                                rlt = 0;
                                tle = "“" + row[0].ToString() + "”的原文加密时所用密码，与当前统一使用的加密密码不一致。请联系系统管理解决此问题。";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }
                    }
                }
            }

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "原文解密", "原文解密", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文解密，影响记录" + pms.Count + "条";
            OperateRecHlp.RcdUserOprationCommon(opInfo, pms);
            rlt = 1;
            tle = counter + "条原文解密成功！" + (dt.Rows.Count - counter) + "条原文解密失败（原因可能是有的原文本身为非加密的）。";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
        }

        public async Task<IActionResult> DecryptContent(string table, string where, string pms, string searchmode, string userid)
        {
            await Task.Delay(5);
            string searchConditon = "";
            int rlt = 0;
            string tle = "";

            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            {
                searchConditon = "把表" + table + "的全部记录 ";
            }
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
                searchConditon = "把表" + table + "基本搜索的全部记录 ";
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
                searchConditon = "把表" + table + "高级搜索的全部记录 ";
            }

            string openPwd = string.Empty;
            string permitPwd = string.Empty;
            int counter = 0;

            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    string path = ywrootPath + row[0].ToString();
                    if (System.IO.File.Exists(path))
                    {
                        if (string.IsNullOrEmpty(permitPwd))//
                        {
                            GetPasswords(out openPwd, out permitPwd);
                            if (string.IsNullOrEmpty(permitPwd))
                            {
                                rlt = 0;
                                tle = "原文打开密码和操作密码均还未配置，请到管理配置中配置后继续！";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }
                        FileStream fs = new FileStream(path, FileMode.Open);//使用FileStream而不是使用LoadFromFile,是为了解决pdf文件被进程占用的问题 20200629
                        long size = fs.Length;
                        byte[] array = new byte[size];
                        fs.Read(array, 0, array.Length);
                        fs.Close();//必须关闭
                        try
                        {
                            PdfDocument pdf = new PdfDocument(array, permitPwd);//不论pdf是否加密，最好都带上originalPermissionPassword这个参数，因为如果已加密，则会弹出异常
                            if (pdf.IsEncrypted)//只有加密过的才能解密
                            {
                                //解密PDF文件
                                pdf.Security.Encrypt(string.Empty, string.Empty, PdfPermissionsFlags.Default, PdfEncryptionKeySize.Key128Bit, permitPwd);

                                //将文档保存到文件
                                pdf.SaveToFile(path, FileFormat.PDF);
                                counter++;
                            }
                            pdf.Dispose();
                        }
                        catch (Exception e)
                        {
                            if (e.Message.ToLower().Contains("password is invalid"))
                            {
                                rlt = 0;
                                tle = "“" + row[0].ToString() + "”的原文加密时所用密码，与当前统一使用的加密密码不一致。请联系系统管理解决此问题。";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }
                    }
                }
            }

            int cnt = dt.Rows.Count;
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "原文批量解密", "原文解密", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文批量解密，影响记录" + cnt + "条";
            if (cnt <= 500)
            {
                sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
                DataTable dtrec = SqlHelper.GetDataTable(sql, list.ToArray());
                OperateRecHlp.RcdUserOpration2(opInfo, dtrec);//批量生成档号记录到用户操作记录中;2020年3月13日
                dtrec.Dispose();
            }
            else
            {
                List<string> ids = new List<string>();
                OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//记录超过数量时，就不记录影响的记录ID了
            }

            rlt = 1;
            tle = counter + "条原文解密成功！" + (dt.Rows.Count - counter) + "条原文解密失败（原因可能是有的原文本身为非加密的）。";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
        }

        //public IActionResult ExportToExcel([FromForm]string table, [FromForm]string where, [FromForm]string pms, [FromForm]string searchmode)
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

        public async Task<IActionResult> WaterMarkSContent(string table, List<string> pms, string userid)
        {
            await Task.Delay(5);
            string searchConditon = "";
            string where = "";

            for (int i = 0; i < pms.Count; i++)
            {
                if (i == 0)
                {
                    where += " Unique_code=" + pms[i];
                    searchConditon += " ID等于" + pms[i];
                }
                else
                {
                    where += " OR Unique_code=" + pms[i];
                    searchConditon += "," + pms[i];
                }
            }
            searchConditon += "的记录";

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    string path = ywrootPath + row[0].ToString();
                    PdfDocument pdf = new PdfDocument();
                    pdf.LoadFromFile(path);
                    for (int j = 0; j < pdf.Pages.Count; j++)
                    {
                        PdfPageBase page = pdf.Pages[j];
                        //添加文本水印到文件的第一页，设置文本格式
                        PdfTilingBrush brush = new PdfTilingBrush(new SizeF(page.Canvas.ClientSize.Width / 3, page.Canvas.ClientSize.Height / 3)); //设置每行每列几个水印
                        brush.Graphics.SetTransparency(0.2f); //透明度
                        brush.Graphics.Save();
                        brush.Graphics.TranslateTransform(brush.Size.Width / 2, brush.Size.Height / 2);
                        brush.Graphics.RotateTransform(-45); //旋转角度
                        brush.Graphics.DrawString("Draft Version", new PdfFont(PdfFontFamily.Helvetica, 40), PdfBrushes.Blue, 0, 0, new PdfStringFormat(PdfTextAlignment.Center));
                        brush.Graphics.Restore();
                        brush.Graphics.SetTransparency(1);
                        page.Canvas.DrawRectangle(brush, new RectangleF(new PointF(0, 0), page.Canvas.ClientSize));
                    }
                    //保存文件
                    pdf.SaveToFile(path, FileFormat.PDF);
                }
            }

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "原文加水印", "加水印", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文加水印，影响记录" + pms.Count + "条";
            OperateRecHlp.RcdUserOprationCommon(opInfo, pms);

            int rlt = 1;
            string title = "原文添加水印成功！";
            return Json(new { rst = rlt, info = title });
        }

        public async Task<IActionResult> WaterMarkContent(string table, string where, string pms, string searchmode, string userid)
        {
            await Task.Delay(5);
            string searchConditon = "";
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            {
                searchConditon = "把表" + table + "的全部记录 ";
            }
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
                searchConditon = "把表" + table + "基本搜索的全部记录 ";
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
                searchConditon = "把表" + table + "高级搜索的全部记录 ";
            }

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                if (row[0] != DBNull.Value && !string.IsNullOrEmpty(row[0].ToString()))
                {
                    string path = ywrootPath + row[0].ToString();
                    PdfDocument pdf = new PdfDocument();
                    pdf.LoadFromFile(path);
                    for (int j = 0; j < pdf.Pages.Count; j++)
                    {
                        PdfPageBase page = pdf.Pages[j];
                        //添加文本水印到文件的第一页，设置文本格式
                        PdfTilingBrush brush = new PdfTilingBrush(new SizeF(page.Canvas.ClientSize.Width / 3, page.Canvas.ClientSize.Height / 3)); //设置每行每列几个水印
                        brush.Graphics.SetTransparency(0.2f); //透明度
                        brush.Graphics.Save();
                        brush.Graphics.TranslateTransform(brush.Size.Width / 2, brush.Size.Height / 2);
                        brush.Graphics.RotateTransform(-45); //旋转角度
                        brush.Graphics.DrawString("Draft Version", new PdfFont(PdfFontFamily.Helvetica, 40), PdfBrushes.Blue, 0, 0, new PdfStringFormat(PdfTextAlignment.Center));
                        brush.Graphics.Restore();
                        brush.Graphics.SetTransparency(1);
                        page.Canvas.DrawRectangle(brush, new RectangleF(new PointF(0, 0), page.Canvas.ClientSize));
                    }
                    //保存文件
                    pdf.SaveToFile(path, FileFormat.PDF);

                }
            }

            int cnt = dt.Rows.Count;
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "原文批量加水印", "加水印", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文批量加水印，影响记录" + cnt + "条";
            if (cnt <= 500)
            {
                sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
                DataTable dtrec = SqlHelper.GetDataTable(sql, list.ToArray());
                OperateRecHlp.RcdUserOpration2(opInfo, dtrec);//
            }
            else
            {
                List<string> ids = new List<string>();
                OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//记录超过数量时，就不记录影响的记录ID了
            }

            int rlt = 1;
            string title = "原文批量添加水印成功！";
            return Json(new { rst = rlt, info = title });
        }

        public IActionResult GetArecordById(string table, string id)
        {
            string sql = "SELECT * FROM " + table + " WHERE Unique_code=" + id;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public void GetPasswords(out string open, out string permit)
        {
            string sql = "SELECT pwd_type,name,pwd,Unique_code FROM t_config_yw_permit";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            DataRow[] drs = table.Select("pwd_type = 'foropen'");
            if (drs.Length > 0)
                open = drs[0]["pwd"].ToString();
            else
                open = "";

            drs = table.Select("pwd_type = 'forpermmit'");
            if (drs.Length > 0)
                permit = drs[0]["pwd"].ToString();
            else
                permit = "";
        }

        protected OperationInfo MakeOperInfo(string userid, string table, string funcName, string funcModal, string operTag, string sourceIP)
        {
            OperationInfo oInfo = new OperationInfo();
            oInfo.UserName = userid;
            oInfo.FuncName = funcName;
            oInfo.FuncName = funcModal;
            oInfo.OperTag = operTag;
            oInfo.SourceIP = sourceIP;
            oInfo.TableName = table;

            string sql = "SELECT A.Unique_code,B.role_name FROM t_user AS A,t_config_role AS B \r\n";
            sql += "WHERE A.role_id=B.Unique_code AND A.user_name='" + userid + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                oInfo.UserId = dt.Rows[0][0].ToString();
                oInfo.Department = dt.Rows[0][1].ToString();
            }

            sql = "SELECT name FROM t_config_type_tree WHERE code='" + table + "'";
            object tableCode = SqlHelper.ExecuteScalar(sql, null);
            if (tableCode != null)
            {
                oInfo.ArchType = tableCode.ToString();
            }
            return oInfo;
        }
    }
}
