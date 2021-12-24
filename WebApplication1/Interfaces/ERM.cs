
namespace paems.Interfaces
{


    public class ERMAddReq
    {
        public string token { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string machine_type { get; set; }
    }


    public class ERMAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }


    public class ERMDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class ERMDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class ERMChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string machine_type { get; set; }

    }


    public class ERMChangeRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }

    }



    public class ERMRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class ERMRetrieveRes
    {
        public string success { get; set; }
        public ERMData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class ERMData
    {
        public ERMResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class ERMResult
    {
        public decimal id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string machine_type { get; set; }

    }

}
