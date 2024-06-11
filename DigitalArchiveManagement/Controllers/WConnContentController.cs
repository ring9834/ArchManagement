using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.IO;
using System.Net.Http;
using System.Data;
using NetCoreDbUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ArchiveFileManagementNs.Controllers
{
    public class WConnContentController : WBaseController
    {
        private IHttpContextAccessor _accessor;
        public WConnContentController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public IActionResult Index(string id, string userid, string other, string othertwo)
        {
            ViewData["HasContent"] = other;
            ViewData["ContentType"] = othertwo;
            ViewData["TableName"] = id;
            ViewData["UserId"] = userid;
            return View();
        }

        public IActionResult PickPathView()
        {
            return View("PickFilePath");
        }

        public IActionResult RecCtnFrmClientView(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View("ImpCntFrmClient");//挂接客户端原文
        }

        public IActionResult GetDirectories(string path)
        {
            var list = new List<FileSystemInfo>();
            if (string.IsNullOrWhiteSpace(path))
            {
                return View(list);
            }
            if (path.StartsWith("c:", StringComparison.OrdinalIgnoreCase))
            {
                var result = 0;
                var info = "此磁盘无权限访问！";
                //JsonSerializerSettings setting = new JsonSerializerSettings();
                return Json(new { rst = result, title = info });
            }
            if (!System.IO.Directory.Exists(path))
            {
                var result = 0;
                var info = "此磁盘不可用或无目录！";
                //JsonSerializerSettings setting = new JsonSerializerSettings();
                return Json(new { rst = result, title = info });
            }
            DirectoryInfo dic = new DirectoryInfo(path);
            list = dic.GetFileSystemInfos()
                .Where(t => !t.Attributes.ToString().ToLower().Contains("hidden"))
                .Where(t => !t.Attributes.ToString().ToLower().Contains("system"))
                .Where(t => !t.Attributes.ToString().ToLower().Contains("readonly"))
                .OrderByDescending(b => b.LastWriteTime)
                .ToList();

            List<FilePathInfo> lp = new List<FilePathInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                FilePathInfo pi = new FilePathInfo();
                pi.Name = list[i].Name;
                var fname = list[i].FullName;
                var fn = fname.Replace("\\", "/");//Important，让绝对路径使用斜杠而不是反斜杠，避开转义的麻烦
                pi.FullName = fn;
                pi.Attributes = list[i].Attributes;
                pi.Extension = list[i].Extension;
                lp.Add(pi);
            }

            //返回部分视图
            return PartialView("DiskPartial", lp);
        }

        public async Task<ContentResult> GetSelData2()
        {
            //var apiUrl = $"https://{Request.Host.Host}:{Request.Host.Port}/js/diskdata/diskconf1.json";
            var apiUrl = $"{AppSetting.GetConfig("UrlTransProtocal:CustomHeader")}{Request.Host.Host}:{Request.Host.Port}/js/diskdata/diskconf1.json";
            var str = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                str = await client.GetStringAsync(apiUrl);
            }
            return Content(str);
        }

        public IActionResult ShowConnRcds(string tableName, int pageIndex, int pageSize)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_archive_link";
            string fields = "Unique_code,data_from ,start_time ,end_time ,link_status ,total_count ,success_count";
            string where = "code_name='" + tableName + "'";
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult DeleteConnRcd(string tableName, string uniqueCode)
        {
            string sql = "DELETE FROM t_archive_link WHERE Unique_code=" + uniqueCode;

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            var result = 0;
            var title = "删除失败！";
            int r = SqlHelper.ExecNonQuery(sql, null);
            if (r > 0)
            {
                result = 1;
                title = "本条挂接记录已被删除！";
            }
            return Json(new { rst = result, info = title });
        }

        public IActionResult AddConnRecds(string table, string datafrom, string userid)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径

            var result = 0;
            var title = string.Empty;
            if (ywrootPath == DBNull.Value || ywrootPath == null)
            {
                title = "此库的原文根路径还未配置，请在档案类型库配置功能中配置！";
                return Json(new { rst = result, info = title });
            }

            sql = string.Empty;
            sql = "SELECT archive_num_field FROM t_config_archive_num_makeup WHERE code_name='" + table + "'";
            object archiveNoFieldName = SqlHelper.ExecuteScalar(sql, null);
            if (archiveNoFieldName == DBNull.Value || archiveNoFieldName == null)
            {
                title = "请为此类型库配置恰当的档号字段，再实施原文挂接！";
                return Json(new { rst = result, info = title });
            }

            var frm = datafrom.Substring(2);
            sql = "INSERT  t_archive_link(code_name,data_from,start_time) \r\n";
            sql += " VALUES('" + table + "','" + frm + "','" + DateTime.Now.ToString() + "'); \r\n";
            sql += "SELECT TOP 1 Unique_code FROM t_archive_link WHERE code_name='" + table + "' ORDER BY Unique_code DESC";
            DataTable uniquecodeDt = SqlHelper.GetDataTable(sql, null);
            object uniquecode = uniquecodeDt.Rows[0][0].ToString();
            uniquecodeDt.Dispose();

            List<string> ids = new List<string>();
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "挂接原文", "预归档", "", ipAddr);
            opInfo.OperTag = "挂接一批原文";
            OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//挂接一批原文的用户操作登记;2020年3月13日

            result = 1;
            title = "挂接记录增加成功！";
            return Json(new { rst = result, info = title, id = uniquecode.ToString(), dhfield = archiveNoFieldName.ToString(), rootpath = ywrootPath.ToString() });
        }

        /// <summary>
        /// cttype原文类型的Unique_code
        /// </summary>
        /// <param name="othertwo"></param>
        /// <returns></returns>
        public async Task<IActionResult> CatalogConnContentWork(int cttype, string datafrom, string table, string userid, string uniquecode, string archiveNoFieldName, string ywrootPath)
        {
            var codeVal = string.Empty;
            GetContentType(cttype, out codeVal);

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = string.Empty;

            //动态创建临时表，此临时表用于批量挂接
            string tempTableName = table + "_temp_" + uniquecode;
            if (codeVal.ToLower().Contains("pdf"))
            {
                sql = "IF OBJECT_ID(N'" + tempTableName + "',N'U') IS NOT NULL \r\n";
                sql += "DELETE FROM " + tempTableName + " \r\n";
                sql += "ELSE \r\n";
                sql += "CREATE TABLE [dbo].[" + tempTableName + "]( \r\n";
                sql += "[DH] [nvarchar](50) NOT NULL,[FilePath] [nvarchar](500) NOT NULL \r\n";
                sql += ") ON [PRIMARY]";
            }
            else//JPG，TIFF等图片格式
            {
                sql = "IF OBJECT_ID(N'" + tempTableName + "',N'U') IS NOT NULL \r\n";
                sql += "DELETE FROM " + tempTableName + " \r\n";
                sql += "ELSE \r\n";
                sql += "CREATE TABLE [dbo].[" + tempTableName + "]( \r\n";
                sql += "[DH] [nvarchar](50) NOT NULL,[FilePath] [xml] \r\n";
                sql += ") ON [PRIMARY]";
            }
            SqlHelper.ExecNonQuery(sql, null);

            //挂接
            if (codeVal.ToLower().Contains("pdf"))
            {
                ConCnnResult cctask = await ConnectCatalogAndContentFile(table, datafrom, tempTableName, codeVal, archiveNoFieldName, ywrootPath);
                //挂接完后处理
                string endtime = DateTime.Now.ToString();
                sql = "UPDATE t_archive_link SET total_count='" + cctask.Total + "',success_count='" + cctask.SuccessCount + "',end_time='" + endtime + "',link_status='2' WHERE Unique_code=" + uniquecode.ToString();
                SqlHelper.ExecNonQuery(sql, null);
                return Json(new { rst = cctask.Result, info = cctask.Title });
            }
            else
            {
                //增添于2020年4月17日
                ConCnnResult cctask = await ConnectCatalogAndImageContentFile(table, datafrom, tempTableName, codeVal, archiveNoFieldName, ywrootPath);
                string endtime = DateTime.Now.ToString();
                sql = "UPDATE t_archive_link SET total_count='" + cctask.Total + "',success_count='" + cctask.SuccessCount + "',end_time='" + endtime + "',link_status='2' WHERE Unique_code=" + uniquecode.ToString();
                SqlHelper.ExecNonQuery(sql, null);
                return Json(new { rst = cctask.Result, info = cctask.Title });
            }
        }

        private async Task<ConCnnResult> ConnectCatalogAndContentFile(string tableName, string dataFrom, string tempTableName, string contentFormat, string archiveNoFieldName, string ywrootPath)
        {
            await Task.Delay(5);

            var result = 0;
            var info = string.Empty;
            DataTable dtTemp = new DataTable(tempTableName);
            dtTemp.Columns.Add("DH", typeof(string));
            dtTemp.Columns.Add("FilePath", typeof(string));
            ExtractArchiveNosToDataTable(dataFrom, ywrootPath.ToString(), dtTemp, contentFormat);//提取临时表
            SqlHelper.InsertByBulk(dtTemp, tempTableName);//效率非常高的批量插入数据表
            int totalCount = dtTemp.Rows.Count;

            string sql = "UPDATE t1 SET yw= t2.FilePath FROM " + tableName + " t1 INNER JOIN " + tempTableName + " t2 ON t1." + archiveNoFieldName + "=t2.DH \r\n";
            sql += "SELECT @@ROWCOUNT \r\n";
            sql += "DROP TABLE " + tempTableName;

            DataTable successcountDt = SqlHelper.GetDataTable(sql, null);
            int successcount = int.Parse(successcountDt.Rows[0][0].ToString());
            successcountDt.Dispose();

            result = 1;//挂接成功
            info = "原文成功挂接" + successcount + "条！";
            ConCnnResult cr = new ConCnnResult();
            cr.Result = result;
            cr.Title = info;
            cr.Total = totalCount;
            cr.SuccessCount = successcount;
            return cr;
        }

        private async Task<ConCnnResult> ConnectCatalogAndImageContentFile(string tableName, string dataFrom, string tempTableName, string contentFormat, string archiveNoFieldName, string ywrootPath)
        {
            await Task.Delay(5);
            var result = 0;
            var info = string.Empty;

            //执行挂接1：在数据库中创建临时表
            string sql = string.Empty;
            sql = "SELECT archive_num_field,archive_num_parts_amount,connect_char FROM t_config_archive_num_makeup WHERE code_name='" + tableName + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            int archiveNoComposedPart = int.Parse(dt.Rows[0]["archive_num_parts_amount"].ToString());
            string splitChar = dt.Rows[0]["connect_char"].ToString();
            dt.Dispose();

            DataTable dtTemp = new DataTable(tempTableName);
            dtTemp.Columns.Add("DH", typeof(string));
            dtTemp.Columns.Add("FilePath", typeof(string));
            ExtractArchiveNosFromFileNameToDataTable(dataFrom, ywrootPath, dtTemp, contentFormat, archiveNoComposedPart, splitChar);//提取临时表
            SqlHelper.InsertByBulk(dtTemp, tempTableName); //效率非常高的批量插入数据表
            int totalCount = dtTemp.Rows.Count;

            //执行挂接2：挂接         
            sql = "UPDATE t1 SET yw_xml= t2.FilePath FROM " + tableName + " t1 INNER JOIN " + tempTableName + " t2 ON t1." + archiveNoFieldName + "=t2.DH \r\n";
            sql += "SELECT @@ROWCOUNT \r\n";
            sql += "DROP TABLE " + tempTableName;

            DataTable successcountDt = SqlHelper.GetDataTable(sql, null);
            int successcount = int.Parse(successcountDt.Rows[0][0].ToString());
            successcountDt.Dispose();

            result = 1;//挂接成功
            info = "图片原文成功挂接" + successcount + "条！";
            ConCnnResult cr = new ConCnnResult();
            cr.Result = result;
            cr.Title = info;
            cr.Total = totalCount;
            cr.SuccessCount = successcount;
            return cr;
        }

        /// <summary>
        /// 把图片所在路径存到XML中
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="dt"></param>
        /// <param name="archiveNoComposedPart">判断是否盛放图片的文件夹（文件夹名称应以档号命名）</param>
        /// <param name="splitChar"></param>
        private void ExtractArchiveNosFromFileNameToDataTable(string folderPath, string ywrootPath, DataTable dt, string contentFormat, int archiveNoComposedPart, string splitChar)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            string[] patterns = contentFormat.Split(';');
            FileInfo[] tempFileInfo;
            FileInfo[] fiA = null;
            for (int i = 0; i < patterns.Length; i++)
            {
                tempFileInfo = di.GetFiles(patterns[i]);
                if (i == 0)
                    fiA = tempFileInfo;
                else
                    fiA = (fiA.Concat((FileInfo[])tempFileInfo.Clone())).ToArray<FileInfo>();
            }

            if (fiA.Length > 0)
            {
                string folderName = fiA[0].Directory.Name;
                int num = folderName.Split(splitChar.ToCharArray()).Length;
                if (num == archiveNoComposedPart)
                {
                    DataRow r = dt.NewRow();
                    r.SetField<string>("DH", folderName);
                    string contentFileRootPath = ywrootPath;
                    string xml = "<YW>";
                    for (int i = 0; i < fiA.Length; i++)
                    {
                        string fullName = fiA[i].FullName;
                        string relativeName = fullName.Substring(contentFileRootPath.Length);
                        xml += "<ywPath>" + relativeName + "</ywPath>";
                    }
                    xml += "</YW>";
                    r.SetField<string>("FilePath", xml);
                    dt.Rows.Add(r);
                }
            }

            DirectoryInfo[] diA = di.GetDirectories();
            for (int i = 0; i < diA.Length; i++)
            {
                ExtractArchiveNosFromFileNameToDataTable(diA[i].FullName, ywrootPath, dt, contentFormat, archiveNoComposedPart, splitChar);
            }
        }

        private void ExtractArchiveNosToDataTable(string folderPath, string ywrootPath, DataTable dt, string contentFormat)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            string[] extens = contentFormat.Split(';');
            for (int m = 0; m < extens.Length; m++)
            {
                string format = extens[m];
                string[] fmts = format.Split('.');
                string format2 = fmts.Length > 1 ? "." + fmts[1] : ".";
                FileInfo[] fiA = di.GetFiles(format);
                for (int j = 0; j < fiA.Length; j++)
                {
                    string name2 = fiA[j].Name.ToLower();
                    string name3 = fiA[j].Name;
                    string dh = name3.Substring(0, name2.LastIndexOf(format2.ToLower()));
                    string contentFileRootPath = ywrootPath;
                    string pdfPathAll = fiA[j].FullName;
                    string pdfPath = pdfPathAll.Substring(contentFileRootPath.Length);

                    DataRow r = dt.NewRow();
                    r.SetField<string>("DH", dh);
                    r.SetField<string>("FilePath", pdfPath);
                    dt.Rows.Add(r);
                }
            }

            DirectoryInfo[] diA = di.GetDirectories();
            for (int i = 0; i < diA.Length; i++)
            {
                ExtractArchiveNosToDataTable(diA[i].FullName, ywrootPath, dt, contentFormat);
            }
        }

        public string GetContentType(int key, out string codeValue)
        {
            string sql = "SELECT code_name,code_value FROM t_config_codes WHERE Unique_code=" + key;
            DataTable t = SqlHelper.GetDataTable(sql, null);
            if (t.Rows.Count == 0)
            {
                codeValue = string.Empty;
                return string.Empty;
            }
            codeValue = t.Rows[0]["code_value"].ToString();
            return t.Rows[0]["code_name"].ToString();
        }

        //modified on 20201014,table param added
        public IActionResult GetContentTp(int key, string table)
        {
            string sql = "SELECT A.code_name,A.code_value,B.yw_path FROM t_config_codes A INNER JOIN t_config_type_tree B ON A.Unique_code=B.content_type \r\n";
            sql += "WHERE A.Unique_code=" + key + " AND B.code='" + table + "'";
            DataTable t = SqlHelper.GetDataTable(sql, null);

            FieldValuePair fv = new FieldValuePair();
            if (t.Rows.Count == 0)
            {
                fv.Field = string.Empty;
                fv.Value = null;
            }
            else
            {
                fv.Field = t.Rows[0]["code_value"].ToString();
                fv.Value = t.Rows[0]["yw_path"].ToString();
            }
            return Json(fv);
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

        public IActionResult IfYwRootExist(string table)
        {
            int rlt;
            string title;
            string sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            if (ywrootPath == DBNull.Value || ywrootPath == null)
            {
                rlt = 0;
                title = "此库的原文根路径还未配置，请在档案类型库配置功能中配置！";
                return Json(new { rst = rlt, info = title });
            }
            return Json(new { rst = 1, info = "" });
        }

        //从客户端上传原文
        [DisableRequestSizeLimit]
        public async Task<IActionResult> RecCtnFrmClient(string id, string userid)
        {
            string table = id;
            string user = userid;
            int rlt;
            string title;
            List<FieldValuePair> kps = new List<FieldValuePair>();
            try
            {
                string sql = "SELECT archive_num_field FROM t_config_archive_num_makeup WHERE code_name='" + table + "'";
                object archiveNoFieldName = SqlHelper.ExecuteScalar(sql, null);
                if (archiveNoFieldName == DBNull.Value || archiveNoFieldName == null)
                {
                    rlt = 0;
                    title = "请为此类型库配置恰当的档号字段，再实施原文挂接！";
                    return Json(new { rst = rlt, info = title });
                }
                string dhField = archiveNoFieldName.ToString();

                string dhGroup = "";
                var ff = Request.Form.Files;
                for (int i = 0; i < ff.Count; i++)
                {
                    var oFile = ff[i];
                    var dhstr = oFile.FileName.Substring(0, oFile.FileName.LastIndexOf('.'));//文件名：档号
                    if (string.IsNullOrEmpty(dhGroup))
                        dhGroup += "'" + dhstr + "'";
                    else
                        dhGroup += ",'" + dhstr + "'";
                }
                sql = "SELECT COUNT(Unique_code) FROM " + table + " WHERE " + dhField + " IN (" + dhGroup + ")";
                object recCountObj = SqlHelper.ExecuteScalar(sql, null);
                if (int.Parse(recCountObj.ToString()) == 0)
                {
                    rlt = 0;
                    title = "将要上传的档案原文在本档案库中未找到对应的档案目录，此次上传挂接任务终止！";
                    return Json(new { rst = rlt, info = title });
                }

                var filePath = "";                

                for (int i = 0; i < ff.Count; i++)
                {
                    FieldValuePair kp = new FieldValuePair();
                    var oFile = ff[i];//使用bootstrap-fileinput控件
                    kp.Field = oFile.FileName.Substring(0, oFile.FileName.LastIndexOf('.'));//文件名：档号
                    if (string.IsNullOrEmpty(filePath))
                    {
                        sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
                        object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
                        if (ywrootPath == DBNull.Value || ywrootPath == null)
                        {
                            rlt = 0;
                            title = "此库的原文根路径还未配置，请在档案类型库配置功能中配置！";
                            return Json(new { rst = rlt, info = title });
                        }
                        filePath = ywrootPath.ToString();
                    }
                    var thePath = "";
                    var relativePath = "";
                    if (filePath.EndsWith('/'))
                    {
                        thePath = filePath + table + "/";
                        relativePath = table + "/";
                    }
                    else
                    {
                        thePath = filePath + "/" + table + "/";
                        relativePath = "/" + table + "/";
                    }
                    kp.Value = relativePath + oFile.FileName;//相对路径
                    kps.Add(kp);

                    if (!Directory.Exists(thePath))//创建路径
                        Directory.CreateDirectory(thePath);

                    //Stream sm = oFile.OpenReadStream();
                    //string filename = oFile.FileName;
                    //string fl = thePath + filename;
                    //FileStream fs = new FileStream(fl, FileMode.Create);
                    //byte[] buffer = new byte[sm.Length];
                    //sm.Read(buffer, 0, buffer.Length);
                    //fs.Write(buffer, 0, buffer.Length);
                    //fs.Dispose();

                    string filename = oFile.FileName;
                    using (var stream = oFile.OpenReadStream())
                    {
                        await WriteFileAsync(stream, Path.Combine(thePath, filename));
                    }
                }

                string sql2 = "";
                List<string> ids = new List<string>();
                for (int i = 0; i < kps.Count; i++)
                {
                    sql2 += "UPDATE " + table + " SET yw='" + kps[i].Value + "' where " + dhField + "='" + kps[i].Field + "' \r\n";
                    ids.Add(kps[i].Field);
                }
                SqlHelper.ExecNonQuery(sql2, null);//批量（多条同时）挂接原文

                string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
                OperationInfo opInfo = MakeOperInfo(userid, table, "客户端上传并挂接档案原文", "客户端挂接原文", "", ipAddr);
                opInfo.OperTag = "客户端挂接档案原文，影响记录" + kps.Count + "条";                
                OperateRecHlp.RcdUserOprationCommon(opInfo, ids);

                rlt = 1;
                title = kps.Count + "个档案原文上传并挂接成功！";
                return Json(new { rst = rlt, info = title });
            }
            catch
            {
                rlt = 0;
                title = "档案原文上传失败！";
                return Json(new { rst = rlt, info = title });
            }
        }

        /// <summary>
        /// 写文件导到磁盘
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="path">文件保存路径</param>
        /// <returns></returns>
        public async Task<int> WriteFileAsync(System.IO.Stream stream, string path)
        {
            const int FILE_WRITE_SIZE = 84975;//写出缓冲区大小
            int writeCount = 0;
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, FILE_WRITE_SIZE, true))
            {
                byte[] byteArr = new byte[FILE_WRITE_SIZE];
                int readCount = 0;
                while ((readCount = await stream.ReadAsync(byteArr, 0, byteArr.Length)) > 0)
                {
                    await fileStream.WriteAsync(byteArr, 0, readCount);
                    writeCount += readCount;
                }
            }
            return writeCount;
        }
    }
}
