USE [inflotst-db-sql]
GO

DELETE FROM [dbo].[Configuration_Roadway]
      WHERE [RoadwayId]>=100
GO

DELETE FROM [dbo].[Configuration_RoadwayLinks]
      WHERE [RoadwayId]>=100
GO

DELETE FROM [dbo].[Configuration_RoadwayMileMarkers]
      WHERE [RoadwayId]>=100
GO

DELETE FROM [dbo].[Configuration_RoadwaySubLinks]
      WHERE [RoadwayId]>=100
GO

