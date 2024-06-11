using ArchiveFileManagementNs.Models;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System.Data;

namespace ArchiveFileManagementNs.Controllers
{
    public class WOperateRecsController : WBaseController
    {
        public IActionResult Index(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View();
        }

        public IActionResult SearchCondtions()
        {
            return View("SearchCondtions");
        }

        public IActionResult PickTablev()
        {
            return View("PickTable");
        }

        //public IActionResult fetchOprtionRecs(int pageSize, int pageIndex)
        //{
        //    //string sql = "SELECT operate_info,operaterr,operate_time,Unique_code FROM t_operate_rec";
        //    //DataTable dt = SqlHelper.GetDataTable(sql, null);

        //    //JsonSerializerSettings setting = new JsonSerializerSettings();
        //    string codeName = " t_operate_rec A ";
        //    string fields = " operate_info,operaterr,operate_time,Unique_code,x.m.value('(@table)[1]','nvarchar(MAX)') tb ";
        //    string innerjoin = " cross apply A.records_influenced.nodes('/influData/influTable') x(m) ";
        //    string where = " 1=1 ";
        //    string sort = "Unique_code DESC";
        //    int pageCount = 0;
        //    int recordCount = 0;
        //    DataTable table = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
        //    return Json(new { total = recordCount, rows = table });
        //}

        public IActionResult fetchOprtionRecs(OperationInfo opInfo, int pageSize, int pageIndex)
        {
            string where = " 1=1 ";
            if (!string.IsNullOrEmpty(opInfo.TableName))
                where += " AND x.m.value('(@table)[1]','nvarchar(Max)')= \'" + opInfo.TableName + "\' ";
            if (!string.IsNullOrEmpty(opInfo.FuncModal))
                where += " AND y.n.value('(@funcModal)[1]','nvarchar(Max)') LIKE \'%" + opInfo.FuncModal + "%\' ";
            if (!string.IsNullOrEmpty(opInfo.FuncName))
                where += " AND y.n.value('(@funcName)[1]','nvarchar(Max)') LIKE \'%" + opInfo.FuncName + "%\' ";
            if (!string.IsNullOrEmpty(opInfo.Department))
                where += " AND y.n.value('(@depart)[1]','nvarchar(Max)') LIKE \'%" + opInfo.Department + "%\' ";
            if (!string.IsNullOrEmpty(opInfo.SourceIP))
                where += " AND y.n.value('(@ip)[1]','nvarchar(Max)') LIKE \'%" + opInfo.SourceIP + "%\' ";
            if (!string.IsNullOrEmpty(opInfo.OperTag))
                where += " AND T.operate_info LIKE \'%" + opInfo.OperTag + "%\' ";
            if (!string.IsNullOrEmpty(opInfo.OperTime))
                where += " AND T.operate_time LIKE \'%" + opInfo.OperTime + "%\' ";
            if (!string.IsNullOrEmpty(opInfo.UserName))
                where += " AND (T.operaterr LIKE \'%" + opInfo.UserName + "%\' OR T.operaterr LIKE \'%" + opInfo.UserName + "%\') ";

            string codeName = " t_operate_rec T ";
            string fields = " operate_info,operaterr,operate_time,Unique_code,x.m.value('(@table)[1]','nvarchar(MAX)') tb, \r\n";
            fields += "y.n.value('(@archType)[1]','nvarchar(MAX)') archtype, y.n.value('(@depart)[1]','nvarchar(MAX)') depart, \r\n";
            fields += "y.n.value('(@funcName)[1]','nvarchar(MAX)') funcname, y.n.value('(@funcModal)[1]','nvarchar(MAX)') funcmodal, \r\n";
            fields += "y.n.value('(@ip)[1]','nvarchar(MAX)') ip \r\n";
            string innerjoin = " cross apply T.records_influenced.nodes('/influData/influTable') x(m) ";
            innerjoin += " outer apply T.base_info.nodes('/operInfo/info') y(n) ";//结果集中将包含使右表表达式为空的左表表达式中的行
            string sort = "T.Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult oprtionRecDetailView(string id)
        {
            ViewData["id"] = id;
            return View("OperationRecDetail");
        }

        public IActionResult oprtionRecDetail(string uniquecode, int pageSize, int pageIndex)
        {
            //string sql = "select x.m.value('(@table)[1]','nvarchar(MAX)') tb, \r\n";
            //sql += "y.n.value('text()[1]','nvarchar(MAX)') id \r\n";
            //sql += "from t_operate_rec A  \r\n";
            //sql += "cross apply A.records_influenced.nodes('/influData/influTable') x(m) \r\n";
            //sql += "cross apply A.records_influenced.nodes('/influData/influTable/id') y(n) \r\n";
            //sql += "where Unique_code=" + uniquecode;

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = " t_operate_rec A ";
            string fields = " x.m.value('(@table)[1]','nvarchar(MAX)') tb, y.n.value('text()[1]','nvarchar(MAX)') id ";
            string innerjoin = " cross apply A.records_influenced.nodes('/influData/influTable') x(m) ";
            innerjoin += " cross apply A.records_influenced.nodes('/influData/influTable/id') y(n) ";
            string where = " 1=1 AND A.Unique_code=" + uniquecode;
            string sort = " A.Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult GetArchTypesWithYw()
        {
            string sql = "SELECT Unique_code,code,name FROM t_config_type_tree WHERE has_content=1";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }
    }
}
