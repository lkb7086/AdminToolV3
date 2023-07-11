using AdminToolV3.CommonClass;
using AdminToolV3.Models.Home;
using Common.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using MySqlConnector;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace AdminToolV3.Controllers
{
    public class LogController : Controller
    {
        public async Task<IActionResult> UserLog(ControlData controlData)
        {
            if (HttpContext.Session.GetString(CommonDefine.UserID) == null)
            {
                return RedirectToAction("index", "account");
            }

            // 입력된 시간 검사
            //string timePattern = @"^(0[0-9]|1[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])$";
            //if(false == Regex.IsMatch(controlData.Time1, timePattern))
            //{
            //    ViewData["Alert"] = "<script>Swal.fire('', '시작 날짜의 시간 설정이 잘못되었습니다.', 'warning');</script>";
            //}

            //if(false == Regex.IsMatch(controlData.Time2, timePattern))
            //{
            //    ViewData["Alert"] = "<script>Swal.fire('', '종료 날짜의 시간 설정이 잘못되었습니다.', 'warning');</script>";
            //}

            List<int> widList = new List<int>();
            List<int> sidList = new List<int>();

            // 각각 웹사이트에서 디폴트로 보여줄 0값을 넣었습니다.
            widList.Add(0);
            sidList.Add(0);

            using (MySqlConnection connection = await DBManager.Instance.CommonConnection())
            {
                string query = $"SELECT SID, WID FROM common.serverlist";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                try
                {
                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            widList.Add(reader.GetInt32("WID"));
                            sidList.Add(reader.GetInt32("SID"));
                        }
                    }

                    // 그룹질의로 불가능하여 List에서 중복제거 했습니다.
                    widList = widList.Distinct().ToList();
                    sidList = sidList.Distinct().ToList();
                }
                catch (Exception ex)
                {
                    NLogManager.Instance.Log(ex.Message);
                    return View(ex.ToString());
                }
            }

            int totalAddTime = 0;
            string utcAddMinusString = string.Empty;
            string fiexdeUtcTimeParam = string.Empty;
            TimeSpan ts;
            DateTime targetDateTime;
            DateTime fixedUtcNow;

            try
            {
                // 선택된 UTC 시간 파싱
                utcAddMinusString = controlData.UtcTime.Substring(0, 1);
                fiexdeUtcTimeParam = controlData.UtcTime.Substring(1);
                targetDateTime = DateTime.Parse(fiexdeUtcTimeParam);
                ts = targetDateTime.TimeOfDay;
                totalAddTime = (int)ts.TotalSeconds;
            }
            catch (Exception ex)
            {
                NLogManager.Instance.Log(ex.Message);
                return View(ex.ToString());
            }

            int rowCount = 0;            
            List<PacketLog> packetLogList = new List<PacketLog>();
            
            using (MySqlConnection connection = await DBManager.Instance.LogConnection())
            {
                const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                const string dateFormat = "yyyy-MM-dd";
                const string timeFormat = "HH:00:00";
                int offset = (controlData.CurPage - 1) * controlData.RowLimit;
                string query1 = string.Empty;
                string query2 = string.Empty;

                switch (controlData.SearchType)
                {
                    case 0: // 웹페이지 로드
                        {
                            DateTime now = DateTime.Now;
                            if (utcAddMinusString.Contains('+'))
                                fixedUtcNow = now.AddSeconds(-totalAddTime);
                            else
                                fixedUtcNow = now.AddSeconds(totalAddTime);

                            // 선택된 UTC 시간 파싱
                            string beginUTCDate = fixedUtcNow.ToString(dateFormat);
                            string beginUTCTime = fixedUtcNow.ToString(timeFormat);
                            string endUTCDate = fixedUtcNow.AddHours(1).ToString(dateFormat);
                            string endUTCTime = fixedUtcNow.AddHours(1).ToString(timeFormat);
                            string totalBeginUTC = beginUTCDate + " " + beginUTCTime;
                            string totalEndUTC = endUTCDate + " " + endUTCTime;

                            // 웹페이지 로드시 초기값 설정
                            controlData.Date1 = now.ToString(dateFormat);
                            controlData.Date2 = now.AddHours(1).ToString(dateFormat);
                            controlData.Time1 = now.ToString(timeFormat);
                            controlData.Time2 = now.AddHours(1).ToString(timeFormat);

                            query1 = $"SELECT COUNT(*) AS Cnt FROM log.packetlog " +
                                     $"WHERE '{totalBeginUTC}' <= UTCCreateTime AND UTCCreateTime <= '{totalEndUTC}';";

                            query2 = $"SELECT UniqueID, UTCNowTick, UTCCreateTime, PID, WID, SID, PacketName, ErrorCode, JsonReqData, JsonDetailData FROM log.packetlog " +
                                     $"WHERE '{totalBeginUTC}' <= UTCCreateTime AND UTCCreateTime <= '{totalEndUTC}' " +
                                     $"ORDER BY UTCNowTick DESC " +
                                     $"LIMIT {controlData.RowLimit} OFFSET {offset};";
                        }
                        break;
                    case 1: // 검색
                        {
                            // PIDQuery, WIDQuery, SIDQuery 변수는 경우에 따라 검색에서 제외하기 위해 쓰였습니다.
                            string PIDQuery = (controlData.PID == null || controlData.PID.Length == 0) ? string.Empty : $"AND PID = '{controlData.PID}'";
                            string WIDQuery = (controlData.WID == 0) ? string.Empty : $"AND WID = {controlData.WID}";
                            string SIDQuery = (controlData.SID == 0) ? string.Empty : $"AND SID = {controlData.SID}";
                            string totalBeginDateTime = controlData.Date1 + " " + controlData.Time1;
                            string totalEndDateTime = controlData.Date2 + " " + controlData.Time2;

                            // 선택된 UTC 시간 파싱
                            DateTime targetDateTime1 = DateTime.Parse(totalBeginDateTime);
                            DateTime targetDateTime2 = DateTime.Parse(totalEndDateTime);
                            if (utcAddMinusString.Contains('+'))
                            {
                                totalBeginDateTime = targetDateTime1.AddSeconds(-totalAddTime).ToString(dateTimeFormat);
                                totalEndDateTime = targetDateTime2.AddSeconds(-totalAddTime).ToString(dateTimeFormat);
                            }
                            else
                            {
                                totalBeginDateTime = targetDateTime1.AddSeconds(totalAddTime).ToString(dateTimeFormat);
                                totalEndDateTime = targetDateTime2.AddSeconds(totalAddTime).ToString(dateTimeFormat);
                            }

                            query1 = $"SELECT COUNT(*) AS Cnt FROM log.packetlog " +
                                     $"WHERE '{totalBeginDateTime}' <= UTCCreateTime AND UTCCreateTime <= '{totalEndDateTime}' " +
                                     $"{PIDQuery} {WIDQuery} {SIDQuery};";

                            query2 = $"SELECT UniqueID, UTCNowTick, UTCCreateTime, PID, WID, SID, PacketName, ErrorCode, JsonReqData, JsonDetailData FROM log.packetlog " +
                                     $"WHERE '{totalBeginDateTime}' <= UTCCreateTime AND UTCCreateTime <= '{totalEndDateTime}' " +
                                     $"{PIDQuery} {WIDQuery} {SIDQuery} " +
                                     $"ORDER BY UTCNowTick " +
                                     $"DESC LIMIT {controlData.RowLimit} OFFSET {offset};";
                        }
                        break;
                    default:
                        NLogManager.Instance.Log("LogController.UserLog / 검색타입이 맞지 않습니다. SearchType: " + controlData.SearchType);
                        return View("LogController.UserLog / 검색타입이 맞지 않습니다. SearchType: " + controlData.SearchType);
                }

                try
                {
                    MySqlCommand countCommand = new MySqlCommand(query1, connection);
                    rowCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync().ConfigureAwait(false));
                }
                catch (Exception ex)
                {
                    NLogManager.Instance.Log(ex.Message);
                    return View(ex.ToString());
                }

                if (rowCount > 0)
                {
                    try
                    {
                        MySqlCommand logCommand = new MySqlCommand(query2, connection);
                        using (MySqlDataReader reader = await logCommand.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                PacketLog packetLog = CreatePacketLog(reader);
                                // 웹에서 보여줄 시간은 UTC시간이 아니고 각 나라를 기준으로 한 시간이기 때문에 UTC시간(UTCCreateTime)에 값을 더하거나 빼줍니다.
                                if (utcAddMinusString.Contains('+'))
                                {
                                    packetLog.UTCCreateTime = packetLog.UTCCreateTime.AddSeconds(totalAddTime);
                                }
                                else
                                {
                                    packetLog.UTCCreateTime = packetLog.UTCCreateTime.AddSeconds(-totalAddTime);
                                }

                                packetLogList.Add(packetLog);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogManager.Instance.Log(ex.Message);
                        return View(ex.ToString());
                    }
                }
            }

            // UTC 시간 목록 추가
            List<string> utcList = new List<string>
            {
                "+14:00:00",
                "+13:00:00",
                "+12:45:00",
                "+12:00:00",
                "+11:30:00",
                "+11:00:00",
                "+10:30:00",
                "+10:00:00",
                "+09:30:00",
                "+09:00:00",
                "+08:45:00",
                "+08:00:00",
                "+07:00:00",
                "+06:30:00",
                "+06:00:00",
                "+05:45:00",
                "+05:30:00",
                "+05:00:00",
                "+04:30:00",
                "+04:00:00",
                "+03:30:00",
                "+03:00:00",
                "+02:00:00",
                "+01:00:00",
                "+00:00:00",
                "-01:00:00",
                "-02:00:00",
                "-03:00:00",
                "-03:30:00",
                "-04:00:00",
                "-05:00:00",
                "-06:00:00",
                "-07:00:00",
                "-08:00:00",
                "-09:00:00",
                "-09:30:00",
                "-10:00:00",
                "-11:00:00",
                "-12:00:00"
            };

            controlData.MaxPage = (rowCount / controlData.PageLimit) + 1;
            ViewBag.PacketLogList = packetLogList;
            ViewBag.WIDList = widList;
            ViewBag.SIDList = sidList;
            ViewBag.UTCList = utcList;
            ViewBag.ControlData = controlData;

            return View();
        }

        private PacketLog CreatePacketLog(MySqlDataReader reader)
        {
            PacketLog packetLog = new PacketLog();
            packetLog.UniqueID = reader.GetInt64("UniqueID");
            //packetLog.UTCNowTick = reader.GetInt64("UTCNowTick");
            packetLog.UTCCreateTime = reader.GetDateTime("UTCCreateTime");
            packetLog.PID = reader.GetString("PID");
            packetLog.WID = reader.GetInt32("WID");
            packetLog.SID = reader.GetInt32("SID");
            packetLog.PacketName = reader.GetString("PacketName");
            packetLog.ErrorCode = reader.GetInt32("ErrorCode");
            packetLog.JsonReqData = reader.GetString("JsonReqData");
            packetLog.JsonDetailData = reader.GetString("JsonDetailData");

            return packetLog;
        }
    }
}
