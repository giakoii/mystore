using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.ProductTypes;

public record ProductTypeUpdateResponse : AbstractApiResponse<string>
{
    public override string Response { get; set; }
}
