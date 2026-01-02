# PaginationControl Component - Integration Guide

**Versiune:** 1.0  
**Data:** 2025-01-02  
**Status:** ✅ Production Ready

## Overview

`PaginationControl` este o componentă refolosibilă pentru paging server-side în toate paginile cu Syncfusion Grid. Componenta oferă:
- Navigare (Prima, Anterioara, Următoarea, Ultima)
- Informații despre pagină curentă
- Selector pentru număr înregistrări pe pagină

## Usage

### 1. În fișierul `.razor` (markup)

Înlocuiește HTML-ul de paging existent cu:

```razor
<PaginationControl CurrentPage="@CurrentPage"
                 PageSize="@PageSize"
                 TotalRecords="@TotalRecords"
                 PageSizeOptions="@PageSizeArray"
                 OnFirstPage="@GoToFirstPage"
                 OnPreviousPage="@GoToPreviousPage"
                 OnNextPage="@GoToNextPage"
                 OnLastPage="@GoToLastPage"
                 OnPageSizeChanged="@ChangePageSize" />
```

### 2. În fișierul `.razor.cs` (code-behind)

#### Proprietăți necesare:

```csharp
// Pagination state
private int CurrentPage { get; set; } = 1;
private int PageSize { get; set; } = 20;
private int TotalRecords { get; set; } = 0;
private int[] PageSizeArray = new int[] { 10, 20, 50, 100, 250 };

// Computed property
private int TotalPages => TotalRecords > 0 
    ? (int)Math.Ceiling((double)TotalRecords / PageSize) 
    : 0;
```

#### Metode necesare:

```csharp
private async Task GoToFirstPage()
{
    if (CurrentPage == 1) return;
    CurrentPage = 1;
    await LoadPagedData(); // Sau metoda ta de încărcare date
}

private async Task GoToPreviousPage()
{
    if (CurrentPage <= 1) return;
    CurrentPage--;
    await LoadPagedData();
}

private async Task GoToNextPage()
{
    if (CurrentPage >= TotalPages) return;
    CurrentPage++;
    await LoadPagedData();
}

private async Task GoToLastPage()
{
    if (CurrentPage == TotalPages) return;
    CurrentPage = TotalPages;
    await LoadPagedData();
}

private async Task ChangePageSize(int newPageSize)
{
    if (PageSize == newPageSize) return;
    
    PageSize = newPageSize;
    CurrentPage = 1; // Reset la prima pagină
    await LoadPagedData();
}
```

## Integration Steps (Pas cu Pas)

### Pas 1: Verifică dacă pagina ta folosește server-side paging

Caută în `.razor.cs`:
- `CurrentPage`, `PageSize`, `TotalRecords`
- Metode de tip `LoadPagedData()` sau `LoadData()`

### Pas 2: Adaugă metodele lipsă

Dacă pagina ta NU are metodele `GoToFirstPage`, `GoToPreviousPage`, etc., adaugă-le folosind template-ul de mai sus.

### Pas 3: Înlocuiește HTML-ul de paging

În fișierul `.razor`, găsește secțiunea cu paging (de obicei după `</SfGrid>`) și înlocuiește-o cu componenta `<PaginationControl>`.

### Pas 4: Testează

1. Build aplicația: `dotnet build`
2. Rulează aplicația: `dotnet run`
3. Testează navigarea între pagini
4. Testează schimbarea page size

## Examples

### Exemplu complet: AdministrarePacienti

**AdministrarePacienti.razor:**
```razor
</SfGrid>

<!-- Pagination -->
<PaginationControl CurrentPage="@CurrentPage"
                 PageSize="@PageSize"
                 TotalRecords="@TotalRecords"
                 PageSizeOptions="@PageSizeArray"
                 OnFirstPage="@GoToFirstPage"
                 OnPreviousPage="@GoToPreviousPage"
                 OnNextPage="@GoToNextPage"
                 OnLastPage="@GoToLastPage"
                 OnPageSizeChanged="@ChangePageSize" />
```

**AdministrarePacienti.razor.cs:**
```csharp
// Properties
private int CurrentPage { get; set; } = 1;
private int PageSize { get; set; } = 20;
private int TotalRecords { get; set; } = 0;
private int[] PageSizeArray = new int[] { 10, 20, 50, 100, 250 };
private int TotalPages => TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;

// Methods
private async Task GoToFirstPage()
{
    if (CurrentPage == 1) return;
    CurrentPage = 1;
    await LoadPagedDataAsync();
}

private async Task GoToPreviousPage()
{
    if (CurrentPage <= 1) return;
    CurrentPage--;
    await LoadPagedDataAsync();
}

private async Task GoToNextPage()
{
    if (CurrentPage >= TotalPages) return;
    CurrentPage++;
    await LoadPagedDataAsync();
}

private async Task GoToLastPage()
{
    if (CurrentPage == TotalPages) return;
    CurrentPage = TotalPages;
    await LoadPagedDataAsync();
}

private async Task ChangePageSize(int newPageSize)
{
    if (PageSize == newPageSize) return;
    PageSize = newPageSize;
    CurrentPage = 1;
    await LoadPagedDataAsync();
}
```

## Pagini care trebuie actualizate

### ✅ Complet integrate:
1. **AdministrarePacienti** - ✅ DONE

### 🔄 Necesită integrare:
1. **VizualizarePacienti** - Are paging, lipsesc metode navigare
2. **AdministrarePersonalMedical** - Are paging, lipsește ChangePageSize
3. **ListaProgramari** - Verifică dacă are server-side paging
4. **AdministrareDepartamente** - Verifică dacă are server-side paging
5. **AdministrareSpecializari** - Verifică dacă are server-side paging
6. **AdministrarePozitii** - Verifică dacă are server-side paging
7. **AdministrareRoluri** - Verifică dacă are server-side paging

## Component API

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `CurrentPage` | `int` | 1 | Pagina curentă (1-based) |
| `PageSize` | `int` | 20 | Număr înregistrări pe pagină |
| `TotalRecords` | `int` | 0 | Total înregistrări în DB |
| `PageSizeOptions` | `int[]` | `[10,20,50,100,250]` | Opțiuni pentru page size |
| `OnFirstPage` | `EventCallback` | - | Callback pentru prima pagină |
| `OnPreviousPage` | `EventCallback` | - | Callback pentru pagina anterioară |
| `OnNextPage` | `EventCallback` | - | Callback pentru pagina următoare |
| `OnLastPage` | `EventCallback` | - | Callback pentru ultima pagină |
| `OnPageSizeChanged` | `EventCallback<int>` | - | Callback pentru schimbare page size |

### Styling

Componenta vine cu CSS scoped (`PaginationControl.razor.css`) care include:
- Design blue pastel consistent cu tema aplicației
- Layout responsive (mobile-friendly)
- Hover effects și transitions
- Disable state pentru butoane

## Troubleshooting

### Eroare: "The name 'PaginationControl' does not exist"
**Soluție:** Build aplicația (`dotnet build`) pentru a regenera componenta.

### Paginarea nu funcționează
**Verifică:**
1. Metoda `LoadPagedData()` actualizează `TotalRecords`
2. Proprietățile `CurrentPage`, `PageSize` sunt actualizate corect
3. `TotalPages` este calculat corect

### Butoanele sunt disabled
**Cauză:** `CurrentPage` sau `TotalPages` sunt 0.  
**Soluție:** Verifică că `LoadPagedData()` setează `TotalRecords` > 0.

## Related Files

- **Component:** `ValyanClinic/Components/Shared/PaginationControl.razor`
- **Code-behind:** `ValyanClinic/Components/Shared/PaginationControl.razor.cs`
- **Styles:** `ValyanClinic/Components/Shared/PaginationControl.razor.css`
- **Example:** `ValyanClinic/Components/Pages/Pacienti/AdministrarePacienti.razor`

## Future Enhancements

Posibile îmbunătățiri viitoare:
- [ ] Direct page jump (input pentru număr pagină)
- [ ] Localstorage pentru page size preference
- [ ] Export pagination state în URL (query params)
- [ ] Animații pentru tranziții între pagini
- [ ] Keyboard shortcuts (←→ pentru navigare)

---

**Autor:** GitHub Copilot  
**Versiune:** 1.0  
**Last Updated:** 2025-01-02
