CREATE TABLE [dbo].[UserRolePermissions] (
    [UserRoleId]       INT            NOT NULL,
    [PermissionId]     INT            NOT NULL,
    [IsGranted]        BIT            NOT NULL,
    [IsActive]         BIT            NOT NULL,
    [CreatedAt]        DATETIME2 (7)  DEFAULT (getutcdate()) NOT NULL,
    [CreatedByUserId]  NVARCHAR (MAX) NULL,
    [ModifiedAt]       DATETIME2 (7)  NULL,
    [ModifiedByUserId] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_UserRolePermissions] PRIMARY KEY CLUSTERED ([UserRoleId] ASC, [PermissionId] ASC),
    CONSTRAINT [FK_UserRolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRolePermissions_UserRoles_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [dbo].[UserRoles] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserRolePermissions_PermissionId]
    ON [dbo].[UserRolePermissions]([PermissionId] ASC);

