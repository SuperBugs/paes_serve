using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace paems.Interfaces
{
    public class SendMailContent
    {
        /// <summary>
        /// 邮件主题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 邮件主体
        /// </summary>
        public MessageBody Body { get; set; }
        /// <summary>
        /// 发送人列表
        /// </summary>
        public List<EmailAddress> ToList { get; set; }
        /// <summary>
        /// 抄送人列表
        /// </summary>
        public List<EmailAddress> CcList { get; set; }
        /// <summary>
        /// 密件抄送收件人列表
        /// </summary>
        public List<EmailAddress> BccList { get; set; }
        /// <summary>
        /// 发送后是否保存到已发送邮件列表
        /// </summary>
        public bool IsSendSave { get; set; }
        /// <summary>
        /// 附件文件路径列表
        /// </summary>
        public List<string> AttachmentFileNames { get; set; }

        /// <summary>
        /// 内嵌照片路径和其对应的CID名称
        /// </summary>
        public Dictionary<string, string> InnerPictures { get; set; }

    }
}
