CREATE TABLE [dbo].[IPSafeListings] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [IPAddress]        NVARCHAR (45)  NOT NULL,
    [UserId]           NVARCHAR (450) NULL,
    [Label]            NVARCHAR (200) NULL,
    [ExpiryDate]       DATETIME2 (7)  NULL,
    [IsActive]         BIT            NOT NULL,
    [CreatedAt]        DATETIME2 (7)  DEFAULT (getutcdate()) NOT NULL,
    [CreatedByUserId]  NVARCHAR (MAX) NULL,
    [ModifiedAt]       DATETIME2 (7)  NULL,
    [ModifiedByUserId] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_IPSafeListings] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_IPSafeListings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_IPSafeListings_UserId]
    ON [dbo].[IPSafeListings]([UserId] ASC);

