using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using NetCoreDbUtility;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace ArchiveFileManagementNs.Controllers
{
    public class WVirStockController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewStock(string userid)
        {
            ViewData["userid"] = userid;
            return View("ViewStock");
        }

        public IActionResult DenseFrame(string id,string userid)
        {
            ViewData["stockId"] = id;
            ViewData["userid"] = userid;
            return View("DenseFrame");
        }

        public IActionResult ConfigIndex()
        {
            return View("ConfigIndex");
        }

        public IActionResult AddStockV()
        {
            return View("AddStock");
        }

        public IActionResult AddDenseFrmV(string id)
        {
            ViewData["BaseId"] = id;
            return View("AddDenseFrm");
        }

        public IActionResult ModiStockV(string id)
        {
            ViewData["BaseId"] = id;
            return View("ModiStock");
        }

        public IActionResult ModiDenseFrmV(string id)
        {
            ViewData["SubId"] = id;
            return View("ModiDenseFrm");
        }

        public IActionResult DealTireV(string id, string other, string userid)
        {
            ViewData["ID"] = id;
            ViewData["TireCount"] = userid;
            ViewData["SquareCount"] = other;
            return View("TireConfig");
        }

        public IActionResult ConfigDataIndex(string userid)
        {
            ViewData["userid"] = userid;
            return View("ConfigDataIndex");
        }

        public IActionResult ViewStockForSelect()
        {
            return View("ViewStock_Select");
        }

        public IActionResult FileOutView(string id, string userid)
        {
            ViewData["Stock_DenseFrm_Info"] = id;
            ViewData["userid"] = userid;
            return View("FileOutOprtn");
        }

        public IActionResult DenseFrameSelect(string id)
        {
            ViewData["stockId"] = id;
            return View("DenseFrame_Select");
        }

        public IActionResult TableView(string id)
        {
            ViewData["type"] = id;//p表示基本搜索，p表示高级索索
            return View("PickTable");
        }

        public IActionResult SuperSearchView(string id)
        {
            ViewData["table"] = id;
            return View("SuperSearch");
        }

        public IActionResult ViewSquareDataV(string id, string userid)
        {
            ViewData["userid"] = userid;
            ViewData["StockFrmInfo"] = id;
            return View("ViewSquareArch");
        }

        public IActionResult ViewSquareDataV2(string id, string userid)
        {
            ViewData["userid"] = userid;
            ViewData["StockFrmInfo"] = id;
            return View("ViewSquareArch2");
        }

        public IActionResult GetStocks()
        {
            //string sql = "SELECT Unique_code,code_key,base_name,ISNULL(comments,'') AS cmt FROM t_config_stocks";//modified on 20201116
            string sql = "SELECT Unique_code,code_key,base_name,ISNULL(comments,'') AS cmt FROM t_config_stocks";//modified on 20201116
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetDenseFrms(string stockId)
        {
            string sql = "SELECT Unique_code,code_name,code_value,order_id,tire_count,sqare_count FROM t_config_dense_frames WHERE parent_code=" + stockId;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetDenseWithFrmData(string stockId)
        {
            string sql = "SELECT Unique_code,code_name,code_value,order_id,tire_count,sqare_count FROM t_config_dense_frames WHERE parent_code=" + stockId;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            List<DenseFrmEntity> list = new List<DenseFrmEntity>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DenseFrmEntity dfe = new DenseFrmEntity();
                string frm = table.Rows[i]["Unique_code"].ToString();
                dfe.Unique_code = frm;
                string tireCount = table.Rows[i]["tire_count"].ToString();
                string sqrCount = table.Rows[i]["sqare_count"].ToString();
                dfe.tire_count = int.Parse(tireCount);
                dfe.sqare_count = int.Parse(sqrCount);
                dfe.code_name = table.Rows[i]["code_name"].ToString();
                dfe.code_value = table.Rows[i]["code_value"].ToString();
                dfe.order_id = table.Rows[i]["order_id"].ToString();
                List<FrmData> fds = new List<FrmData>();
                for (int m = 1; m <= dfe.tire_count; m++)
                {
                    for (int n = 1; n <= dfe.sqare_count; n++)
                    {
                        string sqr = m + "_" + n;
                        sql = "SELECT x.m.value('@name','NVARCHAR(MAX)') FROM t_dense_frame_data T CROSS APPLY T.Summary.nodes('/Summary/item') x(m) \r\n";
                        sql += "WHERE dense_frame_id=" + frm + " AND square_no='" + sqr + "'";
                        DataTable dt = SqlHelper.GetDataTable(sql, null);
                        FrmData fd = new FrmData();
                        fd.DenseFrmInfo = frm + "_" + sqr;
                        string arch = "";
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            if (j == 0)
                                arch += dt.Rows[j][0].ToString();
                            else
                                arch += "," + dt.Rows[j][0].ToString();
                        }
                        fd.ArchInfo = string.IsNullOrEmpty(arch) ? "暂未存放档案数据" : arch;
                        fds.Add(fd);
                        dt.Dispose();
                    }
                }
                dfe.FrameData = fds;
                list.Add(dfe);
            }
            table.Dispose();
            return Json(list);
        }

        public IActionResult DeleteDenseFrm(int denseFrmId)
        {
            int result = 0;
            string sql = "SELECT COUNT(Unique_code) FROM t_dense_frame_data T CROSS APPLY T.archive_data.nodes('/Archive/item') x(m) \r\n";
            sql += "WHERE x.m.value('@recid','NVARCHAR(MAX)') IS NOT NULL AND dense_frame_id=" + denseFrmId;
            object c = SqlHelper.ExecuteScalar(sql, null);
            if (int.Parse(c.ToString()) > 0)
            {
                result = -5;//有数据，密集架信息不能删除
                return Json(new { rst = result });
            }
            sql = "DELETE FROM t_config_dense_frames WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", denseFrmId);
            SqlParameter[] param = new SqlParameter[] { para1 };
            result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult DeleteStock(int stockId)
        {
            int result = 0;
            //string sql = "SELECT COUNT(Unique_code) FROM t_dense_frame_data T CROSS APPLY T.archive_data.nodes('/Archive/item') x(m) \r\n";
            //sql += "WHERE x.m.value('@recid','NVARCHAR(MAX)') IS NOT NULL AND \r\n";
            //sql += "EXISTS(SELECT Unique_code FROM t_config_dense_frames T2 WHERE T.dense_frame_id=T2.Unique_code AND parent_code=" + stockId + ")";
            //object c = SqlHelper.ExecuteScalar(sql, null);
            //if (int.Parse(c.ToString()) > 0)
            //{
            //    result = -5;//有数据，密集架信息不能删除
            //    return Json(new { rst = result });
            //}

            string sql = "DELETE FROM t_config_stocks WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", stockId);
            SqlParameter[] param = new SqlParameter[] { para1 };
            result = SqlHelper.ExecNonQuery(sql, param);
            return Json(new { rst = result });
        }

        public IActionResult AddStock(string baseName, string baseCode, string baseComment)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("base_name", baseName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_key", baseCode);
            object cm = DBNull.Value;
            if (baseComment != null)
                cm = baseComment;
            SqlParameter para3 = SqlHelper.MakeInParam("comments", cm);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3 };

            string sql = "IF (SELECT COUNT(*) FROM t_config_stocks WHERE base_name=@base_name OR code_key = @code_key) > 0 \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//库房名或库房值不允许重复
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    INSERT INTO t_config_stocks(base_name,code_key,comments) VALUES(@base_name,@code_key,@comments) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END ";

            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult AddDenseFrm(string subName, string subCode, string subOrder, string parentId)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("code_name", subName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_value", subCode);
            SqlParameter para3 = SqlHelper.MakeInParam("order_id", subOrder);
            SqlParameter para4 = SqlHelper.MakeInParam("parent_code", parentId);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4 };

            string sql = "IF (SELECT COUNT(*) FROM t_config_dense_frames WHERE (code_name=@code_name OR code_value = @code_value) AND parent_code=@parent_code) > 0 \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//同一库房内，密集架名或密集架值不允许重复
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    INSERT INTO t_config_dense_frames(code_name,code_value,order_id,parent_code) VALUES(@code_name,@code_value,@order_id,@parent_code) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END ";

            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult GetStockInfoById(string id)
        {
            string sql = "SELECT Unique_code,code_key,base_name,comments FROM t_config_stocks WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult ModiStock(string baseName, string baseCode, string baseComment, string unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("base_name", baseName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_key", baseCode);
            SqlParameter para3 = SqlHelper.MakeInParam("comments", baseComment == null ? "" : baseComment);
            SqlParameter para4 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4 };
            string sql = "UPDATE t_config_stocks SET base_name=@base_name,code_key=@code_key,comments=@comments WHERE Unique_code=@Unique_code";
            int result = SqlHelper.ExecNonQuery(sql, param);
            return Json(new { rst = result });
        }

        public IActionResult GetDenseFrmById(string id)
        {
            string sql = "SELECT Unique_code,code_name,code_value,order_id FROM t_config_dense_frames WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult ModiDenseFrm(string subName, string subCode, string subOrder, string unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("code_name", subName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_value", subCode);
            SqlParameter para3 = SqlHelper.MakeInParam("order_id", subOrder);
            SqlParameter para4 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4 };
            string sql = "UPDATE t_config_dense_frames SET code_name=@code_name,code_value=@code_value,order_id=@order_id WHERE Unique_code=@Unique_code";
            int result = SqlHelper.ExecNonQuery(sql, param);
            return Json(new { rst = result });
        }

        public IActionResult DealTireInfo(string t, string s, string id)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("sqare_count", s);
            SqlParameter para2 = SqlHelper.MakeInParam("tire_count", t);
            SqlParameter para3 = SqlHelper.MakeInParam("Unique_code", id);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3 };
            string sql = "UPDATE t_config_dense_frames SET sqare_count=@sqare_count,tire_count=@tire_count WHERE Unique_code=@Unique_code";
            int result = SqlHelper.ExecNonQuery(sql, param);
            return Json(new { rst = result });
        }

        public IActionResult ShowInitialSearchRecs(string tableName, int pageSize, int pageIndex)
        {
            if (string.IsNullOrEmpty(tableName))
                return Json("");

            string fieldStr = string.Empty;
            string colFields = GetFieldsStrShowing(tableName, out fieldStr);
            ViewData["colFields"] = colFields;
            ViewData["fieldStr"] = fieldStr;

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName;
            string fields = "Unique_code,yw,yw_xml," + fieldStr;
            string where = " 1=1 AND is_deleted <> '1' ";//预归档库和档案总库中的数据，都搜索
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table, conditon = where, pms = "", searchmode = 0 });
        }

        private string GetFieldsStrShowing(string id, out string fieldStr)
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

        public IActionResult CreateSearchControls(string id)
        {
            string fieldStr = string.Empty;
            string colFields = GetFieldsStrShowing(id, out fieldStr);
            ViewData["colFields"] = colFields;
            ViewData["fieldStr"] = fieldStr;

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
            string where = " 1=1 AND store_type='2' AND is_deleted <> '1' \r\n";//资源总库中的数据才能放入密集架
            where += " AND NOT EXISTS(SELECT x.m.value('(@recid)[1]','nvarchar(MAX)') FROM t_dense_frame_data DF ";
            where += "cross apply DF.archive_data.nodes('/Archive/item') x(m) WHERE x.m.value('(@recid)[1]','nvarchar(MAX)')=" + codeName + ".Unique_code ";
            where += "AND x.m.value('(@table)[1]','nvarchar(MAX)')='" + codeName + "')";
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
            string where = " 1=1 And store_type='2' AND is_deleted <> '1' \r\n";//资源总库中的数据才能放入密集架
            where += " AND NOT EXISTS(SELECT x.m.value('(@recid)[1]','nvarchar(MAX)') FROM t_dense_frame_data DF ";
            where += "cross apply DF.archive_data.nodes('/Archive/item') x(m) WHERE x.m.value('(@recid)[1]','nvarchar(MAX)')=" + codeName + ".Unique_code ";
            where += "AND x.m.value('(@table)[1]','nvarchar(MAX)')='" + codeName + "')";
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

        public IActionResult GetStockFrmInfo(string ids)
        {
            string[] arr = ids.Split('_');
            string stockId = arr[0];
            string frmId = arr[1];
            string sql = "SELECT base_name FROM t_config_stocks WHERE Unique_code=" + stockId;
            object stockObj = SqlHelper.ExecuteScalar(sql, null);
            sql = "SELECT code_name FROM  t_config_dense_frames WHERE Unique_code=" + frmId;
            object frmObj = SqlHelper.ExecuteScalar(sql, null);
            return Json(new { stock = stockObj, frame = frmObj, square = arr[2] + '_' + arr[3] });
        }

        public async Task<IActionResult> IntoSquare(string table, string stockInfo, List<int> ids, string userid)
        {
            int result = 0;
            await Task.Run(() =>
            {
                string[] arr = stockInfo.Split('_');
                string frm = arr[1];
                string sqr = arr[2] + "_" + arr[3];

                string sql0 = "SELECT x.m.value('(@recid)[1]','nvarchar(MAX)') AS id FROM t_dense_frame_data T \r\n";//recid为记录ID
                sql0 += "CROSS APPLY T.archive_data.nodes('/Archive/item') x(m) \r\n";
                sql0 += "WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'";
                DataTable dt = SqlHelper.GetDataTable(sql0, null);

                string updateStr = "";
                string items = "";
                int counter = 0;//计算ids中的元素是否都已放入档案格
                int counter2 = 0;//用于计算新放入档案格的记录数
                for (int i = 0; i < ids.Count; i++)
                {
                    DataRow[] rows = dt.Select("id='" + ids[i] + "'");
                    if (rows.Length == 0)//如果档案记录已经存放到此档案格，则忽略不放
                    {
                        string item = "<item recid=\"" + ids[i] + "\" table=\"" + table + "\"></item>";
                        updateStr += "   update T SET archive_data.modify(' \r\n";
                        updateStr += "   insert " + item + "\r\n";
                        updateStr += "as first into (/Archive)[1]') FROM t_dense_frame_data T WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'; \r\n";
                        items += "<item recid=\"" + ids[i] + "\" table=\"" + table + "\"></item>";
                        counter2++;
                    }
                    else
                    {
                        counter++;
                    }
                }
                if (counter2 > 0)//有新记录放入档案格时
                {
                    updateStr += "UPDATE t_dense_frame_data SET arhive_count=" + (dt.Rows.Count + counter2) + " WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'; \r\n";
                }
                dt.Dispose();

                string xml = "<Archive>" + items + "</Archive>";
                string sql = "IF (SELECT count(archive_data) FROM t_dense_frame_data WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "') = 0 \r\n";//modified on 20201117 to debug
                sql += "BEGIN \r\n";
                sql += "   INSERT INTO t_dense_frame_data(archive_data,square_no,dense_frame_id,the_operator,last_update,arhive_count) \r\n";
                sql += "   VALUES('" + xml + "','" + sqr + "','" + frm + "','" + userid + "','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "'," + ids.Count + ") \r\n";
                sql += "END \r\n";
                if (!string.IsNullOrEmpty(updateStr))//有新记录放入档案格时
                {
                    sql += "ELSE \r\n";
                    sql += "BEGIN \r\n";
                    sql += updateStr + " \r\n";
                    sql += "END \r\n";
                }
                result = SqlHelper.ExecNonQuery(sql, null);
                if (counter == ids.Count)
                {
                    result = -5;//表示发过来的全部记录已在档案格中
                }
            });
            return Json(new { rst = result });
        }

        public IActionResult AllIntoSquare(string table, string searchmode, string stockInfo, string where, string pms, string userid)
        {
            //string searchConditon = "";
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            {
                //searchConditon = "把表" + table + "的全部记录 ";
            }
            else if (int.Parse(searchmode) == 1)//基本搜索的传参
            {
                Newtonsoft.Json.Linq.JArray prms = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(pms);
                List<SearchCondtionModel> pList = prms.ToObject<List<SearchCondtionModel>>();
                for (int i = 0; i < pList.Count; i++)
                {
                    SearchCondtionModel md = pList[i];
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
                //searchConditon = "把表" + table + "基本搜索的全部记录 ";
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
                //searchConditon = "把表" + table + "高级搜索的全部记录 ";
            }

            string[] arr = stockInfo.Split('_');
            string stock = arr[0];
            string frm = arr[1];
            string sqr = arr[2] + "_" + arr[3];
            //string sql = "SELECT base_name FROM t_config_stocks WHERE Unique_code=" + stock;
            //object stockObj = SqlHelper.ExecuteScalar(sql, null);
            //sql = "SELECT code_name FROM  t_config_dense_frames WHERE Unique_code=" + frm;
            //object frmObj = SqlHelper.ExecuteScalar(sql, null);
            //string archLocation = "【" + stockObj.ToString() +" "+ frmObj.ToString()+ " " + sqr + "档案格】";

            //sql = "SELECT COUNT(Unique_code) as cnt FROM " + table + " WHERE " + where;
            //object cntObj = SqlHelper.ExecuteScalar(sql, list.ToArray());
            //int cnt = int.Parse(cntObj.ToString());

            //if (cnt <= 500)
            //{
            //    sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
            //    DataTable dt = SqlHelper.GetDataTable(sql, list.ToArray());
            //    OperateRecHlp.RcdUserOpration2(searchConditon + "放入"+ archLocation + "，影响记录" + cnt + "条", userid, table, dt);//把所有页（可能是搜索结果的所有页）放入密集架，记录到用户操作记录中;2020年6月12日
            //}
            //else
            //{
            //    List<string> ids = new List<string>();
            //    OperateRecHlp.RcdUserOprationCommon(searchConditon + "放入" + archLocation + "，影响记录" + cnt + "条", userid, table, ids);//记录超过数量时，就不记录影响的记录ID了
            //}

            string sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
            DataTable dtid = SqlHelper.GetDataTable(sql, list.ToArray());

            string sql0 = "SELECT x.m.value('(@recid)[1]','nvarchar(MAX)') AS id FROM t_dense_frame_data T \r\n";//recid为记录ID
            sql0 += "CROSS APPLY T.archive_data.nodes('/Archive/item') x(m) \r\n";
            sql0 += "WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'";
            DataTable dt = SqlHelper.GetDataTable(sql0, null);

            string updateStr = "";
            string items = "";
            int counter = 0;//计算ids中的元素是否都已放入档案格
            int counter2 = 0;//用于计算新放入档案格的记录数
            for (int i = 0; i < dtid.Rows.Count; i++)
            {
                DataRow row = dtid.Rows[i];
                DataRow[] rows = dt.Select("id='" + row[0].ToString() + "'");
                if (rows.Length == 0)//如果档案记录已经存放到此档案格，则忽略不放
                {
                    string item = "<item recid=\"" + row[0].ToString() + "\" table=\"" + table + "\"></item>";
                    updateStr += "   update T SET archive_data.modify(' \r\n";
                    updateStr += "   insert " + item + "\r\n";
                    updateStr += "as first into (/Archive)[1]') FROM t_dense_frame_data T WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'; \r\n";
                    items += "<item recid=\"" + row[0].ToString() + "\" table=\"" + table + "\"></item>";
                    counter2++;
                }
                else
                {
                    counter++;
                }
            }
            if (counter2 > 0)//有新记录放入档案格时
            {
                updateStr += "UPDATE t_dense_frame_data SET arhive_count=" + (dt.Rows.Count + counter2) + " WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'; \r\n";
            }
            dt.Dispose();

            string xml = "<Archive>" + items + "</Archive>";
            sql = "IF (SELECT count(archive_data) FROM t_dense_frame_data WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "') = 0 \r\n";//修改于20201117 for debugging
            sql += "BEGIN \r\n";
            sql += "   INSERT INTO t_dense_frame_data(archive_data,square_no,dense_frame_id,the_operator,last_update,arhive_count) \r\n";
            sql += "   VALUES('" + xml + "','" + sqr + "','" + frm + "','" + userid + "','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "'," + dtid.Rows.Count + ") \r\n";
            sql += "END \r\n";
            if (!string.IsNullOrEmpty(updateStr))//有新记录放入档案格时
            {
                sql += "ELSE \r\n";
                sql += "BEGIN \r\n";
                sql += updateStr + " \r\n";
                sql += "END \r\n";
            }
            int result = SqlHelper.ExecNonQuery(sql, null);
            if (counter == dtid.Rows.Count)
            {
                result = -5;//表示发过来的全部记录已在档案格中
            }

            dtid.Dispose();
            return Json(new { rst = result });
        }

        public IActionResult GetTablesInSquare(string stockFrm, string userid)
        {
            string[] arr = stockFrm.Split('_');
            string stock = arr[0];
            string frm = arr[1];
            string sqr = arr[2] + "_" + arr[3];

            string sql = "SELECT DISTINCT(x.m.value('(@table)[1]','nvarchar(MAX)')) AS tb FROM t_dense_frame_data T \r\n";
            sql += "CROSS APPLY T.archive_data.nodes('/Archive/item') x(m) \r\n";
            sql += "WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            List<DynamicTable> list = new List<DynamicTable>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string tableName = dt.Rows[i][0].ToString();
                string fieldStr = string.Empty;
                string colFields = GetFieldsStrShowing(tableName, out fieldStr);
                DynamicTable dnt = new DynamicTable();
                dnt.TableName = tableName;
                dnt.FieldsStr = fieldStr;
                dnt.ColFieldStr = colFields;
                list.Add(dnt);
            }
            dt.Dispose();
            return Json(list);
        }

        public IActionResult GetArchsByLocate(string stockFrm, string tableName, string fieldsStr, int pageSize, int pageIndex)
        {
            string[] arr = stockFrm.Split('_');
            string stock = arr[0];
            string frm = arr[1];
            string sqr = arr[2] + "_" + arr[3];

            string codeName = tableName + " T,t_dense_frame_data T2 ";
            string fields = "T.Unique_code,T.yw,T.yw_xml," + fieldsStr + " \r\n";
            string innerjoin = " CROSS APPLY T2.archive_data.nodes('/Archive/item') x(m) \r\n";
            string where = " T.is_deleted <> '1' AND x.m.value('(@table)[1]','nvarchar(MAX)') = '" + tableName + "' \r\n";
            where += " AND x.m.value('(@recid)[1]','nvarchar(MAX)') = T.Unique_code \r\n";
            where += " AND T2.dense_frame_id='" + frm + "' AND T2.square_no='" + sqr + "'";
            string sort = " T.Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            dt.Columns.Add("tableName");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["tableName"] = tableName;//每行数据中都记住tableName名称
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult MakeSummaryByLocate(string stockFrm)
        {
            string[] arr = stockFrm.Split('_');
            string stock = arr[0];
            string frm = arr[1];
            string sqr = arr[2] + "_" + arr[3];

            string sql = "declare @tableName nvarchar(50) \r\n";
            sql += "declare @dhPara int \r\n";
            sql += "declare @dhField nvarchar(100) \r\n";
            sql += "declare @i int \r\n";
            sql += "create table #dhTbl(dh nvarchar(100)) \r\n";
            sql += "create table #dhTb2(dh nvarchar(100)) \r\n";
            sql += "create table #dhTb3(dh nvarchar(100)) \r\n";
            sql += " \r\n";
            sql += "select distinct(x.m.value('@table','nvarchar(MAX)')) tbl, rowid = IDENTITY(INT,1,1),dealflag=0 \r\n";
            sql += "into #tempTable \r\n";
            sql += "from t_dense_frame_data T cross apply T.archive_data.nodes('/Archive/item') x(m) \r\n";
            sql += "where dense_frame_id= '" + frm + "' AND  square_no= '" + sqr + "' \r\n";
            sql += " \r\n";
            sql += "SELECT @i = MIN(rowid) FROM #tempTable WHERE dealflag = 0 \r\n";
            sql += "WHILE @i IS NOT NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "    SELECT @tableName=tbl FROM #tempTable WHERE rowid=@i \r\n";
            sql += "    IF OBJECT_ID(N''+ @tableName +'',N'U') IS NOT NULL \r\n";
            sql += "    BEGIN \r\n";
            sql += "       declare @sql nvarchar(MAX) \r\n";
            sql += "       set @sql = 'select @dhPara=archive_num_parts_amount,@dhField=archive_num_field from t_config_archive_num_makeup where code_name='''+@tableName + '''' \r\n";
            sql += "       execute sp_executesql @sql, N'@dhPara int output,@dhField nvarchar(100) output', @dhPara output, @dhField output \r\n";//输出参数@dhPara
            sql += " \r\n";
            sql += "       set @sql ='select '+@dhField+' dh from '+@tableName+' T,t_dense_frame_data T2 CROSS APPLY T2.archive_data.nodes(''/Archive/item'') x(m) \r\n";
            sql += "           where T.is_deleted <> ''1'' AND x.m.value(''(@table)[1]'',''nvarchar(MAX)'') = '''+@tableName+''' AND x.m.value(''(@recid)[1]'',''nvarchar(MAX)'') = T.Unique_code \r\n";
            sql += "           AND T2.dense_frame_id= ''" + frm + "'' AND  T2.square_no= ''" + sqr + "''' \r\n";
            sql += "       insert into #dhTbl exec(@sql) \r\n";
            sql += " \r\n";
            sql += "       insert into #dhTb2 select case when dh is null then '' else dh end dh from #dhTbl \r\n"; //如果dh为null，则使其结果从null变为‘’字符串
            sql += "       insert into #dhTb3 select distinct(substring(dh,1,LEN(dh)-CHARINDEX('-',REVERSE(dh)))) dh from #dhTbl \r\n"; //去掉档号最后一个'-'及其后面的字符串
            sql += "       delete from #dhTbl \r\n";
            sql += "       delete from #dhTb2 \r\n";
            sql += "    END \r\n";
            sql += "    ELSE \r\n";
            sql += "    BEGIN \r\n";
            sql += "       SET @sql = 'UPDATE t_dense_frame_data SET archive_data.modify(''delete /Archive/item[@table='''''+ @tableName +''''']'')  \r\n";
            sql += "       WHERE dense_frame_id= ''"+ frm + "'' AND  square_no= ''"+ sqr + "''' \r\n";
            sql += "        exec(@sql) \r\n";
            sql += "    END \r\n";
            sql += " \r\n";
            sql += "    UPDATE #tempTable SET dealflag = 1 WHERE rowid = @i \r\n";
            sql += "    SELECT @i = MIN(rowid) FROM #tempTable WHERE dealflag = 0 \r\n";
            sql += "END \r\n";
            sql += "SELECT dh FROM #dhTb3 \r\n";
            sql += "DROP table #dhTbl \r\n";
            sql += "DROP table #dhTb2 \r\n";
            sql += "DROP table #dhTb3 \r\n";
            sql += "DROP table #tempTable \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            string xml = "<Summary>";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                xml += "<item name=\"" + dt.Rows[i][0].ToString() + "\"></item>";
            }
            xml += "</Summary>";
            dt.Dispose();

            sql = "UPDATE t_dense_frame_data SET summary='" + xml + "' WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'";
            int result = SqlHelper.ExecNonQuery(sql, null);
            return Json(new { rst = result });
        }

        public IActionResult GetTablesInSquare2(string stockFrm, string userid)
        {
            string[] arr = stockFrm.Split('_');
            //string stock = arr[0];
            string frm = arr[0];
            string sqr = arr[1] + "_" + arr[2];

            string sql = "SELECT DISTINCT(x.m.value('(@table)[1]','nvarchar(MAX)')) AS tb FROM t_dense_frame_data T \r\n";
            sql += "CROSS APPLY T.archive_data.nodes('/Archive/item') x(m) \r\n";
            sql += "WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            List<DynamicTable> list = new List<DynamicTable>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string tableName = dt.Rows[i][0].ToString();
                string fieldStr = string.Empty;
                string colFields = GetFieldsStrShowing(tableName, out fieldStr);
                DynamicTable dnt = new DynamicTable();
                dnt.TableName = tableName;
                dnt.FieldsStr = fieldStr;
                dnt.ColFieldStr = colFields;
                list.Add(dnt);
            }
            dt.Dispose();
            return Json(list);
        }

        public IActionResult GetArchsByLocate2(string stockFrm, string tableName, string fieldsStr, int pageSize, int pageIndex)
        {
            string[] arr = stockFrm.Split('_');
            //string stock = arr[0];
            string frm = arr[0];
            string sqr = arr[1] + "_" + arr[2];

            string codeName = tableName + " T,t_dense_frame_data T2 ";
            string fields = "T.Unique_code,T.yw,T.yw_xml," + fieldsStr + " \r\n";
            string innerjoin = " CROSS APPLY T2.archive_data.nodes('/Archive/item') x(m) \r\n";
            string where = " T.is_deleted <> '1' AND x.m.value('(@table)[1]','nvarchar(MAX)') = '" + tableName + "' \r\n";
            where += " AND x.m.value('(@recid)[1]','nvarchar(MAX)') = T.Unique_code \r\n";
            where += " AND T2.dense_frame_id='" + frm + "' AND T2.square_no='" + sqr + "'";
            string sort = " T.Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            dt.Columns.Add("tableName");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["tableName"] = tableName;//每行数据中都记住tableName名称
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult DelSquareData(string id, string table, string stockFrm)
        {
            string[] arr = stockFrm.Split('_');
            //string stock = arr[0];
            string frm = arr[1];
            string sqr = arr[2] + "_" + arr[3];
            string sql = "UPDATE t_dense_frame_data SET archive_data.modify('delete /Archive/item[@recid=\"" + id + "\"][@table=\"" + table + "\"]') \r\n";
            sql += "WHERE dense_frame_id='" + frm + "' AND square_no='" + sqr + "'";
            int result = SqlHelper.ExecNonQuery(sql, null);
            return Json(new { rst = result });
        }
    }
}
