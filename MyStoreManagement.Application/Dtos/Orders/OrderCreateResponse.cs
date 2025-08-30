using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.Orders;

public record OrderCreateResponse : AbstractApiResponse<string>
{
    public override string Response { get; set; }
}