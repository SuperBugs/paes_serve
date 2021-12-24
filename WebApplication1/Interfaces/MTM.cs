
namespace paems.Interfaces
{


    public class MTMAddReq
    {
        public string token { get; set; }
        public string machine_type { get; set; }
    }


    public class MTMAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }


    public class MTMDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class MTMDeleteRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }
    }

    public class MTMChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string machine_type { get; set; }
    }


    public class MTMChangeRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }

    }



    public class MTMRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class MTMRetrieveRes
    {
        public string success { get; set; }
        public MTMData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class MTMData
    {
        public MTMResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class MTMResult
    {
        public decimal id { get; set; }
        public string machine_type { get; set; }
    }

}
