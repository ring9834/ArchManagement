using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NetCoreDbUtility;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System;
using System.Web;

namespace ArchiveFileManagementNs.Controllers
{
    public class WMenuMngController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddNavMenuV()
        {
            return View("AddNavMenu");
        }

        public IActionResult ModiNavMenuV(string id)
        {
            ViewData["uniqueCode"] = id;
            return View("ModiNavMenu");
        }

        public IActionResult SelectRoleV(string id,string userid)
        {
            ViewData["uniqueCode"] = id;
            //ViewData["roles"] = HttpUtility.UrlDecode(EnDecoderService.Base64Decode(userid));//modified for debugging on 20201119
            return View("SelectRole");
        }

        public IActionResult GetNavMenus()
        {
            string sql = "SELECT Unique_code,parent_id,menu_name as name,hrl,ntl,css_class,li_id,roles FROM t_config_nav_menu";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult GetNavMenusByUser(string userName)
        {
            string sql = "SELECT role_id from t_user WHERE user_name='" + userName + "'";
            object rid = SqlHelper.ExecuteScalar(sql, null);
            if (rid != null && rid != DBNull.Value)
            {
                sql = "SELECT li_id,roles FROM t_config_nav_menu";
                DataTable dt = SqlHelper.GetDataTable(sql, null);
                dt.Columns.Add("li_visible");//根据用户所在角色，判断其是否有对应菜单的显示权限
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string xml = dt.Rows[i]["roles"].ToString();
                    if (string.IsNullOrEmpty(xml))
                    {
                        dt.Rows[i]["li_visible"] = false;
                    }
                    else
                    {
                        dt.Rows[i]["li_visible"] = IfInConfigedRoles(rid.ToString(), xml);
                    }
                }
                dt.Columns.Remove("roles");//此字段在前端用不到，故返回前端前删除
                return Json(dt);
            }
            return null;
        }

        public IActionResult getMenuInfoById(string unique_code)
        {
            string sql = "SELECT Unique_code,parent_id,menu_name as name,hrl,ntl,css_class,li_id FROM t_config_nav_menu WHERE Unique_code=" + unique_code;
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            return Json(dt);
        }

        public IActionResult AddMenuInfo(string name, string url, string css, string liid, string parentid, string sphere)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("parent_id", parentid);
            SqlParameter para2 = SqlHelper.MakeInParam("menu_name", name);
            SqlParameter para3 = SqlHelper.MakeInParam("hrl", url == null ? "" : url);
            SqlParameter para4 = SqlHelper.MakeInParam("ntl", sphere == null ? "" : sphere);
            SqlParameter para5 = SqlHelper.MakeInParam("css_class", css == null ? "" : css);
            SqlParameter para6 = SqlHelper.MakeInParam("li_id", liid == null ? "" : liid);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6 };
            string sql = "IF (SELECT COUNT(*) FROM t_config_nav_menu WHERE menu_name=@menu_name) = 0 \r\n";//所有菜单名称都不能重复 modifed on 20201125
            sql += "BEGIN \r\n";
            sql +="    INSERT INTO t_config_nav_menu(parent_id,menu_name,hrl,ntl,css_class,li_id) VALUES(@parent_id,@menu_name,@hrl,@ntl,@css_class,@li_id) \r\n";
            sql += "   SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//相同的菜单名已存在，无法重复添加
            sql += " END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult ModiMenuInfo(string name, string url, string css, string liid, string sphere, int unique_code)
        {
            SqlParameter para2 = SqlHelper.MakeInParam("menu_name", name);
            SqlParameter para3 = SqlHelper.MakeInParam("hrl", url == null ? "" : url);
            SqlParameter para4 = SqlHelper.MakeInParam("ntl", sphere == null ? "" : sphere);
            SqlParameter para5 = SqlHelper.MakeInParam("css_class", css == null ? "" : css);
            SqlParameter para6 = SqlHelper.MakeInParam("li_id", liid == null ? "" : liid);
            SqlParameter para7 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para2, para3, para4, para5, para6, para7 };
            string sql = "IF (SELECT COUNT(*) FROM t_config_nav_menu WHERE menu_name=@menu_name AND Unique_code != @Unique_code) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "   UPDATE t_config_nav_menu SET menu_name=@menu_name,hrl=@hrl,ntl=@ntl,css_class=@css_class,li_id=@li_id WHERE Unique_code=@Unique_code \r\n";
            sql += "   SELECT 1 \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//相同的菜单名已存在，无法重复添加
            sql += " END ";
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult DelMenInfo(int unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1 };
            string sql = "DELETE FROM t_config_nav_menu WHERE Unique_code=@Unique_code";
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
            string sql = "UPDATE t_config_nav_menu SET roles = '" + rolesXml + "' WHERE Unique_code IN(" + idStr + ")";
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

        protected bool IfInConfigedRoles(string roleid, string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList nodes = doc.SelectNodes(@"AsignedRoles/Role");
            for (int i = 0; i < nodes.Count; i++)
            {
                string rid = nodes[i].Attributes["roleid"].Value;
                if (roleid.Equals(rid))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
