using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStoreManagement.Application.Dtos.ProductTypes;
using MyStoreManagement.Application.Interfaces.ProductTypes;
using MyStoreManagement.Application.Utils.Const;
using OpenIddict.Validation.AspNetCore;

namespace MyStoreManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductTypeController : ControllerBase
{ 
    private readonly IProductTypeService _productTypeService;

    public ProductTypeController(IProductTypeService productTypeService)
    {
        _productTypeService = productTypeService;
    }
    
    [HttpPost]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<ProductTypeCreateResponse> CreateProductType(ProductTypeCreateRequest request)
    {
        return await _productTypeService.CreateProductTypeAsync(request);
    }

    [HttpGet]
    public async Task<ProductTypeSelectsResponse> SelectProductTypes([FromQuery] ProductTypeSelectsRequest request)
    {
        return await _productTypeService.SelectProductTypesAsync(request);
    }

    [HttpPut]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<ProductTypeUpdateResponse> UpdateProductType(ProductTypeUpdateRequest request)
    {
        return await _productTypeService.UpdateProductTypeAsync(request);
    }

    [HttpDelete]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<ProductTypeDeleteResponse> DeleteProductType(ProductTypeDeleteRequest request)
    {
        return await _productTypeService.DeleteProductTypeAsync(request);
    }
}