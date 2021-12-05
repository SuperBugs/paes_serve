

namespace paems.Interfaces
{
    public class UnChamberSearchReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public string lab { get; set; }
        public string deviceName { get; set; }
        public string[] filters { get; set; }
        public string[] date { get; set; }
    }

    public class UnChamberSearchRes
    {
        public string success { get; set; }
        public UnChamberSearchData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class UnChamberSearchData
    {
        public UnChamberSearchResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class UnChamberSearchResult
    {
        public string num { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string lab { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string test_type { get; set; }
        public string lend_time { get; set; }
        public string return_time { get; set; }
        public string return_staff { get; set; }
    }

    public class ChamberSearchReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public string test_count { get; set; }
        public string test_project { get; set; }
        public string[] filters { get; set; }
        public string[] date { get; set; }
    }

    public class ChamberSearchRes
    {
        public string success { get; set; }
        public ChamberSearchData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class ChamberSearchData
    {
        public ChamberSearchResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class ChamberSearchResult
    {
        public string num { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string lab { get; set; }
        public string status { get; set; }
        public string remain_count { get; set; }
        public string use_count { get; set; }
        public string lend_time { get; set; }
        public string return_time { get; set; }
        public string return_staffs { get; set; }
    }

    public class UnChamberAEReq
    {
        public string token { get; set; }
        public decimal id { get; set; }
        public decimal test_count { get; set; }
        public string customer { get; set; }
        public string test_type { get; set; }
        public string test_stage { get; set; }
        public string test_program { get; set; }
        public string test_target { get; set; }
        public string lend_time { get; set; }
        public string return_time { get; set; }
    }

    public class UnChamberAERes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }

    }

    public class UnChamberScheduleReq
    {
        public string token { get; set; }
        public decimal id { get; set; }

    }

    public class UnChamberScheduleRes
    {
        public string success { get; set; }
        public UnChamberScheduleData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class UnChamberScheduleData
    {
        public UnChamberScheduleResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class UnChamberScheduleResult
    {
        public string name { get; set; }
        public string num { get; set; }
        public string phone { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
    }
    public class UnChamberQueryDeviceNameReq
    {
        public string token { get; set; }
        public string query { get; set; }
    }
    public class UnChamberQueryDeviceNameRes
    {
        public string success { get; set; }
        public UnChamberQueryDeviceNameData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class UnChamberQueryDeviceNameData
    {
        public UnChamberQueryDeviceNameResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class UnChamberQueryDeviceNameResult
    {
        public string name { get; set; }

    }

    public class UnChamberQueryLabNameReq
    {
        public string token { get; set; }
        public string query { get; set; }
    }
    public class UnChamberQueryLabNameRes
    {
        public string success { get; set; }
        public UnChamberQueryLabNameData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class UnChamberQueryLabNameData
    {
        public UnChamberQueryLabNameResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class UnChamberQueryLabNameResult
    {
        public string name { get; set; }

    }

    public class UnChamberQueryProjectNameReq
    {
        public string token { get; set; }
        public string query { get; set; }
    }
    public class UnChamberQueryProjectNameRes
    {
        public string success { get; set; }
        public UnChamberQueryProjectNameData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class UnChamberQueryProjectNameData
    {
        public UnChamberQueryProjectNameResult[] result { get; set; }
        public UnChamberQueryCustomerResult[] customer { get; set; }
        public UnChamberQueryMachineTypeResult[] machine { get; set; }
        public decimal total { get; set; }

    }
    public class UnChamberQueryProjectNameResult
    {
        public string name { get; set; }

    }
    public class UnChamberQueryCustomerResult
    {
        public string value { get; set; }
        public string label { get; set; }

    }
    public class UnChamberQueryMachineTypeResult
    {
        public string label { get; set; }
        public string value { get; set; }

    }

    public class ChamberQueryProjectNameReq
    {
        public string token { get; set; }
        public string query { get; set; }
    }
    public class ChamberQueryProjectNameRes
    {
        public string success { get; set; }
        public ChamberQueryProjectNameData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class ChamberQueryProjectNameData
    {
        public ChamberQueryProjectNameResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class ChamberQueryProjectNameResult
    {
        public string name { get; set; }

    }
}
