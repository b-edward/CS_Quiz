CREATE DATABASE IF NOT EXISTS quiz_db;
USE quiz_db;

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
	(1, 'Where is Bavaria?'),    
	(2, 'Which is the top predator?'),    
	(3, 'What beverage did Homer Simpson create?'),
	(4, 'What city has the most michelin stars?'),
	(5, 'Which country is the smallest?'),
	(6, 'Which empire lasted the shortest time?'),
	(7, 'Which food originated Europe?'),
	(8, 'What is the Ottoman infantry unit?'),
	(9, 'Where is the worlds largest pyramid?'),
	(10, 'Which is physically closest to Canada?');

INSERT INTO `Answers` (`answerText`) VALUES 
	('Austria'),
	('Switzerland'),
	('USA'),
	('Germany'),
	('Clown Fish'),
	('Orca'),
	('Dolphin'),
	('Great White Shark'),
	('Duff Sour'),
	('Cup of Marge'),
	('Flaming Homer'),
	('Radioactive Sangria'),
	('Tokyo'),
	('Paris'),
	('San Sebastian'),
	('Rome'),
	('Jamaica'),
	('Cuba'),
	('Malta'),
	('Liechtenstein'),
	('Ottoman Empire'),
	('Roman Empire'),
	('Byzantine Empire'),
	('Holy Roman Empire'),
	('Potatoes'),
	('Tomatoes'),
	('Apples'),
	('Strawberries'),
	('Rajput'),
	('Janissary'),
	('Rodelero'),
	('Akmal'),
	('Egypt'),
	('Mexico'),
	('Sudan'),
	('Italy'),
	('France'),
	('Ireland'),
	('Portugal'),
	('Iceland');
        
INSERT INTO `QuestionAnswers` (`questionID`, `answerID`, `isCorrect`) VALUES 
	(1, 1, 0),
    (1, 2, 0),
    (1, 3, 0),
    (1, 4, 1),
    (2, 5, 0),
    (2, 6, 1),
    (2, 7, 0),
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
	('IMAPWNU', 147),
	('Hermione', 99),
	('LeisureSuitLarry', 69),
    ('FPS_Doug', 45),
	('AshKetchum', 12);

-- ServerA4, A4password69420

