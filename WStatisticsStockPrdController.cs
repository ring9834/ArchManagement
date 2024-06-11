using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Xml;
using System.Data;
using NetCoreDbUtility;

namespace ArchiveFileManagementNs.Controllers
{
    public class WStatisticsStockPrdController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TypeGear()
        {
            return View("StatisticGear");
        }

        public IActionResult TypePreview()
        {
            return View("StatisticPreview");
        }

        /// <summary>
        /// 根据表名取得其字段
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetFieldsByTableName(string id)
        {
            DataTable table = null;
            string sql = "SELECT Unique_code,col_name,show_name FROM t_config_col_dict WHERE code='" + id + "' AND field_type = 0";
            await Task.Run(() =>
            {
                table = SqlHelper.GetDataTable(sql);
            });
            return Json(table);
        }

        public IActionResult GetTableFields()
        {
            List<TableFieldPair> list = new List<TableFieldPair>();
            string sql = "SELECT statistic_tables FROM t_statistic_period";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                XmlDocument doc = new XmlDocument();
                string xmlfile = dt.Rows[0]["statistic_tables"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlNodeList nodeList = doc.SelectNodes(@"Tables/tbitem");
                for (int i = 0; i < nodeList.Count; i++)
                {
                    TableFieldPair tp = new TableFieldPair();
                    tp.Table = nodeList[i].Attributes["name"].Value;
                    tp.Field = nodeList[i].Attributes["field"].Value;
                    list.Add(tp);
                }
            }
            return Json(list);
        }

        public IActionResult AddStcTemplData(string tableName)
        {
            string tableFieldXml = GetTableFieldsXML(tableName);
            string sql = string.Empty;
            sql += "IF (SELECT COUNT(*) FROM t_statistic_period) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "INSERT t_statistic_period(statistic_tables)\r\n";
            sql += "VALUES('" + tableFieldXml + "') \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "UPDATE  t_statistic_period SET statistic_tables='" + tableFieldXml + "'\r\n";
            sql += "END \r\n";
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string title = string.Empty;
            if (result > 0)
            {
                result = 1;
                title = "配置成功！";
                return Json(new { rst = result, info = title });
            }
            result = 0;
            title = "配置失败！";
            return Json(new { rst = result, info = title });
        }

        /// <summary>
        /// 更新XML字段的内容
        /// </summary>
        protected string GetTableFieldsXML(string tableName)
        {
            string sql = "SELECT statistic_tables FROM t_statistic_period";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            XmlDocument doc = new XmlDocument();
            if (dt.Rows.Count > 0)//如果已经存在记录（保管期限统计在t_statistic_period中仅能存在一条记录）
            {
                string xmlfile = dt.Rows[0]["statistic_tables"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlElement root = doc.DocumentElement;

                XmlNodeList nodeList = doc.SelectNodes(@"Tables/tbitem[@name='" + tableName + "']");
                if (nodeList.Count == 0)//档案库不能重复统计
                {
                    XmlElement element = doc.CreateElement("tbitem");
                    element.SetAttribute("name", tableName);
                    element.SetAttribute("field", "");
                    root.AppendChild(element);
                }
            }
            else
            {
                XmlElement root = doc.CreateElement("Tables");
                doc.AppendChild(root);

                XmlElement element = doc.CreateElement("tbitem");
                element.SetAttribute("name", tableName);
                element.SetAttribute("field", "");
                root.AppendChild(element);
            }
            return doc.OuterXml;
        }

        public IActionResult DeleteInfo(string table)
        {
            string sql = "SELECT statistic_tables FROM t_statistic_period";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            XmlDocument doc = new XmlDocument();
            if (dt.Rows.Count > 0)//如果已经存在某种数量类型的统计
            {
                string xmlfile = dt.Rows[0]["statistic_tables"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlElement root = doc.DocumentElement;
                XmlNodeList nodeList = doc.SelectNodes(@"Tables/tbitem[@name='" + table + "']");
                if (nodeList.Count > 0)//档案类型库不能重复统计
                {
                    XmlNode node = nodeList[0];
                    doc.DocumentElement.RemoveChild(node);
                }

                nodeList = doc.SelectNodes(@"Tables/tbitem");
                if (nodeList.Count > 0)
                    sql = "UPDATE t_statistic_period SET statistic_tables='" + doc.OuterXml + "'";
                else
                    sql = "DELETE FROM t_statistic_period";

                SqlHelper.ExecNonQuery(sql, null);
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = 1 });
        }

        public IActionResult UpdateInfo(string table, string field)
        {
            string sql = "SELECT statistic_tables FROM t_statistic_period";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            XmlDocument doc = new XmlDocument();
            if (dt.Rows.Count > 0)//如果已经存在某种数量类型的统计
            {
                string xmlfile = dt.Rows[0]["statistic_tables"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlElement root = doc.DocumentElement;
                XmlNodeList nodeList = doc.SelectNodes(@"Tables/tbitem[@name='" + table + "']");
                if (nodeList.Count > 0)//档案类型库不能重复统计
                {
                    XmlNode node = nodeList[0];
                    node.Attributes["field"].Value = string.IsNullOrEmpty(field) || field == "-1" ? "" : field;
                }

                sql = "UPDATE t_statistic_period SET statistic_tables='" + doc.OuterXml + "'";
                SqlHelper.ExecNonQuery(sql, null);
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = 1 });
        }

        /// <summary>
        /// 馆藏保管期限统计
        /// </summary>
        /// <returns></returns>
        public string UpdateStckPrdSttstcAllData()
        {
            string sql = "SELECT statistic_tables,period_count FROM t_statistic_period";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            List<TableFieldPair> tfs = new List<TableFieldPair>();
            if (dt.Rows.Count > 0)
            {
                XmlDocument doc = new XmlDocument();
                var tablesObj = dt.Rows[0]["statistic_tables"];
                if (tablesObj != DBNull.Value && !string.IsNullOrEmpty(tablesObj.ToString()))
                {
                    string xmlfile = tablesObj.ToString();
                    doc.LoadXml(xmlfile.ToString());
                    XmlNodeList nodeList = doc.SelectNodes(@"Tables/tbitem");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        XmlNode xn = nodeList[i];
                        TableFieldPair tf = new TableFieldPair();
                        tf.Table = xn.Attributes["name"].Value;
                        tf.Field = xn.Attributes["field"].Value;
                        tfs.Add(tf);
                    }

                    sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='GCBGQXTJ')";
                    DataTable dt2 = SqlHelper.GetDataTable(sql, null);

                    string outXML = "";//记录所有保管期限对应的documentElement生成后的XML
                    var prdCountObj = dt.Rows[0]["period_count"];
                    for (int j = 0; j < dt2.Rows.Count; j++)
                    {
                        var cv = dt2.Rows[j]["code_value"];
                        List<string> lks = new List<string>();
                        string like = "";
                        if (cv != null && !string.IsNullOrEmpty(cv.ToString()))
                        {
                            lks = cv.ToString().Split('|').ToList();
                            for (int m = 0; m < lks.Count; m++)
                            {
                                if (m == 0)
                                    like += " {0} LIKE '%" + lks[m] + "%' ";
                                else
                                    like += " OR {0} LIKE '%" + lks[m] + "%' ";
                            }

                            int totalForSpecificStockPeriod = 0;
                            for (int k = 0; k < tfs.Count; k++)
                            {
                                //await Task.Run(() =>
                                //{
                                if (!string.IsNullOrEmpty(tfs[k].Field))//this condition added on 20210105
                                {
                                    sql = "IF OBJECT_ID(N'[" + tfs[k].Table + "]',N'U') IS NOT NULL \r\n";
                                    sql += "BEGIN \r\n";
                                    sql += "    SELECT COUNT(Unique_code) AS cnt FROM " + tfs[k].Table + " WHERE " + like + " \r\n";
                                    sql += "END \r\n";
                                    sql += "ELSE \r\n";
                                    sql += "BEGIN \r\n";
                                    sql += "    UPDATE t_statistic_period SET statistic_tables.modify('delete /Tables/tbitem[@name=\""+ tfs[k].Table + "\"]') \r\n";
                                    sql += "    SELECT 0 \r\n";
                                    sql += "END \r\n";

                                    sql = string.Format(sql, tfs[k].Field);
                                    var ct = SqlHelper.ExecuteScalar(sql, null);
                                    totalForSpecificStockPeriod += int.Parse(ct.ToString());
                                }
                                //});
                            }

                            var prdName = dt2.Rows[j]["code_name"].ToString();
                            prdCountObj = outXML;//更新下面函数的输入
                            GetPrdCountXML(prdCountObj, prdName, totalForSpecificStockPeriod.ToString(), out outXML);
                        }
                    }
                    sql = "UPDATE t_statistic_period SET period_count = '" + outXML + "'";
                    SqlHelper.ExecNonQuery(sql, null);
                    dt2.Dispose();
                }
            }
            dt.Dispose();
            return null;
        }

        private void GetPrdCountXML(object xmlObj, string period, string count, out string outXML)
        {
            XmlDocument doc = new XmlDocument();
            if (xmlObj == DBNull.Value || string.IsNullOrEmpty(xmlObj.ToString()))
            {
                XmlElement root = doc.CreateElement("PrdCounts");
                doc.AppendChild(root);

                XmlElement element = doc.CreateElement("prdItem");
                element.SetAttribute("name", period);
                element.SetAttribute("count", count);
                root.AppendChild(element);
            }
            else
            {
                doc.LoadXml(xmlObj.ToString());
                XmlElement root = doc.DocumentElement;
                XmlNodeList nodeList = doc.SelectNodes(@"PrdCounts/prdItem[@name='" + period + "']");
                if (nodeList.Count == 0)//没有某一保管期限对应的数量
                {
                    XmlElement element = doc.CreateElement("prdItem");
                    element.SetAttribute("name", period);
                    element.SetAttribute("count", count);
                    root.AppendChild(element);
                }
                else
                {//有某一保管期限对应的数量，则更新
                    nodeList[0].Attributes["count"].Value = count;
                }
            }
            outXML = doc.OuterXml;
        }

        /// <summary>
        /// 获取首页馆藏保管期限统计结果
        /// </summary>
        /// <returns></returns>
        public IActionResult GetStckPrdStatiscInfo()
        {
            string sql = "SELECT period_count FROM t_statistic_period";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            List<FieldValuePair> list = new List<FieldValuePair>();
            if (dt.Rows.Count > 0)
            {
                var prdObj = dt.Rows[0]["period_count"];
                if (prdObj != DBNull.Value && !string.IsNullOrEmpty(prdObj.ToString()))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(prdObj.ToString());
                    XmlElement root = doc.DocumentElement;
                    XmlNodeList nodeList = doc.SelectNodes(@"PrdCounts/prdItem");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        FieldValuePair fvp = new FieldValuePair();
                        fvp.Field = nodeList[i].Attributes["name"].Value;
                        fvp.Value = nodeList[i].Attributes["count"].Value;
                        list.Add(fvp);
                    }
                }
            }
            return Json(list);
        }

        /// <summary>
        /// 更新数据库中保管期限统计结果后，再为首页获取馆藏保管期限统计结果
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetStckPrdStatiscInfo2()
        {
            UpdateStckPrdSttstcAllData();//先更新数据库
            return GetStckPrdStatiscInfo();
        }
    }
}
