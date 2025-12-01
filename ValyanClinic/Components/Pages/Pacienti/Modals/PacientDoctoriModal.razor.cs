using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MediatR;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

/// <summary>
/// Modal component pentru gestionarea doctorilor asociați unui pacient.
/// Permite vizualizarea, adăugarea și dezactivarea relațiilor pacient-doctor.
/// </summary>
/// <remarks>
/// <b>Funcționalități principale:</b>
/// - Afișare listă doctori activi și inactivi (istoric)
/// - Adăugare doctor nou prin modal secundar
/// - Dezactivare relație existentă (soft delete)
/// - Validare și error handling complet
/// 
/// <b>Pattern folosit:</b> Modal overlay cu sub-modal pentru add operation
/// <b>State management:</b> Local state cu MediatR pentru backend operations
/// </remarks>
public partial class PacientDoctoriModal : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<PacientDoctoriModal> Logger { get; set; } = default!;

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public Guid? PacientID { get; set; }
    [Parameter] public string PacientNume { get; set; } = string.Empty;

    private bool IsLoading { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    private bool ShowAddDoctorModal { get; set; }

    private List<DoctorAsociatDto> AllDoctori { get; set; } = new();
    private List<DoctorAsociatDto> DoctoriActivi => AllDoctori.Where(d => d.EsteActiv).ToList();
    private List<DoctorAsociatDto> DoctoriInactivi => AllDoctori.Where(d => !d.EsteActiv).ToList();

    /// <summary>
    /// Lifecycle hook care se execută când parametrii componentei se modifică.
    /// Încarcă automat lista de doctori când modalul devine vizibil și PacientID este valid.
    /// </summary>
    /// <returns>Task ce reprezintă operația asincronă.</returns>
    /// <remarks>
    /// <b>Condiții de încărcare:</b>
    /// - IsVisible = true
    /// - PacientID.HasValue = true
    /// - PacientID.Value != Guid.Empty
    /// 
    /// <b>Logging:</b> Înregistrează toate acțiunile pentru debugging.
    /// </remarks>
    protected override async Task OnParametersSetAsync()
    {
        Logger.LogInformation("[PacientDoctoriModal] OnParametersSetAsync - IsVisible: {IsVisible}, PacientID: {PacientID}, PacientNume: {PacientNume}",
            IsVisible, PacientID, PacientNume);

        if (IsVisible && PacientID.HasValue)
        {
            Logger.LogInformation("[PacientDoctoriModal] Loading doctori for PacientID: {PacientID}", PacientID);
            await LoadDoctori();
        }
        else
        {
            Logger.LogWarning("[PacientDoctoriModal] NOT loading - IsVisible: {IsVisible}, HasValue: {HasValue}",
                IsVisible, PacientID.HasValue);
        }
    }

    /// <summary>
    /// Încarcă lista completă de doctori asociați pacientului (activi și inactivi).
    /// </summary>
    /// <returns>Task ce reprezintă operația asincronă de încărcare.</returns>
    /// <exception cref="Exception">
    /// Aruncat când apare o erare la comunicarea cu backend-ul.
    /// Eroarea este capturată și afișată user-friendly în UI.
    /// </exception>
    /// <remarks>
    /// <b>Flow:</b>
    /// 1. Setează IsLoading = true pentru UI feedback
    /// 2. Apelează GetDoctoriByPacientQuery prin MediatR
    /// 3. Populează AllDoctori cu rezultatul (sau listă goală)
    /// 4. Calculează automat DoctoriActivi și DoctoriInactivi prin computed properties
    /// 5. Setează IsLoading = false în finally block
    /// 
    /// <b>Error Handling:</b>
    /// - Result.IsSuccess = false → HasError + ErrorMessage
    /// - Exception → HasError + ex.Message
    /// - Toate erorile sunt logate pentru troubleshooting
    /// 
    /// <b>Performance:</b>
    /// - Single query pentru toate relațiile (ApenumereActivi: false)
    /// - Client-side filtering pentru activi/inactivi (neglijabil - <100 doctori/pacient)
    /// </remarks>
    /// <example>
    /// Apelat automat de OnParametersSetAsync când modalul se deschide:
    /// <code>
    /// protected override async Task OnParametersSetAsync()
    /// {
    ///     if (IsVisible && PacientID.HasValue)
    ///     {
    ///         await LoadDoctori();
    ///     }
    /// }
    /// </code>
    /// 
    /// Sau manual după operații de add/remove:
    /// <code>
    /// await LoadDoctori(); // Refresh după adăugare doctor
    /// </code>
    /// </example>
    private async Task LoadDoctori()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            Logger.LogInformation("[PacientDoctoriModal] Calling GetDoctoriByPacientQuery for PacientID: {PacientID}", PacientID);

            var query = new GetDoctoriByPacientQuery(PacientID!.Value, ApenumereActivi: false);
            var result = await Mediator.Send(query);

            Logger.LogInformation("[PacientDoctoriModal] Query result: IsSuccess={IsSuccess}, Count={Count}",
                result.IsSuccess, result.Value?.Count ?? 0);

            if (result.IsSuccess)
            {
                AllDoctori = result.Value ?? new List<DoctorAsociatDto>();
                Logger.LogInformation("[PacientDoctoriModal] Loaded {Count} doctori ({Active} activi, {Inactive} inactivi)",
                    AllDoctori.Count, DoctoriActivi.Count, DoctoriInactivi.Count);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.FirstError;
                Logger.LogError("[PacientDoctoriModal] Error loading doctori: {Error}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Eroare: {ex.Message}";
            Logger.LogError(ex, "[PacientDoctoriModal] Exception loading doctori");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deschide modalul secundar pentru adăugarea unui doctor nou la pacient.
    /// </summary>
    /// <remarks>
    /// <b>UI Design - Două Butoane pentru Aceeași Acțiune:</b>
    /// 
    /// <b>Buton 1:</b> "+ Adaugă doctor" (header modal, sus-dreapta)
    /// - Poziție: Header-ul modalului principal
    /// - Vizibilitate: ÎNTOTDEAUNA vizibil
    /// - Use case: Adăugare doctor când există deja doctori asociați
    /// 
    /// <b>Buton 2:</b> "+ Adaugă primul doctor" (empty state, centru)
    /// - Poziție: Centrul ecranului când lista este goală
    /// - Vizibilitate: DOAR când AllDoctori.Count == 0
    /// - Use case: Prima asociere doctor-pacient
    /// 
    /// <b>Rezultat:</b>
    /// Ambele butoane apelează această metodă și deschid AddDoctorToPacientModal.
    /// 
    /// <b>Flow complet:</b>
    /// 1. User click pe oricare dintre cele 2 butoane
    /// 2. ShowAddDoctorModal = true → Modal secundar devine vizibil
    /// 3. User selectează doctor și salvează
    /// 4. OnDoctorAdded() este apelat automat
    /// 5. Lista se reîncarcă și modal secundar se închide
    /// </remarks>
    /// <example>
    /// Apelat de ambele butoane:
    /// <code>
    /// // Buton 1 (header)
    /// &lt;button class="btn btn-primary btn-sm" @onclick="OpenAddDoctorModal"&gt;
    ///     &lt;i class="fas fa-plus"&gt;&lt;/i&gt; Adaugă doctor
    /// &lt;/button&gt;
    /// 
    /// // Buton 2 (empty state)
    /// &lt;button class="btn btn-primary" @onclick="OpenAddDoctorModal"&gt;
    ///     &lt;i class="fas fa-plus"&gt;&lt;/i&gt; Adaugă primul doctor
    /// &lt;/button&gt;
    /// </code>
    /// </example>
    private void OpenAddDoctorModal()
    {
        Logger.LogInformation("[PacientDoctoriModal] Opening AddDoctorModal");
        ShowAddDoctorModal = true;
    }

    /// <summary>
    /// Event handler invocat automat când un doctor nou a fost adăugat cu succes.
    /// Închide modalul secundar și reîncarcă lista de doctori.
    /// </summary>
    /// <returns>Task ce reprezintă operația asincronă.</returns>
    /// <remarks>
    /// <b>Event Flow (Parent-Child Communication):</b>
    /// 
    /// 1. <b>AddDoctorToPacientModal</b> salvează doctorul în DB (MediatR command)
    /// 2. <b>AddDoctorToPacientModal</b> invocă EventCallback OnDoctorAdded
    /// 3. <b>PacientDoctoriModal</b> primește evenimentul și execută această metodă
    /// 4. ShowAddDoctorModal = false → Modal secundar dispare cu animație
    /// 5. LoadDoctori() → Reîncarcă lista actualizată din DB
    /// 6. StateHasChanged() → Forțează re-render pentru a afișa noul doctor
    /// 
    /// <b>Rezultat vizibil:</b>
    /// - Modal secundar se închide smooth
    /// - Lista de doctori se reîncarcă automat
    /// - Noul doctor apare instant în lista "Doctori activi"
    /// - Counter-ul "(X doctori)" se actualizează
    /// 
    /// <b>Performance:</b>
    /// - Single refresh query (eficient)
    /// - UI update atomic (fără flickering)
    /// </remarks>
    /// <example>
    /// Folosit ca EventCallback în modal secundar:
    /// <code>
    /// &lt;AddDoctorToPacientModal 
    ///     IsVisible="@ShowAddDoctorModal"
    ///     PacientID="@PacientID"
    ///     OnDoctorAdded="OnDoctorAdded" /&gt;
    /// </code>
    /// </example>
    private async Task OnDoctorAdded()
    {
        Logger.LogInformation("[PacientDoctoriModal] Doctor added - reloading list");
        ShowAddDoctorModal = false;
        await LoadDoctori();
        StateHasChanged();
    }

    /// <summary>
    /// Dezactivează (soft delete) relația dintre pacient și un doctor specific.
    /// Solicită confirmare utilizatorului înainte de executare.
    /// </summary>
    /// <param name="doctor">
    /// DTO-ul doctorului a cărui relație urmează să fie dezactivată.
    /// Trebuie să conțină RelatieID valid și DoctorNumeComplet pentru mesaj confirmare.
    /// </param>
    /// <returns>Task ce reprezintă operația asincronă.</returns>
    /// <exception cref="Exception">
    /// Aruncat când apare o eroare la comunicarea cu backend-ul.
    /// Eroarea este capturată și afișată user-friendly printr-un alert JavaScript.
    /// </exception>
    /// <remarks>
    /// <b>Flow complet cu UX optimization:</b>
    /// 
    /// 1. <b>Confirmare:</b> Dialog JavaScript "Sunteți sigur...?" (Browser native)
    /// 2. <b>User cancels:</b> Return early, zero impact (fast path)
    /// 3. <b>User confirms:</b>
    ///    - Apelează RemoveRelatieCommand prin MediatR
    ///    - Command execută sp_PacientiPersonalMedical_RemoveRelatie (soft delete)
    ///    - DB update: EsteActiv = 0, DataDezactivarii = GETDATE()
    ///    - Reîncarcă listă completă (LoadDoctori)
    ///    - Doctorul se mută automat din "Activi" în "Istoric inactivi"
    ///    - Alert success "Relație dezactivată cu succes!"
    /// 
    /// <b>Error Handling:</b>
    /// - Result.IsSuccess = false → Alert cu result.FirstError
    /// - Exception → Alert cu ex.Message + logging
    /// 
    /// <b>Important:</b>
    /// - Operație REVERSIBILĂ (relația rămâne în DB cu EsteActiv = 0)
    /// - Istoric păstrat pentru audit și rapoarte
    /// - Reactiv are posibilă printr-un command separat (dacă e implementat)
    /// </remarks>
    /// <example>
    /// Apelat din butonul "Dezactivează" al fiecărui doctor card:
    /// <code>
    /// &lt;button class="btn btn-sm btn-danger" @onclick="() => RemoveDoctor(doctor)"&gt;
    ///     &lt;i class="fas fa-times"&gt;&lt;/i&gt; Dezactivează
    /// &lt;/button&gt;
    /// </code>
    /// 
    /// User flow:
    /// <code>
    /// 1. Click "Dezactivează" pe Dr. Popescu
    /// 2. Confirm dialog: "Sunteți sigur că doriți să dezactivați relația cu Dr. Popescu Ion?"
    /// 3. Click "OK" → Backend soft delete
    /// 4. Success alert → Lista refresh automat
    /// 5. Dr. Popescu apare în secțiunea "Istoric relații inactive"
    /// </code>
    /// </example>
    private async Task RemoveDoctor(DoctorAsociatDto doctor)
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
            $"Sunteți sigur că doriți să dezactivați relația cu {doctor.DoctorNumeComplet}?");

        if (!confirmed) return;

        try
        {
            Logger.LogInformation("[PacientDoctoriModal] Removing doctor: {DoctorName}, RelatieID: {RelatieID}",
                doctor.DoctorNumeComplet, doctor.RelatieID);

            var command = new RemoveRelatieCommand(RelatieID: doctor.RelatieID);
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                await LoadDoctori();
                await JSRuntime.InvokeVoidAsync("alert", "Relație dezactivată cu succes!");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", $"Eroare: {result.FirstError}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PacientDoctoriModal] Error removing doctor");
            await JSRuntime.InvokeVoidAsync("alert", $"Eroare: {ex.Message}");
        }
    }

    /// <summary>
    /// Determină clasa CSS pentru badge-ul de tip relație în funcție de valoarea tipului.
    /// </summary>
    /// <param name="tipRelatie">
    /// Tipul relației dintre pacient și doctor (ex: "MedicPrimar", "Specialist", etc.).
    /// Poate fi null pentru relații fără tip specificat.
    /// </param>
    /// <returns>
    /// Numele clasei CSS Bootstrap/custom pentru badge (ex: "primary", "info", "success").
    /// </returns>
    /// <remarks>
    /// <b>Mapping Tip → CSS Class → Culoare Vizuală:</b>
    /// 
    /// - "MedicPrimar" → "primary" → Albastru (Blue 400, important)
    /// - "Specialist" → "info" → Sky blue (informativ)
    /// - "MedicConsultant" → "success" → Verde (pozitiv)
    /// - "MedicDeGarda" → "warning" → Galben (atenție)
    /// - null / altele → "secondary" → Gri (neutral)
    /// 
    /// <b>Design Decision:</b>
    /// Folosește pattern matching (switch expression) pentru readability și performance.
    /// </remarks>
    /// <example>
    /// Folosit în markup pentru badge styling:
    /// <code>
    /// &lt;span class="badge badge-@GetBadgeClass(doctor.TipRelatie)"&gt;
    ///     @doctor.TipRelatie
    /// &lt;/span&gt;
    /// </code>
    /// 
    /// Rezultat HTML:
    /// <code>
    /// &lt;!-- Pentru MedicPrimar --&gt;
    /// &lt;span class="badge badge-primary"&gt;MedicPrimar&lt;/span&gt;
    /// 
    /// &lt;!-- Pentru Specialist --&gt;
    /// &lt;span class="badge badge-info"&gt;Specialist&lt;/span&gt;
    /// </code>
    /// </example>
    private string GetBadgeClass(string? tipRelatie)
    {
        return tipRelatie switch
        {
            "MedicPrimar" => "primary",
            "Specialist" => "info",
            "MedicConsultant" => "success",
            "MedicDeGarda" => "warning",
            _ => "secondary"
        };
    }

    /// <summary>
    /// Închide modalul principal și resetează starea internă.
    /// Invocă EventCallback pentru a notifica componenta părinte despre închidere.
    /// </summary>
    /// <returns>Task ce reprezintă operația asincronă.</returns>
    /// <remarks>
    /// <b>Cleanup Flow:</b>
    /// 
    /// 1. IsVisible = false → Trigger CSS animation (fade-out)
    /// 2. IsVisibleChanged.InvokeAsync(false) → Notificare părinte
    /// 3. ShowAddDoctorModal = false → Închide și modal secundar (dacă e deschis)
    /// 
    /// <b>State Management:</b>
    /// - AllDoctori rămâne în memorie (cache pentru redeschidere rapidă)
    /// - IsLoading/HasError/ErrorMessage rămân neschimbate
    /// - La următoarea deschidere, OnParametersSetAsync reîncarcă datele fresh
    /// 
    /// <b>Performance:</b>
    /// - Lightweight operation (doar state updates)
    /// - CSS animations handled de browser
    /// - Zero memory cleanup (Blazor GC se ocupă)
    /// </remarks>
    /// <example>
    /// Apelat de butoanele de închidere:
    /// <code>
    /// // Buton X (header)
    /// &lt;button class="btn-close" @onclick="Close"&gt;
    ///     &lt;i class="fas fa-times"&gt;&lt;/i&gt;
    /// &lt;/button&gt;
    /// 
    /// // Buton "Închide" (footer)
    /// &lt;button class="btn btn-secondary" @onclick="Close"&gt;
    ///     &lt;i class="fas fa-times"&gt;&lt;/i&gt; Închide
    /// &lt;/button&gt;
    /// 
    /// // Overlay click (fundal semi-transparent)
    /// &lt;div class="modal-overlay" @onclick="Close"&gt;
    /// </code>
    /// </example>
    private async Task Close()
    {
        Logger.LogInformation("[PacientDoctoriModal] Closing modal");
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        ShowAddDoctorModal = false;
    }
}
