CREATE TABLE [dbo].[PRAIMDBTable] (
    [ActionItemId] INT            NOT NULL,
    [Priority]     INT            NULL,
    [ProjectID]    INT            NULL,
    [Version]      NVARCHAR(MAX)            NULL,
    [DateTime]     DATETIME       NULL,
    [Comments]     NVARCHAR (MAX) NULL,
    [Snapshot]     IMAGE          NULL,
    PRIMARY KEY CLUSTERED ([ActionItemId] ASC)
);

