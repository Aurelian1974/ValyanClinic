using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

// QUICK TEST: See how Dapper handles dynamic object from sp_Consultatie_GetDraftByPacient

var connectionString = "Server=.\\ERP;Database=ValyanMed;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

var parameters = new DynamicParameters();
parameters.Add("@PacientID", Guid.Empty);
parameters.Add("@MedicID", (Guid?)null);
parameters.Add("@DataConsultatie", DateTime.Today);
parameters.Add("@ProgramareID", Guid.Parse("329DE6EB-F2C1-F011-B33E-94BB437E849C"));

Console.WriteLine("Calling SP with parameters:");
Console.WriteLine($"  @ProgramareID = 329DE6EB-F2C1-F011-B33E-94BB437E849C");
Console.WriteLine($"  @PacientID = {Guid.Empty}");
Console.WriteLine();

var result = await connection.QueryAsync<dynamic>(
    "sp_Consultatie_GetDraftByPacient",
    parameters,
    commandType: CommandType.StoredProcedure
);

var row = result.FirstOrDefault();
if (row == null)
{
    Console.WriteLine("❌ NO ROW RETURNED");
    return;
}

Console.WriteLine("✅ Row returned!");
Console.WriteLine();

// Try to access as dynamic
try
{
    Console.WriteLine("Testing dynamic access:");
    Console.WriteLine($"  ConsultatieID: {row.ConsultatieID}");
    Console.WriteLine($"  MotivePrezentare_Id: {row.MotivePrezentare_Id ?? "NULL"}");
    Console.WriteLine($"  MotivePrezentare_MotivPrezentare: '{row.MotivePrezentare_MotivPrezentare ?? "NULL"}'");
    Console.WriteLine($"  MotivePrezentare_IstoricBoalaActuala: '{row.MotivePrezentare_IstoricBoalaActuala ?? "NULL"}'");
    Console.WriteLine($"  Antecedente_Id: {row.Antecedente_Id ?? "NULL"}");
    Console.WriteLine($"  Antecedente_APP_Medicatie: '{row.Antecedente_APP_Medicatie ?? "NULL"}'");
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Dynamic access failed: {ex.Message}");
}

// Try to access as IDictionary
try
{
    Console.WriteLine("Testing IDictionary access:");
    var dict = (IDictionary<string, object>)row;
    Console.WriteLine($"  Dictionary has {dict.Count} keys");
    
    if (dict.ContainsKey("MotivePrezentare_MotivPrezentare"))
        Console.WriteLine($"  MotivePrezentare_MotivPrezentare: '{dict["MotivePrezentare_MotivPrezentare"]}'");
    else
        Console.WriteLine("  MotivePrezentare_MotivPrezentare: KEY NOT FOUND");
        
    if (dict.ContainsKey("Antecedente_APP_Medicatie"))
        Console.WriteLine($"  Antecedente_APP_Medicatie: '{dict["Antecedente_APP_Medicatie"]}'");
    else
        Console.WriteLine("  Antecedente_APP_Medicatie: KEY NOT FOUND");
        
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ IDictionary access failed: {ex.Message}");
}

Console.WriteLine("✅ Test complete");
