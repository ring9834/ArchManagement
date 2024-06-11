using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.IO;
using System.Net.Http;
using System.Text;
using NetCoreDbUtility;
using System.Data;
using Spire.Pdf;
using Spire.Pdf.General.Find;
using System.Web;

namespace ArchiveFileManagementNs.Controllers
{
    public class WFullContSearchController : WBaseController
    {
        public IActionResult Index(string userid)
        {
            ViewData["userid"] = userid;
            return View();
        }

        public IActionResult PickPathView()
        {
            return View("PickFilePath");
        }

        public IActionResult IndexCreationConfView()
        {
            return View("IndexCreationConf");
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

        private void MakeFullContentIndexFromTxt(string path)
        {
            //this.IsGenerating = true;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fi0 = di.GetFiles("*.txt");
            for (int n = 0; n < fi0.Length; n++)
            {
                FileInfo fitxt = fi0.ElementAt(n);
                string file = fitxt.FullName;
                //List<SearchUnit> list = new List<SearchUnit>();
                SearchUnit su = new SearchUnit();
                su.id = fitxt.Name.Replace(fitxt.Extension, string.Empty);
                su.content = ReadContentFromTxt(file);//从txt文件中读取
                su.imageurl = file.Replace(fitxt.Extension, ".pdf");//使用对应的PDF文件
                su.updatetime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                //list.Add(su);
                PanGuLuceneHelper.instance.CreateIndex(su);//添加索引  
                //ShowDynamicTextInRichText("已为：" + file + "创建全文索引 " + su.updatetime + "... \r\n");
            }
            DirectoryInfo[] diA = di.GetDirectories();
            for (int i = 0; i < diA.Length; i++)
            {
                MakeFullContentIndexFromTxt(diA[i].FullName);
            }
        }

        /// <summary>
        /// 使用TXT文件的内容建立索引
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<IActionResult> CreateTxtFileIndexes(string path)
        {
            int result = 0;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (!Directory.Exists(path))//输入的路径不存在
            {
                return Json(new { rst = result });
            }
            //List<SearchUnit> list = new List<SearchUnit>();
            bool flag = await MakeIndexFromTxt(path);

            int docCount = PanGuLuceneHelper.instance.SpecificWriter.NumDocs;
            PanGuLuceneHelper.instance.SpecificWriter.Dispose();//销毁indexwriter
            if (flag)
            {
                result = 1;
                AddIndexCreationRec(path.Substring(1), docCount.ToString(), new DateTime().ToString("yyyy-MM-dd hh:mm:ss"));
            }

            return Json(new { rst = result });
        }

        public async Task<bool> MakeIndexFromTxt(string path)
        {
            await Task.Delay(5);
            MakeFullContentIndexFromTxt(path);//递归

            return true;
        }


        /// <summary>
        /// 使用PDF文件的内容建立索引
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public async Task<IActionResult> CreatePdfFileIndexes(string path)
        {
            int result = 0;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (!Directory.Exists(path))//输入的路径不存在
            {
                return Json(new { rst = result });
            }
            //List<SearchUnit> list = new List<SearchUnit>();
            bool flag = await MakeIndexFromPdf(path);//创建索引

            int docCount = PanGuLuceneHelper.instance.SpecificWriter.NumDocs;
            //PanGuLuceneHelper.instance.SpecificWriter.Dispose();//销毁indexwriter
            PanGuLuceneHelper.instance.SpecificWriter.Commit();
            PanGuLuceneHelper.instance.SpecificWriter.WaitForMerges();
            if (flag)
            {
                result = 1;
                AddIndexCreationRec(path.Substring(2), docCount.ToString(), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            }

            //return Json(new { rst = result , fileFrom = path.Substring(2), docCount = docCount.ToString(), indexTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") }, setting);
            return Json(new { rst = result });
        }

        public async Task<bool> MakeIndexFromPdf(string path)
        {
            await Task.Delay(5);
            MakeFullContentIndexFromPdf(path);//递归
            return true;
        }
        private void MakeFullContentIndexFromPdf(string path)
        {
            //this.IsGenerating = true;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fi0 = di.GetFiles("*.pdf");
            for (int n = 0; n < fi0.Length; n++)
            {
                FileInfo fitxt = fi0.ElementAt(n);
                string file = fitxt.FullName;
                //List<SearchUnit> list = new List<SearchUnit>();
                SearchUnit su = new SearchUnit();
                su.id = fitxt.Name.Replace(fitxt.Extension, string.Empty);
                //su.content = ExtractTxtFromPdfFile(file);//从PDF中读取内容
                //su.content = ExtractTextFromPDFPage(file);
                su.content = ExtractTextFromPDFPageUsingSpire(file);
                su.imageurl = file;
                su.updatetime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                //list.Add(su);
                PanGuLuceneHelper.instance.CreateIndex(su);//添加索引  
                //ShowDynamicTextInRichText("已为：" + file + "创建全文索引 " + su.updatetime + "... \r\n");
            }

            DirectoryInfo[] diA = di.GetDirectories();
            for (int i = 0; i < diA.Length; i++)
            {
                MakeFullContentIndexFromPdf(diA[i].FullName);
            }
        }

        private string ReadContentFromTxt(string fileName)
        {
            string str = string.Empty;
            try
            {
                StreamReader sr = new StreamReader(fileName, false);
                while (!sr.EndOfStream)
                {
                    str += sr.ReadLine().ToString();
                }
                sr.Close();
            }
            catch { }
            return str;
        }

        /// <summary>
        /// 使用Spire.PDF框架
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <returns></returns>
        public string ExtractTextFromPDFPageUsingSpire(string pdfFile)
        {
            StringBuilder content = new StringBuilder();
            try
            {
                PdfDocument document = new PdfDocument();
                document.LoadFromFile(pdfFile);

                foreach (PdfPageBase page in document.Pages)
                {
                    content.Append(page.ExtractText());
                }
            }
            catch { }
            return content.ToString();
        }

        public void AddIndexCreationRec(string fileFrom, string docCount, string indexTime)
        {
            string sql = "INSERT INTO t_index_creation_rec(filep_from,docu_count,indexc_time) VALUES('" + fileFrom + "','" + docCount + "','" + indexTime + "')";
            //SqlParameter para1 = SqlHelper.MakeInParam("filep_from", fileFrom);
            //SqlParameter para2 = SqlHelper.MakeInParam("docu_count", docCount);
            //SqlParameter para3 = SqlHelper.MakeInParam("indexc_time", indexTime);
            //SqlParameter[] param = new SqlParameter[] { para1, para2, para3 };
            int result = SqlHelper.ExecNonQuery(sql, null);
        }

        public IActionResult GetIndexCreationRecs(int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_index_creation_rec";
            string fields = "Unique_code,filep_from,docu_count,indexc_time";
            string where = " 1=1 ";
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        /// <summary>
        /// 搜索结果（分页）
        /// </summary>
        /// <param name="keyWords"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult FullContSearchByKeyword(string keyWords)
        {
            List<SearchUnit> searchlist = PanGuLuceneHelper.instance.Search(keyWords);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { total = searchlist.Count, rows = searchlist });
        }

        public IActionResult GetHighlightedPdf(string id, string userid)
        {
            string fileName = HttpUtility.UrlDecode(Base64Decode(id));
            string keyword = userid;
            //fileName = @"D:\pdf\0173-003\004\0173-003-004-003\0173-003-004-003.pdf";
            PdfDocument pdf = new PdfDocument();
            if (System.IO.File.Exists(fileName))
            {
                pdf.LoadFromFile(fileName);
                //PdfTextFind[] AllMatchedText = null;                
                for (int i = 0; i < pdf.Pages.Count; i++)
                {
                    try
                    {
                        PdfTextFindCollection cltion = pdf.Pages[i].FindText(keyword, TextFindParameter.None);
                        if (cltion != null)
                        {
                            PdfTextFind[] findResults = cltion.Finds;
                            if (findResults != null)
                            {
                                for (int j = 0; j < findResults.Length; j++)
                                {
                                    findResults[j].ApplyHighLight();
                                    //text.ApplyHighLight(Color.Green); //设置自定义背景颜色
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                Stream fs = new MemoryStream();
                pdf.SaveToStream(fs);
                fs.Seek(0, SeekOrigin.Begin);//从流的起始位置开始
                return new FileStreamResult(fs, "application/pdf");
            }
            return View("FileNotExist");
        }

        public IActionResult GetEncoderStr(string toEncode)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string result = Base64Encode(toEncode);
            return Json(new { rst = result });
        }

        /// <summary>
        /// 将字符串转换成base64格式,使用UTF8字符集
        /// </summary>
        /// <param name="content">加密内容</param>
        /// <returns></returns>
        public string Base64Encode(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// 将base64格式，转换utf8
        /// </summary>
        /// <param name="content">解密内容</param>
        /// <returns></returns>
        public string Base64Decode(string content)
        {
            byte[] bytes = Convert.FromBase64String(content);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
