
namespace paems.Interfaces
{
    public class TMChamberAddReq
    {
        public string token { get; set; }
        public string machine_type { get; set; }
        public string test_item { get; set; }
        public string test_time { get; set; }

    }


    public class TMChamberAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class TMUnChamberAddReq
    {
        public string token { get; set; }
        public string machine_type { get; set; }
        public string test_item { get; set; }
    }


    public class TMUnChamberAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class TMChamberDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class TMChamberDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class TMUnChamberDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class TMUnChamberDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class TMChamberChangeReq
    {
        public string token { get; set; }
        public decimal id { get; set; }
        public string machine_type { get; set; }
        public string test_item { get; set; }
        public string test_time { get; set; }
    }


    public class TMChamberChangeRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }
    }
    public class TMUnChamberChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string machine_type { get; set; }
        public string test_item { get; set; }
    }


    public class TMUnChamberChangeRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }

    }

    public class TMChamberRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class TMChamberRetrieveRes
    {
        public string success { get; set; }
        public TMChamberData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class TMChamberData
    {
        public TMChamberResult[] result { get; set; }
        public decimal total { get; set; }
        
    }
    public class TMChamberResult
    { 
        public decimal id { get; set; }
        public string test_time { get; set; }
        public string token { get; set; }
        public string machine_type { get; set; }
        public string test_item { get; set; }

    }

    public class TMUnChamberRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class TMUnChamberRetrieveRes
    {
        public string success { get; set; }
        public TMUnChamberData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class TMUnChamberData
    {
        public TMUnChamberResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class TMUnChamberResult
    {
        public decimal id { get; set; }
        public string token { get; set; }
        public string machine_type { get; set; }
        public string test_item { get; set; }

    }
}
