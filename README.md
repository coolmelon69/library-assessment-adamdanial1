# Library System Assessment

This is a one-project ASP.NET Core MVC + API library system built with .NET 8, Entity Framework Core, SQL Server LocalDB, and xUnit tests.

## What To Install First

Install these before running the project:

1. Visual Studio 2022
2. .NET 8 SDK
3. SQL Server Express LocalDB

Visual Studio 2022 usually includes LocalDB if the ASP.NET/web development workload is installed.

## Open The Project

1. Open Visual Studio 2022.
2. Click `Open a project or solution`.
3. Open this file:

```text
library-system.sln
```

## Create The Database Tables

1. Open a terminal in the main project folder.
2. Go inside the web app folder:

```powershell
cd library-system
```

3. Restore the project:

```powershell
dotnet restore
```

4. Restore the EF database tool:

```powershell
dotnet tool restore
```

5. Create the database tables:

```powershell
dotnet tool run dotnet-ef database update
```

This creates the LocalDB database named `LibrarySystemDb`.

## Add Sample Data

After creating the database, run this SQL file:

```text
library-system\sql\seed.sql
```

Simple Visual Studio way:

1. Open `View` > `SQL Server Object Explorer`.
2. Expand `(localdb)\MSSQLLocalDB`.
3. Expand `Databases`.
4. Click `LibrarySystemDb`.
5. Open `library-system\sql\seed.sql`.
6. Click `Execute`.

The seed file can be run again if needed. It resets the sample books, members, and loans.

## Run The Website

From the `library-system` folder, run:

```powershell
dotnet run
```

Then open the local website URL shown in the terminal.

Common URL:

```text
https://localhost:7092
```

If the browser warns about a developer certificate, run this once:

```powershell
dotnet dev-certs https --trust
```

## Test The Project

From the folder that contains `library-system.sln`, run:

```powershell
dotnet test ".\XUnit Test for Library System\XUnit Test for Library System.csproj"
```

If Visual Studio is locking files, use:

```powershell
dotnet test ".\XUnit Test for Library System\XUnit Test for Library System.csproj" -p:UseAppHost=false -p:OutputPath=.test-output\
```

## Optional: Google Login And Admin Access

Public book pages can be reviewed without Google login.

To test login/admin features, add real Google OAuth values in `library-system\appsettings.json` or user secrets:

```json
"Authentication": {
  "Google": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  }
}
```

After signing in once, promote that account to admin:

```sql
UPDATE dbo.Members
SET Role = N'Admin'
WHERE Email = N'your-email@gmail.com';
```

## Manual SQL Backup Option

If EF migrations are not used, run these SQL files in this order:

1. `library-system\sql\schema.sql`
2. `library-system\sql\seed.sql`

The `schema.sql` file matches the current EF Core migrations and marks those migrations as applied.
