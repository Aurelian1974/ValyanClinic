using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DevSupport.ICD10Translate;

namespace DevSupport.Tools.ICD10Translate;

/// <summary>
/// Program pentru traducerea descrierilor ICD-10 din engleză în română
/// 
/// Utilizare:
///   dotnet run translate
///   
/// Configurare:
///   Folosește Google Translate gratuit (fără API key) sau setează API key în config
/// </summary>
public class TranslateProgram
{
    private const string DefaultConnectionString = "Server=DESKTOP-3Q8HI82\\ERP;Database=ValyanMed;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True";

    public static async Task RunAsync(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       ICD-10 Translation Tool for ValyanClinic           ║");
        Console.WriteLine("║                    Version 1.1                           ║");
        Console.WriteLine("║          (Google Translate Gratuit - Fără API Key)       ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Încarcă configurația
        var config = LoadConfiguration();
        var connectionString = config.ConnectionString ?? DefaultConnectionString;

        // Afișează meniul
        await ShowMenuAsync(connectionString, config);
    }

    private static async Task ShowMenuAsync(string connectionString, TranslationConfig config)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("📋 MENIU TRADUCERE ICD-10");
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine("  1. Afișează statistici traduceri");
            Console.WriteLine("  2. Traduce toate codurile (automat)");
            Console.WriteLine("  3. Traduce doar codurile comune");
            Console.WriteLine("  4. Traduce o categorie specifică");
            Console.WriteLine("  5. Traduce un număr limitat de coduri");
            Console.WriteLine("  6. Configurare (provider, batch size)");
            Console.WriteLine("  7. Test traducere (un singur text)");
            Console.WriteLine("  8. Import traduceri din CSV");
            Console.WriteLine("  0. Ieșire");
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine($"  Provider curent: {config.Provider ?? "GoogleFree"} (gratuit)");
            Console.WriteLine("════════════════════════════════════════");
            Console.Write("Alegere: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await ShowStatisticsAsync(connectionString, config);
                    break;

                case "2":
                    await TranslateAllAsync(connectionString, config);
                    break;

                case "3":
                    await TranslateCommonAsync(connectionString, config);
                    break;

                case "4":
                    await TranslateCategoryAsync(connectionString, config);
                    break;

                case "5":
                    await TranslateLimitedAsync(connectionString, config);
                    break;

                case "6":
                    await ConfigureAsync(config);
                    break;

                case "7":
                    await TestTranslationAsync(config);
                    break;

                case "8":
                    await ImportFromCsvAsync(connectionString);
                    break;

                case "0":
                case "exit":
                case "quit":
                    Console.WriteLine("👋 La revedere!");
                    return;

                default:
                    Console.WriteLine("❌ Opțiune invalidă!");
                    break;
            }
        }
    }

    private static async Task ShowStatisticsAsync(string connectionString, TranslationConfig config)
    {
        Console.WriteLine();
        
        using var service = CreateTranslationService(config);
        var translator = new ICD10Translator(connectionString, service);
        await translator.ShowStatisticsAsync();
    }

    private static async Task TranslateAllAsync(string connectionString, TranslationConfig config)
    {
        Console.WriteLine();

        using var service = CreateTranslationService(config);
        var translator = new ICD10Translator(connectionString, service);
        
        // Afișează statistici mai întâi
        await translator.ShowStatisticsAsync();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  ATENȚIE: Traducerea a ~47,000 coduri poate dura câteva ore!");
        Console.WriteLine("   Folosește opțiunea 5 pentru a traduce un număr limitat de coduri.");
        Console.ResetColor();
        Console.Write("Continuați? (da/nu): ");

        var response = Console.ReadLine()?.ToLower().Trim();
        if (response != "da" && response != "d")
        {
            Console.WriteLine("❌ Operațiune anulată.");
            return;
        }

        await translator.TranslateAllUntranslatedAsync();
    }

    private static async Task TranslateCommonAsync(string connectionString, TranslationConfig config)
    {
        Console.WriteLine();

        using var service = CreateTranslationService(config);
        var translator = new ICD10Translator(connectionString, service, batchSize: config.BatchSize);
        await translator.TranslateCommonCodesAsync();
    }

    private static async Task TranslateCategoryAsync(string connectionString, TranslationConfig config)
    {
        Console.WriteLine();
        Console.WriteLine("Categorii disponibile:");
        Console.WriteLine("  - Infectioase      - Neoplasme       - Sange");
        Console.WriteLine("  - Endocrin         - Mental          - Nervos");
        Console.WriteLine("  - Ochi             - Ureche          - Cardiovascular");
        Console.WriteLine("  - Respirator       - Digestiv        - Piele");
        Console.WriteLine("  - Musculo-scheletic- Genito-urinar   - Obstetric");
        Console.WriteLine("  - Perinatal        - Malformatii     - Simptome");
        Console.WriteLine("  - Traumatisme      - Cauze externe   - Factori sanatate");
        Console.WriteLine();
        Console.Write("Categorie: ");

        var category = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(category))
        {
            Console.WriteLine("❌ Categorie invalidă!");
            return;
        }

        using var service = CreateTranslationService(config);
        var translator = new ICD10Translator(connectionString, service, batchSize: config.BatchSize);
        await translator.TranslateByCategoryAsync(category);
    }

    private static async Task TranslateLimitedAsync(string connectionString, TranslationConfig config)
    {
        Console.WriteLine();
        Console.Write("Câte coduri să traducă? (default: 100): ");

        var input = Console.ReadLine()?.Trim();
        var count = 100;
        if (int.TryParse(input, out var parsed) && parsed > 0)
        {
            count = parsed;
        }

        Console.WriteLine($"🔄 Se traduc {count} coduri...");
        Console.WriteLine();

        using var service = CreateTranslationService(config);
        var translator = new ICD10Translator(connectionString, service, batchSize: config.BatchSize);
        await translator.TranslateLimitedAsync(count);
    }

    private static async Task ConfigureAsync(TranslationConfig config)
    {
        Console.WriteLine();
        Console.WriteLine("🔧 CONFIGURARE");
        Console.WriteLine("════════════════════════════════════════");
        Console.WriteLine("  1. Google Translate Gratuit (recomandat, fără API key)");
        Console.WriteLine("  2. Azure Translator (necesită API key)");
        Console.WriteLine("  3. DeepL (necesită API key)");
        Console.WriteLine("  4. LibreTranslate (gratuit, calitate mai slabă)");
        Console.WriteLine("════════════════════════════════════════");
        Console.Write("Provider (default: 1): ");

        var providerChoice = Console.ReadLine()?.Trim();
        
        config.Provider = providerChoice switch
        {
            "1" => "GoogleFree",
            "2" => "Azure",
            "3" => "DeepL",
            "4" => "LibreTranslate",
            _ => "GoogleFree"
        };

        if (config.Provider != "GoogleFree")
        {
            Console.Write($"API Key pentru {config.Provider}: ");
            var apiKey = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(apiKey))
            {
                config.ApiKey = apiKey;
            }

            if (config.Provider == "Azure")
            {
                Console.Write("Region (default: westeurope): ");
                var region = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(region))
                {
                    config.Region = region;
                }
            }
        }

        Console.Write($"Batch size - câte coduri pe rundă (default: {config.BatchSize}): ");
        var batchSize = Console.ReadLine()?.Trim();
        if (int.TryParse(batchSize, out var bs) && bs > 0)
        {
            config.BatchSize = bs;
        }

        SaveConfiguration(config);
        Console.WriteLine("✅ Configurație salvată!");

        await Task.CompletedTask;
    }

    private static async Task TestTranslationAsync(TranslationConfig config)
    {
        Console.WriteLine();

        Console.Write("Text de test (EN) [sau Enter pentru exemplu]: ");
        var text = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            text = "Type 2 diabetes mellitus with diabetic chronic kidney disease";
        }

        Console.WriteLine($"📝 Text: {text}");
        Console.WriteLine($"🔄 Se traduce cu {config.Provider ?? "GoogleFree"}...");

        using var service = CreateTranslationService(config);
        var translation = await service.TranslateAsync(text);

        if (!string.IsNullOrEmpty(translation))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Traducere: {translation}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Traducerea a eșuat!");
            Console.ResetColor();
        }
    }

    private static async Task ImportFromCsvAsync(string connectionString)
    {
        Console.WriteLine();
        Console.WriteLine("📁 IMPORT DIN CSV");
        Console.WriteLine("════════════════════════════════════════");
        Console.WriteLine("Formatul CSV trebuie să fie:");
        Console.WriteLine("  Code,ShortDescriptionRo");
        Console.WriteLine("  sau");
        Console.WriteLine("  Code,ShortDescriptionEn,ShortDescriptionRo");
        Console.WriteLine();
        Console.Write("Calea către fișierul CSV: ");

        var csvPath = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
        {
            Console.WriteLine("❌ Fișierul nu există!");
            return;
        }

        try
        {
            var importer = new CsvTranslationImporter(connectionString);
            await importer.ImportFromCsvAsync(csvPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Eroare la import: {ex.Message}");
        }
    }

    private static ICD10TranslationService CreateTranslationService(TranslationConfig config)
    {
        var provider = config.Provider?.ToLower() switch
        {
            "googlefree" => ICD10TranslationService.TranslationProvider.GoogleFree,
            "azure" => ICD10TranslationService.TranslationProvider.AzureTranslator,
            "deepl" => ICD10TranslationService.TranslationProvider.DeepL,
            "libretranslate" => ICD10TranslationService.TranslationProvider.LibreTranslate,
            _ => ICD10TranslationService.TranslationProvider.GoogleFree  // Default gratuit
        };

        return new ICD10TranslationService(
            provider,
            config.ApiKey,
            config.Region,
            config.Endpoint
        );
    }

    #region Configuration

    private static readonly string ConfigPath = Path.Combine(
        AppContext.BaseDirectory, 
        "translation_config.json");

    private static TranslationConfig LoadConfiguration()
    {
        try
        {
            // Încearcă să citească din appsettings.json al proiectului principal
            var appSettingsPath = Path.Combine(
                Directory.GetCurrentDirectory(), 
                "..", "ValyanClinic", "appsettings.json");

            if (File.Exists(appSettingsPath))
            {
                var json = File.ReadAllText(appSettingsPath);
                using var doc = JsonDocument.Parse(json);
                
                var config = new TranslationConfig();
                
                // Citește connection string
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var connStrings))
                {
                    if (connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                    {
                        config.ConnectionString = defaultConn.GetString();
                    }
                }

                // Citește configurația de traducere dacă există
                if (doc.RootElement.TryGetProperty("Translation", out var translation))
                {
                    if (translation.TryGetProperty("Provider", out var provider))
                        config.Provider = provider.GetString();
                    if (translation.TryGetProperty("ApiKey", out var apiKey))
                        config.ApiKey = apiKey.GetString();
                    if (translation.TryGetProperty("Region", out var region))
                        config.Region = region.GetString();
                    if (translation.TryGetProperty("BatchSize", out var batchSize))
                        config.BatchSize = batchSize.GetInt32();
                }

                return config;
            }

            // Încearcă configurația locală
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<TranslationConfig>(json) ?? new TranslationConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Nu s-a putut citi configurația: {ex.Message}");
        }

        // Default: Google Free (fără API key)
        return new TranslationConfig 
        { 
            Provider = "GoogleFree",
            BatchSize = 20  // Batch mai mic pentru rate limiting
        };
    }

    private static void SaveConfiguration(TranslationConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Nu s-a putut salva configurația: {ex.Message}");
        }
    }

    private class TranslationConfig
    {
        public string? ConnectionString { get; set; }
        public string? Provider { get; set; } = "GoogleFree";
        public string? ApiKey { get; set; }
        public string? Region { get; set; } = "westeurope";
        public string? Endpoint { get; set; }
        public int BatchSize { get; set; } = 20;
    }

    #endregion
}
