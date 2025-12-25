namespace ValyanClinic.Application.Authorization;

/// <summary>
/// Definește toate permisiunile granulare din sistem.
/// Folosite pentru Policy-Based Authorization.
/// 
/// Naming Convention: {Entity}.{Action} sau {Entity}.{Action}.{Field}
/// Exemplu: Pacient.Edit, Pacient.Edit.CNP, Pacient.View.DateMedicale
/// </summary>
public static class Permissions
{
    #region Pacient Permissions
    
    /// <summary>Permisiuni pentru gestiunea pacienților</summary>
    public static class Pacient
    {
        // Permisiuni de bază
        public const string View = "Pacient.View";
        public const string Create = "Pacient.Create";
        public const string Edit = "Pacient.Edit";
        public const string Delete = "Pacient.Delete";
        public const string ViewSensitiveData = "Pacient.ViewSensitiveData"; // CNP, date medicale
        public const string Export = "Pacient.Export";
        
        /// <summary>Permisiuni granulare pentru vizualizare câmpuri</summary>
        public static class ViewField
        {
            // Date personale
            public const string CNP = "Pacient.View.CNP";
            public const string DataNasterii = "Pacient.View.DataNasterii";
            public const string Sex = "Pacient.View.Sex";
            
            // Contact
            public const string Telefon = "Pacient.View.Telefon";
            public const string Email = "Pacient.View.Email";
            public const string ContactUrgenta = "Pacient.View.ContactUrgenta";
            
            // Adresa
            public const string Adresa = "Pacient.View.Adresa";
            
            // Date medicale (sensibile)
            public const string Alergii = "Pacient.View.Alergii";
            public const string BoliCronice = "Pacient.View.BoliCronice";
            public const string GrupaSanguina = "Pacient.View.GrupaSanguina";
            public const string Greutate = "Pacient.View.Greutate";
            public const string Inaltime = "Pacient.View.Inaltime";
            public const string Observatii = "Pacient.View.Observatii";
            
            // Asigurare
            public const string Asigurare = "Pacient.View.Asigurare";
            public const string CAS = "Pacient.View.CAS";
        }
        
        /// <summary>Permisiuni granulare pentru editare câmpuri</summary>
        public static class EditField
        {
            // Date personale
            public const string CNP = "Pacient.Edit.CNP";
            public const string Nume = "Pacient.Edit.Nume";
            public const string Prenume = "Pacient.Edit.Prenume";
            public const string DataNasterii = "Pacient.Edit.DataNasterii";
            public const string Sex = "Pacient.Edit.Sex";
            public const string Activ = "Pacient.Edit.Activ";
            
            // Contact
            public const string Telefon = "Pacient.Edit.Telefon";
            public const string TelefonSecundar = "Pacient.Edit.TelefonSecundar";
            public const string Email = "Pacient.Edit.Email";
            public const string PersoanaContact = "Pacient.Edit.PersoanaContact";
            public const string RelatieContact = "Pacient.Edit.RelatieContact";
            public const string TelefonUrgenta = "Pacient.Edit.TelefonUrgenta";
            
            // Adresa
            public const string Adresa = "Pacient.Edit.Adresa";
            public const string Localitate = "Pacient.Edit.Localitate";
            public const string Judet = "Pacient.Edit.Judet";
            public const string CodPostal = "Pacient.Edit.CodPostal";
            
            // Date medicale (doar medical staff)
            public const string Alergii = "Pacient.Edit.Alergii";
            public const string BoliCronice = "Pacient.Edit.BoliCronice";
            public const string GrupaSanguina = "Pacient.Edit.GrupaSanguina";
            public const string Greutate = "Pacient.Edit.Greutate";
            public const string Inaltime = "Pacient.Edit.Inaltime";
            public const string Observatii = "Pacient.Edit.Observatii";
            
            // Asigurare
            public const string TipAsigurare = "Pacient.Edit.TipAsigurare";
            public const string NumarAsigurare = "Pacient.Edit.NumarAsigurare";
            public const string CasJudetean = "Pacient.Edit.CasJudetean";
            public const string DataExpirareAsigurare = "Pacient.Edit.DataExpirareAsigurare";
        }
    }
    
    #endregion
    
    #region Consultatie Permissions
    
    /// <summary>Permisiuni pentru consultații medicale</summary>
    public static class Consultatie
    {
        // Permisiuni de bază
        public const string View = "Consultatie.View";
        public const string ViewOwn = "Consultatie.ViewOwn"; // Doar consultațiile proprii
        public const string ViewDepartment = "Consultatie.ViewDepartment"; // Consultațiile din departament
        public const string Create = "Consultatie.Create";
        public const string Edit = "Consultatie.Edit";
        public const string EditOwn = "Consultatie.EditOwn"; // Doar consultațiile proprii
        public const string Delete = "Consultatie.Delete";
        public const string Finalize = "Consultatie.Finalize"; // Finalizare consultație
        public const string Prescribe = "Consultatie.Prescribe"; // Prescrie medicamente
        
        /// <summary>Permisiuni granulare pentru editare câmpuri consultație</summary>
        public static class EditField
        {
            public const string Diagnostic = "Consultatie.Edit.Diagnostic";
            public const string Simptome = "Consultatie.Edit.Simptome";
            public const string ExamenClinic = "Consultatie.Edit.ExamenClinic";
            public const string Tratament = "Consultatie.Edit.Tratament";
            public const string Recomandari = "Consultatie.Edit.Recomandari";
            public const string Reteta = "Consultatie.Edit.Reteta";
            public const string Investigatii = "Consultatie.Edit.Investigatii";
            public const string DataControl = "Consultatie.Edit.DataControl";
            public const string TipConsultatie = "Consultatie.Edit.TipConsultatie";
        }
    }
    
    #endregion
    
    #region Programare Permissions
    
    /// <summary>Permisiuni pentru programări</summary>
    public static class Programare
    {
        public const string View = "Programare.View";
        public const string Create = "Programare.Create";
        public const string Edit = "Programare.Edit";
        public const string Delete = "Programare.Delete";
        public const string Cancel = "Programare.Cancel";
        public const string Confirm = "Programare.Confirm";
        
        /// <summary>Permisiuni granulare pentru editare câmpuri programare</summary>
        public static class EditField
        {
            public const string Data = "Programare.Edit.Data";
            public const string Ora = "Programare.Edit.Ora";
            public const string Durata = "Programare.Edit.Durata";
            public const string Doctor = "Programare.Edit.Doctor";
            public const string TipProgramare = "Programare.Edit.TipProgramare";
            public const string Motiv = "Programare.Edit.Motiv";
            public const string Observatii = "Programare.Edit.Observatii";
            public const string Status = "Programare.Edit.Status";
        }
    }
    
    #endregion
    
    #region Personal Permissions
    
    /// <summary>Permisiuni pentru gestiunea personalului</summary>
    public static class Personal
    {
        public const string View = "Personal.View";
        public const string Create = "Personal.Create";
        public const string Edit = "Personal.Edit";
        public const string Delete = "Personal.Delete";
        public const string ManageRoles = "Personal.ManageRoles";
        
        /// <summary>Permisiuni granulare pentru editare câmpuri personal</summary>
        public static class EditField
        {
            public const string CNP = "Personal.Edit.CNP";
            public const string Nume = "Personal.Edit.Nume";
            public const string Prenume = "Personal.Edit.Prenume";
            public const string Email = "Personal.Edit.Email";
            public const string Telefon = "Personal.Edit.Telefon";
            public const string Specializare = "Personal.Edit.Specializare";
            public const string Departament = "Personal.Edit.Departament";
            public const string CodParafa = "Personal.Edit.CodParafa";
            public const string Program = "Personal.Edit.Program";
            public const string Salariu = "Personal.Edit.Salariu";
        }
    }
    
    #endregion
    
    #region Administration Permissions
    
    /// <summary>Permisiuni pentru administrare sistem</summary>
    public static class Admin
    {
        public const string AccessDashboard = "Admin.AccessDashboard";
        public const string ViewAuditLog = "Admin.ViewAuditLog";
        public const string ManageSettings = "Admin.ManageSettings";
        public const string ManageUsers = "Admin.ManageUsers";
        public const string ViewReports = "Admin.ViewReports";
        public const string ExportData = "Admin.ExportData";
        
        /// <summary>Permisiuni pentru gestiunea rolurilor</summary>
        public static class Roluri
        {
            public const string View = "Admin.Roluri.View";
            public const string Create = "Admin.Roluri.Create";
            public const string Edit = "Admin.Roluri.Edit";
            public const string Delete = "Admin.Roluri.Delete";
            public const string AssignPermissions = "Admin.Roluri.AssignPermissions";
        }
    }
    
    #endregion
    
    #region Special Access
    
    /// <summary>Permisiuni speciale (urgențe, break-glass)</summary>
    public static class Special
    {
        /// <summary>
        /// Break-glass access - permite acces în situații de urgență.
        /// Toate accesările sunt logate pentru audit.
        /// </summary>
        public const string EmergencyAccess = "Special.EmergencyAccess";
        
        /// <summary>
        /// Acces complet la toate datele - doar pentru admin sistem.
        /// </summary>
        public const string FullAccess = "Special.FullAccess";
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Returnează toate permisiunile definite în sistem ca listă plată.
    /// Util pentru popularea UI-ului de administrare roluri.
    /// </summary>
    public static IReadOnlyList<PermissionDefinition> GetAllPermissions()
    {
        return new List<PermissionDefinition>
        {
            // Pacient - Permisiuni de bază
            new(Pacient.View, "Pacient", "Vizualizare Pacienți", "Permite vizualizarea listei de pacienți"),
            new(Pacient.Create, "Pacient", "Creare Pacient", "Permite crearea unui pacient nou"),
            new(Pacient.Edit, "Pacient", "Editare Pacient", "Permite editarea datelor pacientului"),
            new(Pacient.Delete, "Pacient", "Ștergere Pacient", "Permite ștergerea unui pacient"),
            new(Pacient.ViewSensitiveData, "Pacient", "Date Sensibile", "Permite vizualizarea CNP și date medicale"),
            new(Pacient.Export, "Pacient", "Export Pacienți", "Permite exportul listei de pacienți"),
            
            // Pacient - Permisiuni granulare View
            new(Pacient.ViewField.CNP, "Pacient.View", "Vizualizare CNP", "Vizualizare CNP pacient", true),
            new(Pacient.ViewField.DataNasterii, "Pacient.View", "Vizualizare Data Nașterii", "Vizualizare data nașterii", true),
            new(Pacient.ViewField.Telefon, "Pacient.View", "Vizualizare Telefon", "Vizualizare număr telefon", true),
            new(Pacient.ViewField.Email, "Pacient.View", "Vizualizare Email", "Vizualizare adresă email", true),
            new(Pacient.ViewField.Adresa, "Pacient.View", "Vizualizare Adresă", "Vizualizare adresă completă", true),
            new(Pacient.ViewField.Alergii, "Pacient.View", "Vizualizare Alergii", "Vizualizare alergii (date sensibile)", true),
            new(Pacient.ViewField.BoliCronice, "Pacient.View", "Vizualizare Boli Cronice", "Vizualizare boli cronice (date sensibile)", true),
            new(Pacient.ViewField.Asigurare, "Pacient.View", "Vizualizare Asigurare", "Vizualizare date asigurare", true),
            
            // Pacient - Permisiuni granulare Edit
            new(Pacient.EditField.CNP, "Pacient.Edit", "Editare CNP", "Modificare CNP pacient", true),
            new(Pacient.EditField.Nume, "Pacient.Edit", "Editare Nume", "Modificare nume pacient", true),
            new(Pacient.EditField.Prenume, "Pacient.Edit", "Editare Prenume", "Modificare prenume pacient", true),
            new(Pacient.EditField.DataNasterii, "Pacient.Edit", "Editare Data Nașterii", "Modificare data nașterii", true),
            new(Pacient.EditField.Telefon, "Pacient.Edit", "Editare Telefon", "Modificare telefon principal", true),
            new(Pacient.EditField.Email, "Pacient.Edit", "Editare Email", "Modificare adresă email", true),
            new(Pacient.EditField.Adresa, "Pacient.Edit", "Editare Adresă", "Modificare adresă completă", true),
            new(Pacient.EditField.Alergii, "Pacient.Edit", "Editare Alergii", "Modificare alergii (date medicale)", true),
            new(Pacient.EditField.BoliCronice, "Pacient.Edit", "Editare Boli Cronice", "Modificare boli cronice (date medicale)", true),
            new(Pacient.EditField.TipAsigurare, "Pacient.Edit", "Editare Asigurare", "Modificare date asigurare", true),
            
            // Consultatie - Permisiuni de bază
            new(Consultatie.View, "Consultatie", "Vizualizare Consultații", "Permite vizualizarea consultațiilor"),
            new(Consultatie.ViewOwn, "Consultatie", "Consultații Proprii", "Doar consultațiile proprii"),
            new(Consultatie.ViewDepartment, "Consultatie", "Consultații Departament", "Consultațiile din departament"),
            new(Consultatie.Create, "Consultatie", "Creare Consultație", "Permite crearea unei consultații"),
            new(Consultatie.Edit, "Consultatie", "Editare Consultație", "Permite editarea consultațiilor"),
            new(Consultatie.EditOwn, "Consultatie", "Editare Proprii", "Editare doar consultații proprii"),
            new(Consultatie.Delete, "Consultatie", "Ștergere Consultație", "Permite ștergerea consultațiilor"),
            new(Consultatie.Finalize, "Consultatie", "Finalizare", "Finalizare consultație"),
            new(Consultatie.Prescribe, "Consultatie", "Prescriere", "Prescriere medicamente/rețete"),
            
            // Consultatie - Permisiuni granulare Edit
            new(Consultatie.EditField.Diagnostic, "Consultatie.Edit", "Editare Diagnostic", "Modificare diagnostic", true),
            new(Consultatie.EditField.Simptome, "Consultatie.Edit", "Editare Simptome", "Modificare simptome", true),
            new(Consultatie.EditField.Tratament, "Consultatie.Edit", "Editare Tratament", "Modificare tratament", true),
            new(Consultatie.EditField.Reteta, "Consultatie.Edit", "Editare Rețetă", "Modificare rețetă medicală", true),
            new(Consultatie.EditField.Recomandari, "Consultatie.Edit", "Editare Recomandări", "Modificare recomandări", true),
            
            // Programare - Permisiuni de bază
            new(Programare.View, "Programare", "Vizualizare Programări", "Vizualizare calendar programări"),
            new(Programare.Create, "Programare", "Creare Programare", "Creare programare nouă"),
            new(Programare.Edit, "Programare", "Editare Programare", "Modificare programare existentă"),
            new(Programare.Delete, "Programare", "Ștergere Programare", "Ștergere programare"),
            new(Programare.Cancel, "Programare", "Anulare Programare", "Anulare programare"),
            new(Programare.Confirm, "Programare", "Confirmare Programare", "Confirmare programare"),
            
            // Personal - Permisiuni de bază
            new(Personal.View, "Personal", "Vizualizare Personal", "Vizualizare listă personal"),
            new(Personal.Create, "Personal", "Creare Personal", "Adăugare personal nou"),
            new(Personal.Edit, "Personal", "Editare Personal", "Modificare date personal"),
            new(Personal.Delete, "Personal", "Ștergere Personal", "Ștergere personal"),
            new(Personal.ManageRoles, "Personal", "Gestionare Roluri", "Atribuire roluri la personal"),
            
            // Admin - Permisiuni
            new(Admin.AccessDashboard, "Admin", "Acces Dashboard", "Acces la dashboard-ul administrativ"),
            new(Admin.ViewAuditLog, "Admin", "Jurnal Audit", "Vizualizare jurnal de audit"),
            new(Admin.ManageSettings, "Admin", "Setări Sistem", "Modificare setări sistem"),
            new(Admin.ManageUsers, "Admin", "Gestionare Utilizatori", "Administrare utilizatori"),
            new(Admin.ViewReports, "Admin", "Rapoarte", "Vizualizare rapoarte"),
            new(Admin.ExportData, "Admin", "Export Date", "Export date din sistem"),
            
            // Admin.Roluri
            new(Admin.Roluri.View, "Admin.Roluri", "Vizualizare Roluri", "Vizualizare lista de roluri"),
            new(Admin.Roluri.Create, "Admin.Roluri", "Creare Rol", "Creare rol nou"),
            new(Admin.Roluri.Edit, "Admin.Roluri", "Editare Rol", "Modificare rol existent"),
            new(Admin.Roluri.Delete, "Admin.Roluri", "Ștergere Rol", "Ștergere rol"),
            new(Admin.Roluri.AssignPermissions, "Admin.Roluri", "Atribuire Permisiuni", "Atribuire permisiuni la rol"),
            
            // Special
            new(Special.EmergencyAccess, "Special", "Acces Urgență", "Break-glass access pentru urgențe"),
            new(Special.FullAccess, "Special", "Acces Complet", "Acces total la toate funcționalitățile"),
        };
    }
    
    /// <summary>
    /// Returnează permisiunile grupate pe categorii.
    /// </summary>
    public static IReadOnlyDictionary<string, List<PermissionDefinition>> GetPermissionsGrouped()
    {
        return GetAllPermissions()
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
    
    #endregion
}

/// <summary>
/// Definiția unei permisiuni pentru UI
/// </summary>
public record PermissionDefinition(
    string Code,
    string Category,
    string DisplayName,
    string Description,
    bool IsFieldLevel = false
);

/// <summary>
/// Definește numele politicilor de autorizare.
/// Folosite cu [Authorize(Policy = Policies.XXX)]
/// </summary>
public static class Policies
{
    // Politici pentru Pacienți
    public const string CanViewPatients = "CanViewPatients";
    public const string CanManagePatients = "CanManagePatients";
    public const string CanViewSensitivePatientData = "CanViewSensitivePatientData";
    
    // Politici pentru Consultații
    public const string CanViewConsultations = "CanViewConsultations";
    public const string CanCreateConsultations = "CanCreateConsultations";
    public const string CanEditOwnConsultations = "CanEditOwnConsultations";
    public const string CanPrescribe = "CanPrescribe";
    
    // Politici pentru Programări
    public const string CanViewAppointments = "CanViewAppointments";
    public const string CanManageAppointments = "CanManageAppointments";
    
    // Politici pentru Administrare
    public const string CanAccessAdmin = "CanAccessAdmin";
    public const string CanViewAuditLog = "CanViewAuditLog";
    public const string CanManageUsers = "CanManageUsers";
    
    // Politici speciale
    public const string RequiresMedicalStaff = "RequiresMedicalStaff";
    public const string RequiresDoctor = "RequiresDoctor";
    public const string RequiresAdmin = "RequiresAdmin";
}
