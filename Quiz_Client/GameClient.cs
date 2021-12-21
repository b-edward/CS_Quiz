/*
* FILE          :	GameClient.cs
* PROJECT       :	
* PROGRAMMERS   :	Edward Boado
* FINAL VERSION :	2021-11-21
* DESCRIPTION   :	This file contains the GameClient class, which will facilitate communication between the user and the game server.
*                   The program will model a Hi-Lo game client. It will provide a user interface for interacting
*                   with a game server. The game will tell you if you are too high or too low in your guesses 
*                   until you win by guessing the correct number
*/


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_Client
{
    class GameClient
    {
        // Constants 
        const int MAX_STRING = Int16.MaxValue;                          // Number of bytes for buffer must be less than an Int16

        // Properties
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public Dictionary<int, string> Answers { get; set; }
        public int Selection1 { get; set; }
        public int Selection2 { get; set; }
        public int Selection3 { get; set; }
        public int Selection4 { get; set; }
        public Dictionary<string, string> Leaderboard { get; set; }             // userName, topScore
        public Dictionary<string, string> QuestionAnswerTexts { get; set; }     // The text of all questions with correct answer text
        public int CorrectAnswer { get; set; }                                  // answerID
        public int Score { get; set; }                                          // Score for current answered question
        public string Error { get; set; }
        public IPAddress IP { get; set; }
        public Int32 Port { get; set; }
        private Logger clientLog = null;


        /*
        *	NAME	:	GameClient -- CONSTRUCTOR
        *	PURPOSE	:	This constructor will initialize a GameClient object with default values.
        *	INPUTS	:	None
        *	RETURNS	:	None
        */
        public GameClient()
        {
            UserID = 0;
            UserName = null;
            CorrectAnswer = 0;
            QuestionID = 0;
            QuestionText = null;
            Score = 0;
            Leaderboard = new Dictionary<string, string>();
            Answers = new Dictionary<int, string>();
            QuestionAnswerTexts = new Dictionary<string, string>();
            Selection1 = 0;
            Selection2 = 0;
            Selection3 = 0;
            Selection4 = 0;

            string logFile = ConfigurationManager.AppSettings.Get("clientLogFile");
            clientLog = new Logger(logFile);
        }




        /*
        *	NAME	:	SendToServer
        *	PURPOSE	:	This method will establish a connection with the server, and build a package string to send a request.
        *	            It will use the established package protocol to send and receive data from the server, then call a method
        *	            to process the respone.
        *	INPUTS	:	None
        *	RETURNS	:	string status 
        */
        public string SendToServer(string stringToSend)
        {
            string status = "OK";

            string readIP = ConfigurationManager.AppSettings.Get("ip");
            string readPort = ConfigurationManager.AppSettings.Get("port");
            bool serverOK = ValidateServerInfo(readIP, readPort);

            if(serverOK)
            {
                try
                {
                    // Create a new TCP Client
                    TcpClient client = new TcpClient(IP.ToString(), Port);

                    // Translate the passed message into ASCII and store it as a Byte array.
                    byte[] data = System.Text.Encoding.ASCII.GetBytes(stringToSend);

                    // Get a client stream for reading and writing.
                    NetworkStream stream = client.GetStream();

                    // Send the message to the connected TcpServer. 
                    stream.Write(data, 0, data.Length);

                    // Reset buffer to store the server response bytes.
                    data = new byte[MAX_STRING];

                    // String to store the response ASCII representation.
                    string responseData = string.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    string packageReceived = responseData;

                    // Close connection
                    stream.Close();
                    client.Close();

                    // Parse the response
                    status = ProcessResponse(packageReceived);                
                }
                catch
                {
                    clientLog.Log("[ERROR] - Could not send to server");
                    status = "Error - could not send to server";
                }
            }
            else
            {
                clientLog.Log("[ERROR] - Server info is invalid");
                status = "Sorry, there's a problem connecting to the server";
            }            

            return status;
        }


        /*
        *	NAME	:	NewGame
        *	PURPOSE	:	This method will build a string with the game data to send to the server.
        *	INPUTS	:	None
        *	RETURNS	:	string status - error message or OK
        */
        public string NewGame()
        {
            string status = "OK";

            string sendString = "userName=" + UserName;

            status= SendToServer(sendString);

            return status;
        }


        /*
        *	NAME	:	SendAnswer
        *	PURPOSE	:	This method will build a string with the answer data to send to the server.
        *	INPUTS	:	None
        *	RETURNS	:	string status - error message or OK
        */
        public string SendAnswer(int answer, string countdown)
        {
            string status = "OK";
            StringBuilder sendString = new StringBuilder();

            // Map button selection to answerID
            switch(answer)
            {
                case 1:
                    answer = Selection1;
                    break;
                case 2:
                    answer = Selection2;
                    break;
                case 3:
                    answer = Selection3;
                    break;
                case 4:
                    answer = Selection4;
                    break;
            }

            int score = 0;
            if (answer == CorrectAnswer)
            {
                Score += int.Parse(countdown);
                score += int.Parse(countdown);
            }

            sendString.Append("userID=" + UserID + ";");
            sendString.Append("questionID=" + QuestionID + ";");
            sendString.Append("answerID=" + answer + ";");
            sendString.Append("score=" + score + ";");

            status = SendToServer(sendString.ToString());

            return status;
        }



        /*
        *	NAME	:	ProcessResponse
        *	PURPOSE	:	This method will parse the response package from the server and assign new values to data members.
        *	INPUTS	:	None
        *	RETURNS	:	bool processed - indicates the process was completed
        */
        public string ProcessResponse(string packageReceived)
        {
            string status = "OK";

            // Check if its game over
            if (packageReceived.ToUpper().Contains("GAMEOVER"))
            {
                status = GameOver(packageReceived);
                return status;
            }

            // Check if its an error message
            if (packageReceived.ToUpper().Contains("ERROR"))
            {
                // Save error and return 
                Error = packageReceived;
                status = Error;
                return status;
            }

            // Check if its the answers
            if (packageReceived.ToUpper().Contains("ANSWERS"))
            {
                status = ShowAnswers(packageReceived);
                return status;
            }

            // Parse the server response using, separating by ';'
            string[] serverResponse = packageReceived.Split(';');
            Stack<int> selectStack = new Stack<int>();

            for (int i = 0; i < serverResponse.Length; i++)
            {
                if(serverResponse[i].Contains("userID"))
                {
                    string[] userIDbuf = serverResponse[i].Split('=');
                    UserID = int.Parse(userIDbuf[1]);
                }
                if (serverResponse[i].Contains("userName"))
                {
                    string[] userNamebuf = serverResponse[i].Split('=');
                    UserName = userNamebuf[1];
                }
                if (serverResponse[i].Contains("questionID"))
                {
                    string[] userIDbuf = serverResponse[i].Split('=');
                    QuestionID = int.Parse(userIDbuf[1]);
                }
                if (serverResponse[i].Contains("questionText"))
                {
                    string[] userNamebuf = serverResponse[i].Split('=');
                    QuestionText = userNamebuf[1];
                }
                if (serverResponse[i].Contains("answer"))
                {
                    string[] answerBuf = serverResponse[i].Split('=');
                    string[] idBuf = answerBuf[0].Split(':');
                    Answers.Add(int.Parse(idBuf[1]), answerBuf[1]);
                    selectStack.Push(int.Parse(idBuf[1]));
                }
                if (serverResponse[i].Contains("isCorrect"))
                {
                    string[] correctBuf = serverResponse[i].Split('=');
                    CorrectAnswer = int.Parse(correctBuf[1]);
                }
            }
            // Map answers to selectors
            Selection4 = selectStack.Pop();
            Selection3 = selectStack.Pop();
            Selection2 = selectStack.Pop();
            Selection1 = selectStack.Pop();

            return status;
        }


        public string GameOver(string packageReceived)
        {
            string response = "GAMEOVER";

            int index = packageReceived.IndexOf("leaderboard");
            string getleaderboard = packageReceived.Substring(index);
            string getScore = packageReceived.Substring(0, index);

            // Parse leaderboard and add to dictionary
            string[] leaderboard = getleaderboard.Split(';');

            // Get each player and score
            for (int i = 0; i < leaderboard.Length; i++)
            {
                if (leaderboard[i].Contains("="))
                {
                    string[] playerScores = leaderboard[i].Split('=');
                    // Add to dictionary
                    Leaderboard.Add(playerScores[0], playerScores[1]);
                }
            }

            // Parse getScore and add topScore to Score
            string[] gameScore = getScore.Split(';');

            // Get the current game's score
            for (int i = 0; i < gameScore.Length; i++)
            {
                if (gameScore[i].Contains("currentScore"))
                {
                    string[] playerScore = gameScore[i].Split('=');
                    // Save the game's score
                    Score = int.Parse(playerScore[1]);
                }
            }

            return response;
        }

        public void GetAnswers()
        {
            SendToServer("ANSWERS");
        }


        public string ShowAnswers(string packageReceived)
        {
            string result = "OK";

            // Parse the server string
            string[] serverResponse = packageReceived.Split(';');

            for (int i = 0; i < serverResponse.Length; i++)
            {
                if (serverResponse[i].Contains("="))
                {
                    string[] texts = serverResponse[i].Split('=');

                    // Add each question and answer pair to the dictionary
                    QuestionAnswerTexts.Add(texts[0], texts[1]);
                }
            }

            return result;
        }


        /*
        *	NAME	:	ValidateServerInfo
        *	PURPOSE	:	This method will validate the user server input. It will check that the ip is a valid IP address, and 
        *	            that the port number is an integer greater than 0.
        *	INPUTS	:	string ip - the ip address input from the user
        *	            string port - the port number input from the user
        *	RETURNS	:	bool isValid - true if the ip and port are valid
        */
        public bool ValidateServerInfo(string ip, string port)
        {
            bool isValid = true;

            // Validate the port number 
            bool portOK = int.TryParse(port, out Int32 parsedPort);
            if (portOK && parsedPort > 0)
            {
                Port = parsedPort;
            }
            else
            {
                isValid = false;
            }

            // Validate the IP address
            try
            {
                IPAddress parsedIP = IPAddress.Parse(ip);
                IP = parsedIP;
            }
            catch
            {
                isValid = false;
            }


            return isValid;
        }


    }
}
