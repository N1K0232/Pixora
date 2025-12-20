CREATE TABLE [dbo].[IpAddressBans]
(
	[Id]        UNIQUEIDENTIFIER          NOT NULL,
    [Value]     NVARCHAR (255)            NOT NULL,
    [Reason]    NVARCHAR (4000)           NULL,
    [BannedAt]  DATETIMEOFFSET (7)        NOT NULL,
    [ExpiresAt] DATETIMEOFFSET (7)        NULL
);

GO
ALTER TABLE [dbo].[IpAddressBans]
ADD CONSTRAINT [PK_IpAddressBans] PRIMARY KEY ([Id] ASC);

GO
ALTER TABLE [dbo].[IpAddressBans]
ADD CONSTRAINT [DF_IpAddressBans_Id] DEFAULT (NEWSEQUENTIALID()) FOR [Id];