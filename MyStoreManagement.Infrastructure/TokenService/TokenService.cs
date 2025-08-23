using System.Security.Claims;
using AuthService.Application.Interfaces.TokenServices;
using Microsoft.IdentityModel.Tokens;
using MyStoreManagement.Application.Interfaces.Dtos;
using MyStoreManagement.Application.Interfaces.IdentityHepers;
using MyStoreManagement.Application.Interfaces.TokenServices;
using OpenIddict.Abstractions;
using Shared.Common.Utils.Const;

namespace MyStoreManagement.Infrastructure.TokenService;

public class TokenService : ITokenService
{
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly IIdentityService _identityApiClient;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="scopeManager"></param>
    /// <param name="identityApiClient"></param>
    /// <param name="tokenManager"></param>
    public TokenService(IOpenIddictScopeManager scopeManager, IIdentityService identityApiClient, IOpenIddictTokenManager tokenManager)
    {
        _scopeManager = scopeManager;
        _identityApiClient = identityApiClient;
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Generate a new ClaimsPrincipal for the user based on the UserLoginDto.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<ClaimsPrincipal> GenerateClaimsPrincipal(UserLoginDto user)
    {
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role
        );
        
        // Set claims for the identity
        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.UserId.ToString(), OpenIddictConstants.Destinations.AccessToken);
        identity.SetClaim(OpenIddictConstants.Claims.Name, user.FullName, OpenIddictConstants.Destinations.AccessToken);
        identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email, OpenIddictConstants.Destinations.AccessToken);
        identity.SetClaim(OpenIddictConstants.Claims.Role, user.RoleName, OpenIddictConstants.Destinations.AccessToken);
        identity.SetClaim(OpenIddictConstants.Claims.Audience, ConstToken.Audience, OpenIddictConstants.Destinations.AccessToken);

        // Destination rules
        identity.SetDestinations(claim =>
        {
            return claim.Type switch
            {
                _ => new[] { OpenIddictConstants.Destinations.AccessToken }
            };
        });
        
        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(
            OpenIddictConstants.Scopes.Roles, 
            OpenIddictConstants.Scopes.OfflineAccess, 
            OpenIddictConstants.Scopes.Profile);

        // Set resources
        var resources = await _scopeManager.ListResourcesAsync(claimsPrincipal.GetScopes()).ToListAsync();

        claimsPrincipal.SetResources(resources);

        // Set lifetime
        claimsPrincipal.SetAccessTokenLifetime(TimeSpan.FromHours(1));
        claimsPrincipal.SetRefreshTokenLifetime(TimeSpan.FromHours(2));

        return claimsPrincipal;
    }

    /// <summary>
    /// Generate a new ClaimsPrincipal from an existing one, typically used for refresh tokens.
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public async Task<ClaimsPrincipal> GenerateClaimsPrincipal(ClaimsPrincipal claimsPrincipal)
    {
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role
        );

        // Copy existing claims from the refresh token
        foreach (var claim in claimsPrincipal.Claims)
        {
            // Copy essential claims
            switch (claim.Type)
            {
                case OpenIddictConstants.Claims.Subject:
                    identity.SetClaim(OpenIddictConstants.Claims.Subject, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken);
                    break;
                case OpenIddictConstants.Claims.Name:
                    identity.SetClaim(OpenIddictConstants.Claims.Name, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken);
                    break;
                case OpenIddictConstants.Claims.Email:
                    identity.SetClaim(OpenIddictConstants.Claims.Email, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken);
                    break;
                case OpenIddictConstants.Claims.PhoneNumber:
                    identity.SetClaim(OpenIddictConstants.Claims.PhoneNumber, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken);
                    break;
                case OpenIddictConstants.Claims.Address:
                    identity.SetClaim(OpenIddictConstants.Claims.Address, claim.Value);
                    break;
                case OpenIddictConstants.Claims.Birthdate:
                    identity.SetClaim(OpenIddictConstants.Claims.Birthdate, claim.Value);
                    break;
                case OpenIddictConstants.Claims.Role:
                    identity.SetClaim(OpenIddictConstants.Claims.Role, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken);
                    break;
                case OpenIddictConstants.Claims.Audience:
                    identity.SetClaim(OpenIddictConstants.Claims.Audience, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken);
                    break;
            }
        }

        // Set destinations for claims
        identity.SetDestinations(claim =>
        {
            return claim.Type switch
            {
                _ => new[] { OpenIddictConstants.Destinations.AccessToken }
            };
        });

        // Create new claims principal
        var newClaimsPrincipal = new ClaimsPrincipal(identity);

        // Set scopes (preserve original scopes)
        newClaimsPrincipal.SetScopes(claimsPrincipal.GetScopes());

        // Set resources
        newClaimsPrincipal.SetResources(await _scopeManager.ListResourcesAsync(newClaimsPrincipal.GetScopes())
            .ToListAsync());

        // Set token lifetimes
        // Set token lifetimes
        newClaimsPrincipal.SetAccessTokenLifetime(TimeSpan.FromHours(1));
        newClaimsPrincipal.SetRefreshTokenLifetime(TimeSpan.FromHours(2));

        return newClaimsPrincipal;
    }
    
    public async Task<(bool Success, string Message)> LogoutAsync(ClaimsPrincipal user, string accessToken)
    {
        var identity = _identityApiClient.GetIdentity(user);

        if (identity == null)
        {
            return (false, "Invalid user identity.");
        }

        // Revoke the current access token if exists
        if (!string.IsNullOrEmpty(accessToken))
        {
            var token = await _tokenManager.FindByReferenceIdAsync(accessToken);
            if (token != null)
            {
                await _tokenManager.TryRevokeAsync(token);
            }
        }

        // Revoke all active tokens for this user
        await foreach (var token in _tokenManager.FindBySubjectAsync(identity.UserId))
        {
            await _tokenManager.TryRevokeAsync(token);
        }

        return (true, "Logged out successfully. All tokens have been revoked.");
    }

    /// <summary>
    /// Retrieves the current user's session details (email, name, user ID, role).
    /// </summary>
    /// <returns></returns>
    public UserSessionResponse UserSession()
    {
        var currentUser = _identityApiClient.GetCurrentUser();

        return new UserSessionResponse
        {
            Email = currentUser!.Email,
            Name = currentUser.FullName,
            UserId = Guid.Parse(currentUser.UserId),
            Role = currentUser.RoleName,
        };
    }
}