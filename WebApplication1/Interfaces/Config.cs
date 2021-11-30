
namespace paems.Interfaces
{
    public class ConfigAddReq
    {
        public string token { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string is_run { get; set; }
    }


    public class ConfigAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class ConfigDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class ConfigDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class ConfigChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string is_run { get; set; }
    }


    public class ConfigChangeRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class ConfigRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
    }


    public class ConfigRetrieveRes
    {
        public string success { get; set; }
        public ConfigData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class ConfigData
    {
        public ConfigResult[] result { get; set; }
        public decimal total { get; set; }
        
    }
    public class ConfigResult
    { 
        public string name { get; set; }
        public decimal id { get; set; }
        public string url { get; set; }
        public string end_time { get; set; }
        public string start_time { get; set; }
        public string is_run { get; set; }
    }

}
