using Dapper;
using ValyanClinic.Domain.Entities;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Infrastructure.Data;

namespace ValyanClinic.Infrastructure.Repositories;

/// <summary>
/// Repository pentru ICD10Code cu Dapper și Stored Procedures
/// Implementează căutare și autocomplete pentru coduri ICD-10
/// Tabel: ICD10_Codes (15 coloane)
/// </summary>
public class ICD10Repository : BaseRepository, IICD10Repository
{
    public ICD10Repository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<ICD10Code>> SearchAsync(
        string searchTerm,
        string? category = null,
        bool onlyCommon = false,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            SearchTerm = searchTerm,
            Category = category,
            OnlyCommon = onlyCommon,
            OnlyLeaf = true, // Pentru autocomplete, vrem doar leaf nodes
            MaxResults = maxResults
        };

        return await QueryAsync<ICD10Code>(
            "sp_ICD10_Search",
            parameters,
            cancellationToken);
    }

    public async Task<ICD10Code?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { ICD10_ID = id };
        return await QueryFirstOrDefaultAsync<ICD10Code>(
            "sp_ICD10_GetById",
            parameters,
            cancellationToken);
    }

    public async Task<ICD10Code?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { Code = code };
        return await QueryFirstOrDefaultAsync<ICD10Code>(
            "sp_ICD10_GetByCode",
            parameters,
            cancellationToken);
    }

    public async Task<IEnumerable<ICD10Code>> GetByCategoryAsync(
        string category,
        bool onlyLeafNodes = true,
        CancellationToken cancellationToken = default)
    {
        // Folosim Search cu filtru pe categorie
        var parameters = new
        {
            SearchTerm = "", // Empty search = toate din categorie
            Category = category,
            OnlyCommon = false,
            OnlyLeafNodes = onlyLeafNodes,
            MaxResults = 1000
        };

        return await QueryAsync<ICD10Code>(
            "sp_ICD10_Search",
            parameters,
            cancellationToken);
    }

    public async Task<IEnumerable<ICD10Code>> GetCommonCodesAsync(
        string? category = null,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            Category = category,
            MaxResults = maxResults
        };

        return await QueryAsync<ICD10Code>(
            "sp_ICD10_GetCommon",
            parameters,
            cancellationToken);
    }

    public async Task<IEnumerable<ICD10Code>> GetChildCodesAsync(
        string parentCode,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { ParentCode = parentCode };
        return await QueryAsync<ICD10Code>(
            "sp_ICD10_GetChildren",
            parameters,
            cancellationToken);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<CategoryDto>(
            "sp_ICD10_GetCategories",
            cancellationToken: cancellationToken);

        return results.Select(r => r.Category);
    }

    public async Task<(int totalCodes, int commonCodes, int categories)> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await QueryFirstOrDefaultAsync<StatisticsDto>(
            "sp_ICD10_GetStatistics",
            cancellationToken: cancellationToken);

        return (
            result?.TotalCodes ?? 0,
            result?.CommonCodes ?? 0,
            result?.Categories ?? 0
        );
    }

    public async Task<bool> IsValidCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var parameters = new { Code = code };
        var result = await ExecuteScalarAsync<int>(
            "sp_ICD10_ValidateCode",
            parameters,
            cancellationToken);

        return result > 0;
    }

    public async Task<int> BulkImportAsync(
        IEnumerable<ICD10Code> codes,
        CancellationToken cancellationToken = default)
    {
        var successCount = 0;

        foreach (var code in codes)
        {
            var parameters = new
            {
                code.Code,
                code.FullCode,
                code.Category,
                code.ShortDescription,
                code.LongDescription,
                code.EnglishDescription,
                code.ParentCode,
                code.IsLeafNode,
                code.IsCommon,
                code.Severity,
                code.SearchTerms,
                code.Notes
            };

            try
            {
                await ExecuteAsync(
                    "sp_ICD10_Insert",
                    parameters,
                    cancellationToken);
                successCount++;
            }
            catch
            {
                // Ignore duplicates for bulk import
            }
        }

        return successCount;
    }

    #region DTOs for result mapping

    private class CategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int CodeCount { get; set; }
    }

    private class StatisticsDto
    {
        public int TotalCodes { get; set; }
        public int CommonCodes { get; set; }
        public int Categories { get; set; }
        public int LeafNodes { get; set; }
        public int MildCodes { get; set; }
        public int ModerateCodes { get; set; }
        public int SevereCodes { get; set; }
        public int CriticalCodes { get; set; }
    }

    #endregion
}
