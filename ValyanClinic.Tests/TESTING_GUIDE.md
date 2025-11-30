# 🧪 ValyanClinic - bUnit Testing Guide

## Overview

This guide provides patterns and examples for testing Blazor components using **bUnit** in the ValyanClinic project.

---

## 📦 Test Infrastructure

### Base Classes

#### `ComponentTestBase`
Base class for all component tests. Provides:
- ✅ **MediatR** mock
- ✅ **Authentication** mock (TestAuthorizationContext)
- ✅ **JSInterop** mocks (LocalStorage, console.log)
- ✅ **IMC Calculator** service mock
- ✅ **Draft Storage** service mock
- ✅ **Helper methods** (`RenderWithAuth`, `WaitForAsync`)

**Location:** `ValyanClinic.Tests/Infrastructure/ComponentTestBase.cs`

---

## 🎯 Test Patterns

### Pattern 1: Simple Render Test

```csharp
[Fact(DisplayName = "Component - When rendered - Should contain expected elements")]
public void Component_WhenRendered_ShouldContainExpectedElements()
{
    // Arrange & Act
    var cut = RenderComponent<MyComponent>(parameters => parameters
        .Add(p => p.MyProperty, "test value"));
    
    // Assert
    var element = cut.Find(".my-element");
    element.Should().NotBeNull();
    element.TextContent.Should().Contain("expected text");
}
```

### Pattern 2: Event Handler Test

```csharp
[Fact(DisplayName = "Button click - Should invoke callback")]
public void ButtonClick_ShouldInvokeCallback()
{
    // Arrange
    var callbackInvoked = false;
    var cut = RenderComponent<MyComponent>(parameters => parameters
        .Add(p => p.OnClick, EventCallback.Factory.Create(this, () => callbackInvoked = true)));
    
    // Act
    var button = cut.Find("button.my-button");
    button.Click();
    
    // Assert
    callbackInvoked.Should().BeTrue();
}
```

### Pattern 3: Markup-Based Property Testing

**❌ DON'T** - Access private properties:
```csharp
// This will NOT work if properties are private!
cut.Instance.MyPrivateProperty.Should().Be("value");
```

**✅ DO** - Test via markup:
```csharp
// Test the rendered output instead
var input = cut.Find("input[name='my-field']");
input.GetAttribute("value").Should().Be("expected value");
```

### Pattern 4: Async Operations

```csharp
[Fact(DisplayName = "Async operation - Should complete successfully")]
public async Task AsyncOperation_ShouldCompleteSuccessfully()
{
    // Arrange
    var cut = RenderComponent<MyComponent>();
    
    // Act
    await cut.Instance.MyAsyncMethod();
    await WaitForAsync(cut); // Wait for re-render
    
    // Assert
    var result = cut.Find(".result-element");
    result.TextContent.Should().Contain("success");
}
```

### Pattern 5: MediatR Integration Test

```csharp
[Fact(DisplayName = "Save button - Should call MediatR Send")]
public async Task SaveButton_ShouldCallMediatorSend()
{
    // Arrange
    var testId = Guid.NewGuid();
    MockMediator
        .Setup(m => m.Send(It.IsAny<MyCommand>(), default))
        .ReturnsAsync(Result<Guid>.Success(testId));
    
    var cut = RenderComponent<MyComponent>();
    
    // Act
    var saveButton = cut.Find("button.btn-save");
    saveButton.Click();
    await WaitForAsync(cut);
    
    // Assert
    MockMediator.Verify(m => m.Send(
        It.IsAny<MyCommand>(), 
        default), Times.Once);
}
```

---

## 🛠️ Common Services

### Mock MediatR

```csharp
MockMediator.Setup(m => m.Send(It.IsAny<MyCommand>(), default))
    .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));
```

### Mock IMC Calculator

```csharp
MockIMCCalculator.Setup(x => x.Calculate(It.IsAny<decimal>(), It.IsAny<decimal>()))
    .Returns(new IMCResult 
    { 
        Value = 24.5m, 
        Category = IMCCategory.Normal 
    });
```

### JSInterop Verification

```csharp
// Verify LocalStorage was called
JSInterop.VerifyInvoke("localStorage.setItem", times: 1);
```

---

## ⚠️ Known Limitations

### Dispatcher Thread Issues

**Problem:** `StateHasChanged()` called from async methods may throw:
```
The current thread is not associated with the Dispatcher
```

**Workaround:** Wrap async operations in `InvokeAsync()`:
```csharp
await cut.InvokeAsync(async () => await cut.Instance.MyAsyncMethod());
```

**Or:** Test only the synchronous parts and mock async dependencies.

---

## 📊 Test Coverage Goals

| Component Type | Target Coverage |
|----------------|----------------|
| **Modals** | ~85% UI logic |
| **Pages** | ~80% interactions |
| **Shared Components** | ~90% logic |

---

## 🎯 Best Practices

1. ✅ **Use descriptive test names** - `[Fact(DisplayName = "...")]`
2. ✅ **Follow AAA pattern** - Arrange, Act, Assert
3. ✅ **Test via markup** - Don't access private properties
4. ✅ **Use FluentAssertions** - Readable assertions
5. ✅ **Mock external dependencies** - MediatR, services
6. ✅ **Keep tests isolated** - Each test is independent
7. ✅ **Test user interactions** - Clicks, inputs, events
8. ❌ **Don't test framework code** - Only test YOUR logic

---

## 📚 Resources

- **bUnit Documentation:** https://bunit.dev/
- **FluentAssertions:** https://fluentassertions.com/
- **xUnit:** https://xunit.net/

---

**Status:** Foundation Complete  
**Date:** 2025-11-30  
**Version:** 1.0  
**Coverage:** ConsultatieModal - 33% (4/12 tests passing)
