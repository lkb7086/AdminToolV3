using AdminToolV3.CommonClass;
using AdminToolV3.Models.Account;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace AdminToolV3.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString(CommonDefine.UserID) != null)
            {
                return RedirectToAction("index", "home");
            }

            return View();
        }

        [HttpPost]
#if DEBUG
        public IActionResult Index(User model)
#else // #if DEBUG
        public async Task<IActionResult> Index(User model)
#endif // #if DEBUG
        {
            if (HttpContext.Session.GetString(CommonDefine.UserID) != null)
            {
                return RedirectToAction("index", "home");
            }

#if DEBUG
            HttpContext.Session.SetString("UserID", model.UserID);
            return RedirectToAction("index", "home");
#else
            if (ModelState.IsValid) // 입력값이 모두 입력되었는지
            {
                using (MySqlConnection connection = await DBManager.Instance.AdminConnection())
                {
                    string query = $"SELECT COUNT(*) AS Cnt FROM admin.accesstoken " +
                                   $"WHERE UserName = '{model.UserID}' AND Token = '{model.Password}';";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    try
                    {
                        int row = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                        if (row > 0)
                        {
                            HttpContext.Session.SetString("UserID", model.UserID);
                            return RedirectToAction("index", "home");
                        }
                        else
                        {
                            ViewData["Alert"] = "<script>Swal.fire('', '아이디 또는 비밀번호가 올바르지 않습니다.', 'warning');</script>";
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogManager.Instance.Log(ex.Message);
                        return View(ex.ToString());
                    }
                }
            }

            return View();
#endif
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("index", "account");
        }
    }
}
