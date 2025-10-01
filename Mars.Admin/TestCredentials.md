# Test Credentials

This document contains the test user credentials for the Mars Admin role-based access control system.

## 🔐 User Accounts

All test users use the password: **`12345Si!`**

| Role                 | Email                            | Password   | Permissions                |
| -------------------- | -------------------------------- | ---------- | -------------------------- |
| **SuperAdmin**       | `superadmin@safelyinsured.co.uk` | `12345Si!` | All Individual Permissions |
| **Developer**        | `developer@safelyinsured.co.uk`  | `12345Si!` | All Individual Permissions |
| **Manager**          | `manager@safelyinsured.co.uk`    | `12345Si!` | Quotes.Read, Policies.Read |
| **Customer Service** | `support@safelyinsured.co.uk`    | `12345Si!` | Quotes.Read, Policies.Read |
| **Account**          | `account@safelyinsured.co.uk`    | `12345Si!` | Quotes.Read, Policies.Read |
| **Viewer**           | `viewer@safelyinsured.co.uk`     | `12345Si!` | Quotes.Read, Policies.Read |
| **Page Editor**      | `editor@safelyinsured.co.uk`     | `12345Si!` | None (from this set)       |

## 🌐 Website Access

All users have access to the following websites:

- **ID**: InsureLearnerDriver (https://www.InsureLearnerDriver.co.uk)
- **ILD**: InsureLearnerDriver (https://www.InsureLearnerDriver.co.uk)
- **SI**: SafelyInsured (https://www.SafelyInsured.co.uk)

## 🔒 IP Safe Listing

### Office IPs (Global Access)

- `127.0.0.1` - Local Development
- `::1` - Local Development IPv6
- `192.168.1.0/24` - Office Network

### Individual IPs

- Set per user as needed through the admin interface

## 🚀 Getting Started

1. **Run the application**: `dotnet run` in the Mars.Admin directory
2. **Navigate to**: `https://localhost:5001` (or your configured port)
3. **Login** with any of the credentials above
4. **Test features**:
   - Role-based access to admin pages
   - Permission-based UI elements
   - IP safe listing (try from different IPs)
   - Website scope filtering

## 📋 Available Permissions

- **User.Create**: Create new users
- **User.Read**: View user information
- **User.Update**: Modify user details
- **User.Delete**: Remove users
- **Quotes.Read**: Access quotes data
- **Policies.Read**: Access policies data
- **AccessLogs.Create**: Create access log entries
- **AccessLogs.Read**: View unauthorized access logs
- **AccessLogs.Update**: Modify access log entries
- **AccessLogs.Delete**: Delete access log entries

## 🛡️ Security Features

- **IP Safe Listing**: Two-tier protection (Office + Individual)
- **Role-Based Access**: Granular permission system
- **Website Scoping**: Per-user website access control
- **Audit Logging**: Track all system changes
- **Automatic Redirects**: Unauthorized access redirects to https://www.SafelyInsured.co.uk

## 📝 Notes

- All test users are created automatically on application startup
- Passwords can be changed through the user management interface
- IP safe listings can be managed through the admin interface
- Role permissions can be modified through the roles management page

---

_Generated for Mars Admin RBAC System_
