

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

}
