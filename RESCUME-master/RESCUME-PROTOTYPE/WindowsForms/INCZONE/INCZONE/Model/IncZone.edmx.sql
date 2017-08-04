
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 06/16/2014 12:41:59
-- Generated from EDMX file: C:\Users\triplettl\Documents\Visual Studio 2013\Projects\WindowsForms\INCZONE\INCZONE\Model\IncZone.edmx
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    ALTER TABLE [AudibleVisualAlarms] DROP CONSTRAINT [FK_AlarmConfigurationAudibleVisualAlarm];
GO
    ALTER TABLE [AudibleVisualAlarms] DROP CONSTRAINT [FK_AlarmLevelAudibleVisualAlarm];
GO
    ALTER TABLE [EventLogs] DROP CONSTRAINT [FK_EventTypeEventLog];
GO
    ALTER TABLE [EventLogs] DROP CONSTRAINT [FK_LogLevelEventLog];
GO
    ALTER TABLE [MapLinks] DROP CONSTRAINT [FK_MapLink_MapNode_endId];
GO
    ALTER TABLE [MapLinks] DROP CONSTRAINT [FK_MapLink_MapNode_startId];
GO
    ALTER TABLE [MapNodes] DROP CONSTRAINT [FK_MapNode_MapSet];
GO
    ALTER TABLE [VehicleAlarms] DROP CONSTRAINT [FK_AlarmConfigurationVehicleAlarm];
GO
    ALTER TABLE [VehicleAlarms] DROP CONSTRAINT [FK_AlarmLevelVehicleAlarm];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- NOTE: if the table does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    DROP TABLE [AlarmConfigurations];
GO
    DROP TABLE [AlarmLevels];
GO
    DROP TABLE [AudibleVisualAlarms];
GO
    DROP TABLE [CapWINConfigurations];
GO
    DROP TABLE [DGPSConfigurations];
GO
    DROP TABLE [DSRCConfigurations];
GO
    DROP TABLE [EventLogs];
GO
    DROP TABLE [EventTypes];
GO
    DROP TABLE [LogLevels];
GO
    DROP TABLE [MapLinks];
GO
    DROP TABLE [MapNodes];
GO
    DROP TABLE [MapSets];
GO
    DROP TABLE [VehicleAlarms];
GO
    DROP TABLE [BluetoothConfigs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'AlarmConfigurations'
CREATE TABLE [AlarmConfigurations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(255)  NOT NULL,
    [IsDefault] bit  NOT NULL
);
GO

-- Creating table 'AlarmLevels'
CREATE TABLE [AlarmLevels] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Level] int  NOT NULL
);
GO

-- Creating table 'AudibleVisualAlarms'
CREATE TABLE [AudibleVisualAlarms] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Duration] nvarchar(4000)  NOT NULL,
    [Frequency] nvarchar(4000)  NOT NULL,
    [AlarmLevel_Id] int  NOT NULL,
    [AlarmConfiguration_Id] int  NOT NULL,
    [Persistance] int  NOT NULL,
    [RadioActive] bit  NOT NULL
);
GO

-- Creating table 'CapWINConfigurations'
CREATE TABLE [CapWINConfigurations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(100)  NOT NULL,
    [Password] nvarchar(100)  NOT NULL,
    [HostURL] nvarchar(255)  NOT NULL,
    [ComPort] nvarchar(10)  NOT NULL,
    [BaudRate] nvarchar(10)  NOT NULL,
    [DistanceToIncident] int  NOT NULL,
    [LaneData] int  NOT NULL
);
GO

-- Creating table 'DGPSConfigurations'
CREATE TABLE [DGPSConfigurations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(256)  NOT NULL,
    [Username] nvarchar(100)  NOT NULL,
    [Password] nvarchar(100)  NOT NULL,
    [HostIP] nvarchar(50)  NOT NULL,
    [HostPort] nvarchar(10)  NOT NULL,
    [RefreshRate] int  NOT NULL,
    [IsDefault] bit  NOT NULL,
    [LocationRefreshRate] int  NOT NULL
);
GO

-- Creating table 'DSRCConfigurations'
CREATE TABLE [DSRCConfigurations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ACM] int  NOT NULL,
    [BSM] int  NOT NULL,
    [EVA] int  NOT NULL,
    [TIM] int  NOT NULL
);
GO

-- Creating table 'EventLogs'
CREATE TABLE [EventLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EventMessage] nvarchar(256)  NOT NULL,
    [EventDate] datetime  NOT NULL,
    [EventType_Id] int  NOT NULL,
    [LogLevel_Id] int  NOT NULL,
    [EventInfo] nvarchar(4000)  NOT NULL
);
GO

-- Creating table 'EventTypes'
CREATE TABLE [EventTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'LogLevels'
CREATE TABLE [LogLevels] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'MapLinks'
CREATE TABLE [MapLinks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [startId] int  NOT NULL,
    [endId] int  NOT NULL
);
GO

-- Creating table 'MapNodes'
CREATE TABLE [MapNodes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [lat] float  NOT NULL,
    [long] float  NOT NULL,
    [elevation] float  NOT NULL,
    [laneWidth] int  NOT NULL,
    [directionality] int  NOT NULL,
    [xOffset] int  NOT NULL,
    [yOffset] int  NOT NULL,
    [zOffset] int  NOT NULL,
    [positionalAccuracyP1] int  NOT NULL,
    [positionalAccuracyP2] int  NOT NULL,
    [positionalAccuracyP3] int  NOT NULL,
    [laneOrder] int  NOT NULL,
    [postedSpeed] int  NOT NULL,
    [mapSetId] int  NOT NULL,
    [distance] float  NOT NULL,
    [GuId] uniqueidentifier  NOT NULL,
    [mapSetGuId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'MapSets'
CREATE TABLE [MapSets] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(50)  NOT NULL,
    [description] nvarchar(4000)  NULL,
    [GuId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'VehicleAlarms'
CREATE TABLE [VehicleAlarms] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Persistance] int  NOT NULL,
    [Active] bit  NOT NULL,
    [AlarmConfiguration_Id] int  NOT NULL,
    [AlarmLevel_Id] int  NOT NULL
);
GO

-- Creating table 'BluetoothConfigs'
CREATE TABLE [BluetoothConfigs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Arada] varbinary(8000)  NOT NULL,
    [Vital] nvarchar(4000)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'AlarmConfigurations'
ALTER TABLE [AlarmConfigurations]
ADD CONSTRAINT [PK_AlarmConfigurations]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'AlarmLevels'
ALTER TABLE [AlarmLevels]
ADD CONSTRAINT [PK_AlarmLevels]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'AudibleVisualAlarms'
ALTER TABLE [AudibleVisualAlarms]
ADD CONSTRAINT [PK_AudibleVisualAlarms]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'CapWINConfigurations'
ALTER TABLE [CapWINConfigurations]
ADD CONSTRAINT [PK_CapWINConfigurations]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'DGPSConfigurations'
ALTER TABLE [DGPSConfigurations]
ADD CONSTRAINT [PK_DGPSConfigurations]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'DSRCConfigurations'
ALTER TABLE [DSRCConfigurations]
ADD CONSTRAINT [PK_DSRCConfigurations]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'EventLogs'
ALTER TABLE [EventLogs]
ADD CONSTRAINT [PK_EventLogs]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'EventTypes'
ALTER TABLE [EventTypes]
ADD CONSTRAINT [PK_EventTypes]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'LogLevels'
ALTER TABLE [LogLevels]
ADD CONSTRAINT [PK_LogLevels]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'MapLinks'
ALTER TABLE [MapLinks]
ADD CONSTRAINT [PK_MapLinks]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'MapNodes'
ALTER TABLE [MapNodes]
ADD CONSTRAINT [PK_MapNodes]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'MapSets'
ALTER TABLE [MapSets]
ADD CONSTRAINT [PK_MapSets]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'VehicleAlarms'
ALTER TABLE [VehicleAlarms]
ADD CONSTRAINT [PK_VehicleAlarms]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'BluetoothConfigs'
ALTER TABLE [BluetoothConfigs]
ADD CONSTRAINT [PK_BluetoothConfigs]
    PRIMARY KEY ([Id] );
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [AlarmConfiguration_Id] in table 'AudibleVisualAlarms'
ALTER TABLE [AudibleVisualAlarms]
ADD CONSTRAINT [FK_AlarmConfigurationAudibleVisualAlarm]
    FOREIGN KEY ([AlarmConfiguration_Id])
    REFERENCES [AlarmConfigurations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_AlarmConfigurationAudibleVisualAlarm'
CREATE INDEX [IX_FK_AlarmConfigurationAudibleVisualAlarm]
ON [AudibleVisualAlarms]
    ([AlarmConfiguration_Id]);
GO

-- Creating foreign key on [AlarmLevel_Id] in table 'AudibleVisualAlarms'
ALTER TABLE [AudibleVisualAlarms]
ADD CONSTRAINT [FK_AlarmLevelAudibleVisualAlarm]
    FOREIGN KEY ([AlarmLevel_Id])
    REFERENCES [AlarmLevels]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_AlarmLevelAudibleVisualAlarm'
CREATE INDEX [IX_FK_AlarmLevelAudibleVisualAlarm]
ON [AudibleVisualAlarms]
    ([AlarmLevel_Id]);
GO

-- Creating foreign key on [EventType_Id] in table 'EventLogs'
ALTER TABLE [EventLogs]
ADD CONSTRAINT [FK_EventTypeEventLog]
    FOREIGN KEY ([EventType_Id])
    REFERENCES [EventTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_EventTypeEventLog'
CREATE INDEX [IX_FK_EventTypeEventLog]
ON [EventLogs]
    ([EventType_Id]);
GO

-- Creating foreign key on [LogLevel_Id] in table 'EventLogs'
ALTER TABLE [EventLogs]
ADD CONSTRAINT [FK_LogLevelEventLog]
    FOREIGN KEY ([LogLevel_Id])
    REFERENCES [LogLevels]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_LogLevelEventLog'
CREATE INDEX [IX_FK_LogLevelEventLog]
ON [EventLogs]
    ([LogLevel_Id]);
GO

-- Creating foreign key on [endId] in table 'MapLinks'
ALTER TABLE [MapLinks]
ADD CONSTRAINT [FK_MapLink_MapNode_endId]
    FOREIGN KEY ([endId])
    REFERENCES [MapNodes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_MapLink_MapNode_endId'
CREATE INDEX [IX_FK_MapLink_MapNode_endId]
ON [MapLinks]
    ([endId]);
GO

-- Creating foreign key on [startId] in table 'MapLinks'
ALTER TABLE [MapLinks]
ADD CONSTRAINT [FK_MapLink_MapNode_startId]
    FOREIGN KEY ([startId])
    REFERENCES [MapNodes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_MapLink_MapNode_startId'
CREATE INDEX [IX_FK_MapLink_MapNode_startId]
ON [MapLinks]
    ([startId]);
GO

-- Creating foreign key on [mapSetId] in table 'MapNodes'
ALTER TABLE [MapNodes]
ADD CONSTRAINT [FK_MapNode_MapSet]
    FOREIGN KEY ([mapSetId])
    REFERENCES [MapSets]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_MapNode_MapSet'
CREATE INDEX [IX_FK_MapNode_MapSet]
ON [MapNodes]
    ([mapSetId]);
GO

-- Creating foreign key on [AlarmConfiguration_Id] in table 'VehicleAlarms'
ALTER TABLE [VehicleAlarms]
ADD CONSTRAINT [FK_AlarmConfigurationVehicleAlarm]
    FOREIGN KEY ([AlarmConfiguration_Id])
    REFERENCES [AlarmConfigurations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_AlarmConfigurationVehicleAlarm'
CREATE INDEX [IX_FK_AlarmConfigurationVehicleAlarm]
ON [VehicleAlarms]
    ([AlarmConfiguration_Id]);
GO

-- Creating foreign key on [AlarmLevel_Id] in table 'VehicleAlarms'
ALTER TABLE [VehicleAlarms]
ADD CONSTRAINT [FK_AlarmLevelVehicleAlarm]
    FOREIGN KEY ([AlarmLevel_Id])
    REFERENCES [AlarmLevels]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
-- Creating non-clustered index for FOREIGN KEY 'FK_AlarmLevelVehicleAlarm'
CREATE INDEX [IX_FK_AlarmLevelVehicleAlarm]
ON [VehicleAlarms]
    ([AlarmLevel_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------