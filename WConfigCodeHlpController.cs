using System;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System.Data;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WConfigCodeHlpController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddBaseCodeV()
        {
            return View("AddBaseCode");
        }

        public IActionResult ModiBaseCodeV(string id)
        {
            ViewData["BaseId"] = id;
            return View("ModiBaseCode");
        }

        public IActionResult AddSubCodeV(string id)
        {
            ViewData["BaseId"] = id;
            return View("AddSubCode");
        }

        public IActionResult ModiSubCodeV(string id)
        {
            ViewData["SubId"] = id;
            return View("ModiSubCode");
        }

        public IActionResult GetCodebases()
        {
            string sql = "SELECT Unique_code,code_key,base_name,comments FROM t_config_codes_base";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetCodes(string baseCodeId)
        {
            string sql = "SELECT Unique_code,code_name,code_value,order_id FROM t_config_codes WHERE parent_code=" + baseCodeId;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetCodebaseById(string id)
        {
            string sql = "SELECT Unique_code,code_key,base_name,comments FROM t_config_codes_base WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetSubCodeById(string id)
        {
            string sql = "SELECT Unique_code,code_name,code_value,order_id FROM t_config_codes WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult AddBaseCode(string baseName, string baseCode, string baseComment)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("base_name", baseName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_key", baseCode);
            object cm = DBNull.Value;
            if (baseComment != null)
                cm = baseComment;
            SqlParameter para3 = SqlHelper.MakeInParam("comments", cm);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3 };

            string sql = "IF(SELECT COUNT(*) FROM t_config_codes_base WHERE base_name=@base_name OR code_key=@code_key) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "    INSERT INTO t_config_codes_base(base_name,code_key,comments) VALUES(@base_name,@code_key,@comments) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法插入重复记录
            sql += " END ";

            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult AddSubCode(string subName, string subCode, string subOrder,string parentId)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("code_name", subName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_value", subCode);
            SqlParameter para3 = SqlHelper.MakeInParam("order_id", subOrder);
            SqlParameter para4 = SqlHelper.MakeInParam("parent_code", parentId);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4 };

            string sql = "IF(SELECT COUNT(*) FROM t_config_codes WHERE code_name=@code_name OR code_value=@code_value) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "    INSERT INTO t_config_codes(code_name,code_value,order_id,parent_code) VALUES(@code_name,@code_value,@order_id,@parent_code) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法插入重复记录
            sql += " END ";

            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }
        
        public IActionResult ModiSubCode(string subName, string subCode, string subOrder, string unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("code_name", subName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_value", subCode);
            SqlParameter para3 = SqlHelper.MakeInParam("order_id", subOrder);
            SqlParameter para4 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4 };

            string sql = "IF(SELECT COUNT(*) FROM t_config_codes WHERE (code_name=@code_name OR code_value=@code_value) AND Unique_code != @Unique_code) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "    UPDATE t_config_codes SET code_name=@code_name,code_value=@code_value,order_id=@order_id WHERE Unique_code=@Unique_code \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法修改
            sql += " END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult ModiBaseCode(string baseName, string baseCode, string baseComment, string unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("base_name", baseName);
            SqlParameter para2 = SqlHelper.MakeInParam("code_key", baseCode);
            SqlParameter para3 = SqlHelper.MakeInParam("comments", baseComment == null ? "" : baseComment);
            SqlParameter para4 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4 };

            string sql = "IF(SELECT COUNT(*) FROM t_config_codes_base WHERE (base_name=@base_name OR code_key=@code_key) AND Unique_code != @Unique_code) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "    UPDATE t_config_codes_base SET base_name=@base_name,code_key=@code_key,comments=@comments WHERE Unique_code=@Unique_code";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法修改
            sql += " END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult DeleteBaseCode(int baseCodeId)
        {
            string sql = "DELETE FROM t_config_codes_base WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", baseCodeId);
            SqlParameter[] param = new SqlParameter[] { para1 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult DeleteSubCode(int subCodeId)
        {
            string sql = "DELETE FROM t_config_codes WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", subCodeId);
            SqlParameter[] param = new SqlParameter[] { para1 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

    }
}
