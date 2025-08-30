using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStoreManagement.Application.Dtos.Pricings;
using MyStoreManagement.Application.Interfaces.Pricings;
using MyStoreManagement.Application.Utils.Const;
using OpenIddict.Validation.AspNetCore;

namespace MyStoreManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PricingController : ControllerBase
{
    private readonly IPricingService _pricingService;

    public PricingController(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    /// <summary>
    /// Create a new pricing batch every day
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("batch")]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<PricingBatchCreateResponse> CreatePricingBatch(PricingBatchCreateRequest request)
    {
        return await _pricingService.CreatePricingBatchAsync(request);
    }

    /// <summary>
    /// Get pricing batches with pagination and filtering
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("batches")]
    public async Task<PricingBatchSelectsResponse> GetPricingBatches([FromQuery] PricingBatchSelectsRequest request)
    {
        return await _pricingService.GetPricingBatchesAsync(request);
    }
}