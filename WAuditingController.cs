using ArchiveFileManagementNs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WAuditingController : WBaseController
    {
        private IHttpContextAccessor _accessor;
        public WAuditingController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public IActionResult Index(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View();
        }

        public IActionResult ShowApplctWin(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View("Epplication");
        }

        public IActionResult GetAuditRecs(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View("Auditings");
        }

        public IActionResult GetAuditDoneRecs(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View("AuditsDone");
        }

        public IActionResult GetAuditAllRecs(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View("AuditsAll");
        }

        public IActionResult ShowToCheckWin(string id)
        {
            ViewData["id"] = id;
            return View("CheckingIn");
        }

        public IActionResult VerifyIfAdtNeeded(string table, List<string> ids)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT check_inout FROM t_config_type_tree WHERE code='"+ table +"'";
            object checkObj = SqlHelper.ExecuteScalar(sql, null);
            if (checkObj == DBNull.Value || checkObj.ToString() == "0" || checkObj.ToString().ToLower()=="false")
            {
                return Json(new { ifcheck = 0 });//不需要审批后入库，以上代码2020年3月13日增加
            }
            
            //string codes = "";
            //for (int i = 0; i < ids.Count; i++)
            //{
            //    if (i == 0)
            //        codes += ids[i];
            //    else
            //        codes += "," + ids[i];
            //}

            //sql += " SELECT COUNT(Unique_code) FROM " + table + " WHERE check_status ='0' AND  Unique_code IN(" + codes + ")";
            //object cnt = SqlHelper.ExecuteScalar(sql, null);
            //int nocheck = int.Parse(cnt.ToString());

            //sql = "SELECT COUNT(Unique_code) FROM " + table + " WHERE check_status ='2' AND  Unique_code IN(" + codes + ")";
            //cnt = SqlHelper.ExecuteScalar(sql, null);
            //int haveChecked = int.Parse(cnt.ToString());

            //取得某所有ids/id节点下所有属性为id的值
            sql = "SELECT DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)')) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m)\r\n";
            sql += " WHERE (check_status='1' OR check_status='2') AND to_stable_done='0' AND T.table_code='" + table + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            List<string> listFrmArray = dt.AsEnumerable().Select(d => d.Field<string>(0)).ToList();//将DataTable的第一列中所有数据转换成为List
            IEnumerable<string> list = ids.Except(listFrmArray);//取差集，若list中元素个数为0，说明ids中的所有元素都在listFrmArray中
            int count = list.Count();

            sql = "SELECT DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)')) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m)\r\n";
            sql += " WHERE check_status='2' AND to_stable_done='0' AND T.table_code='" + table + "'";
             dt = SqlHelper.GetDataTable(sql, null);
            listFrmArray = dt.AsEnumerable().Select(d => d.Field<string>(0)).ToList();
            list = ids.Except(listFrmArray);//取差集，若list中元素个数为0，说明ids中的所有元素都在listFrmArray中
            int haveChcked = list.Count();

            dt.Dispose();            
            return Json(new { nocheck = count, haveChecked = haveChcked });
        }

        public IActionResult VerifyIfAdtNeededForAll(string table, string where, string pms, string searchmode)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT check_inout FROM t_config_type_tree WHERE code='" + table + "'";
            object checkObj = SqlHelper.ExecuteScalar(sql, null);
            if (checkObj == DBNull.Value || checkObj.ToString() == "0" || checkObj.ToString().ToLower() == "false")
            {
                return Json(new { ifcheck = 0 });//不需要审批后入库，以上代码2020年3月13日增加
            }

            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            { }
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
            }

            //sql += "SELECT COUNT(Unique_code) FROM " + table + " WHERE check_status ='0' AND" + where;
            //object cnt = SqlHelper.ExecuteScalar(sql, null);
            //int nocheck = int.Parse(cnt.ToString());

            //sql = "SELECT COUNT(Unique_code) FROM " + table + " WHERE check_status ='1' AND" + where;
            //cnt = SqlHelper.ExecuteScalar(sql, null);
            //int checking = int.Parse(cnt.ToString());

            sql = "SELECT CAST(Unique_code AS nvarchar(MAX)) FROM " + table + " WHERE " + where;
            DataTable dt0 = SqlHelper.GetDataTable(sql, list.ToArray());
            List<string> listFrmArray0 = dt0.AsEnumerable().Select(d => d.Field<string>(0)).ToList();//将DataTable的第一列中所有数据转换成为List

            sql = "SELECT DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)')) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m)\r\n";
            sql += " WHERE (check_status='1' OR check_status='2') AND to_stable_done='0' AND T.table_code='" + table + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            List<string> listFrmArray = dt.AsEnumerable().Select(d => d.Field<string>(0)).ToList();//将DataTable的第一列中所有数据转换成为List
            IEnumerable<string> lst = listFrmArray0.Except(listFrmArray);//取交集，若list中元素个数为0，说明listFrmArray0中的所有元素都在listFrmArray中
            int count = lst.Count();

            sql = "SELECT DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)')) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m)\r\n";
            sql += " WHERE check_status='2' AND to_stable_done='0' AND T.table_code='" + table + "'";
            dt = SqlHelper.GetDataTable(sql, null);
            listFrmArray = dt.AsEnumerable().Select(d => d.Field<string>(0)).ToList();//将DataTable的第一列中所有数据转换成为List
            lst = listFrmArray0.Except(listFrmArray);//取交集，若list中元素个数为0，说明listFrmArray0中的所有元素都在listFrmArray中
            int haveChcked = lst.Count();

            dt0.Dispose();
            dt.Dispose();
            return Json(new { nocheck = count, haveChecked = haveChcked });
        }

        public IActionResult RecordAuditInfo(string table, string applytitle, string applier, string checker, string comment, List<int> ids)
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

            //审核记录的ID列表
            XmlDocument ids_doc = new XmlDocument();
            XmlElement ids_root = ids_doc.CreateElement("ids");
            for (int i = 0; i < ids.Count; i++)
            {
                XmlElement doc_elemt = ids_doc.CreateElement("id");
                doc_elemt.SetAttribute("id", ids[i].ToString());
                ids_root.AppendChild(doc_elemt);
            }
            ids_doc.AppendChild(ids_root);

            string where = "Unique_code=";
            for (int i = 0; i < ids.Count; i++)
            {
                if (i == 0)
                    where += ids[i];
                else
                    where += " OR Unique_code=" + ids[i];
            }
            //更新记录本身的审核状态
            //string sql = "UPDATE " + table + " SET check_status='1' WHERE " + where + " \r\n";//0表示待审核，1表示审核中，2表示已审核
            //插入审核日志
            string sql = "INSERT INTO t_auditing(table_code,application_info,auditing_link,application_time,end_time,archive_dealt) \r\n";
            sql += "VALUES(@table_code,@application_info,@auditing_link,@application_time,@end_time,@archive_dealt)";
            SqlParameter para1 = SqlHelper.MakeInParam("table_code", table);
            SqlParameter para2 = SqlHelper.MakeInParam("application_info", applytitle);
            SqlParameter para3 = SqlHelper.MakeInParam("auditing_link", link_doc.OuterXml);
            SqlParameter para4 = SqlHelper.MakeInParam("application_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para5 = SqlHelper.MakeInParam("end_time", "");
            SqlParameter para6 = SqlHelper.MakeInParam("archive_dealt", ids_doc.OuterXml);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6 };
            int result = SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        /// <summary>
        /// 前端全选搜索结果进行审核时
        /// </summary>
        /// <param name="table"></param>
        /// <param name="applytitle"></param>
        /// <param name="applier"></param>
        /// <param name="checker"></param>
        /// <param name="comment"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public IActionResult RecordAuditInfoAll(string table, string applytitle, string applier, string checker, string comment, string where, string pms, string searchmode)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            if (int.Parse(searchmode) == 0)//初始搜索的传参
            { }
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
            }

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

            //审核记录的ID列表 -- 前端全选搜索结果进行审核时，这样记录ids
            string sql = "";
            sql = "SELECT Unique_code FROM " + table + " WHERE" + where;
            DataTable dt = SqlHelper.GetDataTable(sql, list.ToArray());
            XmlDocument ids_doc = new XmlDocument();
            XmlElement ids_root = ids_doc.CreateElement("ids");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                XmlElement doc_elemt = ids_doc.CreateElement("id");
                doc_elemt.SetAttribute("id", dt.Rows[i][0].ToString());
                ids_root.AppendChild(doc_elemt);
            }
            ids_doc.AppendChild(ids_root);
            dt.Dispose();

            //更新记录本身的审核状态
            //sql = "UPDATE " + table + " SET check_status='1' WHERE " + where + " \r\n";//0表示待审核，1表示审核中，2表示已审核
            //插入审核日志
            sql = "INSERT INTO t_auditing(table_code,application_info,auditing_link,application_time,end_time,archive_dealt) \r\n";
            sql += "VALUES(@table_code,@application_info,@auditing_link,@application_time,@end_time,@archive_dealt)";
            SqlParameter para1 = SqlHelper.MakeInParam("table_code", table);
            SqlParameter para2 = SqlHelper.MakeInParam("application_info", applytitle);
            SqlParameter para3 = SqlHelper.MakeInParam("auditing_link", link_doc.OuterXml);
            SqlParameter para4 = SqlHelper.MakeInParam("application_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para5 = SqlHelper.MakeInParam("end_time", "");
            SqlParameter para6 = SqlHelper.MakeInParam("archive_dealt", ids_doc.OuterXml);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6 };
            int result = SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult GetOngoingAuditing(string table, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_auditing T1";
            string fields = " T1.application_info,T1.application_time,T1.end_time,T1.check_status,T1.Unique_code,T2.nick_name applier,T3.nick_name checker ";
            string innerjoin = " cross apply T1.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            innerjoin += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            innerjoin += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            string where = "T1.check_status='1' AND T1.table_code='" + table + "'";
            string sort = "T1.application_time DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult GetDoneAudits(string table, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_auditing T1";
            string fields = " T1.application_info,T1.application_time,T1.end_time,T1.check_status,T1.Unique_code,T2.nick_name applier,T3.nick_name checker, \r\n";
            fields += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            string innerjoin = " cross apply T1.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            innerjoin += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";//获取入库申请者
            innerjoin += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";//获取入库审批者
            string where = "T1.check_status='2' AND T1.to_stable_done='0' AND T1.table_code='" + table + "'";//已经入资源总库的审批记录不显示：to_stable_done='0'
            string sort = "T1.application_time DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult GetAllAudits(string table, int pageSize, int pageIndex)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_auditing T1";
            string fields = " T1.application_info,T1.application_time,T1.end_time,T1.check_status,T1.to_stable_done,T1.Unique_code,T2.nick_name applier,T3.nick_name checker, \r\n";
            fields += " x.m.value('(@checkTime)[1]','nvarchar(MAX)') check_time ";
            string innerjoin = " cross apply T1.auditing_link.nodes('archiveAudit/auditNode') AS x(m) \r\n";
            innerjoin += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') \r\n";
            innerjoin += "INNER JOIN t_user AS T3 ON T3.user_name=x.m.value('@afterPerson','nvarchar(MAX)') \r\n";
            string where = " T1.table_code='" + table + "'";
            string sort = "T1.application_time DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, innerjoin, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = dt });
        }

        public IActionResult AuditRecDetailView(string id, string userid, string other)
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

            ViewData["table"] = id;//传参
            ViewData["userId"] = userid;
            ViewData["bundle"] = other;
            ViewData["colFields"] = colFields;
            ViewData["fieldStr"] = fieldStr;
            return View("SpecificAuditDetail");
        }

        public IActionResult GetAuditRecordDetail(string tableName, string bundle, string fieldStr, int pageSize, int pageIndex)
        {
            string sql = "SELECT archive_dealt FROM t_auditing WHERE Unique_code=" + bundle;
            object xmlObj = SqlHelper.ExecuteScalar(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (xmlObj != DBNull.Value)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlObj.ToString());
                XmlNodeList nodes = doc.SelectNodes(@"ids/id");
                string wherec = " ";
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (i == 0)
                        wherec += "Unique_code=" + nodes[i].Attributes["id"].Value;
                    else
                        wherec += " OR Unique_code=" + nodes[i].Attributes["id"].Value + " ";
                }

                string codeName = tableName;
                string fields = fieldStr;
                string where = wherec;
                string sort = "Unique_code DESC";
                int pageCount = 0;
                int recordCount = 0;
                DataTable table = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);

                return Json(new { total = recordCount, rows = table });
            }
            return Json(new { total = 0, rows = DBNull.Value });
        }

        public IActionResult GetAuditingsByChecker(string checkerName)
        {
            //使用了SQL SERVER的XML的query方法，即：where 字段.exist(xpath)=1
            string sql = "SELECT Unique_code,table_code,application_info,application_time \n\r";
            sql += "FROM dbo.t_auditing WHERE check_status='1' AND auditing_link.exist('archiveAudit/auditNode[@afterPerson=\"" + checkerName + "\"]')=1";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetDoneAuditsByChecker(string checkerName)
        {
            //使用了SQL SERVER的XML的query方法，即：where 字段.exist(xpath)=1
            string sql = "SELECT Unique_code,table_code,application_info,application_time \n\r";
            sql += "FROM dbo.t_auditing WHERE check_status='2' AND auditing_link.exist('archiveAudit/auditNode[@afterPerson=\"" + checkerName + "\"]')=1";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetApplierInfo(string uniquecode)
        {
            string sql = "SELECT T1.application_info,T1.auditing_link.value('(archiveAudit/auditNode/@selfPlace)[1]','nvarchar(MAX)') AS applier,T2.nick_name,\r\n";
            sql += "T1.auditing_link.value('(archiveAudit/auditNode/@selfComment)[1]','nvarchar(MAX)') AS comment \r\n";
            sql += "FROM t_auditing AS T1 CROSS APPLY T1.auditing_link.nodes('archiveAudit/auditNode') x(m) \r\n";
            sql += "INNER JOIN t_user AS T2 ON T2.user_name=x.m.value('@selfPlace','nvarchar(MAX)') WHERE T1.Unique_code=" + uniquecode;//使用了一个表的XML字段中节点属性与另外一个表的字段进行的JINNER JOIN （2020年3月8日，worth remembering）
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        //修改xml某节点属性值，一条一条地修改
        public IActionResult UpdateAuditResult(string uniquecode, string checkResult, string checkComment)
        {
            string status = "1";//1表示所选记录的审核状态在审核中，2表示审核通过，3表示未通过
            if (checkResult.Equals("yes"))
                status = "2";
            else
                status = "3";

            string sql = "UPDATE T SET auditing_link.modify('replace value of (archiveAudit/auditNode/@checkResult)[1] with \"" + checkResult + "\"') FROM t_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE T SET auditing_link.modify('replace value of (archiveAudit/auditNode/@checkerComment)[1] with \"" + checkComment + "\"') FROM t_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE T SET auditing_link.modify('replace value of (archiveAudit/auditNode/@checkTime)[1] with \"" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "\"') FROM t_auditing T WHERE T.Unique_code=" + uniquecode + " \r\n";
            sql += "UPDATE t_auditing SET check_status='" + status + "' WHERE Unique_code=" + uniquecode;
            int result = SqlHelper.ExecNonQuery(sql, null);
            return Json(result);
        }

        public IActionResult ToStableStoreFromChecked(string id, string table, string userid)
        {
            string sql = "SELECT DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)')) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m) WHERE Unique_code=" + id;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            string where = "Unique_code=";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i == 0)
                    where += dt.Rows[i][0].ToString();
                else
                    where += " OR Unique_code=" + dt.Rows[i][0].ToString();
            }            

            sql = "UPDATE " + table + " SET store_type='2' WHERE " + where + " \r\n";//从预归档库更新到资源总库
            sql += "UPDATE t_auditing SET to_stable_done='1',end_time='" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "' WHERE Unique_code=" + id;//彻底结束审批状态
            int result = SqlHelper.ExecNonQuery(sql, null);

            List<string> ids = dt.AsEnumerable().Select(d => d.Field<string>(0)).ToList();

            string ipAddr = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();//取得客户端的IP地址
            OperationInfo opInfo = MakeOperInfo(userid, table, "入资源总库", "预归档", "", ipAddr);
            opInfo.OperTag = "入库审核通过后的入资源总库操作";
            OperateRecHlp.RcdUserOprationCommon(opInfo, ids);//入库审核通过后的入资源总库操作，记录到用户操作记录中;2020年3月13日
            dt.Dispose();

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
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
