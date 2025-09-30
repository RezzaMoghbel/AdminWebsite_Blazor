CREATE TABLE [dbo].[Websites] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Code]             NVARCHAR (50)  NOT NULL,
    [Name]             NVARCHAR (200) NOT NULL,
    [Url]              NVARCHAR (500) NULL,
    [IsActive]         BIT            NOT NULL,
    [CreatedAt]        DATETIME2 (7)  DEFAULT (getutcdate()) NOT NULL,
    [CreatedByUserId]  NVARCHAR (MAX) NULL,
    [ModifiedAt]       DATETIME2 (7)  NULL,
    [ModifiedByUserId] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Websites] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Websites_Code]
    ON [dbo].[Websites]([Code] ASC);

