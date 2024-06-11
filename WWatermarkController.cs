using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;
using Spire.Pdf.Graphics;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using ArchiveFileManagementNs.Models;
using Newtonsoft.Json;
using Spire.Pdf;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ArchiveFileManagementNs.Controllers
{
    public class WWatermarkController : WBaseController
    {
        private IHttpContextAccessor _accessor;
        public WWatermarkController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetPdfColors()
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            KeyValuePair<string, string> kp1 = new KeyValuePair<string, string>("Y", "黄色");
            list.Add(kp1);
            KeyValuePair<string, string> kp2 = new KeyValuePair<string, string>("B", "蓝色");
            list.Add(kp2);
            KeyValuePair<string, string> kp3 = new KeyValuePair<string, string>("R", "红色");
            list.Add(kp3);
            KeyValuePair<string, string> kp4 = new KeyValuePair<string, string>("G", "绿色");
            list.Add(kp4);
            KeyValuePair<string, string> kp5 = new KeyValuePair<string, string>("BL", "黑色");
            list.Add(kp5);
            KeyValuePair<string, string> kp6 = new KeyValuePair<string, string>("Z", "紫色");
            list.Add(kp6);
            return Json(list);
        }

        public PdfBrush ColorAt(string key)
        {
            if (key.ToLower().Equals("y"))
                return PdfBrushes.DarkOrange;

            if (key.ToLower().Equals("b"))
                return PdfBrushes.Blue;

            if (key.ToLower().Equals("r"))
                return PdfBrushes.Red;

            if (key.ToLower().Equals("g"))
                return PdfBrushes.DarkGreen;

            if (key.ToLower().Equals("bl"))
                return PdfBrushes.Black;

            if (key.ToLower().Equals("z"))
                return PdfBrushes.Purple;
            return PdfBrushes.DarkGray;
        }

        public IActionResult SetWatermarkParam(string words, string rows, string cols, string trsparency, string rotate, string color)
        {
            string sql = "IF(SELECT COUNT(Unique_code) FROM t_config_watermark)>0 \r\n";
            sql += "UPDATE t_config_watermark SET mark_words=@mark_words,mark_rows=@mark_rows,mark_cols=@mark_cols,transparency=@transparency,rotation=@rotation,mark_color=@mark_color \r\n";
            sql += "ELSE \r\n";
            sql += "INSERT INTO t_config_watermark(mark_words,mark_rows,mark_cols,transparency,rotation,mark_color) VALUES(@mark_words,@mark_rows,@mark_cols,@transparency,@rotation,@mark_color)";
            SqlParameter para0 = SqlHelper.MakeInParam("mark_words", words);
            SqlParameter para1 = SqlHelper.MakeInParam("mark_rows", rows);
            SqlParameter para2 = SqlHelper.MakeInParam("mark_cols", cols);
            SqlParameter para3 = SqlHelper.MakeInParam("transparency", trsparency);
            SqlParameter para4 = SqlHelper.MakeInParam("rotation", rotate);
            SqlParameter para5 = SqlHelper.MakeInParam("mark_color", color);
            SqlParameter[] param = new SqlParameter[] { para0, para1, para2, para3, para4, para5 };
            int result = SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功
            return Json(new { rst = result });
        }

        public IActionResult GetWatermarkParam()
        {
            string sql = "SELECT mark_words,mark_rows,mark_cols,transparency,rotation,mark_color FROM t_config_watermark";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public async Task<IActionResult> WaterMarkSContent(string table, List<string> pms, string userid)
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

            sql = "SELECT mark_words,mark_rows,mark_cols,transparency,rotation,mark_color FROM t_config_watermark";
            DataTable dtpara = SqlHelper.GetDataTable(sql, null);
            string words = "";
            int rows = 0;
            int cols = 0;
            float transparency = 0.0f;
            int rotate = 0;
            PdfBrush color = PdfBrushes.Blue;
            if (dtpara.Rows.Count == 0)
            {
                rlt = 0;
                tle = "水印参数还未配置，请到管理配置中配置后继续！";
                dtpara.Dispose();
                return Json(new { rst = rlt, info = tle });
            }
            DataRow dr = dtpara.Rows[0];
            words = dr["mark_words"].ToString();
            rows = int.Parse(dr["mark_rows"].ToString());
            cols = int.Parse(dr["mark_cols"].ToString());
            transparency = float.Parse(dr["transparency"].ToString());
            rotate = int.Parse(dr["rotation"].ToString());
            color = ColorAt(dr["mark_color"].ToString());
            dtpara.Dispose();

            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            string permitPwd = string.Empty;
            int counter = 0;
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
                            GetPasswords(out permitPwd);
                            if (string.IsNullOrEmpty(permitPwd))
                            {
                                rlt = 0;
                                tle = "原文操作密码还未配置，请到管理配置中配置后继续！";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }
                        FileStream fs = new FileStream(path, FileMode.Open);//使用FileStream而不是使用LoadFromFile,是为了解决pdf文件被进程占用的问题 20200629
                        long size = fs.Length;
                        byte[] array = new byte[size];
                        fs.Read(array, 0, array.Length);
                        fs.Close();//必须关闭
                        PdfDocument pdf = new PdfDocument(array, permitPwd);//不论pdf是否加密，最好都带上originalPermissionPassword这个参数，因为如果已加密，则会弹出异常
                        for (int j = 0; j < pdf.Pages.Count; j++)
                        {
                            PdfPageBase page = pdf.Pages[j];
                            //添加文本水印到文件的第一页，设置文本格式
                            PdfTilingBrush brush = new PdfTilingBrush(new SizeF(page.Canvas.ClientSize.Width / cols, page.Canvas.ClientSize.Height / rows)); //设置每行每列几个水印 
                            brush.Graphics.SetTransparency(transparency); //透明度
                            brush.Graphics.Save();
                            brush.Graphics.TranslateTransform(brush.Size.Width / 2, brush.Size.Height / 2);
                            brush.Graphics.RotateTransform(rotate); //旋转角度
                            System.Drawing.Font font = new System.Drawing.Font("黑体", 22, FontStyle.Regular);//使用中文字体，而不是PdfFont的默认字体
                            PdfTrueTypeFont trueTypeFont = new PdfTrueTypeFont(font, true);
                            brush.Graphics.DrawString(words, trueTypeFont, color, 0, 0, new PdfStringFormat(PdfTextAlignment.Center));
                            brush.Graphics.Restore();
                            brush.Graphics.SetTransparency(1);
                            page.Canvas.DrawRectangle(brush, new RectangleF(new PointF(0, 0), page.Canvas.ClientSize));
                        }
                        //保存文件
                        pdf.SaveToFile(path, FileFormat.PDF);
                        counter++;
                    }
                }
            }

            //int cnt = pms.Count;
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "对选定的记录对应的原文加水印", "加水印", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文加水印，" + counter + "条成功，" + (pms.Count - counter) + "条失败（可能原因为原文不存在或路径有误）。";

            OperateRecHlp.RcdUserOprationCommon(opInfo, pms);

            rlt = 1;
            tle = "原文添加水印成功！";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
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

        public async Task<IActionResult> WaterMarkContent(string table, string where, string pms, string searchmode, string userid)
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

            sql = "SELECT mark_words,mark_rows,mark_cols,transparency,rotation,mark_color FROM t_config_watermark";
            DataTable dtpara = SqlHelper.GetDataTable(sql, null);
            string words = "";
            int rows = 0;
            int cols = 0;
            float transparency = 0.0f;
            int rotate = 0;
            PdfBrush color = PdfBrushes.Blue;
            if (dtpara.Rows.Count == 0)
            {
                rlt = 0;
                tle = "水印参数还未配置，请到管理配置中配置后继续！";
                dtpara.Dispose();
                return Json(new { rst = rlt, info = tle });
            }
            DataRow dr = dtpara.Rows[0];
            words = dr["mark_words"].ToString();
            rows = int.Parse(dr["mark_rows"].ToString());
            cols = int.Parse(dr["mark_cols"].ToString());
            transparency = float.Parse(dr["transparency"].ToString());
            rotate = int.Parse(dr["rotation"].ToString());
            color = ColorAt(dr["mark_color"].ToString());
            dtpara.Dispose();

            sql = "SELECT yw FROM " + table + " WHERE " + where;
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            string permitPwd = string.Empty;
            int counter = 0;
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
                            GetPasswords(out permitPwd);
                            if (string.IsNullOrEmpty(permitPwd))
                            {
                                rlt = 0;
                                tle = "原文操作密码还未配置，请到管理配置中配置后继续！";
                                return Json(new { rst = rlt, info = tle });
                            }
                        }
                        FileStream fs = new FileStream(path, FileMode.Open);//使用FileStream而不是使用LoadFromFile,是为了解决pdf文件被进程占用的问题 20200629
                        long size = fs.Length;
                        byte[] array = new byte[size];
                        fs.Read(array, 0, array.Length);
                        fs.Close();//必须关闭
                        PdfDocument pdf = new PdfDocument(array, permitPwd);//不论pdf是否加密，最好都带上originalPermissionPassword这个参数，因为如果已加密，则会弹出异常
                        for (int j = 0; j < pdf.Pages.Count; j++)
                        {
                            PdfPageBase page = pdf.Pages[j];
                            //添加文本水印到文件的第一页，设置文本格式
                            PdfTilingBrush brush = new PdfTilingBrush(new SizeF(page.Canvas.ClientSize.Width / cols, page.Canvas.ClientSize.Height / rows)); //设置每行每列几个水印
                            brush.Graphics.SetTransparency(transparency); //透明度
                            brush.Graphics.Save();
                            brush.Graphics.TranslateTransform(brush.Size.Width / 2, brush.Size.Height / 2);
                            brush.Graphics.RotateTransform(rotate); //旋转角度
                            System.Drawing.Font font = new System.Drawing.Font("黑体", 22, FontStyle.Regular);//使用中文字体，而不是PdfFont的默认字体
                            PdfTrueTypeFont trueTypeFont = new PdfTrueTypeFont(font, true);
                            brush.Graphics.DrawString(words, trueTypeFont, color, 0, 0, new PdfStringFormat(PdfTextAlignment.Center));
                            brush.Graphics.Restore();
                            brush.Graphics.SetTransparency(1);
                            page.Canvas.DrawRectangle(brush, new RectangleF(new PointF(0, 0), page.Canvas.ClientSize));
                        }
                        //保存文件
                        pdf.SaveToFile(path, FileFormat.PDF);
                        counter++;
                    }

                }
            }

            int cnt = dt.Rows.Count;
            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "原文批量加水印", "加水印", "", ipAddr);
            opInfo.OperTag = searchConditon + "原文加水印，" + counter + "条成功，" + (cnt - counter) + "条失败（可能原因为原文不存在或路径有误）。";
            if (cnt <= 500)
            {
                sql = "SELECT Unique_code FROM " + table + " WHERE " + where;
                DataTable dtrec = SqlHelper.GetDataTable(sql, list.ToArray());
                OperateRecHlp.RcdUserOpration2(opInfo, dtrec);//批量加水印
            }
            else
            {
                List<string> ids = new List<string>();
                OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//记录超过数量时，就不记录影响的记录ID了
            }

            rlt = 1;
            tle = "原文批量添加水印成功！";
            dt.Dispose();
            return Json(new { rst = rlt, info = tle });
        }

        public void GetPasswords(out string permit)
        {
            string sql = "SELECT pwd_type,name,pwd,Unique_code FROM t_config_yw_permit";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            DataRow[] drs = table.Select("pwd_type = 'foropen'");
            drs = table.Select("pwd_type = 'forpermmit'");
            if (drs.Length > 0)
                permit = drs[0]["pwd"].ToString();
            else
                permit = "";
        }
    }
}
