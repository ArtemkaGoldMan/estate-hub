using CSharpFunctionalExtensions;
using EstateHub.Authorization.Core.Services;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.User;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Domain.Interfaces.DataAccessInterfaces;
using EstateHub.SharedKernel.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EstateHub.Authorization.Core.Tests;

public class UsersServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly Mock<ISessionsRepository> _sessionsRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UsersService>> _loggerMock;
    private readonly UsersService _usersService;

    public UsersServiceTests()
    {
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _sessionsRepositoryMock = new Mock<ISessionsRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UsersService>>();

        // Setup unit of work to return success
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(Result.Success(true));
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .ReturnsAsync(Result.Success(true));

        _usersService = new UsersService(
            _usersRepositoryMock.Object,
            _sessionsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(user);

        // Act
        var result = await _usersService.GetByIdAsync<UserDto>(userId, false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.Id);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyGuid_ReturnsFailure()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _usersService.GetByIdAsync<UserDto>(emptyId, false);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _usersService.GetByIdAsync<UserDto>(userId, false);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithIncludeDeleted_IncludesDeletedUsers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deletedUser = new UserDto
        {
            Id = userId,
            Email = "deleted@example.com",
            UserName = "deleted@example.com",
            DisplayName = "Deleted User",
            IsDeleted = true
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, true))
            .ReturnsAsync(deletedUser);

        // Act
        var result = await _usersService.GetByIdAsync<UserDto>(userId, true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsDeleted);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, true), Times.Once);
    }

    #endregion

    #region GetByIdsAsync Tests

    [Fact]
    public async Task GetByIdsAsync_WithValidIds_ReturnsUsers()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var ids = new List<Guid> { userId1, userId2 };

        var users = new List<UserDto>
        {
            new UserDto { Id = userId1, Email = "user1@example.com", UserName = "user1@example.com", DisplayName = "User 1", IsDeleted = false },
            new UserDto { Id = userId2, Email = "user2@example.com", UserName = "user2@example.com", DisplayName = "User 2", IsDeleted = false }
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdsAsync<UserDto>(ids, false))
            .ReturnsAsync(users);

        // Act
        var result = await _usersService.GetByIdsAsync<UserDto>(ids, false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        _usersRepositoryMock.Verify(r => r.GetByIdsAsync<UserDto>(ids, false), Times.Once);
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ReturnsFailure()
    {
        // Arrange
        var emptyIds = new List<Guid>();

        // Act
        var result = await _usersService.GetByIdsAsync<UserDto>(emptyIds, false);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdsAsync<UserDto>(It.IsAny<List<Guid>>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdsAsync_WithNoMatchingUsers_ReturnsFailure()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _usersRepositoryMock
            .Setup(r => r.GetByIdsAsync<UserDto>(ids, false))
            .ReturnsAsync(new List<UserDto>());

        // Act
        var result = await _usersService.GetByIdsAsync<UserDto>(ids, false);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdsAsync<UserDto>(ids, false), Times.Once);
    }

    #endregion

    #region UpdateByIdAsync Tests

    [Fact]
    public async Task UpdateByIdAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Old Name",
            IsDeleted = false
        };

        var updateRequest = new UserUpdateRequest
        {
            DisplayName = "New Name",
            PhoneNumber = "+1234567890",
            Country = "USA",
            City = "New York"
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(existingUser);

        _usersRepositoryMock
            .Setup(r => r.UpdateByIdAsync(userId, It.IsAny<UserUpdateDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.UpdateByIdAsync(userId, updateRequest);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
        _usersRepositoryMock.Verify(r => r.UpdateByIdAsync(userId, It.IsAny<UserUpdateDto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateByIdAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UserUpdateRequest { DisplayName = "New Name" };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _usersService.UpdateByIdAsync(userId, updateRequest);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
        _usersRepositoryMock.Verify(r => r.UpdateByIdAsync(It.IsAny<Guid>(), It.IsAny<UserUpdateDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateByIdAsync_WithEmptyDisplayName_UsesExistingDisplayName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Existing Name",
            IsDeleted = false
        };

        var updateRequest = new UserUpdateRequest
        {
            DisplayName = string.Empty // Empty display name
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(existingUser);

        _usersRepositoryMock
            .Setup(r => r.UpdateByIdAsync(userId, It.IsAny<UserUpdateDto>()))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.UpdateByIdAsync(userId, updateRequest);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.UpdateByIdAsync(userId, It.Is<UserUpdateDto>(dto => 
            dto.DisplayName == existingUser.DisplayName)), Times.Once);
    }

    #endregion

    #region DeleteByIdAsync Tests

    [Fact]
    public async Task DeleteByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User",
            IsDeleted = false
        };

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync(user);

        _usersRepositoryMock
            .Setup(r => r.DeleteByIdAsync(userId))
            .ReturnsAsync(true);

        _sessionsRepositoryMock
            .Setup(r => r.DeleteByUserIdAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.DeleteByIdAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
        _usersRepositoryMock.Verify(r => r.DeleteByIdAsync(userId), Times.Once);
        _sessionsRepositoryMock.Verify(r => r.DeleteByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithEmptyGuid_ReturnsFailure()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _usersService.DeleteByIdAsync(emptyId);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteByIdAsync_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserDto>(userId, false))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _usersService.DeleteByIdAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserDto>(userId, false), Times.Once);
        _usersRepositoryMock.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region GetUsersAsync Tests (Admin)

    [Fact]
    public async Task GetUsersAsync_WithValidPagination_ReturnsPagedUsers()
    {
        // Arrange
        var page = 1;
        var pageSize = 20;
        var users = new List<UserDto>
        {
            new UserDto { Id = Guid.NewGuid(), Email = "user1@example.com", UserName = "user1@example.com", DisplayName = "User 1", IsDeleted = false },
            new UserDto { Id = Guid.NewGuid(), Email = "user2@example.com", UserName = "user2@example.com", DisplayName = "User 2", IsDeleted = false }
        };
        var total = 2;

        _usersRepositoryMock
            .Setup(r => r.GetUsersAsync<UserDto>(page, pageSize, false))
            .ReturnsAsync(users);

        _usersRepositoryMock
            .Setup(r => r.GetUsersCountAsync(false))
            .ReturnsAsync(total);

        // Act
        var result = await _usersService.GetUsersAsync<UserDto>(page, pageSize, false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(total, result.Value.Total);
        Assert.Equal(page, result.Value.Page);
        Assert.Equal(pageSize, result.Value.PageSize);
        _usersRepositoryMock.Verify(r => r.GetUsersAsync<UserDto>(page, pageSize, false), Times.Once);
        _usersRepositoryMock.Verify(r => r.GetUsersCountAsync(false), Times.Once);
    }

    #endregion

    #region GetUserStatsAsync Tests (Admin)

    [Fact]
    public async Task GetUserStatsAsync_ReturnsUserStatistics()
    {
        // Arrange
        var totalUsers = 100;
        var activeUsers = 80;
        var suspendedUsers = 10;
        var newUsersThisMonth = 5;

        _usersRepositoryMock
            .Setup(r => r.GetUsersCountAsync(false))
            .ReturnsAsync(totalUsers);

        _usersRepositoryMock
            .Setup(r => r.GetActiveUsersCountAsync())
            .ReturnsAsync(activeUsers);

        _usersRepositoryMock
            .Setup(r => r.GetSuspendedUsersCountAsync())
            .ReturnsAsync(suspendedUsers);

        _usersRepositoryMock
            .Setup(r => r.GetNewUsersThisMonthCountAsync())
            .ReturnsAsync(newUsersThisMonth);

        // Act
        var result = await _usersService.GetUserStatsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(totalUsers, result.Value.TotalUsers);
        Assert.Equal(activeUsers, result.Value.ActiveUsers);
        Assert.Equal(suspendedUsers, result.Value.SuspendedUsers);
        Assert.Equal(newUsersThisMonth, result.Value.NewUsersThisMonth);
    }

    #endregion

    #region AssignUserRoleAsync Tests (Admin)

    [Fact]
    public async Task AssignUserRoleAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = "Admin";

        _usersRepositoryMock
            .Setup(r => r.AssignUserRoleAsync(userId, role))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.AssignUserRoleAsync(userId, role);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.AssignUserRoleAsync(userId, role), Times.Once);
    }

    [Fact]
    public async Task AssignUserRoleAsync_WithEmptyGuid_ReturnsFailure()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var role = "Admin";

        // Act
        var result = await _usersService.AssignUserRoleAsync(emptyId, role);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.AssignUserRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AssignUserRoleAsync_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = "Admin";

        _usersRepositoryMock
            .Setup(r => r.AssignUserRoleAsync(userId, role))
            .ReturnsAsync(false);

        // Act
        var result = await _usersService.AssignUserRoleAsync(userId, role);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.AssignUserRoleAsync(userId, role), Times.Once);
    }

    #endregion

    #region RemoveUserRoleAsync Tests (Admin)

    [Fact]
    public async Task RemoveUserRoleAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = "User";

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserWithRolesDto>(userId, false))
            .ReturnsAsync(new UserWithRolesDto
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "test@example.com",
                DisplayName = "Test User",
                IsDeleted = false,
                Roles = new List<string> { "User" }
            });

        _usersRepositoryMock
            .Setup(r => r.RemoveUserRoleAsync(userId, role))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.RemoveUserRoleAsync(userId, role);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.RemoveUserRoleAsync(userId, role), Times.Once);
    }

    [Fact]
    public async Task RemoveUserRoleAsync_WhenRemovingOwnAdminRole_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = "Admin";

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserWithRolesDto>(userId, false))
            .ReturnsAsync(new UserWithRolesDto
            {
                Id = userId,
                Email = "admin@example.com",
                UserName = "admin@example.com",
                DisplayName = "Admin User",
                IsDeleted = false,
                Roles = new List<string> { "Admin" }
            });

        // Act
        var result = await _usersService.RemoveUserRoleAsync(userId, role, userId);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.GetByIdAsync<UserWithRolesDto>(userId, false), Times.Once);
        _usersRepositoryMock.Verify(r => r.RemoveUserRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RemoveUserRoleAsync_WhenRemovingOtherUserAdminRole_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var role = "Admin";

        _usersRepositoryMock
            .Setup(r => r.GetByIdAsync<UserWithRolesDto>(userId, false))
            .ReturnsAsync(new UserWithRolesDto
            {
                Id = userId,
                Email = "admin@example.com",
                UserName = "admin@example.com",
                DisplayName = "Admin User",
                IsDeleted = false,
                Roles = new List<string> { "Admin" }
            });

        _usersRepositoryMock
            .Setup(r => r.RemoveUserRoleAsync(userId, role))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.RemoveUserRoleAsync(userId, role, currentUserId);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.RemoveUserRoleAsync(userId, role), Times.Once);
    }

    #endregion

    #region SuspendUserAsync Tests (Admin)

    [Fact]
    public async Task SuspendUserAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reason = "Violation of terms";

        _usersRepositoryMock
            .Setup(r => r.SuspendUserAsync(userId, reason))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.SuspendUserAsync(userId, reason);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.SuspendUserAsync(userId, reason), Times.Once);
    }

    [Fact]
    public async Task SuspendUserAsync_WithEmptyGuid_ReturnsFailure()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var reason = "Violation";

        // Act
        var result = await _usersService.SuspendUserAsync(emptyId, reason);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.SuspendUserAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region ActivateUserAsync Tests (Admin)

    [Fact]
    public async Task ActivateUserAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _usersRepositoryMock
            .Setup(r => r.ActivateUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.ActivateUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.ActivateUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task ActivateUserAsync_WithEmptyGuid_ReturnsFailure()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _usersService.ActivateUserAsync(emptyId);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.ActivateUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region AdminDeleteUserAsync Tests (Admin)

    [Fact]
    public async Task AdminDeleteUserAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _usersRepositoryMock
            .Setup(r => r.DeleteByIdAsync(userId))
            .ReturnsAsync(true);

        _sessionsRepositoryMock
            .Setup(r => r.DeleteByUserIdAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _usersService.AdminDeleteUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _usersRepositoryMock.Verify(r => r.DeleteByIdAsync(userId), Times.Once);
        _sessionsRepositoryMock.Verify(r => r.DeleteByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task AdminDeleteUserAsync_WithEmptyGuid_ReturnsFailure()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _usersService.AdminDeleteUserAsync(emptyId);

        // Assert
        Assert.True(result.IsFailure);
        _usersRepositoryMock.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion
}


