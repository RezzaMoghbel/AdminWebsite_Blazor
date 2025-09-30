CREATE TABLE [dbo].[UserRoles] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (100) NOT NULL,
    [Description]      NVARCHAR (500) NULL,
    [IsSuperAdmin]     BIT            NOT NULL,
    [IsActive]         BIT            NOT NULL,
    [CreatedAt]        DATETIME2 (7)  DEFAULT (getutcdate()) NOT NULL,
    [CreatedByUserId]  NVARCHAR (MAX) NULL,
    [ModifiedAt]       DATETIME2 (7)  NULL,
    [ModifiedByUserId] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserRoles_Name]
    ON [dbo].[UserRoles]([Name] ASC);

