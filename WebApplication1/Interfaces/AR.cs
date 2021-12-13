

namespace paems.Interfaces
{
    public class ARUnChamberSearchReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public string testProject { get; set; }
        public string deviceName { get; set; }
        public string[] filters { get; set; }
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
        public decimal id { get; set; }
        public string name { get; set; }
        public string num { get; set; }
        public string status { get; set; }
        public string test_stage { get; set; }
        public string test_item { get; set; }
        public string test_count { get; set; }
        public string lend_time { get; set; }
        public string return_time { get; set; }
    }
}
