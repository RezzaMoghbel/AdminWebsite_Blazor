# IIS Deployment Guide for Mars Admin

## 📋 Overview

This guide explains how to deploy the Mars Admin Blazor application to IIS on Windows Server using the pre-configured `web.config` files.

## 🔧 Pre-Configured Web.config Files

### 1. **web.config** (Production Default)

- **Environment**: Production
- **Features**: HTTPS redirect, security headers, compression, caching
- **Use**: Default deployment configuration

### 2. **web.Development.config** (Development)

- **Environment**: Development
- **Features**: No HTTPS redirect, simplified configuration
- **Use**: Local development with IIS

### 3. **web.Staging.config** (Staging)

- **Environment**: Staging
- **Features**: HTTPS redirect, production-like settings
- **Use**: Staging environment testing

## 🚀 Deployment Steps

### Step 1: Publish Application

```bash
# From Mars.Admin directory
dotnet publish -c Release -o ./publish
```

### Step 2: Choose Web.config for Environment

#### **For Production Deployment:**

- Use `web.config` (default)
- Environment: `ASPNETCORE_ENVIRONMENT=Production`
- Database: `MarsAdminProduction`

#### **For Staging Deployment:**

- Rename `web.Staging.config` to `web.config`
- Environment: `ASPNETCORE_ENVIRONMENT=Staging`
- Database: `MarsAdminStaging`

#### **For Development with IIS:**

- Rename `web.Development.config` to `web.config`
- Environment: `ASPNETCORE_ENVIRONMENT=Development`
- Database: `MarsAdminDev`

### Step 3: Copy Files to Server

```powershell
# Copy published files to IIS directory
Copy-Item -Path ".\publish\*" -Destination "C:\inetpub\wwwroot\MarsAdmin" -Recurse
```

### Step 4: Set Permissions

```powershell
# Grant IIS permissions
icacls "C:\inetpub\wwwroot\MarsAdmin" /grant "IIS_IUSRS:(OI)(CI)F" /T
icacls "C:\inetpub\wwwroot\MarsAdmin" /grant "IIS AppPool\MarsAdmin:(OI)(CI)F" /T

# Create logs directory
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\MarsAdmin\logs"
icacls "C:\inetpub\wwwroot\MarsAdmin\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

### Step 5: Configure IIS

#### **Create Application Pool:**

- Name: `MarsAdmin`
- .NET CLR Version: `No Managed Code`
- Managed Pipeline Mode: `Integrated`
- Identity: `ApplicationPoolIdentity`

#### **Create Website:**

- Site Name: `MarsAdmin`
- Application Pool: `MarsAdmin`
- Physical Path: `C:\inetpub\wwwroot\MarsAdmin`
- Binding: `http://yourdomain.com:80` or `https://yourdomain.com:443`

## 🔒 Security Features Included

### **Security Headers:**

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: SAMEORIGIN`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy: geolocation=(), microphone=(), camera=()`

### **Request Filtering:**

- Maximum content length: 50MB
- Blocked file extensions: `.config`, `.cs`, `.csproj`, `.sln`

### **HTTPS Redirect:**

- Automatically redirects HTTP to HTTPS (except localhost)
- Permanent redirect (301) for SEO

## 📊 Performance Features

### **Compression:**

- Dynamic compression for: JSON, JavaScript, CSS, HTML
- Static compression for: JSON, JavaScript, CSS, HTML

### **Caching:**

- Static content cache: 7 days
- Proper MIME types for fonts and JSON

### **SPA Routing:**

- Handles Blazor client-side routing
- Excludes API and Identity routes

## 🗄️ Database Configuration

### **Connection Strings by Environment:**

#### **Development:**

```json
"DefaultConnection": "Server=Dev-Db19;Database=MarsAdminDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

#### **Staging:**

```json
"DefaultConnection": "Server=Dev-Db19;Database=MarsAdminStaging;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

#### **Production:**

```json
"DefaultConnection": "Server=Dev-Db19;Database=MarsAdminProduction;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## 📝 Logging Configuration

### **Log Locations:**

- **IIS Logs**: `C:\inetpub\logs\LogFiles\W3SVC1\`
- **Application Logs**: `C:\inetpub\wwwroot\MarsAdmin\logs\`
- **SQL Server Logs**: `MarsAdmin{Environment}.dbo.Logs` table

### **Log Levels by Environment:**

- **Development**: Information level
- **Staging**: Warning level
- **Production**: Warning level

## 🔧 Troubleshooting

### **Common Issues:**

#### **500.30 Error (Startup Error):**

- Check `ASPNETCORE_ENVIRONMENT` setting in web.config
- Verify database connection string
- Check application pool identity permissions

#### **500.19 Error (Configuration Error):**

- Verify web.config syntax
- Check file permissions
- Ensure ASP.NET Core Module is installed

#### **Database Connection Issues:**

- Verify SQL Server is running
- Check Windows Authentication permissions
- Ensure database exists

### **Enable Detailed Logging:**

```xml
<!-- In web.config -->
<aspNetCore processPath="dotnet"
            arguments=".\Mars.Admin.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
```

## 🚀 Quick Deployment Commands

### **Production Deployment:**

```bash
# 1. Publish
dotnet publish -c Release -o ./publish

# 2. Copy to server (run on server)
Copy-Item -Path "C:\temp\MarsAdmin\publish\*" -Destination "C:\inetpub\wwwroot\MarsAdmin" -Recurse

# 3. Set permissions (run on server)
icacls "C:\inetpub\wwwroot\MarsAdmin" /grant "IIS_IUSRS:(OI)(CI)F" /T
icacls "C:\inetpub\wwwroot\MarsAdmin" /grant "IIS AppPool\MarsAdmin:(OI)(CI)F" /T
```

### **Staging Deployment:**

```bash
# 1. Publish
dotnet publish -c Release -o ./publish

# 2. Rename staging config
Rename-Item -Path ".\publish\web.Staging.config" -NewName "web.config"

# 3. Copy to server
Copy-Item -Path ".\publish\*" -Destination "C:\inetpub\wwwroot\MarsAdminStaging" -Recurse
```

## ✅ Verification Checklist

- [ ] Application pool created and configured
- [ ] Website created and bound to application pool
- [ ] Files copied to correct directory
- [ ] Permissions set correctly
- [ ] Database connection working
- [ ] Logs directory created and accessible
- [ ] HTTPS certificate installed (if using HTTPS)
- [ ] Application accessible via browser
- [ ] Logging working (check logs directory and database)

## 📞 Support

If you encounter issues:

1. Check IIS logs: `C:\inetpub\logs\LogFiles\W3SVC1\`
2. Check application logs: `C:\inetpub\wwwroot\MarsAdmin\logs\`
3. Check SQL Server logs: `MarsAdmin{Environment}.dbo.Logs` table
4. Verify web.config syntax
5. Check application pool and website configuration
