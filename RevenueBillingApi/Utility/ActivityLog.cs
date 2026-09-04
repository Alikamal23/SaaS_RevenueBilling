namespace RevenueBillingApi.Utility
{
    public class ActivityLog
    {
        public static readonly int ActivityID_Login = 1;
        public static readonly int ActivityID_LogOut = 2;
        public static readonly int ActivityID_Get = 3;
        public static readonly int ActivityID_Insert = 4;
        public static readonly int ActivityID_Update = 5;
        public static readonly int ActivityID_Delete = 6;
        public static readonly int ActivityID_View = 7;
        public static readonly int ActivityID_Search = 8;
        public static readonly int ActivityID_Error = 9;

        public static readonly string ActivityDetails_Login = "Logged In";
        public static readonly string ActivityDetails_LogOut = "Logged Out";
        public static readonly string ActivityDetails_Get = "Data Is Retrived By Using " ;
        public static readonly string ActivityDetails_Insert = "Data Is Inserted By Using ";
        public static readonly string ActivityDetails_Update = "Data Is Updated By Using ";
        public static readonly string ActivityDetails_Delete = "Data Is Deleted By Using ";
        public static readonly string ActivityDetails_View = "User Visited This Page ";
        public static readonly string ActivityDetails_Search = "Data Is Searched By Using ";
        public static readonly string ActivityDetails_Get2 = "Data Is Not Retrived By Using ";
        public static readonly string ActivityDetails_Insert2 = "Data Is Not Inserted By Using ";
        public static readonly string ActivityDetails_Update2 = "Data Is Not Updated By Using ";
        public static readonly string ActivityDetails_Delete2 = "Data Is Not Deleted By Using ";
        public static readonly string ActivityDetails_Search2 = "Data Is Not Searched By Using ";

        public static string EmailSubject = "Your Randomly Generated Password";

        //public static readonly string EmailBody = "Dear User,\r\n\r\nWe hope this email finds you well. " +
        //                     "As part of our security protocols, we have generated a random password for your account. " +
        //                     "Please find your new password below:\r\n\r\n" +
        //                     "\r\n\r\nFor security reasons, we recommend changing this password to something memorable as soon as possible. " +
        //                     "Should you have any questions or concerns, please don't hesitate to reach out to our support team." +
        //                     "\r\n\r\nThank you for your attention to this matter.";

        public static readonly string EmailBody = @"
                    <!DOCTYPE html>
                    <html>
                    <head>
                    <meta charset='utf-8' />
                    </head>
                    <body style='margin:0; padding:0; background-color:#f4f5f7; font-family: Arial, Helvetica, sans-serif;'>
                        <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f5f7; padding: 30px 0;'>
                            <tr>
                                <td align='center'>
                                    <table role='presentation' width='600' cellpadding='0' cellspacing='0' style='background-color:#ffffff; border-radius:8px; overflow:hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.08);'>

                                        <!-- Header -->
                                        <tr>
                                            <td style='background-color:#1d4ed8; padding: 28px 30px;' align='center'>
                                                <h1 style='margin:0; color:#ffffff; font-size:22px; font-weight:700;'>Revenue &amp; Billing Suite</h1>
                                            </td>
                                        </tr>

                                        <!-- Body -->
                                        <tr>
                                            <td style='padding: 32px 30px;'>
                                                <p style='margin:0 0 16px; font-size:15px; color:#111827; line-height:1.6;'>Dear User,</p>

                                                <p style='margin:0 0 16px; font-size:15px; color:#374151; line-height:1.6;'>
                                                    We hope this email finds you well. As part of our security protocols, we have generated
                                                    a random password for your account. Please find your new password below:
                                                </p>

                                                <!-- Password Box -->
                                                <table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='margin: 20px 0;'>
                                                    <tr>
                                                        <td style='background-color:#f3f4f6; border-left: 4px solid #1d4ed8; padding: 16px 20px; border-radius: 4px;'>
                                                            <p style='margin:0 0 4px; font-size:12px; color:#6b7280; text-transform:uppercase; letter-spacing:0.5px;'>Your Password</p>
                                                            <p style='margin:0; font-size:20px; font-weight:700; color:#111827; font-family: Consolas, monospace; letter-spacing: 1px;'>{{PASSWORD}}</p>
                                                        </td>
                                                    </tr>
                                                </table>

                                                <p style='margin:0 0 16px; font-size:15px; color:#374151; line-height:1.6;'>
                                                    For security reasons, we recommend changing this password to something memorable as soon as possible.
                                                </p>

                                                <p style='margin:0 0 24px; font-size:15px; color:#374151; line-height:1.6;'>
                                                    Should you have any questions or concerns, please don't hesitate to reach out to our support team.
                                                </p>

                                                <p style='margin:0; font-size:15px; color:#111827; line-height:1.6;'>
                                                    Thank you,<br/>
                                                    <strong style='color:#1d4ed8;'>Revenue &amp; Billing Team</strong>
                                                </p>
                                            </td>
                                        </tr>

                                        <!-- Footer -->
                                        <tr>
                                            <td style='background-color:#f9fafb; padding: 18px 30px; text-align:center; border-top:1px solid #e5e7eb;'>
                                                <p style='margin:0; font-size:12px; color:#9ca3af;'>This is an automated system email. Please do not reply to this message.</p>
                                            </td>
                                        </tr>

                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>";



    }
}
