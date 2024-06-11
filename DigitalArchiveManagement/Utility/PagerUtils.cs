using NetCoreDbUtility;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs
{
    public class PagerUtils
    {
        public static DataTable GetPagedDataTable(string tableStr, string fieldStr, string whereStr, string sortStr, int pageIndex, int pageSize, ref int pageCout, ref int recordCount)
        {
            string sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "  SELECT * FROM (SELECT ROW_NUMBER() \r\n";
            sql += "  OVER(ORDER BY " + sortStr + ") AS rownum, " + fieldStr + " FROM " + tableStr + " WHERE " + whereStr + " ) AS DWHERE \r\n";
            sql += "  WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "  AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20)) \r\n";
            sql += "END \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "  SELECT COUNT(1) FROM " + tableStr + " WHERE " + whereStr;
            object rc = SqlHelper.ExecuteScalar(sql);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            if (dt.Columns.Count > 0)
                dt.Columns.RemoveAt(0);
            return dt;
        }

        public static DataTable GetPagedDataTable(string tableStr, string fieldStr, string whereStr, string sortStr, int pageIndex, int pageSize, SqlParameter[] pms, ref int pageCout, ref int recordCount)
        {
            string sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "  SELECT * FROM (SELECT ROW_NUMBER() \r\n";
            sql += "  OVER(ORDER BY " + sortStr + ") AS rownum, " + fieldStr + " FROM " + tableStr + " WHERE " + whereStr + " ) AS DWHERE \r\n";
            sql += "  WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "  AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20)) \r\n";
            sql += "END \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, pms);

            sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "  SELECT COUNT(1) FROM " + tableStr + " WHERE " + whereStr;
            object rc = SqlHelper.ExecuteScalar(sql, pms);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            if (dt.Columns.Count > 0)
                dt.Columns.RemoveAt(0);
            return dt;
        }

        public static DataTable GetPagedDataTable(string tableStr, string fieldStr, string whereStr, string sortStr, List<string> whereFieldArray, List<string> whereFieldValueArray, int pageIndex, int pageSize, ref int pageCout, ref int recordCount)
        {
            string sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "  SELECT * FROM (SELECT ROW_NUMBER() \r\n";
            sql += "  OVER(ORDER BY " + sortStr + ") AS rownum, " + fieldStr + " FROM " + tableStr + " WHERE " + whereStr + " ) AS DWHERE \r\n";
            sql += "  WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "  AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20)) \r\n";
            sql += "END \r\n";
            SqlParameter[] param = new SqlParameter[whereFieldArray.Count];
            for (int i = 0; i < whereFieldArray.Count; i++)
            {
                param[i] = SqlHelper.MakeInParam(whereFieldArray[i], whereFieldValueArray[i]);
            }
            DataTable dt = SqlHelper.GetDataTable(sql, param);

            sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "  SELECT COUNT(1) FROM " + tableStr + " WHERE " + whereStr;
            object rc = SqlHelper.ExecuteScalar(sql, param);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            if (dt.Columns.Count > 0)
                dt.Columns.RemoveAt(0);
            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableStr"></param>
        /// <param name="fieldStr"></param>
        /// <param name="whereStr"></param>
        /// <param name="sortStr"></param>
        /// <param name="groupStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pms"></param>
        /// <param name="pageCout"></param>
        /// <param name="recordCount"></param>
        /// <param name="isSum">显示总和，还是显示去重后的数目</param>
        /// <returns></returns>
        public static DataTable GetPagedDataTable(string tableStr, string fieldStr, string whereStr, string sortStr, string groupStr, int pageIndex, int pageSize, SqlParameter[] pms, ref int pageCout, ref int recordCount, bool isSum, bool isNumStatistic)
        {
            string numStr = "";
            string groupStr2 = "";
            if (isNumStatistic)
            {
                if (!isSum)//显示去重后的数目
                    numStr = "COUNT(DISTINCT " + fieldStr + ") AS ct,";
                else
                    numStr = "SUM(CASE (PATINDEX('%[^0-9]%', " + fieldStr + ")) WHEN 0  THEN CAST(" + fieldStr + " AS INT)  ELSE 0 END) AS ct,";
                groupStr2 = groupStr;
            }
            else
            {
                numStr = fieldStr + " AS ct,";//不统计数量，仅统计去重后的字段信息
                groupStr2 = fieldStr + "," + groupStr;
            }

            string sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "  SELECT * FROM (SELECT ROW_NUMBER() \r\n";
            sql += "  OVER(ORDER BY " + sortStr + ") AS rownum," + numStr + groupStr + " FROM " + tableStr + " WHERE " + whereStr + " GROUP BY " + groupStr2 + ") AS DWHERE \r\n";
            sql += "  WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "  AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20)) \r\n";
            sql += "END \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, pms);

            sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "  SELECT COUNT(1) FROM (SELECT " + numStr + groupStr + " FROM " + tableStr + " WHERE " + whereStr + " GROUP BY " + groupStr2 + ") AS T";
            object rc = SqlHelper.ExecuteScalar(sql, pms);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            if (dt.Columns.Count > 0)
                dt.Columns.RemoveAt(0);
            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableStr"></param>
        /// <param name="innerJoinTable"></param>
        /// <param name="foreignKey">第一个表的外键</param>
        /// <param name="uniqueKey">第二个表的主键</param>
        /// <param name="fieldStr"></param>
        /// <param name="whereStr"></param>
        /// <param name="sortStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCout"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public static DataTable GetPagedDataTable(string tableStr, string innerJoinTable, string foreignKey, string uniqueKey, string fieldStr, string whereStr, string sortStr, int pageIndex, int pageSize, ref int pageCout, ref int recordCount)
        {
            string innerJoin = " AS T1 INNER JOIN " + innerJoinTable + " AS T2 ON T1." + foreignKey + "=t2." + uniqueKey;
            string sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "  SELECT * FROM (SELECT ROW_NUMBER() \r\n";
            sql += "  OVER(ORDER BY " + sortStr + ") AS rownum, " + fieldStr + " FROM " + tableStr + innerJoin + " WHERE " + whereStr + " ) AS DWHERE \r\n";
            sql += "  WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "  AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20)) \r\n";
            sql += "END \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            sql = "IF OBJECT_ID(N'" + tableStr + "',N'U') IS NOT NULL \r\n";
            sql += "  SELECT COUNT(1) FROM " + tableStr + innerJoin + " WHERE " + whereStr;
            object rc = SqlHelper.ExecuteScalar(sql);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            if (dt.Columns.Count > 0)
                dt.Columns.RemoveAt(0);
            return dt;
        }

        /// <summary>
        /// innerJoinTable 需要直接在调用此函数前，把innerJoinTable的字符串做好，可以是多个INNER JOIN。
        /// 2020年3月10日，用到了入库审核记录列表显示中。
        /// </summary>
        /// <param name="tableStr"></param>
        /// <param name="innerJoinTable"></param>
        /// <param name="fieldStr"></param>
        /// <param name="whereStr"></param>
        /// <param name="sortStr"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCout"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public static DataTable GetPagedDataTable(string tableStr, string innerJoinTable, string fieldStr, string whereStr, string sortStr, int pageIndex, int pageSize, ref int pageCout, ref int recordCount)
        {
            string innerJoin = innerJoinTable;
            string sql = "  SELECT * FROM (SELECT ROW_NUMBER() \r\n";
            sql += "  OVER(ORDER BY " + sortStr + ") AS rownum, " + fieldStr + " FROM " + tableStr + innerJoin + " WHERE " + whereStr + " ) AS DWHERE \r\n";
            sql += "  WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "  AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20)) \r\n";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            sql = "  SELECT COUNT(1) FROM " + tableStr + innerJoin + " WHERE " + whereStr;
            object rc = SqlHelper.ExecuteScalar(sql);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            return dt;
        }

        public static DataTable GetUnionPagedDataTable(string table1, string table2, string innerJoin1, string innerJoin2, string fieldStr1, string fieldStr2, string whereStr1, string whereStr2, string sortStr, int pageIndex, int pageSize, ref int pageCout, ref int recordCount)
        {
            string sql = "SELECT * FROM ( \r\n";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY application_time DESC) AS rownum,* \r\n";
            sql += "FROM( \r\n";
            sql += "    SELECT " + fieldStr1 + " \r\n";
            sql += "    FROM " + table1 + " \r\n";
            sql += "    " + innerJoin1 + " \r\n";
            sql += "    WHERE " + whereStr1 + " \r\n";
            sql += "  UNION \r\n";
            sql += "    SELECT " + fieldStr2 + " \r\n";
            sql += "    FROM " + table2 + " \r\n";
            sql += "    " + innerJoin2 + " \r\n";
            sql += "    WHERE " + whereStr2 + " \r\n";
            sql += "  ) AS TALL \r\n";
            sql += ") AS DWHERE \r\n";
            sql += "WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20))";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            sql = "SELECT SUM(cnt) FROM( \r\n";
            sql += "  SELECT COUNT(1) AS cnt FROM " + table1 + innerJoin1 + " WHERE " + whereStr1 + " \r\n";
            sql += "  UNION \r\n";
            sql += "  SELECT COUNT(1) AS cnt FROM " + table2 + innerJoin2 + " WHERE " + whereStr2 + " \r\n";
            sql += ") AS TB";
            object rc = SqlHelper.ExecuteScalar(sql);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            return dt;
        }

        public static DataTable GetUnionListPagedDataTable(List<string> tables, List<string> joins, List<string> fields, List<string> wheres, string sortStr, int pageIndex, int pageSize, ref int pageCout, ref int recordCount)
        {
            string sql = "SELECT * FROM ( \r\n";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY application_time DESC) AS rownum,* \r\n";
            sql += "FROM( \r\n";

            for (int i = 0; i < tables.Count; i++)
            {
                if (i == 0)
                {
                    sql += "    SELECT " + fields[i] + " \r\n";
                    sql += "    FROM " + tables[i] + " \r\n";
                    sql += "    " + joins[i] + " \r\n";
                    sql += "    WHERE " + wheres[i] + " \r\n";
                }
                else
                {
                    sql += "  UNION \r\n";
                    sql += "    SELECT " + fields[i] + " \r\n";
                    sql += "    FROM " + tables[i] + " \r\n";
                    sql += "    " + joins[i] + " \r\n";
                    sql += "    WHERE " + wheres[i] + " \r\n";
                }
            }
            sql += "  ) AS TALL \r\n";
            sql += ") AS DWHERE \r\n";
            sql += "WHERE rownum BETWEEN CAST((" + pageIndex + "*" + pageSize + " + 1) as nvarchar(20)) \r\n";
            sql += "AND cast(((" + pageIndex + "+1)*" + pageSize + ") as nvarchar(20))";
            DataTable dt = SqlHelper.GetDataTable(sql, null);

            sql = "SELECT SUM(cnt) FROM( \r\n";

            for (int i = 0; i < tables.Count; i++)
            {
                if (i == 0)
                {
                    sql += "  SELECT COUNT(1) AS cnt FROM " + tables[i] + joins[i] + " WHERE " + wheres[i] + " \r\n";
                }
                else
                {
                    sql += "  UNION \r\n";
                    sql += "  SELECT COUNT(1) AS cnt FROM " + tables[i] + joins[i] + " WHERE " + wheres[i] + " \r\n";
                }
            }
            sql += ") AS TB";
            object rc = SqlHelper.ExecuteScalar(sql);
            if (rc != null) { recordCount = int.Parse(rc.ToString()); }
            else { recordCount = 0; }
            int mod = recordCount % pageSize;
            if (mod == 0) { pageCout = recordCount / pageSize; }
            else { pageCout = recordCount / pageSize + 1; }

            return dt;
        }
    }
}
