using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WFuncMngController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddFuncV()
        {
            return View("AddFuncMng");
        }

        public IActionResult ModiFuncV(string id)
        {
            ViewData["uniqueCode"] = id;
            return View("ModiFuncMng");
        }

        public IActionResult SelectRoleV(string id, string userid)
        {
            ViewData["uniqueCode"] = id;
            ViewData["roles"] = userid;
            return View("SelectRole");
        }

        public IActionResult GetFuncPoints()
        {
            string sql = "SELECT Unique_code,parent_id,func_name as name,func_symble,roles FROM t_config_func_point";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult getFuncInfoById(string unique_code)
        {
            string sql = "SELECT Unique_code,parent_id,func_name as name,func_symble FROM t_config_func_point WHERE Unique_code=" + unique_code;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult AddFuncPoint(string name, string symble, string parentid)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("parent_id", parentid);
            SqlParameter para2 = SqlHelper.MakeInParam("func_name", name);
            SqlParameter para3 = SqlHelper.MakeInParam("func_symble", symble);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3 };
            string sql = "IF (SELECT COUNT(*) FROM t_config_func_point WHERE func_name=@func_name AND parent_id = @parent_id) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "  INSERT INTO t_config_func_point(parent_id,func_name,func_symble) VALUES(@parent_id,@func_name,@func_symble) \r\n";
            sql += "   SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//相同的功能名已存在，无法重复添加
            sql += " END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult ModiFuncInfo(string name, string symble, string parentid, int unique_code)
        {
            SqlParameter para2 = SqlHelper.MakeInParam("func_name", name);
            SqlParameter para3 = SqlHelper.MakeInParam("func_symble", symble);
            SqlParameter para7 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter para8 = SqlHelper.MakeInParam("parent_id", parentid);
            SqlParameter[] param = new SqlParameter[] { para2, para3, para7, para8 };
            string sql = "IF (SELECT COUNT(*) FROM t_config_func_point WHERE func_name=@func_name AND Unique_code != @Unique_code AND parent_id=@parent_id) = 0 \r\n";//同一父功能下不能有同名的，不同父功能下可能同名
            sql += "BEGIN \r\n";
            sql += "   UPDATE t_config_func_point SET func_name=@func_name,func_symble=@func_symble WHERE Unique_code=@Unique_code \r\n";
            sql += "   SELECT 1 \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//相同的功能名已存在，无法重复添加
            sql += "END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult DelFuncInfo(int unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1 };
            string sql = "DELETE FROM t_config_func_point WHERE Unique_code=@Unique_code";
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult AsignRoles(List<string> ids, List<string> roles)
        {
            var rolesXml = GetRolesXML(roles);
            string idStr = string.Empty;
            for (int i = 0; i < ids.Count; i++)
            {
                if (i == 0)
                    idStr = ids[i];
                else
                    idStr += "," + ids[i];
            }
            string sql = "UPDATE t_config_func_point SET roles = '" + rolesXml + "' WHERE Unique_code IN(" + idStr + ")";
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        protected string GetRolesXML(List<string> list)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("AsignedRoles");
            doc.AppendChild(root);
            for (int i = 0; i < list.Count; i++)
            {
                XmlElement element = doc.CreateElement("Role");
                element.SetAttribute("roleid", list[i]);
                root.AppendChild(element);
            }
            return doc.OuterXml;
        }
    }
}
