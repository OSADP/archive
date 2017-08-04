USE [infloseattletst-db-sql]
GO

DELETE FROM [dbo].[Configuration_Roadway]
      WHERE [RoadwayId]=1100
GO

DELETE FROM [dbo].[Configuration_RoadwayESS]
      WHERE [RoadwayId]=1100
GO

DELETE FROM [dbo].[Configuration_RoadwayLinks]
      WHERE [RoadwayId]=1100
GO

DELETE FROM [dbo].[Configuration_RoadwayMileMarkers]
      WHERE [RoadwayId]=1100
GO

DELETE FROM [dbo].[Configuration_RoadwaySubLinks]
      WHERE [RoadwayId]=1100
GO

