
namespace paems.Interfaces
{


    public class CMAddReq
    {
        public string token { get; set; }
        public string type { get; set; }
    }


    public class CMAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }


    public class CMDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class CMDeleteRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }
    }

    public class CMChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string type { get; set; }
    }


    public class CMChangeRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }

    }



    public class CMRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class CMRetrieveRes
    {
        public string success { get; set; }
        public CMData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class CMData
    {
        public CMResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class CMResult
    {
        public decimal id { get; set; }
        public string type { get; set; }
    }

}
