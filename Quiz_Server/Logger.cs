///
/// FILE            : Logger.cs
/// PROJECT         : Quiz_Server - Demo Day
/// PROGRAMMER     : Edward Boado
/// FIRST VERSION   : 2021-11-24
/// DESCRIPTION     : This file contains the Logger class, which will write status and error messages to a txt file.
///                   The class uses a try/catch to handle exceptions. 
/// 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_Server
{
    /// <summary>
    /// CLASS NAME  : Logger
    /// DESCRIPTION : This class will will write status and error messages to a txt file. It uses try/catches to handle exceptions.
    ///               If writing to the file fails, it will instead write to the EventLog. 
    /// </summary>
    public class Logger
    {
        public string logPath;      // A string to hold the path to the log file

        /// <summary>
        /// METHOD NAME : Logger -- CONSTRUCTOR
        /// DESCRIPTION : This constructor will initialize a new Logger object and write to a specified file.
        /// </summary>
        /// <param name="logFileName"></param>
        public Logger(string logFileName)
        {
            try
            {
                // Get .exe directory path
                string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                // Get the absolute path to the log file in the current exe directory
                logPath = exePath + "\\" + logFileName;

                // Check for existing file
                if (!File.Exists(logPath))
                {
                    // Create a new file if none exists
                    using (FileStream fs = File.Create(logPath))
                        fs.Close();
                }
            }
            catch
            {

                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine(now + " : [ERROR] - Logger class failed to create or find the specified log file: " + logPath);
            }
        }

        /// <summary>
        /// METHOD NAME : Log
        /// DESCRIPTION : This method will log messages to a text file. The message is specified as a parameter, and printed
        ///               to the file along with date and time.
        /// </summary>
        /// <param name="message"></param>
        public bool Log(string message)
        {
            bool success = false;

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // If the parameter is empty, don't write anything to the log
            if (message != null && message != "")
            {
                try
                {
                    // Check for existing file
                    if (!File.Exists(logPath))
                    {
                        // Create a new file if none exists
                        using (FileStream fs = File.Create(logPath))
                            fs.Close();
                    }

                    // Get the file
                    FileInfo fileToWrite = new FileInfo(logPath);

                    // Open file for appending
                    using (StreamWriter sw = fileToWrite.AppendText())
                    {
                        // Write error string into file
                        sw.WriteLine(now + " - " + message);
                        success = true;
                    }
                }
                catch
                {
                    Console.WriteLine(now + " : [ERROR] - Could not write to the default log file:\n>>> " + message);
                    success = false;
                }
            }
            return success;
        }


        /// <summary>
        /// METHOD NAME : OpenLog
        /// DESCRIPTION : This method will open log files, read the text and return it as a string
        /// </summary>
        /// \returns string logString - the log file contents
        public string OpenLog()
        {
            string logContents = "";

            try
            {
                // Check if the file exists
                if (File.Exists(logPath))
                {
                    // Open file to read
                    using (StreamReader log = new StreamReader(logPath))
                    {
                        string logLine;
                        // Get the lines from the database
                        while ((logLine = log.ReadLine()) != null)
                        {
                            logContents += logLine + "\n";
                        }
                    }
                }
            }
            catch
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine(now + " : [ERROR] - could not open the log file:\n>>> " + logPath); 
            }
            return logContents;
        }
    }
}
