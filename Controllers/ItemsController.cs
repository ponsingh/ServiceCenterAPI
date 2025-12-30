using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCenterAPI.Application.Services;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Items Controller - Manages items for service orders
/// Complete CRUD operations for Items
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService)
    {
        _itemService = itemService;
    }

    // ============ GET ENDPOINTS ============

    /// <summary>
    /// Get Item with all its Jobs and JobParts
    /// </summary>
    /// <param name="itemId">Item ID</param>
    [HttpGet("{itemId}")]
    [ProducesResponseType(typeof(ItemDetailedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItem(int itemId)
    {
        if (itemId <= 0)
            return BadRequest(new { success = false, message = "Invalid item ID" });

        var item = await _itemService.GetItemDetailedAsync(itemId);
        if (item == null)
            return NotFound(new { success = false, message = $"Item with ID {itemId} not found" });

        return Ok(new { success = true, data = item, message = "Item retrieved successfully" });
    }

    /// <summary>
    /// Get all Items for a Service Order
    /// </summary>
    /// <param name="serviceOrderId">Service Order ID</param>
    [HttpGet("service-order/{serviceOrderId}")]
    [ProducesResponseType(typeof(IEnumerable<ItemDetailedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetItemsByServiceOrder(int serviceOrderId)
    {
        if (serviceOrderId <= 0)
            return BadRequest(new { success = false, message = "Invalid service order ID" });

        var items = await _itemService.GetItemsByServiceOrderAsync(serviceOrderId);
        return Ok(new { success = true, data = items, message = "Items retrieved successfully" });
    }

    // ============ CREATE ENDPOINT ============

    /// <summary>
    /// Create Item for a Service Order
    /// </summary>
    /// <param name="serviceOrderId">Service Order ID</param>
    /// <param name="dto">Item creation data</param>
    [HttpPost("service-order/{serviceOrderId}")]
    [ProducesResponseType(typeof(ItemDetailedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateItem(int serviceOrderId, [FromBody] CreateItemDto dto)
    {
        if (serviceOrderId <= 0)
            return BadRequest(new { success = false, message = "Invalid service order ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var item = await _itemService.AddItemToServiceOrderAsync(serviceOrderId, dto);
            return Created(new Uri($"api/items/{item.ItemId}", UriKind.Relative),
                new { success = true, data = item, message = "Item created successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    // ============ UPDATE ENDPOINT ============

    /// <summary>
    /// Update Item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="dto">Item update data</param>
    [HttpPut("{itemId}")]
    [ProducesResponseType(typeof(ItemDetailedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateItemDto dto)
    {
        if (itemId <= 0)
            return BadRequest(new { success = false, message = "Invalid item ID" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var item = await _itemService.UpdateItemAsync(itemId, dto);
        if (item == null)
            return NotFound(new { success = false, message = $"Item with ID {itemId} not found" });

        return Ok(new { success = true, data = item, message = "Item updated successfully" });
    }

    // ============ DELETE ENDPOINT ============

    /// <summary>
    /// Delete Item (soft delete)
    /// Also deletes all jobs and job parts for this item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    [HttpDelete("{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(int itemId)
    {
        if (itemId <= 0)
            return BadRequest(new { success = false, message = "Invalid item ID" });

        var success = await _itemService.DeleteItemAsync(itemId);
        if (!success)
            return NotFound(new { success = false, message = $"Item with ID {itemId} not found" });

        return Ok(new { success = true, message = "Item deleted successfully" });
    }
}