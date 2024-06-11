
using ArchiveFileManagementNs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace ArchiveFileManagementNs.Controllers
{
    public class WRecvCatalogController : WBaseController
    {
        private IHttpContextAccessor _accessor;
        public WRecvCatalogController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public IActionResult Index(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["url2"] = "/" + id + "/" + userid;
            return View();
        }

        //目录文件上传，返回上传后的路径
        public IActionResult UploadFile()
        {
            uploadResult result = new uploadResult();
            try
            {
                var oFile = Request.Form.Files["xlsx_file"];//使用bootstrap-fileinput控件
                //int index = oFile.FileName.LastIndexOf(".");
                //string extention = oFile.FileName.Substring(index);
                Stream sm = oFile.OpenReadStream();

                if (!Directory.Exists(AppContext.BaseDirectory + "xlss\\"))
                {
                    Directory.CreateDirectory(AppContext.BaseDirectory + "xlss\\");
                }

                //string newName = DateTime.Now.ToString("yyyymmddhhMMssss") + Guid.NewGuid().ToString() + extention;
                //string filename = AppContext.BaseDirectory + "xlss\\" + newName;
                //result.fileName = newName;

                string filename = oFile.FileName;
                result.fileName = filename;
                string fl = AppContext.BaseDirectory + "xlss\\" + filename;
                FileStream fs = new FileStream(fl, FileMode.Create);
                byte[] buffer = new byte[sm.Length];
                sm.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);
                fs.Dispose();
            }
            catch (Exception ex)
            {
                result.error = ex.Message;
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        //带参（tablename,userid,上传后的目录文件名称）页面跳转
        public IActionResult CatalogMatchView(string id, string userid, string other)
        {
            ViewBag.table = id;
            ViewBag.user = userid;
            ViewBag.xlsFile = other;
            return View("CatalogMatch");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="other">已经接收的xls文件路径</param>
        /// <returns></returns>
        public IActionResult LoadDataToLeftTable(string id, string other)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string filePath = AppContext.BaseDirectory + "xlss\\" + other;
            var result = 0;
            if (ExcelHelper.HaveNullHeader(filePath, 0))
            {
                result = 0;//失败
                var title = "选择的目录文件中有无用的空表头（或空列），建议把excel表中所有有用的列的数据复制粘贴到一个新的excel表中然后再尝试匹配!";
                return Json(new { rst = result, info = title });
            }

            DataTable dt = ExcelHelper.GetDataHeader(filePath, 0);
            List<XlsHeader> list = new List<XlsHeader>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                XlsHeader xh = new XlsHeader();
                xh.ID = i;
                xh.ColName = dt.Columns[i].ColumnName;
                list.Add(xh);
            }
            result = 1;//成功
            return Json(new { rst = result, rows = list });
        }

        public IActionResult LoadDataToRightTable(string id, string other)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT Unique_code,show_name,col_name FROM t_config_col_dict WHERE code='" + id + "'";
            DataTable dtFromDB = SqlHelper.GetDataTable(sql, null);

            var result = 1;
            return Json(new { rst = result, rows = dtFromDB });
        }

        public IActionResult ImpCatalogToTableVerified(string tableName, string fileName, List<FieldMatched> matchList, string userid)
        {
            string fn = AppContext.BaseDirectory + "xlss\\" + fileName;
            DataTable tableFromExcel = ExcelHelper.GetDataTable(fn, 0);

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (tableFromExcel.Rows.Count == 1)
            {
                var result = 0;//失败
                var title = "文件中目录条数为0，或只有列名行，接收已终止！";
                return Json(new { rst = result, info = title });
            }

            for (int i = 0; i < tableFromExcel.Columns.Count; i++)
            {
                string colName = tableFromExcel.Columns[i].ColumnName;
                //if (!string.IsNullOrEmpty(matchList.Keys.Where(s => s == colName).FirstOrDefault()))//更换列名，与数据库表中的列名对应
                tableFromExcel.Columns[i].ColumnName = matchList.Where(s => s.ColName == colName).FirstOrDefault().col_name;
            }

            //bool flag = VerifyIsCatalogAccedptedOverAgain(tableFromExcel, tableName);
            //if (flag)//有重复
            //{
            //    var result = 2;//confirm确认标识
            //    var title = "相同或相似的数据曾被接收过一批！允许重复接收，但重复接收的数据批次可根据需要在接收记录中手动删除！确认接收吗？";
            //    return Json(new { rst = result, info = title }, setting);
            //}

            //业务数据字段中有不可为空的字段，但excel表格中没有此对应字段 ADDED ON 20201125
            string nullFields = verifyFieldsCannotBeNullButNotInExecl(tableName, tableFromExcel);
            if (!string.IsNullOrEmpty(nullFields))
            {
                var result = 0;//失败
                var title = nullFields + " 字段值设置为不允许有空值，但接收数据中不存在这些字段对应的数据，接收已终止！";
                return Json(new { rst = result, info = title });
            }

            string info = CheckExcelContentValid(tableFromExcel, tableName);//检查EXCEL内容合法性
            if (!string.IsNullOrEmpty(info))//报错，就返回
            {
                var result = 0;//EXCEL中存在错误
                return Json(new { rst = result, info = info });
            }

            return ImpCatalogToTable(tableName, fileName, matchList, userid);
        }

        public IActionResult ImpCatalogToTable(string tableName, string fileName, List<FieldMatched> matchList, string userid)
        {
            string fn = AppContext.BaseDirectory + "xlss\\" + fileName;
            DataTable tableFromExcel = ExcelHelper.GetDataTable(fn, 0);

            for (int i = 0; i < tableFromExcel.Columns.Count; i++)
            {
                string colName = tableFromExcel.Columns[i].ColumnName;
                //if (!string.IsNullOrEmpty(matchList.Keys.Where(s => s == colName).FirstOrDefault()))//更换列名，与数据库表中的列名对应
                tableFromExcel.Columns[i].ColumnName = matchList.Where(s => s.ColName == colName).FirstOrDefault().col_name;
            }

            var import_Time = DateTime.Now.ToString();
            var record_Number = (tableFromExcel.Rows.Count - 1).ToString();
            //var config_Name = string.Empty;
            //var config_XML = string.Empty;

            string sql = "INSERT t_config_imp_catalogs_rec(table_code,excel_file_name,import_time,import_user,record_number) \r\n";
            sql += "VALUES('" + tableName + "','" + fileName + "','" + import_Time + "','" + userid + "','" + record_Number + "')";
            SqlHelper.ExecNonQuery(sql, null);
            //返回本次导入的批次号
            sql = "SELECT TOP 1 Unique_code from t_config_imp_catalogs_rec ORDER BY Unique_code DESC";
            object bundleNumber = SqlHelper.ExecuteScalar(sql, null);

            tableFromExcel.Columns.Add("import_bundle");
            for (int i = 0; i < tableFromExcel.Rows.Count; i++)//更新列值
                tableFromExcel.Rows[i]["import_bundle"] = bundleNumber;

            tableFromExcel.TableName = tableName;//对应的表名      
            tableFromExcel.Rows.RemoveAt(0);//表头去掉
            SqlHelper.InsertByBulk(tableFromExcel, tableName);//效率非常高，简直秒杀啊！    
            tableFromExcel.Dispose();

            List<string> ids = new List<string>();
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, tableName, "接收目录", "预归档", "", ipAddr);
            opInfo.OperTag = "接收一批目录";
            OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//接收一批目录的用户操作登记;2020年3月13日

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            var result = 1;
            var title = tableFromExcel.Rows.Count + "条目录已接收成功！";
            return Json(new { rst = result, info = title });
        }

        bool VerifyIsCatalogAccedptedOverAgain(DataTable dt, string tableName)
        {
            string condition = string.Empty;
            SqlParameter[] param = new SqlParameter[dt.Columns.Count];
            int rowCount = dt.Rows.Count;
            Random random = new Random();
            int index = random.Next(1, rowCount);//取小于记录行数的随即数

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataRow dr = dt.Rows[index];
                string colName = dt.Columns[i].ColumnName;
                if (i == 0)
                    condition = colName + "=@" + colName;
                else
                    condition += " AND " + colName + "=@" + colName;
                param[i] = SqlHelper.MakeInParam(colName, dr[colName]);
            }
            string sql = "SELECT COUNT(*) AS count FROM " + tableName + " WHERE " + condition;
            object count = SqlHelper.ExecuteScalar(sql, param);
            if (int.Parse(count.ToString()) > 0)
                return true;
            return false;
        }

        private string CheckExcelContentValid(DataTable tableFromExcel, string tableName)
        {
            //检查个字段是否超长或是否含有“'”字符。
            string checkSql = "SELECT name,TYPE_NAME(SYSTEM_TYPE_ID)AS fieldType,MAX_LENGTH AS fieldLenth,is_nullable as canbeNull FROM sys.columns WHERE OBJECT_ID=OBJECT_ID('" + tableName + "')";
            DataTable dtCheck = SqlHelper.GetDataTable(checkSql, null);
            for (int i = 1; i < tableFromExcel.Rows.Count; i++)
            {
                for (int j = 0; j < dtCheck.Rows.Count; j++)
                {
                    for (int k = 0; k < tableFromExcel.Columns.Count; k++)
                    {
                        string colName = tableFromExcel.Columns[k].ColumnName;
                        if (dtCheck.Rows[j]["name"].ToString().Equals(colName))
                        {
                            string fieldType = dtCheck.Rows[j]["fieldType"].ToString();
                            if (!fieldType.ToLower().Equals("numberic") && !fieldType.ToLower().Equals("int"))
                            {
                                int realLength = tableFromExcel.Rows[i][colName].ToString().Length;
                                int principleLength = int.Parse(dtCheck.Rows[j]["fieldLenth"].ToString());
                                if (realLength * 2 > principleLength)
                                {
                                    var info = colName + "字段第" + (i + 1) + "行的内容长度超长，请检查修改后再重新接收目录！";
                                    return info;
                                }
                            }
                            string canbeNull = dtCheck.Rows[j]["canbeNull"].ToString();
                            if (!Boolean.Parse(canbeNull))//不允许为空
                            {
                                object cr = tableFromExcel.Rows[i][colName];
                                if (cr == null || cr == DBNull.Value || string.IsNullOrEmpty(cr.ToString()))
                                {
                                    var info = colName + "字段在业务数据字典中设置不允许为空，但目录源表格中第" + i + "行存在空值！";
                                    return info;
                                }
                            }
                            break;
                        }
                        if (tableFromExcel.Rows[i][colName].ToString().Contains(@"'"))
                        {
                            var info = colName + "字段第" + (i + 1) + "行的内容中包含“‘”字符，请检查修改后再重新接收目录！";
                            return info;
                        }
                    }
                }
            }
            dtCheck.Dispose();
            return string.Empty;
        }

        /// <summary>
        /// added on 20201125
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tableFromExcel"></param>
        /// <returns></returns>
        private string verifyFieldsCannotBeNullButNotInExecl(string tableName, DataTable tableFromExcel)
        {
            string result = "";
            string sql = "SELECT col_name,show_name FROM t_config_col_dict WHERE code='" + tableName + "' AND col_null=0";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var colName = dt.Rows[i][0].ToString();
                var showName = dt.Rows[i][1].ToString();
                bool flag = tableFromExcel.Columns.Contains(colName);
                if (!flag)
                {
                    if (string.IsNullOrEmpty(result))
                        result += showName;
                    else
                        result += "、" + showName;
                }
            }
            return result;
        }

        public IActionResult ImpCataRecView(string id, string userid)
        {
            ViewData["tableName"] = id;
            ViewData["loggedUser"] = userid;
            return View("ImpCatalogRec");
        }

        public IActionResult ShowCatalogImgRecs(string tableName, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_config_imp_catalogs_rec";
            string fields = "Unique_code,excel_file_name,import_time,import_user,record_number,which_store";
            string where = "table_code='" + tableName + "'";
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult BundleImpRecDetailView(string id, string userid, string other)
        {
            string sql = "SELECT t1.col_name,t1.show_name FROM t_config_col_dict t1 \r\n";
            sql += "INNER JOIN  t_config_field_show_list t2 ON t1.Unique_code=t2.selected_code \r\n";
            sql += "WHERE t1.code='" + id + "'\r\n";
            sql += " ORDER BY CAST(t2.order_number AS INT) ASC";
            DataTable fieldDt = SqlHelper.GetDataTable(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (fieldDt.Rows.Count == 0)
            {
                var result = 0;
                var title = "此档案类型库还未进行显示配置，请配置后继续！";
                return Json(new { rst = result, info = title });
            }

            string fieldStr = string.Empty;
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

            ViewData["tableName"] = id;//传参
            ViewData["userId"] = userid;
            ViewData["impBundle"] = other;
            ViewData["colFields"] = colFields;
            ViewData["fieldStr"] = fieldStr;
            return View("BundleImpRecDetail");
        }

        public IActionResult GetBundleImpRecordDetail(string tableName, string impBundle, string fieldStr, int pageSize, int pageIndex)
        {
            string codeName = tableName;
            string fields = fieldStr;
            string where = "import_bundle='" + impBundle + "'";
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult ImportToMangeStore(string tableName, string impBundle)
        {
            string sql = "UPDATE " + tableName + " SET store_type='1' WHERE import_bundle='" + impBundle + "'";
            int result = SqlHelper.ExecNonQuery(sql, null);
            JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result }, setting);
        }

        public IActionResult VerifyInMangeStore(string tableName, string impBundle)
        {
            string sql = "SELECT COUNT(*) FROM " + tableName + " WHERE (store_type='1' OR store_type='2')  AND import_bundle='" + impBundle + "'";
            object r = SqlHelper.ExecuteScalar(sql, null);
            int result = int.Parse(r.ToString());
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult DeleteImportBundle(string tableName, string impBundle, string userid)
        {
            string sql = "SELECT COUNT(*) FROM " + tableName + " WHERE import_bundle='" + impBundle + "' AND store_type='2'";
            object c = SqlHelper.ExecuteScalar(sql, null);
            var result = 0;
            var title = string.Empty;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (int.Parse(c.ToString()) > 0)
            {
                title = "删除失败！原因：本批数据已入资源总库，请先将有关记录退回到预归档库后再删除。";
                return Json(new { rst = result, info = title });
            }

            sql = "DELETE FROM t_config_imp_catalogs_rec WHERE Unique_code=" + impBundle + "\r\n";
            //sql += "DELETE FROM " + tableName + " WHERE import_bundle='" + impBundle + "'";//;2020年3月11日注释掉
            title = "删除失败！";
            int r = SqlHelper.ExecNonQuery(sql, null);
            if (r > 0)
            {
                result = 1;
                title = "本批次导入记录及对应目录都已被删除！";
            }

            sql = "SELECT Unique_code FROM " + tableName + " WHERE import_bundle='" + impBundle + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, tableName, "删除记录", "预归档", "", ipAddr);
            opInfo.OperTag = "以接收目录批次为单位进行了删除";
            OperateRecHlp.RcdUserOpration(opInfo, dt);//删除一批记录，记录到用户操作记录中;2020年3月11日
            dt.Dispose();

            return Json(new { rst = result, info = title });
        }

        //更新目录接收
        public IActionResult GetUpdatedStoreInfo(string table)
        {
            string sql = "";
            sql += "declare @id int,@cnt1 int,@cnt2 int,@result nvarchar(100) \r\n";
            sql += "select deal_flag = 0,Unique_code,table_code INTO #T FROM t_config_imp_catalogs_rec where table_code='" + table + "' \r\n";
            sql += "select @id = min(Unique_code) FROM #T WHERE deal_flag=0 \r\n";
            sql += "while @id is not null \r\n";
            sql += "    begin \r\n";
            sql += "    select @cnt1=count(*)  FROM " + table + " WHERE store_type = '1' and import_bundle =  + @id \r\n";
            sql += "    select @cnt2=count(*)  FROM " + table + " WHERE store_type = '2' and import_bundle =  + @id \r\n";
            sql += "    set @result = '预归档库：' + convert(varchar(20),@cnt1) + '条 资源总库：' + convert(varchar(20),@cnt2) + '条' \r\n";
            sql += "    update t_config_imp_catalogs_rec set which_store = @result where Unique_code = @id \r\n";
            sql += "    UPDATE #T SET deal_flag = 1 WHERE Unique_code = @id \r\n";
            sql += "    select @id = min(Unique_code) FROM #T WHERE deal_flag=0 \r\n";
            sql += "end \r\n";
            sql += "drop table #T";
            SqlHelper.ExecNonQuery(sql, null);
            return Json('1');
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

public class uploadResult
{
    public string fileName { get; set; }
    public string error { get; set; }
}
