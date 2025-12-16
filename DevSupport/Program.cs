using System;
using System.Threading.Tasks;

namespace DevSupport;

/// <summary>
/// Program principal DevSupport cu meniu pentru toate tool-urile disponibile
/// </summary>
public class MainProgram
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Verifică dacă avem argumente directe
        if (args.Length > 0)
        {
            var command = args[0].ToLower();
            var remainingArgs = args.Length > 1 ? args[1..] : Array.Empty<string>();

            switch (command)
            {
                case "import":
                    await Tools.ICD10Import.Program.Main(remainingArgs);
                    return;

                case "translate":
                    await Tools.ICD10Translate.TranslateProgram.RunAsync(remainingArgs);
                    return;

                default:
                    Console.WriteLine($"❌ Comandă necunoscută: {command}");
                    ShowUsage();
                    return;
            }
        }

        // Afișează meniul principal
        await ShowMainMenuAsync();
    }

    private static async Task ShowMainMenuAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           DevSupport Tools - ValyanClinic                ║");
            Console.WriteLine("║                    Version 1.0                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("🛠️  TOOL-URI DISPONIBILE");
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine("  1. ICD-10 Import (din XML)");
            Console.WriteLine("  2. ICD-10 Traducere (EN → RO)");
            Console.WriteLine("  0. Ieșire");
            Console.WriteLine("════════════════════════════════════════");
            Console.Write("Alegere: ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    await Tools.ICD10Import.Program.Main(Array.Empty<string>());
                    Console.WriteLine();
                    Console.WriteLine("Apăsați orice tastă pentru a continua...");
                    if (!Console.IsInputRedirected) Console.ReadKey();
                    break;

                case "2":
                    Console.Clear();
                    await Tools.ICD10Translate.TranslateProgram.RunAsync(Array.Empty<string>());
                    break;

                case "0":
                case "exit":
                case "quit":
                    Console.WriteLine("👋 La revedere!");
                    return;

                default:
                    Console.WriteLine("❌ Opțiune invalidă!");
                    await Task.Delay(1000);
                    break;
            }
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine();
        Console.WriteLine("Utilizare:");
        Console.WriteLine("  dotnet run                    - Afișează meniul principal");
        Console.WriteLine("  dotnet run import             - Rulează import ICD-10 din XML");
        Console.WriteLine("  dotnet run translate          - Rulează traducere ICD-10");
        Console.WriteLine();
    }
}
