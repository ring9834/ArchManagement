using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WUseRcdAuditingController : WBaseController
    {
        public IActionResult ApplicationView(string other, string userid)
        {
            ViewData["userid"] = userid;
            ViewData["requestId"] = other;
            return View("Epplication");
        }

        public IActionResult ShowToCheckWin(string id)
        {
            ViewData["id"] = id;
            return View("CheckingIn");
        }

        public IActionResult AllUseRcdAuditsView(string id, string userid)
        {
            ViewData["userid"] = userid;
            return View("AuditsAll");
        }

        public IActionResult RecordAuditInfo(string requestId, string applytitle, string applier, string checker, string comment, string expireDate)
        {
            //申请、审核节点信息
            XmlDocument link_doc = new XmlDocument();
            XmlElement link_root = link_doc.CreateElement("useRcdAudit");
            XmlElement link_elemt = link_doc.CreateElement("auditNode");
            link_elemt.SetAttribute("prevPerson", "-1");//-1表示前节点
            link_elemt.SetAttribute("selfPlace", applier);//本身节点
            link_elemt.SetAttribute("afterPerson", checker);//下一节点
            link_elemt.SetAttribute("checkResult", "");//本节点审核结果
            link_elemt.SetAttribute("checkTime", "");
            link_elemt.SetAttribute("checkStatus", "");//审核状态，如未审核，正在审核，审核通过，审核未通过
            link_elemt.SetAttribute("isActive", "false");//激活状态，false表示本节点还未通过审核，不能进行下一审核节点；true表示可以进入下一节点进行审核
            link_elemt.SetAttribute("selfComment", comment);//申请人备注
            link_elemt.SetAttribute("checkerComment", "");//节点审核人备注
            link_root.AppendChild(link_elemt);

            XmlElement stats_elemt = link_doc.CreateElement("auditStatus");//审批状态节点
            stats_elemt.SetAttribute("presentStatus", "1");//1表示已申请待审批状态，2表示审批通过，3表示审批未通过
            link_root.AppendChild(stats_elemt);

            XmlElement origin_applier = link_doc.CreateElement("originApplier");
            origin_applier.SetAttribute("applier", applier);//记录原始申请人
            link_root.AppendChild(origin_applier);

            XmlElement expireDate_ele = link_doc.CreateElement("expireDate");
            expireDate_ele.SetAttribute("date", expireDate);//允许非借阅登记人查阅别人登记的借阅详情的过时日期
            link_root.AppendChild(expireDate_ele);

            link_doc.AppendChild(link_root);

            string sql = "INSERT INTO t_archuse_auditing(request_id,application_info,auditing_link,application_time,end_time) \r\n";
            sql += "VALUES(@request_id,@application_info,@auditing_link,@application_time,@end_time)";
            SqlParameter para1 = SqlHelper.MakeInParam("request_id", requestId);//作为外键，与档案借阅表archive_use_rec相关联
            SqlParameter para2 = SqlHelper.MakeInParam("application_info", applytitle);
            SqlParameter para3 = SqlHelper.MakeInParam("auditing_link", link_doc.OuterXml);
            SqlParameter para4 = SqlHelper.MakeInParam("application_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para5 = SqlHelper.MakeInParam("end_time", "");
            //把用户调档的信息放到archive_dealt字段，目的是为了之后与t_archive_use_rec的file_out进行对比，是否用户在审核通过后，又调了其它档案（这属于钻漏洞）
            //SqlParameter para6 = SqlHelper.MakeInParam("archive_dealt", fileOut == DBNull.Value ? "" : fileOut.ToString());
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5 };
            int result = SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            return Json(new { rst = result });
        }

        /// <summary>
        /// 20201007 mark
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="clerk"></param>
        /// <returns></returns>
        public IActionResult VerifyDifferentStatus(string requestId, string clerk)
        {
            int result = 0;
            string sql = "SELECT COUNT(Unique_code) AS cnt FROM t_archive_use_rec ";
            sql += "WHERE file_clerk='" + clerk + "' AND  Unique_code=" + requestId;
            object cntObj = SqlHelper.ExecuteScalar(sql, null);
            int cnt = int.Parse(cntObj.ToString());//如果cnt>0，表示登录人可以查看自己办理的借阅单详情
            if (cnt > 0)
            {
                result = 1;
                return Json(new { rst = result });
            }

            //看是否具有查看所有人登记的借阅单权限的人
            sql = "SELECT COUNT(T.Unique_code) FROM t_config_func_point T \r\n";
            sql += "cross apply T.roles.nodes('/AsignedRoles/Role') x(m) \r\n";
            sql += "INNER JOIN t_config_role K ON x.m.value('(@roleid)[1]','nvarchar(MAX)')=K.Unique_code \r\n";
            sql += "INNER JOIN t_user M ON M.role_id=K.Unique_code \r\n";
            sql += "WHERE T.func_symble='use_view' AND M.user_name='"+ clerk + "'";
            cntObj = SqlHelper.ExecuteScalar(sql, null);
            cnt = int.Parse(cntObj.ToString());
            if (cnt > 0)//具有，则可以查看所有借阅单
            {
                result = 1;
                return Json(new { rst = result });
            }

            sql = "SELECT COUNT(Unique_code) AS cnt FROM t_archuse_auditing T \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/originApplier') x(m) \r\n";
            sql += "WHERE x.m.value('(@applier)[1]','nvarchar(MAX)')='" + clerk + "' AND T.request_id=" + requestId;
            cntObj = SqlHelper.ExecuteScalar(sql, null);
            cnt = int.Parse(cntObj.ToString());
            if (cnt == 0)//如果没有申请审批记录，则提示申请审批
            {
                result = 2;
                return Json(new { rst = result });
            }

            sql = "SELECT COUNT(Unique_code) AS cnt FROM t_archuse_auditing T \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/auditStatus') x(m) \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/originApplier') y(n) \r\n";
            sql += "WHERE x.m.value('(@presentStatus)[1]','nvarchar(MAX)')='1' \r\n";//待审批
            sql += "AND y.n.value('(@applier)[1]','nvarchar(MAX)')='" + clerk + "' AND T.request_id=" + requestId;
            cntObj = SqlHelper.ExecuteScalar(sql, null);
            cnt = int.Parse(cntObj.ToString());
            if (cnt > 0)//如果有待审批的记录，则提示正在审批，暂不能打开借阅单
            {
                result = 4;
                return Json(new { rst = result });
            }

            sql = "SELECT COUNT(Unique_code) AS cnt FROM t_archuse_auditing T \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/auditStatus') x(m) \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/originApplier') y(n) \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/expireDate') k(t) \r\n";
            sql += "WHERE x.m.value('(@presentStatus)[1]','nvarchar(MAX)')='2' ";//已审批通过
            sql += "AND DATEDIFF(day,k.t.value('(@date)[1]','nvarchar(MAX)'),convert(nvarchar,getdate(),23)) < 0 \r\n";//未到查看借阅单期限;DATEDIFF(day,start,end)<0 说明start在end之后
            sql += "AND y.n.value('(@applier)[1]','nvarchar(MAX)')='" + clerk + "' AND T.request_id=" + requestId;
            cntObj = SqlHelper.ExecuteScalar(sql, null);
            cnt = int.Parse(cntObj.ToString());
            if (cnt > 0)//如果有已审批且未到查看期限的
            {
                result = 6;//告诉前端打开借阅单
                return Json(new { rst = result });
            }
            result = 7;//告诉前端打开审批申请
            return Json(new { rst = result });

        }

        public IActionResult GetAuditingsByChecker(string checkerName)
        {
            //使用了SQL SERVER的XML的query方法，即：where 字段.exist(xpath)=1
            string sql = "SELECT Unique_code,application_info,application_time FROM dbo.t_archuse_auditing T \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/auditStatus') x(m) \r\n";
            sql += "WHERE x.m.value('(@presentStatus)[1]','nvarchar(MAX)')='1' AND auditing_link.exist('useRcdAudit/auditNode[@afterPerson=\"" + checkerName + "\"]')=1";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetDoneAuditsByChecker(string checkerName)
        {
            string sql = "SELECT Unique_code,application_info,application_time FROM dbo.t_archuse_auditing T \r\n";
            sql += "cross apply T.auditing_link.nodes('/useRcdAudit/auditStatus') x(m) \r\n";
            sql += "WHERE (x.m.value('(@presentStatus)[1]','nvarchar(MAX)')='2' OR x.m.value('(@presentStatus)[1]','nvarchar(MAX)')='3') \r\n";//审核通过和未通过的
            sql += "AND auditing_link.exist('useRcdAudit/auditNode[@afterPerson=\"" + checkerName + "\"]')=1";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetApplierInfo(string uniquecode)
        {
            string sql = "SELECT T1.application_info,x.m.value('(@selfPlace)[1]','nvarchar(MAX)') AS applier,T2.nick_name,\r\n";
            sql += "x.m.value('(@selfComment)[1]','nvarchar(MAX)') AS comment,y.n.value('(@date)[1]','nvarchar(MAX)') as expdate \r\n";
            sql += "FROM t_archuse_auditing AS T1 ";
            sql += "cross apply T1.auditing_link.nodes('useRcdAudit/auditNode') x(m) \r\n";
            sql += "cross apply T1.auditing_link.nodes('/useRcdAudit/expireDate') y(n) \r\n";
            sql += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') ";
            sql += "WHERE T1.Unique_code=" + uniquecode;//使用了一个表的XML字段中节点属性与另外一个表的字段进行的JINNER JOIN （2020年3月8日，worth remembering）
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        //修改xml某节点属性值，一条一条地修改
        public IActionResult UpdateAuditResult(string uniquecode, string checkResult, string checkComment)
        {
            string status = "1";//1表示已申请待审批状态，2表示审批通过，3表示审批未通过
            if (checkResult.Equals("yes"))
                status = "2";
            else
                status = "3";

            string sql = "UPDATE T SET auditing_link.modify('replace value of (useRcdAudit/auditNode/@checkResult)[1] with \"" + checkResult + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE T SET auditing_link.modify('replace value of (useRcdAudit/auditNode/@checkerComment)[1] with \"" + checkComment + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE T SET auditing_link.modify('replace value of (useRcdAudit/auditNode/@checkTime)[1] with \"" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE T SET auditing_link.modify('replace value of (useRcdAudit/auditStatus/@presentStatus)[1] with \"" + status + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            //sql += "UPDATE T SET auditing_link.modify('replace value of (useRcdAudit/expireDate/@date)[1] with \"" + expireDate + "\"') FROM t_archuse_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            int result = SqlHelper.ExecNonQuery(sql, null);
            return Json(result);
        }

        public IActionResult GetAllUseRcdAudits(int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName2 = "t_archuse_auditing TB";
            string fields2 = " TB.application_info,TB.application_time,TB.end_time,y.n.value('(@presentStatus)[1]','nvarchar(MAX)') check_status,T2.nick_name applier,T3.nick_name checker, \r\n";
            fields2 += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            string innerjoin2 = " cross apply TB.auditing_link.nodes('useRcdAudit/auditNode') AS x(m) \r\n";
            innerjoin2 += " cross apply TB.auditing_link.nodes('/useRcdAudit/auditStatus') y(n) \r\n";
            innerjoin2 += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            innerjoin2 += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            string where2 = " 1=1 ";
            string sort = " application_time DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName2, innerjoin2, fields2, where2, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }
    }
}
