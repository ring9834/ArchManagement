using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;
using NetCoreDbUtility;

namespace ArchiveFileManagementNs.Controllers
{
    public class WPreArchiveController : WBaseController
    {
        public IActionResult Index(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View();
        }

        public IActionResult YearFdView(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View("YearField");
        }

        public IActionResult GetDataTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='RKSHJL')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        /// <summary>
        /// 异步获取未审核、正审核、已审核待入库数据
        /// </summary>
        /// <param name="statuscode"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetCheckStatusNumber(string table)
        {
            List<FieldValuePair> list = new List<FieldValuePair>();
            //string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='RKSHJL')";
            //DataTable dt = SqlHelper.GetDataTable(sql, null);
            await Task.Run(() =>
            {
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    sql = "SELECT COUNT(Unique_code) AS cnt FROM " + table + " WHERE store_type='1' AND check_status='" + dt.Rows[i]["code_value"].ToString() + "'";
                //    object result = SqlHelper.ExecuteScalar(sql, null);
                //    FieldValuePair fvp = new FieldValuePair();
                //    fvp.Field = dt.Rows[i]["code_name"].ToString();
                //    fvp.Value = result.ToString();
                //    list.Add(fvp);
                //}
                string sql = "IF OBJECT_ID(N'" + table + "',N'U') IS NOT NULL \r\n";
                sql += "    SELECT COUNT(Unique_code) AS cnt FROM " + table + " WHERE store_type='1' \r\n";
                sql += "ELSE \r\n";
                sql += "   SELECT 0 ";
                object allObj = SqlHelper.ExecuteScalar(sql, null);
                int all = int.Parse(allObj.ToString());

                sql = "IF OBJECT_ID(N'" + table + "',N'U') IS NOT NULL \r\n";
                sql += "BEGIN \r\n";
                sql += "    IF(SELECT COUNT(Unique_code) FROM " + table + " WHERE store_type='1') > 0 \r\n";
                sql += "    BEGIN \r\n";
                sql += "        SELECT COUNT(DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)'))) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m)\r\n";
                sql += "        WHERE check_status='1' AND T.table_code='" + table + "' \r\n";
                sql += "    END \r\n";
                sql += "    ELSE \r\n";
                sql += "    BEGIN \r\n";
                sql += "        DELETE FROM t_auditing WHERE table_code='" + table + "' \r\n";//直接把有关table表的审核审批记录删除 20210105
                sql += "        SELECT 0 \r\n";
                sql += "    END \r\n";
                sql += "END \r\n";
                sql += "ELSE \r\n";
                sql += "   SELECT 0 ";
                object chckingObj = SqlHelper.ExecuteScalar(sql, null);
                int checking = int.Parse(chckingObj.ToString());
                FieldValuePair fvpChecking = new FieldValuePair();
                fvpChecking.Field = "正审核";
                fvpChecking.Value = checking.ToString();

                sql = "IF OBJECT_ID(N'" + table + "',N'U') IS NOT NULL \r\n";
                sql += "SELECT DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)')) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m) \r\n";
                sql += " WHERE check_status='2' AND T.table_code='" + table + "' \r\n";
                //sql += "ELSE \r\n";
                //sql += "   SELECT 0 ";
                DataTable dt = SqlHelper.GetDataTable(sql, null);
                int haveChecked = dt.Rows.Count;
                FieldValuePair fvpChecked = new FieldValuePair();
                fvpChecked.Field = "已审核待入库";
                fvpChecked.Value = haveChecked.ToString();

                sql = "IF OBJECT_ID(N'" + table + "',N'U') IS NOT NULL \r\n";
                sql += "SELECT DISTINCT(x.m.value('(@id)[1]','nvarchar(Max)')) FROM t_auditing AS T cross apply T.archive_dealt.nodes('ids/id') AS x(m) \r\n";
                sql += " WHERE check_status='3' AND T.table_code='" + table + "' \r\n";
                //sql += "ELSE \r\n";
                //sql += "   SELECT 0 ";
                dt = SqlHelper.GetDataTable(sql, null);
                int checkedButNotOver = dt.Rows.Count;//审核未通过
                FieldValuePair fvpCheckedBut = new FieldValuePair();
                fvpCheckedBut.Field = "未审核通过";
                fvpCheckedBut.Value = checkedButNotOver.ToString();

                int noCheck = all - checking - haveChecked - checkedButNotOver;
                FieldValuePair fvpNoCheck = new FieldValuePair();
                fvpNoCheck.Field = "未审核";
                fvpNoCheck.Value = noCheck.ToString();
                list.Add(fvpNoCheck);//未审核
                list.Add(fvpChecking);//正审核
                list.Add(fvpChecked);//已审核待入库
                list.Add(fvpCheckedBut);//未审核通过 added on 20201212

                FieldValuePair fvp2 = new FieldValuePair();
                fvp2.Field = "预归档总数";
                fvp2.Value = all.ToString();
                list.Add(fvp2);
                dt.Dispose();
            });
            return Json(list);
        }

        public async Task<IActionResult> GetCheckStatusChart(string table)
        {
            ChartData cd = new ChartData();
            cd.Legends = new List<string>();
            cd.Years = new List<string>();
            cd.Series = new List<List<int>>();

            string sql = "SELECT name,code FROM t_config_type_tree WHERE super_id IN(SELECT super_id FROM t_config_type_tree WHERE code='" + table + "')";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            await Task.Run(() =>
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    cd.Legends.Add(row["name"].ToString());
                    sql = "SELECT COUNT(Unique_code) AS cnt FROM " + row["code"].ToString() + " WHERE store_type='1'";
                    object rlt = SqlHelper.ExecuteScalar(sql, null);
                    List<int> srs = new List<int>();
                    srs.Add(int.Parse(rlt.ToString()));
                    cd.Series.Add(srs);
                }
                cd.Years.Add(DateTime.Now.ToString("yyyy-MM-dd"));
            });
            return Json(cd);
        }

        public IActionResult GetFieldsByTableName(string id)
        {
            string sql = "SELECT Unique_code,col_name,show_name FROM t_config_col_dict WHERE code='" + id + "' AND field_type = 0";
            DataTable table = SqlHelper.GetDataTable(sql);
            return Json(table);
        }

        public IActionResult SaveYearFld(string table, string fld)
        {
            string sql = "";
            sql += "IF (SELECT COUNT(*) FROM t_pre_arch_year WHERE table_name='" + table + "') = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "INSERT t_pre_arch_year(year_field,table_name)\r\n";
            sql += "VALUES('" + fld + "','" + table + "') \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "UPDATE  t_pre_arch_year SET year_field='" + fld + "' WHERE table_name='" + table + "'\r\n";
            sql += "END \r\n";
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string title = string.Empty;
            return Json(new { rst = result });
        }

        public IActionResult GetYearFldInfo(string table)
        {
            string sql = "SELECT year_field FROM t_pre_arch_year WHERE table_name='" + table + "'";
            object obj = SqlHelper.ExecuteScalar(sql, null);
            int result = 0;
            string fld = string.Empty;
            if (obj != null && obj != DBNull.Value && obj.ToString() != "")
            {
                result = 1;
                fld = obj.ToString();
            }
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result, field = fld });
        }

        public async Task<IActionResult> GetTotalAndPerYear(string table)
        {
            ChartData2 cd = new ChartData2();
            cd.Legends = new List<string>();
            cd.Amounts = new List<int>();
            int result = 0;
            string title = string.Empty;
            //JsonSerializerSettings setting = new JsonSerializerSettings();

            string sql = "SELECT year_field FROM t_pre_arch_year WHERE table_name='" + table + "'";
            object obj = SqlHelper.ExecuteScalar(sql, null);

            if (obj == null || obj == DBNull.Value || obj.ToString() == "")
            {
                result = 0;
                title = "此类型档案库还未设置“年度”字段，请到“管理配置”的“预归档字段配置”中进行设置！";
                return Json(new { rst = result, title = title });
            }
            await Task.Run(() =>
            {
                sql = "SELECT COUNT(Unique_code) cnt," + obj.ToString() + " yr FROM " + table + " WHERE store_type='1' GROUP BY " + obj.ToString() + " ORDER BY " + obj.ToString();
                DataTable dt = SqlHelper.GetDataTable(sql, null);
                int total = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cd.Legends.Add(dt.Rows[i]["yr"].ToString());
                    int yearCount = int.Parse(dt.Rows[i]["cnt"].ToString());
                    cd.Amounts.Add(yearCount);
                    total += yearCount;
                }
                cd.Legends.Insert(0, "总数");
                cd.Amounts.Insert(0, total);
                dt.Dispose();

                result = 1;
                title = "统计成功！";
            });
            return Json(new { rst = result, title = title, charData = cd });
        }
    }
}
