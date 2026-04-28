DROP TABLE IF EXISTS Users;

CREATE TABLE Users (
	UserID INT PRIMARY KEY,
	Username VARCHAR(50) NOT NULL,
	Password VARCHAR(255) NOT NULL,
	Role VARCHAR(20) DEFAULT 'user',
);

INSERT INTO Users (UserID, Username, Password, Role) VALUES (2, 'chris', 'admin124', 'user');

SELECT * FROM Users;

