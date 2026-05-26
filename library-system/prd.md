# Library System Assessment PRD

## Overview

This project is a one-tier ASP.NET Core library management system for the Software Developer coding assessment. The application must manage books, members, and loans in a single ASP.NET Core project. It must not use a separate frontend and backend.

The app will expose MVC/Razor pages where useful for reviewer usability and API-style endpoints for the assessment-required behavior. The API behavior is the source of truth for books, members, authentication, and loans.

## Confirmed Stack

- Language: C# only
- Framework: ASP.NET Core on .NET 8
- Application shape: one-tier MVC + APIs in one project
- Data access: Entity Framework Core
- Database: Microsoft SQL Server Express LocalDB
- Authentication: Google bearer ID token validation
- Validation: DataAnnotations unless a later phase explicitly changes this
- Testing: xUnit
- IDE target: Visual Studio 2022

## Functional Requirements

### Books

- Add a new book with:
  - Title
  - Author
  - ISBN
  - PublishedYear
  - TotalCopies
- List all books.
- Support optional case-insensitive partial filtering by title.
- Support optional case-insensitive partial filtering by author.
- Retrieve a single book by ID.
- Book detail responses must include currently available copies.

Available copies are calculated as:

```text
TotalCopies - number of active loans for the book
```

An active loan is a loan where `ReturnedDate` is null.

### Members

- Members do not register manually.
- Members are provisioned from Google token claims on first authenticated request.
- Required token claims:
  - `sub` as the stable Google subject identifier
  - `name` as the member full name
  - `email` as the member email
- On first sign-in/request, create a member record with:
  - SsoSubject
  - FullName
  - Email
  - JoinedDate
- On later requests, resolve the existing member by `SsoSubject`.
- `GET /me` returns the currently authenticated member profile.
- Member identity must always come from token claims, never from request body values or URL member IDs.

### Loans

- Authenticated members can borrow one copy of a book.
- Borrowing must be rejected if no copies are available.
- Borrowing must be rejected if the member already has 3 or more active loans.
- Authenticated members can return their own active loans.
- Members cannot return another member's loan.
- Returning a loan records `ReturnedDate`.
- `GET /me/loans` returns active loans for the current authenticated member.

## Authentication Requirements

The member-facing endpoints are protected:

- `GET /me`
- `GET /me/loans`
- `POST /books/{bookId}/borrow`
- `POST /loans/{loanId}/return`

Protected endpoints require:

```http
Authorization: Bearer <google-id-token>
```

The application must use ASP.NET Core authentication middleware to validate Google-issued tokens. It must not roll its own token validation.

Token validation must cover:

- Issuer
- Audience
- Signature
- Expiry

Expected behavior:

- Missing token returns `401 Unauthorized`.
- Invalid or expired token returns `401 Unauthorized`.
- Authenticated member accessing another member's data returns `403 Forbidden`.

Google configuration must use placeholders only. Real secrets must not be committed.

Expected configuration keys:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "<google-oauth-client-id>"
    }
  }
}
```

## Database Requirements

Use SQL Server Express LocalDB as the default local database.

Default connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LibrarySystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Core tables/entities:

- Books
- Members
- Loans

Required constraints:

- Book ISBN must be unique.
- Member SSO subject must be unique.
- Required text fields must be non-empty.
- TotalCopies must be non-negative.
- Loans must reference valid books and members.
- ReturnedDate is nullable and indicates whether a loan is active.

## API Behavior

Book read endpoints are public:

- `GET /books`
- `GET /books/{id}`

Book creation is authenticated:

- `POST /books`

Member and loan endpoints are authenticated:

- `GET /me`
- `GET /me/loans`
- `POST /books/{bookId}/borrow`
- `POST /loans/{loanId}/return`

Expected response behavior:

- Invalid input returns `400 Bad Request`.
- Missing or invalid authentication returns `401 Unauthorized`.
- Access to another member's loan returns `403 Forbidden`.
- Missing resources return `404 Not Found`.
- Successful book creation returns `201 Created`.

## SQL Deliverables

Create a `/sql` folder with the following files:

- `schema.sql`
- `seed.sql`
- `queries.sql`

`schema.sql` must include:

- CREATE TABLE statements for Books, Members, and Loans
- Primary keys
- Foreign keys
- Indexes
- Constraints
- SQL comments explaining index choices

`seed.sql` must include:

- At least 10 books
- At least 5 members
- At least 10 loans
- A mix of active and returned loans

`queries.sql` must include:

- Top 5 most-borrowed books of all time
- Members with overdue active loans older than 14 days
- Loan count for each month of the last 12 months, including zero-loan months
- Books that have never been borrowed
- Member with the longest single returned-loan duration

All SQL must be T-SQL and runnable on SQL Server 2019 or later.

## Testing Requirements

Use xUnit for automated tests.

Minimum business-rule tests:

- Borrowing is rejected when no copies are available.
- Borrowing is rejected when the member already has 3 active loans.
- A new member is provisioned from Google claims on first request.
- An existing member is reused for the same Google `sub`.
- Returning another member's loan is rejected.

Prefer service-level tests for business rules so the tests remain focused and fast.

## Documentation And Submission Requirements

The final submission must include:

- README.md
- Standard Visual Studio/.NET `.gitignore`
- SQL scripts in `/sql`
- Unit tests
- Clear setup instructions
- Clear run instructions
- Clear test instructions
- Google OAuth client setup instructions
- LocalDB connection string documentation
- EF Core data-access justification
- Authorization decision notes
- Any unfinished work, if applicable

Do not commit:

- Google client secrets
- Signing keys
- Real credentials
- `bin/`
- `obj/`
- Build artifacts

## Assumptions

- One-tier means one ASP.NET Core project and one deployable app.
- MVC/Razor views and API endpoints can coexist in the same project.
- Google bearer ID token validation satisfies the assessment requirement for a valid token from an external identity provider.
- SQL Server Express LocalDB is the default reviewer database.
- Book list and book detail are public.
- Book creation, member profile, and loan actions require authentication.
