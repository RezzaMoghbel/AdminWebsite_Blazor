# Database Reset Guide

This guide provides step-by-step instructions for resetting the Mars Admin database to a fresh state with all seed data.

## Prerequisites

- **Database**: MS SQL Server
- **Database Name**: MarsAdminDB
- **Server**: HELLODIGI
- **User**: MarsAdminDBUser
- **Password**: StrongP@s$w0rd

## Quick Reset Commands

### Option 1: Complete Database Reset (Recommended)

```powershell
# 1. Stop any running Mars.Admin processes
Get-Process -Name "Mars.Admin" -ErrorAction SilentlyContinue | Stop-Process -Force

# 2. Drop the entire database
dotnet ef database drop --project Mars.Admin --force

# 3. Recreate the database with all migrations
dotnet ef database update --project Mars.Admin

# 4. Start the application (seed data runs automatically)
dotnet run --project Mars.Admin
```

### Option 2: Manual SQL Server Reset

```sql
-- Connect to SQL Server Management Studio or sqlcmd
-- Run these commands:

USE master;
GO

-- Drop the database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'MarsAdminDB')
BEGIN
    ALTER DATABASE MarsAdminDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MarsAdminDB;
END
GO

-- Recreate the database
CREATE DATABASE MarsAdminDB;
GO
```

Then run:

```powershell
dotnet ef database update --project Mars.Admin
dotnet run --project Mars.Admin
```

## Detailed Step-by-Step Process

### Step 1: Stop Running Application

**Check for running processes:**

```powershell
Get-Process -Name "Mars.Admin" -ErrorAction SilentlyContinue
```

**Stop the process if running:**

```powershell
# If you see a process, note the ProcessId and stop it
Stop-Process -Id <ProcessId> -Force

# Or stop all Mars.Admin processes
Get-Process -Name "Mars.Admin" -ErrorAction SilentlyContinue | Stop-Process -Force
```

### Step 2: Drop Database

**Using Entity Framework (Recommended):**

```powershell
dotnet ef database drop --project Mars.Admin --force
```

**Expected Output:**

```
Build started...
Build succeeded.
Dropping database 'MarsAdminDB' on server 'HELLODIGI'.
Successfully dropped database 'MarsAdminDB'.
```

### Step 3: Recreate Database

**Apply all migrations:**

```powershell
dotnet ef database update --project Mars.Admin
```

**Expected Output:**

```
Build started...
Build succeeded.
Creating database 'MarsAdminDB' on server 'HELLODIGI'.
Applying migration '00000000000000_CreateIdentitySchema'...
Applying migration '20250922150730_AddUserRolesPermissionsWebsitesIpSafe'...
Applying migration '20250922153420_UpdateUserEmailsToSIDomain'...
Applying migration '20250922185227_AddUserActiveDeletedFields'...
Applying migration '20250922191652_FixEmailDomainsToSafelyInsured'...
Applying migration '20250922205019_FixCorruptedUserWebsiteAccess'...
Applying migration '20250922205449_FixNullUserRoleId'...
Applying migration '20250923085327_RemoveTypeFromIPSafeListing'...
Done.
```

### Step 4: Start Application

**Run the application:**

```powershell
dotnet run --project Mars.Admin
```

**Expected Output:**

```
Build started...
Build succeeded.
info: Program[0] Seed data completed
info: Microsoft.Hosting.Lifetime[14] Now listening on: http://localhost:5065
info: Microsoft.Hosting.Lifetime[0] Application started. Press Ctrl+C to shut down.
```

### Step 5: Verify Application

**Check if application is running:**

```powershell
Get-Process -Name "Mars.Admin" -ErrorAction SilentlyContinue
netstat -ano | findstr :5065
```

**Access the application:**

- **URL**: `http://localhost:5065`
- **Login**: Use credentials from `TestCredentials.md`

## What Gets Reset

### Database Structure

- **AspNetUsers**: Identity users table
- **AspNetRoles**: Identity roles table
- **UserRoles**: Custom user roles (SuperAdmin, Developer, Manager, etc.)
- **Permissions**: System permissions (User.Create, User.Read, etc.)
- **UserRolePermissions**: Role-permission assignments
- **Websites**: Website entities (InsureLearnerDriver, SafelyInsured)
- **UserWebsiteAccesses**: User-website access assignments
- **IPSafeListings**: IP address safe listings
- **AuditLogs**: System audit trail

### Seed Data Created

- **6 User Roles**: SuperAdmin, Developer, Manager, Account, Viewer, Customer Service
- **24 Permissions**: Full CRUD permissions for all entities
- **7 Test Users**: One for each role with test credentials
- **3 Websites**: InsureLearnerDriver (ID, ILD), SafelyInsured (SI)
- **21 Website Access Records**: All users assigned to all websites
- **11 IP Safe Listings**: Office IPs (127.0.0.1, ::1, private ranges) + individual IPs

## Troubleshooting

### Common Issues

**1. Process Lock Error:**

```
The file is locked by: "Mars.Admin (PID)"
```

**Solution:**

```powershell
taskkill /F /PID <PID>
# Or
Get-Process -Name "Mars.Admin" | Stop-Process -Force
```

**2. Port Already in Use Error:**

```
Failed to bind to address http://127.0.0.1:5065: address already in use
```

**Solution:**

```powershell
# Find what's using port 5065
netstat -ano | findstr :5065

# Stop the process using that port
Stop-Process -Id <PID> -Force

# Or stop all Mars.Admin processes
Get-Process -Name "Mars.Admin" | Stop-Process -Force
```

**3. Database Connection Error:**

```
Cannot connect to server 'HELLODIGI'
```

**Solution:**

- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure user has proper permissions

**4. Migration Error:**

```
Migration already applied
```

**Solution:**

```powershell
dotnet ef database drop --project Mars.Admin --force
dotnet ef database update --project Mars.Admin
```

**5. Seed Data Not Running:**

- Check `Program.cs` for `seedService.SeedAsync()` call
- Verify `SeedDataService` is registered in DI container
- Check application logs for seed data messages

### Verification Commands

**Check database exists:**

```sql
SELECT name FROM sys.databases WHERE name = 'MarsAdminDB'
```

**Check tables created:**

```sql
USE MarsAdminDB;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'
```

**Check seed data:**

```sql
USE MarsAdminDB;
SELECT COUNT(*) FROM UserRoles;        -- Should be 6
SELECT COUNT(*) FROM Permissions;     -- Should be 24
SELECT COUNT(*) FROM AspNetUsers;     -- Should be 7
SELECT COUNT(*) FROM Websites;        -- Should be 3
SELECT COUNT(*) FROM IPSafeListings;  -- Should be 11
```

## Alternative Reset Methods

### Method 1: Clear Data Only (Keep Structure)

```sql
USE MarsAdminDB;
-- Clear all data but keep tables
DELETE FROM UserWebsiteAccesses;
DELETE FROM UserRolePermissions;
DELETE FROM IPSafeListings;
DELETE FROM AspNetUsers;
DELETE FROM UserRoles;
DELETE FROM Permissions;
DELETE FROM Websites;
DELETE FROM AuditLogs;
```

### Method 2: Reset Specific Tables

```sql
USE MarsAdminDB;
-- Reset only specific tables
DELETE FROM UserWebsiteAccesses;
DELETE FROM UserRolePermissions;
DELETE FROM IPSafeListings;
```

### Method 3: Backup and Restore

```powershell
# Backup current database
sqlcmd -S HELLODIGI -U MarsAdminDBUser -P StrongP@s$w0rd -Q "BACKUP DATABASE MarsAdminDB TO DISK = 'C:\Backup\MarsAdminDB.bak'"

# Restore from backup
sqlcmd -S HELLODIGI -U MarsAdminDBUser -P StrongP@s$w0rd -Q "RESTORE DATABASE MarsAdminDB FROM DISK = 'C:\Backup\MarsAdminDB.bak'"
```

## Post-Reset Checklist

- [ ] Database dropped successfully
- [ ] Database recreated with all migrations
- [ ] Application starts without errors
- [ ] Seed data completed (check logs)
- [ ] Can access application at `http://localhost:5065`
- [ ] Can login with test credentials
- [ ] All admin pages accessible
- [ ] Permissions working correctly
- [ ] Website access functioning
- [ ] IP safe listing working

## Notes

- **Seed data runs automatically** on application startup
- **No manual data insertion** required
- **All relationships** are properly established
- **Permissions are assigned** to appropriate roles
- **Test users** are created with proper role assignments
- **IP safe listings** include office and individual IPs
- **Website access** is granted to all users for all websites

## Support

If you encounter issues:

1. Check the application logs for error messages
2. Verify SQL Server connectivity
3. Ensure all prerequisites are met
4. Review the troubleshooting section above
5. Check the `TestCredentials.md` file for login information

---

**Last Updated**: January 2025  
**Version**: 1.0  
**Database**: MarsAdminDB (MS SQL Server)
