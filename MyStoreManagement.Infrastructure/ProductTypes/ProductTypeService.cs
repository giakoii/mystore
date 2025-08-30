using MyStoreManagement.Application.Dtos.ProductTypes;
using MyStoreManagement.Application.Interfaces.ProductTypes;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Domain.Models;
using Shared.Application.Utils.Const;

namespace MyStoreManagement.Infrastructure.ProductTypes;

public class ProductTypeService : IProductTypeService
{
    private readonly IRepository<ProductType> _productTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductTypeService(IRepository<ProductType> productTypeRepository, IUnitOfWork unitOfWork)
    {
        _productTypeRepository = productTypeRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Create a new product type
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ProductTypeCreateResponse> CreateProductTypeAsync(ProductTypeCreateRequest request)
    {
        var response = new ProductTypeCreateResponse {Success = false};
        
        var productTypeExists = await _productTypeRepository.FirstOrDefaultAsync(pt => pt.TypeName == request.TypeName);
        if (productTypeExists != null)
        {
            response.SetMessage(MessageId.E00000, "Product type already exists.");
            return response;
        }
        
        var newProductType = new ProductType
        {
            TypeName = request.TypeName
        };
        
        await _productTypeRepository.AddAsync(newProductType);
        await _unitOfWork.SaveChangesAsync();
        
        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }

    /// <summary>
    /// Get product types for select options
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ProductTypeSelectsResponse> SelectProductTypesAsync(ProductTypeSelectsRequest request)
    {
        var response = new ProductTypeSelectsResponse {Success = false};
        
        var productTypes = await _productTypeRepository.ToListAsync();
        response.Response = productTypes.Select(pt => new ProductTypeSelectsEntity
            {
                ProductTypeId = pt.ProductTypeId,
                TypeName = pt.TypeName
            }
        ).ToList();
        
        // True
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }

    /// <summary>
    /// Update an existing product type
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ProductTypeUpdateResponse> UpdateProductTypeAsync(ProductTypeUpdateRequest request)
    {
        var response = new ProductTypeUpdateResponse { Success = false };
        
        var existingProductType = await _productTypeRepository.FirstOrDefaultAsync(pt => pt.ProductTypeId == request.ProductTypeId);
        if (existingProductType == null)
        {
            response.SetMessage(MessageId.E00000, "Product type not found.");
            return response;
        }
        
        // Check if the new type name already exists (excluding current product type)
        var duplicateProductType = await _productTypeRepository.FirstOrDefaultAsync(pt => pt.TypeName == request.TypeName && pt.ProductTypeId != request.ProductTypeId);
        if (duplicateProductType != null)
        {
            response.SetMessage(MessageId.E00000, "Product type name already exists.");
            return response;
        }
        
        existingProductType.TypeName = request.TypeName;
        _productTypeRepository.Update(existingProductType);
        await _unitOfWork.SaveChangesAsync();
        
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }

    /// <summary>
    /// Delete a product type
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ProductTypeDeleteResponse> DeleteProductTypeAsync(ProductTypeDeleteRequest request)
    {
        var response = new ProductTypeDeleteResponse { Success = false };
        
        var existingProductType = await _productTypeRepository.FirstOrDefaultAsync(pt => pt.ProductTypeId == request.ProductTypeId);
        if (existingProductType == null)
        {
            response.SetMessage(MessageId.E00000, "Product type not found.");
            return response;
        }
        
        // Check if product type is being used in any orders or product prices
        if (existingProductType.OrderDetails.Any() || existingProductType.ProductPrices.Any())
        {
            response.SetMessage(MessageId.E00000, "Cannot delete product type as it is being used in orders or product pricing.");
            return response;
        }
        
        _productTypeRepository.Delete(existingProductType);
        await _unitOfWork.SaveChangesAsync();
        
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}