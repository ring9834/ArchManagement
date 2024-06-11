using NetCoreDbUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs
{
    public class SortService
    {
        public static string GetSortStringByTableName(string tableName)
        {
            string sql = "SELECT t2.Unique_code,t2.col_name,t3.code_value FROM t_config_sorted AS t1 \r\n";
            sql += "INNER JOIN t_config_col_dict AS t2 ON t1.selected_code=t2.Unique_code \r\n";
            sql += "LEFT JOIN t_config_codes AS t3 ON t1.sort_code=t3. Unique_code \r\n";
            sql += "WHERE t2.code='" + tableName + "' ORDER BY order_number ASC ";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            string sortString = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string sort = dr["code_value"] == DBNull.Value ? "ASC" : dr["code_value"].ToString();
                if (i == 0)
                    sortString += dr["col_name"].ToString() + " " + sort;
                else
                    sortString += "," + dr["col_name"].ToString() + " " + sort;
            }
            if (string.IsNullOrEmpty(sortString))//如果没有配置排序方式
            {
                sortString = "Unique_code ASC";
            }
            return sortString;
        }
    }
}
