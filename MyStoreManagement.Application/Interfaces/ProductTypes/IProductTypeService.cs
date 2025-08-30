using MyStoreManagement.Application.Dtos.ProductTypes;

namespace MyStoreManagement.Application.Interfaces.ProductTypes;

public interface IProductTypeService
{
    Task<ProductTypeCreateResponse> CreateProductTypeAsync(ProductTypeCreateRequest request);
    
    Task<ProductTypeSelectsResponse> SelectProductTypesAsync(ProductTypeSelectsRequest request);
    
    Task<ProductTypeUpdateResponse> UpdateProductTypeAsync(ProductTypeUpdateRequest request);
    
    Task<ProductTypeDeleteResponse> DeleteProductTypeAsync(ProductTypeDeleteRequest request);
}