using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStoreManagement.Application.Dtos.Users;
using MyStoreManagement.Application.Interfaces.TokenServices;
using MyStoreManagement.Application.Interfaces.Users;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;

namespace MyStoreManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public UserController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }
    
    [HttpPost("create")]
    public async Task<UserCreateResponse> CreateUser(UserCreateRequest request)
    {
        return await _userService.CreateUserAsync(request);
    }
    
    [HttpPost("user-role")]
    public async Task<UserRoleSelectResponse> UserRoleSelect(UserRoleSelectRequest request)
    {
        return await _userService.UserRoleSelectAsync(request);
    }
    
    /// <summary>
    /// Exchange token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("~/connect/token")]
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange([FromForm] UserLoginRequest request)
    {
        var openIdRequest = HttpContext.GetOpenIddictServerRequest();

        // Password
        if (openIdRequest!.IsPasswordGrantType())
        {
            return await TokensForPasswordGrantType(request);
        }

        // Refresh token
        if (openIdRequest!.IsRefreshTokenGrantType())
        {
            return await TokensForRefreshTokenGrantType();
        }

        // Unsupported grant type
        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType
        });
    }
    
    /// <summary>
    /// Logout endpoint - revoke tokens properly
    /// </summary>
    /// <returns></returns>
    [HttpPost("~/connect/logout")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Get the access token from the request headers
            var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            // If no access token is provided, return an error
            var result = await _tokenService.LogoutAsync(User, accessToken!);
            if (!result.Success)
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidRequest,
                    ErrorDescription = result.Message
                });
            }

            // Logout the user from OpenIddict
            await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return Ok(new
            {
                Success = true, result.Message
            });
        }
        catch
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.ServerError,
                ErrorDescription = "An error occurred while logging out."
            });
        }
    }

    [HttpGet("/session")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public UserSessionResponse UserSession()
    {
        return _tokenService.UserSession();
    }

    /// <summary>
    /// Handle refresh token grant type
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<IActionResult> TokensForPasswordGrantType(UserLoginRequest request)
    {
        var loginResult = await _userService.LoginAsync(request);
        if (loginResult == null)
        {
            var errorResponse = new OpenIddictResponse();

            errorResponse.Error = OpenIddictConstants.Errors.InvalidGrant;
            errorResponse.ErrorDescription = "The username or password is incorrect.";
            return BadRequest(errorResponse);
        }
        
        var userLoginDto = new UserLoginResponse
        {
            UserId = loginResult.UserId,
            FullName = loginResult.FullName,
            RoleName = loginResult.RoleName,
            PhoneNumber = loginResult.PhoneNumber,
        };

        var claimsPrincipal = await _tokenService.GenerateClaimsPrincipal(userLoginDto);

        // Generate access and refresh tokens
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handle refresh token grant type
    /// </summary>
    /// <returns></returns>
    private async Task<IActionResult> TokensForRefreshTokenGrantType()
    {
        try
        {
            // Authenticate the refresh token
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return Unauthorized(new OpenIddictResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = "The refresh token is invalid."
                });
            }
            
            var claimsPrincipal = authenticateResult.Principal;
            var newClaimsPrincipal = await _tokenService.GenerateClaimsPrincipal(claimsPrincipal);
            
            return SignIn(newClaimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        catch (Exception)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.ServerError,
                ErrorDescription = "An error occurred while processing the refresh token."
            });
        }
    }
}