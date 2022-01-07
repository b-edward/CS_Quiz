# Web Development Study Quiz
This project is a trivia/quiz game to aid students in studying material for web development tests and exams. It was created as a solo-submission for Google Developer Student Club's Demo Day competition 2021.

## Features
* 20 second answer timer
* Score based on answer time remaining
* Saves users and scores to MySQL database
* Tracks the top scores and displays leaderboard
* Option to display correct answers 
* Clients can connect from any machine via TCP socket
* Server can support multiple clients simultaneously

### Implementation
This project was based on a combination of projects, using both new as well as pre-existing modular code from several assignments ranging from Web Development, Relational Databases, and Windows Development.
* Client created with .NET WPF
* Server created as a .NET console application
* Database created using MySQL
* Server and database deployed on an Azure Virtual Machine

### Try it out
To test the client application:
* Copy the Quiz Game and Quiz Server folders
* In the server folder, run SetupDB.sql in MySQL. 
* In then Quiz_Server\bin\Debug folder, change Quiz_Server.exe.config user, password, port, and ip to match your local MySQL server. 
* Run Quiz_Server.exe. 
* Then in Quiz Game folder, change Quiz_Client.exe.config ip and port to match server config file. 
* Finally, run Quiz_Client.exe to play the game.

#### Screenshots

![User Interface Example](https://github.com/b-edward/Study_Quiz/blob/main/Info/Start.png)<br/>
![User Interface Example](https://github.com/b-edward/Study_Quiz/blob/main/Info/Quiz.png)<br/>
![User Interface Example](https://github.com/b-edward/Study_Quiz/blob/main/Info/Scores.png)<br/>
![User Interface Example](https://github.com/b-edward/Study_Quiz/blob/main/Info/MultipleClients.png)<br/>
![User Interface Example](https://github.com/b-edward/Study_Quiz/blob/main/Info/ServerVM.png)<br/>
![User Interface Example](https://github.com/b-edward/Study_Quiz/blob/main/Info/AzureDeployment.png)<br/>
