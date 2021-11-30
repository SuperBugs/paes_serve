using paems.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using System;

namespace paems.Common
{

    public class EWSHelper
    {
        private static readonly object _lock = new object();
        private static EWSHelper _instance = null;
        /// <summary>
        /// ExchangeService对象
        /// </summary>
        private static ExchangeService service;

        private EWSHelper()
        {

        }
        public static EWSHelper GetInStance()
        {

            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
                        service.Credentials = new WebCredentials(Config.EmailName, Config.EmailPass);
                        //service.Url = new Uri("webmail.lcfuturecenter.com");
                        service.TraceEnabled = true;
                        service.TraceFlags = TraceFlags.All;
                        service.AutodiscoverUrl(Config.Email, RedirectionUrlValidationCallback);

                        _instance = new EWSHelper();
                    }
                }
            }
            return _instance;
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);
            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }

        public bool SendEmail(SendMailContent content)
        {

            //InitializeEWS();
            EmailMessage message = new EmailMessage(service);
            // 邮件主题
            message.Subject = content.Subject;
            message.Body = content.Body;
            message.ToRecipients.AddRange(content.ToList);
            if (content.CcList?.Count > 0)
                message.CcRecipients.AddRange(content.CcList);
            if (content.BccList?.Count > 0)
                message.BccRecipients.AddRange(content.CcList);
            //添加内嵌照片
            if (content.InnerPictures?.Count > 0)
            {
                int i = 0;
                foreach (var item in content.InnerPictures)
                {
                    message.Attachments.AddFileAttachment(item.Key, item.Value);
                    message.Attachments[i].IsInline = true;
                    //message.Attachments[i].ContentType=
                    message.Attachments[i].ContentId = item.Key;
                    i++;
                }
            }
            //添加附件
            if (content.AttachmentFileNames?.Count > 0)
                foreach (var item in content.AttachmentFileNames)
                {
                    message.Attachments.AddFileAttachment(item);
                }

            if (content.IsSendSave)
            {
                message.SendAndSaveCopy();
                return true;
            }
            else
            {
                message.Send();
                return true;
            }
            //}
            // 保存草稿
            //message.save();
            // 只发送不保存邮件
            // message.Send();
            // 发送并保存已发送邮件
        }


    }
}
