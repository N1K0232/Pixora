CREATE TABLE [dbo].[ChatParticipants]
(
	[ConversationId]        UNIQUEIDENTIFIER        NOT NULL,
    [UserId]                UNIQUEIDENTIFIER        NOT NULL,
    [JoinedAt]              DATETIMEOFFSET (7)      NOT NULL
);

GO
ALTER TABLE [dbo].[ChatParticipants]
ADD CONSTRAINT [PK_ChatParticipants] PRIMARY KEY ([ConversationId] ASC, [UserId] ASC);

GO
ALTER TABLE [dbo].[ChatParticipants]
ADD CONSTRAINT [FK_ChatParticipants_Conversations] FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[ChatConversations]([Id])
ON DELETE CASCADE;

GO
ALTER TABLE [dbo].[ChatParticipants]
ADD CONSTRAINT [FK_ChatParticipants_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
ON DELETE CASCADE;

GO
ALTER TABLE [dbo].[ChatParticipants]
ADD CONSTRAINT [DF_ChatParticipants_JoinedAt] DEFAULT (SYSDATETIMEOFFSET()) FOR [JoinedAt];

GO
CREATE NONCLUSTERED INDEX [IX_ChatParticipants_UserId]
ON [dbo].[ChatParticipants]([UserId] ASC);