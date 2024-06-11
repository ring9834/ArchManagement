using Microsoft.AspNetCore.Mvc;
using System.Data;
using NetCoreDbUtility;
using System.Data.SqlClient;

namespace ArchiveFileManagementNs.Controllers
{
    public class WUserController : WBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddUserView()
        {
            return View("AddUser");
        }

        public IActionResult PickDepartv()
        {
            return View("PickDepart");
        }

        public IActionResult ModiPssView(string userid)
        {
            ViewData["userid"] = userid;
            return View("UpdatePwd");
        }

        public IActionResult GetUsers()
        {
            string sql = "SELECT user_name,nick_name,work_place,tel,role_id,user_depart,Unique_code FROM t_user";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult getUserInfo(string id)
        {
            string sql = "SELECT user_name,nick_name,work_place,tel,role_id,user_depart,Unique_code FROM t_user WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            ViewData["userinfo"] = table;
            return View("UpdateUser");
        }

        public IActionResult getUserInfo2(string id,string userid,string other)
        {
            string sql = "SELECT user_name,nick_name,work_place,tel,role_id,user_depart,Unique_code FROM t_user WHERE Unique_code=" + id;
            DataTable table = SqlHelper.GetDataTable(sql, null);
            ViewData["userinfo"] = table;
            ViewData["departinfo"] = other;
            return View("UpdateUser");
        }

        public IActionResult GetRoles()
        {
            string sql = "SELECT Unique_code,role_name FROM t_config_role";
            DataTable table = SqlHelper.GetDataTable(sql, null);
            return Json(table);
        }

        public IActionResult AddUser(string userName, string userNick, string workPlace, string userTel, string userRole, string userDepart)
        {
            string pwd = "123456";//默认密码
            string userPass = MdfvService.Encrypt(pwd);//加密
            string sql = "IF(SELECT COUNT(*) FROM t_user WHERE user_name=@user_name) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "    INSERT INTO t_user(user_name,nick_name,password,work_place,tel,role_id,user_depart) VALUES(@user_name,@nick_name,@password,@work_place,@tel,@role_id,@user_depart) \r\n";
            sql += "    SELECT 1 \r\n";
            sql += " END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//记录已存在，无法插入重复记录
            sql += " END ";
            SqlParameter para1 = SqlHelper.MakeInParam("user_name", userName);
            SqlParameter para2 = SqlHelper.MakeInParam("nick_name", userNick == null ? "" : userNick);
            SqlParameter para3 = SqlHelper.MakeInParam("password", userPass);
            SqlParameter para4 = SqlHelper.MakeInParam("work_place", workPlace == null ? "" : workPlace);
            SqlParameter para5 = SqlHelper.MakeInParam("tel", userTel == null ? "" : userTel);
            SqlParameter para6 = SqlHelper.MakeInParam("role_id", userRole);
            SqlParameter para7 = SqlHelper.MakeInParam("user_depart", userDepart);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para3, para4, para5, para6, para7 };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult UpdateUser(string userName, string userNick, string workPlace, string userTel, string userRole, string userDepart, string userId)
        {
            string sql = "IF(SELECT COUNT(*) FROM t_user WHERE user_name=@user_name AND Unique_code != @Unique_code) = 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "    UPDATE t_user SET user_name=@user_name,nick_name=@nick_name,work_place=@work_place,tel=@tel,role_id=@role_id,user_depart=@user_depart WHERE Unique_code=@Unique_code \r\n";
            sql += "    SELECT 1 \r\n";
            sql += "END \r\n";
            sql += "ELSE \r\n";
            sql += " BEGIN \r\n";
            sql += "    SELECT -2 \r\n";//相同的用户名已存在，无法修改
            sql += " END ";
            SqlParameter para1 = SqlHelper.MakeInParam("user_name", userName);
            SqlParameter para2 = SqlHelper.MakeInParam("nick_name", userNick == null ? "" : userNick);
            //SqlParameter para3 = SqlHelper.MakeInParam("password", userPass);
            SqlParameter para4 = SqlHelper.MakeInParam("work_place", workPlace == null ? "" : workPlace);
            SqlParameter para5 = SqlHelper.MakeInParam("tel", userTel == null ? "" : userTel);
            SqlParameter para6 = SqlHelper.MakeInParam("role_id", userRole);
            SqlParameter para7 = SqlHelper.MakeInParam("user_depart", userDepart);
            SqlParameter para8 = SqlHelper.MakeInParam("Unique_code", userId);
            SqlParameter[] param = new SqlParameter[] { para1, para2, para4, para5, para6, para7, para8 };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = result.ToString() });
        }

        public IActionResult DeleteUserInfo(string userId)
        {
            string sql = "DELETE FROM t_user WHERE Unique_code=@Unique_code";
            SqlParameter para1 = SqlHelper.MakeInParam("Unique_code", userId);
            SqlParameter[] param = new SqlParameter[] { para1 };
            int result = SqlHelper.ExecNonQuery(sql, param);
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            return Json(new { rst = result });
        }

        public IActionResult UpdatePssd(string userId,string pssd,string oldpssd)
        {
            string oldpwd = MdfvService.Encrypt(oldpssd);
            string pwd = MdfvService.Encrypt(pssd);
            string sql = "IF(SELECT COUNT(Unique_code) FROM t_user WHERE user_name=@user_name AND password=@oldPassword) > 0 \r\n";
            sql += "BEGIN \r\n";
            sql += "   UPDATE t_user SET password=@password WHERE user_name=@user_name \r\n";
            sql += "   SELECT 1 \r\n";
            sql += "END \r\n";
            sql += "BEGIN \r\n";
            sql += "   SELECT -1 \r\n";
            sql += "END \r\n";
            SqlParameter para1 = SqlHelper.MakeInParam("user_name", userId);
            SqlParameter para2 = SqlHelper.MakeInParam("password", pwd);
            SqlParameter para3 = SqlHelper.MakeInParam("oldPassword", oldpwd);
            SqlParameter[] param = new SqlParameter[] { para1, para2 , para3 };
            object result = SqlHelper.ExecuteScalar(sql, param);
            return Json(new { rst = int.Parse(result.ToString()) });
        }
    }
}
