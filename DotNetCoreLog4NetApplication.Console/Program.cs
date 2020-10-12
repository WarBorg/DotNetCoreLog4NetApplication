using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace DotNetCoreLog4NetApplication.ConsoleUI
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()
                                                                          .DeclaringType);

        static void Main(string[] args)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();

            //Use DB in project directory.  If it does not exist, create it:
            connectionStringBuilder.DataSource = "./log4net.db";

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                //Create a table (drop if already exists first):

                var delTableCmd = connection.CreateCommand();
                delTableCmd.CommandText = "DROP TABLE IF EXISTS Log";
                delTableCmd.ExecuteNonQuery();

                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = "CREATE TABLE Log (" +
                                             "LogId INTEGER PRIMARY KEY," +
                                             "Date DATETIME NOT NULL," +
                                             "Level VARCHAR(50) NOT NULL," +
                                             "Logger VARCHAR(255) NOT NULL," +
                                             "Message TEXT DEFAULT NULL" +
                                             "); ";
                createTableCmd.ExecuteNonQuery();
            }

            // Load configuration
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Console.WriteLine("Hello world!");

            // Log some things
            log.Info("Hello logging world!");
            log.Error("Error!");
            log.Warn("Warn!");
            log.Debug("Debug!!");

            Console.ReadLine();
        }
    }
}
