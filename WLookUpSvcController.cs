using ArchiveFileManagementNs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
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
    public class WLookUpSvcController : WBaseController
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public WLookUpSvcController(IWebHostEnvironment hostingEnvironment)//IHostingEnvironment在.net CORE3.0中已弃用
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(string userid)
        {
            //string webRootPath = _hostingEnvironment.WebRootPath;
            //string contentRootPath = _hostingEnvironment.ContentRootPath;
            //return Content(webRootPath + "\n" + contentRootPath);
            ViewData["userid"] = userid;
            return View();
        }

        public IActionResult CamCaptureView()
        {
            return View("FetchPhotoFrmCam");
        }

        public IActionResult ImgCaptureView()
        {
            return View("FetchPhotoFrmImage");
        }

        public IActionResult userRegView(string id, string userid)
        {
            ViewData["CorB"] = id;
            ViewData["userid"] = userid;
            return View("RegistRqsterInfo");
        }

        public IActionResult userRegViewB(string id, string userid)
        {
            ViewData["CorB"] = id;
            ViewData["userid"] = userid;
            return View("RegistRqsterBInfo");
        }

        public IActionResult FileOutView(string id, string userid, string other)
        {
            ViewData["CorB"] = id;
            ViewData["userid"] = userid;
            ViewData["requestId"] = other;//某次调档的ID，以一次登记为准
            return View("FileOutOprtn");
        }

        public IActionResult FileOutViewB(string id, string userid, string other)
        {
            ViewData["CorB"] = id;
            ViewData["userid"] = userid;
            ViewData["requestId"] = other;//某次调档的ID，以一次登记为准
            return View("FileOutOprtnB");
        }

        public IActionResult PrimSearchView()
        {
            //ViewData["userid"] = userid;
            return View("PrimSearch");
        }
        public IActionResult TableView(string id)
        {
            ViewData["type"] = id;//p表示基本搜索，p表示高级索索  SheePrintView
            return View("PickTable");
        }

        public IActionResult SuperSearchView(string id)
        {
            ViewData["table"] = id;
            return View("SuperSearch");
        }

        public IActionResult ElecSearchView(string id)
        {
            ViewData["requestId"] = id;//通过URL放table的位置传过来 
            return View("ElecContentSearch");
        }

        public IActionResult ElecSearchBView(string id)
        {
            ViewData["requestId"] = id;
            return View("ElecContentSearchB");
        }

        public IActionResult ViewCartView(string id, string userid)
        {
            ViewData["requestId"] = id;
            ViewData["userid"] = userid;
            return View("ViewCartWin");
        }

        public IActionResult SheePrintView(string id, string userid)
        {
            ViewData["requestId"] = id;
            ViewData["userid"] = userid;
            return View("PrintSheet");
        }

        public IActionResult SheeBPrintView(string id, string userid, string other)
        {
            ViewData["requestId"] = id;
            ViewData["userid"] = userid;
            ViewData["beOver"] = other;
            return View("PrintSheetB");
        }

        public IActionResult ReturnBckView(string id, string userid, string other)
        {
            ViewData["requestId"] = id;
            ViewData["userid"] = userid;
            ViewData["beOver"] = other;//added on 20201123
            return View("GiveThemBack");
        }
        public IActionResult ProlongDaysView(string id, string userid)
        {
            ViewData["requestId"] = id;
            ViewData["itemId"] = userid;
            return View("ProlongDays");
        }

        public IActionResult HintExpiredView(string id, string userid)
        {
            ViewData["userid"] = userid;
            return View("HintExpired");
        }

        public IActionResult AllAuditsView(string id, string userid)
        {
            ViewData["userid"] = userid;
            return View("AuditsAll");
        }

        public IActionResult SearchCndtionView(string id, string userid)
        {
            ViewData["userid"] = userid;
            return View("RqstSeasrch");
        }

        public IActionResult StcPrvView(string id, string userid)
        {
            ViewData["userid"] = userid;
            return View("StcPrv");
        }

        public IActionResult Capture(string imgData)
        {
            string inputStr = imgData;
            byte[] arr = Convert.FromBase64String(inputStr);
            MemoryStream ms = new MemoryStream(arr);
            Bitmap bmps = new Bitmap(ms);

            string webRootPath = _hostingEnvironment.WebRootPath;
            //if (!Directory.Exists(AppContext.BaseDirectory + "phts\\")) 
            //{
            //    Directory.CreateDirectory(AppContext.BaseDirectory + "phts\\");
            //}
            if (!Directory.Exists(webRootPath + "\\phts\\"))
            {
                Directory.CreateDirectory(webRootPath + "\\phts\\");
            }
            string dateStr = DateTime.Now.ToString("yyMMddhhmmss");
            //string relativePath = "/phts/"+ dateStr + "_photo_.jpg";
            string relativePath = "\\phts\\" + dateStr + "_photo_.jpg";
            string fileName = webRootPath + "\\phts\\" + dateStr + "_photo_.jpg";
            bmps.Save(fileName);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = relativePath });
        }

        //查档证件类型
        public IActionResult GetCertiTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='CDZJLX')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        //查档目的
        public IActionResult GetReqstAimTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='CDMDD')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult AddRequesterInfo(string userName, string certifType, string certifNo, string workplace, string address, string telephone, string lookUpAim, string lookUpContent, string userid)
        {
            string vitalization_code = DateTime.Now.ToString("yyyyMMddhhmmss");//查档唯一编号
            string file_clerk = userid;//档案员
            string checkin_time = DateTime.Now.ToString("yyyy-MM-dd");//查档时间
            string aiming = lookUpAim;//查档目的
            string file_content = lookUpContent;//查档内容

            XmlDocument requester_doc = new XmlDocument();
            XmlElement requester_root = requester_doc.CreateElement("requestInfo");
            XmlElement requester_elemt = requester_doc.CreateElement("requester");
            requester_elemt.SetAttribute("userName", userName);//查档者姓名
            requester_elemt.SetAttribute("certifType", certifType);//
            requester_elemt.SetAttribute("certifNo", certifNo);//证件号码
            requester_elemt.SetAttribute("workplace", workplace);//工作单位或就学学校
            requester_elemt.SetAttribute("address", address);//地址
            requester_elemt.SetAttribute("telephone", telephone);//电话
            requester_elemt.SetAttribute("userPic", "");//用户头像或照片
            requester_root.AppendChild(requester_elemt);
            requester_doc.AppendChild(requester_root);

            string sql = "INSERT INTO t_archive_use_rec(vitalization_code,file_clerk,checkin_time,aiming,file_content,file_requester_info) \r\n";
            sql += "VALUES(@vitalization_code,@file_clerk,@checkin_time,@aiming,@file_content,@file_requester_info)";

            SqlParameter para1 = SqlHelper.MakeInParam("vitalization_code", vitalization_code);
            SqlParameter para2 = SqlHelper.MakeInParam("file_clerk", file_clerk);
            SqlParameter para3 = SqlHelper.MakeInParam("checkin_time", checkin_time);
            SqlParameter para4 = SqlHelper.MakeInParam("aiming", aiming);
            SqlParameter para5 = SqlHelper.MakeInParam("file_content", file_content == null ? "" : file_content);
            SqlParameter para6 = SqlHelper.MakeInParam("file_requester_info", requester_doc.OuterXml);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6 };
            int result = SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult AddRequesterBInfo(string userName, string certifType, string certifNo, string workplace, string address, string telephone, string lookUpAim, string lookUpContent, string userid, string corb)
        {
            string vitalization_code = DateTime.Now.ToString("yyyyMMddhhmmss");//借档唯一编号
            string file_clerk = userid;//档案员
            string checkin_time = DateTime.Now.ToString("yyyy-MM-dd");//借档时间
            string aiming = lookUpAim;//借档目的
            string file_content = lookUpContent;//借档内容

            XmlDocument requester_doc = new XmlDocument();
            XmlElement requester_root = requester_doc.CreateElement("requestInfo");
            XmlElement requester_elemt = requester_doc.CreateElement("requester");
            requester_elemt.SetAttribute("userName", userName);//借档者姓名
            requester_elemt.SetAttribute("certifType", certifType);//
            requester_elemt.SetAttribute("certifNo", certifNo);//证件号码
            requester_elemt.SetAttribute("workplace", workplace);//工作单位或就学学校
            requester_elemt.SetAttribute("address", address);//地址
            requester_elemt.SetAttribute("telephone", telephone);//电话
            requester_elemt.SetAttribute("userPic", "");//用户头像或照片
            requester_root.AppendChild(requester_elemt);
            requester_doc.AppendChild(requester_root);

            string sql = "INSERT INTO t_archive_use_rec(vitalization_code,file_clerk,checkin_time,aiming,file_content,file_requester_info,copy_borrow) \r\n";
            sql += "VALUES(@vitalization_code,@file_clerk,@checkin_time,@aiming,@file_content,@file_requester_info,@copy_borrow)";

            SqlParameter para1 = SqlHelper.MakeInParam("vitalization_code", vitalization_code);
            SqlParameter para2 = SqlHelper.MakeInParam("file_clerk", file_clerk);
            SqlParameter para3 = SqlHelper.MakeInParam("checkin_time", checkin_time);
            SqlParameter para4 = SqlHelper.MakeInParam("aiming", aiming);
            SqlParameter para5 = SqlHelper.MakeInParam("file_content", file_content);
            SqlParameter para6 = SqlHelper.MakeInParam("file_requester_info", requester_doc.OuterXml);
            SqlParameter para7 = SqlHelper.MakeInParam("copy_borrow", corb);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6, para7 };
            int result = SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }


        public IActionResult ftchRequestInfos(int pageSize, int pageIndex)
        {
            //string sql = "SELECT Unique_code,vitalization_code,checkin_time,be_over,x.m.value('(@userName)[1]','nvarchar(MAX)') file_requester FROM t_archive_use_rec T \r\n";
            //sql += "CROSS APPLY T.file_requester_info.nodes('/requestInfo/requester') x(m)";

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_archive_use_rec T";
            string fields = " Unique_code,vitalization_code,checkin_time,be_over,copy_borrow, \r\n";
            fields += " x.m.value('(@userName)[1]','nvarchar(MAX)') file_requester ";
            string innerjoin = " CROSS APPLY T.file_requester_info.nodes('/requestInfo/requester') x(m) \r\n";
            string where = " 1=1 ";
            string sort = " Unique_code DESC ";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult ftchRequestCbInfos(int pageSize, int pageIndex, string copyOrBorrow)
        {
            //string sql = "SELECT Unique_code,vitalization_code,checkin_time,be_over,x.m.value('(@userName)[1]','nvarchar(MAX)') file_requester FROM t_archive_use_rec T \r\n";
            //sql += "CROSS APPLY T.file_requester_info.nodes('/requestInfo/requester') x(m)";

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_archive_use_rec T";
            string fields = " Unique_code,vitalization_code,checkin_time,be_over,copy_borrow, \r\n";
            fields += " x.m.value('(@userName)[1]','nvarchar(MAX)') file_requester ";
            string innerjoin = " CROSS APPLY T.file_requester_info.nodes('/requestInfo/requester') x(m) \r\n";
            string where = " 1=1 AND copy_borrow='" + copyOrBorrow + "'";
            string sort = " Unique_code DESC ";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
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

        public IActionResult CreateSuperSearchControls(string id)
        {
            string fieldStr = string.Empty;
            string colFields = GetFieldsStrShowing(id, out fieldStr);

            string sql = "SELECT t2.col_name,t2.show_name,t3.code_value,t3.code_name FROM t_config_primary_search AS t1 INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code\r\n ";
            sql += "INNER JOIN t_config_codes AS t3 ON t1.search_code=t3.Unique_code \r\n";
            sql += "WHERE t2.code='" + id + "' ORDER BY t1.order_number ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rows = dt, fieldsStr = fieldStr, colFieldStr = colFields });
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

        public async Task<IActionResult> IntoShoppingCartPs(string searchMode, string table, string requestId, string catalogId)//基本搜索和高级搜索用
        {
            int result = 0;
            await Task.Run(() =>
            {
                string sql0 = "DECLARE @archNum nvarchar(MAX),@sql nvarchar(MAX) \r\n";
                sql0 += "SELECT @archNum=archive_num_field FROM t_config_archive_num_makeup WHERE code_name='" + table + "' \r\n";
                sql0 += "IF @archNum IS NOT NULL AND trim(@archNum) <> '' \r\n";
                sql0 += "BEGIN \r\n";
                sql0 += "   SET @sql = 'SELECT '+@archNum+' FROM " + table + " WHERE Unique_code = " + catalogId + "' \r\n";
                sql0 += "   EXEC(@sql) \r\n";//执行动态sql
                sql0 += "END \r\n";
                sql0 += "ELSE \r\n";
                sql0 += "BEGIN \r\n";
                sql0 += "   SELECT '' \r\n";
                sql0 += "END \r\n";
                object dhObj = SqlHelper.ExecuteScalar(sql0, null);//档号值

                string initXml = "<Cart><items mode=\"p\"></items><items mode=\"s\"></items><items mode=\"f\"></items></Cart>";
                string xmlItem = "<item id=\"" + DateTime.Now.ToString("yyyyMMddhhmmss") + "\" table=\"" + table + "\" catalogid=\"" + catalogId + "\" dh=\"" + dhObj.ToString() + "\"></item>";//加id属性的目的，是避免item重复致使sql报错
                string sql = "IF (SELECT file_out FROM t_archive_use_rec WHERE Unique_code=" + requestId + ") IS NULL \r\n";
                sql += "BEGIN \r\n";
                sql += "   UPDATE  t_archive_use_rec SET file_out='" + initXml + "' WHERE Unique_code=" + requestId + "; \r\n";
                sql += "   update t_archive_use_rec SET file_out.modify(' \r\n";
                sql += "   insert " + xmlItem + "\r\n";
                sql += "   as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n";
                sql += "   SELECT 1 \r\n";
                sql += "END \r\n";
                sql += "ELSE \r\n";
                sql += "BEGIN \r\n";
                sql += "   IF(SELECT COUNT(*) FROM t_archive_use_rec WHERE file_out.exist('/Cart/items/item[@catalogid=\"" + catalogId + "\"]') = 1 AND file_out.exist('/Cart/items/item[@table=\"" + table + "\"]') = 1 AND Unique_code = " + requestId + ") = 0 \r\n";
                sql += "   BEGIN \r\n";
                sql += "     update t_archive_use_rec SET file_out.modify(' \r\n";
                sql += "     insert " + xmlItem + "\r\n";
                sql += "     as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n"; //最新添加的放在列表首位
                sql += "     SELECT 1 \r\n";
                sql += "   END \r\n";
                sql += "   ELSE \r\n";
                sql += "     SELECT -2 \r\n";//提示前端不能重复添加
                sql += "END \r\n";
                object robj = SqlHelper.ExecuteScalar(sql, null);
                result = int.Parse(robj.ToString());
            });
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public async Task<IActionResult> IntoShoppingCartBPs(string searchMode, string table, string requestId, string catalogId)//借档用
        {
            int result = 0;
            await Task.Run(() =>
            {
                string sql0 = "DECLARE @archNum nvarchar(MAX),@sql nvarchar(MAX) \r\n";
                sql0 += "SELECT @archNum=archive_num_field FROM t_config_archive_num_makeup WHERE code_name='" + table + "' \r\n";
                sql0 += "IF @archNum IS NOT NULL AND trim(@archNum) <> '' \r\n";
                sql0 += "BEGIN \r\n";
                sql0 += "   SET @sql = 'SELECT '+@archNum+' FROM " + table + " WHERE Unique_code = " + catalogId + "' \r\n";
                sql0 += "   EXEC(@sql) \r\n";//执行动态sql
                sql0 += "END \r\n";
                sql0 += "ELSE \r\n";
                sql0 += "BEGIN \r\n";
                sql0 += "   SELECT '' \r\n";
                sql0 += "END \r\n";
                object dhObj = SqlHelper.ExecuteScalar(sql0, null);//档号值

                string initXml = "<Cart><items mode=\"p\"></items><items mode=\"s\"></items><items mode=\"f\"></items></Cart>";
                //加id属性的目的，是避免item重复致使sql报错;beReturned标识是否已换，默认0为未还，1为已还，2位延期;prolongDays延期天数，默认0;记录上归还人userBack、归还份数sharesBack和备注commentBack
                string xmlItem = "<item id=\"" + DateTime.Now.ToString("yyyyMMddhhmmss") + "\" table=\"" + table + "\" catalogid=\"" + catalogId + "\" dh=\"" + dhObj.ToString() + "\" beReturned=\"0\" prolongDays=\"0\" userBack=\"\" sharesBack=\"\" commentBack=\"\"></item>";
                string sql = "IF (SELECT file_out FROM t_archive_use_rec WHERE Unique_code=" + requestId + ") IS NULL \r\n";
                sql += "BEGIN \r\n";
                sql += "   UPDATE  t_archive_use_rec SET file_out='" + initXml + "' WHERE Unique_code=" + requestId + "; \r\n";
                sql += "   update t_archive_use_rec SET file_out.modify(' \r\n";
                sql += "   insert " + xmlItem + "\r\n";
                sql += "   as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n";
                sql += "   SELECT 1 \r\n";
                sql += "END \r\n";
                sql += "ELSE \r\n";
                sql += "BEGIN \r\n";
                sql += "   IF(SELECT COUNT(*) FROM t_archive_use_rec WHERE file_out.exist('/Cart/items/item[@catalogid=\"" + catalogId + "\"]') = 1 AND file_out.exist('/Cart/items/item[@table=\"" + table + "\"]') = 1 AND Unique_code = " + requestId + ") = 0 \r\n";
                sql += "   BEGIN \r\n";
                sql += "     update t_archive_use_rec SET file_out.modify(' \r\n";
                sql += "     insert " + xmlItem + "\r\n";
                sql += "     as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n"; //最新添加的放在列表首位
                sql += "     SELECT 1 \r\n";
                sql += "   END \r\n";
                sql += "   ELSE \r\n";
                sql += "     SELECT -2 \r\n";//提示前端不能重复添加
                sql += "END \r\n";
                object robj = SqlHelper.ExecuteScalar(sql, null);
                result = int.Parse(robj.ToString());

            });
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult IntoShoppingCartF(string searchMode, string dh, string path, string catalogTitle, string requestId, string keyWords)//用于全文检索（查档）
        {
            string initXml = "<Cart><items mode=\"p\"></items><items mode=\"s\"></items><items mode=\"f\"></items></Cart>";
            string xmlItem = "<item id=\"" + DateTime.Now.ToString("yyyyMMddhhmmss") + "\" dh=\"" + dh + "\" path=\"" + path + "\" keyWords=\"" + keyWords + "\"></item>";//加id属性的目的，是避免item重复致使sql报错
            string sql = "IF (SELECT file_out FROM t_archive_use_rec WHERE Unique_code=" + requestId + ") IS NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "   UPDATE  t_archive_use_rec SET file_out='" + initXml + "' WHERE Unique_code=" + requestId + "; \r\n";
            sql += "   update t_archive_use_rec SET file_out.modify(' \r\n";
            sql += "   insert " + xmlItem + "\r\n";
            sql += "   as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n";
            sql += "   SELECT 1 \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "   IF(SELECT COUNT(*) FROM t_archive_use_rec WHERE file_out.exist('/Cart/items/item[@dh=\"" + dh + "\"]') = 1 AND file_out.exist('/Cart/items[@mode=\"f\"]') = 1 AND Unique_code = " + requestId + ") = 0 \r\n";
            sql += "   BEGIN \r\n";
            sql += "     update t_archive_use_rec SET file_out.modify(' \r\n";
            sql += "     insert " + xmlItem + "\r\n";
            sql += "     as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n"; //最新添加的放在列表首位
            sql += "     SELECT 1 \r\n";
            sql += "   END \r\n";
            sql += "   ELSE \r\n";
            sql += "     SELECT -2 \r\n";//提示前端不能重复添加
            sql += "END \r\n";
            object robj = SqlHelper.ExecuteScalar(sql, null);
            int result = int.Parse(robj.ToString());
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult IntoShoppingCartBF(string searchMode, string dh, string path, string catalogTitle, string requestId, string keyWords)//用于全文检索(借档)
        {
            string initXml = "<Cart><items mode=\"p\"></items><items mode=\"s\"></items><items mode=\"f\"></items></Cart>";
            //加id属性的目的，是避免item重复致使sql报错;beReturned标识是否已换，默认0为未还，1为已还，2位延期;prolongDays延期天数，默认0;记录上归还人userBack、归还份数sharesBack和备注commentBack
            string xmlItem = "<item id=\"" + DateTime.Now.ToString("yyyyMMddhhmmss") + "\" dh=\"" + dh + "\" beReturned=\"0\" prolongDays=\"0\" userBack=\"\" sharesBack=\"\" commentBack=\"\" path=\"" + path + "\" keyWords=\"" + keyWords + "\"></item>";
            string sql = "IF (SELECT file_out FROM t_archive_use_rec WHERE Unique_code=" + requestId + ") IS NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "   UPDATE  t_archive_use_rec SET file_out='" + initXml + "' WHERE Unique_code=" + requestId + "; \r\n";
            sql += "   update t_archive_use_rec SET file_out.modify(' \r\n";
            sql += "   insert " + xmlItem + "\r\n";
            sql += "   as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n";
            sql += "   SELECT 1 \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "   IF(SELECT COUNT(*) FROM t_archive_use_rec WHERE file_out.exist('/Cart/items/item[@dh=\"" + dh + "\"]') = 1 AND file_out.exist('/Cart/items[@mode=\"f\"]') = 1 AND Unique_code = " + requestId + ") = 0 \r\n";
            sql += "   BEGIN \r\n";
            sql += "     update t_archive_use_rec SET file_out.modify(' \r\n";
            sql += "     insert " + xmlItem + "\r\n";
            sql += "     as first into (/Cart/items[@mode=\"" + searchMode.ToLower().Trim() + "\"])[1]') WHERE Unique_code=" + requestId + " \r\n"; //最新添加的放在列表首位
            sql += "     SELECT 1 \r\n";
            sql += "   END \r\n";
            sql += "   ELSE \r\n";
            sql += "     SELECT -2 \r\n";//提示前端不能重复添加
            sql += "END \r\n";
            object robj = SqlHelper.ExecuteScalar(sql, null);
            int result = int.Parse(robj.ToString());
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult UsingTablesIncartByClerk(string userid, string requestId)
        {
            string sql = "SELECT DISTINCT(x.m.value('(@table)[1]','nvarchar(MAX)')) AS tb FROM t_archive_use_rec T \r\n";
            sql += "CROSS APPLY T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            sql += "WHERE (x.m.value('(../@mode)[1]','nvarchar(MAX)') ='p' \r\n";
            sql += "OR x.m.value('(../@mode)[1]','nvarchar(MAX)') ='s') AND be_over='0' \r\n";
            sql += "AND T.file_clerk='" + userid + "' \r\n";
            sql += "AND T.Unique_code=" + requestId;
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

        public IActionResult UsingTablesIncart(string requestId)
        {
            string sql = "SELECT DISTINCT(x.m.value('(@table)[1]','nvarchar(MAX)')) AS tb FROM t_archive_use_rec T \r\n";
            sql += "CROSS APPLY T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            sql += "WHERE (x.m.value('(../@mode)[1]','nvarchar(MAX)') ='p' \r\n";
            sql += "OR x.m.value('(../@mode)[1]','nvarchar(MAX)') ='s') \r\n";
            sql += "AND T.Unique_code=" + requestId;
            //sql += "AND T.file_clerk='" + userid + "'";
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

        public IActionResult GetCartsPS(string requestId, string tableName, string fieldsStr, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = tableName + " T,t_archive_use_rec T2 ";
            string fields = "T.Unique_code,T.yw,T.yw_xml,x.m.value('(@id)[1]','nvarchar(MAX)') recid,";
            fields += " x.m.value('(@beReturned)[1]','nvarchar(MAX)') beReturned,x.m.value('(@prolongDays)[1]','nvarchar(MAX)') prolongDays,";
            fields += " x.m.value('(@userBack)[1]','nvarchar(MAX)') userBack,x.m.value('(@sharesBack)[1]','nvarchar(MAX)') sharesBack,x.m.value('(@commentBack)[1]','nvarchar(MAX)') commentBack," + fieldsStr + " \r\n";
            string innerjoin = " CROSS APPLY T2.file_out.nodes('/Cart/items/item') x(m) \r\n";
            string where = " T.is_deleted <> '1' AND x.m.value('(@catalogid)[1]','nvarchar(MAX)') = T.Unique_code \r\n";
            where += " AND (x.m.value('(../@mode)[1]','nvarchar(MAX)') ='p' OR x.m.value('(../@mode)[1]','nvarchar(MAX)') ='s') \r\n";
            where += " AND T2.Unique_code=" + requestId;
            string sort = " T.Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            dt.Columns.Add("tableName");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["tableName"] = tableName;//每行数据中都记住tableName名称
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult GetCartsF(string requestId, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = " t_archive_use_rec T ";
            string fields = "x.m.value('(@dh)[1]','nvarchar(MAX)') AS dh,x.m.value('(@path)[1]','nvarchar(MAX)') AS path,x.m.value('(@keyWords)[1]','nvarchar(MAX)') AS keyWords,x.m.value('(@id)[1]','nvarchar(MAX)') recid, \r\n";
            fields += " x.m.value('(@beReturned)[1]','nvarchar(MAX)') beReturned,x.m.value('(@prolongDays)[1]','nvarchar(MAX)') prolongDays,";
            fields += " x.m.value('(@userBack)[1]','nvarchar(MAX)') userBack,x.m.value('(@sharesBack)[1]','nvarchar(MAX)') sharesBack,x.m.value('(@commentBack)[1]','nvarchar(MAX)') commentBack \r\n";
            string innerjoin = " CROSS APPLY T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            string where = " x.m.value('(../@mode)[1]','nvarchar(MAX)') ='f' \r\n";
            where += " AND T.Unique_code=" + requestId;
            string sort = "Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult DeleteCartItem(string id, string requestId)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT T.be_over FROM t_archive_use_rec T WHERE T.file_out.exist('/Cart/items/item[@id=\"" + id + "\"]') = 1 AND Unique_code=" + requestId;
            object overObj = SqlHelper.ExecuteScalar(sql, null);
            if (overObj.ToString() == "1")//若调档已经结束，则不能删除
            {
                return Json(new { rst = -2 });
            }

            sql = "UPDATE t_archive_use_rec SET file_out.modify('delete /Cart/items/item[@id=\"" + id + "\"]') WHERE Unique_code=" + requestId;
            int result = SqlHelper.ExecNonQuery(sql, null);

            return Json(new { rst = result });
        }

        public IActionResult FileOutOver(string id)
        {
            string sql = "UPDATE t_archive_use_rec SET be_over='1' WHERE Unique_code=" + id;
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult VerifyBeOver(string requestId)//是否整个借阅档过程结束
        {
            string sql = "SELECT be_over FROM t_archive_use_rec WHERE Unique_code=" + requestId;
            object result = SqlHelper.ExecuteScalar(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result.ToString() });
        }

        public IActionResult VerifyIfOver(string id)//若调档已经结束，则不能删除
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT T.be_over FROM t_archive_use_rec T WHERE T.file_out.exist('/Cart/items/item[@id=\"" + id + "\"]') = 1";
            object overObj = SqlHelper.ExecuteScalar(sql, null);
            if (overObj != DBNull.Value && overObj != null && overObj.ToString() == "1")
            {
                return Json(new { rst = -2 });
            }
            return Json(new { rst = 0 });
        }

        public IActionResult UpdateFileOutResult(string pieces, string pages, string comment, string rslt, string requestId)
        {
            string xml = "<fileOutResult><result pieces=\"" + pieces + "\" pages=\"" + pages + "\" comment=\"" + comment + "\" rslt=\"" + rslt + "\"></result></fileOutResult>";
            string sql = "UPDATE t_archive_use_rec SET file_result='" + xml + "' \r\n";
            sql += "WHERE Unique_code=" + requestId;
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult UpdateFileOutBResult(string pieces, string shares, string comment, string days, string requestId)
        {
            string xml = "<fileOutResult><result pieces=\"" + pieces + "\" shares=\"" + shares + "\" comment=\"" + comment + "\" days=\"" + days + "\"></result></fileOutResult>";
            string sql = "UPDATE t_archive_use_rec SET file_result='" + xml + "',be_over='5' \r\n";//5表示，档案已借出的状态。
            sql += "WHERE Unique_code=" + requestId;
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult fetchDhsById(string requestId)
        {
            string sql = "SELECT x.m.value('(@dh)[1]','nvarchar(MAX)') AS dh FROM t_archive_use_rec T cross apply T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            sql += "WHERE Unique_code=" + requestId;
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            sql = "SELECT T.vitalization_code,T.checkin_time,B.nick_name,T.aiming,T.file_content, \r\n";
            sql += "x.m.value('(@userName)[1]','nvarchar(MAX)') vitalizer,x.m.value('(@certifType)[1]','nvarchar(MAX)') certitype,x.m.value('(@certifNo)[1]','nvarchar(MAX)') certino, \r\n";
            sql += "x.m.value('(@workplace)[1]','nvarchar(MAX)') workplace,x.m.value('(@telephone)[1]','nvarchar(MAX)') telephone \r\n";
            sql += "FROM t_archive_use_rec T cross apply T.file_requester_info.nodes('/requestInfo/requester') x(m) \r\n";
            sql += "INNER JOIN t_user AS B ON T.file_clerk=B.user_name collate Chinese_PRC_CS_AS \r\n";//collate Chinese_PRC_CS_AS是=号两端的表在排序规则上保持一致，CS区分大小写，CI不区分
            sql += "WHERE T.Unique_code=" + requestId;
            DataTable requestInfo = SqlHelper.GetDataTable(sql, null);

            sql = "SELECT x.m.value('(@pieces)[1]','nvarchar(MAX)') pieces,x.m.value('(@pages)[1]','nvarchar(MAX)') pages,\r\n";
            sql += "x.m.value('(@comment)[1]','nvarchar(MAX)') comment, x.m.value('(@rslt)[1]','nvarchar(MAX)') rslt \r\n";
            sql += "FROM t_archive_use_rec T cross apply T.file_result.nodes('/fileOutResult/result') x(m) \r\n";
            sql += "WHERE T.Unique_code=" + requestId;
            DataTable resultInfo = SqlHelper.GetDataTable(sql, null);

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { dhs = dt, rqstInfo = requestInfo, rsltInfo = resultInfo });
        }

        public IActionResult fetchBrrwDhsById(string requestId)
        {
            string sql = "SELECT x.m.value('(@dh)[1]','nvarchar(MAX)') AS dh FROM t_archive_use_rec T cross apply T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            sql += "WHERE Unique_code=" + requestId;
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            sql = "SELECT T.vitalization_code,T.checkin_time,B.nick_name,T.aiming,T.file_content, \r\n";
            sql += "x.m.value('(@userName)[1]','nvarchar(MAX)') vitalizer,x.m.value('(@certifType)[1]','nvarchar(MAX)') certitype,x.m.value('(@certifNo)[1]','nvarchar(MAX)') certino, \r\n";
            sql += "x.m.value('(@workplace)[1]','nvarchar(MAX)') workplace,x.m.value('(@telephone)[1]','nvarchar(MAX)') telephone \r\n";
            sql += "FROM t_archive_use_rec T cross apply T.file_requester_info.nodes('/requestInfo/requester') x(m) \r\n";
            sql += "INNER JOIN t_user AS B ON T.file_clerk=B.user_name collate Chinese_PRC_CS_AS \r\n";//collate Chinese_PRC_CS_AS是=号两端的表在排序规则上保持一致，CS区分大小写，CI不区分
            sql += "WHERE T.Unique_code=" + requestId;
            DataTable requestInfo = SqlHelper.GetDataTable(sql, null);

            sql = "SELECT x.m.value('(@pieces)[1]','nvarchar(MAX)') pieces,x.m.value('(@shares)[1]','nvarchar(MAX)') shares,\r\n";
            sql += "x.m.value('(@comment)[1]','nvarchar(MAX)') comment, x.m.value('(@days)[1]','nvarchar(MAX)') days \r\n";
            sql += "FROM t_archive_use_rec T cross apply T.file_result.nodes('/fileOutResult/result') x(m) \r\n";
            sql += "WHERE T.Unique_code=" + requestId;
            DataTable resultInfo = SqlHelper.GetDataTable(sql, null);

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { dhs = dt, rqstInfo = requestInfo, rsltInfo = resultInfo });
        }

        public IActionResult VerifyIfAuditPassed(string requestId)
        {
            string sql = "SELECT be_over FROM t_archive_use_rec WHERE Unique_code=" + requestId;
            object obj = SqlHelper.ExecuteScalar(sql, null);
            int result = 0;
            if (obj.ToString().Equals("3") || obj.ToString().Equals("5") || obj.ToString().Equals("6") || obj.ToString().Equals("7"))//只有审批通过的、或档案借出中的、或借出已归还的或延期的，才能打印借档单
            {
                result = 1;
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult ShowOutInfo(string requestId)//归还档案时调用的简单借档信息
        {
            string sql = "IF(SELECT count(k.t.value('(@shares)[1]','nvarchar(MAX)')) FROM t_archive_use_rec T0 cross apply T0.file_result.nodes('/fileOutResult/result') k(t) WHERE T0.Unique_code=" + requestId + ") > 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "    SELECT T.checkin_time outtime, \r\n";
            sql += "    x.m.value('(@userName)[1]','nvarchar(MAX)') vitalizer, \r\n";
            sql += "    y.n.value('(@pieces)[1]','nvarchar(MAX)') pieces,y.n.value('(@shares)[1]','nvarchar(MAX)') shares \r\n";
            sql += "    FROM t_archive_use_rec T cross apply T.file_requester_info.nodes('/requestInfo/requester') x(m) \r\n";
            sql += "    cross apply T.file_result.nodes('/fileOutResult/result') y(n) \r\n";
            //sql += "    INNER JOIN t_user AS B ON T.file_clerk=B.user_name collate Chinese_PRC_CS_AS \r\n";//collate Chinese_PRC_CS_AS是=号两端的表在排序规则上保持一致，CS区分大小写，CI不区分
            sql += "    WHERE T.Unique_code=" + requestId + " \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "    SELECT T.checkin_time outtime, \r\n";
            sql += "    x.m.value('(@userName)[1]','nvarchar(MAX)') vitalizer, '0' AS pieces,'0' AS shares \r\n";
            //sql += "    y.n.value('(@pieces)[1]','nvarchar(MAX)') pieces,y.n.value('(@shares)[1]','nvarchar(MAX)') shares \r\n";
            sql += "    FROM t_archive_use_rec T cross apply T.file_requester_info.nodes('/requestInfo/requester') x(m) \r\n";
            //sql += "    cross apply T.file_result.nodes('/fileOutResult/result') y(n) \r\n";
            //sql += "    INNER JOIN t_user AS B ON T.file_clerk=B.user_name collate Chinese_PRC_CS_AS \r\n";//collate Chinese_PRC_CS_AS是=号两端的表在排序规则上保持一致，CS区分大小写，CI不区分
            sql += "    WHERE T.Unique_code=" + requestId + " \r\n";
            sql += "END \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult RestoreArch(string requestId, string itemId, string userBack, string sharesBack, string commentBack)//还档
        {
            string attch0 = "";
            attch0 += " FROM t_archive_use_rec T \r\n";
            attch0 += " cross apply T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            attch0 += "WHERE Unique_code = " + requestId + " \r\n";

            string attch2 = attch0 + " AND x.m.value('(@beReturned)[1]','nvarchar(MAX)') = '1'";

            string sql = "UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@beReturned)[1] with \"1\" ') \r\n" + attch0;//beReturned设置为1已还
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@userBack)[1] with \"" + userBack + "\" ') \r\n" + attch0;
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@sharesBack)[1] with \"" + sharesBack + "\" ') \r\n" + attch0;
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@commentBack)[1] with \"" + commentBack + "\" ') \r\n" + attch0;
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@prolongDays)[1] with \"0\" ') \r\n" + attch0;//归还了，就把延期清零
            int result = SqlHelper.ExecNonQuery(sql, null);
            sql = " DECLARE @cnt1 int,@cnt2 int \r\n";
            sql += " SELECT @cnt1 = (SELECT COUNT(x.m.value('(@beReturned)[1]','nvarchar(MAX)')) " + attch0 + ") \r\n";
            sql += " SELECT @cnt2 = (SELECT COUNT(x.m.value('(@beReturned)[1]','nvarchar(MAX)')) " + attch2 + ") \r\n";
            sql += " IF(@cnt1 = @cnt2) \r\n";
            sql += " BEGIN \r\n";
            sql += "    UPDATE T SET be_over='6' " + attch0;//6,表示借出的档案已全部归还
            sql += " END";
            SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult ProlongArch(string requestId, string itemId, string days, string userBack, string sharesBack, string commentBack)//延期
        {
            string attch = "";
            attch += " FROM t_archive_use_rec T \r\n";
            attch += " cross apply T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            attch += "WHERE Unique_code = " + requestId + " \r\n";

            string sql = "UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@beReturned)[1] with \"2\" ') \r\n" + attch;//beReturned设置为2延期
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@prolongDays)[1] with \"" + days + "\" ') \r\n" + attch;
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@userBack)[1] with \"" + userBack + "\" ') \r\n" + attch;
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@sharesBack)[1] with \"" + sharesBack + "\" ') \r\n" + attch;
            sql += " UPDATE T SET file_out.modify('replace value of (/Cart/items/item[@id=\"" + itemId + "\"]/@commentBack)[1] with \"" + commentBack + "\" ') \r\n" + attch;
            sql += " UPDATE T SET be_over='7' \r\n" + attch;//7,表示借出的档案中有延期的
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult GetExpiredBrrws(int pageSize, int pageIndex)
        {//所有到期的借档
            //string sql = "select vitalization_code,file_clerk,checkin_time,CONVERT(char(10),dateadd(dd,CONVERT(int,x.m.value('(@days)[1]','nvarchar(MAX)')),checkin_time),120) as over_time \r\n";
            //sql += " FROM t_archive_use_rec T \r\n";
            //sql += " cross apply T.file_result.nodes('/fileOutResult/result') x(m) \r\n";
            //sql += " WHERE copy_borrow='1' and T.file_result is not null AND dateadd(dd,CONVERT(int,x.m.value('(@days)[1]','nvarchar(MAX)')),checkin_time) <= GETDATE()";
            //DataTable dt = SqlHelper.GetDataTable(sql, null);
            //return Json(dt);

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = " t_archive_use_rec T ";
            string fields = "vitalization_code,file_clerk,checkin_time,CONVERT(char(10),dateadd(dd,CONVERT(int,x.m.value('(@days)[1]','nvarchar(MAX)')),checkin_time),120) as over_time \r\n";
            string innerjoin = " cross apply T.file_result.nodes('/fileOutResult/result') x(m) \r\n";
            string where = " copy_borrow='1' and T.file_result is not null AND dateadd(dd,CONVERT(int,x.m.value('(@days)[1]','nvarchar(MAX)')),checkin_time) <= GETDATE() \r\n";//涉及日期转换和比较
            string sort = " DATEDIFF(day,DATEADD(dd,CONVERT(int,x.m.value('(@days)[1]','nvarchar(MAX)')),checkin_time),GETDATE()) ASC";//过期时间越长的，优先显示列前头
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult GetAllAudits(int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName2 = "t_archuse_auditing TB";
            string fields2 = " TB.application_info,TB.application_time,TB.end_time,TB.check_status,T2.nick_name applier,T3.nick_name checker, \r\n";
            fields2 += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            string innerjoin2 = " cross apply TB.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            innerjoin2 += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            innerjoin2 += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            string where2 = " 1=1 ";
            string sort = " application_time DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName2, innerjoin2, fields2, where2, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult GetSearchRst(string fromdate, string toDate, string requester, string clerk, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = " t_archive_use_rec T ";
            string fields = " T.Unique_code,T.vitalization_code,T.checkin_time,T.be_over,T.copy_borrow, \r\n";
            fields += " x.m.value('(@userName)[1]','nvarchar(MAX)') file_requester ";
            string innerjoin = " CROSS APPLY T.file_requester_info.nodes('/requestInfo/requester') x(m) \r\n";
            innerjoin += " INNER JOIN t_user U ON T.file_clerk=U.user_name collate Chinese_PRC_CS_AS \r\n";
            string sort = " T.Unique_code DESC ";
            int pageCount = 0;
            int recordCount = 0;

            string where = " 1=1 ";
            if ((fromdate == null || fromdate.Trim() == "") && toDate != null && toDate.ToLower() != "")
            {
                where += " AND checkin_time='" + toDate + "' ";
            }
            if ((toDate == null || toDate.Trim() == "") && fromdate != null && fromdate.ToLower() != "")
            {
                where += " AND checkin_time='" + fromdate + "' ";
            }
            if (toDate != null && toDate.Trim() != "" && fromdate != null && fromdate.ToLower() != "")
            {
                where += " AND checkin_time BETWEEN '" + fromdate + "' AND '" + toDate + "' ";
            }
            if (requester != null && requester.Trim() != "")
            {
                where += " AND CHARINDEX('" + requester + "',x.m.value('(@userName)[1]','nvarchar(MAX)')) > 0 ";
            }
            if (clerk != null && clerk.Trim() != "")
            {
                where += " AND CHARINDEX('" + clerk + "',U.nick_name) > 0 ";
            }

            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult CountGroupByYearNType()
        {
            string sql = "SELECT YEAR(checkin_time) yr,CASE copy_borrow WHEN '1' THEN '借档' WHEN '0' THEN '查阅' END cb,COUNT(Unique_code) cnt FROM t_archive_use_rec \r\n";
            sql += "GROUP BY YEAR(checkin_time),copy_borrow ORDER BY YEAR(checkin_time) ASC \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            ChartData cd = new ChartData();
            cd.Legends = new List<string>();
            cd.Years = new List<string>();
            cd.Series = new List<List<int>>();

            int[] yearArr = dt.AsEnumerable().Select(d => d.Field<int>("yr")).ToArray();//把DataTable某列数据转化为数组
            yearArr = yearArr.Distinct().ToArray();//去重
            cd.Years = new List<string>(yearArr.ToList().ConvertAll<string>(x => x.ToString()));//List<int>转List<string>

            string[] legenArr = dt.AsEnumerable().Select(d => d.Field<string>("cb")).ToArray();//把DataTable某列数据转化为数组
            legenArr = legenArr.Distinct().ToArray();//去重
            cd.Legends = new List<string>(legenArr);//数组转List

            for (int i = 0; i < cd.Legends.Count; i++)//先以legend进行分类
            {
                List<int> cntInt = new List<int>();
                for (int j = 0; j < cd.Years.Count; j++)//再以年度为单位形成数组，每个legend对应一个数组，数组中元素的数量为年度的总数
                {
                    List<int> cntArr = dt.Select("yr='" + cd.Years[j] + "' and cb='" + cd.Legends[i] + "'").AsEnumerable().Select(d => d.Field<int>("cnt")).ToList<int>();
                    cntInt.Add(cntArr[0]);
                }
                cd.Series.Add(cntInt);
            }

            dt.Dispose();
            return Json(cd);
        }

        public IActionResult CountGroupByAiming()
        {
            string sql = "SELECT YEAR(checkin_time) yr,aiming,COUNT(Unique_code) cnt FROM t_archive_use_rec \r\n";
            sql += "GROUP BY YEAR(checkin_time),aiming ORDER BY YEAR(checkin_time) ASC \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            ChartData cd = new ChartData();
            cd.Legends = new List<string>();
            cd.Years = new List<string>();
            cd.Series = new List<List<int>>();

            int[] yearArr = dt.AsEnumerable().Select(d => d.Field<int>("yr")).ToArray();//把DataTable某列数据转化为数组
            yearArr = yearArr.Distinct().ToArray();//去重
            cd.Years = new List<string>(yearArr.ToList().ConvertAll<string>(x => x.ToString()));//List<int>转List<string>

            string[] legenArr = dt.AsEnumerable().Select(d => d.Field<string>("aiming")).ToArray();//把DataTable某列数据转化为数组
            legenArr = legenArr.Distinct().ToArray();//去重
            cd.Legends = new List<string>(legenArr);//数组转List

            for (int i = 0; i < cd.Legends.Count; i++)//先以legend进行分类
            {
                List<int> cntInt = new List<int>();
                for (int j = 0; j < cd.Years.Count; j++)//再以年度为单位形成数组，每个legend对应一个数组，数组中元素的数量为年度的总数
                {
                    List<int> cntArr = dt.Select("yr='" + cd.Years[j] + "' and aiming='" + cd.Legends[i] + "'").AsEnumerable().Select(d => d.Field<int>("cnt")).ToList<int>();
                    cntInt.Add(cntArr[0]);
                }
                cd.Series.Add(cntInt);
            }

            dt.Dispose();
            return Json(cd);
        }
    }
}
