 /*
  * FILE            : Server.cs
  * PROJECT         : Quiz_Server - Demo Day
  * PROGRAMMER     : Edward Boado
  * FIRST VERSION   : 2021 - 12 - 04
  * DESCRIPTION     : This file contains the Server class, which will listen and connect with clients via TCP socket.
  *                   When a client connects, it will parse the received strings, call GameInstance methods to build responses,
  *                   and then return the response before disconnecting.
  */                  


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quiz_Server
{
    public sealed class Server
    {
        public static volatile bool Done = false;                       // Volatile bool for start/stop
        private static readonly object lockServer = new object();       // Make critical code thread safe
        private static Server gameServer = null;                        // A private instance of the server

        // Private constructor
        private Server()
        {
        }

        // Method to create/return an instance, and only allow one instance
        public static Server getServerInstance
        {
            get
            {
                // make thread safe
                lock (lockServer)
                {
                    if (gameServer == null)
                    {
                        gameServer = new Server();
                    }
                    return gameServer;
                }
            }
        }

        /*
        *	NAME	:	Run
        *	PURPOSE	:	This method will initiate running of the server by calling the Listen method until
        *	            the service is stopped.
        *	INPUTS	:	None
        *	RETURNS	:	None
        */
        public void Run()
        {
            while (!Done)
            {
                Listen();
            }
        }

        /*
        *	NAME	:	Listen
        *	PURPOSE	:	This asynchronous method will listen for a client request, connect with the client,
        *	            and use multithreading to assign processing tasks in parallel.
        *	INPUTS	:	None
        *	RETURNS	:	void Task
        */
        public static async Task Listen()
        {
            TcpListener server = null;

            string readIP = ConfigurationManager.AppSettings.Get("ip");
            string readPort = ConfigurationManager.AppSettings.Get("port");

            // Validate port and IP
            bool readPortSuccess = Int32.TryParse(readPort, out int parsedPort);
            bool readIPSuccess = IPAddress.TryParse(readIP, out IPAddress parsedIP);

            IPAddress localIP = null;
            Int32 port = 0;

            // Assign properties if valid
            if (readPortSuccess && parsedPort > 0)
            {
                port = parsedPort;
            }
            if (readIPSuccess)
            {
                localIP = parsedIP;
            }

            try
            {
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localIP, port);

                // Start listening for client requests.

                server.Start();

                // Enter the listening loop.
                while (!Done)
                {
                    if (server.Pending())
                    {
                        // Get a new connection
                        TcpClient client = server.AcceptTcpClient();

                        // Create a task and supply delegate
                        Task processRequest = new Task(() => HandleRequest(client));
                        // New task run by a thread while main thread returns to listening loop
                        processRequest.Start();
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            }
            // Handle exeptions
            catch
            {
                string logFile = ConfigurationManager.AppSettings.Get("serverLogFile");
                Logger serverLog = new Logger(logFile);
                serverLog.Log("Could not establish connection to a client.");
            }
            finally
            {
                // Stop listening for new clients.
                if (server != null)
                {
                    server.Stop();
                }
            }
        }


        /*
        *	NAME	:	HandleRequest
        *	PURPOSE	:	This method will allow a single task thread to handle a client request in parallel.
        *	INPUTS	:	Object clientObject - holds the TCPClient object that was connected to the client
        *	RETURNS	:	void 
        */
        public static void HandleRequest(Object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;
            // Buffer for reading data
            Byte[] bytes = new Byte[1024];
            string packageReceived = null;

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();
            Console.WriteLine("[CONNECTED] - Connected to client");

            // Get data from socket and convert to string
            try
            {
                // Convert bytes to ascii string.   
                int numBytes = stream.Read(bytes, 0, bytes.Length);
                packageReceived = System.Text.Encoding.ASCII.GetString(bytes, 0, numBytes);
            }
            catch
            {
                string logFile = ConfigurationManager.AppSettings.Get("serverLogFile");
                Logger serverLog = new Logger(logFile);
                serverLog.Log("[ERROR] Could not read data from client");
                Console.WriteLine("[ERROR] Could not read data from client");
            }

            // Parse the received string
            Console.WriteLine("[RECEIVED] - Data from client received");
            string packageToSend = ParseReceived(packageReceived);                    

            // Send response back
            try
            {
                // Convert string response to bytes and send to client
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(packageToSend);
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("[SENT] - Response sent to client");
            }
            catch
            {

                string logFile = ConfigurationManager.AppSettings.Get("serverLogFile");
                Logger serverLog = new Logger(logFile);
                serverLog.Log("[ERROR] Could not write response to client");
                Console.WriteLine("[ERROR] Could not write response to client");
            }

            // Disconnect from client
            client.Close();
            Console.WriteLine("[DISCONNECTED] - Connected from client");
        }


        /*
        *	NAME	:	ParseReceived
        *	PURPOSE	:	This method will take the received string, check what action needs to be taken, 
        *	            and call method to handle it.
        *	INPUTS	:	string received - the string from the client
        *	RETURNS	:	string  responseToSend.ToString() - the returned response from the method that is called
        */
        public static string ParseReceived(string received)
        {
            GameInstance currentGame = new GameInstance();
            StringBuilder responseToSend = new StringBuilder();
            
            // Check if user wants to see the answers
            if (received.Contains("ANSWERS"))
            {
                
                responseToSend = ShowAnswers(currentGame);
            }
            // Check for userID
            else if (!received.Contains("userID"))
            {
                // If no userID, its a new game
                responseToSend = NewGame(currentGame, received);
            }            
            else
            {
                // A game is in progress
                responseToSend = ContinueGame(currentGame, received);
            }       
            return responseToSend.ToString();
        }


        /*
        *	NAME	:	ContinueGame
        *	PURPOSE	:	This method will take the received string, parse it, and build a response depending on
        *	            which question number the player is at.
        *	INPUTS	:	string received - the string from the client
        *	            GameInstance currentGame - the GameInstance object for the current game
        *	RETURNS	:	StringBuilder response - the returned response with data from the methods called
        */
        public static StringBuilder ContinueGame(GameInstance currentGame, string received)
        {
            StringBuilder response = new StringBuilder();

            // Gather data from string
            int userID = 0;
            int questionID = 0;
            int score = 0;
            int answerID = 0;

            string[] receivedFields = received.Split(';');
            for (int i = 0; i < receivedFields.Length; i++)
            {
                // Get userID
                if (receivedFields[i].Contains("userID"))
                {
                    string[] buf = receivedFields[i].Split('=');
                    userID = int.Parse(buf[1]);
                }
                // Get the question
                if (receivedFields[i].Contains("questionID"))
                {
                    string[] buf = receivedFields[i].Split('=');
                    questionID = int.Parse(buf[1]);
                }
                // Get answerID
                if (receivedFields[i].Contains("answerID"))
                {
                    string[] buf = receivedFields[i].Split('=');
                    answerID = int.Parse(buf[1]);
                }
                // Get score
                if (receivedFields[i].Contains("score"))
                {
                    string[] buf = receivedFields[i].Split('=');
                    score = int.Parse(buf[1]);
                }
            }

            // Log the score, and time to answer if applicable
            currentGame.LogScore(userID, questionID, answerID, score);

            // Check if its the last question
            if(questionID >= 10)
            {
                // Build gameover response
                response = EndGame(currentGame, received, userID);                
            }
            else
            {
                // Get next question and answers
                string question = currentGame.GetQuestion(questionID + 1);
                string answers = currentGame.GetAnswers(questionID + 1);

                // Build response string
                response.Append($"userID={userID};");
                response.Append(question);
                response.Append(answers);
            }
            return response;
        }


        /*
        *	NAME	:	EndGame
        *	PURPOSE	:	This method will get the leaderboard players and scores and return them
        *	INPUTS	:	string received - the string from the client
        *	            GameInstance currentGame - the GameInstance object for the current game
        *	            int userID
        *	RETURNS	:	StringBuilder response - the returned response with data from the methods called
        */
        public static StringBuilder EndGame(GameInstance currentGame, string received, int userID)
        {
            StringBuilder response = new StringBuilder();

            // Mark gameover 
            response.Append("GAMEOVER;");

            // Get end game scores and add them
            response.Append(currentGame.GameOver(userID));

            return response;
        }


        /*
        *	NAME	:	NewGame
        *	PURPOSE	:	This method will get client string and parse it, then call methods to create a new user ID,
        *	            then get the first question and its answers and return them
        *	INPUTS	:	string received - the string from the client
        *	            GameInstance currentGame - the GameInstance object for the current game
        *	RETURNS	:	StringBuilder response - the returned response with data from the methods called
        */
        public static StringBuilder NewGame(GameInstance currentGame, string received)
        {
            StringBuilder response = new StringBuilder();

            if (received.Contains("userName"))
            {
                // Get name string and send
                string[] user = received.Split('=');
                int userID = currentGame.NewUser(user[1]);
                // Add to response
                response.Append($"userID={userID};");

                int questionID = 1;

                // Get first question and answer
                string question = currentGame.GetQuestion(questionID);

                // Get answers
                string answers = currentGame.GetAnswers(questionID);

                // Add to string
                response.Append(question);
                response.Append(answers);
            }

            return response;
        }


        /*
        *	NAME	:	ShowAnswers
        *	PURPOSE	:	This method will get the questions and their correct answers
        *	INPUTS	:   GameInstance currentGame - the GameInstance object for the current game
        *	RETURNS	:	StringBuilder response - the returned response with data from the methods called
        */
        public static StringBuilder ShowAnswers(GameInstance currentGame)
        {
            StringBuilder response = new StringBuilder();

            // Get the question and answer texts 
            response = currentGame.ShowAnswers();

            return response;
        }
    }
}