CREATE TABLE [dbo].[AuditLogs] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [EntityName]        NVARCHAR (100) NOT NULL,
    [EntityKey]         NVARCHAR (100) NOT NULL,
    [Action]            NVARCHAR (50)  NOT NULL,
    [BeforeJson]        NVARCHAR (MAX) NULL,
    [AfterJson]         NVARCHAR (MAX) NULL,
    [PerformedAt]       DATETIME2 (7)  DEFAULT (getutcdate()) NOT NULL,
    [PerformedByUserId] NVARCHAR (450) NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AuditLogs_AspNetUsers_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_AuditLogs_PerformedByUserId]
    ON [dbo].[AuditLogs]([PerformedByUserId] ASC);

