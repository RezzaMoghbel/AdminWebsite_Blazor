CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (450)     NOT NULL,
    [UserName]             NVARCHAR (256)     NULL,
    [NormalizedUserName]   NVARCHAR (256)     NULL,
    [Email]                NVARCHAR (256)     NULL,
    [NormalizedEmail]      NVARCHAR (256)     NULL,
    [EmailConfirmed]       BIT                NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)     NULL,
    [SecurityStamp]        NVARCHAR (MAX)     NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
    [PhoneNumber]          NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed] BIT                NOT NULL,
    [TwoFactorEnabled]     BIT                NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]       BIT                NOT NULL,
    [AccessFailedCount]    INT                NOT NULL,
    [UserRoleId]           INT                NULL,
    [DeletedAt]            DATETIME2 (7)      NULL,
    [DeletedBy]            NVARCHAR (MAX)     NULL,
    [IsActive]             BIT                DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [IsDeleted]            BIT                DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [AttentionCreatedAt]   DATETIME2 (7)      NULL,
    [AttentionIgnoredAt]   DATETIME2 (7)      NULL,
    [AttentionIgnoredBy]   NVARCHAR (MAX)     NULL,
    [IsNewUser]            BIT                DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [LastLoginAt]          DATETIME2 (7)      NULL,
    [NeedsAttention]       BIT                DEFAULT (CONVERT([bit],(0))) NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUsers_UserRoles_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [dbo].[UserRoles] ([Id]) ON DELETE SET NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_AspNetUsers_UserRoleId]
    ON [dbo].[AspNetUsers]([UserRoleId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);

