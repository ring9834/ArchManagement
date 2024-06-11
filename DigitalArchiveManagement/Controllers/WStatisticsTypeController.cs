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
    public class WStatisticsTypeController : WBaseController
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
        /// 获取所有数量统计类型
        /// </summary>
        /// <returns></returns>
        public IActionResult GetTypeStTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='GCDALXTJ')";
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
            string sql = "SELECT table_field_name FROM t_statistic_type WHERE statistic_type_id =" + statisticsTypeId;
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
                    //tp.CountType = nodeList[i].Attributes["countType"].Value;
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
                        //tp.CountType = nodeList[i].Attributes["countType"].Value;
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
            sql += "IF (SELECT COUNT(*) FROM t_statistic_type WHERE statistic_type_id=" + statisticTypeId + ") = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "INSERT t_statistic_type(statistic_type_id,table_field_name)\r\n";
            sql += "VALUES(" + statisticTypeId + ",'" + tableFieldXml + "') \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "UPDATE  t_statistic_type SET table_field_name='" + tableFieldXml + "'\r\n";
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
            string sql = "SELECT table_field_name FROM t_statistic_type WHERE statistic_type_id=" + statistic_type_id;
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
                    //element.SetAttribute("countType", "");
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
                //element.SetAttribute("countType", "");
                root.AppendChild(element);
            }
            return doc.OuterXml;
        }

        public IActionResult DeleteInfo(string typeId, string sid)
        {
            string sql = "SELECT table_field_name FROM t_statistic_type WHERE statistic_type_id=" + typeId;
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
                    sql = "UPDATE  t_statistic_type SET table_field_name='" + doc.OuterXml + "' WHERE statistic_type_id=" + typeId;
                else
                    sql = "DELETE FROM t_statistic_type WHERE statistic_type_id=" + typeId;

                SqlHelper.ExecNonQuery(sql, null);
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = 1 });
        }

        public IActionResult UpdateInfo(string typeId, string sid, string fieldName)
        {
            string sql = "SELECT table_field_name FROM t_statistic_type WHERE statistic_type_id=" + typeId;
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
                    //node.Attributes["countType"].Value = string.IsNullOrEmpty(countType) || countType == "-1" ? "" : countType;
                }

                sql = "UPDATE  t_statistic_type SET table_field_name='" + doc.OuterXml + "' WHERE statistic_type_id=" + typeId;
                SqlHelper.ExecNonQuery(sql, null);
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = 1 });
        }

        /// <summary>
        /// 馆藏类型统计
        /// </summary>
        /// <returns></returns>
        public void UpdateAmntSttstcAllData()
        {
            string sql = "SELECT T1.statistic_type_id,T1.table_field_name,T2.code_name FROM t_statistic_type AS T1,t_config_codes AS T2  \r\n";
            sql += "WHERE T1.statistic_type_id = T2.Unique_code AND T2.parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='GCDALXTJ')";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            for (int m = 0; m < dt.Rows.Count; m++)
            {
                XmlDocument doc = new XmlDocument();
                string xmlfile = dt.Rows[m]["table_field_name"].ToString();
                string typeId = dt.Rows[m]["statistic_type_id"].ToString();
                doc.LoadXml(xmlfile.ToString());
                XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem");
                DataTable dt2 = new DataTable();
                for (int i = 0; i < nodeList.Count; i++)
                {
                    XmlNode xn = nodeList[i];
                    string tn = xn.Attributes["tableName"].Value;
                    string fn = xn.Attributes["fieldName"].Value;
                    if (!string.IsNullOrEmpty(fn))//不设置年度字段的库，不计算
                    {
                        sql = "SELECT distinct " + fn + ",count(Unique_code) as cnt FROM " + tn + " GROUP BY " + fn + " ORDER BY " + fn + " ASC";
                        dt2 = SqlHelper.GetDataTable(sql, null);

                        string nds = "";
                        string cnts = "";
                        for (int k = 0; k < dt2.Rows.Count; k++)
                        {
                            var theNd = dt2.Rows[k]["nd"].ToString();
                            var theCount = dt2.Rows[k]["cnt"].ToString();
                            if (k == 0)
                            {
                                nds += theNd;
                                cnts += theCount;
                            }
                            else
                            {
                                nds += "," + theNd;
                                cnts += "," + theCount;
                            }
                        }
                        XmlElement xe = (XmlElement)xn;
                        xe.SetAttribute("nds", nds);
                        xe.SetAttribute("cnts", cnts);
                    }
                }
                sql = "UPDATE t_statistic_type SET table_field_name='" + doc.InnerXml + "',last_update='" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "' WHERE statistic_type_id=" + typeId;
                SqlHelper.ExecNonQuery(sql, null);
                dt2.Dispose();
            }
            dt.Dispose();
        }

        /// <summary>
        /// 获取首页馆藏类型统计结果
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAmountStatiscInfo()
        {
            string sql = "SELECT T1.statistic_type_id,T1.table_field_name,T2.code_name FROM t_statistic_type AS T1,t_config_codes AS T2  \r\n";
            sql += "WHERE T1.statistic_type_id = T2.Unique_code AND T2.parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='GCDALXTJ')";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            ChartData cd = new ChartData();
            cd.Legends = new List<string>();
            cd.Years = new List<string>();
            cd.Series = new List<List<int>>();

            List<List<string>> tmpYears = new List<List<string>>();
            List<List<YearCountPair>> tmpCounts = new List<List<YearCountPair>>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                List<string[]> ndList = new List<string[]>();//形成echart的xAxis数据所用
                List<YearCountPair> cntList = new List<YearCountPair>();//形成echart的yAxis数据所用

                var lname = dt.Rows[i]["code_name"].ToString();
                cd.Legends.Add(lname);//获得的Legend names

                var xml = dt.Rows[i]["table_field_name"].ToString();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNodeList nodeList = doc.SelectNodes(@"TableFieldsMakeup/TfItem");

                for (int j = 0; j < nodeList.Count; j++)//为ndList,cntList收集数据
                {
                    XmlNode xn = nodeList[j];
                    string tn = xn.Attributes["tableName"].Value;
                    string fn = xn.Attributes["fieldName"].Value;
                    if (!string.IsNullOrEmpty(fn))//年度字段若未设置，则tn对应的库不参与统计
                    {
                        var nds = xn.Attributes["nds"];
                        if (nds != null && !string.IsNullOrEmpty(nds.Value))//nds和cnts不能为空
                        {
                            string[] ndArr = nds.Value.Split(",");
                            ndList.Add(ndArr);

                            string cnts = xn.Attributes["cnts"].Value;
                            string[] cntArr = cnts.Split(",");
                            for (int m = 0; m < cntArr.Length; m++)
                            {
                                YearCountPair ycp = new YearCountPair();
                                ycp.Year = ndArr[m];
                                ycp.Count = cntArr[m];
                                cntList.Add(ycp);
                            }
                        }
                    }
                }

                //============第一次合并========
                List<string> yearList = new List<string>();
                for (int n = 0; n < ndList.Count; n++)
                {
                    List<string> ls = new List<string>(ndList[n]);
                    yearList = yearList.Union(ls).ToList();//合并所有的年度，然后去重
                }
                tmpYears.Add(yearList);

                List<YearCountPair> countList = new List<YearCountPair>();
                for (int n = 0; n < yearList.Count; n++)
                {
                    var year1 = yearList[n];
                    YearCountPair ycp = new YearCountPair();
                    ycp.Year = year1;

                    for (int k = 0; k < cntList.Count; k++)//合并每个年度对应的数量（记录数）
                    {
                        if (cntList[k].Year.Equals(year1))
                            ycp.Count += int.Parse(cntList[k].Count);
                    }
                    countList.Add(ycp);
                }
                tmpCounts.Add(countList);
                //===============================
            }

            //===============第二次合并==========
            for (int o = 0; o < tmpYears.Count; o++)
                cd.Years = cd.Years.Union(tmpYears[o]).ToList();//各统计类型间合并年度，然后去重 ///

            for (int r = 0; r < tmpCounts.Count; r++)//年度合并后，再以年度为准，排列对应年度的Count
            {
                List<int> resultCountList = new List<int>();
                List<YearCountPair> ycps = tmpCounts[r];
                for (int w = 0; w < ycps.Count; w++)
                {
                    var flag = false;
                    YearCountPair ycp = ycps[w];
                    for (int p = 0; p < cd.Years.Count; p++)
                    {
                        var resultYear = cd.Years[p];
                        if (ycp.Year.Equals(resultYear))
                        {
                            resultCountList.Add(int.Parse(ycp.Count));//若有对应的年度，则将对应年度的Count加入到resultCountList
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)//若没有对应年度，则resultCountList加入0作为占位
                        resultCountList.Add(0);
                }
                cd.Series.Add(resultCountList);
            }
            //=====================================
            cd.Years.Sort();
            return Json(cd);
        }

        /// <summary>
        /// 更新数据库中类型统计结果后，再为首页获取馆藏类型统计结果
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAmountStatiscInfo2()
        {
            UpdateAmntSttstcAllData();
            return GetAmountStatiscInfo();
        }
    }
}
