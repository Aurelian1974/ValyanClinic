using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using ValyanClinic.Application.Features.PacientManagement.Commands.CreatePacient;
using ValyanClinic.Application.Features.PacientManagement.Commands.UpdatePacient;
using ValyanClinic.Application.Features.PacientManagement.Queries.GetPacientById;
using ValyanClinic.Application.Features.PacientManagement.Queries.CheckDuplicatePacient;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Queries.GetDoctoriByPacient;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.DTOs;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.RemoveRelatie;
using ValyanClinic.Application.Features.PacientPersonalMedicalManagement.Commands.ActivateRelatie; // ✅ ADDED
using ValyanClinic.Application.Interfaces;
using ValyanClinic.Application.Services.Location;
using ValyanClinic.Services;

namespace ValyanClinic.Components.Pages.Pacienti.Modals;

// IDisposable pentru cleanup corect
public partial class PacientAddEditModal : ComponentBase, IDisposable
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<PacientAddEditModal> Logger { get; set; } = default!;
    [Inject] private INotificationService NotificationService { get; set; } = default!;
    [Inject] private IFieldPermissionService FieldPermissions { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private IJudeteService JudeteService { get; set; } = default!; // ✅ ADDED for loading judete from database
    [Inject] private IPhotonService PhotonService { get; set; } = default!; // ✅ ADDED for street autocomplete

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }
    [Parameter] public Guid? PacientId { get; set; }

    // Guard flag
    private bool _disposed = false;

    // State
    private bool IsEditMode => PacientId.HasValue && PacientId != Guid.Empty;
    private bool IsLoading { get; set; }
    private bool IsSaving { get; set; }
    private bool HasError { get; set; }
    private string? ErrorMessage { get; set; }
    private string ActiveTab { get; set; } = "personal";

    // Doctori Management
    private List<DoctorAsociatDto> DoctoriAsociati { get; set; } = new();
    private List<DoctorAsociatDto> DoctoriActivi => DoctoriAsociati.Where(d => d.EsteActiv).ToList();
    private List<DoctorAsociatDto> DoctoriInactivi => DoctoriAsociati.Where(d => !d.EsteActiv).ToList();
    private bool IsLoadingDoctori { get; set; }
    private bool ShowAddDoctorModal { get; set; }

    // Confirm modal state
    private bool ShowConfirmRemoveDoctor { get; set; }
    private DoctorAsociatDto? DoctorToRemove { get; set; }
    private bool ShowConfirmAddDoctors { get; set; }

    // ✅ ADDED: Confirm modal state pentru reactivare
    private bool ShowConfirmActivateDoctor { get; set; }
    private DoctorAsociatDto? DoctorToActivate { get; set; }

    // ✅ Permisiuni la nivel de câmp - încărcate din DB
    private bool _permissionsLoaded;
    
    // Form Model
    private PacientFormModel FormModel { get; set; } = new();

    #region Field Permission Helpers
    
    /// <summary>
    /// Verifică dacă un câmp poate fi editat (bazat pe permisiuni din DB).
    /// </summary>
    private bool CanEditField(string fieldName) => 
        FieldPermissions.CanEditField("Pacient", fieldName);
    
    /// <summary>
    /// Verifică dacă un câmp poate fi vizualizat.
    /// </summary>
    private bool CanViewField(string fieldName) => 
        FieldPermissions.CanViewField("Pacient", fieldName);
    
    /// <summary>
    /// Returnează starea unui câmp (Hidden, ReadOnly, Editable).
    /// </summary>
    private FieldState GetFieldState(string fieldName) => 
        FieldPermissions.GetFieldState("Pacient", fieldName, IsEditMode);
    
    /// <summary>
    /// Încarcă permisiunile din DB pentru rolul curent.
    /// </summary>
    private async Task LoadFieldPermissionsAsync()
    {
        if (_permissionsLoaded) return;
        
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var roleClaim = authState.User.FindFirst(ClaimTypes.Role);
            
            if (roleClaim != null)
            {
                await FieldPermissions.LoadPermissionsAsync(roleClaim.Value);
                _permissionsLoaded = true;
                Logger.LogDebug("Field permissions loaded for role: {Role}", roleClaim.Value);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load field permissions");
        }
    }
    
    #endregion

    // Dropdown Options
    private List<string> SexOptions { get; set; } = new() { "M", "F" };

    // Judete list - loaded from database via IJudeteService
    private List<string> JudeteList { get; set; } = new();
    private bool _judeteLoaded = false;

    // ✅ ADDED: Localitati list - loaded when judet is selected
    private List<string> LocalitatiList { get; set; } = new();
    private bool _isLoadingLocalitati = false;
    private string? _selectedJudet = null;

    // ✅ ADDED: Street autocomplete data (Photon API)
    private List<StreetOption> StradaOptions { get; set; } = new();
    private bool IsLoadingStrada { get; set; }
    private System.Timers.Timer? _debounceStradaTimer;
    private const int DebounceDelay = 400; // ms

    public class StreetOption
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? PostCode { get; set; }
    }

    // ✅ ADDED: Unsaved changes detection
    private string? _originalFormJson = null;
    private bool _hasUnsavedChanges => HasFormChanged();

    // ✅ ADDED: Tab validation state
    private Dictionary<string, bool> TabValidationState { get; set; } = new()
    {
        { "personal", true },
        { "contact", true },
        { "adresa", true },
        { "medical", true },
        { "asigurare", true },
        { "doctori", true }
    };

    // ✅ ADDED: Confirm close modal
    private bool ShowConfirmCloseModal { get; set; }

    // ✅ ADDED: Duplicate check result
    private bool _isDuplicateCheckPending = false;
    private string? _duplicateWarning = null;

    /// <summary>
    /// Checks if form has unsaved changes by comparing JSON serialization.
    /// </summary>
    private bool HasFormChanged()
    {
        if (string.IsNullOrEmpty(_originalFormJson)) return false;
        try
        {
            var currentJson = JsonSerializer.Serialize(FormModel);
            return currentJson != _originalFormJson;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Saves the original form state for change detection.
    /// </summary>
    private void SaveOriginalFormState()
    {
        try
        {
            _originalFormJson = JsonSerializer.Serialize(FormModel);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[PacientAddEditModal] Failed to serialize form for change detection");
        }
    }

    /// <summary>
    /// Loads judete from database using centralized IJudeteService.
    /// Results are cached in-memory for 1 hour for performance.
    /// </summary>
    private async Task LoadJudeteAsync()
    {
        if (_disposed || _judeteLoaded) return;

        try
        {
            Logger.LogDebug("[PacientAddEditModal] Loading judete from database...");
            
            var result = await JudeteService.GetJudeteAsync();
            
            if (_disposed) return;
            
            if (result.IsSuccess)
            {
                JudeteList = result.Value.Select(j => j.Nume).ToList();
                _judeteLoaded = true;
                Logger.LogDebug("[PacientAddEditModal] Loaded {Count} judete from database", JudeteList.Count);
            }
            else
            {
                Logger.LogWarning("[PacientAddEditModal] Failed to load judete: {Error}", result.FirstError);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PacientAddEditModal] Error loading judete");
        }
    }

    /// <summary>
    /// ✅ ADDED: Loads localitati from database when judet is selected.
    /// </summary>
    private async Task LoadLocalitatiAsync(string? judetName)
    {
        if (_disposed || string.IsNullOrEmpty(judetName)) 
        {
            LocalitatiList = new List<string>();
            return;
        }

        // Skip if same judet
        if (_selectedJudet == judetName && LocalitatiList.Any()) return;

        _selectedJudet = judetName;
        _isLoadingLocalitati = true;

        try
        {
            Logger.LogDebug("[PacientAddEditModal] Loading localitati for judet: {Judet}", judetName);
            
            var result = await JudeteService.GetLocalitatiByJudetNameAsync(judetName);
            
            if (_disposed) return;
            
            if (result.IsSuccess)
            {
                LocalitatiList = result.Value.Select(l => l.Nume).ToList();
                Logger.LogDebug("[PacientAddEditModal] Loaded {Count} localitati for {Judet}", LocalitatiList.Count, judetName);
            }
            else
            {
                LocalitatiList = new List<string>();
                Logger.LogWarning("[PacientAddEditModal] Failed to load localitati: {Error}", result.FirstError);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PacientAddEditModal] Error loading localitati");
            LocalitatiList = new List<string>();
        }
        finally
        {
            _isLoadingLocalitati = false;
        }
    }

    /// <summary>
    /// ✅ ADDED: Handles judet selection change and loads localitati.
    /// </summary>
    private async Task OnJudetChanged(string? newJudet)
    {
        if (_disposed) return;
        
        FormModel.Judet = newJudet;
        FormModel.Localitate = null; // Reset localitate when judet changes
        await LoadLocalitatiAsync(newJudet);
        ValidateTab("adresa");
        StateHasChanged();
    }

    #region Street Autocomplete (Photon API)

    /// <summary>
    /// Handler pentru input pe câmpul adresă (strada) - cu debounce.
    /// </summary>
    private void OnAdresaInput(ChangeEventArgs args)
    {
        var query = args.Value?.ToString() ?? string.Empty;
        FormModel.Adresa = query; // Update model value
        
        // Verifică dacă utilizatorul a selectat o stradă din listă (pentru a actualiza codul poștal)
        var selectedStreet = StradaOptions.FirstOrDefault(s => 
            s.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
        if (selectedStreet != null && !string.IsNullOrEmpty(selectedStreet.PostCode))
        {
            FormModel.Cod_Postal = selectedStreet.PostCode;
            Logger.LogInformation("Auto-filled PostCode: {PostCode}", selectedStreet.PostCode);
            InvokeAsync(StateHasChanged);
        }
        
        // Reset timer
        _debounceStradaTimer?.Stop();
        _debounceStradaTimer?.Dispose();
        
        if (query.Length < 3)
        {
            StradaOptions.Clear();
            InvokeAsync(StateHasChanged);
            return;
        }
        
        // Start debounce timer
        _debounceStradaTimer = new System.Timers.Timer(DebounceDelay);
        _debounceStradaTimer.Elapsed += async (s, e) =>
        {
            _debounceStradaTimer?.Stop();
            await SearchStrada(query);
        };
        _debounceStradaTimer.AutoReset = false;
        _debounceStradaTimer.Start();
    }

    /// <summary>
    /// Caută străzi folosind Photon API.
    /// </summary>
    private async Task SearchStrada(string query)
    {
        try
        {
            await InvokeAsync(() =>
            {
                IsLoadingStrada = true;
                StateHasChanged();
            });

            // Adăugăm orașul și județul curent pentru rezultate mai precise
            var searchQuery = query;
            if (!string.IsNullOrEmpty(FormModel.Localitate))
            {
                searchQuery += $" {FormModel.Localitate}";
            }
            if (!string.IsNullOrEmpty(FormModel.Judet))
            {
                searchQuery += $" {FormModel.Judet}";
            }

            Logger.LogInformation("Photon search: '{Query}'", searchQuery);

            var result = await PhotonService.SearchStreetsAsync(searchQuery, 10, CancellationToken.None);

            await InvokeAsync(() =>
            {
                if (result.IsSuccess && result.Value != null)
                {
                    var allStreets = result.Value
                        .Select(s => new StreetOption
                        {
                            Name = s.Name,
                            DisplayName = s.DisplayName,
                            City = s.City,
                            PostCode = s.PostCode
                        })
                        .ToList();
                    
                    // Filtrăm după localitatea selectată (dacă există)
                    if (!string.IsNullOrEmpty(FormModel.Localitate))
                    {
                        var localitate = FormModel.Localitate.ToLowerInvariant();
                        StradaOptions = allStreets
                            .Where(s => !string.IsNullOrEmpty(s.City) && 
                                       s.City.ToLowerInvariant().Contains(localitate))
                            .ToList();
                        
                        // Dacă nu găsim nimic în orașul selectat, arătăm toate rezultatele
                        if (StradaOptions.Count == 0)
                        {
                            StradaOptions = allStreets;
                        }
                    }
                    else
                    {
                        StradaOptions = allStreets;
                    }

                    Logger.LogInformation("Photon returned {Count} streets", StradaOptions.Count);
                }
                else
                {
                    StradaOptions.Clear();
                    Logger.LogWarning("Photon search failed: {Error}", result.FirstError);
                }

                IsLoadingStrada = false;
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching streets");
            await InvokeAsync(() =>
            {
                StradaOptions.Clear();
                IsLoadingStrada = false;
                StateHasChanged();
            });
        }
    }

    #endregion

    #region CNP Auto-Parse

    /// <summary>
    /// ✅ ADDED: Parses CNP and auto-fills Data Nasterii and Sex.
    /// Romanian CNP format: SAALLZZJJNNNC
    /// S = Sex (1,2 = M/F born 1900-1999; 5,6 = M/F born 2000+)
    /// AA = Year, LL = Month, ZZ = Day
    /// JJ = County code, NNN = Sequential number, C = Checksum
    /// </summary>
    private void ParseCNPAndFillFields()
    {
        if (string.IsNullOrEmpty(FormModel.CNP) || FormModel.CNP.Length != 13) return;

        try
        {
            var cnp = FormModel.CNP;
            
            // Extract sex from first digit
            var sexDigit = int.Parse(cnp[0].ToString());
            FormModel.Sex = (sexDigit % 2 == 1) ? "M" : "F";

            // Extract birth date
            int century = sexDigit switch
            {
                1 or 2 => 1900,
                3 or 4 => 1800,
                5 or 6 => 2000,
                7 or 8 => 2000, // Rezident
                _ => 1900
            };

            int year = century + int.Parse(cnp.Substring(1, 2));
            int month = int.Parse(cnp.Substring(3, 2));
            int day = int.Parse(cnp.Substring(5, 2));

            // Validate date
            if (month >= 1 && month <= 12 && day >= 1 && day <= 31)
            {
                try
                {
                    FormModel.Data_Nasterii = new DateTime(year, month, day);
                    Logger.LogDebug("[PacientAddEditModal] CNP parsed: Sex={Sex}, BirthDate={Date}", 
                        FormModel.Sex, FormModel.Data_Nasterii.ToString("dd.MM.yyyy"));
                }
                catch
                {
                    // Invalid date combination, ignore
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[PacientAddEditModal] Failed to parse CNP: {CNP}", FormModel.CNP);
        }
    }

    /// <summary>
    /// ✅ ADDED: Handles CNP input change with debouncing.
    /// </summary>
    private void OnCNPChanged(string? newCnp)
    {
        FormModel.CNP = newCnp;
        
        if (!string.IsNullOrEmpty(newCnp) && newCnp.Length == 13)
        {
            ParseCNPAndFillFields();
            StateHasChanged();
        }
        
        ValidateTab("personal");
    }

    #endregion

    #region Tab Validation

    /// <summary>
    /// ✅ ADDED: Validates fields in a specific tab and updates state.
    /// </summary>
    private void ValidateTab(string tabName)
    {
        bool isValid = tabName switch
        {
            "personal" => !string.IsNullOrWhiteSpace(FormModel.Nume) && 
                          !string.IsNullOrWhiteSpace(FormModel.Prenume) &&
                          !string.IsNullOrWhiteSpace(FormModel.Sex),
            "contact" => true, // All optional
            "adresa" => true, // All optional
            "medical" => true, // All optional
            "asigurare" => !FormModel.Asigurat || 
                           (!string.IsNullOrEmpty(FormModel.CNP_Asigurat) || !string.IsNullOrEmpty(FormModel.Nr_Card_Sanatate)),
            "doctori" => true,
            _ => true
        };

        TabValidationState[tabName] = isValid;
    }

    /// <summary>
    /// ✅ ADDED: Validates all tabs and returns overall validity.
    /// </summary>
    private bool ValidateAllTabs()
    {
        ValidateTab("personal");
        ValidateTab("contact");
        ValidateTab("adresa");
        ValidateTab("medical");
        ValidateTab("asigurare");
        
        return TabValidationState.Values.All(v => v);
    }

    /// <summary>
    /// ✅ ADDED: Gets tab validation icon class.
    /// </summary>
    private string GetTabValidationClass(string tabName)
    {
        if (!TabValidationState.ContainsKey(tabName)) return "";
        return TabValidationState[tabName] ? "tab-valid" : "tab-invalid";
    }

    #endregion

    #region Duplicate Detection

    /// <summary>
    /// ✅ ADDED: Checks if a patient with same CNP or Name+BirthDate exists.
    /// </summary>
    private async Task<bool> CheckForDuplicatesAsync()
    {
        if (_disposed) return false;

        _isDuplicateCheckPending = true;
        _duplicateWarning = null;

        try
        {
            // Check by CNP if provided
            if (!string.IsNullOrEmpty(FormModel.CNP))
            {
                var query = new CheckDuplicatePacientQuery
                {
                    CNP = FormModel.CNP,
                    ExcludeId = IsEditMode ? PacientId : null
                };

                var result = await Mediator.Send(query);
                
                if (result.IsSuccess && result.Value.HasDuplicate)
                {
                    _duplicateWarning = $"⚠️ Există deja un pacient cu CNP {FormModel.CNP}: {result.Value.DuplicatePacientName}";
                    return true;
                }
            }

            // Check by Name + BirthDate
            if (!string.IsNullOrEmpty(FormModel.Nume) && !string.IsNullOrEmpty(FormModel.Prenume))
            {
                var query = new CheckDuplicatePacientQuery
                {
                    Nume = FormModel.Nume,
                    Prenume = FormModel.Prenume,
                    DataNasterii = FormModel.Data_Nasterii,
                    ExcludeId = IsEditMode ? PacientId : null
                };

                var result = await Mediator.Send(query);
                
                if (result.IsSuccess && result.Value.HasDuplicate)
                {
                    _duplicateWarning = $"⚠️ Există deja un pacient similar: {result.Value.DuplicatePacientName}";
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[PacientAddEditModal] Error checking duplicates");
            return false; // Continue with save if check fails
        }
        finally
        {
            _isDuplicateCheckPending = false;
        }
    }

    #endregion

    #region Keyboard Navigation

    /// <summary>
    /// ✅ ADDED: Handles keyboard shortcuts (Ctrl+S for save, Escape for close).
    /// </summary>
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (_disposed) return;

        // Ctrl+S = Save
        if (e.CtrlKey && e.Key == "s")
        {
            await HandleSubmit();
        }
        // Escape = Try to close
        else if (e.Key == "Escape")
        {
            await TryClose();
        }
    }

    /// <summary>
    /// ✅ ADDED: Attempts to close with unsaved changes check.
    /// </summary>
    private async Task TryClose()
    {
        if (_hasUnsavedChanges)
        {
            ShowConfirmCloseModal = true;
        }
        else
        {
            await Close();
        }
    }

    /// <summary>
    /// ✅ ADDED: Confirms closing and discards changes.
    /// </summary>
    private async Task ConfirmCloseAndDiscard()
    {
        ShowConfirmCloseModal = false;
        await Close();
    }

    /// <summary>
    /// ✅ ADDED: Cancels close operation.
    /// </summary>
    private void CancelClose()
    {
        ShowConfirmCloseModal = false;
    }

    #endregion

    // Dispose pentru cleanup corect
    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            Logger.LogDebug("[PacientAddEditModal] Disposing - Starting cleanup");

            // Setează flag IMEDIAT
            _disposed = true;

            // Cleanup timers
            _debounceStradaTimer?.Stop();
            _debounceStradaTimer?.Dispose();
            _debounceStradaTimer = null;

            // Clear toate listele pentru a elibera memoria
            DoctoriAsociati?.Clear();
            DoctoriAsociati = new();
            JudeteList?.Clear();
            SexOptions?.Clear();
            StradaOptions?.Clear();
            LocalitatiList?.Clear();

            // Reset state flags
            ShowAddDoctorModal = false;
            ShowConfirmRemoveDoctor = false;
            ShowConfirmAddDoctors = false;
            DoctorToRemove = null;

            Logger.LogDebug("[PacientAddEditModal] Dispose complete");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PacientAddEditModal] Error during dispose");
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_disposed) return; // ADDED: Guard check

        if (IsVisible)
        {
            // ✅ Încarcă permisiunile la nivel de câmp când modalul devine vizibil
            await LoadFieldPermissionsAsync();
            
            // ✅ Încarcă județele din baza de date
            await LoadJudeteAsync();
            
            if (IsEditMode)
            {
                await LoadPacientData();
                await LoadDoctoriAsociati();
                
                // ✅ ADDED: Load localitati if judet is set
                if (!string.IsNullOrEmpty(FormModel.Judet))
                {
                    await LoadLocalitatiAsync(FormModel.Judet);
                }
                
                // ✅ ADDED: Save original state for change detection
                SaveOriginalFormState();
            }
            else
            {
                ResetForm();
                // ✅ ADDED: Save original state for new form
                SaveOriginalFormState();
            }
            
            // ✅ ADDED: Validate all tabs initially
            ValidateAllTabs();
        }
    }

    private async Task LoadPacientData()
    {
        if (_disposed) return; // ADDED: Guard check

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var query = new GetPacientByIdQuery(PacientId!.Value);
            var result = await Mediator.Send(query);

            if (_disposed) return; // ADDED: Check after async

            if (result.IsSuccess && result.Value != null)
            {
                var pacient = result.Value;
                
                // Separă adresa în stradă și număr (dacă există)
                string? strada = pacient.Adresa;
                string? numar = null;
                
                if (!string.IsNullOrWhiteSpace(pacient.Adresa))
                {
                    // Încearcă să găsească numărul la sfârșit (ex: "Str. Zorilor 123A")
                    var match = System.Text.RegularExpressions.Regex.Match(
                        pacient.Adresa, 
                        @"^(.+?)\s+(\d+[A-Za-z]?|[A-Za-z]\d+)\s*$"
                    );
                    
                    if (match.Success)
                    {
                        strada = match.Groups[1].Value.Trim();
                        numar = match.Groups[2].Value.Trim();
                    }
                }
                
                FormModel = new PacientFormModel
                {
                    Nume = pacient.Nume,
                    Prenume = pacient.Prenume,
                    CNP = pacient.CNP,
                    Cod_Pacient = pacient.Cod_Pacient,
                    Data_Nasterii = pacient.Data_Nasterii,
                    Sex = pacient.Sex,
                    Telefon = pacient.Telefon,
                    Telefon_Secundar = pacient.Telefon_Secundar,
                    Email = pacient.Email,
                    Judet = pacient.Judet,
                    Localitate = pacient.Localitate,
                    Adresa = strada,
                    Numar = numar,
                    Cod_Postal = pacient.Cod_Postal,
                    Asigurat = pacient.Asigurat,
                    CNP_Asigurat = pacient.CNP_Asigurat,
                    Nr_Card_Sanatate = pacient.Nr_Card_Sanatate,
                    Casa_Asigurari = pacient.Casa_Asigurari,
                    Alergii = pacient.Alergii,
                    Boli_Cronice = pacient.Boli_Cronice,
                    Medic_Familie = pacient.Medic_Familie,
                    Persoana_Contact = pacient.Persoana_Contact,
                    Telefon_Urgenta = pacient.Telefon_Urgenta,
                    Relatie_Contact = pacient.Relatie_Contact,
                    Activ = pacient.Activ,
                    Observatii = pacient.Observatii
                };
            }
            else
            {
                HasError = true;
                ErrorMessage = result.FirstError ?? "Eroare la încărcarea datelor pacientului.";
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed while loading data");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                HasError = true;
                ErrorMessage = $"Eroare: {ex.Message}";
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsLoading = false;
            }
        }
    }

    private async Task LoadDoctoriAsociati()
    {
        if (_disposed || !IsEditMode) return; // ADDED: Guard check

        IsLoadingDoctori = true;

        try
        {
            Logger.LogInformation("[PacientAddEditModal] Loading doctori for PacientID: {PacientId}", PacientId);

            var query = new GetDoctoriByPacientQuery(PacientId!.Value, ApenumereActivi: false);
            var result = await Mediator.Send(query);

            if (_disposed) return; // ADDED: Check after async

            if (result.IsSuccess && result.Value != null)
            {
                DoctoriAsociati = result.Value;
                Logger.LogInformation("[PacientAddEditModal] Loaded {Count} doctori", DoctoriAsociati.Count);
            }
            else
            {
                DoctoriAsociati = new List<DoctorAsociatDto>();
                Logger.LogWarning("[PacientAddEditModal] Failed to load doctori: {Error}", result.FirstError);
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed while loading doctori");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "[PacientAddEditModal] Exception loading doctori");
                DoctoriAsociati = new List<DoctorAsociatDto>();
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsLoadingDoctori = false;
            }
        }
    }

    private async Task OpenAddDoctorModal()
    {
        if (_disposed) return; // ADDED: Guard check

        if (!IsEditMode)
        {
            await NotificationService.ShowWarningAsync(
         "Vă rugăm să salvați mai întâi pacientul înainte de a adăuga doctori.",
               "Atenție");
            return;
        }

        Logger.LogInformation("[PacientAddEditModal] Opening AddDoctorModal");
        ShowAddDoctorModal = true;
    }

    private async Task OnDoctorAdded()
    {
        if (_disposed) return; // ADDED: Guard check

        Logger.LogInformation("[PacientAddEditModal] Doctor added - reloading list");
        ShowAddDoctorModal = false;
        await LoadDoctoriAsociati();
        StateHasChanged();
    }

    private void RemoveDoctor(DoctorAsociatDto doctor)
    {
        if (_disposed) return; // ADDED: Guard check

        DoctorToRemove = doctor;
        ShowConfirmRemoveDoctor = true;
    }

    // ✅ ADDED: Metodă pentru reactivare relație
    private void ActivateDoctor(DoctorAsociatDto doctor)
    {
        if (_disposed) return;

        DoctorToActivate = doctor;
        ShowConfirmActivateDoctor = true;
    }

    private async Task HandleRemoveDoctorConfirmed()
    {
        if (_disposed || DoctorToRemove == null) return; // ADDED: Guard check

        ShowConfirmRemoveDoctor = false;

        try
        {
            Logger.LogInformation("[PacientAddEditModal] Removing doctor: {DoctorName}, RelatieID: {RelatieID}",
              DoctorToRemove.DoctorNumeComplet, DoctorToRemove.RelatieID);

            var command = new RemoveRelatieCommand(RelatieID: DoctorToRemove.RelatieID);
            var result = await Mediator.Send(command);

            if (_disposed) return; // ADDED: Check after async

            if (result.IsSuccess)
            {
                await LoadDoctoriAsociati();
                await NotificationService.ShowSuccessAsync("Relație dezactivată cu succes!");
            }
            else
            {
                await NotificationService.ShowErrorAsync(result.FirstError ?? "Eroare la dezactivare");
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed during remove operation");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "[PacientAddEditModal] Error removing doctor");
                await NotificationService.ShowErrorAsync(ex.Message, "Eroare");
            }
        }
        finally
        {
            if (!_disposed)
            {
                DoctorToRemove = null;
            }
        }
    }

    // ✅ ADDED: Handler pentru confirmare reactivare
    private async Task HandleActivateDoctorConfirmed()
    {
        if (_disposed || DoctorToActivate == null) return;

        ShowConfirmActivateDoctor = false;

        try
        {
            Logger.LogInformation("[PacientAddEditModal] Activating doctor: {DoctorName}, RelatieID: {RelatieID}",
    DoctorToActivate.DoctorNumeComplet, DoctorToActivate.RelatieID);

            var command = new ActivateRelatieCommand(
     RelatieID: DoctorToActivate.RelatieID,
       Observatii: "Relație reactivată din interfața pacient",
           Motiv: "Reluarea tratamentului cu acest doctor",
      ModificatDe: "System");

            var result = await Mediator.Send(command);

            if (_disposed) return;

            if (result.IsSuccess)
            {
                await LoadDoctoriAsociati();
                await NotificationService.ShowSuccessAsync(
        result.SuccessMessage ?? "Relație reactivată cu succes!");
            }
            else
            {
                await NotificationService.ShowErrorAsync(
                 result.FirstError ?? "Eroare la reactivare");
            }
        }
        catch (ObjectDisposedException)
        {
            Logger.LogDebug("[PacientAddEditModal] Component disposed during activate operation");
        }
        catch (Exception ex)
        {
            if (!_disposed)
            {
                Logger.LogError(ex, "[PacientAddEditModal] Error activating doctor");
                await NotificationService.ShowErrorAsync(ex.Message, "Eroare");
            }
        }
        finally
        {
            if (!_disposed)
            {
                DoctorToActivate = null;
            }
        }
    }

    private string GetBadgeClass(string? tipRelatie)
    {
        return tipRelatie switch
        {
            "MedicPrimar" => "badge-primary",
            "Specialist" => "badge-info",
            "MedicConsultant" => "badge-success",
            "MedicDeGarda" => "badge-warning",
            "MedicFamilie" => "badge-secondary",
            _ => "badge-secondary"
        };
    }

    private string FormatZile(int zile)
    {
        if (zile < 30)
            return $"{zile} zile";
        if (zile < 365)
            return $"{zile / 30} luni";
        return $"{zile / 365} ani";
    }

    private void ResetForm()
    {
        if (_disposed) return; // ADDED: Guard check

        FormModel = new PacientFormModel
        {
            Data_Nasterii = DateTime.Now.AddYears(-30),
            Activ = true,
            Asigurat = false
        };
        ActiveTab = "personal";
        HasError = false;
        ErrorMessage = null;
        DoctoriAsociati = new();
        ShowAddDoctorModal = false;
    }

    private void SetActiveTab(string tab)
    {
        if (_disposed) return; // ADDED: Guard check

        ActiveTab = tab;
        ValidateTab(tab); // ✅ ADDED: Validate when switching tabs
    }

    private async Task HandleSubmit()
    {
        if (_disposed) return; // ADDED: Guard check

        // ✅ ADDED: Validate all tabs first
        if (!ValidateAllTabs())
        {
            // Find first invalid tab and switch to it
            var firstInvalidTab = TabValidationState.FirstOrDefault(t => !t.Value).Key;
            if (!string.IsNullOrEmpty(firstInvalidTab))
            {
                ActiveTab = firstInvalidTab;
                await NotificationService.ShowWarningAsync(
                    "Vă rugăm să completați toate câmpurile obligatorii.", "Validare");
            }
            return;
        }

        IsSaving = true;
        HasError = false;
        ErrorMessage = null;
        _duplicateWarning = null;

        try
        {
            // ✅ ADDED: Check for duplicates before saving (only for new patients or CNP change)
            if (!IsEditMode || (_originalFormJson != null && HasFormChanged()))
            {
                var hasDuplicate = await CheckForDuplicatesAsync();
                if (hasDuplicate && !string.IsNullOrEmpty(_duplicateWarning))
                {
                    // Show warning but allow user to proceed
                    var confirmed = await JSRuntime.InvokeAsync<bool>(
                        "confirm", 
                        $"{_duplicateWarning}\n\nDoriți să continuați salvarea?");
                    
                    if (!confirmed)
                    {
                        IsSaving = false;
                        return;
                    }
                }
            }

            if (IsEditMode)
            {
                await UpdatePacient();
            }
            else
            {
                await CreatePacient();
            }
        }
        finally
        {
            if (!_disposed)
            {
                IsSaving = false;
            }
        }
    }

    private async Task CreatePacient()
    {
        if (_disposed) return; // ADDED: Guard check

        // Concatenează strada și numărul pentru salvare
        var adresaCompleta = string.IsNullOrWhiteSpace(FormModel.Adresa) 
            ? null 
            : string.IsNullOrWhiteSpace(FormModel.Numar)
                ? FormModel.Adresa.Trim()
                : $"{FormModel.Adresa.Trim()} {FormModel.Numar.Trim()}";

        var command = new CreatePacientCommand
        {
            Nume = FormModel.Nume,
            Prenume = FormModel.Prenume,
            CNP = FormModel.CNP,
            Cod_Pacient = FormModel.Cod_Pacient,
            Data_Nasterii = FormModel.Data_Nasterii,
            Sex = FormModel.Sex,
            Telefon = FormModel.Telefon,
            Telefon_Secundar = FormModel.Telefon_Secundar,
            Email = FormModel.Email,
            Judet = FormModel.Judet,
            Localitate = FormModel.Localitate,
            Adresa = adresaCompleta,
            Cod_Postal = FormModel.Cod_Postal,
            Asigurat = FormModel.Asigurat,
            CNP_Asigurat = FormModel.CNP_Asigurat,
            Nr_Card_Sanatate = FormModel.Nr_Card_Sanatate,
            Casa_Asigurari = FormModel.Casa_Asigurari,
            Alergii = FormModel.Alergii,
            Boli_Cronice = FormModel.Boli_Cronice,
            Medic_Familie = FormModel.Medic_Familie,
            Persoana_Contact = FormModel.Persoana_Contact,
            Telefon_Urgenta = FormModel.Telefon_Urgenta,
            Relatie_Contact = FormModel.Relatie_Contact,
            Activ = FormModel.Activ,
            Observatii = FormModel.Observatii,
            CreatDe = "System"
        };

        var result = await Mediator.Send(command);

        if (_disposed) return; // ADDED: Check after async

        if (result.IsSuccess)
        {
            Logger.LogInformation("[PacientAddEditModal] Pacient created successfully with ID: {PacientId}", result.Value);

            PacientId = result.Value;
            ShowConfirmAddDoctors = true;
        }
        else
        {
            HasError = true;
            ErrorMessage = string.Join("\n", result.Errors);
            await NotificationService.ShowErrorAsync(ErrorMessage, "Eroare la salvare");
        }
    }

    private async Task HandleAddDoctorsConfirmed()
    {
        if (_disposed) return; // ADDED: Guard check

        ShowConfirmAddDoctors = false;

        await LoadPacientData();
        await LoadDoctoriAsociati();

        ActiveTab = "doctori";
        StateHasChanged();

        await Task.Delay(300);
        OpenAddDoctorModal();
    }

    private async Task HandleAddDoctorsDeclined()
    {
        if (_disposed) return; // ADDED: Guard check

        ShowConfirmAddDoctors = false;

        await NotificationService.ShowSuccessAsync("Pacient creat cu succes!");
        await Close();
        await OnSaved.InvokeAsync();
    }

    private async Task UpdatePacient()
    {
        if (_disposed) return; // ADDED: Guard check

        // Concatenează strada și numărul pentru salvare
        var adresaCompleta = string.IsNullOrWhiteSpace(FormModel.Adresa) 
            ? null 
            : string.IsNullOrWhiteSpace(FormModel.Numar)
                ? FormModel.Adresa.Trim()
                : $"{FormModel.Adresa.Trim()} {FormModel.Numar.Trim()}";

        var command = new UpdatePacientCommand
        {
            Id = PacientId!.Value,
            Nume = FormModel.Nume,
            Prenume = FormModel.Prenume,
            CNP = FormModel.CNP,
            Data_Nasterii = FormModel.Data_Nasterii,
            Sex = FormModel.Sex,
            Telefon = FormModel.Telefon,
            Telefon_Secundar = FormModel.Telefon_Secundar,
            Email = FormModel.Email,
            Judet = FormModel.Judet,
            Localitate = FormModel.Localitate,
            Adresa = adresaCompleta,
            Cod_Postal = FormModel.Cod_Postal,
            Asigurat = FormModel.Asigurat,
            CNP_Asigurat = FormModel.CNP_Asigurat,
            Nr_Card_Sanatate = FormModel.Nr_Card_Sanatate,
            Casa_Asigurari = FormModel.Casa_Asigurari,
            Alergii = FormModel.Alergii,
            Boli_Cronice = FormModel.Boli_Cronice,
            Medic_Familie = FormModel.Medic_Familie,
            Persoana_Contact = FormModel.Persoana_Contact,
            Telefon_Urgenta = FormModel.Telefon_Urgenta,
            Relatie_Contact = FormModel.Relatie_Contact,
            Activ = FormModel.Activ,
            Observatii = FormModel.Observatii,
            ModificatDe = "System"
        };

        var result = await Mediator.Send(command);

        if (_disposed) return; // ADDED: Check after async

        if (result.IsSuccess)
        {
            await NotificationService.ShowSuccessAsync(
          result.SuccessMessage ?? "Pacient actualizat cu succes!");
            await Close();
            await OnSaved.InvokeAsync();
        }
        else
        {
            HasError = true;
            ErrorMessage = string.Join("\n", result.Errors);
            await NotificationService.ShowErrorAsync(ErrorMessage, "Eroare la salvare");
        }
    }

    private async Task HandleOverlayClick()
    {
        if (_disposed) return; // ADDED: Guard check

        // Pentru a proteja datele introduse, modalul nu se închide la click pe overlay
        return;
    }

    private async Task Close()
    {
        if (_disposed) return; // ADDED: Guard check

        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        ResetForm();
    }

    // Form Model Class
    public class PacientFormModel
    {
        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele nu poate depăși 100 de caractere")]
        public string Nume { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prenumele este obligatoriu")]
        [StringLength(100, ErrorMessage = "Prenumele nu poate depăși 100 de caractere")]
        public string Prenume { get; set; } = string.Empty;

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie să conțină exact 13 cifre")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNP-ul trebuie să conțină doar cifre")]
        public string? CNP { get; set; }

        public string? Cod_Pacient { get; set; }

        [Required(ErrorMessage = "Data nașterii este obligatorie")]
        public DateTime Data_Nasterii { get; set; }

        [Required(ErrorMessage = "Sexul este obligatoriu")]
        public string Sex { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon { get; set; }

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon_Secundar { get; set; }

        [EmailAddress(ErrorMessage = "Format email invalid")]
        public string? Email { get; set; }

        public string? Judet { get; set; }
        public string? Localitate { get; set; }
        public string? Adresa { get; set; }
        
        [StringLength(20, ErrorMessage = "Numărul nu poate depăși 20 de caractere")]
        public string? Numar { get; set; }

        [StringLength(6, ErrorMessage = "Codul poștal nu poate depăși 6 caractere")]
        public string? Cod_Postal { get; set; }

        public bool Asigurat { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP Asigurat trebuie să conțină exact 13 cifre")]
        public string? CNP_Asigurat { get; set; }

        public string? Nr_Card_Sanatate { get; set; }
        public string? Casa_Asigurari { get; set; }
        public string? Alergii { get; set; }
        public string? Boli_Cronice { get; set; }
        public string? Medic_Familie { get; set; }
        public string? Persoana_Contact { get; set; }

        [Phone(ErrorMessage = "Format telefon invalid")]
        public string? Telefon_Urgenta { get; set; }

        public string? Relatie_Contact { get; set; }
        public bool Activ { get; set; }
        public string? Observatii { get; set; }
    }
}
