using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;
using NetCoreDbUtility;
using System.Xml;

namespace ArchiveFileManagementNs.Controllers
{
    public class WConfigArchNumController : WBaseController
    {
        public IActionResult Index(string id)
        {
            ViewData["table"] = id;
            return View();
        }

        public IActionResult GetFieldsByTableName(string id)
        {
            string sql = "SELECT Unique_code,col_name,show_name FROM t_config_col_dict WHERE code='" + id + "' AND field_type = 0";
            DataTable table = SqlHelper.GetDataTable(sql);
            return Json(table);
        }

        public async Task<IActionResult> GetFieldsByTableName2(string id)
        {
            DataTable table = null;
            string sql = "SELECT Unique_code,col_name,show_name FROM t_config_col_dict WHERE code='" + id + "' AND field_type = 0";
            await Task.Run(() =>
            {
                table = SqlHelper.GetDataTable(sql);
            });
            return Json(table);
        }

        public IActionResult GetArchNumConfigInfo(string table)
        {
            int result = 0;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string sql = "SELECT archive_body,archive_num_field,connect_char FROM t_config_archive_num_makeup WHERE code_name='" + table + "'";
            DataTable dt = SqlHelper.GetDataTable(sql, null);
            if (dt.Rows.Count > 0)
            {
                string xmlfile = dt.Rows[0]["archive_body"].ToString();
                string dhfield = dt.Rows[0]["archive_num_field"].ToString();
                //string prefix = dt.Rows[0]["archive_prefix"].ToString();
                string connchar = dt.Rows[0]["connect_char"].ToString();
                dt.Dispose();

                ArchNumMakeUp anm = new ArchNumMakeUp();
                anm.ArchFieldName = dhfield;
                anm.ConnectChar = connchar;
                List<ArchNumItem> list = new List<ArchNumItem>();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlfile.ToString());
                XmlNodeList nodeList = doc.SelectNodes(@"ArchiveNumMakeup/MakeupItem");
                for (int i = 0; i < nodeList.Count; i++)
                {
                    string prefix = nodeList[i].Attributes["Prefix"].Value;
                    ArchNumItem ani = new ArchNumItem();
                    ani.ID = i;
                    ani.ShowName = nodeList[i].Attributes["Name"].Value;
                    ani.ColName = nodeList[i].Attributes["value"].Value;
                    ani.FieldPrefix = nodeList[i].Attributes["Prefix"].Value;
                    list.Add(ani);
                }
                anm.Amount = list.Count;
                anm.ArchItems = list;

                result = 1;
                return Json(new { rst = result, configinfo = anm });
            }

            return Json(new { rst = result, configinfo = new ArchNumMakeUp() });
        }

        public IActionResult SaveArchNumConfitInfo(string table, ArchNumMakeUp anm)
        {
            string archive_num_parts_amount = anm.Amount.ToString();
            string connect_char = anm.ConnectChar;
            string archive_num_field = anm.ArchFieldName;
            string archive_body = GetConfigXML(anm.ArchItems);
            string sql = string.Empty;
            sql += "IF (SELECT COUNT(*) FROM t_config_archive_num_makeup WHERE code_name='" + table + "') = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "INSERT t_config_archive_num_makeup(archive_num_parts_amount,archive_body,connect_char,archive_num_field,code_name)\r\n";
            sql += "VALUES('" + archive_num_parts_amount + "','" + archive_body + "','" + connect_char + "','" + archive_num_field + "','" + table + "') \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += "BEGIN \r\n";
            sql += "UPDATE  t_config_archive_num_makeup SET archive_num_parts_amount='" + archive_num_parts_amount + "',archive_body='" + archive_body + "',\r\n";
            sql += "connect_char='" + connect_char + "', archive_num_field='" + archive_num_field + "' WHERE code_name='" + table + "'\r\n";
            sql += "END \r\n";
            int result = SqlHelper.ExecNonQuery(sql, null);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            string title = string.Empty;
            if (result > 0)
            {
                result = 1;
                title = "档号配置成功！";
                return Json(new { rst = result ,info = title});
            }
            result = 0;
            title = "档号配置失败！";
            return Json(new { rst = result, info = title });
        }

        protected string GetConfigXML(List<ArchNumItem> list)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ArchiveNumMakeup");
            doc.AppendChild(root);
            for (int i = 0; i < list.Count; i++)
            {
                ArchNumItem ani = list[i];
                XmlElement element = doc.CreateElement("MakeupItem");
                element.SetAttribute("Name", ani.ShowName);
                element.SetAttribute("value", ani.ColName);
                element.SetAttribute("Prefix", ani.FieldPrefix == null ? string.Empty : ani.FieldPrefix);
                root.AppendChild(element);
            }
            return doc.OuterXml;
        }
    }
}
