using System.Configuration;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Net.Mail;

namespace Encrypt_Decrypt_UBL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region  Event
        //    private void Encrypt_Click(object sender, EventArgs e)
        //    {
        //        string clearText = txt_Encrypt.Text;

        //        if (!string.IsNullOrEmpty(clearText))
        //        {
        //            try
        //            {
        //                string encryptedText = EncryptPassword(clearText);

        //                txt_Decrypt.Text = encryptedText;
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Encryption failed: {ex.Message}");
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please enter text to encrypt.");
        //        }
        //    }

        //private void Decrypt_Click(object sender, EventArgs e)
        //    {
        //        string cipherText = txt_Decrypt.Text;

        //        // Decrypt the text
        //        if (!string.IsNullOrEmpty(cipherText))
        //        {
        //            try
        //            {
        //                string decryptedText = DecryptPassword(cipherText);

        //                txt_Encrypt.Text = decryptedText;
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Decryption failed: {ex.Message}");
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please enter text to decrypt.");
        //        }
        //    }
        #endregion

        #region  Function
        public string EncryptPassword(string clearText)
        {
            string key = "MAKV2SPBNI99212";
            byte[] keyBytes = GenerateKey(key);
            byte[] iv = GenerateIV(12);
            byte[] plainBytes = Encoding.UTF8.GetBytes(clearText);

            using (AesGcm aesGcm = new AesGcm(keyBytes))
            {
                byte[] cipherBytes = new byte[plainBytes.Length];
                byte[] tag = new byte[16];

                aesGcm.Encrypt(iv, plainBytes, cipherBytes, tag);
                return $"{Convert.ToBase64String(iv)}.{Convert.ToBase64String(cipherBytes)}.{Convert.ToBase64String(tag)}";
            }
        }
        public string DecryptPassword(string cipherText)
        {
            string key = "MAKV2SPBNI99212";
            byte[] keyBytes = GenerateKey(key);
            string[] parts = cipherText.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid cipher text format");

            byte[] iv = Convert.FromBase64String(parts[0]);
            byte[] cipherBytes = Convert.FromBase64String(parts[1]);
            byte[] tag = Convert.FromBase64String(parts[2]);
            using (AesGcm aesGcm = new AesGcm(keyBytes))
            {
                byte[] plainBytes = new byte[cipherBytes.Length];
                aesGcm.Decrypt(iv, cipherBytes, tag, plainBytes);
                return Encoding.UTF8.GetString(plainBytes);
            }
        }
        private byte[] GenerateKey(string plainKey)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(plainKey)).Take(32).ToArray();
            }
        }
        private byte[] GenerateIV(int size)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] iv = new byte[size];
                rng.GetBytes(iv);
                return iv;
            }
        }
        #endregion

        // for connection string
        private void Encrypt_Click(object sender, EventArgs e)
        {
            string clearText = txt_Input.Text;

            if (!string.IsNullOrEmpty(clearText))
            {
                try
                {
                    string encryptedText = EncryptPassword(clearText);

                    txt_Result.Text = encryptedText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Encryption failed: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please enter text to encrypt.");
            }
        }

        private void Decrypt_Click(object sender, EventArgs e)
        {
            string cipherText = txt_Input.Text;

            // Decrypt the text
            if (!string.IsNullOrEmpty(cipherText))
            {
                try
                {
                    string decryptedText = DecryptPassword(cipherText);

                    txt_Result.Text = decryptedText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Decryption failed: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please enter text to decrypt.");
            }

        }


        // for password encrypt
        private void button1_Click(object sender, EventArgs e)
        {
            string clearText = txt_Input.Text;

            if (!string.IsNullOrEmpty(clearText))
            {
                try
                {
                    string encryptedText = HashPassword(clearText);

                    txt_Result.Text = encryptedText;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Encryption failed: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please enter text to encrypt.");
            }
        }

        public string HashPassword(string password)
        {
            byte[] salt = GenerateSalt(16);
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] hash = argon2.GetBytes(32);
                return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

            }
        }

        private byte[] GenerateSalt(int size)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[size];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string emailRecipientsString = "ali.kamal@bmcsolution.com";
            List<string> emailRecipients = emailRecipientsString.Split(',').ToList();
            string EmailSubject = "Check Sending Emails to Group ";
            string EmailBody = "Check Sending Emails to Group ";
            SendEmailToUserss(emailRecipients, EmailBody, EmailSubject);
        }

        public bool SendEmailToUserss(List<string> EmailList, string EmailBody, string EmailSubject, List<string> ccList = null, List<string> attachments = null)
        {
            bool CheckEmail = false;
            try
            {
                string smtpServer = txt_server.Text.ToString();  //"mail.bmcsolution.com";
                int smtpPort = Convert.ToInt32(txt_port.Text.ToString());  //587;

                //string smtpUsername = "muhammad.faraz@bmcsolution.com";
                //string smtpPassword = "F@r@z#0312";

                string smtpUsername = txt_username.Text.ToString();  //"ali.kamal@bmcsolution.com";
                string smtpPassword = txt_password.Text.ToString();  //"K@m@1#0300";

                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // Use TLS 1.2

                string subject = EmailSubject;
                string body = EmailBody;

                string EnableSSL = "true";
                bool ssl_condition = Convert.ToBoolean(chk_ssl.Checked);     //true;
                bool.TryParse(EnableSSL, out ssl_condition);

                SmtpClient client = new SmtpClient(smtpServer, smtpPort);
                client.UseDefaultCredentials = false;
                client.EnableSsl = ssl_condition;
                //client.Timeout = 100000;


                // SMTP class to send Email
                if (smtpPassword != null && smtpPassword != string.Empty)
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                }

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
                return CheckEmail;
            }
        }


        public bool VerifyHashedPassword(string enteredPassword, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(enteredPassword)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] computedHash = argon2.GetBytes(32);

                return storedHashBytes.SequenceEqual(computedHash);
            }
        }

        private void btn_very_hashed_pw_Click(object sender, EventArgs e)
        {
            string enteredPassword = txt_Input.Text;
            string storedHash = txt_Result.Text;
            // storedHash usually DB se aata hai

            bool ok = VerifyHashedPassword(enteredPassword, storedHash);

            if (ok)
            {
                MessageBox.Show("Password Match — Login Success");
            }
            else
            {
                MessageBox.Show("Password Wrong!");
            }
        }


        //public bool SendEmailToUsers(List<string> EmailList, string EmailBody, string EmailSubject, List<string> ccList = null, List<string> attachments = null)
        //{
        //    bool CheckEmail = false;
        //    try
        //    {
        //        string smtpServer = "mail.bmcsolution.com";
        //        int smtpPort = 465;
        //        string smtpUsername = "muhammad.faraz@bmcsolution.com";
        //        string smtpPassword = "F@r@z#0312";
        //        string subject = EmailSubject;
        //        string body = EmailBody;

        //        SmtpClient client = new SmtpClient(smtpServer, smtpPort)
        //        {
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
        //            EnableSsl = true
        //        };


        //        MailMessage mail = new MailMessage
        //        {
        //            From = new MailAddress(smtpUsername, "Check")
        //        };

        //        foreach (var to in EmailList)
        //        {

        //            var emails = to.Split(',');
        //            foreach (var email in emails)
        //            {
        //                mail.To.Add(email.Trim());
        //            }

        //        }

        //        mail.Subject = subject;
        //        mail.Body = body;
        //        mail.IsBodyHtml = true;

        //        if (ccList != null && ccList.Count > 0)
        //        {
        //            foreach (var cc in ccList)
        //            {
        //                mail.CC.Add(cc);
        //            }
        //        }

        //        if (attachments != null && attachments.Count > 0)
        //        {
        //            foreach (var attach in attachments)
        //            {
        //                mail.Attachments.Add(new Attachment(attach));
        //            }
        //        }


        //        client.Send(mail);
        //        Console.Write("Email Sent Successfully");
        //        CheckEmail = true;
        //        return CheckEmail;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error on Sending Mail" + ex.Message);
        //        return CheckEmail;
        //    }
        //}

    }
}