using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data.Common;
using System.Web;
using System.Text;

namespace ArchiveFileManagementNs.Controllers
{
    public class WStatisticsController : WBaseController
    {
        public IActionResult CountStatistics(string id, string stctype)
        {
            ViewData["table"] = id;
            ViewData["stctype"] = stctype;//从首页传过来的统计类型（用户点击选择的）
            return View();
        }

        public IActionResult SearchConditionView(string id)
        {
            ViewData["table"] = id;
            return View("SearchCondition");
        }

        public IActionResult StatisticFieldsView(string id)
        {
            ViewData["table"] = id;
            return View("StatisticFields");
        }

        public IActionResult GroupFieldsView(string id)
        {
            ViewData["table"] = id;
            return View("GroupFields");
        }

        public IActionResult StatisticPreviewView(string id)
        {
            ViewData["table"] = id;
            return View("StatisticPreview");
        }

        public IActionResult StcTemplateView()
        {
            return View("StcTemplate");
        }

        public IActionResult AddStcTemplateView()
        {
            return View("AddStcTemplate");
        }

        public IActionResult SpecificTemplatesView()
        {
            return View("SpecificTemplates");
        }

        public IActionResult GetFieldsByTableName(string id)
        {
            string sql = "SELECT Unique_code,col_name,show_name FROM t_config_col_dict WHERE code='" + id + "' AND field_type = 0";
            DataTable table = SqlHelper.GetDataTable(sql);
            return Json(table);
        }

        public IActionResult GetSelectedFields(string id)
        {
            string sql = "SELECT t2.Unique_code,t2.col_name,t2.show_name,t1.search_code FROM t_config_primary_search AS t1 INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code WHERE t2.code='" + id + "' ORDER BY order_number ASC";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetSConditions()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='jstj')";
            DataTable dtcondtion = SqlHelper.GetDataTable(sql, null);
            return Json(dtcondtion);
        }

        public IActionResult ExecStatistic(List<SearchCondition> searchCondition, List<StatisticFields> statisticFields, List<GroupFields> groupFields, int pageSize, int pageIndex, string table, bool isSum, bool isNumStatistic)
        {
            string fieldStr = string.Empty;
            string groupStr = string.Empty;
            string where = "1=1 AND is_deleted <> '1' ";
            int ordial = 0;
            List<ParamValue> paraList = new List<ParamValue>();

            foreach (IGrouping<string, SearchCondition> group in searchCondition.GroupBy(x => x.col_name))
            {
                if (group.Count() > 1)
                {
                    string same = "AND (1=1 ";
                    foreach (SearchCondition sc in group)
                    {
                        if (sc.col_condition.ToLower().Contains("like"))
                            same += sc.col_andor + " " + sc.col_name + " " + sc.col_condition + " '%'+" + "@param" + ordial + "+'%' ";
                        else
                            same += sc.col_andor + " " + sc.col_name + " " + sc.col_condition + "@param" + ordial;
                        ParamValue pv = new ParamValue();
                        pv.Val = sc.col_value;
                        paraList.Add(pv);
                        ordial++;
                    }
                    same += ")";
                    where += same;
                }
                else
                {
                    SearchCondition sc = group.ElementAt(0);
                    if (sc.col_condition.ToLower().Contains("like"))
                        where += sc.col_andor + " " + sc.col_name + " " + sc.col_condition + " '%'+" + "@param" + ordial + "+'%' ";
                    else
                        where += sc.col_andor + " " + sc.col_name + " " + sc.col_condition + "@param" + ordial;
                    ParamValue pv = new ParamValue();
                    pv.Val = sc.col_value;
                    paraList.Add(pv);
                    ordial++;
                }
            }

            for (int m = 0; m < statisticFields.Count; m++)
            {
                if (m == 0)
                    fieldStr = statisticFields[m].col_name;
                else
                    fieldStr += "+'-'+" + statisticFields[m].col_name;
            }

            for (int m = 0; m < groupFields.Count; m++)
            {
                if (m == 0)
                    groupStr = groupFields[m].col_name;
                else
                    groupStr += "," + groupFields[m].col_name;
            }

            //string sql = "SELECT COUNT(DISTINCT " + fieldStr + ") AS ct," + groupStr + " FROM " + table + " WHERE " + where + " GROUP BY " + groupStr + " ORDER BY ct ASC";
            SqlParameter[] param = new SqlParameter[paraList.Count];
            for (int k = 0; k < paraList.Count; k++)
                param[k] = SqlHelper.MakeInParam("param" + k, paraList[k].Val);

            string outSql = "";
            string numStr = "";
            if (isNumStatistic)
            {
                if (!isSum)//显示去重后的数目
                    numStr = "COUNT(DISTINCT " + fieldStr + ") AS ct,";
                else
                    numStr = "SUM(CASE (PATINDEX('%[^0-9]%', " + fieldStr + ")) WHEN 0  THEN CAST(" + fieldStr + " AS INT)  ELSE 0 END) AS ct,";
                outSql = "  SELECT " + numStr + groupStr + " FROM " + table + " WHERE " + where + " GROUP BY " + groupStr + " ORDER BY " + groupStr + " ASC";
            }
            else
            {
                numStr = fieldStr + " AS ct,";//不统计数量，仅统计去重后的字段信息
                string numStr2 = fieldStr + ",";
                outSql = "  SELECT " + numStr + groupStr + " FROM " + table + " WHERE " + where + " GROUP BY " + numStr2 + groupStr + " ORDER BY " + groupStr + " ASC";
            }
            
            string codeName = table;
            string fields = fieldStr;
            string sort = groupStr + " DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable dt = PagerUtils.GetPagedDataTable(codeName, fields, where, sort, groupStr, pageIndex, pageSize, param, ref pageCount, ref recordCount, isSum, isNumStatistic);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { total = recordCount, rows = dt, sql = outSql, prm = paraList });
        }

        //通过NPOI导出数据到EXCEL文件
        public IActionResult ExptDtToExcl(string id, string userid, string other)
        {
            string fileName = id;
            string list = HttpUtility.UrlDecode(Base64Decode(userid));
            List<ParamValue> paraList = JsonConvert.DeserializeObject<List<ParamValue>>(list);
            string sql = HttpUtility.UrlDecode(Base64Decode(other));

            SqlParameter[] param = new SqlParameter[paraList.Count];
            for (int k = 0; k < paraList.Count; k++)
                param[k] = SqlHelper.MakeInParam("param" + k, paraList[k].Val);

            DataTable dt = SqlHelper.GetDataTable(sql, param);
            byte[] bts = ExcelHelper.GetExcelByDataTable(dt);
            FileContentResult fResult = File(bts, "application/vnd.ms-excel", fileName + DateTime.Now.ToString("yyyy_MM_dd_hh_ss_mm") + ".xls");
            dt.Dispose();
            return fResult;
        }

        //查询所有统计模板
        public IActionResult GetStcTemplates(int pageSize, int pageIndex, int stcType)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string codeName = "t_statistic_template";
            string innerjoinTable = "t_config_codes";
            string uniqueKey = "Unique_code";
            string foreignKey = "statistic_type";
            string fields = "T1.Unique_code,T1.template_name,T1.created_date,T1.search_condition,T1.statistic_fields,T1.group_fields,T1.is_numstc,T1.is_sum,T2.code_name";
            string where = " 1=1 AND T1.statistic_type=" + stcType;
            string sort = "T1.Unique_code DESC";
            int pageCount = 0;
            int recordCount = 0;
            DataTable table = PagerUtils.GetPagedDataTable(codeName, innerjoinTable, foreignKey, uniqueKey, fields, where, sort, pageIndex, pageSize, ref pageCount, ref recordCount);
            return Json(new { total = recordCount, rows = table });
        }

        public IActionResult AddStcTemplData(string stcType, string templName, List<SearchCondition> searchCondition, List<StatisticFields> statisticFields, List<GroupFields> groupFields, bool isSum, bool isNumStatistic)
        {
            string std = JsonConvert.SerializeObject(searchCondition);
            string sf = JsonConvert.SerializeObject(statisticFields);
            string gf = JsonConvert.SerializeObject(groupFields);

            string sql = "INSERT INTO t_statistic_template(statistic_type,template_name,search_condition,statistic_fields,group_fields,created_date,is_numstc,is_sum) VALUES(@statistic_type,@template_name,@search_condition,@statistic_fields,@group_fields,@created_date,@is_numstc,@is_sum)";
            SqlParameter para1 = SqlHelper.MakeInParam("statistic_type", stcType);
            SqlParameter para2 = SqlHelper.MakeInParam("template_name", templName);
            SqlParameter para3 = SqlHelper.MakeInParam("search_condition", std);
            SqlParameter para4 = SqlHelper.MakeInParam("statistic_fields", sf);
            SqlParameter para5 = SqlHelper.MakeInParam("group_fields", gf);
            SqlParameter para6 = SqlHelper.MakeInParam("created_date", DateTime.Now.ToString("yyyy-MM-dd hh:ss:mm"));
            SqlParameter para7 = SqlHelper.MakeInParam("is_numstc", isNumStatistic);
            SqlParameter para8 = SqlHelper.MakeInParam("is_sum", isSum);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6, para7, para8 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult DeleteTempl(int uniquecode)
        {
            string sql = "DELETE FROM t_statistic_template WHERE Unique_code=" + uniquecode;
            int result = SqlHelper.ExecNonQuery(sql);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult GetStcTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value,order_id FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='TJLX')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetEncoderStr(string toEncode)
        {
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string result = Base64Encode(toEncode);
            return Json(new { rst = result });
        }

        /// <summary>
        /// 将字符串转换成base64格式,使用UTF8字符集
        /// </summary>
        /// <param name="content">加密内容</param>
        /// <returns></returns>
        public string Base64Encode(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// 将base64格式，转换utf8
        /// </summary>
        /// <param name="content">解密内容</param>
        /// <returns></returns>
        public string Base64Decode(string content)
        {
            byte[] bytes = Convert.FromBase64String(content);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
