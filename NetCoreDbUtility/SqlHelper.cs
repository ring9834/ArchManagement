using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace NetCoreDbUtility
{
    public static class SqlHelper
    {
        #region 获取连接字符串
        private static readonly string _ConnString = string.Empty;

        /// <summary>
                /// 静态构造函数设置SQLServer连接字符串
                /// </summary>
        static SqlHelper()
        {
            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            //builder.DataSource = "your_server.database.windows.net"; //your_server.database.windows.net //DESKTOP-ODCUQBA
            //builder.UserID = "sa"; //your_user
            //builder.Password = "123456"; //your_password
            //builder.InitialCatalog = "Test"; //your_database
            _ConnString = AppSetting.GetConfig("ConnectionStrings:DefaultConnection");
        }
        #endregion

        #region 执行非查询操作（添加、更新、删除）
        /// <summary>
                /// 执行SQL语句，返回影响的记录数
                /// Insert插入,Delete删除,Update更新
                /// </summary>
                /// <param name="cmdText">sql语句</param>
                /// <param name="sqlParams">[可选]sql参数化</param>
                /// <returns>int：受影响的行数</returns>
        public static int ExecNonQuery(string cmdText, params SqlParameter[] sqlParams)
        {
            int rowsCount = 0;
            using (SqlConnection conn = new SqlConnection(_ConnString))
            {
                OpenConnection(conn);
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //cmd.CommandType = CommandType.Text; //指定cmd命令类型为文本类型(默认，可不写);
                    cmd.CommandText = cmdText; //sql语句或存储过程
                    //检查参数组是否有数据
                    if (sqlParams != null && sqlParams.Length > 0)
                        cmd.Parameters.AddRange(sqlParams);
                    rowsCount = cmd.ExecuteNonQuery(); //执行非查询命令，接收受影响行数，大于0的话表示添加成功
                    cmd.Parameters.Clear();
                }
                CloseConnection(conn);
            }
            return rowsCount;
        }

        /// <summary>
                /// 批量插入数据，并返回受影响的行数
                /// </summary>
                /// <param name="dt">DataTable</param>
                /// <param name="dtName">表名称</param>
                /// <returns>int：受影响的行数</returns>
        public static int InsertByBulk(DataTable dt, string dtName)
        {
            int rowsCount = 0;
            using (SqlConnection conn = new SqlConnection(_ConnString))
            {
                OpenConnection(conn);
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                {
                    if (dt != null && dt.Rows.Count != 0)
                    {
                        bulkCopy.DestinationTableName = dtName; //数据表名称
                        foreach (DataColumn dtColumn in dt.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(dtColumn.ColumnName, dtColumn.ColumnName);
                        }
                        //bulkCopy.BulkCopyTimeout = 1000;// 设置超时限制
                        bulkCopy.BatchSize = dt.Rows.Count;
                        bulkCopy.WriteToServer(dt);
                        rowsCount = bulkCopy.BatchSize;
                    }
                }
                CloseConnection(conn);
            }
            return rowsCount;
        }

        /// <summary>
                /// 根据数据源（数据库表）动态构建DataTable类型对象，并填充slq参数化数据
                /// </summary>
                /// <param name="tabName">表名称</param>
                /// <param name="listParameters">sql参数化</param>
                /// <returns>DataTable</returns>
        public static DataTable CreateDataTable(string tabName, List<SqlParameter[]> listParameters)
        {
            #region 1.查询数据源（数据库表）
            string sqlQuery = $"select * from {tabName} where 1!=1";
            DataTable dt = GetDataTable(sqlQuery);
            #endregion

            #region 2.构建DataTable构架：根据数据源（数据库表）动态构建DataTable的构架
            DataTable myDt = new DataTable(tabName.Trim());
            //完成数据库表动态创建DataTable的构架，但是里面是没有任何数据的  
            foreach (DataColumn dc in dt.Columns)
            {
                string columnName = dc.ColumnName.ToString().Trim(); //获取表列名称
                string columnType = dc.DataType.ToString().Trim(); //获取表类型名称
                myDt.Columns.Add(new DataColumn(columnName, Type.GetType(columnType)));
            }
            #endregion

            #region 3.DataTable填充数据：在构建好的DataTable类型中填充（Sql参数化）数据
            //在构建好的myDt(DataTable类型)中填充数据（批量数据）
            foreach (SqlParameter[] parameters in listParameters.CheckNull())
            {
                foreach (SqlParameter parameter in parameters.CheckNull())
                {
                    DataRow dr = myDt.NewRow(); //在构建好的Datatable中动态添加行
                    //参数格式化，防止sql注入
                    var name = parameter.ParameterName.Trim();
                    var val = parameter.Value;
                    foreach (DataColumn dc in myDt.Columns)
                    {
                        string columnName = dc.ColumnName.ToString().Trim(); //获取表列名称
                        dr[columnName] = val;
                    }
                    myDt.Rows.Add(dr);
                }
            }
            #endregion
            return myDt;
        }
        #endregion

        #region 执行存储过程Procedure操作(添加、更新、删除)
        /// <summary>
                /// 存储过程(增、删、改)
                /// </summary>
                /// <param name="cmdText">sql语句或存储过程</param>
                /// <param name="sqlParams">[可选]sql参数化</param>
                /// <returns>int</returns>
        public static int GetCountByProcedure(string cmdText, params SqlParameter[] sqlParams)
        {
            using (SqlConnection conn = new SqlConnection(_ConnString))
            {
                OpenConnection(conn);
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = cmdText; //sql语句或存储过程
                    cmd.CommandType = CommandType.StoredProcedure; //指定cmd命令类型为存储过程
                    //检查参数组是否有数据
                    if (sqlParams != null && sqlParams.Length > 0)
                        cmd.Parameters.AddRange(sqlParams);
                    int count = cmd.ExecuteNonQuery(); //执行非查询命令,并返回受影响的行数
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                    return count;
                }
            }
        }

        /// <summary>
                /// 存储过程(增、删、改)
                /// </summary>
                /// <param name="cmdText">sql语句或存储过程</param>
                /// <param name="sqlParams">[可选]sql参数化</param>
                /// <returns>SqlDataReader</returns>
        public static SqlDataReader GetDataReaderByProcedure(string cmdText, params SqlParameter[] sqlParams)
        {
            using (SqlConnection conn = new SqlConnection(_ConnString))
            {
                OpenConnection(conn);
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure; //指定cmd类型为存储过程
                    //检查可选参数组是否有数据
                    if (sqlParams != null && sqlParams.Length > 0)
                        cmd.Parameters.AddRange(sqlParams);


                    //数据读取器SqlDataReader
                    //保证当SqlDataReader对象被关闭时，其依赖的连接也会被自动关闭CommandBehavior.
                    using (SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        cmd.Parameters.Clear();
                        return dataReader;
                    }
                }
            }
        }
        #endregion

        #region 事务操作Transactions
        /// <summary>
                /// 多语句（sql）事务操作
                /// 带参执行（sql参数化）
                /// </summary>
                /// <param name="dic">Dictionary<string,SqlParameter[]></param>
                /// <returns>int</returns>
        public static int ExecSqlTranByParams(Dictionary<string, SqlParameter[]> dic)
        {
            SqlTransaction transaction = null; // 创建事务对象
            try
            {
                int count = 0;
                using (SqlConnection conn = new SqlConnection(_ConnString))
                {
                    OpenConnection(conn);
                    using (transaction = conn.BeginTransaction())  //开始事务操作
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            foreach (var item in dic)
                            {
                                cmd.CommandText = item.Key.Trim(); //sql语句
                                SqlParameter[] sqlParams = item.Value;//sql参数化
                                //检查可选参数组是否有数据
                                if (sqlParams != null && sqlParams.Length > 0)
                                    cmd.Parameters.AddRange(sqlParams);
                                count += cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                            transaction.Commit(); //事务提交
                            return count;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback(); //事务回滚
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
                /// 多语句（sql）事务操作
                /// 无参执行
                /// </summary>
                /// <param name="sqlStringList">多条SQL语句</param>        
                /// <returns>bool</returns>
        public static bool ExecSqlTran(ArrayList sqlStringList)
        {
            SqlTransaction transaction = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(_ConnString))
                {
                    OpenConnection(conn);
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        using (transaction = conn.BeginTransaction()) //开始事务操作
                        {
                            cmd.Transaction = transaction;
                            for (int n = 0; n < sqlStringList.Count; n++)
                            {
                                string strsql = sqlStringList[n].ToString().Trim();
                                if (strsql.Length > 1)
                                {
                                    cmd.CommandText = strsql;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit(); //事务提交
                        }
                        cmd.Parameters.Clear();
                        return true; //事务执行成功返回true
                    }
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback(); //事务回滚
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion

        #region Query查询
        /// <summary>
                /// 数据读取器，逐行读取数据库表的所有字段的值
                /// 取值reader["字段名"]  
                /// </summary>
                /// <param name="cmdText">sql语句</param>
                /// <param name="sqlParams">sql参数化</param>
                /// <returns>SqlDataReader</returns>
        public static SqlDataReader GetDataReader(string cmdText, params SqlParameter[] sqlParams)
        {
            using (SqlConnection conn = new SqlConnection(_ConnString))
            {
                OpenConnection(conn);
                using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                {
                    if (sqlParams != null && sqlParams.Length > 0)
                        cmd.Parameters.AddRange(sqlParams);
                    //保证当SqlDataReader对象被关闭时，其依赖的连接也会被自动关闭CommandBehavior.
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        cmd.Parameters.Clear();
                        CloseDataReader(reader);
                        cmd.Dispose();
                        return reader;
                    }
                }
            }
        }

        /// <summary>
                /// 返回数据库表DataTable
                /// </summary>
                /// <param name="cmdText">sql语句</param>
                /// <param name="sqlParams">sql参数化</param>
                /// <returns>DataTable</returns>
        public static DataTable GetDataTable(string cmdText, params SqlParameter[] sqlParams)
        {
            using (DataTable dt = new DataTable())
            {
                using (SqlConnection conn = new SqlConnection(_ConnString))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cmdText;
                        //检查参数组是否有数据
                        if (sqlParams != null && sqlParams.Length > 0)
                            cmd.Parameters.AddRange(sqlParams);
                        //适配器自动打开数据库连接
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                        cmd.Parameters.Clear();
                    }
                }
                return dt;
            }
        }

        /// <summary>
                /// 批量查询返回数据集DataSet
                /// </summary>
                /// <param name="sqlParamsDic">Dictionary类型的【sql语句】和【sql参数化】</param>
                /// <param name="tabNames">[可选]DataSet中对应的DataTable名称</param>
                /// <returns>DataSet</returns>
        public static DataSet GetDataSet(Dictionary<string, SqlParameter[]> sqlParamsDic, params string[] tabNames)
        {
            using (DataSet ds = new DataSet())
            {
                string cmdText = string.Empty; //sql语句 
                SqlParameter[] sqlParams = null;//sql参数化
                if (tabNames != null && tabNames.Length > 0 && sqlParamsDic.Count == tabNames.Length)
                {
                    int i = 0; //表名称数组
                    foreach (var cmdTexts in sqlParamsDic.CheckNull())
                    {
                        string tabName = tabNames[i] ?? i.ToString();
                        cmdText = cmdTexts.Key; //sql语句 
                        sqlParams = cmdTexts.Value;
                        using (SqlConnection conn = new SqlConnection(_ConnString))
                        {
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = cmdText;
                                //检查参数组是否有数据
                                if (sqlParams != null && sqlParams.Length > 0)
                                    cmd.Parameters.AddRange(sqlParams);
                                //适配器自动打开数据库连接
                                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                {
                                    da.Fill(ds, tabName); // 将tabName查询结果集合填入DataSet中，并且将DataTable命名为tabName
                                }
                                cmd.Parameters.Clear();
                            }
                        }
                        if (i < tabNames.Length)
                        {
                            i++;
                        }
                    }
                }
                else
                {
                    foreach (var cmdTexts in sqlParamsDic.CheckNull())
                    {
                        cmdText = cmdTexts.Key; //sql语句 
                        sqlParams = cmdTexts.Value;
                        var dt = GetDataTable(cmdText, sqlParams);
                        ds.Tables.Add(dt.Copy());
                    }
                }
                return ds;
            }
        }

        /// <summary>
                /// 返回字典型表
                /// </summary>
                /// <param name="dic">key：表名，val：参数化sql语句</param>
                /// <param name="parameters">key：表名，val：该表对应的sql（格式化）参数</param>
                /// <param name="isCount">[可选]是否统计数据行</param>
                /// <returns>Dictionary<string, DataTable> key：表名，val：表（数据）</returns>
        public static Dictionary<string, ArrayList> GetDictionaryTable(Dictionary<string, string> dic, Dictionary<string, SqlParameter[]> parameters, bool isCount = false)
        {
            Dictionary<string, ArrayList> dicTab = new Dictionary<string, ArrayList>();
            foreach (var item in dic)
            {
                string tbName = item.Key; //表名
                string sql = item.Value; //sql语句
                var dt = GetDataTable(sql, parameters[tbName]);
                dicTab.Add(tbName, ConvertDataTableToArrayList(dt, isCount));
            }
            return dicTab;
        }

        /// <summary>
                /// 执行sql语句，返回第一行第一列的值
                /// </summary>
                /// <param name="cmdText">sql语句</param>
                /// <param name="sqlParams">sql格式化参数</param>
                /// <returns>object</returns>
        public static object ExecuteScalar(string cmdText, params SqlParameter[] sqlParams)
        {
            object id = null; //接收数据表自增长Id         
            using (SqlConnection conn = new SqlConnection(_ConnString)) // 建立数据库连接对象  
            {
                OpenConnection(conn);
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = cmdText; //sql语句或存储过程  
                    if (sqlParams != null && sqlParams.Length > 0) //检查参数组是否有数据  
                    {
                        foreach (SqlParameter sqlParam in sqlParams)
                        {
                            //参数格式化，防止sql注入  
                            cmd.Parameters.Add(sqlParam);
                        }
                    }
                    id = cmd.ExecuteScalar(); //执行命令，返回第一行第一列的值 obj
                    cmd.Parameters.Clear();
                }
                CloseConnection(conn);
            }
            return id;
        }
        #endregion

        #region DataTable,DataSet,DataReader 数据对象处理
        /// <summary>
                /// 获取数据表DataTable中的值，处理转换为ArrayList
                /// </summary>
                /// <param name="ds">DataSet</param>
                /// <param name="isCount">[可选]是否统计数据行</param>
                /// <returns>ArrayList</returns>
        public static ArrayList ConvertDataTableToArrayList(DataTable dt, bool isCount = false)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string key = GetStringFirstUpper(dt.Columns[j].ColumnName.ToString());
                    string val = dt.Rows[i][j].ToString().Trim();
                    dic[key] = val;
                }
                list.Add(dic);
            }
            if (isCount)
            {
                //统计查询数据行数
                Dictionary<string, int> dicCount = new Dictionary<string, int> { { "RowsCount", dt.Rows.Count } };
                list.Add(dicCount);
            }
            return list;
        }

        /// <summary>
                /// 获取数据集DataSet中的值，处理转换为List<ArrayList>
                /// </summary>
                /// <param name="ds">DataSet</param>
                /// <param name="isCount">[可选]是否统计数据行</param>
                /// <returns>List<ArrayList></returns>
        public static List<ArrayList> ConvertDataSetToList(DataSet ds, bool isCount = false)
        {
            List<ArrayList> list = new List<ArrayList>();
            //获取ds中表的数量
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                ArrayList al = new ArrayList { ConvertDataTableToArrayList(ds.Tables[i], isCount) };
                if (isCount)
                {
                    Dictionary<string, int> dicCount = new Dictionary<string, int>();
                    dicCount.Add("TablesCount", ds.Tables.Count); //统计查询数据表行数
                    al.Add(dicCount);
                }
                list.Add(al);
            }
            return list;
        }

        /// <summary>
                /// 数据读取器【DataReader】转换DataTable
                /// </summary>
                /// <param name="dataReader">数据读取器</param>
                /// <returns>DataTable</returns>
        public static DataTable ConvertDataReaderToDataTable(SqlDataReader dataReader)
        {
            using (DataTable dt = new DataTable())
            {
                //动态添加表的数据列  
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    using (DataColumn myDataColumn = new DataColumn())
                    {
                        myDataColumn.DataType = dataReader.GetFieldType(i);
                        myDataColumn.ColumnName = dataReader.GetName(i);
                        dt.Columns.Add(myDataColumn);
                    }
                }

                //添加表的数据  
                while (dataReader.Read())
                {
                    DataRow myDataRow = dt.NewRow();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        myDataRow[i] = dataReader[i].ToString();
                    }
                    dt.Rows.Add(myDataRow);
                }
                //关闭数据读取器DataReader
                CloseDataReader(dataReader);
                return dt;
            }
        }
        #endregion

        #region 开启连接SqlConnection.Open
        /// <summary>
                /// 打开OracleConnection
                /// </summary>
                /// <param name="conn">数据库连接对象</param>
        private static void OpenConnection(SqlConnection conn)
        {
            if (conn.State == ConnectionState.Closed) conn.Open();
        }
        #endregion

        #region 关闭连接，释放资源
        /// <summary>
                /// 关闭Connection
                /// </summary>
                /// <param name="conn">数据库(Oracle)连接对象</param>
        private static void CloseConnection(SqlConnection conn)
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
                conn.Dispose();//释放资源
            }
        }

        /// <summary>
                /// 关闭DataReader
                /// </summary>
                /// <param name="dataReader">数据读取器对象</param>
        private static void CloseDataReader(SqlDataReader dataReader)
        {
            if (dataReader.IsClosed == false) dataReader.Close();
        }
        #endregion

        #region 辅助方法
        /// <summary>
                /// foreach遍历列表或数组时，如果list或数组为null，就会报错
                /// 为了简化这一判断是否null的过程，特写此扩展方法，
                /// 因为列表和数组都继承IEnumerable接口，所以该扩展方法就扩展在IEnumerable类型上，
                /// 为了通用，将这个方法写成了个泛型方法
                /// </summary>
                /// <typeparam name="T"></typeparam>
                /// <param name="list"></param>
                /// <returns>IEnumerable<T></returns>
        public static IEnumerable<T> CheckNull<T>(this IEnumerable<T> list)
        {
            return list ?? new List<T>(0);
        }

        /// <summary>
                /// 获取字符串首字母大写
                /// </summary>
                /// <param name="s">待处理字符串</param>
                /// <returns>string</returns>
        public static string GetStringFirstUpper(string s)
        {
            string firstUpperStr = string.Empty;
            if (!string.IsNullOrEmpty(s))
            {
                if (s.Length > 1) firstUpperStr = char.ToUpper(s[0]) + s.Substring(1).ToLower().Trim();
                else firstUpperStr = char.ToUpper(s[0]).ToString().Trim();
            }
            return firstUpperStr;
        }

        public static SqlParameter MakeInParam(string targetFiled, object targetValue)
        {
            return new SqlParameter("@" + targetFiled, targetValue);
        }
        #endregion
    }
}
