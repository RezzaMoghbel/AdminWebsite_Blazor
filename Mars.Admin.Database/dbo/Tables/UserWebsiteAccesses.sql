CREATE TABLE [dbo].[UserWebsiteAccesses] (
    [UserId]           NVARCHAR (450) NOT NULL,
    [WebsiteId]        INT            NOT NULL,
    [IsGranted]        BIT            NOT NULL,
    [IsActive]         BIT            NOT NULL,
    [CreatedAt]        DATETIME2 (7)  DEFAULT (getutcdate()) NOT NULL,
    [CreatedByUserId]  NVARCHAR (MAX) NULL,
    [ModifiedAt]       DATETIME2 (7)  NULL,
    [ModifiedByUserId] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_UserWebsiteAccesses] PRIMARY KEY CLUSTERED ([UserId] ASC, [WebsiteId] ASC),
    CONSTRAINT [FK_UserWebsiteAccesses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserWebsiteAccesses_Websites_WebsiteId] FOREIGN KEY ([WebsiteId]) REFERENCES [dbo].[Websites] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserWebsiteAccesses_WebsiteId]
    ON [dbo].[UserWebsiteAccesses]([WebsiteId] ASC);

