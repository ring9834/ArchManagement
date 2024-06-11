using ArchiveFileManagementNs.Models;
using NetCoreDbUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace ArchiveFileManagementNs
{
    public class OperateRecHlp
    {
        /// <summary>
        /// 记录用户操作记录：删除、修改、增加记录、接收目录、挂接原文、审核入库（申请、审核）、操作档案类型库、业务数据字典、公共数据字典等
        /// </summary>
        /// <param name="operate_info"></param>
        /// <param name="operaterr"></param>
        /// <param name="influencedTable"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static void RcdUserOprationDel(OperationInfo opinfo, List<string> ids)
        {
            XmlDocument influ_doc = new XmlDocument();
            XmlElement influ_root = influ_doc.CreateElement("influData");
            XmlElement influ_table = influ_doc.CreateElement("influTable");
            influ_table.SetAttribute("table", opinfo.TableName);//影响的数据表

            string where = "";
            for (int i = 0; i < ids.Count; i++)
            {
                XmlElement influ_row = influ_doc.CreateElement("id");
                influ_row.InnerText = ids[i];
                influ_table.AppendChild(influ_row);

                if (i == 0)
                    where += " Unique_code=" + ids[i];
                else
                    where += " OR Unique_code=" + ids[i];
            }

            influ_root.AppendChild(influ_table);
            influ_doc.AppendChild(influ_root);
            string operXml = MakeXmlFromOperInfo(opinfo);

            string sql = "INSERT INTO t_operate_rec(operate_info,operaterr,operate_time,records_influenced) \r\n";
            sql += "VALUES(@operate_info,@operaterr,@operate_time,@records_influenced)";
            SqlParameter para1 = SqlHelper.MakeInParam("operate_info", opinfo.OperTag);
            SqlParameter para2 = SqlHelper.MakeInParam("operaterr", opinfo.UserId);
            SqlParameter para3 = SqlHelper.MakeInParam("operate_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para4 = SqlHelper.MakeInParam("records_influenced", influ_doc.OuterXml);
            SqlParameter para5 = SqlHelper.MakeInParam("base_info", operXml);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5 };
            SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            //string where = "";
            //for (int i = 0; i < ids.Count; i++)
            //{
            //    if (i == 0)
            //        where += " Unique_code=" + ids[i];
            //    else
            //        where += " OR Unique_code=" + ids[i];
            //}

            if (!string.IsNullOrEmpty(where))//有删除的目录记录时
            {
                sql = "IF COL_LENGTH('" + opinfo.TableName + "', 'is_deleted') IS NOT NULL \r\n";
                sql += "  BEGIN \r\n";
                sql += "  UPDATE " + opinfo.TableName + " SET is_deleted='1' WHERE " + where;
                sql += "  END \r\n";
                SqlHelper.ExecNonQuery(sql, null);//在数据表标识为已删除
            }
        }

        public static void RcdUserOprationCommon(OperationInfo opinfo, List<string> ids)
        {
            XmlDocument influ_doc = new XmlDocument();
            XmlElement influ_root = influ_doc.CreateElement("influData");
            XmlElement influ_table = influ_doc.CreateElement("influTable");
            influ_table.SetAttribute("table", opinfo.TableName);//影响的数据表

            for (int i = 0; i < ids.Count; i++)
            {
                XmlElement influ_row = influ_doc.CreateElement("id");
                influ_row.InnerText = ids[i];
                influ_table.AppendChild(influ_row);
            }

            influ_root.AppendChild(influ_table);
            influ_doc.AppendChild(influ_root);
            string operXml = MakeXmlFromOperInfo(opinfo);

            string sql = "INSERT INTO t_operate_rec(operate_info,operaterr,operate_time,records_influenced) \r\n";
            sql += "VALUES(@operate_info,@operaterr,@operate_time,@records_influenced)";
            SqlParameter para1 = SqlHelper.MakeInParam("operate_info", opinfo.OperTag);
            SqlParameter para2 = SqlHelper.MakeInParam("operaterr", opinfo.UserId);
            SqlParameter para3 = SqlHelper.MakeInParam("operate_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para4 = SqlHelper.MakeInParam("records_influenced", influ_doc.OuterXml);
            SqlParameter para5 = SqlHelper.MakeInParam("base_info", operXml);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5 };
            SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功
        }

        //删除记录专用
        public static void RcdUserOpration(OperationInfo opinfo, DataTable dt)
        {
            XmlDocument influ_doc = new XmlDocument();
            XmlElement influ_root = influ_doc.CreateElement("influData");
            XmlElement influ_table = influ_doc.CreateElement("influTable");
            influ_table.SetAttribute("table", opinfo.TableName);//影响的数据表

            string where = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                XmlElement influ_row = influ_doc.CreateElement("id");
                influ_row.InnerText = row[0].ToString();
                influ_table.AppendChild(influ_row);

                if (i == 0)
                    where += " Unique_code=" + row[0].ToString();
                else
                    where += " OR Unique_code=" + row[0].ToString();
            }

            influ_root.AppendChild(influ_table);
            influ_doc.AppendChild(influ_root);
            string operXml = MakeXmlFromOperInfo(opinfo);

            string sql = "INSERT INTO t_operate_rec(operate_info,operaterr,operate_time,records_influenced) \r\n";
            sql += "VALUES(@operate_info,@operaterr,@operate_time,@records_influenced)";
            SqlParameter para1 = SqlHelper.MakeInParam("operate_info", opinfo.OperTag);
            SqlParameter para2 = SqlHelper.MakeInParam("operaterr", opinfo.UserId);
            SqlParameter para3 = SqlHelper.MakeInParam("operate_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para4 = SqlHelper.MakeInParam("records_influenced", influ_doc.OuterXml);
            SqlParameter para5 = SqlHelper.MakeInParam("base_info", operXml);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5 };
            SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功

            //string where = "";
            //for (int i = 0; i < ids.Count; i++)
            //{
            //    if (i == 0)
            //        where += " Unique_code=" + ids[i];
            //    else
            //        where += " OR Unique_code=" + ids[i];
            //}

            if (!string.IsNullOrEmpty(where))//有删除的目录记录时
            {
                sql = "IF COL_LENGTH('" + opinfo.TableName + "', 'is_deleted') IS NOT NULL \r\n";
                sql += "  BEGIN \r\n";
                sql += "  UPDATE " + opinfo.TableName + " SET is_deleted='1' WHERE " + where;
                sql += "  END \r\n";
                SqlHelper.ExecNonQuery(sql, null);//在数据表标识为已删除
            }
            dt.Dispose();
        }

        public static void RcdUserOpration2(OperationInfo opinfo, DataTable dt)
        {
            XmlDocument influ_doc = new XmlDocument();
            XmlElement influ_root = influ_doc.CreateElement("influData");
            XmlElement influ_table = influ_doc.CreateElement("influTable");
            influ_table.SetAttribute("table", opinfo.TableName);//影响的数据表

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                XmlElement influ_row = influ_doc.CreateElement("id");
                influ_row.InnerText = row[0].ToString();
                influ_table.AppendChild(influ_row);
            }

            influ_root.AppendChild(influ_table);
            influ_doc.AppendChild(influ_root);

            string operXml = MakeXmlFromOperInfo(opinfo);

            string sql = "INSERT INTO t_operate_rec(operate_info,operaterr,operate_time,records_influenced) \r\n";
            sql += "VALUES(@operate_info,@operaterr,@operate_time,@records_influenced)";
            SqlParameter para1 = SqlHelper.MakeInParam("operate_info", opinfo.OperTag);
            SqlParameter para2 = SqlHelper.MakeInParam("operaterr", opinfo.UserId);
            SqlParameter para3 = SqlHelper.MakeInParam("operate_time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            SqlParameter para4 = SqlHelper.MakeInParam("records_influenced", influ_doc.OuterXml);
            SqlParameter para5 = SqlHelper.MakeInParam("base_info", operXml);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5 };
            SqlHelper.ExecNonQuery(sql, param);//result大于零表示插入记录成功
            dt.Dispose();
        }

        protected static string MakeXmlFromOperInfo(OperationInfo info)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("operInfo");
            XmlElement infoEle = doc.CreateElement("info");
            infoEle.SetAttribute("depart", info.Department);
            infoEle.SetAttribute("archType", info.ArchType);
            infoEle.SetAttribute("funcName", info.FuncName);
            infoEle.SetAttribute("funcModal", info.FuncModal);
            infoEle.SetAttribute("ip", info.SourceIP);
            root.AppendChild(infoEle);
            doc.AppendChild(infoEle);
            return doc.OuterXml;
        }
    }
}
