using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ValyanClinic.Application.Features.ConsultatieManagement.DTOs;

namespace ValyanClinic.Application.Services.ScrisoareMedicala;

/// <summary>
/// Generator PDF pentru Scrisoare Medicală folosind QuestPDF
/// Replică exact designul HTML din ScrisoareMedicalaPreview.razor
/// </summary>
public class ScrisoareMedicalaPdfGenerator
{
    // Culori din CSS - Blue Theme
    private static readonly string PrimaryBlue = "#3b82f6";
    private static readonly string DarkBlue = "#1e40af";
    private static readonly string LightBlue = "#eff6ff";
    private static readonly string DarkGray = "#1a1a1a";
    private static readonly string MediumGray = "#666666";
    private static readonly string LightGray = "#f8f9fa";
    private static readonly string BorderGray = "#e5e7eb";
    private static readonly string Red = "#dc2626";
    private static readonly string Green = "#16a34a";
    private static readonly string Yellow = "#ca8a04";

    static ScrisoareMedicalaPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(ScrisoareMedicalaDto model)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(12, Unit.Millimetre);
                page.MarginBottom(15, Unit.Millimetre);
                page.MarginHorizontal(15, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, model));
                page.Content().Element(c => ComposeContent(c, model));
                page.Footer().Element(c => ComposePageFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    public void GenerateAndSave(ScrisoareMedicalaDto model, string filePath)
    {
        var pdfBytes = GeneratePdf(model);
        File.WriteAllBytes(filePath, pdfBytes);
    }

    #region Header

    private void ComposeHeader(IContainer container, ScrisoareMedicalaDto model)
    {
        container.Column(column =>
        {
            // Row cu info clinică și anexa badge
            column.Item().Row(row =>
            {
                // Clinic info - stânga
                row.RelativeItem(3).Column(col =>
                {
                    col.Item().Text(model.NumeClinica)
                        .FontSize(16).Bold().FontColor(PrimaryBlue);
                    col.Item().Text(model.TipClinica)
                        .FontSize(9).FontColor(MediumGray);
                    col.Item().Text(model.AdresaClinica)
                        .FontSize(8).FontColor(MediumGray);
                    col.Item().Text($"Tel: {model.TelefonClinica} | Email: {model.EmailClinica}")
                        .FontSize(8).FontColor(MediumGray);
                    col.Item().Text($"CUI: {model.CUIClinica} | Reg. Com.: {model.RegistruComertClinica}")
                        .FontSize(8).FontColor(MediumGray);
                });

                // Anexa badge - dreapta
                row.RelativeItem(1).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Background(LightBlue).Border(1).BorderColor(PrimaryBlue)
                        .Padding(8).Column(badge =>
                        {
                            badge.Item().AlignCenter().Text("ANEXA nr. 43")
                                .FontSize(10).Bold().FontColor(DarkBlue);
                            badge.Item().AlignCenter().Text("Ordin MS nr. 1411/2016")
                                .FontSize(7).FontColor(MediumGray);
                        });
                });
            });

            // Titlu document
            column.Item().PaddingTop(12).AlignCenter().Column(title =>
            {
                title.Item().AlignCenter().Text("SCRISOARE MEDICALĂ")
                    .FontSize(18).Bold().FontColor(DarkBlue);
                title.Item().AlignCenter().Text($"Contract/convenție nr. {model.ContractCAS} | {model.CASJudet}")
                    .FontSize(9).FontColor(MediumGray);
            });

            // Linie separatoare
            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(BorderGray);
        });
    }

    #endregion

    #region Content

    private void ComposeContent(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(10).Column(column =>
        {
            // 1. Intro Text Box
            column.Item().Element(c => ComposeIntroText(c, model));

            // 2. Patient Box
            column.Item().Element(c => ComposePatientBox(c, model));

            // 3. Motivele Prezentării
            if (!string.IsNullOrWhiteSpace(model.MotivPrezentare))
            {
                column.Item().Element(c => ComposeSection(c, "📋 Motivele Prezentării",
                    inner => inner.Text(model.MotivPrezentare).FontSize(9)));
            }

            // 4. Afecțiune Oncologică
            column.Item().Element(c => ComposeOncologicStatus(c, model));

            // 5. Diagnostic
            column.Item().Element(c => ComposeDiagnosticSection(c, model));

            // 6. Anamneză
            column.Item().Element(c => ComposeAnamnesisSection(c, model));

            // 7. Examen Clinic
            column.Item().Element(c => ComposeExamSection(c, model));

            // 8. Rezultate Analize Laborator
            column.Item().Element(c => ComposeLabResultsSection(c, model));

            // 9. Analize Efectuate (tabel)
            column.Item().Element(c => ComposeAnalizeEfectuateSection(c, model));

            // 10. Examene Paraclinice
            column.Item().Element(c => ComposeParaclinicSection(c, model));

            // 11. Tratament Efectuat
            if (!string.IsNullOrWhiteSpace(model.TratamentAnterior))
            {
                column.Item().Element(c => ComposeSection(c, "💊 Tratament Efectuat (Anterior Consultației)",
                    inner => inner.Text(model.TratamentAnterior).FontSize(9)));
            }

            // 12. Alte Informații
            if (!string.IsNullOrWhiteSpace(model.AlteInformatii))
            {
                column.Item().Element(c => ComposeSection(c, "ℹ️ Alte Informații",
                    inner => inner.Text(model.AlteInformatii).FontSize(9)));
            }

            // 13. Tratament Recomandat
            if (model.TratamentRecomandat.Any())
            {
                column.Item().Element(c => ComposeTreatmentSection(c, model));
            }

            // 14. Recomandări
            column.Item().Element(c => ComposeRecommendationsSection(c, model));

            // 15. Analize Recomandate
            if (model.AnalizeRecomandate.Any())
            {
                column.Item().Element(c => ComposeAnalizeRecomandateSection(c, model));
            }

            // 16. Notes Box (ÎNAINTE de checkbox sections - conform HTML)
            column.Item().Element(c => ComposeNotesBox(c));

            // 17. Checkbox Sections (toate 5)
            column.Item().Element(c => ComposeAllCheckboxSections(c, model));

            // 18. Doctor Footer
            column.Item().Element(c => ComposeDoctorFooter(c, model));
        });
    }

    #endregion

    #region Intro Text

    private void ComposeIntroText(IContainer container, ScrisoareMedicalaDto model)
    {
        container.Background(LightBlue).Border(1).BorderColor(PrimaryBlue)
            .Padding(12).Column(col =>
            {
                col.Item().Text(text =>
                {
                    text.Span("Stimate coleg, ").Bold();
                    text.Span("vă informăm că ");
                    text.Span(model.PacientNumeComplet).Bold();
                    text.Span(", născut(ă) la data de ");
                    text.Span(model.DataNasteriiFormatata ?? "-").Bold();
                    text.Span(", CNP ");
                    text.Span(model.PacientCNP ?? "-").Bold();
                    text.Span(", a fost consultat(ă) în serviciul nostru la data de ");
                    text.Span(model.DataConsultatieFormatata).Bold();
                    text.Span(". Nr. din Registrul de consultații/Foaie de observație: ");
                    text.Span(model.NumarRegistruConsultatii).Bold();
                    text.Span(".");
                });
            });
    }

    #endregion

    #region Patient Box

    private void ComposePatientBox(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(12).Border(1).BorderColor(BorderGray).Column(col =>
        {
            // Title bar
            col.Item().Background(LightGray).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("📋 Date Pacient").FontSize(10).Bold().FontColor(DarkGray);
            });

            // Grid cu date pacient - 3 coloane
            col.Item().Padding(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // Row 1
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span("Nume: ").FontSize(8).FontColor(MediumGray);
                    text.Span(model.PacientNumeComplet).Bold().FontSize(9);
                });
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span("CNP: ").FontSize(8).FontColor(MediumGray);
                    text.Span(model.PacientCNP ?? "-").Bold().FontSize(9);
                });
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span("Data nașterii: ").FontSize(8).FontColor(MediumGray);
                    text.Span(model.DataNasteriiFormatata ?? "-").Bold().FontSize(9);
                });

                // Row 2
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span("Vârstă: ").FontSize(8).FontColor(MediumGray);
                    text.Span($"{model.PacientVarsta ?? 0} ani").Bold().FontSize(9);
                });
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span("Sex: ").FontSize(8).FontColor(MediumGray);
                    text.Span(model.PacientSex ?? "-").Bold().FontSize(9);
                });
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span("Data consultației: ").FontSize(8).FontColor(MediumGray);
                    text.Span(model.DataConsultatieFormatata).Bold().FontSize(9);
                });
            });
        });
    }

    private static IContainer CellStyle(IContainer container)
        => container.Padding(4);

    #endregion

    #region Oncologic Status

    private void ComposeOncologicStatus(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(10).Background(LightBlue).Border(2).BorderColor(PrimaryBlue)
            .Padding(10).Row(row =>
            {
                row.RelativeItem(2).AlignMiddle().Text("Pacient diagnosticat cu afecțiune oncologică:")
                    .FontSize(9).Bold();

                row.RelativeItem(1).Row(inner =>
                {
                    // DA
                    inner.ConstantItem(50).Row(r =>
                    {
                        r.ConstantItem(16).Height(16).Border(2).BorderColor(PrimaryBlue)
                            .Background(model.EsteAfectiuneOncologica ? PrimaryBlue : Colors.White)
                            .AlignCenter().AlignMiddle()
                            .Text(model.EsteAfectiuneOncologica ? "✓" : "")
                            .FontColor(Colors.White).FontSize(10).Bold();
                        r.ConstantItem(4);
                        r.RelativeItem().AlignMiddle().Text("DA").FontSize(9).Bold();
                    });

                    inner.ConstantItem(10);

                    // NU
                    inner.ConstantItem(50).Row(r =>
                    {
                        r.ConstantItem(16).Height(16).Border(2).BorderColor(PrimaryBlue)
                            .Background(!model.EsteAfectiuneOncologica ? PrimaryBlue : Colors.White)
                            .AlignCenter().AlignMiddle()
                            .Text(!model.EsteAfectiuneOncologica ? "✓" : "")
                            .FontColor(Colors.White).FontSize(10).Bold();
                        r.ConstantItem(4);
                        r.RelativeItem().AlignMiddle().Text("NU").FontSize(9).Bold();
                    });
                });
            });
    }

    #endregion

    #region Generic Section

    private void ComposeSection(IContainer container, string title, Action<IContainer> contentBuilder)
    {
        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            // Section title
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4).Row(row =>
            {
                row.RelativeItem().Text(title).FontSize(11).Bold().FontColor(PrimaryBlue);
            });

            // Content
            col.Item().PaddingTop(6).Element(contentBuilder);
        });
    }

    #endregion

    #region Diagnostic Section

    private void ComposeDiagnosticSection(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            // Title
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text("🩺 Diagnostic și Cod de Diagnostic").FontSize(11).Bold().FontColor(PrimaryBlue);

            // Diagnostic box
            col.Item().PaddingTop(6).Border(1).BorderColor(BorderGray).Column(box =>
            {
                // Header
                box.Item().Background(LightGray).Padding(6)
                    .Text("Diagnostice Stabilite").FontSize(9).Bold();

                // Content
                box.Item().Padding(8).Column(content =>
                {
                    // Diagnostic principal
                    if (model.DiagnosticPrincipal != null)
                    {
                        content.Item().Row(row =>
                        {
                            row.ConstantItem(70).Background("#dcfce7").Border(1).BorderColor(Green)
                                .Padding(3).AlignCenter()
                                .Text("Principal").FontSize(8).Bold().FontColor(Green);
                            row.ConstantItem(8);
                            row.ConstantItem(70).Background(LightBlue).Border(1).BorderColor(PrimaryBlue)
                                .Padding(3).AlignCenter()
                                .Text(model.DiagnosticPrincipal.CodICD10).FontSize(8).Bold().FontColor(PrimaryBlue);
                            row.ConstantItem(8);
                            row.RelativeItem().AlignMiddle()
                                .Text(model.DiagnosticPrincipal.Denumire).FontSize(9);
                        });

                        if (!string.IsNullOrWhiteSpace(model.DiagnosticPrincipal.Detalii))
                        {
                            content.Item().PaddingLeft(150).PaddingTop(2)
                                .Text(model.DiagnosticPrincipal.Detalii).FontSize(8).Italic().FontColor(MediumGray);
                        }
                    }

                    // Diagnostice secundare
                    foreach (var diag in model.DiagnosticeSecundare)
                    {
                        content.Item().PaddingTop(6).Row(row =>
                        {
                            row.ConstantItem(70).Background("#fef9c3").Border(1).BorderColor(Yellow)
                                .Padding(3).AlignCenter()
                                .Text("Secundar").FontSize(8).Bold().FontColor(Yellow);
                            row.ConstantItem(8);
                            row.ConstantItem(70).Background(LightBlue).Border(1).BorderColor(PrimaryBlue)
                                .Padding(3).AlignCenter()
                                .Text(diag.CodICD10).FontSize(8).Bold().FontColor(PrimaryBlue);
                            row.ConstantItem(8);
                            row.RelativeItem().AlignMiddle()
                                .Text(diag.Denumire).FontSize(9);
                        });

                        if (!string.IsNullOrWhiteSpace(diag.Detalii))
                        {
                            content.Item().PaddingLeft(150).PaddingTop(2)
                                .Text(diag.Detalii).FontSize(8).Italic().FontColor(MediumGray);
                        }
                    }
                });
            });
        });
    }

    #endregion

    #region Anamnesis Section

    private void ComposeAnamnesisSection(IContainer container, ScrisoareMedicalaDto model)
    {
        var hasContent = !string.IsNullOrWhiteSpace(model.AntecendenteHeredoColaterale) ||
                        !string.IsNullOrWhiteSpace(model.AntecendentePatologicePersonale) ||
                        !string.IsNullOrWhiteSpace(model.Alergii) ||
                        !string.IsNullOrWhiteSpace(model.MedicatieCronicaAnterioara) ||
                        !string.IsNullOrWhiteSpace(model.FactoriDeRisc) ||
                        !string.IsNullOrWhiteSpace(model.IstoricBoalaActuala);

        if (!hasContent) return;

        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text("📜 Anamneză").FontSize(11).Bold().FontColor(PrimaryBlue);

            col.Item().PaddingTop(6).Column(content =>
            {
                AddInlineSubsection(content, "Antecedente Heredocolaterale:", model.AntecendenteHeredoColaterale);
                AddInlineSubsection(content, "Antecedente Patologice Personale:", model.AntecendentePatologicePersonale);
                AddInlineSubsection(content, "Alergii:", model.Alergii ?? "Nu sunt cunoscute.");
                AddInlineSubsection(content, "Medicație Cronică Anterioară:", model.MedicatieCronicaAnterioara ?? "Nu există.");
                AddInlineSubsection(content, "Factori de Risc:", model.FactoriDeRisc ?? "Nu au fost identificați.");
                AddInlineSubsection(content, "Istoricul Bolii Actuale:", model.IstoricBoalaActuala);
            });
        });
    }

    private void AddInlineSubsection(ColumnDescriptor col, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        col.Item().PaddingTop(3).Text(text =>
        {
            text.Span(label + " ").Bold().FontSize(9);
            text.Span(value).FontSize(9);
        });
    }

    #endregion

    #region Exam Section

    private void ComposeExamSection(IContainer container, ScrisoareMedicalaDto model)
    {
        var hasVitals = !string.IsNullOrWhiteSpace(model.StareGenerala) ||
                       !string.IsNullOrWhiteSpace(model.TensiuneArteriala) ||
                       model.Puls.HasValue || model.Temperatura.HasValue ||
                       model.Greutate.HasValue || model.Inaltime.HasValue;

        var hasExam = !string.IsNullOrWhiteSpace(model.ExamenObiectivDetaliat) ||
                     !string.IsNullOrWhiteSpace(model.AlteObservatiiClinice) ||
                     !string.IsNullOrWhiteSpace(model.ExamenClinicGeneral) ||
                     !string.IsNullOrWhiteSpace(model.ExamenClinicLocal);

        if (!hasVitals && !hasExam) return;

        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text("🩺 Examen Clinic").FontSize(11).Bold().FontColor(PrimaryBlue);

            col.Item().PaddingTop(6).Column(content =>
            {
                // Examen General - compact vitals
                if (hasVitals)
                {
                    content.Item().Text("Examen Clinic General:").Bold().FontSize(9);
                    content.Item().PaddingTop(3).Background(LightGray).Padding(8).Text(text =>
                    {
                        var items = new List<string>();

                        if (!string.IsNullOrWhiteSpace(model.StareGenerala))
                            items.Add($"Stare generală: {model.StareGenerala}");
                        if (!string.IsNullOrWhiteSpace(model.TensiuneArteriala))
                            items.Add($"TA: {model.TensiuneArteriala} mmHg");
                        if (model.Puls.HasValue)
                            items.Add($"Puls: {model.Puls} bpm");
                        if (model.Temperatura.HasValue)
                            items.Add($"Temp: {model.Temperatura}°C");
                        if (model.Greutate.HasValue)
                            items.Add($"G: {model.Greutate} kg");
                        if (model.Inaltime.HasValue)
                            items.Add($"Î: {model.Inaltime} cm");
                        if (model.IMC.HasValue)
                            items.Add($"IMC: {model.IMC:F1} kg/m²");
                        if (model.SaturatieO2.HasValue)
                            items.Add($"SpO2: {model.SaturatieO2}%");
                        if (model.Glicemie.HasValue)
                            items.Add($"Glicemie: {model.Glicemie} mg/dL");
                        if (!string.IsNullOrWhiteSpace(model.Tegumente))
                            items.Add($"Tegumente: {model.Tegumente}");
                        if (!string.IsNullOrWhiteSpace(model.Mucoase))
                            items.Add($"Mucoase: {model.Mucoase}");
                        if (!string.IsNullOrWhiteSpace(model.GanglioniLimfatici))
                            items.Add($"Ganglioni limfatici: {model.GanglioniLimfatici}");
                        if (!string.IsNullOrWhiteSpace(model.Edeme))
                            items.Add($"Edeme: {model.Edeme}");

                        text.Span(string.Join(" | ", items)).FontSize(9);
                    });
                }

                // Examen General (text liber)
                if (!string.IsNullOrWhiteSpace(model.ExamenClinicGeneral))
                {
                    content.Item().PaddingTop(6).Text(text =>
                    {
                        text.Span("Examen general: ").Bold().FontSize(9);
                        text.Span(model.ExamenClinicGeneral).FontSize(9);
                    });
                }

                // Examen Local
                if (!string.IsNullOrWhiteSpace(model.ExamenObiectivDetaliat))
                {
                    content.Item().PaddingTop(6).Text("Examen Clinic Local:").Bold().FontSize(9);
                    content.Item().PaddingTop(3).Text(model.ExamenObiectivDetaliat).FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(model.ExamenClinicLocal))
                {
                    content.Item().PaddingTop(3).Text(model.ExamenClinicLocal).FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(model.AlteObservatiiClinice))
                {
                    content.Item().PaddingTop(3).Text(model.AlteObservatiiClinice).FontSize(9).Italic();
                }
            });
        });
    }

    #endregion

    #region Lab Results Section

    private void ComposeLabResultsSection(IContainer container, ScrisoareMedicalaDto model)
    {
        var hasLab = (model.RezultateNormale?.Any() == true) || (model.RezultatePatologice?.Any() == true);

        if (!hasLab) return;

        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text("🔬 Examene de Laborator").FontSize(11).Bold().FontColor(PrimaryBlue);

            col.Item().PaddingTop(6).Row(row =>
            {
                // Valori Normale - stânga
                if (model.RezultateNormale?.Any() == true)
                {
                    row.RelativeItem().Background("#f0fdf4").Border(1).BorderColor("#bbf7d0").Column(normCol =>
                    {
                        normCol.Item().Padding(6).Text("✅ Valori Normale")
                            .FontSize(9).Bold().FontColor(Green);

                        foreach (var rez in model.RezultateNormale)
                        {
                            normCol.Item().PaddingHorizontal(6).PaddingVertical(2)
                                .Text($"{rez.Denumire}: {rez.Valoare} {rez.Unitate}").FontSize(8);
                        }
                        normCol.Item().Height(4); // padding bottom
                    });
                }

                if (model.RezultateNormale?.Any() == true && model.RezultatePatologice?.Any() == true)
                {
                    row.ConstantItem(10);
                }

                // Valori Patologice - dreapta
                if (model.RezultatePatologice?.Any() == true)
                {
                    row.RelativeItem().Background("#fef2f2").Border(1).BorderColor("#fecaca").Column(patCol =>
                    {
                        patCol.Item().Padding(6).Text("⚠️ Valori Patologice")
                            .FontSize(9).Bold().FontColor(Red);

                        foreach (var rez in model.RezultatePatologice)
                        {
                            var valRef = !string.IsNullOrWhiteSpace(rez.ValoareNormala) ? $" ({rez.ValoareNormala})" : "";
                            patCol.Item().PaddingHorizontal(6).PaddingVertical(2)
                                .Text($"{rez.Denumire}: {rez.Valoare} {rez.Unitate}{valRef}").FontSize(8);
                        }
                        patCol.Item().Height(4); // padding bottom
                    });
                }
            });
        });
    }

    #endregion

    #region Analize Efectuate Section

    private void ComposeAnalizeEfectuateSection(IContainer container, ScrisoareMedicalaDto model)
    {
        // Folosim AnalizeEfectuate din DTO (AnalizaEfectuataScrisoareDto)
        if (model.AnalizeEfectuate?.Any() != true) return;

        var analizeAnormale = model.AnalizeEfectuate.Where(a => a.EsteAnormal).ToList();
        var analizeNormale = model.AnalizeEfectuate.Where(a => !a.EsteAnormal).ToList();

        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text($"🧪 Analize Efectuate ({model.AnalizeEfectuate.Count})")
                .FontSize(11).Bold().FontColor(PrimaryBlue);

            col.Item().PaddingTop(6).Column(content =>
            {
                // Valori în Afara Limitelor (Anormale) - tabel
                if (analizeAnormale.Any())
                {
                    content.Item().Background("#fef2f2").Border(1).BorderColor("#fecaca").Column(abnCol =>
                    {
                        abnCol.Item().Padding(6).Text($"⚠️ Valori în Afara Limitelor ({analizeAnormale.Count})")
                            .FontSize(9).Bold().FontColor(Red);

                        abnCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);   // Analiză
                                columns.RelativeColumn(1.5f); // Rezultat
                                columns.RelativeColumn(1.5f); // Referință
                                columns.RelativeColumn(1);   // Data
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#fecaca").Padding(4).Text("Analiză").FontSize(7).Bold();
                                header.Cell().Background("#fecaca").Padding(4).Text("Rezultat").FontSize(7).Bold();
                                header.Cell().Background("#fecaca").Padding(4).Text("Referință").FontSize(7).Bold();
                                header.Cell().Background("#fecaca").Padding(4).Text("Data").FontSize(7).Bold();
                            });

                            foreach (var a in analizeAnormale)
                            {
                                table.Cell().BorderBottom(1).BorderColor("#fecaca").Padding(4)
                                    .Text(a.NumeAnaliza).FontSize(8);
                                table.Cell().BorderBottom(1).BorderColor("#fecaca").Padding(4)
                                    .Text($"{a.Rezultat} {a.UnitateMasura}").FontSize(8).Bold().FontColor(Red);
                                table.Cell().BorderBottom(1).BorderColor("#fecaca").Padding(4)
                                    .Text(a.ValoriReferinta ?? "-").FontSize(7).FontColor(MediumGray);
                                table.Cell().BorderBottom(1).BorderColor("#fecaca").Padding(4)
                                    .Text(a.DataEfectuareFormatata).FontSize(7).FontColor(MediumGray);
                            }
                        });
                    });
                }

                // Valori Normale - grid compact
                if (analizeNormale.Any())
                {
                    content.Item().PaddingTop(8).Background("#f0fdf4").Border(1).BorderColor("#bbf7d0").Column(normCol =>
                    {
                        normCol.Item().Padding(6).Text($"✅ Valori Normale ({analizeNormale.Count})")
                            .FontSize(9).Bold().FontColor(Green);

                        // Grid cu 3 coloane pentru analize normale
                        var itemsPerRow = 3;
                        var rows = (int)Math.Ceiling(analizeNormale.Count / (double)itemsPerRow);

                        for (int i = 0; i < rows; i++)
                        {
                            var rowItems = analizeNormale.Skip(i * itemsPerRow).Take(itemsPerRow).ToList();
                            normCol.Item().Padding(4).Row(row =>
                            {
                                foreach (var a in rowItems)
                                {
                                    row.RelativeItem().Text(text =>
                                    {
                                        text.Span(a.NumeAnaliza + ": ").FontSize(8).FontColor(MediumGray);
                                        text.Span($"{a.Rezultat} {a.UnitateMasura}").FontSize(8).Bold().FontColor(Green);
                                    });
                                }
                                // Fill remaining columns if not complete
                                for (int j = rowItems.Count; j < itemsPerRow; j++)
                                {
                                    row.RelativeItem();
                                }
                            });
                        }
                    });
                }
            });
        });
    }

    #endregion

    #region Paraclinic Section

    private void ComposeParaclinicSection(IContainer container, ScrisoareMedicalaDto model)
    {
        var hasContent = !string.IsNullOrWhiteSpace(model.AlteInvestigatii) ||
                        !string.IsNullOrWhiteSpace(model.RezultatEKG) ||
                        !string.IsNullOrWhiteSpace(model.RezultatEcografie) ||
                        !string.IsNullOrWhiteSpace(model.RezultatRx) ||
                        model.InvestigatiiImagisticeEfectuate?.Any() == true ||
                        model.ExplorariEfectuate?.Any() == true ||
                        model.EndoscopiiEfectuate?.Any() == true;

        if (!hasContent) return;

        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text("📊 Examene Paraclinice").FontSize(11).Bold().FontColor(PrimaryBlue);

            col.Item().PaddingTop(6).Column(content =>
            {
                // Investigații Imagistice Efectuate
                if (model.InvestigatiiImagisticeEfectuate?.Any() == true)
                {
                    content.Item().Text("🔬 Investigații Imagistice:").Bold().FontSize(9);
                    foreach (var inv in model.InvestigatiiImagisticeEfectuate)
                    {
                        content.Item().PaddingTop(2).PaddingLeft(15).Text(text =>
                        {
                            text.Span("• " + inv.Denumire).FontSize(9);
                            if (!string.IsNullOrWhiteSpace(inv.Observatii))
                                text.Span($" - {inv.Observatii}").FontSize(8).Italic().FontColor(MediumGray);
                        });
                    }
                }

                // Explorări Efectuate
                if (model.ExplorariEfectuate?.Any() == true)
                {
                    content.Item().PaddingTop(6).Text("📈 Explorări Funcționale:").Bold().FontSize(9);
                    foreach (var exp in model.ExplorariEfectuate)
                    {
                        content.Item().PaddingTop(2).PaddingLeft(15).Text(text =>
                        {
                            text.Span("• " + exp.Denumire).FontSize(9);
                            if (!string.IsNullOrWhiteSpace(exp.Observatii))
                                text.Span($" - {exp.Observatii}").FontSize(8).Italic().FontColor(MediumGray);
                        });
                    }
                }

                // Endoscopii Efectuate
                if (model.EndoscopiiEfectuate?.Any() == true)
                {
                    content.Item().PaddingTop(6).Text("🔭 Endoscopii:").Bold().FontSize(9);
                    foreach (var end in model.EndoscopiiEfectuate)
                    {
                        content.Item().PaddingTop(2).PaddingLeft(15).Text(text =>
                        {
                            text.Span("• " + end.Denumire).FontSize(9);
                            if (!string.IsNullOrWhiteSpace(end.Observatii))
                                text.Span($" - {end.Observatii}").FontSize(8).Italic().FontColor(MediumGray);
                        });
                    }
                }

                // EKG
                if (!string.IsNullOrWhiteSpace(model.RezultatEKG))
                {
                    content.Item().PaddingTop(6).Text(text =>
                    {
                        text.Span($"EKG ({model.DataConsultatieFormatata}): ").Bold().FontSize(9);
                        text.Span(model.RezultatEKG).FontSize(9);
                    });
                }

                // Ecografie
                if (!string.IsNullOrWhiteSpace(model.RezultatEcografie))
                {
                    content.Item().PaddingTop(4).Text(text =>
                    {
                        text.Span($"Ecocardiografie ({model.DataConsultatieFormatata}): ").Bold().FontSize(9);
                        text.Span(model.RezultatEcografie).FontSize(9);
                    });
                }

                // Rx
                if (!string.IsNullOrWhiteSpace(model.RezultatRx))
                {
                    content.Item().PaddingTop(4).Text(text =>
                    {
                        text.Span($"Radiografie ({model.DataConsultatieFormatata}): ").Bold().FontSize(9);
                        text.Span(model.RezultatRx).FontSize(9);
                    });
                }

                // Alte Investigații (text liber)
                if (!string.IsNullOrWhiteSpace(model.AlteInvestigatii))
                {
                    content.Item().PaddingTop(6).Text("Alte Investigații:").Bold().FontSize(9);
                    content.Item().PaddingTop(2).Text(model.AlteInvestigatii).FontSize(9);
                }
            });
        });
    }

    #endregion

    #region Treatment Section

    private void ComposeTreatmentSection(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text($"💊 Tratament Recomandat ({model.TratamentRecomandat.Count} medicamente)")
                .FontSize(11).Bold().FontColor(PrimaryBlue);

            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);   // Medicament
                    columns.RelativeColumn(1.2f); // Doză
                    columns.RelativeColumn(1);   // Frecvență
                    columns.RelativeColumn(1);   // Durată
                    columns.RelativeColumn(2);   // Obs
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(LightGray).Border(1).BorderColor(BorderGray).Padding(6)
                        .Text("MEDICAMENT").FontSize(8).Bold();
                    header.Cell().Background(LightGray).Border(1).BorderColor(BorderGray).Padding(6)
                        .Text("DOZĂ").FontSize(8).Bold();
                    header.Cell().Background(LightGray).Border(1).BorderColor(BorderGray).Padding(6)
                        .Text("FRECVENȚĂ").FontSize(8).Bold();
                    header.Cell().Background(LightGray).Border(1).BorderColor(BorderGray).Padding(6)
                        .Text("DURATĂ").FontSize(8).Bold();
                    header.Cell().Background(LightGray).Border(1).BorderColor(BorderGray).Padding(6)
                        .Text("OBS.").FontSize(8).Bold();
                });

                // Rows
                foreach (var med in model.TratamentRecomandat)
                {
                    table.Cell().Border(1).BorderColor(BorderGray).Padding(5)
                        .Text(med.Denumire).FontSize(9).Bold();
                    table.Cell().Border(1).BorderColor(BorderGray).Padding(5)
                        .Text(med.Doza ?? "-").FontSize(9);
                    table.Cell().Border(1).BorderColor(BorderGray).Padding(5)
                        .Text(med.Frecventa ?? "-").FontSize(9);
                    table.Cell().Border(1).BorderColor(BorderGray).Padding(5)
                        .Text(med.Durata ?? "-").FontSize(9);
                    table.Cell().Border(1).BorderColor(BorderGray).Padding(5)
                        .Text(med.Observatii ?? "").FontSize(8).Italic().FontColor(PrimaryBlue);
                }
            });
        });
    }

    #endregion

    #region Recommendations Section

    private void ComposeRecommendationsSection(IContainer container, ScrisoareMedicalaDto model)
    {
        var hasContent = model.Recomandari.Any() ||
                        model.InvestigatiiImagistice?.Any() == true ||
                        model.Explorari?.Any() == true ||
                        model.Endoscopii?.Any() == true;

        if (!hasContent) return;

        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text("📝 Recomandări").FontSize(11).Bold().FontColor(PrimaryBlue);

            col.Item().PaddingTop(6).Background(LightGray).Padding(10).Column(content =>
            {
                int index = 1;

                foreach (var rec in model.Recomandari)
                {
                    content.Item().PaddingTop(2).Text($"{index}. {rec}").FontSize(9);
                    index++;
                }

                // Investigații imagistice recomandate
                if (model.InvestigatiiImagistice?.Any() == true)
                {
                    content.Item().PaddingTop(6).Text($"{index}. Investigații imagistice recomandate:")
                        .FontSize(9).Bold();
                    foreach (var inv in model.InvestigatiiImagistice)
                    {
                        var text = inv.Denumire;
                        if (!string.IsNullOrWhiteSpace(inv.Observatii))
                            text += $" - {inv.Observatii}";
                        content.Item().PaddingLeft(15).Text($"• {text}").FontSize(9);
                    }
                    index++;
                }

                // Explorări recomandate
                if (model.Explorari?.Any() == true)
                {
                    content.Item().PaddingTop(4).Text($"{index}. Explorări funcționale recomandate:")
                        .FontSize(9).Bold();
                    foreach (var inv in model.Explorari)
                    {
                        var text = inv.Denumire;
                        if (!string.IsNullOrWhiteSpace(inv.Observatii))
                            text += $" - {inv.Observatii}";
                        content.Item().PaddingLeft(15).Text($"• {text}").FontSize(9);
                    }
                    index++;
                }

                // Endoscopii recomandate
                if (model.Endoscopii?.Any() == true)
                {
                    content.Item().PaddingTop(4).Text($"{index}. Endoscopii recomandate:")
                        .FontSize(9).Bold();
                    foreach (var inv in model.Endoscopii)
                    {
                        var text = inv.Denumire;
                        if (!string.IsNullOrWhiteSpace(inv.Observatii))
                            text += $" - {inv.Observatii}";
                        content.Item().PaddingLeft(15).Text($"• {text}").FontSize(9);
                    }
                }
            });
        });
    }

    #endregion

    #region Analize Recomandate

    private void ComposeAnalizeRecomandateSection(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(12).ShowEntire().Column(col =>
        {
            col.Item().BorderBottom(1).BorderColor(BorderGray).PaddingBottom(4)
                .Text($"🧪 Analize Recomandate ({model.AnalizeRecomandate.Count})")
                .FontSize(11).Bold().FontColor(PrimaryBlue);

            // Split into 3 columns
            var analize = model.AnalizeRecomandate.ToList();
            var countPerColumn = (int)Math.Ceiling(analize.Count / 3.0);
            var col1 = analize.Take(countPerColumn).ToList();
            var col2 = analize.Skip(countPerColumn).Take(countPerColumn).ToList();
            var col3 = analize.Skip(countPerColumn * 2).ToList();

            col.Item().PaddingTop(6).Row(row =>
            {
                // Column 1
                row.RelativeItem().Element(c => ComposeAnalizeColumn(c, col1));

                if (col2.Any())
                {
                    row.ConstantItem(8);
                    row.RelativeItem().Element(c => ComposeAnalizeColumn(c, col2));
                }

                if (col3.Any())
                {
                    row.ConstantItem(8);
                    row.RelativeItem().Element(c => ComposeAnalizeColumn(c, col3));
                }
            });
        });
    }

    private void ComposeAnalizeColumn(IContainer container, List<AnalizaRecomandataScrisoareDto> analize)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Analiză
                columns.RelativeColumn(1);   // Categorie
            });

            table.Header(header =>
            {
                header.Cell().Background(LightGray).Border(1).BorderColor(BorderGray).Padding(4)
                    .Text("Analiză").FontSize(7).Bold();
                header.Cell().Background(LightGray).Border(1).BorderColor(BorderGray).Padding(4)
                    .Text("Categorie").FontSize(7).Bold();
            });

            foreach (var a in analize)
            {
                string bgColor = a.EsteCito ? "#fef2f2" : "#ffffff";
                
                table.Cell().Border(1).BorderColor(BorderGray).Background(bgColor).Padding(3).Row(r =>
                {
                    r.RelativeItem().Text(a.NumeAnaliza).FontSize(8);
                    if (a.EsteCito)
                    {
                        r.ConstantItem(12).AlignCenter().Text("!")
                            .FontSize(9).Bold().FontColor(Red);
                    }
                });
                
                table.Cell().Border(1).BorderColor(BorderGray).Background(bgColor).Padding(3)
                    .Text(a.Categorie ?? "-").FontSize(7).FontColor(MediumGray);
            }
        });
    }

    #endregion

    #region All Checkbox Sections

    private void ComposeAllCheckboxSections(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(12).Column(col =>
        {
            // 1. Indicație Internare
            col.Item().Element(c => ComposeFullWidthCheckboxSection(c, "🏥 Indicație de Revenire pentru Internare",
                ($"Da, revine pentru internare în termen de {model.TermenInternare ?? "____________"}", model.AreIndicatieInternare),
                ("Nu, nu este necesară revenirea pentru internare", !model.AreIndicatieInternare)));

            // 2. Prescripție Medicală
            col.Item().PaddingTop(8).Element(c => ComposeFullWidthCheckboxSection(c, "💊 Prescripție Medicală",
                ($"S-a eliberat prescripție medicală - {model.SeriePrescriptie ?? "____________"}", model.SaEliberatPrescriptie),
                ("Nu s-a eliberat prescripție medicală deoarece nu a fost necesar", model.NuSaEliberatPrescriptieNuAFostNecesar),
                ("Nu s-a eliberat prescripție medicală", model.NuSaEliberatPrescriptie)));

            // 3. Concediu Medical
            col.Item().PaddingTop(8).Element(c => ComposeFullWidthCheckboxSection(c, "📋 Concediu Medical",
                ($"S-a eliberat concediu medical - seria și numărul: {model.SerieConcediuMedical ?? "____________"}", model.SaEliberatConcediuMedical),
                ("Nu s-a eliberat concediu medical deoarece nu a fost necesar", model.NuSaEliberatConcediuNuAFostNecesar),
                ("Nu s-a eliberat concediu medical", model.NuSaEliberatConcediuMedical)));

            // 4. Îngrijiri la Domiciliu
            col.Item().PaddingTop(8).Element(c => ComposeFullWidthCheckboxSection(c, "🏠 Îngrijiri Medicale la Domiciliu",
                ("S-a eliberat recomandare pentru îngrijiri medicale la domiciliu/paliative la domiciliu", model.SaEliberatRecomandareIngrijiriDomiciliu),
                ("Nu s-a eliberat recomandare pentru îngrijiri medicale la domiciliu deoarece nu a fost necesar", model.NuSaEliberatIngrijiriNuAFostNecesar)));

            // 5. Dispozitive Medicale
            col.Item().PaddingTop(8).Element(c => ComposeFullWidthCheckboxSection(c, "🔧 Dispozitive Medicale",
                ("S-a eliberat prescripție medicală pentru dispozitive medicale în ambulatoriu", model.SaEliberatPrescriptieDispozitive),
                ("Nu s-a eliberat prescripție medicală pentru dispozitive medicale deoarece nu a fost necesar", model.NuSaEliberatDispozitiveNuAFostNecesar)));
        });
    }

    private void ComposeFullWidthCheckboxSection(IContainer container, string title, params (string label, bool isChecked)[] items)
    {
        container.Border(1).BorderColor(BorderGray).ShowEntire().Column(col =>
        {
            // Title bar
            col.Item().Background(LightGray).Padding(8)
                .Text(title).FontSize(9).Bold().FontColor(PrimaryBlue);

            // Checkboxes
            col.Item().Padding(10).Column(inner =>
            {
                foreach (var item in items)
                {
                    inner.Item().PaddingTop(4).Row(row =>
                    {
                        row.ConstantItem(18).Height(18).Border(2).BorderColor(PrimaryBlue)
                            .Background(item.isChecked ? PrimaryBlue : Colors.White)
                            .AlignCenter().AlignMiddle()
                            .Text(item.isChecked ? "✓" : "").FontColor(Colors.White).FontSize(11).Bold();
                        row.ConstantItem(10);
                        row.RelativeItem().AlignMiddle().Text(item.label).FontSize(9);
                    });
                }
            });
        });
    }

    #endregion

    #region Notes Box

    private void ComposeNotesBox(IContainer container)
    {
        container.PaddingTop(12).Border(1).BorderColor(BorderGray).Column(col =>
        {
            // Title
            col.Item().Background(LightGray).Padding(8)
                .Text("📝 Notă Importantă").FontSize(9).Bold().FontColor(PrimaryBlue);

            // Content
            col.Item().Padding(10).Column(inner =>
            {
                inner.Item().Text("Se va specifica durata pentru care se poate prescrie de medicul din ambulatoriu, inclusiv medicul de familie, fiecare dintre medicamentele recomandate.")
                    .FontSize(8);

                // Attention box
                inner.Item().PaddingTop(8).Background("#fef2f2").Border(1).BorderColor("#fecaca")
                    .Padding(8).Column(att =>
                    {
                        att.Item().Text("⚠️ ATENȚIE!").FontSize(9).Bold().FontColor(Red);
                        att.Item().PaddingTop(4).Text(
                            "Nerespectarea obligației medicului de specialitate din ambulatoriu sau din spital de a inițiate tratamentul prin prescrierea primei rețete pentru medicamente cu sau fără contribuție personală, astfel cum este prevăzut în protocoalele terapeutice, precum și de a elibera prescripția medicală / bilete de trimitere / concediu medical / recomandări pentru îngrijiri la domiciliu / prescripții pentru dispozitive medicale în fiecare caz pentru care este necesar, se sancționează potrivit contractului încheiat de furnizor cu casa de asigurări de sănătate!")
                            .FontSize(7).FontColor("#991b1b");
                    });

                inner.Item().PaddingTop(8).Text("Valabilitatea scrisorii medicale începe de la data eliberării acesteia și este în concordanță cu protocolul terapeutic.")
                    .FontSize(8);
            });
        });
    }

    #endregion

    #region Doctor Footer

    private void ComposeDoctorFooter(IContainer container, ScrisoareMedicalaDto model)
    {
        container.PaddingTop(15).ShowEntire().Column(mainCol =>
        {
            // Footer grid - medic + data
            mainCol.Item().BorderTop(1).BorderColor(BorderGray).PaddingTop(12)
                .Row(row =>
                {
                    // Medic curant - stânga
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Medic Curant").FontSize(9).Bold().FontColor(MediumGray);
                        col.Item().PaddingTop(4).Text(model.MedicNumeComplet).FontSize(12).Bold().FontColor(DarkBlue);
                        col.Item().Text(model.MedicSpecializare ?? "").FontSize(9).FontColor(MediumGray);
                        col.Item().Text($"Cod Parafă: {model.MedicCodParafa ?? "-"}").FontSize(8).FontColor(MediumGray);

                        col.Item().PaddingTop(12).Border(1).BorderColor(BorderGray)
                            .Height(55).Width(140).AlignCenter().AlignMiddle()
                            .Text("Parafă\n& Semnătură").FontSize(8).FontColor(MediumGray);
                    });

                    // Data și transmitere - dreapta
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text("Data Emiterii").FontSize(9).Bold().FontColor(MediumGray);
                        col.Item().AlignRight().PaddingTop(4).Text(model.DataEmitereFormatata)
                            .FontSize(18).Bold().FontColor(PrimaryBlue);

                        col.Item().AlignRight().PaddingTop(15).Text("Calea de Transmitere")
                            .FontSize(9).Bold().FontColor(MediumGray);

                        col.Item().AlignRight().PaddingTop(8).Row(r =>
                        {
                            r.ConstantItem(16).Height(16).Border(2).BorderColor(PrimaryBlue)
                                .Background(model.TransmiterePrinAsigurat ? PrimaryBlue : Colors.White)
                                .AlignCenter().AlignMiddle()
                                .Text(model.TransmiterePrinAsigurat ? "✓" : "").FontColor(Colors.White).FontSize(10);
                            r.ConstantItem(8);
                            r.RelativeItem().AlignMiddle().Text("Prin asigurat").FontSize(9);
                        });

                        col.Item().AlignRight().PaddingTop(4).Row(r =>
                        {
                            r.ConstantItem(16).Height(16).Border(2).BorderColor(PrimaryBlue)
                                .Background(model.TransmiterePrinEmail ? PrimaryBlue : Colors.White)
                                .AlignCenter().AlignMiddle()
                                .Text(model.TransmiterePrinEmail ? "✓" : "").FontColor(Colors.White).FontSize(10);
                            r.ConstantItem(8);
                            r.RelativeItem().AlignMiddle().Text(text =>
                            {
                                text.Span("Prin poștă electronică: ").FontSize(9);
                                text.Span(model.EmailTransmitere ?? "____________").FontSize(9).Bold();
                            });
                        });
                    });
                });

            // Footnotes
            mainCol.Item().PaddingTop(15).BorderTop(1).BorderColor(BorderGray).PaddingTop(10).Column(footnotes =>
            {
                footnotes.Item().Text(text =>
                {
                    text.Span("*) ").Bold().FontSize(7);
                    text.Span("Scrisoarea medicală se întocmește în două exemplare, din care un exemplar rămâne la medicul care a efectuat consultația/serviciul în ambulatoriul de specialitate, iar un exemplar este transmis medicului de familie/medicului de specialitate din ambulatoriul de specialitate.")
                        .FontSize(7).FontColor(MediumGray);
                });

                footnotes.Item().PaddingTop(4).Text("Scrisoarea medicală sau biletul de ieșire din spital sunt documente tipizate care se întocmesc la data externării, într-un singur exemplar care este transmis medicului de familie/medicului de specialitate din ambulatoriul de specialitate, direct, prin intermediul asiguratului ori prin poștă electronică.")
                    .FontSize(7).FontColor(MediumGray);

                footnotes.Item().PaddingTop(4).Text("Scrisoarea medicală trimisă prin poștă electronică este semnată cu semnătura electronică extinsă/calificată.")
                    .FontSize(7).FontColor(MediumGray);
            });
        });
    }

    #endregion

    #region Page Footer

    private void ComposePageFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(8).FontColor(MediumGray));
            text.Span("Pagina ");
            text.CurrentPageNumber();
            text.Span(" din ");
            text.TotalPages();
        });
    }

    #endregion
}
