# Development Documentation

This folder contains technical documentation for developers working on the ValyanClinic project.

## Structure

- **Architecture** - System architecture diagrams and explanations
- **API Documentation** - REST API endpoints and specifications
- **Database** - Database schema, migrations, and data models
- **Coding Standards** - Code style guidelines and best practices
- **Setup Guides** - Development environment setup instructions
- **Testing** - Unit tests, integration tests documentation
- **Deployment** - Deployment procedures and configurations

## Available Documentation

### 🔐 Authentication and Security
- **[Login Page Technical Documentation](Login-Page-Technical-Documentation.md)** - Complete technical analysis of the authentication system

### 👥 User Management System
- **[Users Management Page Technical Documentation](Users-Management-Page-Technical-Documentation.md)** - Main user management interface with Syncfusion DataGrid
- **[Add/Edit User Form Technical Documentation](Add-Edit-User-Form-Technical-Documentation.md)** - Comprehensive form component analysis
- **[User Details View Technical Documentation](User-Details-View-Technical-Documentation.md)** - Premium dashboard view component

### 🗄️ Database and Data Access
- **[Database Configuration with Dapper Technical Documentation](Database-Configuration-Dapper-Technical-Documentation.md)** - Complete database setup, connection management, and Dapper integration

## Technical Stack Overview

### Core Technologies
- **Framework**: .NET 9 with Blazor Server
- **UI Components**: Syncfusion Blazor Enterprise Suite
- **Database**: SQL Server with Dapper ORM
- **Architecture**: Clean Architecture with Domain-Driven Design
- **Language Support**: Romanian localization with full diacritics

### Key Features
- **Interactive Server Rendering** - Real-time UI updates
- **Enterprise Components** - Professional Syncfusion controls
- **Security-First Design** - Server-side processing and validation
- **Premium Medical UI** - Professional healthcare application design
- **Romanian Localization** - Complete UTF-8 diacritics support

## Development Environment Setup

### Prerequisites
- **Visual Studio 2022** (17.8 or later)
- **.NET 9 SDK** installed
- **SQL Server** (2019 or later)
- **Syncfusion License** (configured in appsettings.json)

### Database Configuration
- **Server**: TS1828\ERP
- **Database**: ValyanMed
- **Authentication**: Windows Authentication
- **ORM**: Dapper for high-performance data access

### Project Structure
```
ValyanClinic/                 # Main Blazor Server application
├── Components/
│   ├── Pages/               # Razor pages and components
│   ├── Layout/             # Layout components
│   └── Shared/             # Shared UI components
├── wwwroot/                # Static assets (CSS, JS, images)
└── Program.cs              # Application configuration

ValyanClinic.Domain/         # Domain models and interfaces
ValyanClinic.Application/    # Business logic and services  
ValyanClinic.Infrastructure/ # Data access with Dapper
DevSupport/                 # Documentation and development tools
```

## Guidelines

- All documentation should be in **Markdown format** (.md)
- Use **English** for technical documentation
- Keep documentation **up-to-date** with code changes
- Include **code examples** where appropriate
- Follow **consistent formatting** and structure

## Code Quality Standards

### C# Coding Standards
- **C# 13.0** language features
- **Nullable reference types** enabled
- **Async/await patterns** for all I/O operations
- **Romanian comments** for business logic
- **English comments** for technical implementation

### Blazor Best Practices
- **Interactive Server** rendering mode
- **Component lifecycle** management
- **State management** with proper disposal
- **Event handling** with async patterns
- **CSS isolation** for component styles

### Database Best Practices
- **Dapper** for data access operations
- **Parameterized queries** for security
- **Async methods** for all database operations
- **Connection management** via dependency injection
- **UTF-8 encoding** for Romanian characters

## Security Guidelines

### Authentication & Authorization
- **Server-side validation** only
- **Windows Authentication** for database
- **Role-based access control**
- **Audit logging** for sensitive operations

### Data Protection
- **Connection strings** never exposed to client
- **Sensitive data encryption** in database
- **Input sanitization** and validation
- **SQL injection prevention** via parameterization

## Performance Considerations

### Blazor Server Optimization
- **Efficient state management**
- **Proper component disposal**
- **Minimal SignalR traffic**
- **Client-side caching** where appropriate

### Database Performance
- **Connection pooling** configuration
- **Query optimization** with indexes
- **Batch operations** for bulk data
- **Async patterns** throughout

## Testing Strategy

### Unit Testing
- **xUnit** testing framework
- **Moq** for mocking dependencies
- **FluentAssertions** for readable tests
- **Test coverage** reporting

### Integration Testing
- **In-memory database** for testing
- **TestServer** for Blazor components
- **Authentication mocking** for secure endpoints
- **Romanian text validation**

## Deployment Guidelines

### Environment Configuration
- **Development**: Local SQL Server, detailed logging
- **Production**: TS1828\ERP server, optimized settings
- **Connection string** security
- **Health checks** configuration

### Build and Deployment
- **CI/CD pipelines** configuration
- **Database migration** scripts
- **Static asset optimization**
- **Environment-specific** app settings

---

*This documentation is maintained by the ValyanMed development team and should be updated with any architectural or implementation changes.*
