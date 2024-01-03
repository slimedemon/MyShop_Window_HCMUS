using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using MyShop.Model;
using MyShop.Services;
using System;
using System.Data.OleDb;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Windows.Storage;

namespace MyShop.Repository
{
    public abstract class RepositoryBase
    {
        private string _connectionString;
        private IConfigurationRoot _config;
        public RepositoryBase()
        {
            //Connect to database and verify data
            _config = new ConfigurationBuilder().AddUserSecrets<MainWindow>().Build();
        }

        protected void changeConnectionString()
        {
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
            if (_connectionString == null || _connectionString.Equals("")) 
            {
                _connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=MyShopDB;Integrated Security=True;Trust Server Certificate=True";
            }
        }
        protected SqlConnection GetConnection()
        {
            changeConnectionString();
            return new SqlConnection(_connectionString);
        }

        protected OleDbConnection GetOleSqlConnection(StorageFile file)
        {
            if (file == null) throw new Exception("Not exist file!");

            var builder = new OleDbConnectionStringBuilder();
            var provider = GetAccessDatabaseEngineProvider();
            if (provider.Equals("Provider not determined")) throw new Exception("Provider not determined!");
            builder.Provider = provider;
            builder.DataSource = file.Path;
            return new OleDbConnection(builder.ConnectionString);
        }

        private string GetAccessDatabaseEngineProvider()
        {
            string versionNumber = GetAccessDatabaseEngineVersion();

            switch (versionNumber)
            {
                case "10.0":
                    return "Microsoft.ACE.OLEDB.10.0";
                case "11.0":
                    return "Microsoft.ACE.OLEDB.11.0";
                case "12.0":
                    return "Microsoft.ACE.OLEDB.12.0";
                case "13.0":
                    return "Microsoft.ACE.OLEDB.13.0";
                case "14.0":
                    return "Microsoft.ACE.OLEDB.14.0";
                case "15.0":
                    return "Microsoft.ACE.OLEDB.15.0";
                case "16.0":
                    return "Microsoft.ACE.OLEDB.16.0";
                default:
                    return "Provider not determined";
            }
        }

        private string GetAccessDatabaseEngineVersion()
        {
            Type Access = Type.GetTypeFromProgID("Access.Application");
            if (Access != null)
            {
                dynamic access = Activator.CreateInstance(Access);
                string ver = access.Version;
                return ver;
            }

            return "Version not found";
        }
    }
}
