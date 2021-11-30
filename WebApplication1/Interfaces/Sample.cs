using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace paems.Interfaces
{
    public class SampleAddReq
    {
        public string token { get; set; }
        public string category { get; set; }
        public string classify { get; set; }
        public string key_first { get; set; }
        public string key_second { get; set; }
        public string mode { get; set; }
    }


    public class SampleAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class SampleDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class SampleDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class SampleChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string category { get; set; }
        public string classify { get; set; }
        public string key_first { get; set; }
        public string key_second { get; set; }
        public string mode { get; set; }
    }


    public class SampleChangeRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class SampleRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
    }


    public class SampleRetrieveRes
    {
        public string success { get; set; }
        public SampleData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class SampleData
    {
        public SampleResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class SampleResult
    {
        public decimal id { get; set; }
        public string category { get; set; }
        public string classify { get; set; }
        public string key_first { get; set; }
        public string key_second { get; set; }
        public string mode { get; set; }

    }
}
