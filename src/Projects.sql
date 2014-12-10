CREATE TABLE [dbo].[Projects]
(
	[ProjectName] NVARCHAR(MAX) NOT NULL, 
    [Description] NVARCHAR(MAX) NULL,
	PRIMARY KEY CLUSTERED ([ProjectName] ASC)
);
