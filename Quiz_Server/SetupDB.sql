CREATE DATABASE IF NOT EXISTS quiz;
USE quiz;

-- Reset tables

DROP TABLE IF EXISTS `Users`;
DROP TABLE IF EXISTS `CorrectAnswerTimes`;
DROP TABLE IF EXISTS `Answers`;
DROP TABLE IF EXISTS `Questions`;
DROP TABLE IF EXISTS `QuestionAnswers`;

-- Create fresh tables

CREATE TABLE IF NOT EXISTS `Questions` (
  `questionID` INT NOT NULL AUTO_INCREMENT,
  `questionText` VARCHAR(255) NOT NULL,
  PRIMARY KEY (`questionID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `Users` (
  `userID` INT NOT NULL AUTO_INCREMENT,
  `userName` VARCHAR(45) NOT NULL UNIQUE,
  `lastQuestionSent` INT NOT NULL DEFAULT '1',
  `currentScore` INT,
  `topScore` INT,
  PRIMARY KEY (`userID`), 
  FOREIGN KEY (`lastQuestionSent`) REFERENCES `Questions` (`questionID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `CorrectAnswerTimes` (
  `userID` INT NOT NULL,
  `questionID` INT NOT NULL,
  `answerTime` INT NOT NULL,
  PRIMARY KEY (`userID`, `questionID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `Answers` (
  `answerID` INT NOT NULL AUTO_INCREMENT,
  `answerText` VARCHAR(255) NOT NULL,
  PRIMARY KEY (`answerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
  
  CREATE TABLE IF NOT EXISTS `QuestionAnswers` (
  `questionID` INT NOT NULL,  
  `answerID` INT NOT NULL,
  `isCorrect` INT NOT NULL DEFAULT '0',
  PRIMARY KEY (`questionID`, `answerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- INSERTs of Question and Answer data  
    
INSERT INTO `Questions` (`questionID`, `questionText`) VALUES
	(1, 'What is HTML?'),    
	(2, 'What best describes a static web page?'),    
	(3, 'What best describes a dynamic web page?'),
	(4, 'What is HTTP?'),
	(5, 'What best describes JavaScript?'),
	(6, 'What is a regular expression?'),
	(7, 'What does PHP stand for?'),
	(8, 'Cookies are an example of _____ .'),
	(9, 'MVC is related to which design pattern?'),
	(10, 'What is JSON?');

INSERT INTO `Answers` (`answerText`) VALUES 
	('Hot Tomato Mustard Lettuce'),
	('Hypertext Markup Language'),
	('Hyperion Markup Language'),
	('Hyper Modern Markup Language'),
	('A page that is electrically charged'),
	('A single page that cannot move'),
	('A page with content that never changes'),
	('When a browser cannot render the page'),
	('It is really cool'),
	('Multiple pages that cannot move'),
	('A page with content that can change'),
	('A multi-threaded page'),
	('Hypertext Transfer Protocol'),
	('Hyperion Transit Protocol'),
	('Hydraulic Text Pages'),
	('Homepage Transit Protocol'),
	('It is Java but with Scripts'),
	('It is compiled as Java'),
	('It is not Java, nothing similar about it'),
	('Interpreted language with Java-like syntax'),
	('Common sayings or quotes'),
	('A sequence that defines a search pattern'),
	('Expressions in common English'),
	('A phrase that must be adhered to'),
	('Productive Hypertext Processing'),
	('Periodic Hypertext Program'),
	('PHP Hypertext Processor'),
	('PHP Hypertext Preprocessor'),
	('State Resources'),
	('State Management'),
	('Delicious snacks'),
	('Baked-in web services'),
	('Singleton'),
	('Observer'),
	('Factory Method'),
	('Adapter'),
	('JavaScript Object Notation'),
	('Java Simple Object Notation'),
	('JavaScript Oriented Notation'),
	('Java Super Object-Oriented Nodes');
        
INSERT INTO `QuestionAnswers` (`questionID`, `answerID`, `isCorrect`) VALUES 
	(1, 1, 0),
    (1, 2, 1),
    (1, 3, 0),
    (1, 4, 0),
    (2, 5, 0),
    (2, 6, 0),
    (2, 7, 1),
    (2, 8, 0),
	(3, 9, 0),
    (3, 10, 0),
    (3, 11, 1),
    (3, 12, 0),
	(4, 13, 1),
    (4, 14, 0),
    (4, 15, 0),
    (4, 16, 0),
	(5, 17, 0),
    (5, 18, 0),
    (5, 19, 0),
    (5, 20, 1),
	(6, 21, 0),
    (6, 22, 1),
    (6, 23, 0),
    (6, 24, 0),
	(7, 25, 0),
    (7, 26, 0),
    (7, 27, 0),
    (7, 28, 1),
	(8, 29, 0),
    (8, 30, 1),
    (8, 31, 0),
    (8, 32, 0),
	(9, 33, 0),
    (9, 34, 1),
    (9, 35, 0),
    (9, 36, 0),
	(10, 37, 1),
    (10, 38, 0),
    (10, 39, 0),
    (10, 40, 0);

INSERT INTO `Users` (`userName`, `topScore`) VALUES
	('TehPwnerer', 201),    
    ('EVA-02', 165),
	('IMAPWNU', 147),
	('Hermione', 99),
	('Worf', 80),
	('AshKetchum', 12);

-- ServerA4, A4password69420

