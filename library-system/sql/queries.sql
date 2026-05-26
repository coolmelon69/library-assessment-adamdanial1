-- Library System assessment queries.
-- Run after schema.sql and seed.sql, or against an equivalent database.

-- 1. Top 5 most-borrowed books of all time.
SELECT TOP (5)
    b.Id,
    b.Title,
    b.Author,
    COUNT(l.Id) AS BorrowCount
FROM dbo.Books AS b
INNER JOIN dbo.Loans AS l ON l.BookId = b.Id
GROUP BY b.Id, b.Title, b.Author
ORDER BY BorrowCount DESC, b.Title ASC;

-- 2. Members with overdue active loans older than 14 days.
SELECT
    m.Id AS MemberId,
    m.FullName,
    m.Email,
    l.Id AS LoanId,
    b.Title,
    l.BorrowedDate,
    DATEDIFF(DAY, l.BorrowedDate, SYSUTCDATETIME()) AS DaysBorrowed
FROM dbo.Loans AS l
INNER JOIN dbo.Members AS m ON m.Id = l.MemberId
INNER JOIN dbo.Books AS b ON b.Id = l.BookId
WHERE l.ReturnedDate IS NULL
  AND l.BorrowedDate < DATEADD(DAY, -14, SYSUTCDATETIME())
ORDER BY DaysBorrowed DESC, m.FullName ASC;

-- 3. Loan count for each month of the last 12 months, including zero-loan months.
WITH MonthNumbers AS
(
    SELECT 0 AS MonthOffset
    UNION ALL
    SELECT MonthOffset + 1
    FROM MonthNumbers
    WHERE MonthOffset < 11
),
MonthWindow AS
(
    SELECT
        DATEFROMPARTS(
            YEAR(DATEADD(MONTH, -MonthOffset, SYSUTCDATETIME())),
            MONTH(DATEADD(MONTH, -MonthOffset, SYSUTCDATETIME())),
            1) AS MonthStart
    FROM MonthNumbers
)
SELECT
    mw.MonthStart,
    COUNT(l.Id) AS LoanCount
FROM MonthWindow AS mw
LEFT JOIN dbo.Loans AS l
    ON l.BorrowedDate >= mw.MonthStart
   AND l.BorrowedDate < DATEADD(MONTH, 1, mw.MonthStart)
GROUP BY mw.MonthStart
ORDER BY mw.MonthStart ASC
OPTION (MAXRECURSION 12);

-- 4. Books that have never been borrowed.
SELECT
    b.Id,
    b.Title,
    b.Author,
    b.ISBN
FROM dbo.Books AS b
LEFT JOIN dbo.Loans AS l ON l.BookId = b.Id
WHERE l.Id IS NULL
ORDER BY b.Title ASC;

-- 5. Member with the longest single returned-loan duration.
SELECT TOP (1)
    m.Id AS MemberId,
    m.FullName,
    m.Email,
    l.Id AS LoanId,
    b.Title,
    l.BorrowedDate,
    l.ReturnedDate,
    DATEDIFF(DAY, l.BorrowedDate, l.ReturnedDate) AS LoanDurationDays
FROM dbo.Loans AS l
INNER JOIN dbo.Members AS m ON m.Id = l.MemberId
INNER JOIN dbo.Books AS b ON b.Id = l.BookId
WHERE l.ReturnedDate IS NOT NULL
ORDER BY LoanDurationDays DESC, l.ReturnedDate DESC;
