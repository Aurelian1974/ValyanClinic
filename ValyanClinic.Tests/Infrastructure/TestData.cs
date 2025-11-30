using ValyanClinic.Application.Features.Consultatii.Models;
using ValyanClinic.Application.Features.Programari.DTOs;

namespace ValyanClinic.Tests.Infrastructure;

/// <summary>
/// Test data factories for creating test objects with realistic data.
/// Provides consistent test data across all test files.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Creates a valid ConsultatieModel with default test data
    /// </summary>
    public static ConsultatieModel CreateValidConsultatieModel()
    {
        return new ConsultatieModel
        {
            ProgramareId = Guid.NewGuid(),
            PacientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            DataConsultatie = DateTime.Now,
            
            // Motive Prezentare
            MotivPrezentare = "Dureri abdominale",
            SimptomeAsociate = "Greata, varsaturi",
            Observatii = "Simptome de 3 zile",
            
            // Antecedente
            AntecedentePersonale = "Hipertensiune arteriala",
            AntecedenteFamiliare = "Diabet zaharat la tata",
            Alergii = "Penicilina",
            TratamenteActuale = "Enalapril 10mg",
            
            // Examen
            Greutate = 75.5m,
            Inaltime = 175,
            IndiceMasaCorporala = 24.7m,
            Tensiune = "130/80",
            Puls = "72",
            Temperatura = "36.7",
            ExamenClinic = "Abdomen sensibil la palpare",
            
            // Diagnostic
            DiagnosticPrincipal = "K29.0", // Gastrita acuta
            DiagnosticuriSecundare = "K30", // Dispepsie
            
            // Investigatii
            InvestigatiiRecomandate = "Ecografie abdominala",
            RezultateInvestigatii = "",
            
            // Tratament
            Tratament = "Omeprazol 20mg, 1cp/zi",
            Recomandari = "Dieta usoara, evitarea alimentelor grele",
            ProgramareControl = DateTime.Now.AddDays(14),
            
            // Concluzie
            Concluzie = "Gastrita acuta, tratament medicamentos si dieta",
            NoteSuplimentare = "Monitorizare simptome urmatoarele 7 zile"
        };
    }
    
    /// <summary>
    /// Creates an empty ConsultatieModel (for new consultatie)
    /// </summary>
    public static ConsultatieModel CreateEmptyConsultatieModel()
    {
        return new ConsultatieModel
        {
            ProgramareId = Guid.NewGuid(),
            PacientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            DataConsultatie = DateTime.Now
        };
    }
    
    /// <summary>
    /// Creates a ProgramareDto with test data
    /// </summary>
    public static ProgramareDto CreateProgramareDto()
    {
        return new ProgramareDto
        {
            Id = Guid.NewGuid(),
            PacientId = Guid.NewGuid(),
            PacientNume = "Popescu",
            PacientPrenume = "Ion",
            DoctorId = Guid.NewGuid(),
            DoctorNume = "Dr. Ionescu Maria",
            DataProgramare = DateTime.Today.AddHours(10),
            DurataMinute = 30,
            Status = "Confirmata",
            TipProgramare = "Consultatie",
            Observatii = "Prima consultatie"
        };
    }
    
    /// <summary>
    /// Creates a list of test ICD-10 codes
    /// </summary>
    public static List<string> CreateTestIcd10Codes()
    {
        return new List<string>
        {
            "K29.0", // Gastrita acuta
            "K30",   // Dispepsie
            "R10.4", // Durere abdominala
            "E11.9"  // Diabet zaharat tip 2
        };
    }
    
    /// <summary>
    /// Creates test Guid identifiers
    /// </summary>
    public static class Ids
    {
        public static readonly Guid TestProgramareId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TestPacientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid TestDoctorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public static readonly Guid TestConsultatieId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    }
}
