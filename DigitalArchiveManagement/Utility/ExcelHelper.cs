using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ArchiveFileManagementNs
{
    public class ExcelHelper
    {
        /// <summary>
        /// 根据Excel和Sheet返回DataTable
        /// </summary>
        /// <param name="filePath">Excel文件地址</param>
        /// <param name="sheetIndex">Sheet索引</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(string filePath, int sheetIndex)
        {
            DataSet ds = GetDataSet(filePath, sheetIndex);
            if (ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null)
                    return ds.Tables[0];
                return null;
            }
            return null;
        }

        public static DataTable GetDataTable(string filePath, int sheetIndex, string addedColumnName, string addedColumnValue)
        {
            DataSet ds = GetDataSet(filePath, addedColumnName, addedColumnValue, sheetIndex);
            if (ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null)
                    return ds.Tables[0];
                return null;
            }
            return null;
        }

        public static bool HaveNullHeader(string filePath, int? sheetIndex = null)
        {
            bool flag = false;
            IWorkbook fileWorkbook;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (filePath.Last() == 's')
                {
                    try
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        fileWorkbook = new XSSFWorkbook(fs);
                    }
                    catch
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                }
            }

            for (int i = 0; i < fileWorkbook.NumberOfSheets; i++)
            {
                if (sheetIndex != null && sheetIndex != i)
                    continue;

                ISheet sheet = fileWorkbook.GetSheetAt(i);

                //表头
                IRow header = sheet.GetRow(sheet.FirstRowNum);
                for (int j = 0; j < header.LastCellNum; j++)
                {
                    object obj;
                    if (filePath.Last() == 's')
                        obj = GetValueTypeForXLS(header.GetCell(j) as HSSFCell);
                    else
                        obj = GetValueTypeForXLS(header.GetCell(j) as XSSFCell);

                    if (obj == null || obj.ToString() == string.Empty)
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// 根据Excel返回DataSet
        /// </summary>
        /// <param name="filePath">Excel文件地址</param>
        /// <param name="sheetIndex">Sheet索引，可选，默认返回所有Sheet</param>
        /// <returns>DataSet</returns>
        public static DataTable GetDataHeader(string filePath, int? sheetIndex = null)
        {
            IWorkbook fileWorkbook;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (filePath.Last() == 's')
                {
                    try
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        fileWorkbook = new XSSFWorkbook(fs);
                    }
                    catch
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                }
            }

            DataTable dt = new DataTable();
            for (int i = 0; i < fileWorkbook.NumberOfSheets; i++)
            {
                if (sheetIndex != null && sheetIndex != i)
                    continue;

                ISheet sheet = fileWorkbook.GetSheetAt(i);

                //表头
                IRow header = sheet.GetRow(sheet.FirstRowNum);
                for (int j = 0; j < header.LastCellNum; j++)
                {
                    object obj;
                    if (filePath.Last() == 's')
                        obj = GetValueTypeForXLS(header.GetCell(j) as HSSFCell);
                    else
                        obj = GetValueTypeForXLS(header.GetCell(j) as XSSFCell);

                    if (obj == null || obj.ToString() == string.Empty)
                    {
                        dt.Columns.Add(new DataColumn("Column" + j.ToString()));
                    }
                    else
                        dt.Columns.Add(new DataColumn(obj.ToString()));
                }
            }
            return dt;
        }

        /// <summary>
        /// 根据Excel返回DataSet
        /// </summary>
        /// <param name="filePath">Excel文件地址</param>
        /// <param name="sheetIndex">Sheet索引，可选，默认返回所有Sheet</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string filePath, int? sheetIndex = null)
        {
            DataSet ds = new DataSet();
            IWorkbook fileWorkbook;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (filePath.Last() == 's')
                {
                    try
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        fileWorkbook = new XSSFWorkbook(fs);
                    }
                    catch
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                }
            }

            for (int i = 0; i < fileWorkbook.NumberOfSheets; i++)
            {
                if (sheetIndex != null && sheetIndex != i)
                    continue;
                DataTable dt = new DataTable();
                ISheet sheet = fileWorkbook.GetSheetAt(i);

                //表头
                IRow header = sheet.GetRow(sheet.FirstRowNum);
                List<int> columns = new List<int>();
                for (int j = 0; j < header.LastCellNum; j++)
                {
                    object obj;
                    if (filePath.Last() == 's')
                        obj = GetValueTypeForXLS(header.GetCell(j) as HSSFCell);
                    else
                        obj = GetValueTypeForXLS(header.GetCell(j) as XSSFCell);

                    if (obj == null || obj.ToString() == string.Empty)
                    {
                        dt.Columns.Add(new DataColumn("Column" + j.ToString()));
                    }
                    else
                        dt.Columns.Add(new DataColumn(obj.ToString()));
                    columns.Add(j);
                }
                //数据
                IEnumerator rows = sheet.GetEnumerator();
                int r = 0;
                while (rows.MoveNext())
                {
                    DataRow dr = dt.NewRow();
                    bool hasValue = false;
                    foreach (int K in columns)
                    {
                        IRow rw = sheet.GetRow(r);
                        if (rw == null)
                        {
                            //MessageBox.Show("EXCEL目录文件中第" + r + "行没有数据，请检查修改后继续！\r\n建议把EXCEL文件中可视的所有目录拷贝到新的文件内，再行目录接收。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dt = null;
                            return ds;
                        }
                        if (filePath.Last() == 's')
                        {
                            dr[K] = GetValueTypeForXLS(rw.GetCell(K) as HSSFCell);
                        }
                        else
                        {
                            dr[K] = GetValueTypeForXLS(rw.GetCell(K) as XSSFCell);

                        }
                        if (dr[K] != null && dr[K].ToString() != string.Empty)
                        {
                            hasValue = true;
                        }
                    }
                    if (hasValue)
                    {
                        dt.Rows.Add(dr);
                    }
                    r++;
                }
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public static DataSet GetDataSet(string filePath, string addedColumnName, string addedColumnValue, int? sheetIndex = null)
        {
            DataSet ds = new DataSet();
            IWorkbook fileWorkbook;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (filePath.Last() == 's')
                {
                    try
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        fileWorkbook = new XSSFWorkbook(fs);
                    }
                    catch
                    {
                        fileWorkbook = new HSSFWorkbook(fs);
                    }
                }
            }

            for (int i = 0; i < fileWorkbook.NumberOfSheets; i++)
            {
                if (sheetIndex != null && sheetIndex != i)
                    continue;
                DataTable dt = new DataTable();
                ISheet sheet = fileWorkbook.GetSheetAt(i);

                //表头
                IRow header = sheet.GetRow(sheet.FirstRowNum);
                List<int> columns = new List<int>();
                for (int j = 0; j < header.LastCellNum; j++)
                {
                    object obj;
                    if (filePath.Last() == 's')
                        obj = GetValueTypeForXLS(header.GetCell(j) as HSSFCell);
                    else
                        obj = GetValueTypeForXLS(header.GetCell(j) as XSSFCell);

                    if (obj == null || obj.ToString() == string.Empty)
                    {
                        dt.Columns.Add(new DataColumn("Column" + j.ToString()));
                    }
                    else
                        dt.Columns.Add(new DataColumn(obj.ToString()));
                    columns.Add(j);
                }
                dt.Columns.Add(addedColumnName);
                dt.Columns[addedColumnName].DefaultValue = addedColumnValue;//加一列，带默认值

                //数据
                IEnumerator rows = sheet.GetEnumerator();
                int r = 0;
                while (rows.MoveNext())
                {
                    DataRow dr = dt.NewRow();
                    bool hasValue = false;
                    foreach (int K in columns)
                    {
                        IRow rw = sheet.GetRow(r);
                        if (rw == null)
                        {
                            //MessageBox.Show("EXCEL目录文件中第" + r + "行没有数据，请检查修改后继续！\r\n建议把EXCEL文件中可视的所有目录拷贝到新的文件内，再行目录接收。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dt = null;
                            return ds;
                        }
                        if (filePath.Last() == 's')
                        {
                            dr[K] = GetValueTypeForXLS(rw.GetCell(K) as HSSFCell);
                        }
                        else
                        {
                            dr[K] = GetValueTypeForXLS(rw.GetCell(K) as XSSFCell);

                        }
                        if (dr[K] != null && dr[K].ToString() != string.Empty)
                        {
                            hasValue = true;
                        }
                    }
                    if (hasValue)
                    {
                        dt.Rows.Add(dr);
                    }
                    r++;
                }
                ds.Tables.Add(dt);
            }

            return ds;
        }

        /// <summary>
        /// 根据DataTable导出Excel
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="file">保存地址</param>
        public static byte[] GetExcelByDataTable(DataTable dt)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            return GetExcelByDataSet(ds);
        }

        /// <summary>
        /// 根据DataSet导出Excel
        /// </summary>
        /// <param name="ds">DataSet</param>
        public static byte[] GetExcelByDataSet(DataSet ds)
        {
            IWorkbook fileWorkbook = new HSSFWorkbook();
            int index = 0;
            foreach (DataTable dt in ds.Tables)
            {
                index++;
                ISheet sheet = fileWorkbook.CreateSheet("Sheet" + index);

                //表头
                IRow row = sheet.CreateRow(0);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ICell cell = row.CreateCell(i);
                    cell.SetCellValue(dt.Columns[i].ColumnName);
                }

                //数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IRow row1 = sheet.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ICell cell = row1.CreateCell(j);
                        cell.SetCellValue(dt.Rows[i][j].ToString());
                    }
                }
            }

            //转为字节数组
            //byte[] buffer = new byte[1024 * 5];
            using (MemoryStream ms = new MemoryStream())
            {
                fileWorkbook.Write(ms);
                byte[] buffers = ms.ToArray();
                return buffers;
            }       
        }

        /// <summary>
        /// 根据单元格将内容返回为对应类型的数据
        /// </summary>
        /// <param name="cell">单元格</param>
        /// <returns>数据</returns>
        private static object GetValueTypeForXLS(HSSFCell cell)
        {
            if (cell == null)
                return null;
            switch (cell.CellType)
            {
                case CellType.Blank: //BLANK:
                    return null;
                case CellType.Boolean: //BOOLEAN:
                    return cell.BooleanCellValue;
                case CellType.Numeric: //NUMERIC:
                    return cell.NumericCellValue;
                case CellType.String: //STRING:
                    return cell.StringCellValue;
                case CellType.Error: //ERROR:
                    return cell.ErrorCellValue;
                case CellType.Formula: //FORMULA:
                default:
                    return "=" + cell.CellFormula;
            }
        }

        private static object GetValueTypeForXLS(XSSFCell cell)
        {
            if (cell == null)
                return null;
            switch (cell.CellType)
            {
                case CellType.Blank: //BLANK:
                    return null;
                case CellType.Boolean: //BOOLEAN:
                    return cell.BooleanCellValue;
                case CellType.Numeric: //NUMERIC:
                    return cell.NumericCellValue;
                case CellType.String: //STRING:
                    return cell.StringCellValue;
                case CellType.Error: //ERROR:
                    return cell.ErrorCellValue;
                case CellType.Formula: //FORMULA:
                default:
                    return "=" + cell.CellFormula;
            }
        }
    }
}
