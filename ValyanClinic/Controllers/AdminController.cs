using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ValyanClinic.Controllers;

/// <summary>
/// Controller pentru operatiuni administrative - inclusiv curatarea log-urilor
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AdminController(ILogger<AdminController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Endpoint pentru curatarea manuala a log-urilor (doar in development)
    /// </summary>
    [HttpPost("cleanup-logs")]
    public async Task<IActionResult> CleanupLogs()
    {
        try
        {
            // Security: Only allow in development environment
            if (!_environment.IsDevelopment())
            {
                return BadRequest(new { message = "This endpoint is only available in development environment" });
            }

            _logger.LogInformation("📊 Log status check requested via API (no cleanup performed)");

            var logsDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
            
            if (!Directory.Exists(logsDirectory))
            {
                return Ok(new { message = "Logs directory does not exist", directory = logsDirectory });
            }

            var logFiles = Directory.GetFiles(logsDirectory, "*.log", SearchOption.AllDirectories);
            var results = new List<object>();

            foreach (var logFile in logFiles)
            {
                var fileName = Path.GetFileName(logFile);
                try
                {
                    var fileInfo = new FileInfo(logFile);
                    
                    results.Add(new 
                    { 
                        file = fileName, 
                        action = "preserved", 
                        size = fileInfo.Length,
                        sizeFormatted = FormatBytes(fileInfo.Length),
                        created = fileInfo.CreationTime,
                        modified = fileInfo.LastWriteTime,
                        status = "success",
                        note = "Log file preserved with all historical data" 
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new 
                    { 
                        file = fileName, 
                        action = "error_reading", 
                        status = "error",
                        error = ex.Message 
                    });
                }
            }

            _logger.LogInformation("✅ Log status check completed via API - all logs preserved");

            return Ok(new 
            { 
                message = "Log status check completed - no files were modified", 
                directory = logsDirectory,
                totalFiles = results.Count,
                totalSize = results.Where(r => r.GetType().GetProperty("size") != null)
                                  .Sum(r => (long)(r.GetType().GetProperty("size")?.GetValue(r) ?? 0L)),
                totalSizeFormatted = FormatBytes(results.Where(r => r.GetType().GetProperty("size") != null)
                                                       .Sum(r => (long)(r.GetType().GetProperty("size")?.GetValue(r) ?? 0L))),
                files = results,
                timestamp = DateTime.Now,
                note = "All log files have been preserved with their complete history for debugging purposes"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during log status check");
            return StatusCode(500, new { message = "Error during log status check", error = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint pentru verificarea status-ului log-urilor
    /// </summary>
    [HttpGet("logs-status")]
    public IActionResult GetLogsStatus()
    {
        try
        {
            var logsDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
            
            if (!Directory.Exists(logsDirectory))
            {
                return Ok(new { message = "Logs directory does not exist", directory = logsDirectory });
            }

            var logFiles = Directory.GetFiles(logsDirectory, "*.log", SearchOption.AllDirectories);
            var fileInfos = logFiles.Select(file => 
            {
                var info = new FileInfo(file);
                return new 
                {
                    name = Path.GetFileName(file),
                    size = info.Length,
                    sizeFormatted = FormatBytes(info.Length),
                    created = info.CreationTime,
                    modified = info.LastWriteTime,
                    fullPath = file
                };
            }).OrderBy(f => f.name).ToArray();

            return Ok(new 
            {
                directory = logsDirectory,
                totalFiles = fileInfos.Length,
                totalSize = fileInfos.Sum(f => f.size),
                totalSizeFormatted = FormatBytes(fileInfos.Sum(f => f.size)),
                files = fileInfos,
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs status");
            return StatusCode(500, new { message = "Error getting logs status", error = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint pentru testarea conexiunii la baza de date si personal repository
    /// </summary>
    [HttpPost("test-database")]
    public async Task<IActionResult> TestDatabase([FromServices] ValyanClinic.Infrastructure.Repositories.IPersonalRepository personalRepository)
    {
        try
        {
            // Security: Only allow in development environment
            if (!_environment.IsDevelopment())
            {
                return BadRequest(new { message = "This endpoint is only available in development environment" });
            }

            _logger.LogInformation("🧪 Manual database test requested via API");

            var testResult = await personalRepository.TestDatabaseConnectionAsync();
            
            return Ok(new 
            { 
                message = "Database test completed", 
                success = testResult,
                timestamp = DateTime.Now,
                note = "Check console output for detailed test results"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during manual database test");
            return StatusCode(500, new { message = "Error during database test", error = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint pentru testarea unui Personal specific pentru debugging
    /// </summary>
    [HttpPost("test-personal-save")]
    public async Task<IActionResult> TestPersonalSave([FromServices] ValyanClinic.Application.Services.IPersonalService personalService)
    {
        try
        {
            // Security: Only allow in development environment
            if (!_environment.IsDevelopment())
            {
                return BadRequest(new { message = "This endpoint is only available in development environment" });
            }

            _logger.LogInformation("🧪 Manual personal save test requested via API");

            // Create a test Personal object
            var testPersonal = new ValyanClinic.Domain.Models.Personal
            {
                Id_Personal = Guid.NewGuid(),
                Cod_Angajat = "TEST001",
                CNP = "1234567890123",
                Nume = "Test",
                Prenume = "Personal",
                Data_Nasterii = new DateTime(1990, 1, 1),
                Nationalitate = "Romana",
                Cetatenie = "Romana",
                Adresa_Domiciliu = "Test Address 123",
                Judet_Domiciliu = "Bucuresti",
                Oras_Domiciliu = "Bucuresti",
                Functia = "Test Function",
                Status_Angajat = ValyanClinic.Domain.Enums.StatusAngajat.Activ
            };

            _logger.LogInformation("Testing personal save with data: {CodAngajat}, {CNP}, {Nume}, {Prenume}", 
                testPersonal.Cod_Angajat, testPersonal.CNP, testPersonal.Nume, testPersonal.Prenume);

            var result = await personalService.CreatePersonalAsync(testPersonal, "TEST_USER");
            
            _logger.LogInformation("PersonalService.CreatePersonalAsync returned - IsSuccess: {IsSuccess}, ErrorMessage: {ErrorMessage}", 
                result.IsSuccess, result.ErrorMessage ?? "NULL");

            if (result.IsSuccess)
            {
                // Clean up test data
                try
                {
                    await personalService.DeletePersonalAsync(result.Data!.Id_Personal, "TEST_USER");
                    _logger.LogInformation("Test data cleaned up successfully");
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning("Could not cleanup test data: {Error}", cleanupEx.Message);
                }
            }
            
            return Ok(new 
            { 
                message = "Personal save test completed", 
                success = result.IsSuccess,
                errorMessage = result.ErrorMessage,
                personalCreated = result.Data?.NumeComplet,
                timestamp = DateTime.Now,
                note = "Check log files for detailed test results"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during personal save test");
            return StatusCode(500, new { message = "Error during personal save test", error = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint pentru testarea actualizarii unui Personal existent
    /// </summary>
    [HttpPost("test-personal-update")]
    public async Task<IActionResult> TestPersonalUpdate([FromServices] ValyanClinic.Application.Services.IPersonalService personalService)
    {
        try
        {
            // Security: Only allow in development environment
            if (!_environment.IsDevelopment())
            {
                return BadRequest(new { message = "This endpoint is only available in development environment" });
            }

            _logger.LogInformation("🧪 Manual personal update test requested via API");

            // First, try to find an existing personal to update
            var searchRequest = new ValyanClinic.Application.Services.PersonalSearchRequest(
                PageNumber: 1, PageSize: 10, SearchText: "Badea", 
                Departament: null, Status: null);
            
            var existingPersonal = await personalService.GetPersonalAsync(searchRequest);
            var personalToUpdate = existingPersonal.Data.FirstOrDefault();
            
            if (personalToUpdate == null)
            {
                _logger.LogInformation("No existing personal found with 'Badea', creating test data first");
                
                // Create test personal first
                var testPersonal = new ValyanClinic.Domain.Models.Personal
                {
                    Id_Personal = Guid.NewGuid(),
                    Cod_Angajat = "BADEA001",
                    CNP = "1234567890124", // Different from create test
                    Nume = "Badea",
                    Prenume = "Sorin",
                    Data_Nasterii = new DateTime(1985, 5, 15),
                    Nationalitate = "Romana",
                    Cetatenie = "Romana",
                    Adresa_Domiciliu = "Test Address Badea 123",
                    Judet_Domiciliu = "Bucuresti",
                    Oras_Domiciliu = "Bucuresti",
                    Functia = "Test Specialist",
                    Status_Angajat = ValyanClinic.Domain.Enums.StatusAngajat.Activ,
                    Departament = ValyanClinic.Domain.Enums.Departament.IT
                };

                var createResult = await personalService.CreatePersonalAsync(testPersonal, "TEST_USER");
                if (!createResult.IsSuccess)
                {
                    return Ok(new { message = "Could not create test personal for update test", error = createResult.ErrorMessage });
                }
                
                personalToUpdate = createResult.Data;
                _logger.LogInformation("Created test personal: {PersonalName}", personalToUpdate?.NumeComplet);
            }

            _logger.LogInformation("Testing update on personal - Id: {Id}, Name: {Name}", 
                personalToUpdate!.Id_Personal, personalToUpdate.NumeComplet);

            // Make some changes to test update
            personalToUpdate.Observatii = $"Updated via API test at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            personalToUpdate.Email_Personal = "test.update@valyanmed.ro";
            personalToUpdate.Departament = ValyanClinic.Domain.Enums.Departament.Administratie;

            _logger.LogInformation("Calling PersonalService.UpdatePersonalAsync...");
            var updateResult = await personalService.UpdatePersonalAsync(personalToUpdate, "TEST_USER");
            
            _logger.LogInformation("PersonalService.UpdatePersonalAsync returned - IsSuccess: {IsSuccess}, ErrorMessage: {ErrorMessage}", 
                updateResult.IsSuccess, updateResult.ErrorMessage ?? "NULL");

            if (updateResult.IsSuccess && updateResult.Data != null)
            {
                _logger.LogInformation("Update successful! Personal: {Personal}", updateResult.Data.NumeComplet);
            }
            else
            {
                _logger.LogError("Update failed! Error details: {ErrorDetails}", 
                    updateResult.ErrorMessage ?? "No error message provided");
            }
            
            return Ok(new 
            { 
                message = "Personal update test completed", 
                success = updateResult.IsSuccess,
                errorMessage = updateResult.ErrorMessage,
                personalUpdated = updateResult.Data?.NumeComplet,
                originalId = personalToUpdate.Id_Personal,
                timestamp = DateTime.Now,
                note = "Check log files for detailed test results"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during personal update test");
            return StatusCode(500, new { message = "Error during personal update test", error = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint pentru citirea continutului unui fisier de log specific
    /// </summary>
    [HttpGet("read-log/{fileName}")]
    public async Task<IActionResult> ReadLogFile(string fileName)
    {
        try
        {
            // Security: Only allow in development environment
            if (!_environment.IsDevelopment())
            {
                return BadRequest(new { message = "This endpoint is only available in development environment" });
            }

            // Sanitize filename to prevent directory traversal
            fileName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".log"))
            {
                return BadRequest(new { message = "Invalid log file name. Must be a .log file." });
            }

            var logsDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
            var logFilePath = Path.Combine(logsDirectory, fileName);
            
            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound(new { message = $"Log file '{fileName}' not found", directory = logsDirectory });
            }

            _logger.LogInformation("📖 Reading log file: {FileName}", fileName);

            var fileInfo = new FileInfo(logFilePath);
            var content = await System.IO.File.ReadAllTextAsync(logFilePath);

            // Split into lines for easier analysis
            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var recentLines = lines.TakeLast(100).ToArray(); // Last 100 lines for quick view

            return Ok(new 
            {
                fileName = fileName,
                filePath = logFilePath,
                fileSize = fileInfo.Length,
                fileSizeFormatted = FormatBytes(fileInfo.Length),
                created = fileInfo.CreationTime,
                modified = fileInfo.LastWriteTime,
                totalLines = lines.Length,
                recentLinesCount = recentLines.Length,
                recentLines = recentLines,
                fullContent = content, // Full content for complete analysis
                timestamp = DateTime.Now,
                note = "Complete log file content preserved for debugging analysis"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reading log file: {FileName}", fileName);
            return StatusCode(500, new { message = "Error reading log file", error = ex.Message, fileName = fileName });
        }
    }

    /// <summary>
    /// Endpoint pentru cautarea in log-uri dupa text specific
    /// </summary>
    [HttpGet("search-logs")]
    public async Task<IActionResult> SearchLogs([FromQuery] string searchText, [FromQuery] string? fileName = null, [FromQuery] int maxResults = 50)
    {
        try
        {
            // Security: Only allow in development environment
            if (!_environment.IsDevelopment())
            {
                return BadRequest(new { message = "This endpoint is only available in development environment" });
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return BadRequest(new { message = "Search text is required" });
            }

            _logger.LogInformation("🔍 Searching logs for: {SearchText}", searchText);

            var logsDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
            
            if (!Directory.Exists(logsDirectory))
            {
                return Ok(new { message = "Logs directory does not exist", directory = logsDirectory });
            }

            var logFiles = string.IsNullOrEmpty(fileName) 
                ? Directory.GetFiles(logsDirectory, "*.log", SearchOption.AllDirectories)
                : new[] { Path.Combine(logsDirectory, Path.GetFileName(fileName)) }.Where(f => System.IO.File.Exists(f)).ToArray();

            var searchResults = new List<object>();
            var totalMatches = 0;

            foreach (var logFile in logFiles)
            {
                try
                {
                    var content = await System.IO.File.ReadAllTextAsync(logFile);
                    var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    
                    var matches = lines
                        .Select((line, index) => new { Line = line, LineNumber = index + 1 })
                        .Where(x => x.Line.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        .Take(maxResults)
                        .ToArray();

                    if (matches.Any())
                    {
                        searchResults.Add(new
                        {
                            fileName = Path.GetFileName(logFile),
                            matchCount = matches.Length,
                            matches = matches.Select(m => new 
                            {
                                lineNumber = m.LineNumber,
                                content = m.Line,
                                highlighted = m.Line.Replace(searchText, $"**{searchText}**", StringComparison.OrdinalIgnoreCase)
                            })
                        });
                        totalMatches += matches.Length;
                    }
                }
                catch (Exception fileEx)
                {
                    searchResults.Add(new
                    {
                        fileName = Path.GetFileName(logFile),
                        error = fileEx.Message
                    });
                }
            }

            return Ok(new
            {
                searchText = searchText,
                targetFile = fileName,
                totalFiles = logFiles.Length,
                filesWithMatches = searchResults.Count,
                totalMatches = totalMatches,
                results = searchResults,
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error searching logs");
            return StatusCode(500, new { message = "Error searching logs", error = ex.Message });
        }
    }

    private static string FormatBytes(long bytes)
    {
        const int scale = 1024;
        string[] orders = { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

            max /= scale;
        }
        return "0 Bytes";
    }
}
