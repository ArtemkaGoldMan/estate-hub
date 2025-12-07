using AutoMapper;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Core.Services.Authentication;
using EstateHub.Authorization.Core.Helpers;
using EstateHub.SharedKernel.API.Extensions;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace EstateHub.Authorization.API.Controllers;

[Authorize]
public class AuthorizationController : SessionAwareControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;

    public AuthorizationController(
        IAuthenticationService authenticationService,
        IMapper mapper,
        IWebHostEnvironment environment)
    {
        _authenticationService = authenticationService;
        _mapper = mapper;
        _environment = environment;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">User registration details including email, password, and confirmation callback URL.</param>
    /// <returns>
    /// Returns AuthenticationResponse if email confirmation is not required (user is auto-logged in).
    /// Returns empty response if email confirmation is required (confirmation email will be sent).
    /// </returns>
    /// <response code="200">Registration successful. Returns authentication response if auto-login enabled, otherwise empty.</response>
    /// <response code="400">Registration failed due to validation errors or duplicate email.</response>
    [AllowAnonymous]
    [HttpPost("/user-registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> UserRegistrationAsync(
        [FromBody] UserRegistrationRequest request)
    {
        var registrationResult = await _authenticationService.RegisterAsync(request);

        if (registrationResult.IsFailure)
        {
            return registrationResult.ToProblemDetails();
        }

        if (registrationResult.Value is null)
        {
            return Results.Ok();
        }

        Response.Cookies.Append(
            DefaultAuthenticationTypes.ApplicationCookie, 
            registrationResult.Value.RefreshToken, 
            CookieHelper.CreateRefreshTokenCookieOptions(_environment));

        return Results.Ok(_mapper.Map<AuthenticationResponse>(registrationResult.Value));
    }

    /// <summary>
    /// Confirms user email address using the token received via email.
    /// </summary>
    /// <param name="request">Email confirmation request containing token and user ID.</param>
    /// <returns>Authentication response with access and refresh tokens if confirmation succeeds.</returns>
    /// <response code="200">Email confirmed successfully. Returns authentication tokens.</response>
    /// <response code="400">Confirmation failed due to invalid token or user ID.</response>
    [AllowAnonymous]
    [HttpPatch("/confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        var result = await _authenticationService.ConfirmEmailAsync(request);

        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        Response.Cookies.Append(
            DefaultAuthenticationTypes.ApplicationCookie, 
            result.Value.RefreshToken, 
            CookieHelper.CreateRefreshTokenCookieOptions(_environment));

        return Results.Ok(_mapper.Map<AuthenticationResponse>(result.Value));
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="request">Login credentials (email and password).</param>
    /// <returns>Authentication response containing access token, refresh token, and user information.</returns>
    /// <response code="200">Login successful. Returns authentication tokens and user information.</response>
    /// <response code="400">Login failed due to invalid credentials, locked account, or unconfirmed email.</response>
    [AllowAnonymous]
    [HttpPost("/login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> LogInAsync([FromBody] LoginRequest request)
    {
        var loginResult = await _authenticationService.LoginAsync(request);

        if (loginResult.IsFailure)
        {
            return loginResult.ToProblemDetails();
        }

        Response.Cookies.Append(
            DefaultAuthenticationTypes.ApplicationCookie, 
            loginResult.Value.RefreshToken, 
            CookieHelper.CreateRefreshTokenCookieOptions(_environment));

        return Results.Ok(_mapper.Map<AuthenticationResponse>(loginResult.Value));
    }

    /// <summary>
    /// Initiates password reset process by sending a reset token to the user's email.
    /// </summary>
    /// <param name="request">Password reset request containing email and return URL.</param>
    /// <returns>Empty response if email sent successfully.</returns>
    /// <response code="200">Password reset email sent successfully (if user exists).</response>
    /// <response code="400">Request failed due to invalid email format.</response>
    [AllowAnonymous]
    [HttpPost("/forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authenticationService.ForgotPasswordAsync(request);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    /// <summary>
    /// Resets user password using the token received via email.
    /// </summary>
    /// <param name="request">Password reset request containing token, user ID, and new password.</param>
    /// <returns>Empty response if password reset succeeds.</returns>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Password reset failed due to invalid token, expired token, or password validation failure.</response>
    [AllowAnonymous]
    [HttpPut("/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        var result = await _authenticationService.ResetPasswordAsync(request);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    /// <summary>
    /// Initiates account state management (e.g., account recovery or hard delete request).
    /// Sends a confirmation token to the user's email.
    /// </summary>
    /// <param name="request">Account management request containing email, action type, and return URL.</param>
    /// <returns>Empty response if email sent successfully.</returns>
    /// <response code="200">Account management email sent successfully.</response>
    /// <response code="400">Request failed due to invalid email or user not found.</response>
    [AllowAnonymous]
    [HttpPut("/manage-account-state")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ManageAccountStateAsync([FromBody] ManageAccountRequest request)
    {
        var result = await _authenticationService.ManageAccountState(request);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    /// <summary>
    /// Confirms an account action (e.g., account recovery or hard delete) using the token received via email.
    /// </summary>
    /// <param name="request">Account action confirmation request containing token, user ID, and action type.</param>
    /// <returns>Empty response if action confirmed successfully.</returns>
    /// <response code="200">Account action confirmed successfully.</response>
    /// <response code="400">Confirmation failed due to invalid token, expired token, or invalid action type.</response>
    [AllowAnonymous]
    [HttpPatch("/confirm-account-action")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ConfirmAccountActionAsync([FromBody] ConfirmAccountActionRequest request)
    {
        var result = await _authenticationService.ConfirmAccountAction(request);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token from cookies.
    /// </summary>
    /// <returns>New authentication response with updated access token and refresh token.</returns>
    /// <response code="200">Token refreshed successfully. Returns new authentication tokens.</response>
    /// <response code="400">Token refresh failed due to missing refresh token, invalid token, or expired session.</response>
    [AllowAnonymous]
    [HttpPost("/refresh-access-token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> RefreshAccessTokenAsync()
    {
        var refreshToken = Request.Cookies[DefaultAuthenticationTypes.ApplicationCookie];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Results.BadRequest(AuthorizationErrors.NotFoundRefreshToken());
        }

        var result = await _authenticationService.RefreshAccessTokenAsync(refreshToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            Response.Cookies.Delete(DefaultAuthenticationTypes.ApplicationCookie);
            return result.ToProblemDetails();
        }

        return Results.Ok(result.Value);
    }

    /// <summary>
    /// Logs out the current user by invalidating the refresh token session.
    /// </summary>
    /// <returns>Empty response if logout succeeds.</returns>
    /// <response code="200">Logout successful. Session invalidated.</response>
    /// <response code="400">Logout failed due to missing or invalid refresh token.</response>
    [AllowAnonymous]
    [HttpPost("/logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> LogoutAsync()
    {
        string? refreshToken = Request.Cookies[DefaultAuthenticationTypes.ApplicationCookie];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Results.BadRequest(AuthorizationErrors.NotFoundRefreshToken());
        }

        var result = await _authenticationService.LogoutAsync(refreshToken).ConfigureAwait(false);

        Response.Cookies.Delete(DefaultAuthenticationTypes.ApplicationCookie);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    /// <summary>
    /// Gets the current user's ID from the JWT token.
    /// Used primarily for microservice-to-microservice communication.
    /// </summary>
    /// <returns>User ID extracted from the authenticated token.</returns>
    /// <response code="200">Returns the user ID from the token.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("/user-id-from-token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserIdFromTokenResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetUserInfoAsync()
    {
        var userId = UserId;

        return Results.Ok(new UserIdFromTokenResponse
        {
            UserId = userId,
        });
    }
}
