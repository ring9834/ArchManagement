using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WFunctionAccessController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetFuncAccessByUser(string userName)
        {
            string sql = "SELECT role_id from t_user WHERE user_name='" + userName + "'";
            object rid = SqlHelper.ExecuteScalar(sql, null);
            sql = "SELECT func_symble,roles FROM t_config_func_point";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            dt.Columns.Add("func_visible");//根据用户所在角色，判断其是否有对功能点的显示权限
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string xml = dt.Rows[i]["roles"].ToString();
                if (string.IsNullOrEmpty(xml))
                {
                    dt.Rows[i]["func_visible"] = false;
                }
                else
                {
                    dt.Rows[i]["func_visible"] = IfInConfigedRoles(rid.ToString(), xml);
                }
            }
            dt.Columns.Remove("roles");//此字段在前端用不到，故返回前端前删除
            return Json(dt);
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
