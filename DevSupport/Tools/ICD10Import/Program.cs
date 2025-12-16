using System;
using System.IO;
using System.Threading.Tasks;
using DevSupport.ICD10Import;

namespace DevSupport.Tools.ICD10Import;

/// <summary>
/// Program pentru importarea codurilor ICD-10 din XML în baza de date
/// 
/// Utilizare:
///   dotnet run -- [cale_xml] [connection_string]
///   
/// Exemple:
///   dotnet run
///   dotnet run -- "D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\data\icd10cm_tabular_2026.xml"
/// </summary>
public class Program
{
    // Configurare implicită
    private const string DefaultXmlPath = @"D:\Lucru\CMS\DevSupport\01_Database\07_ICD10_Data\data\icd10cm_tabular_2026.xml";
    private const string DefaultConnectionString = "Server=DESKTOP-3Q8HI82\\ERP;Database=ValyanMed;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True";

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       ICD-10 XML Import Tool for ValyanClinic            ║");
        Console.WriteLine("║                    Version 2.0                           ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Determină calea XML
        var xmlPath = args.Length > 0 ? args[0] : DefaultXmlPath;
        
        // Determină connection string
        var connectionString = args.Length > 1 ? args[1] : DefaultConnectionString;

        // Verifică dacă fișierul XML există
        if (!File.Exists(xmlPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Fișierul XML nu a fost găsit: {xmlPath}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Utilizare:");
            Console.WriteLine("  ICD10ImportTool.exe [cale_xml] [connection_string]");
            Console.WriteLine();
            Console.WriteLine("Exemplu:");
            Console.WriteLine($"  ICD10ImportTool.exe \"{DefaultXmlPath}\"");
            return;
        }

        // Afișează informații
        Console.WriteLine("📁 Fișier XML: " + xmlPath);
        Console.WriteLine("💾 Database: ValyanMed");
        Console.WriteLine();

        // Confirmă operațiunea
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  ATENȚIE: Această operațiune va șterge datele existente din tabelele ICD-10!");
        Console.ResetColor();
        Console.Write("Continuați? (da/nu): ");

        var response = Console.ReadLine()?.ToLower().Trim();
        if (response != "da" && response != "d" && response != "yes" && response != "y")
        {
            Console.WriteLine("❌ Operațiune anulată.");
            return;
        }

        Console.WriteLine();

        try
        {
            // Creează și rulează importerul
            var importer = new ICD10XmlImporter(connectionString, xmlPath, verbose: true);
            await importer.ExecuteImportAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Eroare fatală: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Apăsați orice tastă pentru a închide...");
        
        // Verifică dacă avem console input disponibil
        if (Console.IsInputRedirected == false)
        {
            Console.ReadKey();
        }
    }
}
