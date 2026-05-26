-- Library System assessment schema for SQL Server 2019 or later.
-- This script mirrors the EF Core model used by the ASP.NET Core app.

IF OBJECT_ID(N'dbo.Loans', N'U') IS NOT NULL
    DROP TABLE dbo.Loans;

IF OBJECT_ID(N'dbo.Members', N'U') IS NOT NULL
    DROP TABLE dbo.Members;

IF OBJECT_ID(N'dbo.Books', N'U') IS NOT NULL
    DROP TABLE dbo.Books;
GO

CREATE TABLE dbo.Books
(
    Id INT IDENTITY(1,1) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(150) NOT NULL,
    ISBN NVARCHAR(20) NOT NULL,
    PublishedYear INT NOT NULL,
    TotalCopies INT NOT NULL,

    CONSTRAINT PK_Books PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Books_Title_NotEmpty CHECK (LEN(TRIM(Title)) > 0),
    CONSTRAINT CK_Books_Author_NotEmpty CHECK (LEN(TRIM(Author)) > 0),
    CONSTRAINT CK_Books_ISBN_NotEmpty CHECK (LEN(TRIM(ISBN)) > 0),
    CONSTRAINT CK_Books_TotalCopies_NonNegative CHECK (TotalCopies >= 0)
);
GO

CREATE TABLE dbo.Members
(
    Id INT IDENTITY(1,1) NOT NULL,
    SsoSubject NVARCHAR(200) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(254) NOT NULL,
    JoinedDate DATETIME2 NOT NULL,

    CONSTRAINT PK_Members PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Members_SsoSubject_NotEmpty CHECK (LEN(TRIM(SsoSubject)) > 0),
    CONSTRAINT CK_Members_FullName_NotEmpty CHECK (LEN(TRIM(FullName)) > 0),
    CONSTRAINT CK_Members_Email_NotEmpty CHECK (LEN(TRIM(Email)) > 0)
);
GO

CREATE TABLE dbo.Loans
(
    Id INT IDENTITY(1,1) NOT NULL,
    BookId INT NOT NULL,
    MemberId INT NOT NULL,
    BorrowedDate DATETIME2 NOT NULL,
    ReturnedDate DATETIME2 NULL,

    CONSTRAINT PK_Loans PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Loans_Books_BookId
        FOREIGN KEY (BookId) REFERENCES dbo.Books(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Loans_Members_MemberId
        FOREIGN KEY (MemberId) REFERENCES dbo.Members(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_Loans_ReturnedAfterBorrowed
        CHECK (ReturnedDate IS NULL OR ReturnedDate >= BorrowedDate)
);
GO

-- Supports unique ISBN lookup and rejects duplicate book creation.
CREATE UNIQUE INDEX IX_Books_ISBN ON dbo.Books (ISBN);
GO

-- Supports available-copy calculations by quickly finding active loans per book.
CREATE INDEX IX_Loans_BookId_ReturnedDate ON dbo.Loans (BookId, ReturnedDate);
GO

-- Supports current-member active loan lists and the 3-active-loan rule.
CREATE INDEX IX_Loans_MemberId_ReturnedDate ON dbo.Loans (MemberId, ReturnedDate);
GO

-- Supports member provisioning by stable Google subject identifier.
CREATE UNIQUE INDEX IX_Members_SsoSubject ON dbo.Members (SsoSubject);
GO
