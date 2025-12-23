CREATE TABLE [dbo].[Posts]
(
	[Id]                UNIQUEIDENTIFIER    NOT NULL,
    [UserId]            UNIQUEIDENTIFIER    NOT NULL,
    [Title]             NVARCHAR (255)      NOT NULL,
    [Content]           NVARCHAR (MAX)      NOT NULL,
    [Visibility]        NVARCHAR (50)       NOT NULL,
    [CreatedAt]         DATETIME2           NOT NULL,
    [LastModifiedAt]    DATETIME2           NULL,
    [IsPublished]       BIT                 NOT NULL,
    [IsEdited]          BIT                 NOT NULL
);

GO
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [PK_Posts] PRIMARY KEY CLUSTERED ([Id] ASC);

GO
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [FK_Posts_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id])
ON DELETE CASCADE;

GO
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [DF_Posts_Id] DEFAULT (NEWSEQUENTIALID()) FOR [Id];

GO
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [DF_Posts_CreatedAt] DEFAULT (SYSUTCDATETIME()) FOR [CreatedAt];

GO
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [DF_Posts_IsPublished] DEFAULT ((1)) FOR [IsPublished];

GO
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [DF_Posts_IsEdited] DEFAULT ((0)) FOR [IsEdited];