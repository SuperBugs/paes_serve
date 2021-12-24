
namespace paems.Interfaces
{


    public class UMTMAddReq
    {
        public string token { get; set; }
        public string machine_type { get; set; }
        public string num { get; set; }
    }


    public class UMTMAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }


    public class UMTMDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class UMTMDeleteRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }
    }

    public class UMTMRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class UMTMRetrieveRes
    {
        public string success { get; set; }
        public UMTMData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class UMTMData
    {
        public UMTMResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class UMTMResult
    {
        public decimal id { get; set; }
        public string machine_type { get; set; }
    }

}
