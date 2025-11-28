using AutoMapper;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.DTO.Authentication.Responses;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.Authorization.Core.Services.Authentication;
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

    public AuthorizationController(
        IAuthenticationService authenticationService,
        IMapper mapper)
    {
        _authenticationService = authenticationService;
        _mapper = mapper;
    }

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

        Response.Cookies.Append(DefaultAuthenticationTypes.ApplicationCookie, registrationResult.Value.RefreshToken, new CookieOptions()
        {
            Secure = false,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return Results.Ok(_mapper.Map<AuthenticationResponse>(registrationResult.Value));
    }

    [AllowAnonymous]
    [HttpPatch("/confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        var result = await _authenticationService.ConfirmEmailAsync(request);

        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        Response.Cookies.Append(DefaultAuthenticationTypes.ApplicationCookie, result.Value.RefreshToken, new CookieOptions()
        {
            Secure = false,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return Results.Ok(_mapper.Map<AuthenticationResponse>(result.Value));
    }

    /// <summary>
    /// Users login.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <returns>Jwt token.</returns>
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

        Response.Cookies.Append(DefaultAuthenticationTypes.ApplicationCookie, loginResult.Value.RefreshToken, new CookieOptions()
        {
            Secure = false,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return Results.Ok(_mapper.Map<AuthenticationResponse>(loginResult.Value));
    }

    [AllowAnonymous]
    [HttpPost("/forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authenticationService.ForgotPasswordAsync(request);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    [AllowAnonymous]
    [HttpPut("/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        var result = await _authenticationService.ResetPasswordAsync(request);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    [AllowAnonymous]
    [HttpPut("/manage-account-state")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ManageAccountStateAsync([FromBody] ManageAccountRequest request)
    {
        var result = await _authenticationService.ManageAccountState(request);

        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

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
    /// Refresh access token.
    /// </summary>
    /// <param name="request">Access token.</param>
    /// <returns>New access token and refresh token.</returns>
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

    [AllowAnonymous]
    [HttpPost("/logout")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
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

    [HttpGet("/user-id-from-token")]
    public async Task<IResult> GetUserInfoAsync()
    {
        var userId = UserId;

        return Results.Ok(new UserIdFromTokenResponse
        {
            UserId = userId,
        });
    }
}
