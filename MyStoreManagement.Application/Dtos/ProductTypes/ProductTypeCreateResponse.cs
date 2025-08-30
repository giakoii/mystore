using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.ProductTypes;

public record ProductTypeCreateResponse : AbstractApiResponse<string>
{
    public override string Response { get; set; }
}