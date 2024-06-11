using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class DragToAnotherTableController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetUsersWithPageA(int pageSize, int pageIndex)
        {
            int total = 0;

            List<UserModel> userList = new List<UserModel>();

            for (int i = 0; i < 100; i++)
            {
                Random ra = new Random((unchecked((int)DateTime.Now.Ticks + i)));
                int age = ra.Next(10, 80); //1到100范围内的整数
                UserModel model = new UserModel();
                model.ID = i;
                model.UserName = "UserA" + i.ToString();
                model.Sex = i % 2 == 1 ? "男" : "女";
                model.Age = age;
                userList.Add(model);
            }

            total = userList.Count;
            var pageUserList = userList.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { total = total, rows = pageUserList });
        }

        public IActionResult GetUsersWithPageB(int pageSize, int pageIndex)
        {
            int total = 0;

            List<UserModel> userList = new List<UserModel>();

            for (int i = 0; i < 100; i++)
            {
                Random ra = new Random((unchecked((int)DateTime.Now.Ticks + i)));
                int age = ra.Next(10, 80); //1到100范围内的整数
                UserModel model = new UserModel();
                model.ID = i;
                model.UserName = "UserB" + i.ToString();
                model.Sex = i % 2 == 1 ? "男" : "女";
                model.Age = age;
                userList.Add(model);
            }

            total = userList.Count;
            var pageUserList = userList.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { total = total, rows = pageUserList });
        }
    }
}
