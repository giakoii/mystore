using System.Security.Claims;

namespace MyStoreManagement.Application.Interfaces.IdentityHepers;

public interface IIdentityService
{
    IdentityEntity? GetIdentity(ClaimsPrincipal user);
    
    IdentityEntity? GetCurrentUser();
}

