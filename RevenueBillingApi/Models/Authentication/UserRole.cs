using System.Data;
using System.Data.SqlClient;

namespace RevenueBillingApi.Models.Authentication
{
    public class UserRole
    {
        public static List<RolesList> GetRolesList(string userid, IConfiguration config)
        {
            using (SqlConnection connection = new SqlConnection(config.GetConnectionString("BOPAuthentication")))
            {
                using (SqlCommand sqlCommand = new SqlCommand("sp_select_userroles", connection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userid", userid);

                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            dataTable.Load(dataReader);
                            List<RolesList> rolesList = dataTable.AsEnumerable().Select(dr => new RolesList() { api_name = dr["api_name"].ToString() }).ToList();
                            if (dataTable.Rows.Count >= 1)
                            {
                                return rolesList;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
        }

    }
    public class RolesList
    {
        public string api_name { get; set; }
    }
}