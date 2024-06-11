using System;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using NetCoreDbUtility;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace ArchiveFileManagementNs.Controllers
{
    public class WLoginController : Controller
    {
        public IActionResult Index()
        {
            //清除Session
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult VerifyUser(string name, string pwd, bool cookie)
        {
            string pwdMd5 = MdfvService.Encrypt(pwd);
            string sql = "SELECT nick_name FROM t_user WHERE user_name=@user_name AND password=@password";
            SqlParameter para1 = SqlHelper.MakeInParam("user_name", name);
            SqlParameter para2 = SqlHelper.MakeInParam("password", pwdMd5);
            SqlParameter[] param = new SqlParameter[] { para1, para2 };
            DataTable dt = SqlHelper.GetDataTable(sql, param);

            int result = dt.Rows.Count;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (result > 0)
            {
                if (cookie)
                {
                    SetCookies("LoginUser", name);
                    SetCookies("LoginPwd", pwdMd5);//把加密后的密码放到cookie
                }                
                var nick = dt.Rows[0]["nick_name"].ToString();
                dt.Dispose();
                HttpContext.Session.Set("User", name);//added on 20201015
                return Json(new { rst = result, nickName = nick });
            }
            return Json(new { rst = result});
        }

        /// <summary>
        /// 如果用户使用了cookie中记录的用户名和密码，则直接验证，无须再加密
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public IActionResult VerifyUsert(string name, string pwd, bool cookie)
        {
            //string pwdMd5 = MdfvService.Encrypt(pwd);
            string sql = "SELECT nick_name FROM t_user WHERE user_name=@user_name AND password=@password";
            SqlParameter para1 = SqlHelper.MakeInParam("user_name", name);
            SqlParameter para2 = SqlHelper.MakeInParam("password", pwd);
            SqlParameter[] param = new SqlParameter[] { para1, para2 };
            DataTable dt = SqlHelper.GetDataTable(sql, param);

            int result = dt.Rows.Count;
            //JsonSerializerSettings setting = new JsonSerializerSettings();
            if (result > 0)
            {
                if (cookie)
                {
                    SetCookies("LoginUser", name);
                    SetCookies("LoginPwd", pwd);
                }
                var nick = dt.Rows[0]["nick_name"].ToString();
                dt.Dispose();
                HttpContext.Session.Set("User", name);//added on 20201015
                return Json(new { rst = result, nickName = nick });
            }
            return Json(new { rst = result });
        }

        /// <summary>
        /// 设置本地cookie
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>  
        /// <param name="minutes">过期时长，单位：分钟</param>      
        protected void SetCookies(string key, string value, int minutes = 30)
        {
            HttpContext.Response.Cookies.Append(key, value, new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(minutes)
            });
        }
        /// <summary>
        /// 删除指定的cookie
        /// </summary>
        /// <param name="key">键</param>
        protected void DeleteCookies(string key)
        {
            HttpContext.Response.Cookies.Delete(key);
        }

        /// <summary>
        /// 获取cookies
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回对应的值</returns>
        protected string GetCookies(string key)
        {
            HttpContext.Request.Cookies.TryGetValue(key, out string value);
            if (string.IsNullOrEmpty(value))
                value = string.Empty;
            return value;
        }
    }
}
