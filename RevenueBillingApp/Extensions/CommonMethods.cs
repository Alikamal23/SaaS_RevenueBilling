using Konscious.Security.Cryptography;
using RevenueBillingApp.Models.Authentication;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace RevenueBillingApp.Extensions
{
    public class CommonMethods
    {

        private readonly IConfiguration _config;
        private static int SaltSize;
        private static int KeySize;
        private static int Iterations;
        private static string EncryptionKey_;

        public CommonMethods(IConfiguration config)
        {
            _config = config;

            SaltSize = Convert.ToInt32(Environment.GetEnvironmentVariable("SALT_SIZE"));
            //KeySize = Convert.ToInt32(Environment.GetEnvironmentVariable("KEY_SIZE"));
            //Iterations = Convert.ToInt32(Environment.GetEnvironmentVariable("ITERATIONS"));
            EncryptionKey_ = Environment.GetEnvironmentVariable("xpt956wxp");
        }

        public List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row => {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            if (row[pro.Name].GetType() != typeof(System.DBNull))
                            {
                                pro.SetValue(objT, row[pro.Name]);
                            }
                        }
                        catch (Exception ex)
                        {
                            var e = ex.Message;
                        }
                    }
                }
                return objT;
            }).ToList();
        }
        public DataTable ConvertCsvToDataTable(string filePath)
        {
            DataTable dataTable = new DataTable();
            bool isFirstRow = true;

            using (var streamReader = new StreamReader(filePath))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var values = line.Split(',');

                    if (isFirstRow)
                    {
                        foreach (var value in values)
                        {
                            dataTable.Columns.Add(value);
                        }

                        DataColumn dataColumn2 = new DataColumn("Sequence", typeof(int));
                        dataColumn2.AutoIncrement = true;
                        dataColumn2.AutoIncrementSeed = 1;
                        dataColumn2.AutoIncrementStep = 1;
                        dataTable.Columns.Add(dataColumn2);
                        
                        isFirstRow = false;
                    }
                    else
                    {
                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < values.Length; i++)
                        {
                            // Check for empty or null values
                            if (!string.IsNullOrEmpty(values[i]))
                            {
                                if (dataTable.Columns[i].ColumnName == "Date")
                                {
                                    // Convert and format the date column
                                    DateTime date;
                                    if (DateTime.TryParse(values[i], out date))
                                        row[i] = date.ToString("yyyy-MM-dd");
                                    else
                                        row[i] = DBNull.Value;
                                }
                                if (dataTable.Columns[i].ColumnName == "DateTime")
                                {
                                    // Convert and format the date column
                                    DateTime date;
                                    if (DateTime.TryParse(values[i], out date))
                                        row[i] = date.ToString("yyyy-MM-dd HH:mm:ss");
                                    else
                                        row[i] = DBNull.Value;
                                }
                                else
                                {
                                    row[i] = values[i];
                                }
                            }
                            else
                            {
                                row[i] = DBNull.Value; // Set DBNull for empty values
                            }
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }

        public List<Form> CreateObject(List<Navigation> navigation)
        {
            List<Form> headerTree = FillRecursive(navigation, 0);
            return headerTree;
        }


        private List<Form> FillRecursive(List<Navigation> flatObjects, int? parentId = null)
        {
            return flatObjects.Where(x => x.ParentFormID.Equals(parentId)).Select(item => new Form
            {
                FormDisplayName = item.FormDisplayName,
                FormID = item.FormID,
                FormController = item.FormController,
                FormAction = item.FormAction,
                iconclass = item.iconclass,
                Childrens = FillRecursive(flatObjects, item.FormID)
            }).ToList();
        }




        public string EncryptPassword(string clearText)
        {
            string key = EncryptionKey_;
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
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                return string.Empty;
            }
            string key = EncryptionKey_;
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
        public bool VerifyPassword(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split('.');

           

            if (parts.Length != 2) return false;

            byte[] saltt = Convert.FromBase64String(parts[0]);
            byte[] hashToCompare = Convert.FromBase64String(parts[1]);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = saltt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] computedHash = argon2.GetBytes(32);
                return computedHash.SequenceEqual(hashToCompare);
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


    }
}
