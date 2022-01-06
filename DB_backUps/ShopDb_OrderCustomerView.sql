CREATE VIEW OrderUserView AS
	SELECT TOP (1000) dbo.Orders.Id AS [Order Id],
						dbo.Orders.DateAndTimeOfCreation AS Time,
						dbo.Orders.TotalCost AS Total,
						IdentityUsers.dbo.AspNetUsers.UserName AS Customer,
						IdentityUsers.dbo.AspNetUsers.Email
	FROM dbo.Orders 
	LEFT OUTER JOIN IdentityUsers.dbo.AspNetUsers ON dbo.Orders.OwnerId = IdentityUsers.dbo.AspNetUsers.Id