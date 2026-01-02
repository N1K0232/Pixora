CREATE TABLE [dbo].[ChatConversations]
(
	[Id]            UNIQUEIDENTIFIER        NOT NULL,
    [CreatedAt]     DATETIMEOFFSET (7)      NOT NULL
);

GO
ALTER TABLE [dbo].[ChatConversations]
ADD CONSTRAINT [PK_ChatConversations] PRIMARY KEY ([Id] ASC);

GO
ALTER TABLE [dbo].[ChatConversations]
ADD CONSTRAINT [DF_ChatConversations_Id] DEFAULT (NEWSEQUENTIALID()) FOR [Id];

GO
ALTER TABLE [dbo].[ChatConversations]
ADD CONSTRAINT [DF_ChatConversations_CreatedAt] DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedAt];