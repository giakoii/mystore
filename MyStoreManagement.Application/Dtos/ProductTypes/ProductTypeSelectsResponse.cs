using MyStoreManagement.Application.ApiEntities;

namespace MyStoreManagement.Application.Dtos.ProductTypes;

public record ProductTypeSelectsResponse : AbstractApiResponse<List<ProductTypeSelectsEntity>>
{
    public override List<ProductTypeSelectsEntity> Response { get; set; }
}

public class ProductTypeSelectsEntity
{
    public int ProductTypeId { get; set; }
    
    public string TypeName { get; set; }
}