using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
namespace RealEstateCRM.Web.BLL
{
    public class EmailHelper
    {
        static string Server = System.Configuration.ConfigurationManager.AppSettings["EmailServer"];
        static string User = System.Configuration.ConfigurationManager.AppSettings["EmailSender"];
        static string Password = System.Configuration.ConfigurationManager.AppSettings["EmailSenderPassword"];
        static public void SendMail(string mailto, string subject, string body)
        {

            if (Server == null || User == null || Password == null) return;
            
            if (mailto == null || Regex.IsMatch(mailto, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$") == false)
            {
                //Console.WriteLine("not email");
                return;
            }
            try
            {
                SmtpClient client = new SmtpClient(Server);
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(User, Password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                MailMessage message = new MailMessage(User, mailto, subject, body);
                //Attachment attachment = new System.Net.Mail.Attachment("c:\\log.log");
                //message.Attachments.Add(attachment);
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                client.Send(message);
            }
            catch
            {
            }
        }
    }
}