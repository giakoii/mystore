using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyStoreManagement.Application.Interfaces.Orders;
using MyStoreManagement.Application.Interfaces.IdentityHepers;
using MyStoreManagement.Application.Dtos.Orders;
using MyStoreManagement.Application.Utils.Const;
using OpenIddict.Validation.AspNetCore;

namespace MyStoreManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IIdentityService _identityService;

    public OrderController(IOrderService orderService, IIdentityService identityService)
    {
        _orderService = orderService;
        _identityService = identityService;
    }

    [HttpPost]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _orderService.CreateOrderAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("my-orders")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMyOrders([FromQuery] OrderSelectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = _identityService.GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized("Unable to get current user information.");
        }

        var result = await _orderService.GetOrdersByUserIdAsync(currentUser.UserId, request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{orderId:int}")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetOrderDetail(int orderId)
    {
        var result = await _orderService.GetOrderDetailAsync(orderId);
        
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [HttpGet("admin/detail/{orderId:int}")]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAdminOrderDetail(int orderId)
    {
        var result = await _orderService.GetAdminOrderDetailAsync(orderId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetAllOrders([FromQuery] AdminOrderSelectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _orderService.GetAllOrdersAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("admin/user/{userId:int}")]
    [Authorize(Roles = ConstRole.Admin, AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetUserOrders(int userId, [FromQuery] OrderSelectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _orderService.GetOrdersByUserIdAsync(userId.ToString(), request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}