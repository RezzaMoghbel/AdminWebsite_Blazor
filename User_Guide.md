# Mars Admin System - User Guide

## Overview

The Mars Admin System is a comprehensive role-based access control (RBAC) platform designed to manage users, roles, permissions, and website access across multiple organizations. This guide will walk you through the complete user journey and system capabilities.

## Getting Started

### 1. User Registration & Login

- **Registration**: New users can register through the `/register` page
- **Login**: Access the system via `/login` with your credentials
- **First Login**: New users are automatically flagged for attention until assigned roles and website access

### 2. Dashboard Overview

After logging in, you'll see the main dashboard with:

- **User Management Alerts**: Real-time notifications about users needing attention
- **Quick Access**: Navigation to all system modules
- **System Status**: Overview of users, roles, permissions, and websites

## User Journey & System Flow

### Phase 1: User Registration & Initial Setup

```
Registration → Login → Dashboard → Alert Notification → Role Assignment → Website Assignment → Complete Setup
```

1. **User Registers**: Creates account with email and password
2. **First Login**: System flags user as "needs attention"
3. **SuperAdmin Notification**: Alert appears on dashboard
4. **Role Assignment**: SuperAdmin assigns appropriate role
5. **Website Assignment**: SuperAdmin grants access to specific websites
6. **Setup Complete**: User can now access assigned features

### Phase 2: Role-Based Access Control

The system operates on three levels of access:

#### **SuperAdmin Role**

- **Full System Access**: Can manage everything
- **User Management**: Create, edit, deactivate users
- **Role Management**: Assign and modify user roles
- **Permission Management**: Control what each role can do
- **Website Management**: Manage website access for users
- **Alert Management**: Handle user attention alerts

#### **Developer Role**

- **Technical Access**: Development and testing capabilities
- **Limited Admin**: Some administrative functions
- **System Monitoring**: Access to logs and system status

#### **Standard Roles** (Viewer, Editor, etc.)

- **Content Access**: Based on assigned permissions
- **Website-Specific**: Limited to assigned websites
- **Feature-Limited**: Access only to permitted features

## Core System Modules

### 1. User Management (`/admin/users`)

**Purpose**: Manage all system users

**Features**:

- View all users with their roles and website assignments
- Create new users
- Edit user details (name, email, role)
- Activate/deactivate user accounts
- Assign roles to users
- View user login history

**User Journey**:

1. Navigate to User Management
2. View user list with current assignments
3. Click "Edit" to modify user details
4. Select appropriate role from dropdown
5. Save changes
6. System automatically updates alerts

### 2. Role Management (`/admin/roles`)

**Purpose**: Define and manage user roles and their permissions

**Features**:

- Create custom roles
- Assign permissions to roles
- Dynamic permission management with real-time updates
- View role hierarchy and capabilities

**User Journey**:

1. Navigate to Role Management
2. View existing roles and their permissions
3. Click "Manage Permissions" for a role
4. **Available Permissions**: List of all system permissions
5. **Assigned Permissions**: Current permissions for the role
6. **Dynamic Movement**: Click to move permissions between lists
7. **Save Confirmation**: Review changes before applying
8. **Unsaved Changes Warning**: System prevents accidental data loss

**Permission Categories**:

- **User Management**: Create, edit, delete users
- **Role Management**: Manage roles and permissions
- **Website Management**: Control website access
- **System Administration**: Advanced system functions
- **Content Management**: Edit and manage content
- **Reporting**: Access to reports and analytics

### 3. Website Management (`/admin/websites`)

**Purpose**: Manage websites and user access to them

**Features**:

- Create and manage websites
- Assign users to websites
- Control website-specific permissions
- Monitor website usage

**User Journey**:

1. Navigate to Website Management
2. View website list with user counts
3. Click "Manage Users" for a website
4. **Available Users**: Users not assigned to this website
5. **Assigned Users**: Users currently assigned to this website
6. **Dynamic Assignment**: Move users between lists
7. **Save Changes**: Apply user assignments
8. **Protection Status**: Websites with users are protected from deletion

### 4. Dashboard Alerts (`/dashboard`)

**Purpose**: Real-time monitoring of users needing attention

**Alert Types**:

#### **Attention Needed**

- **New Users**: Users who haven't been assigned roles/websites
- **Missing Assignments**: Users missing roles or website access
- **Action Required**: Click email to manage user, click links to assign roles/websites

#### **Inactive Users**

- **Long Absence**: Users not logged in for 30+ days
- **Review Required**: Consider deactivating inactive accounts
- **Ignore Option**: Dismiss alerts for users who should remain inactive

#### **Ignored Alerts**

- **Dismissed Notifications**: Alerts that have been ignored
- **Toggle View**: Show/hide ignored alerts
- **Re-activate**: Un-ignore alerts if needed

## Permission System Deep Dive

### How Permissions Work

1. **Role Creation**: SuperAdmin creates roles with specific permissions
2. **User Assignment**: Users are assigned to roles
3. **Website Access**: Users are granted access to specific websites
4. **Feature Access**: Permissions control which features users can access
5. **Dynamic Updates**: Changes take effect immediately

### Permission Categories Explained

#### **User Management Permissions**

- `CreateUser`: Add new users to the system
- `EditUser`: Modify existing user details
- `DeleteUser`: Remove users from the system
- `ViewUsers`: Access user management interface

#### **Role Management Permissions**

- `CreateRole`: Create new user roles
- `EditRole`: Modify role properties
- `DeleteRole`: Remove roles from system
- `ManagePermissions`: Assign permissions to roles

#### **Website Management Permissions**

- `CreateWebsite`: Add new websites
- `EditWebsite`: Modify website properties
- `DeleteWebsite`: Remove websites
- `ManageWebsiteUsers`: Assign users to websites

#### **System Administration Permissions**

- `ViewSystemLogs`: Access system logs
- `ManageSystemSettings`: Modify system configuration
- `BackupDatabase`: Create system backups
- `RestoreDatabase`: Restore from backups

## Best Practices

### For SuperAdmins

1. **Regular Monitoring**: Check dashboard alerts daily
2. **Role Design**: Create roles based on job functions, not individuals
3. **Least Privilege**: Grant minimum necessary permissions
4. **Documentation**: Keep track of role purposes and permissions
5. **Regular Cleanup**: Review and remove inactive users

### For Users

1. **Understand Your Role**: Know what permissions you have
2. **Report Issues**: Contact SuperAdmin for access problems
3. **Secure Practices**: Use strong passwords and log out when done
4. **Stay Active**: Regular login prevents account deactivation

## Troubleshooting Common Issues

### "Access Denied" Errors

- **Check Role**: Ensure you have the correct role assigned
- **Verify Permissions**: Confirm your role has the required permission
- **Website Access**: Ensure you have access to the specific website
- **Contact Admin**: If issues persist, contact your SuperAdmin

### Missing Features

- **Permission Check**: Your role may not have the required permission
- **Website Assignment**: You may not have access to the relevant website
- **Role Update**: Your role may need to be updated with new permissions

### Alert Notifications

- **New User Alerts**: Complete user setup by assigning role and website
- **Inactive User Alerts**: Decide whether to deactivate or ignore
- **Ignored Alerts**: Review periodically to ensure they're still relevant

## System Security Features

### IP Safe Listing

- **Office Networks**: Automatically whitelisted
- **Individual IPs**: SuperAdmin can add specific IPs
- **Security**: Prevents unauthorized access from unknown locations

### User Session Management

- **Automatic Logout**: Sessions expire for security
- **Login Tracking**: System records all login attempts
- **Account Lockout**: Failed login attempts trigger security measures

### Audit Trail

- **Change Tracking**: All modifications are logged
- **User Actions**: System records who did what and when
- **Compliance**: Full audit trail for regulatory requirements

## Getting Help

### Self-Service

1. **Check This Guide**: Most questions are answered here
2. **Dashboard Alerts**: System provides guidance on required actions
3. **Permission System**: Clear indicators of what you can access

### Contact Support

- **SuperAdmin**: For role and permission issues
- **System Administrator**: For technical problems
- **Documentation**: Refer to this guide and system help

## System Updates & Maintenance

### Regular Maintenance

- **Database Updates**: System automatically applies updates
- **Permission Updates**: New permissions may be added
- **Feature Updates**: New capabilities may be introduced

### User Impact

- **Minimal Disruption**: Updates rarely affect user experience
- **Notification**: Users are informed of significant changes
- **Training**: New features include user guidance

---

_This guide is updated regularly to reflect system changes and improvements. For the latest version, check the system documentation or contact your SuperAdmin._
