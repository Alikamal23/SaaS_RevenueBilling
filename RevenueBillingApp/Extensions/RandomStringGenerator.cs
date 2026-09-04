using System.Text;

namespace RevenueBillingApp.Extensions
{
    public class RandomStringGenerator
    {
        public string GetRandomString()
        {
            // Define the character set from which to generate the random string
            string charSet = Environment.GetEnvironmentVariable("AM_CHAR_SET");

            // Include Unix Time in the character set
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
            charSet += unixTime.ToString();

            // Define the length of the random string
            int length = 14; // Adjust the length as needed

            // Create a StringBuilder to store the random string
            StringBuilder randomString = new StringBuilder();

            // Create a Random object
            Random random = new Random();

            // Generate random characters and append them to the StringBuilder
            for (int i = 0; i < length; i++)
            {
                randomString.Append(charSet[random.Next(charSet.Length)]);
            }
            return randomString.ToString();
        }
    }
}
