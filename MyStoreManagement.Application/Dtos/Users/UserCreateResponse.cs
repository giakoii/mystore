using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.Users;

public record UserCreateResponse : AbstractApiResponse<string>
{
    public override string Response { get; set; }
}