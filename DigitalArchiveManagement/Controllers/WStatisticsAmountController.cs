using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Xml;
using System.Data;
using NetCoreDbUtility;

namespace ArchiveFileManagementNs.Controllers
{
    public class WStatisticsAmountController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AmountGear()
        {
            return View("StatisticGear");
        }

        public IActionResult AmountPreview()
        {
            return View("StatisticPreview");
        }

        /// <summary>
        /// 获取所有数量统计类型
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAmountStTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='GCSLTJ')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetCountTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='TJJSFS')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
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

        /// <summary>
        /// 根据数量统计类型的ID，查找统计所需表和字段的列表
        /// </summary>
        /// <param name="statisticsTypeId"></param>
        /// <returns></returns>
        public IActionResult GetTableFields(string statisticsTypeId)
        {
            List<TableFieldPair> list = new List<TableFieldPair>();
            string sql = "SELECT table_field_name FROM t_statistic_amount WHERE statistic_type_id =" + statisticsTypeId;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                XmlDocument doc = new XmlDocument();
                string xmlfile = dt.Rows[0]["table_field_name"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem");
                for (int i = 0; i < nodeList.Count; i++)
                {
                    TableFieldPair tp = new TableFieldPair();
                    tp.Id = nodeList[i].Attributes["id"].Value;
                    tp.Table = nodeList[i].Attributes["tableName"].Value;
                    tp.Field = nodeList[i].Attributes["fieldName"].Value;
                    tp.CountType = nodeList[i].Attributes["countType"].Value;
                    list.Add(tp);
                }
            }
            return Json(list);
        }

        public IActionResult GetTableFields2()
        {
            List<Statistics> sts = new List<Statistics>();
            string sql = "SELECT T1.table_field_name,T1.statistic_type_id,T2.code_name FROM t_statistic_amount AS T1,t_config_codes AS T2 WHERE T1.statistic_type_id=T2.Unique_code";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                XmlDocument doc = new XmlDocument();
                for (int m = 0; m < dt.Rows.Count; m++)
                {
                    Statistics st = new Statistics();
                    string statisticType = dt.Rows[m]["code_name"].ToString();
                    st.statistic_type = statisticType;

                    string xmlfile = dt.Rows[m]["table_field_name"].ToString();
                    dt.Dispose();
                    doc.LoadXml(xmlfile.ToString());
                    XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem");
                    List<TableFieldPair> list = new List<TableFieldPair>();
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        TableFieldPair tp = new TableFieldPair();
                        tp.Table = nodeList[i].Attributes["tableName"].Value;
                        tp.Field = nodeList[i].Attributes["fieldName"].Value;
                        tp.CountType = nodeList[i].Attributes["countType"].Value;
                        list.Add(tp);
                    }
                    st.Tfs = list;
                    sts.Add(st);
                }
            }
            return Json(sts);
        }

        public IActionResult AddStcTemplData(string statisticTypeId, string id, string tableName)
        {
            string tableFieldXml = GetTableFieldsXML(statisticTypeId, id, tableName);
            string sql = string.Empty;
            sql += "IF (SELECT COUNT(*) FROM t_statistic_amount WHERE statistic_type_id=" + statisticTypeId + ") = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "INSERT t_statistic_amount(statistic_type_id,table_field_name)\r\n";
            sql += "VALUES(" + statisticTypeId + ",'" + tableFieldXml + "') \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "UPDATE  t_statistic_amount SET table_field_name='" + tableFieldXml + "'\r\n";
            sql += "WHERE statistic_type_id=" + statisticTypeId + "\r\n";
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
        /// <param name="statistic_type_id"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected string GetTableFieldsXML(string statistic_type_id, string id, string tableName)
        {
            string sql = "SELECT table_field_name FROM t_statistic_amount WHERE statistic_type_id=" + statistic_type_id;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            XmlDocument doc = new XmlDocument();
            if (dt.Rows.Count > 0)//如果已经存在某种数量类型的统计
            {
                string xmlfile = dt.Rows[0]["table_field_name"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlElement root = doc.DocumentElement;

                XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem[@tableName=" + tableName + "]");
                if (nodeList.Count == 0)//档案类型库不能重复统计
                {
                    XmlElement element = doc.CreateElement("TfItem");
                    element.SetAttribute("id", id);
                    element.SetAttribute("tableName", tableName);
                    element.SetAttribute("fieldName", "");
                    element.SetAttribute("countType", "");
                    root.AppendChild(element);
                }
            }
            else
            {

                XmlElement root = doc.CreateElement("TableFieldsMakeup");
                doc.AppendChild(root);

                XmlElement element = doc.CreateElement("TfItem");
                element.SetAttribute("id", id);
                element.SetAttribute("tableName", tableName);
                element.SetAttribute("fieldName", "");
                element.SetAttribute("countType", "");
                root.AppendChild(element);
            }
            return doc.OuterXml;
        }

        public IActionResult DeleteInfo(string typeId, string sid)
        {
            string sql = "SELECT table_field_name FROM t_statistic_amount WHERE statistic_type_id=" + typeId;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            XmlDocument doc = new XmlDocument();
            if (dt.Rows.Count > 0)//如果已经存在某种数量类型的统计
            {
                string xmlfile = dt.Rows[0]["table_field_name"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlElement root = doc.DocumentElement;

                XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem[@id=" + sid + "]");
                if (nodeList.Count > 0)//档案类型库不能重复统计
                {
                    XmlNode node = nodeList[0];
                    doc.DocumentElement.RemoveChild(node);
                }

                nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem");
                if (nodeList.Count > 0)
                    sql = "UPDATE  t_statistic_amount SET table_field_name='" + doc.OuterXml + "' WHERE statistic_type_id=" + typeId;
                else
                    sql = "DELETE FROM t_statistic_amount WHERE statistic_type_id=" + typeId;

                SqlHelper.ExecNonQuery(sql, null);
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = 1 });
        }

        public IActionResult UpdateInfo(string typeId, string sid, string fieldName, string countType)
        {
            string sql = "SELECT table_field_name FROM t_statistic_amount WHERE statistic_type_id=" + typeId;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            XmlDocument doc = new XmlDocument();
            if (dt.Rows.Count > 0)//如果已经存在某种数量类型的统计
            {
                string xmlfile = dt.Rows[0]["table_field_name"].ToString();
                dt.Dispose();
                doc.LoadXml(xmlfile.ToString());
                XmlElement root = doc.DocumentElement;
                XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem[@id=" + sid + "]");
                if (nodeList.Count > 0)//档案类型库不能重复统计
                {
                    XmlNode node = nodeList[0];
                    node.Attributes["fieldName"].Value = string.IsNullOrEmpty(fieldName) || fieldName == "-1" ? "" : fieldName;
                    node.Attributes["countType"].Value = string.IsNullOrEmpty(countType) || countType == "-1" ? "" : countType;
                }

                sql = "UPDATE  t_statistic_amount SET table_field_name='" + doc.OuterXml + "' WHERE statistic_type_id=" + typeId;
                SqlHelper.ExecNonQuery(sql, null);
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = 1 });
        }

        /// <summary>
        /// 馆藏数量统计
        /// </summary>
        /// <returns></returns>
        public void UpdateAmntSttstcAllData()
        {
            string sql = "SELECT T1.statistic_type_id,T1.table_field_name,T2.code_name FROM t_statistic_amount AS T1,t_config_codes AS T2  \r\n";
            sql += "WHERE T1.statistic_type_id = T2.Unique_code AND T2.parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='GCSLTJ')";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            for (int m = 0; m < dt.Rows.Count; m++)
            {
                XmlDocument doc = new XmlDocument();
                string xmlfile = dt.Rows[m]["table_field_name"].ToString();
                string typeId = dt.Rows[m]["statistic_type_id"].ToString();
                doc.LoadXml(xmlfile.ToString());
                XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem");
                DataTable dt2 = new DataTable();
                int total = 0;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    string tn = nodeList[i].Attributes["tableName"].Value;
                    string fn = nodeList[i].Attributes["fieldName"].Value;
                    string ct = nodeList[i].Attributes["countType"].Value;

                    if (ct.Equals("FLD-SUM"))//字段求和
                    {
                        sql = "SELECT dbo.GD_ExtractNumeric(" + fn + ") AS " + fn + " INTO #T FROM " + tn + " \r\n";
                        sql += "SELECT SUM(CAST(" + fn + " AS INT)) FROM #T \r\n";
                        sql += "DROP TABLE #T";
                        dt2 = SqlHelper.GetDataTable(sql, null);
                        var tempSum = dt2.Rows[0][0];
                        total += tempSum ==DBNull.Value ? 0 : int.Parse(tempSum.ToString());
                    }
                    else if (ct.Equals("SECR-COUNT"))//涉密数量
                    {
                        sql = "SELECT COUNT(*) as cnt FROM " + tn + " WHERE " + fn + " LIKE '%密%'";
                        dt2 = SqlHelper.GetDataTable(sql, null);
                        var tempSum = dt2.Rows[0][0];
                        total += tempSum == DBNull.Value ? 0 : int.Parse(tempSum.ToString());
                    }
                    else if (ct.Equals("ELCCONT-COUNT"))//有电子原文的目录数量
                    {
                        sql = "IF EXISTS( \r\n";
                        sql += "    SELECT 1 FROM sysobjects a INNER JOIN syscolumns b \r\n";
                        sql += "    ON a.id=b.id AND a.xtype='U' \r\n";
                        sql += "    WHERE a.name='" + tn + "' AND (b.name ='yw' OR b.name ='yw_xml')) \r\n";
                        sql += "BEGIN \r\n";
                        sql += "    SELECT COUNT(*) FROM " + tn + " WHERE yw IS NOT NULL OR yw_xml IS NOT NULL \r\n";
                        sql += "END \r\n";
                        sql += "ELSE \r\n";
                        sql += "BEGIN \r\n";
                        sql += "    SELECT 0 \r\n";
                        sql += "END \r\n";
                        dt2 = SqlHelper.GetDataTable(sql, null);
                        var tempSum = dt2.Rows[0][0];
                        total += tempSum == DBNull.Value ? 0 : int.Parse(tempSum.ToString());
                    }
                    else if (ct.Equals("CATL-COUNT"))//目录数量
                    {
                        sql = "SELECT COUNT(*) as cnt FROM " + tn;
                        dt2 = SqlHelper.GetDataTable(sql, null);
                        var tempSum = dt2.Rows[0][0];
                        total += tempSum == DBNull.Value ? 0 : int.Parse(tempSum.ToString());
                    }
                }
                sql = "UPDATE t_statistic_amount SET statistic_result=" + total + ",last_update='" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "' WHERE statistic_type_id=" + typeId;
                SqlHelper.ExecNonQuery(sql, null);
                dt2.Dispose();
            }
            dt.Dispose();
        }

        /// <summary>
        /// 获取首页馆藏数量统计结果
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAmountStatiscInfo()
        {
            List<AmountModel> list = new List<AmountModel>();
            string sql = "SELECT T1.statistic_type_id,T1.statistic_result,T2.code_name FROM t_statistic_amount AS T1,t_config_codes AS T2  \r\n";
            sql += "WHERE T1.statistic_type_id = T2.Unique_code AND T2.parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='GCSLTJ')";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                AmountModel am = new AmountModel();
                am.ID = int.Parse(dt.Rows[i]["statistic_type_id"].ToString());
                am.Name = dt.Rows[i]["code_name"].ToString();
                var sresult = dt.Rows[i]["statistic_result"];
                am.Amount = sresult == DBNull.Value ? 0 : int.Parse(sresult.ToString());
                list.Add(am);
            }
            return Json(list);
        }

        /// <summary>
        /// 更新数据库中数量统计结果后，再为首页获取馆藏数量统计结果
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAmountStatiscInfo2()
        {
            UpdateAmntSttstcAllData();
            return GetAmountStatiscInfo();
        }
    }
}
