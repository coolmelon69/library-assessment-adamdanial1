-- Library System assessment seed data.
-- Run after schema.sql.

SET IDENTITY_INSERT dbo.Books ON;

INSERT INTO dbo.Books (Id, Title, Author, ISBN, PublishedYear, TotalCopies)
VALUES
    (1, N'Clean Code', N'Robert C. Martin', N'9780132350884', 2008, 4),
    (2, N'The Pragmatic Programmer', N'David Thomas', N'9780201616224', 1999, 3),
    (3, N'Domain-Driven Design', N'Eric Evans', N'9780321125217', 2003, 2),
    (4, N'Refactoring', N'Martin Fowler', N'9780134757599', 2018, 3),
    (5, N'Working Effectively with Legacy Code', N'Michael Feathers', N'9780131177055', 2004, 2),
    (6, N'Design Patterns', N'Erich Gamma', N'9780201633610', 1994, 2),
    (7, N'Code Complete', N'Steve McConnell', N'9780735619678', 2004, 2),
    (8, N'Patterns of Enterprise Application Architecture', N'Martin Fowler', N'9780321127426', 2002, 2),
    (9, N'Test Driven Development', N'Kent Beck', N'9780321146533', 2002, 3),
    (10, N'Continuous Delivery', N'Jez Humble', N'9780321601919', 2010, 2),
    (11, N'Never Borrowed Reference', N'Assessment Library', N'9780000000011', 2024, 1);

SET IDENTITY_INSERT dbo.Books OFF;

SET IDENTITY_INSERT dbo.Members ON;

INSERT INTO dbo.Members (Id, SsoSubject, FullName, Email, Role, JoinedDate)
VALUES
    (1, N'google-sub-001', N'Aisha Tan', N'aisha@example.test', N'User', DATEADD(DAY, -90, SYSUTCDATETIME())),
    (2, N'google-sub-002', N'Ben Lim', N'ben@example.test', N'User', DATEADD(DAY, -75, SYSUTCDATETIME())),
    (3, N'google-sub-003', N'Chloe Wong', N'chloe@example.test', N'User', DATEADD(DAY, -60, SYSUTCDATETIME())),
    (4, N'google-sub-004', N'Daniel Lee', N'daniel@example.test', N'User', DATEADD(DAY, -45, SYSUTCDATETIME())),
    (5, N'google-sub-005', N'Emily Ng', N'emily@example.test', N'User', DATEADD(DAY, -30, SYSUTCDATETIME()));

SET IDENTITY_INSERT dbo.Members OFF;

SET IDENTITY_INSERT dbo.Loans ON;

INSERT INTO dbo.Loans (Id, BookId, MemberId, BorrowedDate, ReturnedDate)
VALUES
    (1, 1, 1, DATEADD(DAY, -40, SYSUTCDATETIME()), DATEADD(DAY, -32, SYSUTCDATETIME())),
    (2, 1, 2, DATEADD(DAY, -28, SYSUTCDATETIME()), NULL),
    (3, 1, 3, DATEADD(DAY, -15, SYSUTCDATETIME()), DATEADD(DAY, -8, SYSUTCDATETIME())),
    (4, 2, 1, DATEADD(DAY, -20, SYSUTCDATETIME()), NULL),
    (5, 2, 4, DATEADD(DAY, -10, SYSUTCDATETIME()), DATEADD(DAY, -3, SYSUTCDATETIME())),
    (6, 3, 2, DATEADD(DAY, -18, SYSUTCDATETIME()), NULL),
    (7, 4, 5, DATEADD(MONTH, -2, SYSUTCDATETIME()), DATEADD(MONTH, -1, SYSUTCDATETIME())),
    (8, 5, 3, DATEADD(DAY, -5, SYSUTCDATETIME()), NULL),
    (9, 6, 4, DATEADD(MONTH, -4, SYSUTCDATETIME()), DATEADD(MONTH, -3, SYSUTCDATETIME())),
    (10, 7, 5, DATEADD(MONTH, -7, SYSUTCDATETIME()), DATEADD(MONTH, -6, SYSUTCDATETIME())),
    (11, 8, 1, DATEADD(MONTH, -11, SYSUTCDATETIME()), DATEADD(MONTH, -10, SYSUTCDATETIME())),
    (12, 9, 2, DATEADD(DAY, -2, SYSUTCDATETIME()), NULL);

SET IDENTITY_INSERT dbo.Loans OFF;
