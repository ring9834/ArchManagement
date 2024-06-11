using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ArchiveFileManagementNs.Controllers
{
    public class TableController : Controller
    {
        public IActionResult Index88()
        {
            return View();
        }

        public IActionResult GetUsersWithPage(int pageSize, int pageIndex, string userName, string userId)
        {
            int total = 0;

            List<UserModel> userList = new List<UserModel>();

            for (int i = 0; i < 100; i++)
            {
                Random ra = new Random((unchecked((int)DateTime.Now.Ticks + i)));
                int age = ra.Next(10, 80); //1到100范围内的整数
                UserModel model = new UserModel();
                model.ID = i;
                model.UserName = "User" + i.ToString();
                model.Sex = i % 2 == 1 ? "男" : "女";
                model.Age = age;
                userList.Add(model);
            }

            if (!string.IsNullOrEmpty(userName))
                userList = userList.Where(o => o.UserName.Contains(userName)).ToList();

            if (!string.IsNullOrEmpty(userId))
                userList = userList.Where(o => o.ID.ToString() == userId).ToList();
            total = userList.Count;
            var pageUserList = userList.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { total = total, rows = pageUserList }, setting);
        }
    }

    public class UserModel
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Sex { get; set; }
        public int Age { get; set; }
    }
}
