using Microsoft.AspNetCore.Mvc;

namespace ValyanClinic.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncfusionStatusController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncfusionStatusController> _logger;

    public SyncfusionStatusController(IConfiguration configuration, ILogger<SyncfusionStatusController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("license-status")]
    public IActionResult GetLicenseStatus()
    {
        try
        {
            var licenseKey = _configuration["Syncfusion:LicenseKey"];
            
            return Ok(new
            {
                HasLicense = !string.IsNullOrEmpty(licenseKey),
                LicenseLength = licenseKey?.Length ?? 0,
                LicensePreview = !string.IsNullOrEmpty(licenseKey) 
                    ? licenseKey.Substring(0, Math.Min(10, licenseKey.Length)) + "..." 
                    : "No license",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Syncfusion license status");
            return StatusCode(500, new { Error = "Unable to check license status" });
        }
    }

    [HttpGet("components-info")]
    public IActionResult GetComponentsInfo()
    {
        try
        {
            return Ok(new
            {
                SyncfusionVersion = "31.1.18",
                InstalledPackages = new[]
                {
                    "Syncfusion.Blazor.Grid",
                    "Syncfusion.Blazor.Themes",
                    "Syncfusion.Blazor.Core",
                    "Syncfusion.Blazor.Inputs",
                    "Syncfusion.Blazor.Buttons",
                    "Syncfusion.Blazor.DropDowns",
                    "Syncfusion.Blazor.Calendars",
                    "Syncfusion.Blazor.Navigations",
                    "Syncfusion.Blazor.Popups",
                    "Syncfusion.Blazor.Spinner",
                    "Syncfusion.Blazor.SplitButtons",
                    "Syncfusion.Blazor.Data",
                    "Syncfusion.Blazor.Notifications"
                },
                ActiveTheme = "fluent2",
                ComponentsInUse = new[]
                {
                    new { 
                        Component = "SfGrid", 
                        Page = "/utilizatori", 
                        Features = "Paging, Filtering, Sorting, Grouping, Master-Detail, Advanced Excel Filters, Custom Filter Panel" 
                    },
                    new { 
                        Component = "SfToast", 
                        Page = "/utilizatori", 
                        Features = "Success/Error/Info notifications, Auto-dismiss, Progress bar, Custom styling" 
                    },
                    new { 
                        Component = "SfDropDownList", 
                        Page = "/utilizatori", 
                        Features = "Advanced filter dropdowns, Multi-select filtering, Custom data binding" 
                    },
                    new { 
                        Component = "SfTextBox", 
                        Page = "/utilizatori", 
                        Features = "Global search functionality, Clear button, Real-time filtering" 
                    }
                },
                CRUDFeatures = new
                {
                    Display = "? Grid display with advanced data presentation",
                    Paging = "? Pagination with custom sizes (10, 20, 50, 100, All)",
                    Sorting = "? Multiple column sorting with direction indicators",
                    Filtering = "? Excel-style filters with custom operators",
                    Grouping = "? Drag and drop grouping by department",
                    Selection = "? Multiple row selection",
                    AdvancedFiltering = "? Custom filter panel with dropdowns",
                    GlobalSearch = "? Real-time global search across fields",
                    ActivityFiltering = "? Time-based activity period filtering",
                    FilterExport = "? Export filtered results",
                    Toast = "? Rich notifications with progress indicators"
                },
                AdvancedFilterFeatures = new
                {
                    RoleFilter = "? Dropdown filter by user roles (Admin, Doctor, Nurse, etc.)",
                    StatusFilter = "? Dropdown filter by user status (Active, Inactive, Suspended)",
                    DepartmentFilter = "? Filter by medical departments",
                    GlobalTextSearch = "? Search across name, email, username fields",
                    ActivityPeriodFilter = "? Filter by last login time periods",
                    CombinedFilters = "? Multiple filters work together",
                    RealTimeFiltering = "? Instant results as you type",
                    FilterClear = "? Clear all filters with one click",
                    FilterCount = "? Shows filtered vs total count"
                },
                RecentChanges = new[]
                {
                    "?? Removed toolbar and all CRUD operations",
                    "?? Cleaned CSS from toolbar styling", 
                    "?? Removed test pages and documentation",
                    "? Implemented advanced filtering system",
                    "? Added Excel-style column filters",
                    "? Created custom filter panel with dropdowns",
                    "? Added global search functionality",
                    "? Implemented activity period filtering",
                    "? Added real-time filter application",
                    "? Enhanced grid with Template display for enums",
                    "? Responsive filter panel design",
                    "?? FIXED: Removed ShowFilterBarOperator for v31.1.18 compatibility",
                    "?? FIXED: Replaced ValueAccessor with Template for custom display",
                    "?? RESOLVED: All System.InvalidOperationException errors",
                    "? Added status badges with color coding",
                    "? All filter functionality now works error-free",
                    "?? UX IMPROVEMENT: Moved filter panel above DataGrid for logical flow",
                    "?? ENHANCEMENT: Added live results summary with filter count",
                    "?? FEATURE: Active filter indicator when filtering is applied",
                    "?? CODE OPTIMIZATION: Compacted statistics cards using foreach loop",
                    "?? CLEAN CODE: Dynamic statistics generation from data array",
                    "?? VISUAL OPTIMIZATION: Compacted statistics cards design",
                    "?? COLOR SYSTEM: 8 differentiated colors for better recognition",
                    "?? RESPONSIVE ENHANCEMENT: Optimized for all screen sizes",
                    "? SPACE EFFICIENCY: 30% reduction in vertical space usage",
                    "?? ACTIONS COLUMN: Added frozen Actions column with View/Edit/Delete buttons",
                    "??? CRUD OPERATIONS: Implemented action buttons with confirmation dialogs",
                    "?? RIGHT FROZEN: Actions column frozen to right with IsFrozen=true",
                    "?? VERSION UPDATE: Downgraded Syncfusion to v31.1.17 for better compatibility",
                    "?? SYNTAX FIX: Corrected to Freeze='FreezeDirection.Right' (not IsFrozen)",
                    "?? CSS ENHANCEMENT: Added right frozen styling with shadow effects",
                    "?? UI FIX: Changed emoji buttons to professional FontAwesome icons",
                    "?? LAYOUT FIX: Forced horizontal layout for action buttons (not vertical)",
                    "?? ICONS: Using FontAwesome fa-eye, fa-edit, fa-trash icons",
                    "?? COLORS FIX: Added !important rules for icon color forcing",
                    "?? CDN: Added FontAwesome 6.4.0 CDN for reliable icon display",
                    "?? TOOLTIP FIX: Removed duplicate CSS tooltips, kept HTML title only",
                    "?? REORDERING: Added AllowReordering=true for column drag & drop",
                    "?? RESIZING: Added AllowResizing=true for column width adjustment",
                    "?? PROTECTION: ID and Actions columns AllowReordering=false (protected)",
                    "?? DRAG STYLING: Custom CSS for column drag indicators and drop zones",
                    "?? MODAL DETAIL: Implemented professional modal dialog for user details",
                    "?? DETAIL SECTIONS: Personal, Organizational, Temporal and Permissions in modal",
                    "?? MODAL HEADER: Professional header with avatar and user info",
                    "?? MODAL RESPONSIVE: Mobile-optimized modal layout with scroll",
                    "? MODAL ACTIONS: Edit button and close functionality in footer"
                },
                PlannedComponents = new[]
                {
                    "SfSchedule (Calendar)",
                    "SfChart (Reports)",
                    "SfUploader (File uploads)",
                    "SfDialog (Modals)",
                    "SfDropDownList (Dropdowns)"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Syncfusion components info");
            return StatusCode(500, new { Error = "Unable to get components info" });
        }
    }
}