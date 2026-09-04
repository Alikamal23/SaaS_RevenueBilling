using System.Net.Mail;
using System.Net;
using RevenueBillingApi.Extensions;
using System.Data;
using RevenueBillingApi.Context;
using System.Collections.Specialized;

namespace RevenueBillingApi.Utility
{
    public class SendEmail
    {
        private readonly DataEncryptor _dataencryptor;
        public SendEmail(DataEncryptor dataencryptor)
        {
            _dataencryptor = dataencryptor;
        }

        public bool SendEmailToUsers(List<string> EmailList, string EmailBody, string EmailSubject, DataAccessLayer _DAL, List<string> ccList = null, List<string> attachments = null)
        {
            bool CheckEmail = false;
            try
            {
                DataTable? smtpsettings = GetSMTPSettings(_DAL);
                // SMTP INFORMATION
                if (smtpsettings != null && smtpsettings.Rows.Count > 0)
                {
                    string smtpServer = smtpsettings.Rows[0]["Smtp"].ToString();
                    int smtpPort = Convert.ToInt32(smtpsettings.Rows[0]["SmtpPort"].ToString());
                    string smtpUsername = smtpsettings.Rows[0]["SenderEmailID"].ToString();
                    string smtpPassword = _dataencryptor.DecryptPassword(smtpsettings.Rows[0]["SmtpPassword"].ToString());
                    bool isSSL = Convert.ToBoolean(smtpsettings.Rows[0]["EnableSSL"].ToString());

                    string subject = EmailSubject;
                    string body = EmailBody;
                    // SMTP class to send Email
                    SmtpClient client = new SmtpClient(smtpServer, smtpPort)
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                        EnableSsl = isSSL
                    };

                    // Create Compose Email
                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress(smtpUsername)
                    };

                    foreach (var to in EmailList)
                    {
                        mail.To.Add(to);
                    }

                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true; 

                    // ADDING CC RECIPIENT PROVIDER
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

                    // SENDING MAIL
                    client.Send(mail);
                    Console.Write("Email Sent Successfully");
                    CheckEmail = true;
                    return CheckEmail;
                }
                else
                {
                    return CheckEmail;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on Sending Mail" + ex.Message);
                return CheckEmail;
            }
        }

        private DataTable? GetSMTPSettings(DataAccessLayer DAL)
        {
            DataTable? dt = new DataTable();
            try
            {
                NameValueCollection nv = new NameValueCollection();
                nv.Clear();
                nv.Add("WithPassword-INT", "1");
                dt = DAL.GetData("sp_select_SmtpSettings", nv, DAL.CSManagementPortalDatabase);

                if (dt.Rows.Count > 0)
                {
                    return dt;
                }
            }
            catch (Exception ex)
            {
                return dt;
            }
            return dt;
        }

    }
}
