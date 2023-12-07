using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatabaseSync
{
    internal class Program
    {

        private const string configurationFile = "appsettings.json";

        //IAM
        private static string iamUrl = "";
        private static string iamUsername = "";
        private static string iamPassword = "";
        private static string iamRealm = "";
        private static string iamClientId = "";

        //MSSQL
        private static string dburlMSSQL = "";
        private static string dbcatalogMSSQL = "";
        private static string dbunameMSSQL = "";
        private static string dbupassMSSQL = "";
        private static string selectQuery = "";

        //Target
        private static string targetAPI = "";

        private static readonly HttpClient client = new HttpClient();


        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            if (!File.Exists("appsettings.json"))
            {
                Console.WriteLine("Can not find file " + configurationFile);
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Load configuration");
            LoadConfiguration();

            Console.WriteLine("Get all products from database");
            List<Product> products = GetAllProductInMSSQL(selectQuery);
            Console.WriteLine(products.Count);
            if (products.Count == 0)
            {
                Console.WriteLine("No Products");
                Console.ReadLine();
                return;
            }

            string token = GetBearerTokenAsyncMaster(iamUsername, iamPassword, iamClientId, iamRealm, iamUrl).Result;

            if (token == null)
            {
                Console.WriteLine("No token");
                Console.ReadLine();
                return;
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            SendDeleteRequest(token).GetAwaiter().GetResult();
            SendProductsToAPI(products, token).GetAwaiter().GetResult();


            Console.WriteLine("Finished!");
            Console.ReadLine();

        }

        private static void LoadConfiguration()
        {
            string jsonString = File.ReadAllText("appsettings.json");
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                iamUrl = doc.RootElement.GetProperty("IAM").GetProperty("Url").GetString();
                iamUsername = doc.RootElement.GetProperty("IAM").GetProperty("Username").GetString();
                iamPassword = doc.RootElement.GetProperty("IAM").GetProperty("Password").GetString();
                iamRealm = doc.RootElement.GetProperty("IAM").GetProperty("Realm").GetString();
                iamClientId = doc.RootElement.GetProperty("IAM").GetProperty("ClientId").GetString();

                dburlMSSQL = doc.RootElement.GetProperty("MSSQL").GetProperty("Url").GetString();
                dbcatalogMSSQL = doc.RootElement.GetProperty("MSSQL").GetProperty("Catalog").GetString();
                dbunameMSSQL = doc.RootElement.GetProperty("MSSQL").GetProperty("Username").GetString();
                dbupassMSSQL = doc.RootElement.GetProperty("MSSQL").GetProperty("Password").GetString();
                selectQuery = doc.RootElement.GetProperty("MSSQL").GetProperty("ProductQuery").GetString();

                targetAPI = doc.RootElement.GetProperty("Target").GetProperty("API").GetString();
            }
        }


        //MSSQL
        private static SqlConnection GetMSSQLConn()
        {
            string connetionString;
            SqlConnection cnn;
            connetionString = @"Data Source=" + dburlMSSQL + "\\SQLEXPRESS;Initial Catalog=" + dbcatalogMSSQL + ";User ID=" + dbunameMSSQL + ";Password=" + dbupassMSSQL + "";
            cnn = new SqlConnection(connetionString);
            return cnn;
        }
        private static bool CheckConnectionMSSQL()
        {
            SqlConnection conn = GetMSSQLConn();
            try
            {
                conn.Open();
                conn.Close();
                return true;
            }

            catch (Exception ex) { return false; }
        }
        private static List<Product> GetAllProductInMSSQL(string sqlQuery)
        {
            List<Product> products = new List<Product>();
            Console.WriteLine("Get MSSQLConnection");
            SqlConnection cnn = GetMSSQLConn();
            Console.WriteLine("Open cnn");
            cnn.Open();
            Console.WriteLine("Create query " + sqlQuery);;
            SqlCommand command = new SqlCommand(sqlQuery, cnn);
            Console.WriteLine("Execute query");
            SqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                try
                {
                    Product product = new Product();
                    product.productId = Int32.Parse(dataReader.GetString(0));

                    // productName is a string
                    product.productName = dataReader.IsDBNull(1) ? null : dataReader.GetString(1);

                    // Assuming weight is an integer and stored as a string
                    product.weight = dataReader.IsDBNull(2) ? null : dataReader.GetString(2);

                    // description is a string
                    product.description = dataReader.IsDBNull(2) ? null : dataReader.GetString(2);

                    // brand is a string
                    product.brand = dataReader.IsDBNull(3) || dataReader.GetString(3) == "" ? "Sontiges" : dataReader.GetString(3);

                    // category is a string
                    product.category = dataReader.IsDBNull(4) || dataReader.GetString(4) == "" ? "Sontiges" : dataReader.GetString(4);

                    // subCategory is a string
                    product.subCategory = dataReader.IsDBNull(5) || dataReader.GetString(5) == "" ? "Sontiges"  : dataReader.GetString(5);

                    // Assuming pack is an integer and stored as a double
                    product.pack = dataReader.IsDBNull(6) ? 0 : Convert.ToInt32(dataReader.GetDouble(6));

                    // searchIndex is a string
                    product.searchIndex = dataReader.IsDBNull(7) ? null : dataReader.GetString(7);

                    // gtinUnit is a string
                    product.gtinUnit = dataReader.IsDBNull(8) ? null : dataReader.GetString(8);

                    // Assuming price is a double
                    product.price = dataReader.IsDBNull(9) ? 0.0 : dataReader.GetDouble(9);

                    // Assuming stock is an integer and stored as a double
                    product.stock = dataReader.IsDBNull(10) ? 0 : Convert.ToInt32(dataReader.GetDouble(10));

                    // origin is a string
                    product.origin = dataReader.IsDBNull(11) || dataReader.GetString(11) == "" ? "Sontiges" : dataReader.GetString(11);

                    // gtinPack is a string
                    product.gtinPack = dataReader.IsDBNull(12) ? null : dataReader.GetString(12);
                    // Assuming discount is a boolean
                    product.discount = !dataReader.IsDBNull(13) && dataReader.GetInt16(13) == 1;

                    // Assuming tax is a double
                    product.tax = dataReader.IsDBNull(14) ? 0.0 : dataReader.GetDouble(14);

                    // Assuming lastModified is a DateTime
                    if (!dataReader.IsDBNull(15))
                        product.lastModified = dataReader.GetDateTime(15);
                    products.Add(product);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message + " " + ex.StackTrace); break; }
            }
            dataReader.Close();
            cnn.Close();
            return products;
        }

        //IAM
        private static async Task<string> GetBearerTokenAsyncMaster(string iamUser, string iamPassword, string iamClientId, string iamRealm, string iamUrl)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var tokenUrl = iamUrl + "/realms/" + iamRealm + "/protocol/openid-connect/token";
                var tokenData = "client_id=" + iamClientId + "&username=" + iamUser + "&password=" + iamPassword + "&grant_type=password&scope=openid";
                Console.WriteLine(tokenUrl);
                var content = new StringContent(tokenData, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await httpClient.PostAsync(tokenUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to obtain the bearer token. Status code: {response.StatusCode}");
                    return null;
                }

                var token = JsonConvert.DeserializeObject<dynamic>(responseContent)?.access_token;
                return token;
            }
        }

        //API Server
        private static async Task SendDeleteRequest(string bearerToken)
        {
            try
            {
                // Send a DELETE request to the specified URL
                var response = await client.DeleteAsync(targetAPI+"/products");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Delete request successful.");
                    // Optionally read the response body here
                }
                else
                {
                    Console.WriteLine($"Delete request failed. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred in delete request: {ex.Message}");
            }
        }


        private static async Task SendProductsToAPI(List<Product> products, string token)
        {
            foreach (var product in products)
            {
                try
                {
                    string json = System.Text.Json.JsonSerializer.Serialize(product);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Replace 'yourApiUrl' with the actual URL of the API
                    var response = await client.PostAsync(targetAPI+"/product/add", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Product sent successfully: " + product.productId);
                        // Optionally read the response body here
                    }
                    else
                    {
                        Console.WriteLine($"Failed to send product {product.productId}. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while sending product {product.productId}: {ex.Message}");
                }
            }
        }







    }
}
