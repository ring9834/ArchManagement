using System;
using Microsoft.AspNetCore.Mvc;
using ArchiveFileManagementNs.Models;
using System.Data;

namespace ArchiveFileManagementNs.Controllers
{
    public class ButtonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TransferSimpleValueToJs()
        {
            string s = "我的MVC";
            return Json(s);
        }

        public IActionResult TransferDatatableToJs()
        {
            DataTable table = new DataTable();
            DataColumn colID = new DataColumn("Id", typeof(String));
            DataColumn colImage = new DataColumn("Image", typeof(String));
            DataColumn colTitle = new DataColumn("Name", typeof(String));
            DataColumn colDesc = new DataColumn("Desc", typeof(String));
            DataColumn colGroup = new DataColumn("Group", typeof(String));
            table.Columns.Add(colID);
            table.Columns.Add(colImage);
            table.Columns.Add(colTitle);
            table.Columns.Add(colDesc);
            table.Columns.Add(colGroup);

            DataRow row = table.NewRow();
            row[0] = "cn";
            row[1] = "cn";
            row[2] = "中国";
            row[3] = "中国位于东亚，是以华夏文明为主体、中华文化为基础，以汉族为主要民族的统一多民族国家，通用汉语。";
            row[4] = "亚洲";
            table.Rows.Add(row);

            row = table.NewRow();
            row[0] = "fr";
            row[1] = "fr";
            row[2] = "法国";
            row[3] = "法兰西共和国，简称法国，是一个本土位于西欧的总统共和制国家，海外领土包括南美洲和南太平洋的一些地区。";
            row[4] = "欧洲";
            table.Rows.Add(row);

            row = table.NewRow();
            row[0] = "us";
            row[1] = "us";
            row[2] = "美国";
            row[3] = "美利坚合众国，简称美国，是由华盛顿哥伦比亚特区、50个州和关岛等众多海外领土组成的联邦共和立宪制国家。";
            row[4] = "美洲";
            table.Rows.Add(row);

            row = table.NewRow();
            row[0] = "england";
            row[1] = "england";
            row[2] = "英国";
            row[3] = "英国，全称大不列颠及北爱尔兰联合王国，本土位于欧洲大陆西北面的不列颠群岛，被北海、英吉利海峡、凯尔特海、爱尔兰海和大西洋包围。";
            row[4] = "欧洲";
            table.Rows.Add(row);

            row = table.NewRow();
            row[0] = "it";
            row[1] = "it";
            row[2] = "意大利";
            row[3] = "意大利，全称意大利共和国，是一个欧洲国家，主要由南欧的亚平宁半岛及两个位于地中海中的岛屿西西里岛与萨丁岛所组成。";
            row[4] = "欧洲";
            table.Rows.Add(row);

            row = table.NewRow();
            row[0] = "ca";
            row[1] = "ca";
            row[2] = "加拿大";
            row[3] = "加拿大，为北美洲最北的国家，西抵太平洋，东迄大西洋，北至北冰洋，南方与美国本土接壤。领土面积为998万平方千米，位居世界第二。";
            row[4] = "美洲";
            table.Rows.Add(row);
            //return JsonConvert.SerializeObject(table);
            return Json(table);
        }

        public IActionResult TransferObjToBackEnd(UserInfo user)
        {
            // ViewData["myId"] = "通过ViewData传值到前端...";
            return Json(user);
        }

        /// <summary>
        /// 从前台Form传过来的值，用一个有对应字段的Modal接收
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IActionResult TransferFormDataToBackEnd(UserInfo user)
        {
            // ViewData["myId"] = "通过ViewData传值到前端...";
            return Json(user);
        }

        /// <summary>
        /// 从前台Form传过来的值，直接用变量接收值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        //public IActionResult TransferFormDataToBackEnd(string name, int age, bool sex)
        //{
        //    // ViewData["myId"] = "通过ViewData传值到前端...";
        //    return Content(name + " " + age + "  " + sex);
        //}
    }
}
