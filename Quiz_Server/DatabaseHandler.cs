/*
 * FILE            : DatabaseHandler.cs
 * PROJECT         : Quiz_Server - Demo Day
 * PROGRAMMER     : Edward Boado
 * FIRST VERSION   : 2021 - 12 - 08
 * DESCRIPTION     : This file contains the DatabaseHandler class, which will connect to a MySQL database server. It will
 *                   take queries/commands, run them on the database, and then return the response data.
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Quiz_Server
{
    class DatabaseHandler
    {
        // Data members
        private MySqlConnection connection;     // The connection

        private string hostServer;              // The hostname/IP address to connect to
        protected string HostServer { get { return hostServer; } set { hostServer = value; } }

        private string databaseName;            // The name of the database 
        protected string DatabaseName { get { return databaseName; } set { databaseName = value; } }

        private string username;                // The username for MySQL access
        protected string Username { get { return username; } set { username = value; } }

        private string password;                // The password for MySQL access
        protected string Password { get { return password; } set { password = value; } }

        private Logger databaseLog = null;

        // constructor
        public DatabaseHandler()
        {
            hostServer = "";
            databaseName = "";
            username = "";
            password = "";
            connection = null;
            string logFile = ConfigurationManager.AppSettings.Get("dbLogFile");
            databaseLog = new Logger(logFile);
        }

        // login method
        public bool Connect()
        {
            bool connected = false;

            // Get configurable database server settings from file
            string ip = ConfigurationManager.AppSettings.Get("ip");
            string port = ConfigurationManager.AppSettings.Get("dbPort");
            string database = ConfigurationManager.AppSettings.Get("database");
            string username = ConfigurationManager.AppSettings.Get("user");
            string password = ConfigurationManager.AppSettings.Get("password");

            // create connection string and connection
            string connectionString = "SERVER=" + ip + ";DATABASE=" + database + ";UID=" + username + ";PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);

            // open connection using login info
            try
            {
                connection.Open();
                connected = true;
            }
            catch
            {
                connected = false;

                databaseLog.Log("[ERROR] - Could not log into the database");
            }
            return connected;
        }


        // method to disconnect
        public bool Disconnect()
        {
            bool closed = false;       

            try
            {
                connection.Close();
                closed = true;
            }
            catch
            {
                closed = false;

                string logFile = ConfigurationManager.AppSettings.Get("dbLogFile");
                Logger databaseLog = new Logger(logFile);
                databaseLog.Log("[ERROR] - Could not close the database");
            }

            return closed;
        }

        // method for executing commands
        public bool Execute(string sqlCommand)
        {
            bool executed = false;

            MySqlCommand command = new MySqlCommand(sqlCommand, connection);
            try
            {
                command.ExecuteNonQuery();
                executed = true;
            }
            catch 
            {
                executed = false;

                string logFile = ConfigurationManager.AppSettings.Get("dbLogFile");
                Logger databaseLog = new Logger(logFile);
                databaseLog.Log("[ERROR] - Could not execute command: " + sqlCommand);
            }

            return executed;
        }

        // method for select queries
        public DataTable Select(string selectQuery)
        {
            DataTable selectedData = null;

            try
            {
                // create command and adapter
                MySqlCommand selector = new MySqlCommand(selectQuery, connection);
                MySqlDataAdapter selectAdaptor = new MySqlDataAdapter(selector);

                // fill table 
                selectedData = new DataTable();
                selectAdaptor.Fill(selectedData);
            }
            catch 
            {
                string logFile = ConfigurationManager.AppSettings.Get("dbLogFile");
                Logger databaseLog = new Logger(logFile);
                databaseLog.Log("[ERROR] - Could not select command: " + selectQuery);
            }
            return selectedData;
        }
    }
}
