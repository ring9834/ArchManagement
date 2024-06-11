using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WBorrowAuditingController : WBaseController
    {
        public IActionResult ApplicationView(string other, string userid)
        {
            ViewData["userid"] = userid;
            ViewData["requestId"] = other;
            return View("Epplication");
        }

        public IActionResult SendAuditingInfo(string requestId)
        {
            int result = 0;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT x.m.value('(@dh)[1]','nvarchar(MAX)') AS dh FROM t_archive_use_rec T cross apply T.file_out.nodes('/Cart/items/item') x(m) \r\n";
            sql += "WHERE Unique_code=" + requestId;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                result = 1;//如果result==0，则告诉用户：提交借档审批前，请先进行调档
            }

            sql = "SELECT COUNT(Unique_code) AS cnt FROM t_archuse_auditing WHERE request_id=" + requestId;
            object cntObj = SqlHelper.ExecuteScalar(sql, null);
            int cnt = int.Parse(cntObj.ToString());//如果cnt>0，则告诉用户：已提交过借档审批的申请，不能重复提交

            return Json(new { rst = result, cnt = cnt });//如果还没有开始借档的调档，前端提示须先调档
        }

        public IActionResult RecordAuditInfo(string requestId, string applytitle, string applier, string checker, string comment)
        {
            //申请、审核节点信息
            XmlDocument link_doc = new XmlDocument();
            XmlElement link_root = link_doc.CreateElement("archiveAudit");
            XmlElement link_elemt = link_doc.CreateElement("auditNode");
            link_elemt.SetAttribute("prevPerson", "-1");//-1表示前节点
            link_elemt.SetAttribute("selfPlace", applier);//下一节点
            link_elemt.SetAttribute("afterPerson", checker);//下一节点
            link_elemt.SetAttribute("checkResult", "");//本节点审核结果
            link_elemt.SetAttribute("checkTime", "");
            link_elemt.SetAttribute("checkStatus", "");//审核状态，如未审核，正在审核，审核通过，审核未通过
            link_elemt.SetAttribute("isActive", "false");//激活状态，false表示本节点还未通过审核，不能进行下一审核节点；true表示可以进入下一节点进行审核
            link_elemt.SetAttribute("selfComment", comment);//申请人备注
            link_elemt.SetAttribute("checkerComment", "");//节点审核人备注
            link_root.AppendChild(link_elemt);
            link_doc.AppendChild(link_root);

            string sql = "UPDATE t_archive_use_rec SET be_over='2' WHERE Unique_code=" + requestId + " \r\n";//借档状态更新为审批中
            sql += "SELECT file_out FROM t_archive_use_rec WHERE Unique_code=" + requestId;
            object fileOut = SqlHelper.ExecuteScalar(sql, null);

            sql = "INSERT INTO t_archuse_auditing(request_id,application_info,auditing_link,application_time,end_time,archive_dealt) \r\n";
            sql += "VALUES(@request_id,@application_info,@auditing_link,@application_time,@end_time,@archive_dealt)";
            SqlParameter para1 = SqlHelper.MakeInParam("request_id", requestId);//作为外键，与档案借阅表archive_use_rec相关联
            SqlParameter para2 = SqlHelper.MakeInParam("application_info", applytitle);
            SqlParameter para3 = SqlHelper.MakeInParam("auditing_link", link_doc.OuterXml);
            SqlParameter para4 = SqlHelper.MakeInParam("application_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para5 = SqlHelper.MakeInParam("end_time", "");
            //把用户调档的信息放到archive_dealt字段，目的是为了之后与t_archive_use_rec的file_out进行对比，是否用户在审核通过后，又调了其它档案（这属于钻漏洞）
            SqlParameter para6 = SqlHelper.MakeInParam("archive_dealt", fileOut == DBNull.Value ? "" : fileOut.ToString());
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6 };
            int result = SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult GetAuditingsByChecker(string checkerName)
        {
            //使用了SQL SERVER的XML的query方法，即：where 字段.exist(xpath)=1
            string sql = "SELECT Unique_code,application_info,application_time \r\n";
            sql += "FROM dbo.t_archuse_auditing WHERE check_status='1' AND auditing_link.exist('archiveAudit/auditNode[@afterPerson=\"" + checkerName + "\"]')=1";//1表示审核中，2表示已审核，数据库中默认为1
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult ShowToCheckWin(string id)
        {
            ViewData["id"] = id;
            return View("CheckingIn");
        }

        public IActionResult GetApplierInfo(string uniquecode)
        {
            string sql = "SELECT T1.application_info,T1.auditing_link.value('(archiveAudit/auditNode/@selfPlace)[1]','nvarchar(MAX)') AS applier,T2.nick_name,\r\n";
            sql += "T1.auditing_link.value('(archiveAudit/auditNode/@selfComment)[1]','nvarchar(MAX)') AS comment \r\n";
            sql += "FROM t_archuse_auditing AS T1 CROSS APPLY T1.auditing_link.nodes('archiveAudit/auditNode') x(m) \r\n";
            sql += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') WHERE T1.Unique_code=" + uniquecode;//使用了一个表的XML字段中节点属性与另外一个表的字段进行的JINNER JOIN （2020年3月8日，worth remembering）
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        //修改xml某节点属性值，一条一条地修改
        public IActionResult UpdateAuditResult(string uniquecode, string checkResult, string checkComment)
        {
            string status = "1";//1表示所选记录的审核状态在审核中，2表示审核通过，3表示未通过
            string useSatus = "3";//3表示借档审批通过，4表示借档审批未通过
            if (checkResult.Equals("yes"))
            {
                useSatus = "3";
                status = "2";
            }
            else
            {
                status = "3";
                useSatus = "4";
            }

            string sql = "UPDATE T SET auditing_link.modify('replace value of (archiveAudit/auditNode/@checkResult)[1] with \"" + checkResult + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE T SET auditing_link.modify('replace value of (archiveAudit/auditNode/@checkerComment)[1] with \"" + checkComment + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE T SET auditing_link.modify('replace value of (archiveAudit/auditNode/@checkTime)[1] with \"" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE t_archuse_auditing SET check_status='" + status + "' WHERE Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE t_archive_use_rec SET be_over='" + useSatus + "' WHERE Unique_code=(SELECT request_id FROM t_archuse_auditing WHERE Unique_code=" + uniquecode + ")";//20200330 modified
            int result = SqlHelper.ExecNonQuery(sql, null);
            return Json(result);
        }

        public IActionResult GetDoneAuditsByChecker(string checkerName)
        {
            //使用了SQL SERVER的XML的query方法，即：where 字段.exist(xpath)=1
            string sql = "SELECT Unique_code,application_info,application_time \r\n";
            sql += "FROM dbo.t_archuse_auditing WHERE check_status='2' AND auditing_link.exist('archiveAudit/auditNode[@afterPerson=\"" + checkerName + "\"]')=1";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetAuditAllRecs(string userid)
        {
            ViewData["userid"] = userid;
            return View("AuditsAll");
        }

        public IActionResult GetAllAudits(string userid, int pageSize, int pageIndex)
        {
            string codeName1 = "t_auditing T1";
            //string fields1 = " T1.application_info,T1.application_time,T1.end_time,T1.check_status,T2.nick_name applier,T3.nick_name checker, \r\n";
            //fields1 += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            //string innerjoin1 = " cross apply T1.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            //innerjoin1 += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            //innerjoin1 += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            //string where1 = " x.m.value('(@afterPerson)[1]','nvarchar(MAX)')='" + userid + "'";

            string codeName2 = "t_archuse_auditing TB";
            //string fields2 = " TB.application_info,TB.application_time,TB.end_time,TB.check_status,T2.nick_name applier,T3.nick_name checker, \r\n";
            //fields2 += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            //string innerjoin2 = " cross apply TB.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            //innerjoin2 += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            //innerjoin2 += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            //string where2 = " x.m.value('(@afterPerson)[1]','nvarchar(MAX)')='" + userid + "'";

            List<string> tables = new List<string>();
            tables.Add("t_auditing T1");
            tables.Add("t_archuse_auditing TB");
            tables.Add("t_archuse_auditing TC");

            List<string> fields = new List<string>();
            string fields1 = " T1.application_info,T1.application_time,T1.end_time,T1.check_status,T2.nick_name applier,T3.nick_name checker, \r\n";
            fields1 += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            fields.Add(fields1);
            string fields2 = " TB.application_info,TB.application_time,TB.end_time,TB.check_status,T2.nick_name applier,T3.nick_name checker, \r\n";
            fields2 += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            fields.Add(fields2);
            string fields3 = " TC.application_info,TC.application_time,TC.end_time,y.n.value('(@presentStatus)[1]','nvarchar(MAX)') check_status,T2.nick_name applier,T3.nick_name checker, \r\n";
            fields3 += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time \r\n";
            fields.Add(fields3);

            List<string> joins = new List<string>();
            string innerjoin1 = " cross apply T1.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            innerjoin1 += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            innerjoin1 += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            joins.Add(innerjoin1);
            string innerjoin2 = " cross apply TB.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            innerjoin2 += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            innerjoin2 += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            joins.Add(innerjoin2);

            string innerjoin3 = " cross apply TC.auditing_link.nodes('useRcdAudit/auditNode') AS x(m) \r\n";
            innerjoin3 += " cross apply TC.auditing_link.nodes('/useRcdAudit/auditStatus') y(n) \r\n";
            innerjoin3 += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            innerjoin3 += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            joins.Add(innerjoin3);

            List<string> wheres = new List<string>();
            string where1 = " x.m.value('(@afterPerson)[1]','nvarchar(MAX)')='" + userid + "'";
            string where2 = " x.m.value('(@afterPerson)[1]','nvarchar(MAX)')='" + userid + "'";
            string where3 = " x.m.value('(@afterPerson)[1]','nvarchar(MAX)')='" + userid + "'";
            wheres.Add(where1);
            wheres.Add(where2);
            wheres.Add(where3);

            string sort = " application_time DESC";
            int pageCount = 0;
            int recordCount = 0;
            //DataTable dt = PagerUtils.GetUnionPagedDataTable(codeName1, codeName2, innerjoin1, innerjoin2, fields1, fields2, where1, where2, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            DataTable dt = PagerUtils.GetUnionListPagedDataTable(tables, joins, fields, wheres, sort, pageIndex, pageSize, ref pageCount, ref recordCount);

            return Json(new { total = recordCount, rows = dt });
        }
    }
}
