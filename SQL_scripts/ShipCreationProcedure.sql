CREATE PROCEDURE [dbo].[ShipCreation]
	@ShipSize INT
AS
BEGIN
DECLARE @Counter INT;
SET @COUNTER = 1;
	INSERT INTO Ship (TypeId, [Range], Size, Velocity) 
	VALUES (FLOOR(RAND()*(3-1+1)+1), FLOOR(RAND()*(10-1+1)+1), @ShipSize, FLOOR(RAND()*(5-1+1)+1))
END;