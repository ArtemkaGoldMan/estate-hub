using EstateHub.ListingService.Core.Mappers;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using EstateHub.SharedKernel.API.Interfaces;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;
using EstateHub.SharedKernel.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EstateHub.ListingService.Core.Tests;

public class ReportServiceTests
{
    private readonly Mock<ILogger<EstateHub.ListingService.Core.Services.ReportService>> _loggerMock;
    private readonly Mock<IReportRepository> _reportRepositoryMock;
    private readonly Mock<IListingRepository> _listingRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<CreateReportInput>> _createValidatorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserServiceClient> _userServiceClientMock;
    private readonly Mock<ILogger<ReportDtoMapper>> _mapperLoggerMock;
    private readonly ReportDtoMapper _dtoMapper;
    private readonly EstateHub.ListingService.Core.Services.ReportService _reportService;

    public ReportServiceTests()
    {
        _loggerMock = new Mock<ILogger<EstateHub.ListingService.Core.Services.ReportService>>();
        _reportRepositoryMock = new Mock<IReportRepository>();
        _listingRepositoryMock = new Mock<IListingRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _createValidatorMock = new Mock<IValidator<CreateReportInput>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userServiceClientMock = new Mock<IUserServiceClient>();
        _mapperLoggerMock = new Mock<ILogger<ReportDtoMapper>>();

        // Setup unit of work to return success
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success(true));

        // Setup DTO mapper
        _dtoMapper = new ReportDtoMapper(
            _userServiceClientMock.Object,
            _listingRepositoryMock.Object,
            _mapperLoggerMock.Object
        );

        // Setup validators to return success by default
        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateReportInput>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _reportService = new EstateHub.ListingService.Core.Services.ReportService(
            _reportRepositoryMock.Object,
            _listingRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _createValidatorMock.Object,
            _dtoMapper,
            _loggerMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_ReturnsReportId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var input = new CreateReportInput(
            listingId,
            ReportReason.Spam,
            "This listing contains spam content"
        );

        var listing = new Listing(
            Guid.NewGuid(),
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _reportRepositoryMock
            .Setup(r => r.GetByListingIdAsync(listingId))
            .ReturnsAsync(new List<Report>()); // No existing reports

        _reportRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reportService.CreateAsync(input);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _currentUserServiceMock.Verify(s => s.GetUserId(), Times.Once);
        _createValidatorMock.Verify(v => v.ValidateAsync(input, It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.Is<Report>(rep => 
            rep.ReporterId == userId && 
            rep.ListingId == listingId &&
            rep.Reason == ReportReason.Spam
        )), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentListing_ThrowsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var input = new CreateReportInput(
            listingId,
            ReportReason.Spam,
            "This listing contains spam content"
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync((Listing?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _reportService.CreateAsync(input));

        _listingRepositoryMock.Verify(r => r.GetByIdAsync(listingId), Times.Once);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_AsReporter_DeletesReport()
    {
        // Arrange
        var reporterId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var report = new Report(
            reporterId,
            listingId,
            ReportReason.Spam,
            "Test description"
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(reporterId);

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _reportRepositoryMock
            .Setup(r => r.DeleteAsync(reportId))
            .Returns(Task.CompletedTask);

        // Act
        await _reportService.DeleteAsync(reportId);

        // Assert
        _reportRepositoryMock.Verify(r => r.GetByIdAsync(reportId), Times.Once);
        _reportRepositoryMock.Verify(r => r.DeleteAsync(reportId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AsNonReporterWithoutPermission_ThrowsError()
    {
        // Arrange
        var reporterId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var report = new Report(
            reporterId,
            listingId,
            ReportReason.Spam,
            "Test description"
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(otherUserId);

        _currentUserServiceMock
            .Setup(s => s.HasPermission(It.IsAny<string>()))
            .Returns(false);

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _reportService.DeleteAsync(reportId));

        _reportRepositoryMock.Verify(r => r.GetByIdAsync(reportId), Times.Once);
        _reportRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithAlreadyReportedListing_ThrowsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var input = new CreateReportInput(
            listingId,
            ReportReason.Spam,
            "This listing contains spam content"
        );

        var listing = new Listing(
            Guid.NewGuid(),
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        var existingReport = new Report(
            userId,
            listingId,
            ReportReason.Spam,
            "Previous report"
        ) with { Status = ReportStatus.Pending };

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _reportRepositoryMock
            .Setup(r => r.GetByListingIdAsync(listingId))
            .ReturnsAsync(new List<Report> { existingReport });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _reportService.CreateAsync(input));

        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithDismissedReport_AllowsNewReport()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var input = new CreateReportInput(
            listingId,
            ReportReason.Spam,
            "This listing contains spam content"
        );

        var listing = new Listing(
            Guid.NewGuid(),
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        );

        var dismissedReport = new Report(
            userId,
            listingId,
            ReportReason.Spam,
            "Previous report"
        ) with { Status = ReportStatus.Dismissed };

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _reportRepositoryMock
            .Setup(r => r.GetByListingIdAsync(listingId))
            .ReturnsAsync(new List<Report> { dismissedReport });

        _reportRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reportService.CreateAsync(input);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Report>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_WithUnpublishListing_UnpublishesListing()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var listing = new Listing(
            Guid.NewGuid(),
            ListingCategory.Sale,
            PropertyType.Apartment,
            "Test Listing",
            "Description",
            "123 Main St",
            "Downtown",
            "Warsaw",
            "00-001",
            52.2297m,
            21.0122m,
            75.5m,
            3,
            Condition.Good,
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            null,
            500000m,
            null
        ) with { Status = ListingStatus.Published };

        var report = new Report(
            Guid.NewGuid(),
            listingId,
            ReportReason.Spam,
            "Test description"
        );

        var input = new ResolveReportInput(
            reportId,
            "Accepted",
            "Moderator notes",
            UnpublishListing: true,
            UnpublishReason: "Violates guidelines"
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(moderatorId);

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        _reportRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        _listingRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Listing>()))
            .Returns(Task.CompletedTask);

        // Act
        await _reportService.ResolveAsync(input);

        // Assert
        _listingRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Listing>(l => 
            l.Status == ListingStatus.Draft &&
            l.AdminUnpublishReason == "Violates guidelines"
        )), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_WithUnpublishButNoReason_ThrowsError()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var report = new Report(
            Guid.NewGuid(),
            listingId,
            ReportReason.Spam,
            "Test description"
        );

        var input = new ResolveReportInput(
            reportId,
            "Accepted",
            "Moderator notes",
            UnpublishListing: true,
            UnpublishReason: null
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(moderatorId);

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _reportService.ResolveAsync(input));
    }

    [Fact]
    public async Task DismissAsync_AssignsModeratorAndDismisses()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var report = new Report(
            Guid.NewGuid(),
            listingId,
            ReportReason.Spam,
            "Test description"
        );

        var input = new DismissReportInput(
            reportId,
            "Moderator notes"
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(moderatorId);

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _reportRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        // Act
        await _reportService.DismissAsync(input);

        // Assert
        _reportRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Report>(rep => 
            rep.Status == ReportStatus.Dismissed &&
            rep.ModeratorId == moderatorId
        )), Times.Once);
    }

    [Fact]
    public async Task AssignToModeratorAsync_AssignsModerator()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var report = new Report(
            Guid.NewGuid(),
            listingId,
            ReportReason.Spam,
            "Test description"
        );

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _reportRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        // Act
        await _reportService.AssignToModeratorAsync(reportId, moderatorId);

        // Assert
        _reportRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Report>(rep => 
            rep.ModeratorId == moderatorId
        )), Times.Once);
    }

    [Fact]
    public async Task AssignToModeratorAsync_WithNonExistentReport_ThrowsError()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _reportService.AssignToModeratorAsync(reportId, moderatorId));

        _reportRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task CloseAsync_ClosesReport()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var report = new Report(
            Guid.NewGuid(),
            listingId,
            ReportReason.Spam,
            "Test description"
        ) with { Id = reportId, Status = ReportStatus.Resolved };

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _reportRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Report>()))
            .Returns(Task.CompletedTask);

        // Act
        await _reportService.CloseAsync(reportId);

        // Assert
        _reportRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Report>(rep => 
            rep.Status == ReportStatus.Closed
        )), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var filter = new ReportFilter { Status = ReportStatus.Pending };

        var reports = new List<Report>
        {
            new Report(
                Guid.NewGuid(),
                Guid.NewGuid(),
                ReportReason.Spam,
                "Test description"
            )
        };

        _reportRepositoryMock
            .Setup(r => r.GetAllAsync(page, pageSize, filter))
            .ReturnsAsync(reports);

        _reportRepositoryMock
            .Setup(r => r.GetTotalCountAsync(filter))
            .ReturnsAsync(1);

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse?)null);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Listing?)null);

        // Act
        var result = await _reportService.GetAllAsync(filter, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetMyReportsAsync_ReturnsUserReports()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;

        var reports = new List<Report>
        {
            new Report(
                userId,
                Guid.NewGuid(),
                ReportReason.Spam,
                "Test description"
            )
        };

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(userId);

        _reportRepositoryMock
            .Setup(r => r.GetByReporterIdAsync(userId, page, pageSize))
            .ReturnsAsync(reports);

        _reportRepositoryMock
            .Setup(r => r.GetTotalCountAsync(It.Is<ReportFilter>(f => f.ReporterId == userId)))
            .ReturnsAsync(1);

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse?)null);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Listing?)null);

        // Act
        var result = await _reportService.GetMyReportsAsync(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetReportsForModerationAsync_ReturnsPendingReports()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        var reports = new List<Report>
        {
            new Report(
                Guid.NewGuid(),
                Guid.NewGuid(),
                ReportReason.Spam,
                "Test description"
            ) with { Status = ReportStatus.Pending }
        };

        _reportRepositoryMock
            .Setup(r => r.GetAllAsync(page, pageSize, It.Is<ReportFilter>(f => f.Status == ReportStatus.Pending)))
            .ReturnsAsync(reports);

        _reportRepositoryMock
            .Setup(r => r.GetTotalCountAsync(It.Is<ReportFilter>(f => f.Status == ReportStatus.Pending)))
            .ReturnsAsync(1);

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse?)null);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Listing?)null);

        // Act
        var result = await _reportService.GetReportsForModerationAsync(page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetReportsByListingIdAsync_ReturnsListingReports()
    {
        // Arrange
        var listingId = Guid.NewGuid();

        var reports = new List<Report>
        {
            new Report(
                Guid.NewGuid(),
                listingId,
                ReportReason.Spam,
                "Test description"
            ),
            new Report(
                Guid.NewGuid(),
                listingId,
                ReportReason.InappropriateContent,
                "Another description"
            )
        };

        _reportRepositoryMock
            .Setup(r => r.GetByListingIdAsync(listingId))
            .ReturnsAsync(reports);

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse?)null);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync((Listing?)null);

        // Act
        var result = await _reportService.GetReportsByListingIdAsync(listingId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingReport_ReturnsReport()
    {
        // Arrange
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var report = new Report(
            Guid.NewGuid(),
            listingId,
            ReportReason.Spam,
            "Test description"
        ) with { Id = reportId };

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _userServiceClientMock
            .Setup(c => c.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync((EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses.GetUserResponse?)null);

        _listingRepositoryMock
            .Setup(r => r.GetByIdAsync(listingId))
            .ReturnsAsync((Listing?)null);

        // Act
        var result = await _reportService.GetByIdAsync(reportId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reportId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentReport_ReturnsNull()
    {
        // Arrange
        var reportId = Guid.NewGuid();

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        // Act
        var result = await _reportService.GetByIdAsync(reportId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_AsAdmin_DeletesReport()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var report = new Report(
            reporterId,
            listingId,
            ReportReason.Spam,
            "Test description"
        );

        _currentUserServiceMock
            .Setup(s => s.GetUserId())
            .Returns(adminId);

        _currentUserServiceMock
            .Setup(s => s.HasPermission(It.IsAny<string>()))
            .Returns(true);

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _reportRepositoryMock
            .Setup(r => r.DeleteAsync(reportId))
            .Returns(Task.CompletedTask);

        // Act
        await _reportService.DeleteAsync(reportId);

        // Assert
        _reportRepositoryMock.Verify(r => r.DeleteAsync(reportId), Times.Once);
    }
}

