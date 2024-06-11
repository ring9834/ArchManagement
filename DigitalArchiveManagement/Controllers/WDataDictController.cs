using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WDataDictController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddPubDictView()
        {
            return View("AddPubDict");
        }

        public IActionResult UpdatePubDictView()
        {
            return View("UpdatePubDict");
        }


        public IActionResult GetDataTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='SJLX')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult LoadPubDictionary()
        {
            string fieldName = "col_name,show_name,col_datatype,col_maxlen,col_null,col_default,field_type,comments,Unique_code";
            //string fieldNameWithComment = "col_name AS '列名',show_name AS '显示名',col_datatype AS '数据类型',col_maxlen '最大长度',col_null AS '可为空?',col_default '默认值',col_use AS '可用?',query_flag AS '可查?',field_type AS '字段类型',comments AS '备注',Unique_code";
            string sql = "SELECT " + fieldName + " FROM t_config_col_dict_base";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult InsertPubDict(string col_name, string show_name, string col_datatype, string col_maxlen, string col_null, string col_default, string field_type, string comments)
        {
            string sql = "INSERT INTO t_config_col_dict_base(col_name,show_name,col_datatype,col_maxlen,col_null,col_default,field_type,comments) \r\n";
            sql += "VALUES(@col_name,@show_name,@col_datatype,@col_maxlen,@col_null,@col_default,@field_type,@comments) \r\n";

            SqlParameter para1 = SqlHelper.MakeInParam("col_name", col_name);
            SqlParameter para2 = SqlHelper.MakeInParam("show_name", show_name);
            SqlParameter para3 = SqlHelper.MakeInParam("col_datatype", col_datatype);
            SqlParameter para4 = SqlHelper.MakeInParam("col_maxlen", col_maxlen);
            SqlParameter para5 = SqlHelper.MakeInParam("col_null", col_null == "1" ? true : false);
            SqlParameter para6 = SqlHelper.MakeInParam("field_type", field_type == "1" ? true : false);
            SqlParameter para7 = SqlHelper.MakeInParam("col_default", col_default == null ? string.Empty : col_default);
            SqlParameter para8 = SqlHelper.MakeInParam("comments", comments == null ? string.Empty : comments);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6, para7, para8 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult UpdatePubDict(string col_name, string show_name, string col_datatype, string col_maxlen, string col_null, string col_default, string field_type, string comments, int unique_code)
        {
            string sql = "UPDATE t_config_col_dict_base SET col_name=@col_name,show_name=@show_name,col_datatype=@col_datatype,col_maxlen=@col_maxlen,col_null=@col_null,\r\n";
            sql += "col_default=@col_default,field_type=@field_type,comments=@comments \r\n";
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
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult DeletePubDict(int unique_code)
        {
            string sql = "DELETE FROM t_config_col_dict_base WHERE Unique_code=@Unique_code \r\n";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });

        }

        /// <summary>
        /// 判断字段是否已经存在
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        public IActionResult VerifyIfColExist(string colName)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("Column", colName);
            SqlParameter[] param = new SqlParameter[] { para1 };
            string sql = "SELECT Unique_code FROM t_config_col_dict_base WHERE col_name=@Column";
            DataTable d = SqlHelper.GetDataTable(sql, param);
            return Json(d);
        }
    }
}
