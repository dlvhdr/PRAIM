CREATE TABLE [dbo].[Table] (
    [ActionItemId] INT            NOT NULL,
    [Priority]     INT            NULL,
    [ProjectID]    INT            NULL,
    [Version]      INT            NULL,
    [DateTime]     DATETIME       NULL,
    [Comments]     NVARCHAR (MAX) NULL,
    [Snapshot]     IMAGE          NULL,
    PRIMARY KEY CLUSTERED ([ActionItemId] ASC)
);

