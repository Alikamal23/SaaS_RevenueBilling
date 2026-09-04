using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Data;
using RevenueBillingApi.Extensions;

namespace RevenueBillingApi.Context
{
    public class DataAccessLayer
    {
        public string CSManagementPortalDatabase;
        private readonly ILogger<DataAccessLayer> _logger;
        public DataAccessLayer(IConfiguration config, ILogger<DataAccessLayer> logger, CommonMethods CommonMethods)
        {
            CSManagementPortalDatabase = CommonMethods.DecryptPassword(Environment.GetEnvironmentVariable("AM_DATABASE"));
            _logger = logger;
        }

        public DataTable ExecuteQuery(string Query, string CNString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection())
            {
                try
                {
                    con.ConnectionString = CNString;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandTimeout = 0;
                    cmd.Connection = con;
                    cmd.CommandText = Query;

                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    da.Dispose();
                }

                catch (Exception ex)
                {
                    _logger.LogError("{0} {1} {2}", Query, "ExecuteQuery", ex.Message);
                }

                finally
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            return dt;
        }
        public bool CheckData(string spname, NameValueCollection nv, string CNString)
        {
            bool Result = false;
            using (SqlConnection connection = new SqlConnection())
            {
                try
                {
                    connection.ConnectionString = CNString;
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = connection;
                    cmd.CommandText = spname;
                    cmd.CommandTimeout = 999999999;

                    string dbTyper = "";

                    if (nv != null)
                    {
                        for (int i = 0; i < nv.Count; i++)
                        {
                            string[] arraySplit = nv.Keys[i].Split('-');

                            if (arraySplit.Length > 2)
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString() + "," + arraySplit[2].ToString();

                                if (nv[i].ToString() == "NULL")
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                                }
                            }
                            else
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString();

                                if (nv[i].ToString() == "NULL")
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                                }
                            }
                        }
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    Result = reader.HasRows;
                    return Result;
                }
                catch (Exception ex)
                {
                    _logger.LogError("{0} {1} {2}", spname, "CheckData", ex.Message);
                    return false;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
        public DataTable GetData(string SpName, NameValueCollection NV, string CNString)
        {
            string dbTyper = "";
            using (SqlConnection con = new SqlConnection())
            {
                try
                {
                    con.ConnectionString = CNString;
                    DataTable dt = new DataTable();
                    if (con.State == ConnectionState.Open)
                    {
                        con.Open();
                    }

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.CommandText = SpName;
                    cmd.CommandTimeout = 999999999;

                    if (NV != null)
                    {

                        for (int i = 0; i < NV.Count; i++)
                        {
                            string[] arraySplit = NV.Keys[i].Split('-');

                            if (arraySplit.Length > 2)
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString() + "," + arraySplit[2].ToString();

                                if (NV[i].ToString() == "NULL")
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = NV[i].ToString();
                                }
                            }
                            else
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString();

                                if (NV[i].ToString() == "NULL")
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = NV[i].ToString();
                                }
                            }
                        }
                    }

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    da.Dispose();

                    return dt;
                }
                catch (Exception ex)
                {
                    _logger.LogError("{0} {1} {2}", SpName, "GetData", ex.Message);
                    ////return null;
                    throw;   // return null mat karo
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
        }
        public DataSet GetDataSet(string SpName, NameValueCollection NV, string CNString)
        {
            using (SqlConnection con = new SqlConnection(CNString))
            {
                DataSet ds = new DataSet();

                try
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();

                    SqlCommand cmd = new SqlCommand(SpName, con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 999999999;

                    if (NV != null)
                    {
                        for (int i = 0; i < NV.Count; i++)
                        {
                            string[] arraySplit = NV.Keys[i].Split('-');
                            string paramName = arraySplit[0];
                            string paramType = arraySplit[1];

                            if (NV[i].ToString() == "NULL")
                            {
                                cmd.Parameters.AddWithValue(paramName, DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(paramName, NV[i].ToString());
                            }
                        }
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);   // <-- Fills ALL result sets: Master + Detail

                    return ds;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        public bool InsertData(string SpName, NameValueCollection nv, string CNString)
        {
            bool Result = true;
            string dbTyper = "";
            using (SqlConnection con = new SqlConnection())
            {
                try
                {
                    con.ConnectionString = CNString;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    SqlCommand com = new SqlCommand();
                    com.CommandType = CommandType.StoredProcedure;
                    com.Connection = con;
                    com.CommandText = SpName;

                    if (nv != null)
                    {
                        for (int i = 0; i < nv.Count; i++)
                        {
                            string[] arraySplit = nv.Keys[i].Split('-');

                            if (arraySplit.Length > 2)
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString() + "," + arraySplit[2].ToString();

                                if (nv[i].ToString() == "NULL")
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                                }
                            }
                            else
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString();

                                if (nv[i].ToString() == "NULL")
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                                }
                            }
                        }
                    }

                    int j = com.ExecuteNonQuery();

                    if (j != 0)
                    {
                        Result = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("{0} {1} {2}", SpName, "InsertData", ex.Message);
                    Result = false;
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            return Result;
        }
        public DataTable GetData(string SpName, NameValueCollection NV, SqlConnection con, SqlTransaction transaction)
        {
            string dbTyper = "";

            try
            {
                DataTable dt = new DataTable();


                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                cmd.Transaction = transaction;
                cmd.CommandText = SpName;
                cmd.CommandTimeout = 999999999;

                if (NV != null)
                {

                    for (int i = 0; i < NV.Count; i++)
                    {
                        string[] arraySplit = NV.Keys[i].Split('-');

                        if (arraySplit.Length > 2)
                        {
                            dbTyper = "SqlDbType." + arraySplit[1].ToString() + "," + arraySplit[2].ToString();

                            if (NV[i].ToString() == "NULL")
                            {
                                cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = NV[i].ToString();
                            }
                        }
                        else
                        {
                            dbTyper = "SqlDbType." + arraySplit[1].ToString();

                            if (NV[i].ToString() == "NULL")
                            {
                                cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = NV[i].ToString();
                            }
                        }
                    }
                }

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(dt);
                da.Dispose();

                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public bool InsertData(string SpName, NameValueCollection nv, SqlConnection con, SqlTransaction transaction)
        {
            bool Result = true;
            string dbTyper = "";

            try
            {


                SqlCommand com = new SqlCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.Connection = con;
                com.Transaction = transaction;
                com.CommandText = SpName;

                if (nv != null)
                {
                    for (int i = 0; i < nv.Count; i++)
                    {
                        string[] arraySplit = nv.Keys[i].Split('-');

                        if (arraySplit.Length > 2)
                        {
                            dbTyper = "SqlDbType." + arraySplit[1].ToString() + "," + arraySplit[2].ToString();

                            if (nv[i].ToString() == "NULL")
                            {
                                com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                            }
                            else
                            {
                                com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                            }
                        }
                        else
                        {
                            dbTyper = "SqlDbType." + arraySplit[1].ToString();

                            if (nv[i].ToString() == "NULL")
                            {
                                com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                            }
                            else
                            {
                                com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                            }
                        }
                    }
                }

                int j = com.ExecuteNonQuery();

                if (j != 0)
                {
                    Result = true;
                }
            }
            catch (Exception ex)
            {
                Result = false;
            }
            finally
            {

            }

            return Result;
        }
        public bool InsertDataWithType(string SpName, NameValueCollection nv, string[] TypeParameterName, DataSet ds, string CNString)
        {
            bool Result = true;
            string dbTyper = "";
            using (SqlConnection con = new SqlConnection())
            {
                try
                {
                    con.ConnectionString = CNString;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    SqlCommand com = new SqlCommand();
                    com.CommandType = CommandType.StoredProcedure;
                    com.Connection = con;
                    com.CommandText = SpName;

                    if (nv != null)
                    {
                        for (int i = 0; i < nv.Count; i++)
                        {
                            string[] arraySplit = nv.Keys[i].Split('-');

                            if (arraySplit.Length > 2)
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString() + "," + arraySplit[2].ToString();

                                if (nv[i].ToString() == "NULL")
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                                }
                            }
                            else
                            {
                                dbTyper = "SqlDbType." + arraySplit[1].ToString();

                                if (nv[i].ToString() == "NULL")
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = DBNull.Value;
                                }
                                else
                                {
                                    com.Parameters.AddWithValue(arraySplit[0].ToString(), dbTyper).Value = nv[i].ToString();
                                }
                            }
                        }
                    }

                    for (int i = 0; i < TypeParameterName.Length; i++)
                    {
                        com.Parameters.AddWithValue(TypeParameterName[i], ds.Tables[i]);
                    }

                    int j = com.ExecuteNonQuery();

                    if (j != 0)
                    {
                        Result = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("{0} {1} {2}", SpName, "InsertDataWithType", ex.Message);
                    Result = false;
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            return Result;
        }
        public bool InsertDataBulk(string SpName, string ParameterName, DataTable dt, string CNString)
        {
            bool Result = true;

            using (SqlConnection con = new SqlConnection())
            {
                try
                {
                    con.ConnectionString = CNString;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    SqlCommand com = new SqlCommand();
                    com.CommandType = CommandType.StoredProcedure;
                    com.Connection = con;
                    com.CommandText = SpName;
                    com.Parameters.AddWithValue(ParameterName, dt);
                    int j = com.ExecuteNonQuery();

                    if (j != 0)
                    {
                        Result = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("{0} {1} {2}", SpName, "InsertDataBulk", ex.Message);
                    Result = false;
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            return Result;
        }
        public DataTable GetDataAfterBulk(string SpName, string ParameterName, DataTable dt, string CNString)
        {
            bool Result = true;

            using (SqlConnection con = new SqlConnection())
            {
                try
                {
                    DataTable table = new DataTable();
                    con.ConnectionString = CNString;
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    SqlCommand com = new SqlCommand();
                    com.CommandType = CommandType.StoredProcedure;
                    com.Connection = con;
                    com.CommandText = SpName;
                    com.Parameters.AddWithValue(ParameterName, dt);

                    //int j = com.ExecuteNonQuery();

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = com;
                    da.Fill(table);
                    return table;
                }
                catch (Exception ex)
                {
                    _logger.LogError("{0} {1} {2}", SpName, "GetDataAfterBulk", ex.Message);
                    Result = false;
                    return null;
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
        }
        public void Dispose() { }

    }
}
