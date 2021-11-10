INSERT INTO ShipTypes ([Type]) VALUES ('BattleShip'),('RepairShip'),('ComboShip')

DECLARE @BFNumber_min INT, @BFNumber_max INT, @BFSideLength_min INT, @BFSideLength_max INT;
SET @BFNumber_min = 4;
SET @BFNumber_max = 6;
SET @BFSideLength_min= 8;
SET @BFSideLength_max =15;


BEGIN-- BattleFields creation
DECLARE @BFNumber INT, @BFSideLength INT;
SET @BFNumber = FLOOR(RAND()*(@BFNumber_max-@BFNumber_min+1)+@BFNumber_min)

WHILE @BFNumber != 0
BEGIN
	
	SET @BFSideLength = FLOOR(RAND()*(@BFSideLength_max-@BFSideLength_min+1)+@BFSideLength_min);
	INSERT INTO BattleFields (SideLength) VALUES (@BFSideLength);
	BEGIN-- Points creation
	DECLARE @ROW_MIN INT, @COLUMN_MIN INT,@ROW_MAX INT, @COLUMN_MAX INT;
		SET @ROW_MIN = ROUND(-@BFSideLength / 2.0, 1, 0);
	    SET @COLUMN_MIN = ROUND(-@BFSideLength / 2.0, 1, 0);
		SET @ROW_MAX = ROUND(@BFSideLength / 2.0, 1, 0);
	    SET @COLUMN_MAX = ROUND(@BFSideLength / 2.0, 1, 0);

        WHILE @COLUMN_MIN <= @COLUMN_MAX
		BEGIN
			WHILE @ROW_MIN <= @ROW_MAX
				BEGIN
					IF NOT EXISTS (SELECT * FROM Points WHERE (Points.X = @ROW_MIN AND Points.Y = @COLUMN_MIN))
					BEGIN
						INSERT INTO Points (X,Y) VALUES (@ROW_MIN, @COLUMN_MIN) -- if such coordinates not exists - new point will be added
					END
					INSERT INTO Cells (PointID, ShipID,BattleFieldID) VALUES(
																			(SELECT Points.Id FROM Points WHERE Points.X = @ROW_MIN AND Points.Y = @COLUMN_MIN), --Point ID selection for specified coordinates
																			NULL, --ships were not created yet - that is why NULL
																			(SELECT MAX(BattleFields.Id) FROM BattleFields) --last created battlefield selection
																			)
					SET @ROW_MIN = @ROW_MIN + 1;
				END;
			SET @COLUMN_MIN = @COLUMN_MIN + 1;
			SET @ROW_MIN = ROUND(-@BFSideLength / 2.0, 1, 0);
		END;
		END-- Points creation

		-- Test ships binding to created cells
		DECLARE @BattlefieldID INT;
		SET @BattlefieldID = (SELECT MAX(BattleFields.Id) FROM BattleFields);
		EXECUTE ShipToCellBinding @BattlefieldID;

	SET @BFNumber = @BFNumber  -1
END
END-- BattleFields creation