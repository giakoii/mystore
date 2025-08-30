using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.Users;

public record UserRoleSelectResponse : AbstractApiResponse<UserRoleSelectEntity>
{
    public override UserRoleSelectEntity Response { get; set; }
}

public record UserRoleSelectEntity
{
    public string UserRole { get; set; }
}