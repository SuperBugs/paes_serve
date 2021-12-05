

namespace paems.Interfaces
{
    public class UserLoginReq
    {
        public string username { get; set; }
        public string password { get; set; }
    }


    public class UserLoginRes
    {
        public string success { get; set; }
        // data 为token
        public string data { get; set; }
        public string errorMessage { get; set; }

    }

    public class UserCurrentReq
    {
        public string token { get; set; }
    }


    public class UserCurrentRes
    {
        public string success { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string userid { get; set; }
        public string access { get; set; }
        public string errorMessage { get; set; }
    }

}
