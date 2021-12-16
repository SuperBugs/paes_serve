

namespace paems.Interfaces
{
    public class ARUnChamberSearchReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public string testProject { get; set; }
        public string deviceName { get; set; }
        public string testStage { get; set; }
        public string status { get; set; }
    }

    public class ARUnChamberSearchRes
    {
        public string success { get; set; }
        public ARUnChamberSearchData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class ARUnChamberSearchData
    {
        public ARUnChamberSearchResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class ARUnChamberSearchResult
    {
        public string num { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string lab { get; set; }
        public string status { get; set; }
        public string test_stage { get; set; }
        public string test_item { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string return_staff { get; set; }
    }

    public class UnChamberARCancelReq
    {
        public string token { get; set; }
        public decimal id { get; set; }
    }

    public class UnChamberARCancelRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }
    }

    public class ARChamberSearchReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public string testProject { get; set; }
        public string deviceName { get; set; }
        public string testStage { get; set; }
        public string status { get; set; }
    }

    public class ARChamberSearchRes
    {
        public string success { get; set; }
        public ARChamberSearchData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class ARChamberSearchData
    {
        public ARChamberSearchResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class ARChamberSearchResult
    {
        public string num { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string lab { get; set; }
        public string status { get; set; }
        public string test_stage { get; set; }
        public string test_item { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string return_staff { get; set; }
    }

    public class ChamberARCancelReq
    {
        public string token { get; set; }
        public decimal id { get; set; }
    }

    public class ChamberARCancelRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }
    }
}
