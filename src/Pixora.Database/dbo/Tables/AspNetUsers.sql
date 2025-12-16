CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                         UNIQUEIDENTIFIER   CONSTRAINT [DF_AspNetUsers_Id] DEFAULT (newsequentialid()) NOT NULL,
    [UserName]                   NVARCHAR (256)     NULL,
    [NormalizedUserName]         NVARCHAR (256)     NULL,
    [FirstName]                  NVARCHAR (256)     NOT NULL,
    [LastName]                   NVARCHAR (256)     NOT NULL,
    [EnableNotifications]        BIT                CONSTRAINT [DF_AspNetUsers_EnableNotifications] DEFAULT ((0)) NOT NULL,
    [ProfilePhoto]               NVARCHAR (512)     NULL,
    [Email]                      NVARCHAR (256)     NULL,
    [NormalizedEmail]            NVARCHAR (256)     NULL,
    [EmailConfirmed]             BIT                CONSTRAINT [DF_AspNetUsers_EmailConfirmed] DEFAULT ((0)) NOT NULL,
    [PasswordHash]               NVARCHAR (MAX)     NULL,
    [SecurityStamp]              NVARCHAR (MAX)     NULL,
    [ConcurrencyStamp]           NVARCHAR (MAX)     NULL,
    [PhoneNumber]                NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed]       BIT                CONSTRAINT [DF_AspNetUsers_PhoneNumberConfirmed] DEFAULT ((0)) NOT NULL,
    [TwoFactorEnabled]           BIT                CONSTRAINT [DF_AspNetUsers_TwoFactorEnabled] DEFAULT ((0)) NOT NULL,
    [LockoutEnd]                 DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]             BIT                CONSTRAINT [DF_AspNetUsers_LockoutEnabled] DEFAULT ((1)) NOT NULL,
    [AccessFailedCount]          INT                NOT NULL,
    [RefreshToken]               NVARCHAR (2048)    NULL,
    [RefreshTokenExpirationDate] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);

