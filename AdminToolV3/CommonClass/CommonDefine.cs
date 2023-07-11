using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminToolV3.CommonClass
{
    public static class CommonDefine
    {
        public const string UserID = "UserID";
        // ControlData
        public const int PageLimit = 10;
        public const int RowLimit = 10;
        // KPI
        public const int NewUserDay = 14;
        public const int DauDay = 7;
        public const string UtcTime = "+09:00:00";
        // Setting
        static public IConfiguration? Configuration { get; set; }
    }
}
