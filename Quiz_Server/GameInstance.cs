/*
 * FILE            : GameInstance.cs
 * PROJECT         : Quiz_Server - Demo Day
 * PROGRAMMER     : Edward Boado
 * FIRST VERSION   : 2021 - 12 - 12
 * DESCRIPTION     : This file contains the GameInstance class, which will allow the server to run the quiz game.
 *                   It will take requests, and call the database handler to build responses.
 */


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_Server
{
    public class GameInstance
    {
        private Logger gameLog = null;

        private DatabaseHandler dbHandler = null;

        // constructor
        public GameInstance()
        {
            string logFile = ConfigurationManager.AppSettings.Get("gameLogFile");
            gameLog = new Logger(logFile);
            ConnectDB();

        }


        // method to connect to the database
        public bool ConnectDB()
        {
            bool connected = false;

            dbHandler = new DatabaseHandler();

            connected = dbHandler.Connect();

            return connected;
        }


        // Enter new user in db, return the userID
        public int NewUser(string userName)
        {
            int userID = -1;

            // check if username is already in database
            DataTable user = dbHandler.Select($"SELECT * FROM `Users` WHERE userName='{userName}'");

            // if not in database, insert new username
            if(user.Rows.Count == 0)
            {
                dbHandler.Execute($"INSERT INTO `Users` (`userName`) VALUES ('{userName}')");
            }

            // get the userID and assign it to return
            user = dbHandler.Select($"SELECT * FROM `Users` WHERE userName='{userName}'");
            if (user != null)
            {
                // Check the correct tripID
                foreach (DataRow row in user.Rows)
                {
                    string buf = row.Field<string>("userName");
                    if (userName == buf)
                    {
                        userID = row.Field<int>("userID");
                        break;
                    }
                }
            }
            return userID;
        }


        // Enter the correct answer score for the user
        public void LogScore(int userID, int questionID, int answerID, int score)
        {
            // Update `Users` current score
            DataTable getScore = new DataTable();
            getScore = dbHandler.Select($"SELECT * FROM `Users` WHERE userID='{userID}';");
            int? currentScore = 0;

            // Check the data table
            if (getScore != null)
            {
                // Check the correct userID
                foreach (DataRow row in getScore.Rows)
                {
                    int buf = row.Field<int>("userID");
                    if (userID == buf)
                    {
                        // Get the users current game score
                        currentScore = row.Field<int?>("currentScore");
                        break;
                    }
                }
            }

            // set the value
            if(currentScore == null)
            {
                currentScore = 0;
            }
            currentScore += score;

            // Update the currentScore 
            dbHandler.Execute($"UPDATE `Users` SET currentScore={currentScore} WHERE userID={userID};");

            // Check if need to log the time
            // Get the correct answer for this question
            DataTable getAnswer = new DataTable();
            getAnswer = dbHandler.Select($"SELECT `answerID`, `isCorrect` FROM `QuestionAnswers` WHERE answerID={answerID};");
            int isCorrect = 0;
            // Check the data table
            if (getAnswer != null)
            {
                // Check the correct userID
                foreach (DataRow row in getAnswer.Rows)
                {
                    int buf = row.Field<int>("answerID");
                    if (answerID == buf)
                    {
                        // Get the users current game score
                        isCorrect = row.Field<int>("isCorrect");
                        break;
                    }
                }
            }

            if(isCorrect == 1)
            {
                // Insert into `CorrectAnswerTimes`
                int time = 20 - score;
                dbHandler.Execute($"INSERT INTO `CorrectAnswerTimes` (`userID`, `questionID`, `answerTime`) " +
                    $"VALUES ('{userID}', '{questionID}', '{time}')");     
            }    
        }


        // method to get the next question
        public string GetQuestion(int questionID)
        {
            // Select from db to get customers
            DatabaseHandler dbHandler = new DatabaseHandler();
            DataTable dbResponse = null;
            string theQuestion = $"questionID={questionID};";

            // Login to the local db
            bool loggedIn = dbHandler.Connect();

            if (loggedIn)
            {
                // Select from db to get users
                dbResponse = dbHandler.Select($"SELECT * FROM `Questions` WHERE questionID={questionID}");

                if (dbResponse != null)
                {
                    // Check the correct questionID
                    foreach (DataRow row in dbResponse.Rows)
                    {
                        int idBuf = row.Field<int>("questionID");
                        if(idBuf == questionID)
                        {
                            // Save the question text
                            theQuestion += "questionText=" + row.Field<string>("questionText") + ";";
                        }
                    }
                }
                else
                {
                    gameLog.Log("[ERROR] - Could not get question from the database");
                }
            }

            return theQuestion;
        }


        // method to get the answers corresponding to a question
        public string GetAnswers(int questionID)
        {
            DataTable answerTable = new DataTable();
            StringBuilder theAnswers = new StringBuilder();

            // Select from db to get customers
            DatabaseHandler dbHandler = new DatabaseHandler();

            // Login to the local db
            bool loggedIn = dbHandler.Connect();

            if (loggedIn)
            {
                // Select from db to get answerText
                string select = $"SELECT QuestionAnswers.questionID, QuestionAnswers.answerID, QuestionAnswers.isCorrect, " +
                    $"Answers.answerText FROM Answers LEFT JOIN QuestionAnswers ON Answers.answerID = QuestionAnswers.answerID " +
                    $"WHERE questionID='{questionID}';";
                answerTable = dbHandler.Select(select);

                if (answerTable != null)
                {
                    // Build a string using the returned selection
                    theAnswers.Append($"questionID={questionID};");
                    string correct = null;
                    foreach (DataRow row in answerTable.Rows)
                    {
                        string answerID = row.Field<int>("answerID").ToString();
                        string answerText = row.Field<string>("answerText");
                        theAnswers.Append($"answer:{answerID}={answerText};");

                        int isCorrect = row.Field<int>("isCorrect");
                        if (isCorrect == 1)
                        {
                            correct = $"isCorrect={answerID};";
                        }
                    }
                    theAnswers.Append(correct);
                }
                else
                {
                    gameLog.Log("[ERROR] - Could not get answers from the database");
                }
            }
            return theAnswers.ToString();
        }


        // method for end of game display info
        public string GameOver(int userID)
        {
            StringBuilder scores = new StringBuilder();
            scores.Append($"userID={userID};");

            // Check if users currentScore replaces topScore
            DataTable getScore = new DataTable();
            getScore = dbHandler.Select($"SELECT * FROM `Users` WHERE userID='{userID}';");
            int? currentScore = 0;
            int? topScore = 0;

            // Check the data table
            if (getScore != null)
            {
                // Check the correct userID
                foreach (DataRow row in getScore.Rows)
                {
                    int buf = row.Field<int>("userID");
                    if (userID == buf)
                    {
                        // Get the users current game score
                        currentScore = row.Field<int?>("currentScore");
                        topScore = row.Field<int?>("topScore");
                        break;
                    }
                }
            }

            // Compare scores
            if(currentScore > topScore || topScore == null)
            {
                // Update topScore
                dbHandler.Execute($"UPDATE `Users` SET topScore={currentScore} WHERE userID={userID};");

                // Update data table
                getScore = dbHandler.Select($"SELECT * FROM `Users` WHERE userID='{userID}';");

                // Check the data table
                if (getScore != null)
                {
                    // Check the correct userID
                    foreach (DataRow row in getScore.Rows)
                    {
                        int buf = row.Field<int>("userID");
                        if (userID == buf)
                        {
                            // Get the users current game score
                            currentScore = row.Field<int?>("currentScore");
                            topScore = row.Field<int?>("topScore");
                            break;
                        }
                    }
                }
            }

            // Add this game's currentScore
            scores.Append($"currentScore={currentScore};");

            // Reset users currentScore for next game
            dbHandler.Execute($"UPDATE `Users` SET currentScore=0 WHERE userID={userID};");

            // Get the top 5 scores for all players
            DataTable getLeaderboard = new DataTable();
            getLeaderboard = dbHandler.Select($"SELECT * FROM `Users` ORDER BY `topScore` DESC LIMIT 5;");
            scores.Append("leaderboard;");
            // Check the data table
            if (getLeaderboard != null)
            {
                // Add each user and score
                foreach (DataRow row in getLeaderboard.Rows)
                {
                    string userName = row.Field<string>("userName");
                    topScore = row.Field<int?>("topScore");                    
                    scores.Append($"{userName}={topScore};");
                }
            }

            return scores.ToString();
        }


        // method to get the questions and their correct answers
        public StringBuilder ShowAnswers()
        {
            // Select from db to get question and answer texts
            DatabaseHandler dbHandler = new DatabaseHandler();
            DataTable dbResponse = null;
            StringBuilder QAText = new StringBuilder();
            QAText.Append("ANSWERS;");

            // Login to the local db
            bool loggedIn = dbHandler.Connect();

            if (loggedIn)
            {
                // Select from db to get users
                dbResponse = dbHandler.Select($"SELECT Questions.questionText, Answers.answerText FROM QuestionAnswers " +
                    $"INNER JOIN Questions ON Questions.questionID = QuestionAnswers.questionID " +
                    $"INNER JOIN Answers ON Answers.answerID = QuestionAnswers.answerID WHERE isCorrect = 1;");

                if (dbResponse != null)
                {
                    // iterate through the rows
                    foreach (DataRow row in dbResponse.Rows)
                    {
                        // Save the question and answer text
                        string question = row.Field<string>("questionText");
                        string answer = row.Field<string>("answerText");
                        QAText.Append($"{question}={answer};");
                    }
                }
                else
                {
                    gameLog.Log("[ERROR] - Could not get question and answer texts from the database");
                }
            }

            return QAText;
        }
    }
}
