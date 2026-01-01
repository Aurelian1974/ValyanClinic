using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using ValyanClinic.Application.Features.PersonalMedicalManagement.Queries.GetPersonalMedicalList;
using ValyanClinic.Domain.Interfaces.Repositories;
using ValyanClinic.Domain.Entities;
using Xunit;

namespace ValyanClinic.Tests.Application.PersonalMedical
{
    public class GetPersonalMedicalListQueryHandlerAdvancedFiltersTests
    {
        [Fact]
        public async Task Handler_Applies_ColumnFilters_NumeContains()
        {
            // Arrange
            var mockRepo = new Mock<IPersonalMedicalRepository>();
            var logger = new Mock<ILogger<GetPersonalMedicalListQueryHandler>>();

            var sampleData = new List<Domain.Entities.PersonalMedical>
            {
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Ionescu", Prenume = "Ion", Specializare = "Cardiologie", NumarLicenta = "123" },
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Popescu", Prenume = "Ana", Specializare = "Dermatologie", NumarLicenta = "456" },
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Ionescu", Prenume = "Maria", Specializare = "Pediatrie", NumarLicenta = "789" },
            };

            mockRepo.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int pn, int ps, string? s1, string? d1, string? p1, bool? e1, string sc, string sd, string? columnFiltersJson, CancellationToken ct) =>
                {
                    var items = sampleData.AsEnumerable();
                    if (!string.IsNullOrEmpty(columnFiltersJson))
                    {
                        var filters = System.Text.Json.JsonSerializer.Deserialize<List<ColumnFilterDto>>(columnFiltersJson);
                        foreach (var f in filters)
                        {
                            if (f.Column == "Nume")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.Nume == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                            if (f.Column == "Specializare")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.Specializare == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                            if (f.Column == "NumarLicenta")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.NumarLicenta == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                        }
                    }
                    return items.ToList();
                });

            mockRepo.Setup(r => r.GetFilterMetadataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValyanClinic.Domain.Entities.PersonalMedicalFilterMetadataDto { AvailableDepartamente = new List<ValyanClinic.Domain.Entities.FilterOption>(), AvailablePozitii = new List<ValyanClinic.Domain.Entities.FilterOption>() });

            mockRepo.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PersonalMedicalStatisticsDto { TotalActiv = 2, TotalInactiv = 1 });

            var expectedCount = sampleData.Count(x => x.Nume.Contains("Ion"));
            mockRepo.Setup(r => r.GetCountAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCount);

            var handler = new GetPersonalMedicalListQueryHandler(mockRepo.Object, logger.Object);

            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ColumnFilters = new List<ColumnFilterDto> { new ColumnFilterDto { Column = "Nume", Operator = "Contains", Value = "Ion" } }
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.TotalCount); // two records contain "Ion" in name
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task Handler_Applies_ColumnFilters_SpecializareStartsWith()
        {
            // Arrange similar to previous
            var mockRepo = new Mock<IPersonalMedicalRepository>();
            var logger = new Mock<ILogger<GetPersonalMedicalListQueryHandler>>();

            var sampleData = new List<Domain.Entities.PersonalMedical>
            {
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Ionescu", Prenume = "Ion", Specializare = "Cardiologie", NumarLicenta = "123" },
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Popescu", Prenume = "Ana", Specializare = "Dermatologie", NumarLicenta = "456" },
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Marin", Prenume = "Marin", Specializare = "Cardiochirurgie", NumarLicenta = "789" },
            };

            mockRepo.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int pn, int ps, string? s1, string? d1, string? p1, bool? e1, string sc, string sd, string? columnFiltersJson, CancellationToken ct) =>
                {
                    var items = sampleData.AsEnumerable();
                    if (!string.IsNullOrEmpty(columnFiltersJson))
                    {
                        var filters = System.Text.Json.JsonSerializer.Deserialize<List<ColumnFilterDto>>(columnFiltersJson);
                        foreach (var f in filters)
                        {
                            if (f.Column == "Nume")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.Nume == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                            if (f.Column == "Specializare")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.Specializare == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                            if (f.Column == "NumarLicenta")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.NumarLicenta == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                        }
                    }
                    return items.ToList();
                });

            mockRepo.Setup(r => r.GetFilterMetadataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PersonalMedicalFilterMetadataDto { AvailableDepartamente = new List<ValyanClinic.Domain.Entities.FilterOption>(), AvailablePozitii = new List<ValyanClinic.Domain.Entities.FilterOption>() });

            mockRepo.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PersonalMedicalStatisticsDto { TotalActiv = 2, TotalInactiv = 1 });

            var expectedCount = sampleData.Count(x => x.Specializare.StartsWith("Cardio"));
            mockRepo.Setup(r => r.GetCountAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCount);

            var handler = new GetPersonalMedicalListQueryHandler(mockRepo.Object, logger.Object);

            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ColumnFilters = new List<ColumnFilterDto> { new ColumnFilterDto { Column = "Specializare", Operator = "StartsWith", Value = "Cardio" } }
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.TotalCount); // Cardio* matches 2 entries
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task Handler_Applies_Multiple_ColumnFilters_Combined()
        {
            // Arrange similar
            var mockRepo = new Mock<IPersonalMedicalRepository>();
            var logger = new Mock<ILogger<GetPersonalMedicalListQueryHandler>>();

            var sampleData = new List<Domain.Entities.PersonalMedical>
            {
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Ionescu", Prenume = "Ion", Specializare = "Cardiologie", NumarLicenta = "123" },
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Ionescu", Prenume = "Maria", Specializare = "Pediatrie", NumarLicenta = "123" },
                new Domain.Entities.PersonalMedical { PersonalID = Guid.NewGuid(), Nume = "Popescu", Prenume = "Ana", Specializare = "Cardiologie", NumarLicenta = "456" },
            };

            mockRepo.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int pn, int ps, string? s1, string? d1, string? p1, bool? e1, string sc, string sd, string? columnFiltersJson, CancellationToken ct) =>
                {
                    var items = sampleData.AsEnumerable();
                    if (!string.IsNullOrEmpty(columnFiltersJson))
                    {
                        var filters = System.Text.Json.JsonSerializer.Deserialize<List<ColumnFilterDto>>(columnFiltersJson);
                        foreach (var f in filters)
                        {
                            if (f.Column == "Nume")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.Nume == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Nume) && x.Nume.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                            if (f.Column == "Specializare")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.Specializare == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.Specializare) && x.Specializare.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                            if (f.Column == "NumarLicenta")
                            {
                                if (f.Operator == "Contains") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.Contains(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "Equals") items = items.Where(x => x.NumarLicenta == f.Value);
                                if (f.Operator == "StartsWith") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.StartsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                                if (f.Operator == "EndsWith") items = items.Where(x => !string.IsNullOrEmpty(x.NumarLicenta) && x.NumarLicenta.EndsWith(f.Value, StringComparison.OrdinalIgnoreCase));
                            }
                        }
                    }
                    return items.ToList();
                });

            mockRepo.Setup(r => r.GetFilterMetadataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PersonalMedicalFilterMetadataDto { AvailableDepartamente = new List<ValyanClinic.Domain.Entities.FilterOption>(), AvailablePozitii = new List<ValyanClinic.Domain.Entities.FilterOption>() });

            mockRepo.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PersonalMedicalStatisticsDto { TotalActiv = 2, TotalInactiv = 1 });

            var expectedCount = sampleData.Count(x => x.Nume.Contains("Ionescu") && x.Specializare == "Cardiologie");
            mockRepo.Setup(r => r.GetCountAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCount);

            var handler = new GetPersonalMedicalListQueryHandler(mockRepo.Object, logger.Object);

            var query = new GetPersonalMedicalListQuery
            {
                PageNumber = 1,
                PageSize = 10,
                ColumnFilters = new List<ColumnFilterDto>
                {
                    new ColumnFilterDto { Column = "Nume", Operator = "Contains", Value = "Ionescu" },
                    new ColumnFilterDto { Column = "Specializare", Operator = "Equals", Value = "Cardiologie" }
                }
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.TotalCount); // only Ionescu + Cardiologie matches
            Assert.Equal(1, result.Value.Count());
        }
    }
}
