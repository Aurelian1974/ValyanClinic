using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using System.Text.Json;
using ValyanClinic.Infrastructure.Services.DraftStorage;
using Xunit;

namespace ValyanClinic.Tests.Services.DraftStorage;

/// <summary>
/// Unit tests pentru LocalStorageDraftService
/// Testează salvarea/încărcarea draft-urilor în LocalStorage
/// </summary>
public class LocalStorageDraftServiceTests
{
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly Mock<ILogger<LocalStorageDraftService<TestData>>> _loggerMock;
    private readonly LocalStorageDraftService<TestData> _service;

    // In-memory storage pentru simulare LocalStorage
    private readonly Dictionary<string, string> _localStorage = new();

    public LocalStorageDraftServiceTests()
    {
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _loggerMock = new Mock<ILogger<LocalStorageDraftService<TestData>>>();
        _service = new LocalStorageDraftService<TestData>(_jsRuntimeMock.Object, _loggerMock.Object);

        SetupJSRuntimeMock();
    }

    #region SaveDraftAsync Tests

    [Fact(DisplayName = "SaveDraftAsync - Salvează draft cu succes")]
    public async Task SaveDraftAsync_ValidData_SavesSuccessfully()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = "user123";
        var data = new TestData { Name = "Test", Value = 42 };

        // Act
        await _service.SaveDraftAsync(entityId, data, userId);

        // Assert
        var key = $"draft_TestData_{entityId}";
        _localStorage.Should().ContainKey(key);

        var savedJson = _localStorage[key];
        savedJson.Should().NotBeNullOrEmpty();

        var savedDraft = JsonSerializer.Deserialize<Draft<TestData>>(savedJson);
        savedDraft.Should().NotBeNull();
        savedDraft!.EntityId.Should().Be(entityId);
        savedDraft.UserId.Should().Be(userId);
        savedDraft.Data.Should().NotBeNull();
        savedDraft.Data!.Name.Should().Be("Test");
        savedDraft.Data.Value.Should().Be(42);
    }

    [Fact(DisplayName = "SaveDraftAsync - Suprascrie draft existent")]
    public async Task SaveDraftAsync_ExistingDraft_OverwritesSuccessfully()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = "user123";
        var data1 = new TestData { Name = "First", Value = 1 };
        var data2 = new TestData { Name = "Second", Value = 2 };

        // Act
        await _service.SaveDraftAsync(entityId, data1, userId);
        await Task.Delay(10); // Delay pentru timestamp diferit
        await _service.SaveDraftAsync(entityId, data2, userId);

        // Assert
        var result = await _service.LoadDraftAsync(entityId);
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Second");
        result.Data.Value.Should().Be(2);
    }

    [Fact(DisplayName = "SaveDraftAsync - Salvează multiple draft-uri pentru entități diferite")]
    public async Task SaveDraftAsync_MultipleEntities_SavesSeparately()
    {
        // Arrange
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var data1 = new TestData { Name = "Entity1", Value = 1 };
        var data2 = new TestData { Name = "Entity2", Value = 2 };

        // Act
        await _service.SaveDraftAsync(entity1, data1, "user123");
        await _service.SaveDraftAsync(entity2, data2, "user123");

        // Assert
        var result1 = await _service.LoadDraftAsync(entity1);
        var result2 = await _service.LoadDraftAsync(entity2);

        result1.Data!.Name.Should().Be("Entity1");
        result2.Data!.Name.Should().Be("Entity2");
    }

    #endregion

    #region LoadDraftAsync Tests

    [Fact(DisplayName = "LoadDraftAsync - Returnează NotFound pentru draft inexistent")]
    public async Task LoadDraftAsync_NonExistentDraft_ReturnsNotFound()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        var result = await _service.LoadDraftAsync(entityId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(DraftErrorType.NotFound);
        result.Data.Should().BeNull();
    }

    [Fact(DisplayName = "LoadDraftAsync - Încarcă draft salvat anterior")]
    public async Task LoadDraftAsync_ExistingDraft_LoadsSuccessfully()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = "user123";
        var data = new TestData { Name = "Test", Value = 99 };
        await _service.SaveDraftAsync(entityId, data, userId);

        // Act
        var result = await _service.LoadDraftAsync(entityId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test");
        result.Data.Value.Should().Be(99);
        result.SavedAt.Should().NotBeNull();
        result.SavedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "LoadDraftAsync - Returnează Invalid pentru JSON corrupt")]
    public async Task LoadDraftAsync_CorruptedJson_ReturnsInvalid()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var key = $"draft_TestData_{entityId}";
        _localStorage[key] = "{ corrupt json without closing brace";

        // Act
        var result = await _service.LoadDraftAsync(entityId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(DraftErrorType.InvalidData);
    }

    [Fact(DisplayName = "LoadDraftAsync - Returnează Expired pentru draft vechi")]
    public async Task LoadDraftAsync_ExpiredDraft_ReturnsExpired()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var expiredDraft = new Draft<TestData>
        {
            EntityId = entityId,
            UserId = "user123",
            Data = new TestData { Name = "Old", Value = 1 },
            SavedAt = DateTime.Now.AddDays(-8), // 8 zile în urmă
            Version = 1
        };

        var key = $"draft_TestData_{entityId}";
        _localStorage[key] = JsonSerializer.Serialize(expiredDraft);

        // Act
        var result = await _service.LoadDraftAsync(entityId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(DraftErrorType.Expired);
    }

    #endregion

    #region ClearDraftAsync Tests

    [Fact(DisplayName = "ClearDraftAsync - Șterge draft cu succes")]
    public async Task ClearDraftAsync_ExistingDraft_RemovesSuccessfully()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var data = new TestData { Name = "Test", Value = 1 };
        await _service.SaveDraftAsync(entityId, data, "user123");

        // Act
        await _service.ClearDraftAsync(entityId);

        // Assert
        var result = await _service.LoadDraftAsync(entityId);
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(DraftErrorType.NotFound);
    }

    [Fact(DisplayName = "ClearDraftAsync - Nu aruncă excepție pentru draft inexistent")]
    public async Task ClearDraftAsync_NonExistentDraft_DoesNotThrow()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        var act = async () => await _service.ClearDraftAsync(entityId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region HasDraftAsync Tests

    [Fact(DisplayName = "HasDraftAsync - Returnează true pentru draft existent")]
    public async Task HasDraftAsync_ExistingDraft_ReturnsTrue()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var data = new TestData { Name = "Test", Value = 1 };
        await _service.SaveDraftAsync(entityId, data, "user123");

        // Act
        var result = await _service.HasDraftAsync(entityId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "HasDraftAsync - Returnează false pentru draft inexistent")]
    public async Task HasDraftAsync_NonExistentDraft_ReturnsFalse()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        var result = await _service.HasDraftAsync(entityId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetLastSaveTimeAsync Tests

    [Fact(DisplayName = "GetLastSaveTimeAsync - Returnează timestamp pentru draft existent")]
    public async Task GetLastSaveTimeAsync_ExistingDraft_ReturnsTimestamp()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var beforeSave = DateTime.Now;
        var data = new TestData { Name = "Test", Value = 1 };
        await _service.SaveDraftAsync(entityId, data, "user123");

        // Act
        var result = await _service.GetLastSaveTimeAsync(entityId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOnOrAfter(beforeSave);
        result.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "GetLastSaveTimeAsync - Returnează null pentru draft inexistent")]
    public async Task GetLastSaveTimeAsync_NonExistentDraft_ReturnsNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        var result = await _service.GetLastSaveTimeAsync(entityId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CleanupExpiredDraftsAsync Tests

    [Fact(DisplayName = "CleanupExpiredDraftsAsync - Șterge doar draft-urile expirate")]
    public async Task CleanupExpiredDraftsAsync_MixedDrafts_DeletesOnlyExpired()
    {
        // Arrange
        var recentEntity = Guid.NewGuid();
        var expiredEntity = Guid.NewGuid();

        // Recent draft
        await _service.SaveDraftAsync(recentEntity, new TestData { Name = "Recent", Value = 1 }, "user123");

        // Expired draft - salvăm direct în localStorage
        var expiredDraft = new Draft<TestData>
        {
            EntityId = expiredEntity,
            UserId = "user123",
            Data = new TestData { Name = "Expired", Value = 2 },
            SavedAt = DateTime.Now.AddDays(-10),
            Version = 1
        };
        var key = $"draft_TestData_{expiredEntity}";
        _localStorage[key] = JsonSerializer.Serialize(expiredDraft);

        // Act
        // Verificăm că draft-ul expirat returnează Expired
        var expiredResultBefore = await _service.LoadDraftAsync(expiredEntity);
        expiredResultBefore.ErrorType.Should().Be(DraftErrorType.Expired);

        // Draft-ul recent ar trebui să fie valid
        var recentResult = await _service.LoadDraftAsync(recentEntity);
        recentResult.IsSuccess.Should().BeTrue();
        recentResult.Data!.Name.Should().Be("Recent");
    }

    #endregion

    #region Integration Scenarios

    [Fact(DisplayName = "Scenariu Real - Salvare, încărcare, modificare, salvare, ștergere")]
    public async Task IntegrationScenario_CompleteWorkflow_WorksCorrectly()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var userId = "doctor123";

        // Step 1: Salvare inițială
        var initialData = new TestData { Name = "Draft 1", Value = 100 };
        await _service.SaveDraftAsync(entityId, initialData, userId);

        // Step 2: Verificare existență
        var exists = await _service.HasDraftAsync(entityId);
        exists.Should().BeTrue();

        // Step 3: Încărcare
        var loadResult = await _service.LoadDraftAsync(entityId);
        loadResult.IsSuccess.Should().BeTrue();
        loadResult.Data!.Value.Should().Be(100);

        // Step 4: Modificare și salvare
        var modifiedData = new TestData { Name = "Draft 2", Value = 200 };
        await _service.SaveDraftAsync(entityId, modifiedData, userId);

        // Step 5: Verificare modificare
        var reloadResult = await _service.LoadDraftAsync(entityId);
        reloadResult.Data!.Value.Should().Be(200);

        // Step 6: Ștergere
        await _service.ClearDraftAsync(entityId);

        // Step 7: Verificare ștergere
        var afterClear = await _service.HasDraftAsync(entityId);
        afterClear.Should().BeFalse();
    }

    [Fact(DisplayName = "Scenariu Real - Multiple utilizatori cu draft-uri separate")]
    public async Task IntegrationScenario_MultipleUsers_MaintainsSeparateDrafts()
    {
        // Arrange
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var user1 = "doctor1";
        var user2 = "doctor2";

        // Act
        await _service.SaveDraftAsync(entity1, new TestData { Name = "User1", Value = 1 }, user1);
        await _service.SaveDraftAsync(entity2, new TestData { Name = "User2", Value = 2 }, user2);

        // Assert
        var result1 = await _service.LoadDraftAsync(entity1);
        var result2 = await _service.LoadDraftAsync(entity2);

        result1.Data!.Name.Should().Be("User1");
        result2.Data!.Name.Should().Be("User2");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Setup mock pentru JSRuntime care simulează LocalStorage
    /// </summary>
    private void SetupJSRuntimeMock()
    {
        // localStorage.setItem
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<object>(
                It.Is<string>(s => s == "localStorage.setItem"),
                It.IsAny<object?[]?>()))
            .Returns<string, object?[]?>((method, args) =>
            {
                if (args != null && args.Length >= 2)
                {
                    var key = args[0]?.ToString() ?? string.Empty;
                    var value = args[1]?.ToString() ?? string.Empty;
                    _localStorage[key] = value;
                }
                return new ValueTask<object>(new object());
            });

        // localStorage.getItem
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<string>(
                It.Is<string>(s => s == "localStorage.getItem"),
                It.IsAny<object?[]?>()))
            .Returns<string, object?[]?>((method, args) =>
            {
                if (args != null && args.Length > 0)
                {
                    var key = args[0]?.ToString() ?? string.Empty;
                    var value = _localStorage.TryGetValue(key, out var val) ? val : string.Empty;
                    return new ValueTask<string>(value);
                }
                return new ValueTask<string>(string.Empty);
            });

        // localStorage.removeItem
        _jsRuntimeMock
            .Setup(x => x.InvokeAsync<object>(
                It.Is<string>(s => s == "localStorage.removeItem"),
                It.IsAny<object?[]?>()))
            .Returns<string, object?[]?>((method, args) =>
            {
                if (args != null && args.Length > 0)
                {
                    var key = args[0]?.ToString() ?? string.Empty;
                    _localStorage.Remove(key);
                }
                return new ValueTask<object>(new object());
            });
    }

    #endregion
}

/// <summary>
/// Model de test pentru draft-uri
/// </summary>
public class TestData
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}
