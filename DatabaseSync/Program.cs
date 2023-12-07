using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        //Target
        private static string targetAPI = "";


        static void Main(string[] args)
        {
            if (!File.Exists("appsettings.json"))
            {
                Console.WriteLine("Can not find appsettings.json");
                Console.ReadLine();
                return;
            }

            LoadConfiguration();




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

                targetAPI = doc.RootElement.GetProperty("Target").GetProperty("API").GetString();
            }
        }
    
        


    
    
    
    
    
    
    
    
    }
}
