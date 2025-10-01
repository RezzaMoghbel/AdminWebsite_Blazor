# Serilog Configuration Guide for Mars Admin

## 📋 Overview

This document provides a comprehensive guide to the Serilog logging configuration implemented in the Mars Admin application. Serilog is a structured logging framework that provides powerful logging capabilities with multiple output destinations.

## 🎯 What is Serilog?

Serilog is a diagnostic logging library for .NET applications that provides:

- **Structured Logging**: Logs data as structured objects, not just strings
- **Multiple Sinks**: Output to console, files, databases, and more
- **Rich Formatting**: Flexible output templates and formatting
- **Performance**: High-performance async logging
- **Configuration**: JSON-based configuration support

## 🏗️ Architecture

### Log Flow

```
Application Code → Serilog → Multiple Sinks
                              ├── Console (Development)
                              ├── File (mars-admin-.log)
                              ├── Security File (mars-admin-security-.log)
                              └── SQL Server Database (Logs table)
```

### Log Levels Hierarchy

```
Verbose (0) < Debug (1) < Information (2) < Warning (3) < Error (4) < Fatal (5)
```

## 📁 File Structure

### Log Files Created

```
/logs/
├── mars-admin-2024-01-15.log          # General application logs
├── mars-admin-security-2024-01-15.log # Security-related logs (Warning+)
└── (Database: Logs table)             # Structured logs in SQL Server
```

## ⚙️ Configuration Breakdown

### 1. Program.cs Configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)  // Read from appsettings.json
    .Enrich.FromLogContext()                        // Add contextual properties
    .Enrich.WithProperty("Application", "Mars.Admin") // Add application name
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName) // Add environment
    .WriteTo.Console()                              // Console output
    .WriteTo.File(...)                             // File output
    .CreateLogger();
```

**Key Components:**

- **ReadFrom.Configuration**: Loads settings from appsettings.json
- **Enrich.FromLogContext**: Adds contextual information to logs
- **Enrich.WithProperty**: Adds custom properties to all log entries
- **WriteTo.Console**: Outputs to console (development)
- **WriteTo.File**: Outputs to rotating log files

### 2. appsettings.json Configuration

#### Serilog Root Section

```json
"Serilog": {
  "Using": [...],           // Required sink assemblies
  "MinimumLevel": {...},    // Log level configuration
  "WriteTo": [...],         // Output destinations
  "Enrich": [...]          // Log enrichment
}
```

#### Using Section

```json
"Using": [
  "Serilog.Sinks.Console",    // Console output sink
  "Serilog.Sinks.File",       // File output sink
  "Serilog.Sinks.MSSqlServer" // SQL Server sink
]
```

**Purpose**: Declares which Serilog sink assemblies are required for the configuration.

#### MinimumLevel Section

```json
"MinimumLevel": {
  "Default": "Information",    // Default log level
  "Override": {               // Category-specific overrides
    "Microsoft": "Warning",
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore": "Warning",
    "System": "Warning"
  }
}
```

**Explanation:**

- **Default**: Minimum log level for all categories
- **Override**: Specific log levels for different namespaces
- **Microsoft**: Reduces Microsoft framework noise to Warning level
- **Microsoft.AspNetCore**: Reduces ASP.NET Core noise
- **Microsoft.EntityFrameworkCore**: Reduces EF Core query noise
- **System**: Reduces system-level noise

#### WriteTo Section - Console Sink

```json
{
  "Name": "Console",
  "Args": {
    "outputTemplate": "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
  }
}
```

**Template Breakdown:**

- `{Timestamp:HH:mm:ss}`: Time in HH:mm:ss format
- `[{Level:u3}]`: Log level in 3-character format (INF, WRN, ERR)
- `{Message:lj}`: Log message, left-justified
- `{Properties:j}`: Structured properties as JSON
- `{NewLine}`: Line break
- `{Exception}`: Exception details if present

#### WriteTo Section - General File Sink

```json
{
  "Name": "File",
  "Args": {
    "path": "logs/mars-admin-.log",
    "rollingInterval": "Day",
    "retainedFileCountLimit": 30,
    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
  }
}
```

**Parameters:**

- **path**: File path with date placeholder (`-` becomes date)
- **rollingInterval**: How often to create new files (Day, Hour, etc.)
- **retainedFileCountLimit**: How many old files to keep (30 = 30 days)
- **outputTemplate**: Format for log entries

#### WriteTo Section - Security File Sink

```json
{
  "Name": "File",
  "Args": {
    "path": "logs/mars-admin-security-.log",
    "rollingInterval": "Day",
    "retainedFileCountLimit": 30,
    "restrictedToMinimumLevel": "Warning",
    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
  }
}
```

**Key Differences:**

- **restrictedToMinimumLevel**: Only logs Warning level and above
- **path**: Separate file for security events
- **Purpose**: Dedicated security monitoring

#### WriteTo Section - SQL Server Sink

```json
{
  "Name": "MSSqlServer",
  "Args": {
    "connectionString": "Server=...;Database=...",
    "sinkOptionsSection": {
      "tableName": "Logs",
      "schemaName": "dbo",
      "autoCreateSqlTable": true,
      "batchPostingLimit": 50,
      "period": "00:00:05"
    },
    "restrictedToMinimumLevel": "Warning",
    "columnOptionsSection": {
      "timeStamp": {
        "columnName": "Timestamp",
        "convertToUtc": true
      },
      "level": { "columnName": "Level" },
      "message": { "columnName": "Message" },
      "messageTemplate": { "columnName": "MessageTemplate" },
      "exception": { "columnName": "Exception" },
      "properties": { "columnName": "Properties" }
    }
  }
}
```

**SQL Server Parameters:**

- **connectionString**: Database connection
- **tableName**: Target table name
- **schemaName**: Database schema
- **autoCreateSqlTable**: Creates table if it doesn't exist
- **batchPostingLimit**: Number of logs to batch before writing
- **period**: How often to flush logs to database
- **restrictedToMinimumLevel**: Only Warning+ logs to database
- **columnOptionsSection**: Maps log properties to database columns

#### Enrich Section

```json
"Enrich": [
  "FromLogContext",    // Adds contextual properties
  "WithMachineName",   // Adds machine name
  "WithThreadId"       // Adds thread ID
]
```

**Enrichers:**

- **FromLogContext**: Adds properties set in code
- **WithMachineName**: Adds server machine name
- **WithThreadId**: Adds thread identifier

## 🔧 Environment-Specific Configurations

### Development (appsettings.json)

- **Console Output**: Enabled for immediate feedback
- **File Retention**: 30 days
- **Database Logging**: All levels
- **Verbose Logging**: More detailed information

### Production (appsettings.Production.json)

- **Console Output**: Disabled (performance)
- **File Retention**: 90 days general, 365 days security
- **Database Logging**: Warning+ only (performance)
- **Batch Processing**: Optimized for high volume
- **Performance**: Reduced verbosity

## 📊 Log Categories in Mars Admin

### Security Events (Warning Level)

- IP safe listing violations
- Unauthorized access attempts
- Authentication failures
- Permission denials

### Application Events (Information Level)

- User logins/logouts
- Database operations
- Middleware execution
- System startup/shutdown

### Error Events (Error Level)

- Database connection failures
- Middleware exceptions
- Application crashes
- Configuration errors

## 🎯 Structured Logging Examples

### IP Safe Listing Log

```json
{
  "Timestamp": "2024-01-15T10:30:45.123Z",
  "Level": "Warning",
  "Message": "IP 192.168.1.100 is not in safe listing table - denying access to /admin/users from Mozilla/5.0...",
  "Properties": {
    "ClientIP": "192.168.1.100",
    "RequestPath": "/admin/users",
    "UserAgent": "Mozilla/5.0...",
    "Application": "Mars.Admin",
    "Environment": "Production"
  }
}
```

### Database Error Log

```json
{
  "Timestamp": "2024-01-15T10:30:45.123Z",
  "Level": "Error",
  "Message": "Database connection failed",
  "Exception": "SqlException: Login failed for user 'MarsAdminUser'",
  "Properties": {
    "Application": "Mars.Admin",
    "Environment": "Production",
    "MachineName": "WEB-SERVER-01"
  }
}
```

## 🚀 Usage in Code

### Basic Logging

```csharp
_logger.LogInformation("User {UserId} logged in successfully", userId);
_logger.LogWarning("IP {ClientIP} blocked from accessing {Path}", ip, path);
_logger.LogError(ex, "Database operation failed for user {UserId}", userId);
```

### Structured Logging with Properties

```csharp
using (LogContext.PushProperty("UserId", userId))
using (LogContext.PushProperty("Action", "Login"))
{
    _logger.LogInformation("User authentication attempt");
    // All logs in this scope will include UserId and Action properties
}
```

## 📈 Performance Considerations

### File Logging

- **Async Writing**: Non-blocking file operations
- **Buffering**: Logs are buffered before writing
- **Rolling**: Automatic file rotation prevents large files

### Database Logging

- **Batch Processing**: Multiple logs written together
- **Level Filtering**: Only important logs go to database
- **Connection Pooling**: Efficient database connections

### Memory Usage

- **Structured Data**: More memory efficient than string concatenation
- **Property Caching**: Repeated properties are cached
- **Garbage Collection**: Optimized to reduce GC pressure

## 🔍 Troubleshooting

### Common Issues

#### 1. Log Files Not Created

**Symptoms**: No log files in /logs directory
**Causes**:

- Insufficient permissions
- Invalid path configuration
- Application not running

**Solutions**:

- Check folder permissions
- Verify path in configuration
- Ensure application is running

#### 2. Database Logging Not Working

**Symptoms**: No entries in Logs table
**Causes**:

- Database connection issues
- Table creation failures
- Permission problems

**Solutions**:

- Test database connection
- Check SQL Server permissions
- Verify table creation

#### 3. High Disk Usage

**Symptoms**: Large log files consuming disk space
**Causes**:

- Too verbose logging
- No log rotation
- Long retention period

**Solutions**:

- Adjust log levels
- Enable log rotation
- Reduce retention period

### Debug Commands

#### Check Log Configuration

```bash
# View current log configuration
dotnet run --verbosity normal
```

#### Test Logging

```bash
# Access debug page
https://localhost:5001/debug-ip-logging
```

## 📋 Maintenance Tasks

### Daily

- Monitor log file sizes
- Check for error patterns
- Review security logs

### Weekly

- Archive old log files
- Analyze performance metrics
- Review database log table size

### Monthly

- Clean up old log files
- Optimize database indexes
- Review log retention policies

## 🔐 Security Considerations

### Log File Security

- **Permissions**: Restrict access to log files
- **Encryption**: Consider encrypting sensitive logs
- **Retention**: Follow data retention policies

### Database Security

- **Access Control**: Limit database log access
- **Sensitive Data**: Avoid logging passwords/tokens
- **Audit Trail**: Track who accesses logs

### Information Disclosure

- **Sanitization**: Remove sensitive data from logs
- **PII Protection**: Avoid logging personal information
- **Error Details**: Don't expose internal details

## 📚 Additional Resources

### Serilog Documentation

- [Serilog GitHub](https://github.com/serilog/serilog)
- [Serilog Configuration](https://github.com/serilog/serilog-settings-configuration)
- [Serilog Sinks](https://github.com/serilog/serilog/wiki/Provided-Sinks)

### Best Practices

- Use structured logging consistently
- Include relevant context in logs
- Set appropriate log levels
- Monitor log performance impact
- Implement log rotation and cleanup

---

_This guide covers the complete Serilog implementation in Mars Admin. For specific questions or customizations, refer to the Serilog documentation or contact the development team._
