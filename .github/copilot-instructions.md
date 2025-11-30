# ValyanClinic Project Instructions

## Architecture
- Clean Architecture: Domain, Application, Infrastructure, and Presentation layers
- ValyanClinic (Blazor Server) is the presentation layer
- Follow SOLID principles and dependency injection patterns

## Technology Stack
- .NET 9 Blazor Server application
- Use async/await for all I/O operations
- Prefer record types for DTOs and immutable data structures

## Blazor Best Practices
- Use code-behind files (.razor.cs) for component logic
- Keep .razor files focused on markup
- Use dependency injection for services
- Implement proper component lifecycle methods (OnInitializedAsync, etc.)

## Authentication & Security
- Authentication is handled via AuthenticationController
- Session cookies for state management
- Always validate user input and sanitize data

## Code Style
- Use C# 12+ features where appropriate
- Follow existing naming conventions in the codebase
- Keep methods focused and concise
- Add XML documentation for public APIs

## Testing
- Write unit tests in ValyanClinic.Tests project
- Mock dependencies using standard patterns
- Test business logic thoroughly
