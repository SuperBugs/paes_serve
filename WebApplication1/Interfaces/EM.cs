
namespace paems.Interfaces
{
    public class EMChamberAddReq
    {
        public string token { get; set; }
        public string name { get; set; }
        public string num { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public decimal capacity { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string lab { get; set; }
        public string return_staff { get; set; }
    }


    public class EMChamberAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class EMUnChamberAddReq
    {
        public string token { get; set; }
        public string name { get; set; }
        public string num { get; set; }
        public string type { get; set; }
        public string test_type { get; set; }
        public string status { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string lab { get; set; }
        public string return_staff { get; set; }
    }


    public class EMUnChamberAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class EMChamberDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class EMChamberDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class EMUnChamberDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class EMUnChamberDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class EMChamberChangeReq
    {
        public string token { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string num { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public decimal capacity { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string lab { get; set; }
        public string return_staff { get; set; }
    }


    public class EMChamberChangeRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }
    }
    public class EMUnChamberChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string num { get; set; }
        public string type { get; set; }
        public string test_type { get; set; }
        public string status { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string lab { get; set; }
        public string return_staff { get; set; }
    }


    public class EMUnChamberChangeRes
    {
        public string success { get; set; }
        public string errorMessage { get; set; }

    }

    public class EMChamberRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class EMChamberRetrieveRes
    {
        public string success { get; set; }
        public EMChamberData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class EMChamberData
    {
        public EMChamberResult[] result { get; set; }
        public decimal total { get; set; }
        
    }
    public class EMChamberResult
    { 
        public decimal id { get; set; }
        public string name { get; set; }
        public string num { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public decimal capacity { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string lab { get; set; }
        public string return_staff { get; set; }

    }

    public class EMUnChamberRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class EMUnChamberRetrieveRes
    {
        public string success { get; set; }
        public EMUnChamberData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class EMUnChamberData
    {
        public EMUnChamberResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class EMUnChamberResult
    {
        public decimal id { get; set; }
        public string name { get; set; }
        public string num { get; set; }
        public string type { get; set; }
        public string test_type { get; set; }
        public string status { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string lab { get; set; }
        public string return_staff { get; set; }

    }

    public class QueryChambeMachineTypeReq
    {
        public string token { get; set; }
        public string query { get; set; }
    }
    public class QueryChamberMachineTypeRes
    {
        public string success { get; set; }
        public QueryChamberMachineTypeData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class QueryChamberMachineTypeData
    {
        public QueryChamberMachineTypeResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class QueryChamberMachineTypeResult
    {
        public string name { get; set; }

    }

    public class QueryUnChamberMachineTypeReq
    {
        public string token { get; set; }
        public string query { get; set; }
    }
    public class QueryUnChamberMachineTypeRes
    {
        public string success { get; set; }
        public QueryUnChamberMachineTypeData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class QueryUnChamberMachineTypeData
    {
        public QueryUnChamberMachineTypeResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class QueryUnChamberMachineTypeResult
    {
        public string name { get; set; }

    }
}
