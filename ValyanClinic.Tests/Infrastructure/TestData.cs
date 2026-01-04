using ValyanClinic.Application.Features.ConsultatieManagement.Commands.CreateConsultatie;
using ValyanClinic.Application.Features.ProgramareManagement.DTOs;

namespace ValyanClinic.Tests.Infrastructure;

/// <summary>
/// Test data factories for creating test objects with realistic data.
/// Provides consistent test data across all test files.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Creates a valid CreateConsultatieCommand with default test data
    /// </summary>
    public static CreateConsultatieCommand CreateValidConsultatieCommand()
    {
        return new CreateConsultatieCommand
        {
            ProgramareID = Guid.NewGuid(),
            PacientID = Guid.NewGuid(),
            MedicID = Guid.NewGuid(),
            TipConsultatie = "Prima consultatie",

            // Motive Prezentare
            MotivPrezentare = "Dureri abdominale",
            IstoricBoalaActuala = "Simptome de 3 zile - greata, varsaturi",

            // Antecedente (Anamneză)
            IstoricMedicalPersonal = "Hipertensiune arteriala, alergii la Penicilina, tratament Enalapril 10mg",
            IstoricFamilial = "Tata - diabet tip 2, Mama - HTA",

            // Examen
            Greutate = 75.5m,
            Inaltime = 175,
            TensiuneArteriala = "130/80",
            Puls = 72,
            Temperatura = 36.7m,

            // Diagnostic
            DiagnosticPozitiv = "Gastrita acuta",
            CoduriICD10 = "K29.0",
            CoduriICD10Secundare = "K30",

            // Tratament
            TratamentMedicamentos = "Omeprazol 20mg, 1cp/zi",
            RecomandariDietetice = "Dieta usoara, evitarea alimentelor grele",

            // Concluzie
            Concluzie = "Gastrita acuta, tratament medicamentos si dieta",
            ObservatiiMedic = "Monitorizare simptome urmatoarele 7 zile",

            // Audit
            CreatDe = "doctor123"
        };
    }

    /// <summary>
    /// Creates an empty CreateConsultatieCommand (for new consultatie)
    /// </summary>
    public static CreateConsultatieCommand CreateEmptyConsultatieCommand()
    {
        return new CreateConsultatieCommand
        {
            ProgramareID = Guid.NewGuid(),
            PacientID = Guid.NewGuid(),
            MedicID = Guid.NewGuid(),
            TipConsultatie = "Prima consultatie",
            CreatDe = "doctor123"
        };
    }

    /// <summary>
    /// Creates a ProgramareListDto with test data
    /// </summary>
    public static ProgramareListDto CreateProgramareDto()
    {
        return new ProgramareListDto
        {
            ProgramareID = Guid.NewGuid(),
            PacientID = Guid.NewGuid(),
            PacientNumeComplet = "Popescu Ion",
            DoctorID = Guid.NewGuid(),
            DoctorNumeComplet = "Dr. Ionescu Maria",
            DataProgramare = DateTime.Today.AddHours(10),
            OraInceput = new TimeSpan(10, 0, 0),
            OraSfarsit = new TimeSpan(10, 30, 0),
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
