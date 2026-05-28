-- Library System assessment schema for SQL Server 2019 or later.
-- This mirrors the current EF Core migrations:
--   20260526045433_InitialCreate
--   20260528123902_AddMemberRoles

IF OBJECT_ID(N'dbo.Loans', N'U') IS NOT NULL
    DROP TABLE dbo.Loans;

IF OBJECT_ID(N'dbo.Members', N'U') IS NOT NULL
    DROP TABLE dbo.Members;

IF OBJECT_ID(N'dbo.Books', N'U') IS NOT NULL
    DROP TABLE dbo.Books;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
    DROP TABLE dbo.__EFMigrationsHistory;
GO

CREATE TABLE dbo.__EFMigrationsHistory
(
    MigrationId NVARCHAR(150) NOT NULL,
    ProductVersion NVARCHAR(32) NOT NULL,

    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
);
GO

CREATE TABLE dbo.Books
(
    Id INT IDENTITY(1,1) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(150) NOT NULL,
    ISBN NVARCHAR(20) NOT NULL,
    PublishedYear INT NOT NULL,
    TotalCopies INT NOT NULL,

    CONSTRAINT PK_Books PRIMARY KEY (Id),
    CONSTRAINT CK_Books_TotalCopies_NonNegative CHECK ([TotalCopies] >= 0)
);
GO

CREATE TABLE dbo.Members
(
    Id INT IDENTITY(1,1) NOT NULL,
    SsoSubject NVARCHAR(200) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(254) NOT NULL,
    JoinedDate DATETIME2 NOT NULL,
    Role NVARCHAR(30) NOT NULL CONSTRAINT DF_Members_Role DEFAULT N'User',

    CONSTRAINT PK_Members PRIMARY KEY (Id),
    CONSTRAINT CK_Members_Role_Allowed CHECK ([Role] IN ('User', 'Admin'))
);
GO

CREATE TABLE dbo.Loans
(
    Id INT IDENTITY(1,1) NOT NULL,
    BookId INT NOT NULL,
    MemberId INT NOT NULL,
    BorrowedDate DATETIME2 NOT NULL,
    ReturnedDate DATETIME2 NULL,

    CONSTRAINT PK_Loans PRIMARY KEY (Id),
    CONSTRAINT FK_Loans_Books_BookId
        FOREIGN KEY (BookId) REFERENCES dbo.Books (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Loans_Members_MemberId
        FOREIGN KEY (MemberId) REFERENCES dbo.Members (Id) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX IX_Books_ISBN ON dbo.Books (ISBN);
GO

CREATE INDEX IX_Loans_BookId_ReturnedDate ON dbo.Loans (BookId, ReturnedDate);
GO

CREATE INDEX IX_Loans_MemberId_ReturnedDate ON dbo.Loans (MemberId, ReturnedDate);
GO

CREATE UNIQUE INDEX IX_Members_SsoSubject ON dbo.Members (SsoSubject);
GO

INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
VALUES
    (N'20260526045433_InitialCreate', N'8.0.27'),
    (N'20260528123902_AddMemberRoles', N'8.0.27');
GO
