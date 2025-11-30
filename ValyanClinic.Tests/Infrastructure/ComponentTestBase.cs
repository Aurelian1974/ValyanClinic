using Bunit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Bunit.TestDoubles;

namespace ValyanClinic.Tests.Infrastructure;

/// <summary>
/// Base class for all Blazor component tests using bUnit.
/// Provides common setup for services, authentication, and JSInterop.
/// </summary>
public abstract class ComponentTestBase : TestContext
{
    /// <summary>
    /// Mock MediatR instance for testing command/query patterns
    /// </summary>
    protected Mock<IMediator> MockMediator { get; }
    
    /// <summary>
    /// Mock authentication state for testing authorized components
    /// </summary>
    protected TestAuthorizationContext AuthContext { get; }
    
    protected ComponentTestBase()
    {
        // Setup MediatR mock
        MockMediator = new Mock<IMediator>();
        Services.AddSingleton(MockMediator.Object);
        
        // Setup authentication
        AuthContext = this.AddTestAuthorization();
        
        // Setup common services
        SetupCommonServices();
        
        // Setup JSInterop mocks
        SetupJSInterop();
    }
    
    /// <summary>
    /// Creates a mock logger for a specific type
    /// </summary>
    protected Mock<ILogger<T>> MockLogger<T>() => new Mock<ILogger<T>>();
    
    /// <summary>
    /// Renders a component with authentication setup
    /// </summary>
    protected IRenderedComponent<T> RenderWithAuth<T>(params ComponentParameter[] parameters) 
        where T : IComponent
    {
        AuthContext.SetAuthorized("TestUser");
        AuthContext.SetRoles("Admin", "Medic");
        return RenderComponent<T>(parameters);
    }
    
    /// <summary>
    /// Renders a component with specific user roles
    /// </summary>
    protected IRenderedComponent<T> RenderWithRoles<T>(string[] roles, params ComponentParameter[] parameters) 
        where T : IComponent
    {
        AuthContext.SetAuthorized("TestUser");
        AuthContext.SetRoles(roles);
        return RenderComponent<T>(parameters);
    }
    
    private void SetupCommonServices()
    {
        // Add logging
        Services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        
        // Add other common services as needed
        // Services.AddSingleton<IMyService, MyServiceMock>();
    }
    
    private void SetupJSInterop()
    {
        // Mock console.log (common in Blazor components)
        JSInterop.SetupVoid("console.log", _ => true);
        JSInterop.SetupVoid("console.error", _ => true);
        JSInterop.SetupVoid("console.warn", _ => true);
        
        // Mock LocalStorage operations
        JSInterop.Setup<string>("localStorage.getItem", _ => true)
            .SetResult(string.Empty);
        JSInterop.SetupVoid("localStorage.setItem", _ => true);
        JSInterop.SetupVoid("localStorage.removeItem", _ => true);
        
        // Mock Blazor focus operations
        JSInterop.SetupVoid("BlazorFocusElement", _ => true);
    }
    
    /// <summary>
    /// Waits for async operations to complete in the component
    /// </summary>
    protected async Task WaitForAsync(IRenderedFragment component)
    {
        await component.InvokeAsync(() => { });
    }
}

/// <summary>
/// Null logger implementation for testing (no-op logging)
/// </summary>
file class NullLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => default!;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
        Exception? exception, Func<TState, Exception?, string> formatter) { }
}
