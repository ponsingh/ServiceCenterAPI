using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCenterAPI.Application.Services;
using ServiceCenterAPI.Services;
using System;
using System.Threading.Tasks;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Composite Controller - Create complete hierarchies in single transaction
/// Accepts nested objects and saves everything at once
/// </summary>
[ApiController]
[Route("api/composite")]
public class CompositeController : ControllerBase
{
    private readonly ICompositeService _compositeService;

    public CompositeController(ICompositeService compositeService)
    {
        _compositeService = compositeService;
    }

    // ============ SERVICE ORDER COMPLETE ENDPOINT ============

    /// <summary>
    /// Create complete ServiceOrder with Items, Jobs, and JobParts in single transaction
    /// 
    /// This is the MAIN endpoint for client to send complete hierarchy at once
    /// </summary>
    /// <remarks>
    /// Example request body:
    /// {
    ///   "customerId": 1,
    ///   "serviceOrderNumber": "SO-001",
    ///   "notes": "Customer wants quick repair",
    ///   "items": [
    ///     {
    ///       "brand": "Apple",
    ///       "model": "iPhone 13",
    ///       "serialNo": "ABC123",
    ///       "jobs": [
    ///         {
    ///           "priority": "High",
    ///           "diagnosis": "Screen damaged",
    ///           "jobParts": [
    ///             {
    ///               "partId": 5,
    ///               "quantity": 1,
    ///               "unitCost": 300
    ///             }
    ///           ]
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    /// </remarks>
    [HttpPost("serviceorder/complete")]
    [ProducesResponseType(typeof(ServiceOrderDetailedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateServiceOrderComplete([FromBody] CreateServiceOrderCompleteDto dto)
    {
        if (dto == null)
            return BadRequest(new { success = false, message = "Request body cannot be null" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        if (dto.CustomerId <= 0)
            return BadRequest(new { success = false, message = "Invalid customer ID" });

        try
        {
            var result = await _compositeService.CreateServiceOrderCompleteAsync(dto);

            return Created(new Uri($"api/serviceorders/{result.ServiceOrderId}", UriKind.Relative),
                new
                {
                    success = true,
                    data = result,
                    message = $"Service order created successfully with {result.Items.Count} items"
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    // ============ ITEM COMPLETE ENDPOINT ============

    /// <summary>
    /// Create complete Item with Jobs and JobParts in single transaction
    /// </summary>
    /// <remarks>
    /// Example request body:
    /// {
    ///   "brand": "Apple",
    ///   "model": "iPhone 13",
    ///   "serialNo": "ABC123",
    ///   "jobs": [
    ///     {
    ///       "priority": "High",
    ///       "diagnosis": "Screen damaged",
    ///       "jobParts": [
    ///         {
    ///           "partId": 5,
    ///           "quantity": 1,
    ///           "unitCost": 300
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    /// </remarks>
    [HttpPost("serviceorder/{serviceOrderId}/item/complete")]
    [ProducesResponseType(typeof(ItemDetailedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateItemComplete(int serviceOrderId, [FromBody] CreateItemCompositeDto dto)
    {
        if (serviceOrderId <= 0)
            return BadRequest(new { success = false, message = "Invalid service order ID" });

        if (dto == null)
            return BadRequest(new { success = false, message = "Request body cannot be null" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var result = await _compositeService.CreateItemCompleteAsync(serviceOrderId, dto);

            return Created(new Uri($"api/items/{result.ItemId}", UriKind.Relative),
                new
                {
                    success = true,
                    data = result,
                    message = $"Item created successfully with {result.Jobs.Count} jobs"
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    // ============ JOB COMPLETE ENDPOINT ============

    /// <summary>
    /// Create complete Job with JobParts in single transaction
    /// </summary>
    /// <remarks>
    /// Example request body:
    /// {
    ///   "priority": "High",
    ///   "diagnosis": "Screen damaged",
    ///   "jobParts": [
    ///     {
    ///       "partId": 5,
    ///       "quantity": 1,
    ///       "unitCost": 300
    ///     },
    ///     {
    ///       "partId": 10,
    ///       "quantity": 2,
    ///       "unitCost": 50
    ///     }
    ///   ]
    /// }
    /// </remarks>
    [HttpPost("item/{itemId}/job/complete")]
    [ProducesResponseType(typeof(JobDetailedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateJobComplete(int itemId, [FromBody] CreateJobCompositeDto dto)
    {
        if (itemId <= 0)
            return BadRequest(new { success = false, message = "Invalid item ID" });

        if (dto == null)
            return BadRequest(new { success = false, message = "Request body cannot be null" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var result = await _compositeService.CreateJobCompleteAsync(itemId, dto);

            return Created(new Uri($"api/jobs/{result.JobId}", UriKind.Relative),
                new
                {
                    success = true,
                    data = result,
                    message = $"Job created successfully with {result.JobParts.Count} parts. Total cost: ${result.ActualCost}"
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }
}