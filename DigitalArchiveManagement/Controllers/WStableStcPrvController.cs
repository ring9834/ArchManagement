using ArchiveFileManagementNs.Models;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArchiveFileManagementNs.Controllers
{
    public class WStableStcPrvController : WBaseController
    {
        public IActionResult Index(string id, string userid)
        {
            ViewData["table"] = id;
            ViewData["userid"] = userid;
            return View();
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
                    sql = "SELECT COUNT(Unique_code) AS cnt FROM " + row["code"].ToString() + " WHERE store_type='2'";
                    object rlt = SqlHelper.ExecuteScalar(sql, null);
                    List<int> srs = new List<int>();
                    srs.Add(int.Parse(rlt.ToString()));
                    cd.Series.Add(srs);
                }
                cd.Years.Add(DateTime.Now.ToString("yyyy-MM-dd"));
            });
            return Json(cd);
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
                sql = "SELECT COUNT(Unique_code) cnt," + obj.ToString() + " yr FROM " + table + " WHERE store_type='2' GROUP BY " + obj.ToString() + " ORDER BY " + obj.ToString();
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
