using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCenterAPI.Application.Services;
using ServiceCenterAPI.Services;
using System;
using System.Threading.Tasks;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Smart Update Controller (Simplified)
/// Reuses CreateServiceOrderCompleteDto with ServiceOrderId added
/// Automatically detects changes by comparing client data with DB
/// </summary>
[ApiController]
[Route("api/composite")]
public class SmartUpdateControllerSimplified : ControllerBase
{
    private readonly ISmartUpdateCompositeService _smartUpdateService;

    public SmartUpdateControllerSimplified(ISmartUpdateCompositeService smartUpdateService)
    {
        _smartUpdateService = smartUpdateService;
    }

    /// <summary>
    /// Smart Update ServiceOrder
    /// 
    /// Reuses CreateServiceOrderCompleteDto (with ServiceOrderId property added)
    /// Client sends ONLY items they want to keep (new + existing)
    /// API automatically detects and deletes items not in the list
    /// 
    /// Example:
    /// DB has: Item 1, 2, 3
    /// Client sends: Item 1, 3
    /// API automatically deletes: Item 2
    /// </summary>
    /// <remarks>
    /// Example request:
    /// {
    ///   "serviceOrderId": 1,           // ← REQUIRED for update
    ///   "notes": "Updated notes",
    ///   "expectedPickupDate": "2025-02-01",
    ///   "statusId": 2,
    ///   "items": [
    ///     {
    ///       "itemId": 1,               // ← Existing item (update)
    ///       "brand": "Apple (Updated)",
    ///       "model": "iPhone 13 Pro",
    ///       "jobs": [
    ///         {
    ///           "jobId": 1,            // ← Existing job (update)
    ///           "priority": "Critical",
    ///           "diagnosis": "Updated diagnosis",
    ///           "jobParts": [
    ///             {
    ///               "jobPartId": 1,    // ← Existing part (update)
    ///               "quantity": 2,
    ///               "unitCost": 350
    ///             }
    ///             // Part 2 NOT sent → Auto-deleted!
    ///           ]
    ///         },
    ///         {
    ///           // NO jobId → New job
    ///           "priority": "Normal",
    ///           "diagnosis": "New job",
    ///           "jobParts": [...]
    ///         }
    ///       ]
    ///     },
    ///     {
    ///       // NO itemId → New item
    ///       "brand": "Samsung",
    ///       "model": "Galaxy S21",
    ///       "jobs": [...]
    ///     }
    ///     // Item 2 NOT sent → Auto-deleted!
    ///   ]
    /// }
    /// </remarks>
    [HttpPut("serviceorder/smart-update")]
    [ProducesResponseType(typeof(ServiceOrderDetailedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SmartUpdateServiceOrder([FromBody] CreateServiceOrderCompleteDto dto)
    {
        if (dto == null)
            return BadRequest(new { success = false, message = "Request body cannot be null" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        // Check if ServiceOrderId property exists and is valid
        var soIdProp = dto.GetType().GetProperty("ServiceOrderId");
        int serviceOrderId = 0;
        if (soIdProp != null)
        {
            if (int.TryParse(soIdProp.GetValue(dto)?.ToString() ?? "0", out var id))
            {
                serviceOrderId = id;
            }
        }

        if (serviceOrderId <= 0)
            return BadRequest(new { success = false, message = "ServiceOrderId is required and must be > 0" });

        try
        {
            var result = await _smartUpdateService.SmartUpdateServiceOrderAsync(dto);

            return Ok(new
            {
                success = true,
                data = result,
                message = $"Service order updated successfully. Items: {result.Items.Count}, Total Jobs: {result.Items.Sum(i => i.Jobs.Count)}"
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