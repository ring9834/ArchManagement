using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System.Data;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WBDataDictController : WBaseController
    {
        public IActionResult Index(string id)
        {
            ViewData["table"] = id;
            return View();
        }

        public IActionResult AddBusDictView()
        {
            return View("AddBusDict");
        }

        public IActionResult UpdateBusDictView()
        {
            return View("UpdateBusDict");
        }


        public IActionResult GetDataTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='SJLX')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult LoadBusDictionary(string tableName)
        {
            string fieldName = "col_name,show_name,col_datatype,col_maxlen,col_null,col_default,field_type,comments,Unique_code";
            //string fieldNameWithComment = "col_name AS '列名',show_name AS '显示名',col_datatype AS '数据类型',col_maxlen '最大长度',col_null AS '可为空?',col_default '默认值',col_use AS '可用?',query_flag AS '可查?',field_type AS '字段类型',comments AS '备注',Unique_code";
            string sql = "SELECT " + fieldName + " FROM t_config_col_dict where code='" + tableName + "'";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult InsertBusDict(string col_name, string show_name, string col_datatype, string col_datatype_code, string col_maxlen, string col_null, string col_default, string field_type, string comments, string code)
        {
            int result = 0;
            string title = "";
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (col_name.ToLower().Contains("delete") || col_name.ToLower().Contains("update") || col_name.ToLower().Contains("insert") || col_name.ToLower().Contains("exec"))
            {
                title = "列名中包含非法命令，修改终止！";
                return Json(new { rst = result, info = title });
            }

            string sql = "SELECT COUNT(b.name) FROM sysobjects a INNER JOIN syscolumns b \r\n";
            sql += "ON a.id=b.id AND a.xtype='U' \r\n";
            sql += "WHERE a.name='" + code + "' AND b.name ='" + col_name + "'\r\n";
            object rowCount = SqlHelper.ExecuteScalar(sql);
                                   
            if (int.Parse(rowCount.ToString()) > 0)
            {
                title = "字段已存在，增加失败！";
                return Json(new { rst = result, info = title });
            }

            string numbericCol_zero = col_datatype_code.ToLower().Equals("numeric") ? ",0" : "";
            string canbeNull = int.Parse(col_null) == 1 ? " NULL" : " NOT NULL";
            sql = "ALTER TABLE " + code + " ADD " + col_name + " " + col_datatype_code + "(" + col_maxlen + numbericCol_zero + ") " + canbeNull;
            SqlHelper.ExecNonQuery(sql);

            sql = "INSERT INTO t_config_col_dict(col_name,show_name,col_datatype,col_maxlen,col_null,col_default,field_type,comments,code) \r\n";
            sql += "VALUES(@col_name,@show_name,@col_datatype,@col_maxlen,@col_null,@col_default,@field_type,@comments,@code) \r\n";

            SqlParameter para1 = SqlHelper.MakeInParam("col_name", col_name);
            SqlParameter para2 = SqlHelper.MakeInParam("show_name", show_name);
            SqlParameter para3 = SqlHelper.MakeInParam("col_datatype", col_datatype);
            SqlParameter para4 = SqlHelper.MakeInParam("col_maxlen", col_maxlen);
            SqlParameter para5 = SqlHelper.MakeInParam("col_null", col_null == "1" ? true : false);
            SqlParameter para6 = SqlHelper.MakeInParam("field_type", field_type == "1" ? true : false);
            SqlParameter para7 = SqlHelper.MakeInParam("col_default", col_default == null ? string.Empty : col_default);
            SqlParameter para8 = SqlHelper.MakeInParam("comments", comments == null ? string.Empty : comments);
            SqlParameter para9 = SqlHelper.MakeInParam("code", code);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6, para7, para8, para9 };
            result = SqlHelper.ExecNonQuery(sql, param);
            title = "业务字典记录增加成功！";
            return Json(new { rst = result, info = title });
        }

        public IActionResult UpdateBusDict(string col_name, string col_name_origal, string show_name, string col_datatype, string col_datatype_code, string col_maxlen, string col_null, string col_default, string field_type, string comments, string tableName, int unique_code)
        {
            int result = 0;
            string title = string.Empty;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = string.Empty;
            string numbericCol_zero = col_datatype_code.ToLower().Equals("numeric") ? ",0" : "";
            int c = int.Parse(col_null);
            string canbeNull = c == 1 ? " NULL" : " NOT NULL";

            if (col_name.ToLower().Contains("delete") || col_name.ToLower().Contains("update") || col_name.ToLower().Contains("insert") || col_name.ToLower().Contains("exec"))
            {
                title = "列名中包含非法命令，修改终止！";
                return Json(new { rst = result, info = title });
            }

            if (!col_name.Equals(col_name_origal))//如果列名已变 2020年3月17日
            {
                //已经存在相同的列名，故不允许修改 
                sql = "IF EXISTS (SELECT 1 FROM SYSOBJECTS T1 INNER JOIN SYSCOLUMNS T2 ON T1.ID=T2.ID WHERE T1.NAME=@table AND T2.NAME=@column) \r\n";
                sql += "BEGIN \r\n";
                sql += "   SELECT 1 \r\n";
                sql += "END \r\n";
                sql += "ELSE \r\n";
                sql += "BEGIN \r\n";
                sql += "   SELECT 0 \r\n";
                sql += "END";
                SqlParameter paraa = SqlHelper.MakeInParam("column", col_name);
                SqlParameter parab = SqlHelper.MakeInParam("table", tableName);
                SqlParameter[] param0 = new SqlParameter[] { paraa, parab };
                object numObj = SqlHelper.ExecuteScalar(sql, param0);
                if (int.Parse(numObj.ToString()) > 0)
                {
                    if (int.Parse(numObj.ToString()) > 0)
                    {
                        title = "同样的列名已经存在，请另换它名后继续！";
                        return Json(new { rst = result, info = title });
                    }
                }
                else
                {
                    //sql = "EXEC sp_rename '@table.[@column]', '@newColumn' , 'COLUMN' ";//列名修改为新列名
                    //SqlParameter para_1 = SqlHelper.MakeInParam("table", tableName);
                    //SqlParameter para_2 = SqlHelper.MakeInParam("column", col_name_origal);
                    //SqlParameter para_3 = SqlHelper.MakeInParam("newColumn", col_name);
                    //SqlParameter[] prm = new SqlParameter[] { para_1, para_2, para_3 };
                    //SqlHelper.ExecNonQuery(sql, prm);
                    sql = "EXEC sp_rename '" + tableName + ".[" + col_name_origal + "]', '" + col_name + "' , 'COLUMN' ";//列名修改为新列名
                    SqlHelper.ExecNonQuery(sql, null);
                }
            }

            if (col_datatype_code.Contains("num"))//如果字符型字段中已存在非数字型的记录，则不能从字符型修改为数字型。 2020年3月17日
            {
                sql = "SELECT COUNT(Unique_code) FROM " + tableName + " WHERE ISNUMERIC("+ col_name +") =0 ";
                SqlParameter paraa = SqlHelper.MakeInParam("column", col_name);
                //SqlParameter parab = SqlHelper.MakeInParam("table", tableName);
                //SqlParameter[] param0 = new SqlParameter[] { paraa };
                //object numObj = SqlHelper.ExecuteScalar(sql, param0);
                object numObj = SqlHelper.ExecuteScalar(sql, null);
                if (int.Parse(numObj.ToString()) > 0)
                {
                    title = "此列中已存在非数字型的记录，故不能从字符型修改为数字型！";
                    return Json(new { rst = result, info = title });
                }
            }

            if (c == 1)
            {
                sql = "ALTER TABLE " + tableName + " ALTER COLUMN " + col_name + " " + col_datatype_code + "(" + col_maxlen + numbericCol_zero + ") " + canbeNull;
                SqlHelper.ExecNonQuery(sql);
            }
            else
            {
                sql = "SELECT COUNT(*) FROM " + tableName + " WHERE " + col_name + " is null";
                object r = SqlHelper.ExecuteScalar(sql);
                if (int.Parse(r.ToString()) > 0)
                {
                    title = col_name + "列中已存在空值的记录，故此列不能改为非空类型！";
                    return Json(new { rst = result, info = title });
                }
                sql = "ALTER TABLE " + tableName + " ALTER COLUMN " + col_name + " " + col_datatype_code + "(" + col_maxlen + numbericCol_zero + ") " + canbeNull;
                SqlHelper.ExecNonQuery(sql);
            }

            sql = "UPDATE t_config_col_dict SET col_name=@col_name,show_name=@show_name,col_datatype=@col_datatype,col_maxlen=@col_maxlen,col_null=@col_null, \r\n";
            sql += "field_type=@field_type,col_default=@col_default,comments=@comments \r\n";
            sql += "WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("col_name", col_name);
            SqlParameter para2 = SqlHelper.MakeInParam("show_name", show_name);
            SqlParameter para3 = SqlHelper.MakeInParam("col_datatype", col_datatype);
            SqlParameter para4 = SqlHelper.MakeInParam("col_maxlen", col_maxlen);
            SqlParameter para5 = SqlHelper.MakeInParam("col_null", col_null == "1" ? true : false);
            SqlParameter para6 = SqlHelper.MakeInParam("field_type", field_type == "1" ? true : false);
            SqlParameter para7 = SqlHelper.MakeInParam("col_default", col_default == null ? string.Empty : col_default);//应该作出限制：当col_datatype为数字型时，default不能是数字之外的值
            SqlParameter para8 = SqlHelper.MakeInParam("comments", comments == null ? string.Empty : comments);
            SqlParameter para9 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6, para7, para8, para9 };
            result = SqlHelper.ExecNonQuery(sql, param);
            title = "业务字段信息更新成功！";
            return Json(new { rst = result, info = title });
        }

        public IActionResult DeleteBusDict(int unique_code, string tableName, string colName)
        {
            string sql = "DELETE FROM t_config_col_dict WHERE Unique_code=@Unique_code \r\n";
            sql += "ALTER TABLE " + tableName + " DROP COLUMN " + colName;
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });

        }

        public IActionResult VerifyIfBusDictConfigured(string tableName)
        {
            string sql = "SELECT COUNT(*) FROM  t_config_col_dict WHERE code='" + tableName + "'";
            object count = SqlHelper.ExecuteScalar(sql);
            bool result = int.Parse(count.ToString()) == 0 ? false : true;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult InitiateBusDbTabStruct(string tableName)
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN (SELECT Unique_code FROM t_config_codes_base WHERE code_key='SJLX')";
            DataTable dtCodes = SqlHelper.GetDataTable(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();

            bool result = false;
            if (dtCodes.Rows.Count == 0)
            {

                string warning = "编码配置中还没有“数据类型”方面的配置（字符型（nvarchar）、数据型(numberic)），请配置后继续！";
                return Json(new { rst = result, info = warning });
            }

            sql = "SELECT * FROM t_config_col_dict_base";
            DataTable dtColbase = SqlHelper.GetDataTable(sql, null);
            DataTable dtColDict = dtColbase.Copy();//克隆
            dtColDict.TableName = "t_config_col_dict";
            dtColDict.Columns.Add("code", typeof(string));


            string colCreating = "CREATE TABLE [" + tableName + "]( \r\n";
            colCreating += "[Unique_code] [numeric](18, 0) IDENTITY(1,1) NOT NULL,\r\n";
            for (int i = 0; i < dtColbase.Rows.Count; i++)
            {
                DataRow[] drs = dtCodes.Select("Unique_code=" + dtColbase.Rows[i]["col_datatype"].ToString());
                if (drs.Length > 0)
                {
                    string colNull = dtColbase.Rows[i]["col_null"].ToString().ToLower().Equals("true") ? "NULL" : "NOT NULL";
                    string numbericCol_zero = drs[0]["code_value"].ToString().ToLower().Equals("numeric") ? ",0" : "";
                    string colInfo = "[" + dtColbase.Rows[i]["col_name"].ToString() + "] [" + drs[0]["code_value"].ToString() + "](" + dtColbase.Rows[i]["col_maxlen"].ToString() + numbericCol_zero + ") " + colNull + ",\r\n";
                    colCreating += colInfo;
                }
                dtColDict.Rows[i]["code"] = tableName;
            }

            //check_status字段又被去掉，2020年3月9日
            //导入目录批次号、原文字段;2020年2月29加入了check_status字段，表示记录的审核状态；2020年3月1日，store_type默认值改为'1'(表示已入库即在预归档库)
            //增加了is_deleted字段于2020年3月11日 is_deleted NVARCHAR(1) DEFAULT '0'
            colCreating += "import_bundle NVARCHAR(50) NULL,store_type NVARCHAR(1) DEFAULT '1',yw NVARCHAR(MAX) NULL,yw_xml XML NULL,is_deleted NVARCHAR(1) DEFAULT '0',";
            colCreating += " CONSTRAINT [PK_t_" + tableName + "] PRIMARY KEY CLUSTERED \r\n";
            colCreating += "([Unique_code] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] \r\n";
            colCreating += ") ON [PRIMARY]";

            int r1 = SqlHelper.ExecNonQuery(colCreating);//创建档案业务类型表
            int r2 = SqlHelper.InsertByBulk(dtColDict, dtColDict.TableName);//将t_config_col_dict_base中的数据，拷贝到t_config_col_dict表
            result = r2 > 0 ? true : false;
            string inf = string.Empty;
            if (result)
                inf = "数据库数据结构初始化成功！";
            else
                inf = "数据库数据结构初始化失败！请联系系统管理员。";
            dtCodes.Dispose();
            dtColDict.Dispose();
            dtColbase.Dispose();
            return Json(new { rst = result, info = inf });
        }

        /// <summary>
        /// 判断字段是否已经存在
        /// </summary>
        /// <param name="table"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public IActionResult VerifyIfColExist(string table, string colName)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("Table", table);
            SqlParameter para2 = SqlHelper.MakeInParam("Column", colName);
            SqlParameter[] param = new SqlParameter[] { para1, para2 };
            string sql = "SELECT 1 FROM SYSOBJECTS T1 INNER JOIN SYSCOLUMNS T2 ON T1.ID=T2.ID WHERE T1.NAME=@Table AND T2.NAME=@Column";
            DataTable d = SqlHelper.GetDataTable(sql, param);
            return Json(d);
        }
    }
}
