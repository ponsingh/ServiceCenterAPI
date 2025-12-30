using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCenterAPI.Application.Services;
using ServiceCenterAPI.Controllers;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Service Orders Controller - Manages service orders and child entities
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ServiceOrdersController : BaseController
{
    private readonly IServiceOrderService _serviceOrderService;

    public ServiceOrdersController(IServiceOrderService serviceOrderService)
    {
        _serviceOrderService = serviceOrderService;
    }

    // ============ GET ENDPOINTS ============

    /// <summary>
    /// Get Service Order with all child entities (Items, Jobs, JobParts, etc.)
    /// </summary>
    /// <param name="id">Service Order ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceOrderDetailedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceOrder(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid service order ID" });

        var serviceOrder = await _serviceOrderService.GetServiceOrderDetailedAsync(id);
        if (serviceOrder == null)
            return NotFound(new { message = $"Service order with ID {id} not found" });

        return Ok(new { success = true, data = serviceOrder, message = "Service order retrieved successfully" });
    }

    /// <summary>
    /// Get all Service Orders
    /// </summary>
    [HttpGet("list/all")]
    [ProducesResponseType(typeof(IEnumerable<ServiceOrderDetailedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllServiceOrders()
    {
        var serviceOrders = await _serviceOrderService.GetAllServiceOrdersAsync();
        return Ok(new { success = true, data = serviceOrders, message = "Service orders retrieved successfully" });
    }

    /// <summary>
    /// Search Service Orders by Customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    [HttpGet("search/customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<ServiceOrderDetailedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchByCustomer(int customerId)
    {
        if (customerId <= 0)
            return BadRequest(new { message = "Invalid customer ID" });

        var serviceOrders = await _serviceOrderService.SearchByCustomerAsync(customerId);
        return Ok(new { success = true, data = serviceOrders, message = "Service orders retrieved successfully" });
    }

    /// <summary>
    /// Search Service Orders by Service Order Number
    /// </summary>
    /// <param name="serviceOrderNumber">Service Order Number</param>
    [HttpGet("search/number")]
    [ProducesResponseType(typeof(IEnumerable<ServiceOrderDetailedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchByServiceOrderNumber([FromQuery] string serviceOrderNumber)
    {
        if (string.IsNullOrWhiteSpace(serviceOrderNumber))
            return BadRequest(new { message = "Service order number cannot be empty" });

        var serviceOrders = await _serviceOrderService.SearchByServiceOrderNumberAsync(serviceOrderNumber);
        return Ok(new { success = true, data = serviceOrders, message = "Service orders retrieved successfully" });
    }

    // ============ CREATE ENDPOINT ============

    /// <summary>
    /// Create a new Service Order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceOrderDetailedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateServiceOrder([FromBody] CreateServiceOrderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var serviceOrder = await _serviceOrderService.CreateServiceOrderAsync(dto);
            return Created(new Uri($"api/serviceorders/{serviceOrder.ServiceOrderId}", UriKind.Relative),
                new { success = true, data = serviceOrder, message = "Service order created successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ============ UPDATE ENDPOINT ============

    /// <summary>
    /// Update Service Order
    /// </summary>
    /// <param name="id">Service Order ID</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ServiceOrderDetailedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServiceOrder(int id, [FromBody] UpdateServiceOrderDto dto)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid service order ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var serviceOrder = await _serviceOrderService.UpdateServiceOrderAsync(id, dto);
        if (serviceOrder == null)
            return NotFound(new { message = $"Service order with ID {id} not found" });

        return Ok(new { success = true, data = serviceOrder, message = "Service order updated successfully" });
    }

    // ============ DELETE ENDPOINT ============

    /// <summary>
    /// Delete Service Order (soft delete)
    /// </summary>
    /// <param name="id">Service Order ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServiceOrder(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid service order ID" });

        var success = await _serviceOrderService.DeleteServiceOrderAsync(id);
        if (!success)
            return NotFound(new { message = $"Service order with ID {id} not found" });

        return Ok(new { success = true, message = "Service order deleted successfully" });
    }

    // ============ ITEM CHILD ENDPOINTS ============

    /// <summary>
    /// Get all Items for a Service Order
    /// </summary>
    /// <param name="serviceOrderId">Service Order ID</param>
    [HttpGet("{serviceOrderId}/items")]
    [ProducesResponseType(typeof(IEnumerable<ItemDetailedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetItemsByServiceOrder(int serviceOrderId)
    {
        if (serviceOrderId <= 0)
            return BadRequest(new { message = "Invalid service order ID" });

        var serviceOrder = await _serviceOrderService.GetServiceOrderDetailedAsync(serviceOrderId);
        if (serviceOrder == null)
            return NotFound(new { message = $"Service order with ID {serviceOrderId} not found" });

        return Ok(new { success = true, data = serviceOrder.Items, message = "Items retrieved successfully" });
    }

    /// <summary>
    /// Add Item to Service Order
    /// </summary>
    /// <param name="serviceOrderId">Service Order ID</param>
    [HttpPost("{serviceOrderId}/items")]
    [ProducesResponseType(typeof(ItemDetailedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItemToServiceOrder(int serviceOrderId, [FromBody] CreateItemDto dto)
    {
        if (serviceOrderId <= 0)
            return BadRequest(new { message = "Invalid service order ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        // Verify service order exists
        var exists = await _serviceOrderService.ServiceOrderExistsAsync(serviceOrderId);
        if (!exists)
            return NotFound(new { message = $"Service order with ID {serviceOrderId} not found" });

        // TODO: Implement AddItemToServiceOrderAsync in service
        // For now, return not implemented
        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Add item endpoint to be implemented" });
    }
}