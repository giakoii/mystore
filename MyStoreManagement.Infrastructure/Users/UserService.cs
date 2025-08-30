using MyStoreManagement.Application.Dtos;
using MyStoreManagement.Application.Dtos.Users;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Application.Interfaces.Users;
using MyStoreManagement.Application.Utils.Const;
using MyStoreManagement.Domain.Models;
using Shared.Application.Utils.Const;

namespace MyStoreManagement.Infrastructure.Users;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IRepository<User> userRepository, IRepository<Role> roleRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserLoginResponse?> LoginAsync(UserLoginRequest request)
    {
        var userExist = await _userRepository.FirstOrDefaultAsync(x => x.Phone == request.PhoneNumber, false, default, r => r.Role);
        if (userExist == null)
        {
            return null;
        }

        if (userExist.Role!.RoleName == nameof(ConstantEnum.UserRole.Admin))
        {
            if (request.Password != userExist.Password)
            {
                return null;
            }
        }
        
        return new UserLoginResponse
        {
            UserId = userExist.UserId,
            FullName = userExist.FullName,
            PhoneNumber = userExist.Phone,
            RoleName = userExist.Role.RoleName,
        };
    }

    public async Task<UserCreateResponse> CreateUserAsync(UserCreateRequest request)
    {
        var response = new UserCreateResponse { Success = false };
        
        var userExists = await _userRepository.FirstOrDefaultAsync(u => u.Phone == request.PhoneNumber);
        if (userExists != null)
        {
            response.SetMessage(MessageId.E00000, "User already exists");
            return response;
        }

        var role = await _roleRepository.FirstOrDefaultAsync(x => x.RoleName == nameof(ConstantEnum.UserRole.Customer));
        if (role == null)
        {
            response.SetMessage(MessageId.E99999);
            return response;
        }
        
        var newUser = new User
        {
            FullName = request.FullName,
            Phone = request.PhoneNumber,
            RoleId = role.RoleId,
        };
        
        await _userRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();
        
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }

    public async Task<UserRoleSelectResponse> UserRoleSelectAsync(UserRoleSelectRequest request)
    {
        var response = new UserRoleSelectResponse { Success = false };
        var userExist = await _userRepository.FirstOrDefaultAsync(x => x.Phone == request.PhoneNumber, false, CancellationToken.None, r => r.Role);
        if (userExist == null)
        {
            response.SetMessage(MessageId.E00000, "User does not exist");
            return response;
        }

        response.Response = new UserRoleSelectEntity
        {
            UserRole = userExist.Role!.RoleName,
        };
        
        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}