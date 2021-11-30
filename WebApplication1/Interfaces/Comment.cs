using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace paems.Interfaces
{
    public class CommentAddReq
    {
        public string token { get; set; }
        public string date { get; set; }
        public string category { get; set; }
        public string classify { get; set; }
        public string issue_description { get; set; }
        public string comment_type { get; set; }
        public string content { get; set; }
        public string content_id { get; set; }
        public string feedback_comments { get; set; }
        public string model { get; set; }
        public string producturl { get; set; }
        public string imageurls { get; set; }
        public string comment_address { get; set; }
    }


    public class CommentAddRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class CommentDeleteReq
    {

        public string token { get; set; }
        public decimal id { get; set; }

    }


    public class CommentDeleteRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class CommentChangeReq
    {

        public string token { get; set; }
        public decimal id { get; set; }
        public string date { get; set; }
        public string category { get; set; }
        public string classify { get; set; }
        public string issue_description { get; set; }
        public string comment_type { get; set; }
        public string content { get; set; }
        public string content_id { get; set; }
        public string feedback_comments { get; set; }
        public string model { get; set; }
        public string producturl { get; set; }
        public string imageurls { get; set; }
        public string comment_address { get; set; }
    }


    public class CommentChangeRes
    {
        public string success { get; set; }

        public string errorMessage { get; set; }

    }

    public class CommentRetrieveReq
    {
        public string token { get; set; }
        public decimal pageSize { get; set; }
        public decimal currentPage { get; set; }
        public decimal id { get; set; }
    }


    public class CommentRetrieveRes
    {
        public string success { get; set; }
        public CommentData data { get; set; }
        public string errorMessage { get; set; }

    }

    public class CommentData
    {
        public CommentResult[] result { get; set; }
        public decimal total { get; set; }

    }
    public class CommentResult
    {
        public decimal id { get; set; }
        public string date { get; set; }
        public string category { get; set; }
        public string classify { get; set; }
        public string issue_description { get; set; }
        public string comment_type { get; set; }
        public string content { get; set; }
        public string content_id { get; set; }
        public string feedback_comments { get; set; }
        public string model { get; set; }
        public string producturl { get; set; }
        public string imageurls { get; set; }
        public string comment_address { get; set; }

    }
}
