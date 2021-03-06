-- Creation of a procedure that creates ships and binds them to existing cells

CREATE PROCEDURE [dbo].[ShipToCellBinding]
	@BattlefieldID INT
AS
BEGIN
	CREATE TABLE #TempTableWithShips (X INT,Y INT, ShipID INT)
	
	DECLARE @ShipLastID INT;
	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-4, 4, @ShipLastID),
														(-4, 3, @ShipLastID),
														(-4, 2, @ShipLastID)

	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-2, 4, @ShipLastID),
														(-2, 3, @ShipLastID),
														(-2, 2, @ShipLastID)
	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (0, 4, @ShipLastID),
														(0, 3, @ShipLastID),
														(0, 2, @ShipLastID)
	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (2, 4, @ShipLastID),
														(2, 3, @ShipLastID),
														(2, 2, @ShipLastID)
	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (4, 4, @ShipLastID),
														(4, 3, @ShipLastID),
														(4, 2, @ShipLastID)
		EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-3, -2, @ShipLastID),
														(-3, -3, @ShipLastID),
														(-3, -4, @ShipLastID)
	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-1, -2, @ShipLastID),
														(-1, -3, @ShipLastID),
														(-1, -4, @ShipLastID)
	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (1, -2, @ShipLastID),
														(1, -3, @ShipLastID),
														(1, -4, @ShipLastID)
	EXECUTE ShipCreation 2;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (3, -2, @ShipLastID),
														(3, -3, @ShipLastID), 
														(3, -4, @ShipLastID)

	EXECUTE ShipCreation 1;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-3, 0, @ShipLastID),
														(-3, 1, @ShipLastID)
	EXECUTE ShipCreation 1;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-1, 0, @ShipLastID),
														(-1, 1, @ShipLastID)
	EXECUTE ShipCreation 1;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (1, 0, @ShipLastID),
														(1, 1, @ShipLastID)
	EXECUTE ShipCreation 1;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (3, 0, @ShipLastID),
														(3, 1, @ShipLastID)

	EXECUTE ShipCreation 0;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-4, -1, @ShipLastID)
	EXECUTE ShipCreation 0;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (-2, -1, @ShipLastID)
	EXECUTE ShipCreation 0;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (0, -1, @ShipLastID)
	EXECUTE ShipCreation 0;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (2, -1, @ShipLastID)
	EXECUTE ShipCreation 0;
	SET @ShipLastID = (SELECT MAX(Ships.Id) FROM Ships);
	INSERT INTO #TempTableWithShips (X,Y,ShipID) VALUES (4, -1, @ShipLastID)


	CREATE TABLE #ResultTempTable (Id INT Identity(1,1), PointID INT, ShipID INT, BattleFieldID INT)

	INSERT INTO #ResultTempTable
	SELECT 
		(SELECT Points.Id FROM Points WHERE Points.X = #TempTableWithShips.X AND Points.Y = #TempTableWithShips.Y),
		ShipID, 
		BattleFieldID = @BattlefieldID FROM #TempTableWithShips 

	MERGE Cells
	USING #ResultTempTable
	ON #ResultTempTable.PointID = Cells.PointID AND #ResultTempTable.BattleFieldID = Cells.BattleFieldID
		WHEN MATCHED 
			THEN UPDATE 
			SET Cells.ShipID = #ResultTempTable.ShipID
	;

	DROP TABLE #ResultTempTable, #TempTableWithShips;
END