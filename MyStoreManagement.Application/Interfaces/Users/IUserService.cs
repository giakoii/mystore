using MyStoreManagement.Application.Dtos;
using MyStoreManagement.Application.Dtos.Users;

namespace MyStoreManagement.Application.Interfaces.Users;

public interface IUserService
{
    Task<UserLoginResponse?> LoginAsync(UserLoginRequest request);
    
    Task<UserCreateResponse> CreateUserAsync(UserCreateRequest request);
    Task<UserRoleSelectResponse> UserRoleSelectAsync(UserRoleSelectRequest request);
}