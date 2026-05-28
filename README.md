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

## Required: Google SSO Setup

This project uses Google as its only configured identity provider for browser sign-in and authenticated member/admin flows.

A reviewer can open the public catalog pages without Google login, but to run the full application as intended they must create their own Google OAuth client, add its client ID and secret to the app, and sign in with Google.

This repo does not include a local Keycloak or mock OIDC container. The supported reviewer path is Google SSO configuration.

### Step 1: Create A Google OAuth Client

1. Open the [Google Cloud Console](https://console.cloud.google.com/).
2. Create or select a project.
3. Open `APIs & Services` > `Credentials`.
4. Click `Create Credentials` > `OAuth client ID`.
5. If prompted, configure the OAuth consent screen first.
6. Choose `Web application`.
7. Add this authorized redirect URI for `dotnet run`:

```text
https://localhost:7092/signin-oidc
```

8. If you plan to run the app through IIS Express in Visual Studio, also add:

```text
https://localhost:44304/signin-oidc
```

9. Save the client and copy the generated `Client ID` and `Client secret`.

### Step 2: Put The Client ID And Secret Into The App

Use either `appsettings.json` or user secrets.

Option A - edit `library-system\appsettings.json`:

```json
"Authentication": {
  "Google": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  }
}
```

Option B - keep secrets out of the file and use the .NET user-secrets store from the `library-system` folder:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
```

### Step 3: Start The App

From the `library-system` folder, run:

```powershell
dotnet run
```

Use the HTTPS launch URL:

```text
https://localhost:7092
```

If your machine does not trust the ASP.NET Core development certificate yet, run:

```powershell
dotnet dev-certs https --trust
```

### Step 4: Sign In With Google

1. Open `https://localhost:7092`.
2. Click `Sign in with Google`.
3. Sign in with the Google account you want to use for testing.
4. After the redirect back to the app, open the authenticated pages such as `My Profile` / `Me` to confirm the sign-in worked.

On first authenticated access, the app provisions a `Members` row from the Google claims for that account.

### Step 5: Promote A Test Account To Admin If Needed

After signing in once, promote that account to admin:

```sql
UPDATE dbo.Members
SET Role = N'Admin'
WHERE Email = N'your-email@gmail.com';
```

Then sign out and sign in again so the refreshed role claims are applied.

### Troubleshooting

- `invalid_client`: the client ID or secret in the app does not match the Google OAuth client.
- `redirect_uri_mismatch`: the redirect URI in Google Cloud Console does not exactly match `https://localhost:7092/signin-oidc` or the IIS Express URI you are using.
- Browser sign-in loops back to the login button: check that the app is running on HTTPS and that the Google credentials were loaded into configuration.
- Authenticated pages work in browser but bearer-token API calls fail: the API expects a Google-issued bearer token for the same Google client ID configured in the app.

## Manual SQL Backup Option

If EF migrations are not used, run these SQL files in this order:

1. `library-system\sql\schema.sql`
2. `library-system\sql\seed.sql`

The `schema.sql` file matches the current EF Core migrations and marks those migrations as applied.
