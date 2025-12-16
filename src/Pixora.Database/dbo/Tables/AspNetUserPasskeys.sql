CREATE TABLE [dbo].[AspNetUserPasskeys] (
    [Id]                UNIQUEIDENTIFIER   NOT NULL,
    [UserId]            UNIQUEIDENTIFIER   NOT NULL,
    [Name]              NVARCHAR (256)     NULL,
    [PublicKey]         VARBINARY (MAX)    NOT NULL,
    [UserHandle]        VARBINARY (MAX)    NULL,
    [CredentialId]      VARBINARY (MAX)    NOT NULL,
    [SignatureCounter]  BIGINT             NOT NULL,
    [AttestationFormat] NVARCHAR (100)     NULL,
    [AttestationData]   VARBINARY (MAX)    NULL,
    [CreatedAt]         DATETIMEOFFSET (7) DEFAULT (sysdatetimeoffset()) NOT NULL,
    CONSTRAINT [PK_AspNetUserPasskeys] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUserPasskeys_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

