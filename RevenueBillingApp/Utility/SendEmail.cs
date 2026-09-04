using System.Net.Mail;
using System.Net;
using RevenueBillingApp.Extensions;
using RevenueBillingApp.Models.Authentication;

namespace RevenueBillingApp.Utility
{
    public class SendEmail
    {
        private readonly DataEncryptor _dataencryptor;
        public SendEmail(DataEncryptor dataencryptor)
        {
            _dataencryptor = dataencryptor;
        }
        public bool SendEmailToUsers(List<string> EmailList, string EmailBody, string EmailSubject, List<SMTPSettings> smtpsettings, List<string> ccList = null, List<string> attachments = null)
        {
            bool CheckEmail = false;
            try
            {
                string smtpServer = smtpsettings[0].Smtp;
                int smtpPort = Convert.ToInt32(smtpsettings[0].SmtpPort);
                string smtpUsername = smtpsettings[0].SenderEmailID;
                string smtpPassword = _dataencryptor.DecryptPassword(smtpsettings[0].SmtpPassword);
                string subject = EmailSubject;
                string body = EmailBody;

                SmtpClient client = new SmtpClient(smtpServer, smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };


                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(smtpUsername, smtpsettings[0].DisplayName)
                };

                foreach (var to in EmailList)
                {
                    mail.To.Add(to);
                }

                mail.Subject = subject;
                mail.Body = body;


                if (ccList != null && ccList.Count > 0)
                {
                    foreach (var cc in ccList)
                    {
                        mail.CC.Add(cc);
                    }
                }

                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var attach in attachments)
                    {
                        mail.Attachments.Add(new Attachment(attach));
                    }
                }


                client.Send(mail);
                Console.Write("Email Sent Successfully");
                CheckEmail = true;
                return CheckEmail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on Sending Mail" + ex.Message);
                return CheckEmail;
            }
        }
    }
}
