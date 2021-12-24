
namespace paems.Interfaces
{
    public class UserAdminAddReq
    {
        public string token { get; set; }
        public string num { get; set; }
        public string name { get; set; }
        public string pass { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string role { get; set; }
    }



    public class UserAdminAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class UserAdminDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class UserAdminDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class UserAdminChangeReq
    {

        public string token { get; set; }
        public string num { get; set; }
        public decimal id { get; set; }
        public string name { get; set; }
        public string pass { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        
    }


    public class UserAdminChangeRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class UserAdminRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public string[] filters { get; set; }
    }


    public class UserAdminRetrieveRes
    {
        public string success { get; set; }
        public UserAdminData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class UserAdminData
    {
        public UserAdminResult[] result { get; set; }
        public decimal total { get; set; }
        
    }
    public class UserAdminResult
    { 
        public decimal id { get; set; }
        public string num { get; set; }
        public string name { get; set; }
        public string pass { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string role { get; set; }

    }

}
