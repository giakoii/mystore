using System.Linq.Expressions;
using MyStoreManagement.Application.Dtos.Pricings;
using MyStoreManagement.Application.Interfaces.Pricings;
using MyStoreManagement.Application.Interfaces.Repositories;
using MyStoreManagement.Application.Utils.Paginations;
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
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var pricingBatch = new PricingBatch
            {
                Title = request.Title,
                Description = request.Description,
                CreatedAt = vietnamTime
            };

            await _pricingBatchRepository.AddAsync(pricingBatch);
            await _unitOfWork.SaveChangesAsync();

            // Create product prices for this batch
            var productPrices = request.PriceDetails.Select(detail => new ProductPrice
            {
                ProductTypeId = detail.ProductTypeId,
                PricingBatchId = pricingBatch.PricingBatchId,
                Price = detail.Price,
                CreatedAt = vietnamTime
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

        try
        {
            Expression<Func<PricingBatch, bool>>? predicate = null;
            
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                var fromDateTime = request.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                var toDateTime = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue);

                predicate = pb => pb.CreatedAt >= fromDateTime && pb.CreatedAt <= toDateTime;
            }
            else if (request.FromDate.HasValue)
            {
                var fromDateTime = request.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                predicate = pb => pb.CreatedAt >= fromDateTime;
            }
            else if (request.ToDate.HasValue)
            {
                var toDateTime = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                predicate = pb => pb.CreatedAt <= toDateTime;
            }

            // Sử dụng PagedAsync để lấy data với pagination
            var pagedResult = await _pricingBatchRepository.PagedAsync(
                request.Page,
                request.PageSize,
                predicate,
                false,
                CancellationToken.None,
                pb => pb.ProductPrices
            );

            // Lấy product types cho các ProductPrice
            var productTypeIds = pagedResult.Items
                .SelectMany(pb => pb.ProductPrices)
                .Select(pp => pp.ProductTypeId)
                .Distinct()
                .ToList();

            var productTypes = await _productTypeRepository.ToListAsync(
                pt => productTypeIds.Contains(pt.ProductTypeId)
            );

            // Map to response DTOs
            var pricingBatchEntities = pagedResult.Items.Select(pb => new PricingBatchSelectsEntity
            {
                PricingBatchId = pb.PricingBatchId,
                Title = pb.Title,
                Description = pb.Description,
                CreatedAt = pb.CreatedAt,
                PriceDetails = pb.ProductPrices.Select(pp => {
                    var productType = productTypes.FirstOrDefault(pt => pt.ProductTypeId == pp.ProductTypeId);
                    return new ProductPriceEntity
                    {
                        PriceId = pp.PriceId,
                        ProductTypeId = pp.ProductTypeId,
                        TypeName = productType?.TypeName!,
                        Price = pp.Price
                    };
                }).ToList()
            })
            .OrderBy(x => x.CreatedAt)
            .ToList();

            var paginationResponse = new PaginationResponse<PricingBatchSelectsEntity>(
                pricingBatchEntities, pagedResult.TotalCount, request.Page, request.PageSize);

            response.Success = true;
            response.Response = paginationResponse;
            response.SetMessage(MessageId.I00001, "Pricing batches retrieved successfully.");
        }
        catch (Exception ex)
        {
            response.SetMessage(MessageId.E00000, $"Error retrieving pricing batches: {ex.Message}");
        }

        return response;
    }
}
