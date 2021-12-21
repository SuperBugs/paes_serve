
namespace paems.Interfaces
{

    public class OMChamberDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class OMChamberDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class OMUnChamberDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class OMUnChamberDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class OMChamberRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class OMChamberRetrieveRes
    {
        public string success { get; set; }
        public OMChamberData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class OMChamberData
    {
        public OMChamberResult[] result { get; set; }
        public decimal total { get; set; }
        
    }
    public class OMChamberResult
    { 
        public decimal id { get; set; }
        public string machine_id { get; set; }
        public string staff_num { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string order_time { get; set; }
        public string customer_type { get; set; }
        public string test_machine_type { get; set; }
        public string test_stage { get; set; }
        public string test_item { get; set; }
        public string test_count { get; set; }
        public string test_target { get; set; }
        public string status { get; set; }
        public string time_order_id { get; set; }
    }

    public class OMUnChamberRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
        public string[] filters { get; set; }
    }


    public class OMUnChamberRetrieveRes
    {
        public string success { get; set; }
        public OMUnChamberData data { get; set; }
        public string errorMessage { get; set; }
    }

    public class OMUnChamberData
    {
        public OMUnChamberResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class OMUnChamberResult
    {
        public decimal id { get; set; }
        public string machine_id { get; set; }
        public string staff_num { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string order_time { get; set; }
        public string customer_type { get; set; }
        public string test_machine_type { get; set; }
        public string test_stage { get; set; }
        public string test_item { get; set; }
        public string test_count { get; set; }
        public string test_target { get; set; }
        public string status { get; set; }
    }

   
}
