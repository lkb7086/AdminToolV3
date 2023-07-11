using AdminToolV3.CommonClass;
using AdminToolV3.Models;
using AdminToolV3.Models.Home;
using Common.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using NLog.Web;
using NLog;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace AdminToolV3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(ControlData controlData)
        {
            if (HttpContext.Session.GetString(CommonDefine.UserID) == null)
            {
                return RedirectToAction("index", "account");
            }

            // 신규 유저
            List<KPIUserData> newUserList = new List<KPIUserData>();
            for (int i = CommonDefine.NewUserDay; i > 0; i--)
            {
                string date = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                newUserList.Add(new KPIUserData { Date = date, UserCount = 0 });
            }

            using (MySqlConnection connection = await DBManager.Instance.GameConnection())
            {
                string targetDate = DateTime.Today.AddDays(-CommonDefine.NewUserDay).ToString("yyyy-MM-dd");
                string query = $"SELECT DATE(CreateDate) AS DateGroup, COUNT(*) AS Cnt " +
                               $"FROM game.account " +
                               $"GROUP BY DateGroup " +
                               $"HAVING '{targetDate}' <= DateGroup AND DateGroup < '{DateTime.Today.ToString("yyyy-MM-dd")}';";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                try
                {
                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        int i = 0;
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            if (i >= newUserList.Count)
                            {
                                NLogManager.Instance.Log("HomeController.Index / 인덱스 경계 오류1");
                                return View("HomeController.Index / 인덱스 경계 오류1");
                            }

                            string dateOnly = reader.GetDateOnly("DateGroup").ToString();
                            if (dateOnly.Equals(newUserList[i].Date))
                            {
                                newUserList[i].UserCount = reader.GetInt32("Cnt");
                            }

                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Instance.Log(ex.Message);
                    return View(ex.ToString());
                }
            }

            // 이탈 유저
            //string query = $"SELECT DATE(CreateDate) AS DateGroup, COUNT(*) AS Cnt FROM game.account GROUP BY DateGroup HAVING DateGroup >= '{targetDate}'";

            // DAU
            List<KPIUserData> dauList = new List<KPIUserData>();
            using (MySqlConnection connection = await DBManager.Instance.LogConnection())
            {
                // 날짜 범위검색 후 테이블에 일주일치 데이터가 있으면 빠른(캐시) 검색을 하고 그게 아니면 느린(직접) 검색을 합니다.
                bool isCachedDAU = false;
                const string dateFormat = "yyyy-MM-dd 00:00:00";
                string startDate = DateTime.Now.AddDays(-(CommonDefine.DauDay)).ToString(dateFormat);
                string endDate = DateTime.Now.ToString(dateFormat);
                string query = $"SELECT UTCCreateTime, UserCount FROM log.daucache " +
                               $"WHERE '{startDate}' <= UTCCreateTime AND UTCCreateTime < '{endDate}';";

                // 빠른 검색
                MySqlCommand cmd = new MySqlCommand(query, connection);
                try
                {
                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        int row = reader.Cast<object>().Count();
                        if (row >= CommonDefine.DauDay)
                        {
                            isCachedDAU = true;
                        }
                    }

                    if (isCachedDAU && CommonManager.Instance.IsExecutedDAUAfterStart)
                    {
                        using (MySqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                dauList.Add(new KPIUserData { Date = reader.GetDateTime("UTCCreateTime").ToString(dateFormat), UserCount = reader.GetInt32("UserCount") });
                            }
                        }
                    }
                    else // 느린 검색 후 log.daucache 테이블에 INSERT (INSERT는 캐시 목적)
                    {
                        query = "SELECT DateGroup, SUM(Cnt) AS UserCnt FROM " +
                                "(" +
                                "SELECT DATE(UTCCreateTime) AS DateGroup, 1 AS Cnt FROM log.packetlog " +
                                $"WHERE '{startDate}' < UTCCreateTime AND UTCCreateTime < '{endDate}' " +
                                "GROUP BY DateGroup, PID" +
                                ") A " +
                                "GROUP BY DateGroup;";

                        cmd.CommandText = query;
                        using (MySqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                int dailyDAU = reader.GetInt32("UserCnt");
                                dauList.Add(new KPIUserData { Date = reader.GetDateTime("DateGroup").ToString(dateFormat), UserCount = reader.GetInt32("UserCnt") });
                            }
                        }

                        StringBuilder queryBuilder = new StringBuilder();
                        foreach (var item in dauList)
                        {
                            query = $"INSERT INTO log.daucache(UTCCreateTIme, UserCount) " +
                                    $"VALUES('{item.Date}', {item.UserCount}) " +
                                    $"ON DUPLICATE KEY UPDATE UserCount = {item.UserCount}; ";

                            queryBuilder.Append(query);                            
                        }

                        cmd.CommandText = queryBuilder.ToString();
                        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                        CommonManager.Instance.IsExecutedDAUAfterStart = true;
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Instance.Log(ex.Message);
                    return View(ex.ToString());
                }
            }

            ViewBag.NewUserList = newUserList;
            ViewBag.DAUList = dauList;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
