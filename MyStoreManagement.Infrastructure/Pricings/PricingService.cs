using MyStoreManagement.Application.Dtos.Pricings;
using MyStoreManagement.Application.Interfaces.Pricings;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Domain.Models;
using Shared.Application.Utils.Const;

namespace MyStoreManagement.Infrastructure.Pricings;

public class PricingService : IPricingService
{
    private readonly IRepository<PricingBatch> _pricingBatchRepository;
    private readonly IRepository<ProductPrice> _productPriceRepository;
    private readonly IRepository<ProductType> _productTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PricingService(
        IRepository<PricingBatch> pricingBatchRepository,
        IRepository<ProductPrice> productPriceRepository,
        IRepository<ProductType> productTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _pricingBatchRepository = pricingBatchRepository;
        _productPriceRepository = productPriceRepository;
        _productTypeRepository = productTypeRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Tạo mới một đợt cập nhật giá hàng ngày
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<PricingBatchCreateResponse> CreatePricingBatchAsync(PricingBatchCreateRequest request)
    {
        var response = new PricingBatchCreateResponse { Success = false };

        // Validate product types exist
        var productTypeIds = request.PriceDetails.Select(x => x.ProductTypeId).Distinct().ToList();
        var existingProductTypes = await _productTypeRepository.ToListAsync(pt => productTypeIds.Contains(pt.ProductTypeId));
        
        if (existingProductTypes.Count() != productTypeIds.Count)
        {
            response.SetMessage(MessageId.E00000, "Một số loại sản phẩm không tồn tại.");
            return response;
        }

        // Validate prices are positive
        if (request.PriceDetails.Any(x => x.Price <= 0))
        {
            response.SetMessage(MessageId.E00000, "Giá phải lớn hơn 0.");
            return response;
        }

        // Create new pricing batch
        await _unitOfWork.BeginTransactionAsync(async () =>
        {
            var pricingBatch = new PricingBatch
            {
                Title = request.Title,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };

            await _pricingBatchRepository.AddAsync(pricingBatch);
            await _unitOfWork.SaveChangesAsync();

            // Create product prices for this batch
            var productPrices = request.PriceDetails.Select(detail => new ProductPrice
            {
                ProductTypeId = detail.ProductTypeId,
                PricingBatchId = pricingBatch.PricingBatchId,
                Price = detail.Price,
                CreatedAt = DateTime.Now
            }).ToList();

            await _productPriceRepository.AddRangeAsync(productPrices);
            await _unitOfWork.SaveChangesAsync();

            response.Success = true;
            response.SetMessage(MessageId.I00001);
            return true;
        });
        return response;
    }

    /// <summary>
    /// Get pricing batches with pagination and filtering
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<PricingBatchSelectsResponse> GetPricingBatchesAsync(PricingBatchSelectsRequest request)
    {
        var response = new PricingBatchSelectsResponse { Success = false };
    
        // Get all pricing batches with their product prices
        var pricingBatches = await _pricingBatchRepository.ToListAsync(
            null,
            false,
            CancellationToken.None,
            pb => pb.ProductPrices
        );
    
        // Get all product types
        var productTypes = await _productTypeRepository.ToListAsync();
    
        // Map ProductType to each ProductPrice
        foreach (var batch in pricingBatches)
        {
            foreach (var price in batch.ProductPrices)
            {
                price.ProductType = productTypes.FirstOrDefault(pt => pt.ProductTypeId == price.ProductTypeId);
            }
        }
    
        // Map to response DTOs
        response.Response = pricingBatches
            .Select(pb => new PricingBatchSelectsEntity
            {
                PricingBatchId = pb.PricingBatchId,
                Title = pb.Title,
                Description = pb.Description,
                CreatedAt = pb.CreatedAt,
                PriceDetails = pb.ProductPrices.Select(pp => new ProductPriceEntity
                {
                    PriceId = pp.PriceId,
                    ProductTypeId = pp.ProductTypeId,
                    TypeName = pp.ProductType?.TypeName!,
                    Price = pp.Price
                }).ToList()
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    
        response.Success = true;
        response.SetMessage(MessageId.I00001);
        return response;
    }
}
