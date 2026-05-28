# Library System Assessment

One-tier ASP.NET Core library management system for the Software Developer coding assessment.

## Stack

- C# only
- ASP.NET Core on .NET 8
- MVC + API endpoints in one project
- Entity Framework Core
- SQL Server Express LocalDB
- Google bearer ID token authentication
- Database-backed User/Admin roles
- xUnit tests

## Current Status

This project is being implemented phase by phase. The current baseline documents are:

- `prd.md` for product and technical requirements
- `kanban.md` for phased implementation tracking

## Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server Express LocalDB

## Database

Default local database target:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LibrarySystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

## Authentication And Authorization

Member-facing endpoints will use Google bearer ID token authentication.

Protected endpoints will expect:

```http
Authorization: Bearer <google-id-token>
```

Google client configuration will use placeholders only. Real secrets must not be committed.

Members are provisioned from Google claims on first authenticated `/me` access. New members default to the `User` role.

Admin access is stored on the `Members.Role` column. To promote the first admin, sign in once with Google so the member row exists, then run:

```sql
UPDATE dbo.Members
SET Role = N'Admin'
WHERE Email = N'<admin-email>';
```

Chosen book authorization policy:

- Public: `GET /books`, `GET /books/{id}`, and the public catalog UI.
- Any authenticated user: `/me`, `/me/loans`, borrow, and return own loans.
- Admin only: book create, update, delete, and `/admin/books` management screens.

## Protected Endpoints

- `GET /me`
- `GET /me/loans`
- `POST /books/{bookId}/borrow`
- `POST /loans/{loanId}/return`
- `POST /books` requires Admin
- `PUT /books/{id}` requires Admin
- `DELETE /books/{id}` requires Admin

## Public Endpoints

- `GET /books`
- `GET /books/{id}`

## How To Run

Detailed run instructions will be completed during the final documentation phase.

For the current baseline:

```powershell
dotnet build
```

## How To Test

Automated tests are included in the `XUnit Test for Library System` xUnit project.

From the project root, run:

```powershell
dotnet test ".\XUnit Test for Library System\XUnit Test for Library System.csproj"
```

The test suite covers the main business rules required by the assessment:

- Borrowing is rejected when no copies are available.
- Borrowing is rejected when a member already has 3 active loans.
- First-time authenticated members are provisioned from Google claims.
- Existing members are reused by Google `sub`.
- Returning another member's loan is rejected.

If Visual Studio or a running app locks build outputs, run the same test project with an isolated output folder:

```powershell
dotnet test ".\XUnit Test for Library System\XUnit Test for Library System.csproj" -p:UseAppHost=false -p:OutputPath=.phase10-test-bin\
```

## Notes

- This is a one-tier application: no separate frontend or backend project.
- Book list and book detail endpoints are public.
- Book creation, update, and delete require the Admin role.
- Member profile and loan actions require authentication.
- SQL scripts will be added under `/sql` in a later phase.
