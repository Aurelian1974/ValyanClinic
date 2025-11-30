using Bunit;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Bunit.TestDoubles;
using ValyanClinic.Application.Services.IMC;
using ValyanClinic.Application.Services.Draft;
using ValyanClinic.Infrastructure.Services.DraftStorage;

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
    /// Mock IMC Calculator service
    /// </summary>
    protected Mock<IIMCCalculatorService> MockIMCCalculator { get; }
    
    /// <summary>
    /// Mock authentication state for testing authorized components
    /// </summary>
    protected TestAuthorizationContext AuthContext { get; }
    
    protected ComponentTestBase()
    {
        // Setup MediatR mock
        MockMediator = new Mock<IMediator>();
        Services.AddSingleton(MockMediator.Object);
        
        // Setup IMC Calculator mock
        MockIMCCalculator = new Mock<IIMCCalculatorService>();
        MockIMCCalculator.Setup(x => x.Calculate(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(new IMCResult 
            { 
                Value = 24.5m, 
                Category = IMCCategory.Normal, 
                Interpretation = "Normal" 
            });
        MockIMCCalculator.Setup(x => x.AreValuesValid(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(true);
        Services.AddSingleton(MockIMCCalculator.Object);
        
        // Setup Draft Storage Service mock (generic for CreateConsultatieCommand)
        var mockDraftService = new Mock<IDraftStorageService<Application.Features.ConsultatieManagement.Commands.CreateConsultatie.CreateConsultatieCommand>>();
        mockDraftService.Setup(x => x.LoadDraftAsync(It.IsAny<Guid>()))
            .ReturnsAsync(DraftResult<Application.Features.ConsultatieManagement.Commands.CreateConsultatie.CreateConsultatieCommand>.NotFound);
        mockDraftService.Setup(x => x.SaveDraftAsync(It.IsAny<Guid>(), It.IsAny<Application.Features.ConsultatieManagement.Commands.CreateConsultatie.CreateConsultatieCommand>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mockDraftService.Setup(x => x.ClearDraftAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        mockDraftService.Setup(x => x.GetLastSaveTimeAsync(It.IsAny<Guid>()))
            .ReturnsAsync((DateTime?)null);
        Services.AddSingleton(mockDraftService.Object);
        
        // Setup DraftAutoSaveHelper mock
        var mockLogger = MockLogger<DraftAutoSaveHelper<Application.Features.ConsultatieManagement.Commands.CreateConsultatie.CreateConsultatieCommand>>();
        var draftHelper = new DraftAutoSaveHelper<Application.Features.ConsultatieManagement.Commands.CreateConsultatie.CreateConsultatieCommand>(mockLogger.Object);
        Services.AddSingleton(draftHelper);
        
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
