using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net.Util;
using MicroKnights.Log4NetHelper;
using Microsoft.Data.Sqlite;

namespace DotNetCoreLog4NetApplication.ConsoleUI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // TODO: Read from connecionstrings.json
            var connectionString = "Data Source=\"./log4net.db\"";

            // Create table
            /*using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                //Create a table (drop if already exists first):

                await using var delTableCmd = connection.CreateCommand();
                delTableCmd.CommandText = "DROP TABLE IF EXISTS Log";
                delTableCmd.ExecuteNonQuery();

                await using var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = "CREATE TABLE Log (" +
                                             "LogId INTEGER PRIMARY KEY," +
                                             "Date DATETIME NOT NULL," +
                                             "Level VARCHAR(50) NOT NULL," +
                                             "Logger VARCHAR(255) NOT NULL," +
                                             "Message TEXT DEFAULT NULL" +
                                             "); ";
                createTableCmd.ExecuteNonQuery();
            }*/

            // Load configuration
            const string Log4netConfigFilename = "log4net.config";

            if (File.Exists(Log4netConfigFilename) == false)
            {
                throw new FileNotFoundException($"{Log4netConfigFilename} not found", Log4netConfigFilename);
            }

#if DEBUG
            InternalDebugHelper.EnableInternalDebug(delegate (object source, LogReceivedEventArgs eventArgs)
            {
                Console.WriteLine(eventArgs.LogLog.Message);
                if (eventArgs.LogLog.Exception != null)
                    Console.WriteLine(eventArgs.LogLog.Exception.StackTrace);
            });
#endif

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(Log4netConfigFilename));

            Console.WriteLine("Hello world!");

            var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            // Log some things
            log.Info("Hello logging world!");
            log.Error("Error!");
            log.Warn("Warn!");
            log.Debug("Debug!!");

            // Show logged data from the database
            await ShowLogDataFromDatabaseWith(connectionString);

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        private static async Task ShowLogDataFromDatabaseWith(string connectionString)
        {
            using var connection = new SqliteConnection(connectionString);

            connection.Open();

            await using var selectCmd = connection.CreateCommand();

            selectCmd.CommandText = "select * from Log";

            await using var reader = await selectCmd.ExecuteReaderAsync();

            Console.WriteLine(string.Join("\t", Enumerable.Range(0, reader.FieldCount)
                                                          .Select(i => reader.GetName(i))));

            while (await reader.ReadAsync())
            {
                Console.WriteLine(string.Join("\t", Enumerable.Range(0, reader.FieldCount)
                                                              .Select(i => reader.GetString(i))));
            }
        }
    }
}
