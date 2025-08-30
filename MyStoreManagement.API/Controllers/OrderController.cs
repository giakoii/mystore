using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyStoreManagement.Application.Interfaces.Orders;
using MyStoreManagement.Application.Interfaces.IdentityHepers;
using MyStoreManagement.Application.Dtos.Orders;

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

    [HttpGet]
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
}