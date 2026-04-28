
CREATE TABLE Users
(User_ID INT IDENTITY(1,1) PRIMARY KEY,
Username Varchar(255),
Password Varchar(255),
Role Varchar(50));

ALTER TABLE Users
ALTER COLUMN Username VARCHAR(255) NOT NULL;

ALTER TABLE Users
ALTER COLUMN Password VARCHAR(255) NOT NULL;

ALTER TABLE Users
ALTER COLUMN Role VARCHAR(50) NOT NULL;

ALTER TABLE Users
ADD CONSTRAINT UQ_Password
UNIQUE (Password);


INSERT INTO Users
VALUES('admin1', '1234', 'Admin'),
('donor1', '5678', 'Donor'),
('volunteer1', '4321', 'Volunteer');

SELECT * FROM Users;
