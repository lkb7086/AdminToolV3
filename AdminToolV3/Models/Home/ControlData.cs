using AdminToolV3.CommonClass;
using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace AdminToolV3.Models.Home
{
    public class ControlData
    {
        public int SearchType { get; set; }
        public int StartPage { get; set; }
        public int CurPage { get; set; }
        public int MaxPage { get; set; }

        public string Date1 { get; set; } = string.Empty;
        public string Date2 { get; set; } = string.Empty;
        public string Time1 { get; set; } = string.Empty;
        public string Time2 { get; set; } = string.Empty;

        public string PID { get; set; } = string.Empty;
        public int WID { get; set; }
        public int SID { get; set; }

        public int PageLimit { get; set; }
        public int RowLimit { get; set; }

        public string UtcTime { get; set;} = string.Empty;

        public ControlData()
        {
            SearchType = 0;
            StartPage = 1;
            CurPage = 1;
            MaxPage = 0;
            Date1 = string.Empty;
            Date2 = string.Empty;
            Time1 = DateTime.MinValue.Date.TimeOfDay.ToString();
            Time2 = DateTime.MinValue.Date.TimeOfDay.ToString();
            PID = string.Empty;
            WID = 0;
            SID = 0;
            PageLimit = CommonDefine.PageLimit;
            RowLimit = CommonDefine.RowLimit;
            UtcTime = CommonDefine.UtcTime;
        }

        public ControlData(ControlData controlData, int startPage, int curPage, int rowLimit)
        {
            if (controlData == null)
                return;

            Init(controlData);
            StartPage = startPage;
            CurPage = curPage;
            RowLimit = rowLimit;
        }

        public ControlData(int startPage, int curPage, int maxPage, int pageLimit, int searchType, string date1, string date2, string time1, string time2, string pid, int rowCount, int wid, int sid, string utcTime)
        {
            SearchType = searchType;
            StartPage = startPage;
            CurPage = curPage;
            MaxPage = maxPage;
            Date1 = date1;
            Date2 = date2;
            Time1 = time1;
            Time2 = time2;
            PID = pid;
            WID = wid;
            SID = sid;
            PageLimit = pageLimit;
            RowLimit = rowCount;
            UtcTime = utcTime;
        }

        private void Init(ControlData controlData)
        {
            SearchType = controlData.SearchType;
            StartPage = controlData.StartPage;
            CurPage = controlData.CurPage;
            MaxPage = controlData.MaxPage;
            Date1 = controlData.Date1;
            Date2 = controlData.Date2;
            Time1 = controlData.Time1;
            Time2 = controlData.Time2;
            PID = controlData.PID;
            WID = controlData.WID;
            SID = controlData.SID;
            PageLimit = controlData.PageLimit;
            RowLimit = controlData.RowLimit;
            UtcTime = controlData.UtcTime;
        }
    }
}
