CREATE TABLE [dbo].[Images]
(
	[Id]                UNIQUEIDENTIFIER        NOT NULL,
    [UserId]            UNIQUEIDENTIFIER        NOT NULL,
    [FileName]          NVARCHAR (255)          NOT NULL,
    [Path]              NVARCHAR (512)          NOT NULL,
    [Length]            BIGINT                  NOT NULL,
    [ContentType]       NVARCHAR (50)           NOT NULL,
    [Description]       NVARCHAR (4000)         NULL,
    [Tags]              NVARCHAR (MAX)          NULL,
    [CreatedAt]         DATETIME2               NOT NULL,
    [LastModifiedAt]    DATETIME2               NULL,
    [IsPublished]       BIT                     NOT NULL,
    [PublishedAt]       DATETIMEOFFSET (7)      NOT NULL,
);

GO
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [PK_Images] PRIMARY KEY ([Id] ASC);

GO
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [FK_Images_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]);

GO
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [DF_Images_Id] DEFAULT (NEWSEQUENTIALID()) FOR [Id];

GO
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [DF_Images_CreatedAt] DEFAULT (SYSUTCDATETIME()) FOR [CreatedAt];

GO
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [DF_Images_IsPublished] DEFAULT ((1)) FOR [IsPublished];

GO
ALTER TABLE [dbo].[Images]
ADD CONSTRAINT [DF_Images_PublishedAt] DEFAULT (SYSDATETIMEOFFSET()) FOR [PublishedAt];

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Images_FileName]
ON [dbo].[Images]([FileName]);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Images_Path]
ON [dbo].[Images]([Path]);