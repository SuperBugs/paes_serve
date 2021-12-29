using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace paems.Interfaces
{
    public class EmailBean
    {

        public string emailCenterUrl { get; set; }
        public String fromName { get; set; }


        public String body { get; set; }

        public String subject { get; set; }


        public List<String> ToList { get; set; }

        public List<String> ccList { get; set; }
        public List<String> attachPathList { get; set; }

        public String systemName { get; set; }

        public EmailBean(string fromName, string body, string subject, List<string> toList, List<string> ccList, List<string> attachPathList,string emailCenterUrl)
        {
            this.fromName = fromName;
            this.body = body;
            this.subject = subject;
            this.ToList = toList;
            this.ccList = ccList;
            this.systemName = "StudyMapConfig";
            this.attachPathList = attachPathList;
            this.emailCenterUrl = emailCenterUrl;
        }

        public EmailBean()
        {

        }
    }
}
