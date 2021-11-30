
namespace paems.Interfaces
{
    public class ClassifyAddReq
    {
        public string token { get; set; }
        public string content { get; set; }
        public decimal fatherId { get; set; }
    }


    public class ClassifyAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class ClassifyDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class ClassifyDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class ClassifyChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string content { get; set; }
    }


    public class ClassifyChangeRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class ClassifyRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
    }


    public class ClassifyRetrieveRes
    {
        public string success { get; set; }
        public ClassifyData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class ClassifyData
    {
        public ClassifyResult[] result { get; set; }
        public decimal total { get; set; }
        
    }
    public class ClassifyResult
    { 
        public decimal key { get; set; }
        public decimal id { get; set; }
        public string content { get; set; }

    }

}
