using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.ProductTypes;

public record ProductTypeDeleteResponse : AbstractApiResponse<string>
{
    public override string Response { get; set; }
}
