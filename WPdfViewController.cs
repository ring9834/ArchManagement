using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.IO.Image;

namespace ArchiveFileManagementNs.Controllers
{
    public class WPdfViewController : WBaseController
    {
        public async Task<IActionResult> Index(string id, string userid, string other)
        {
            //await Task.Run(() =>
            //{
            string sql = "SELECT yw FROM " + id + " WHERE Unique_code=" + other;
            object r = SqlHelper.ExecuteScalar(sql, null);
            //int result = 0;
            //string title = string.Empty;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            //if (r == DBNull.Value || r == null || string.IsNullOrWhiteSpace(r.ToString()))
            //{
            //    title = "本目录没有原文，或没有挂接！";
            //    return Json(new { rst = result, info = title }, setting);
            //}

            sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + id + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
                                                                   //if (ywrootPath == DBNull.Value || ywrootPath == null)
                                                                   //{
                                                                   //    title = "此库的原文根路径还未配置，请在档案类型库配置功能中配置！";
                                                                   //    return Json(new { rst = result, info = title }, setting);
                                                                   //}
            string path = ywrootPath + r.ToString();
            path = path.Replace("\\", "/");

            //result = 1;
            //title = "原文存在！";
            //return Json(new { rst = result, info = title, ywpath = path }, setting);

            if (System.IO.File.Exists(path))//added on 20201124
            {
                FileInfo file = new FileInfo(path);
                var fs = new FileStream(path, FileMode.Open,FileAccess.Read,FileShare.Read);//读取只读文件，必须加FileMode.Open,FileAccess.Read,FileShare.Read，否则会报错 20201224
                return new FileStreamResult(fs, "application/pdf");
            }
            return View("FileNotExist");//提示前端文件不存在或位置发生改变

            //});
            //return null;
        }

        public async Task<IActionResult> Index1(string id)
        {
            //await Task.Run(() =>
            //{
            var fs = new FileStream(id, FileMode.Open);
            return new FileStreamResult(fs, "application/pdf");
            //});
            //return null;
        }

        public async Task<IActionResult> IfYwExists(string table, string userid, string uniquecode)
        {
            //await Task.Run(() =>
            //{
            string sql = "SELECT yw FROM " + table + " WHERE Unique_code=" + uniquecode;
            object r = SqlHelper.ExecuteScalar(sql, null);
            int result = 0;
            string title = string.Empty;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (r == DBNull.Value || r == null || string.IsNullOrWhiteSpace(r.ToString()))
            {
                title = "本目录没有原文，或没有挂接！";
                return Json(new { rst = result, info = title });
            }

            sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + table + "'";
            object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径
            if (ywrootPath == DBNull.Value || ywrootPath == null)
            {
                title = "此库的原文根路径还未配置，请在档案类型库配置功能中配置！";
                return Json(new { rst = result, info = title });
            }
            string path = ywrootPath + r.ToString();
            path = path.Replace("\\", "/");

            result = 1;
            title = "原文存在！";
            return Json(new { rst = result, info = title, ywpath = path });

            //var fs = new FileStream(path, FileMode.Open);
            //return new FileStreamResult(fs, "application/pdf");
            //});
            //return null;
        }

        public IActionResult IfXmlYwExists(string table, string userid, string uniquecode)
        {
            int result = 0;
            string title = string.Empty;
            //JsonSerializerSettings setting = new JsonSerializerSettings();

            string sql = "SELECT archive_num_field FROM t_config_archive_num_makeup WHERE code_name='" + table + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count == 0)
            {
                dt.Dispose();
                title = "档号还未配置，请到配置页面操作后再继续！";
                return Json(new { rst = result, info = title });
            }
            if (dt.Rows[0]["archive_num_field"] == DBNull.Value || dt.Rows[0]["archive_num_field"] == null)
            {
                dt.Dispose();
                title = "档号对应字段还未配置，请到配置页面操作后再继续！";
                return Json(new { rst = result, info = title });
            }
            string archiveNoFieldName = dt.Rows[0]["archive_num_field"].ToString();
            sql = "SELECT yw_xml," + archiveNoFieldName + " FROM " + table + " WHERE Unique_code=" + uniquecode;
            dt = SqlHelper.GetDataTable(sql, null);
            object ywXMLStr = dt.Rows[0]["yw_xml"];
            if (ywXMLStr == DBNull.Value || string.IsNullOrEmpty(ywXMLStr.ToString()))
            {
                dt.Dispose();
                title = "本条目录的原文还未挂接，或没有原文！";
                return Json(new { rst = result, info = title });
            }
            dt.Dispose();
            result = 1;
            title = "原文存在！";
            return Json(new { rst = result, info = title });
        }

        public async Task<IActionResult> XmlYwShow(string id, string userid, string other)
        {
            await Task.Delay(5);
            string sql = "SELECT yw_xml FROM " + id + " WHERE Unique_code=" + other;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            object ywXMLStr = dt.Rows[0]["yw_xml"];
            dt.Dispose();

            //await Task.Run(() => {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ywXMLStr.ToString());
            XmlNodeList nodeList = doc.SelectNodes(@"YW/ywPath");
            if (nodeList.Count > 0)
            {
                sql = "SELECT yw_path FROM t_config_type_tree WHERE code='" + id + "'";
                object ywrootPath = SqlHelper.ExecuteScalar(sql, null);//配置的原文跟路径

                string dest = "";//生成目标PDF的全路径

                //FileInfo fi0 = new FileInfo(nodeList[0].InnerText);
                string path0 = ywrootPath + nodeList[0].InnerText;
                path0 = path0.Replace("\\", "/");

                if (System.IO.File.Exists(path0))
                {
                    string[] arr = path0.Split("/");
                    string dh = arr[arr.Length - 2];//取倒数第二个元素
                    dest = path0.Replace(arr[arr.Length - 1], dh + ".pdf");//把arr中倒数第一个元素替换为dh+".pdf"

                    if (System.IO.File.Exists(dest))//如果PDF之前已生成过，就不用再生成了
                    {
                        var fs = new FileStream(dest, FileMode.Open);
                        return new FileStreamResult(fs, "application/pdf");//返回
                    }
                }

                PdfWriter writer = null;
                PdfDocument pdf = null;
                Document document = null;

                for (int i = 0; i < nodeList.Count; i++)
                {
                    //FileInfo fi = new FileInfo(nodeList[i].InnerText);
                    //string fileName = fi.Name;
                    string path = ywrootPath + nodeList[i].InnerText;
                    path = path.Replace("\\", "/");

                    if (System.IO.File.Exists(path))//如果图片存在
                    {
                        if (string.IsNullOrEmpty(dest))//如果为空
                        {
                            string[] arr = path.Split("/");
                            string dh = arr[arr.Length - 2];//取倒数第二个元素
                            dest = path.Replace(arr[arr.Length - 1], dh + ".pdf");//把arr中倒数第一个元素替换为dh+".pdf"
                        }
                        if (writer == null)//以下三个对象只初始化一次
                        {
                            writer = new PdfWriter(dest);//Initialize PDF writer   
                            pdf = new PdfDocument(writer);//Initialize PDF document   
                            document = new Document(pdf);// Initialize document
                        }

                        iText.Layout.Element.Image img = new iText.Layout.Element.Image(ImageDataFactory.Create(path));
                        document.Add(img);//生成单层PDF，在PDF文档中添加图片
                    }
                }
                if (document != null)
                {
                    document.Close();
                    var fs = new FileStream(dest, FileMode.Open);
                    return new FileStreamResult(fs, "application/pdf");
                }
            }

            //var fnal = new FileStream("c", FileMode.Open);
            //return new FileStreamResult(fnal, "application/pdf");//返回空文件
            return Json("");//如果已经挂接图片原文，但后来图片路径变化了，这时返回空字符串，以防页面出现错误报警

            //});
        }
    }

    public static class HttpRequestExtensions
    {
        public static string GetAbsoluteUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }
    }
}
