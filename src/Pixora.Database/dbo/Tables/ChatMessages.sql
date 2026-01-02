CREATE TABLE [dbo].[ChatMessages]
(
	[Id]                UNIQUEIDENTIFIER        NOT NULL,
    [ConversationId]    UNIQUEIDENTIFIER        NOT NULL,
    [SenderId]          UNIQUEIDENTIFIER        NOT NULL,
    [Content]           NVARCHAR (4000)         NOT NULL,
    [SentAt]            DATETIMEOFFSET (7)      NOT NULL
);

GO
ALTER TABLE [dbo].[ChatMessages]
ADD CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id] ASC);

GO
ALTER TABLE [dbo].[ChatMessages]
ADD CONSTRAINT [FK_ChatMessages_Conversations] FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[ChatConversations]([Id])
ON DELETE CASCADE;

GO
ALTER TABLE [dbo].[ChatMessages]
ADD CONSTRAINT [FK_ChatMessages_Sender] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[AspNetUsers]([Id])
ON DELETE CASCADE;

GO
ALTER TABLE [dbo].[ChatMessages]
ADD CONSTRAINT [DF_ChatMessages_Id] DEFAULT (NEWSEQUENTIALID()) FOR [Id];

GO
ALTER TABLE [dbo].[ChatMessages]
ADD CONSTRAINT [DF_ChatMessages_SentAt] DEFAULT (SYSDATETIMEOFFSET()) FOR [SentAt];

GO
CREATE NONCLUSTERED INDEX [IX_ChatMessages_ConversationId]
ON [dbo].[ChatMessages]([ConversationId]);

GO
CREATE NONCLUSTERED INDEX [IX_ChatMessages_SentAt]
ON [dbo].[ChatMessages]([SentAt]);