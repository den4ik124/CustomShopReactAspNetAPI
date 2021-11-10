-- Creation of procedure to insert new test ship with random characteristics

CREATE PROCEDURE [dbo].[ShipCreation]
	@ShipSize INT
AS
BEGIN
DECLARE @Counter INT;
SET @COUNTER = 1;
	INSERT INTO Ships (TypeId, [Range], Size, Velocity) 
	VALUES (FLOOR(RAND()*3+1), FLOOR(RAND()*10+1), @ShipSize, FLOOR(RAND()*5+1))
END;