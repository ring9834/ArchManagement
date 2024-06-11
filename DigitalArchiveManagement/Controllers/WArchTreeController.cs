using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using NetCoreDbUtility;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WArchTreeController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddArchTypeView()
        {
            return View("AddArchType");
        }

        public IActionResult UpdateArchTypeView()
        {
            return View("UpdateArchType");
        }

        public IActionResult SelectRoleV(string id)
        {
            ViewData["uniqueCode"] = id;
            return View("SelectRole");
        }

        public IActionResult GetArchTypes()
        {
            string sql = "SELECT Unique_code,super_id,name,code,node_type,order_id,has_content,content_type,yw_path,check_inout FROM t_config_type_tree";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetArchTypesByRole(string id)//id is uerid
        {
            string sql = "SELECT T1.Unique_code,T1.super_id,T1.name,T1.code,T1.node_type,T1.order_id,T1.has_content,T1.content_type,T1.yw_path,T1.check_inout FROM t_config_type_tree AS T1 \r\n";
            sql += "CROSS APPLY T1.access.nodes('/AsignedRoles/Role') x(m) \r\n";
            sql += "INNER JOIN t_config_role AS T2 ON x.m.value('(@roleid)[1]','nvarchar(MAX)') = T2.Unique_code \r\n";
            sql += "INNER JOIN t_user AS T3 ON T2.Unique_code=T3.role_id \r\n";
            sql += "WHERE T3.user_name='" + id + "' \r\n";
            sql += "UNION \r\n";
            sql += "SELECT T1.Unique_code,T1.super_id,T1.name,T1.code,T1.node_type,T1.order_id,T1.has_content,T1.content_type,T1.yw_path,T1.check_inout FROM t_config_type_tree AS T1 \r\n";
            sql += "WHERE node_type = 0";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult GetYwTypes()
        {
            string sql = "SELECT Unique_code,code_name,code_value FROM t_config_codes WHERE parent_code IN(SELECT Unique_code FROM t_config_codes_base WHERE code_key='YWLX')";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult AddArchType(string name, string code, int super_id, string order_id, string has_content, int content_type, string node_type, string check_inout)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("name", name);
            SqlParameter para2 = SqlHelper.MakeInParam("code", code);
            SqlParameter para3 = SqlHelper.MakeInParam("super_id", super_id);
            SqlParameter para4 = SqlHelper.MakeInParam("order_id", string.IsNullOrEmpty(order_id) ? 0 : int.Parse(order_id.ToString()));
            SqlParameter para5 = SqlHelper.MakeInParam("has_content", has_content == "1" ? true : false);
            SqlParameter para6 = SqlHelper.MakeInParam("content_type", content_type);
            SqlParameter para7 = SqlHelper.MakeInParam("node_type", node_type == "1" ? true : false);
            SqlParameter para8 = SqlHelper.MakeInParam("check_inout", check_inout == "1" ? true : false);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6, para7, para8 };

            string sql = "IF((SELECT COUNT(*) FROM t_config_type_tree WHERE code=@code) > 0 OR (SELECT COUNT(*) FROM t_config_type_tree WHERE name=@name AND super_id=@super_id) > 0) \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//档案类型库同级同名，或档案类型库值相同，均不允许
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    INSERT INTO t_config_type_tree(name,code,super_id,order_id,has_content,content_type,node_type,check_inout) VALUES(@name,@code,@super_id,@order_id,@has_content,@content_type,@node_type,@check_inout) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END ";

            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult UpdateArchType(string name, string order_id, string has_content, int content_type, string node_type, string check_inout, int unique_code)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("name", name);
            SqlParameter para4 = SqlHelper.MakeInParam("order_id", string.IsNullOrEmpty(order_id) ? 0 : int.Parse(order_id.ToString()));
            SqlParameter para5 = SqlHelper.MakeInParam("has_content", has_content == "1" ? true : false);
            SqlParameter para6 = SqlHelper.MakeInParam("content_type", content_type);
            SqlParameter para7 = SqlHelper.MakeInParam("node_type", node_type == "1" ? true : false);
            SqlParameter para8 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter para9 = SqlHelper.MakeInParam("check_inout", check_inout == "1" ? true : false);
            SqlParameter[] param = new SqlParameter[] { para1, para4, para5, para6, para7, para8, para9 };
            string sql = "IF(SELECT COUNT(*) FROM t_config_type_tree WHERE name=@name AND super_id=(SELECT super_id FROM t_config_type_tree WHERE Unique_code=@Unique_code) AND Unique_code != @Unique_code) > 0 \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//档案类型库同级同名不允许
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    UPDATE t_config_type_tree SET name=@name,order_id=@order_id,content_type=@content_type,has_content=@has_content,node_type=@node_type,check_inout=@check_inout WHERE Unique_code=@Unique_code \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END ";

            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult DeleteArchType(int unique_code, string table)
        {
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", unique_code);
            SqlParameter[] param = new SqlParameter[] { para1 };
            string sql = "DELETE FROM t_config_type_tree WHERE Unique_code=@Unique_code \r\n";
            sql += "IF OBJECT_ID(N'[" + table + "]',N'U') IS NOT NULL \r\n";
            sql += "BEGIN \r\n";
            sql += "  DELETE FROM [" + table + "] \r\n";
            sql += "  DELETE FROM t_config_field_show_list WHERE selected_code IN(SELECT Unique_code FROM t_config_col_dict WHERE code='" + table + "') \r\n";
            //sql += "  DELETE FROM t_config_manual_check WHERE selected_code IN(SELECT Unique_code FROM t_config_col_dict WHERE code='" + table + "') \r\n";
            //sql += "  DELETE FROM t_config_fields_group_for_autocheck WHERE selected_code IN(SELECT Unique_code FROM t_config_col_dict WHERE code='" + table + "') \r\n";
            sql += "  DELETE FROM t_config_primary_search WHERE selected_code IN(SELECT Unique_code FROM t_config_col_dict WHERE code='" + table + "') \r\n";
            sql += "  DELETE FROM t_config_col_dict WHERE code='" + table + "' \r\n";
            //sql += "  DELETE FROM t_config_field_for_autocheck WHERE table_code='" + table + "' \r\n";
            sql += "  DROP TABLE [" + table + "] \r\n";
            sql += "END";
            int result = SqlHelper.ExecNonQuery(sql, param);

            //删除名带temp的临时表
            string sqla = "DECLARE @tableName VARCHAR(20),@count INT,@i INT,@SQL VARCHAR(200); \r\n";
            sqla += "SELECT * INTO #tmpTable FROM (SELECT name,ROW_NUMBER() over(order by name) as myRow FROM sys.objects WHERE type = 'U' AND name LIKE '%temp%') b; \r\n";
            sqla += "SET @i=1; \r\n";
            sqla += "SELECT @count=COUNT(*) FROM #tmpTable; \r\n";
            sqla += "WHILE (@i <= @count) \r\n";
            sqla += "BEGIN \r\n";
            sqla += "  SELECT @tableName=name FROM #tmpTable WHERE myRow = @i; \r\n";
            sqla += "  SET @SQL = 'DROP TABLE ' + @tableName; \r\n";
            sqla += "  EXEC(@SQL); \r\n";
            sqla += "  SET @i=@i+1; \r\n";
            sqla += "END \r\n";
            sqla += "DROP TABLE #tmpTable \r\n";
            SqlHelper.ExecNonQuery(sqla, null);

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });

        }

        public IActionResult PickPathView()
        {
            return View("PickFilePath");
        }

        public IActionResult GetDirectories(string path)
        {
            var list = new List<FileSystemInfo>();
            if (string.IsNullOrWhiteSpace(path))
            {
                return View(list);
            }
            if (path.StartsWith("c:", StringComparison.OrdinalIgnoreCase))
            {
                var result = 0;
                var info = "此磁盘无权限访问！";
                //JsonSerializerSettings setting = new JsonSerializerSettings();
                return Json(new { rst = result, title = info });
            }
            if (!System.IO.Directory.Exists(path))
            {
                var result = 0;
                var info = "此磁盘不可用或无目录！";
               //JsonSerializerSettings setting = new JsonSerializerSettings();
                return Json(new { rst = result, title = info });
            }
            DirectoryInfo dic = new DirectoryInfo(path);
            list = dic.GetFileSystemInfos()
                .Where(t => !t.Attributes.ToString().ToLower().Contains("hidden"))
                .Where(t => !t.Attributes.ToString().ToLower().Contains("system"))
                .Where(t => !t.Attributes.ToString().ToLower().Contains("readonly"))
                .OrderByDescending(b => b.LastWriteTime)
                .ToList();

            List<FilePathInfo> lp = new List<FilePathInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                FilePathInfo pi = new FilePathInfo();
                pi.Name = list[i].Name;
                var fname = list[i].FullName;
                var fn = fname.Replace("\\", "/");//Important，让绝对路径使用斜杠而不是反斜杠，避开转义的麻烦
                pi.FullName = fn;
                pi.Attributes = list[i].Attributes;
                pi.Extension = list[i].Extension;
                lp.Add(pi);
            }

            //返回部分视图
            return PartialView("DiskPartial", lp);
        }

        public async Task<ContentResult> GetSelData2()
        {
            //var apiUrl = $"https://{Request.Host.Host}:{Request.Host.Port}/js/diskdata/diskconf1.json";
            var apiUrl = $"{AppSetting.GetConfig("UrlTransProtocal:CustomHeader")}{Request.Host.Host}:{Request.Host.Port}/js/diskdata/diskconf1.json";
            var str = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                str = await client.GetStringAsync(apiUrl);
            }
            return Content(str);
        }

        public IActionResult UpdateYwPath(int id,string path)
        {
            string sql = "UPDATE t_config_type_tree SET yw_path=@yw_path WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", id);
            SqlParameter para2 = SqlHelper.MakeInParam("yw_path", path);
            SqlParameter[] param = new SqlParameter[] { para1, para2 };
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
            string sql = "UPDATE t_config_type_tree SET access = '" + rolesXml + "' WHERE Unique_code IN(" + idStr + ")";
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

        public IActionResult GetRolesSet(string id)
        {
            string sql = "SELECT x.m.value('(@roleid)[1]','nvarchar(MAX)') AS ids \r\n";
            sql += "FROM t_config_type_tree AS T1 CROSS APPLY T1.access.nodes('/AsignedRoles/Role') x(m) \r\n";
            sql += "WHERE T1.Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }
    }
}
