using System.Data.SqlClient;

namespace RevenueBillingApi.Models.Authentication
{
    public class LogsDAL
    {
        public static void generate_log(string UserID, string requestType, string accessToken, IConfiguration config)
        {
            using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("AM_DATABASE")))//new SqlConnection(DataEncryptor.DecryptPassword(Environment.GetEnvironmentVariable("AM_AUTHENTICATION")))
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
                using (SqlCommand sqlCommand = new SqlCommand("sp_add_requestslog", connection))
                {
                    sqlCommand.Parameters.AddWithValue("@userid", UserID);
                    sqlCommand.Parameters.AddWithValue("@req_type", requestType);
                    sqlCommand.Parameters.AddWithValue("@token", accessToken);
                    sqlCommand.Parameters.AddWithValue("@req_time", DateTime.Now);
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.ExecuteScalar();
                }
            }
        }
    }
}