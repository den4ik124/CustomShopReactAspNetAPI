--CREATE DATABASE SeaBattleTest_FINAL;

USE SeaBattleTest_FINAL
GO

CREATE TABLE ShipType(
Id INT PRIMARY KEY NOT NULL IDENTITY(1,1),
[Type] NVARCHAR(20) NOT NULL
);

CREATE TABLE Ship (
Id INT PRIMARY KEY NOT NULL IDENTITY(1,1),
TypeId INT NOT NULL REFERENCES ShipType (Id) ON DELETE CASCADE ON UPDATE CASCADE,
Velocity INT NOT NULL,
[Range] INT NOT NULL,
Size INT NOT NULL
);

CREATE TABLE Point(
Id INT PRIMARY KEY NOT NULL IDENTITY(1,1),
X INT NOT NULL,
Y INT NOT NULL
);

CREATE TABLE BattleField(
Id INT PRIMARY KEY NOT NULL IDENTITY(1,1),
SideLength INT NOT NULL
);


CREATE TABLE Cell(
Id INT PRIMARY KEY NOT NULL IDENTITY(1,1),
PointID INT  NOT NULL REFERENCES Point (Id) ON DELETE CASCADE ON UPDATE CASCADE, 
ShipID INT NULL REFERENCES Ship (Id)  ON DELETE SET NULL ON UPDATE CASCADE,
BattleFieldID INT NOT NULL REFERENCES BattleField(Id) ON DELETE CASCADE ON UPDATE CASCADE
);
