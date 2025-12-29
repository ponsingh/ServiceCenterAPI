using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Base Controller
/// Provides standardized API response methods for all controllers
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    // ============ Success Responses (200 OK) ============

    /// <summary>
    /// Return a successful response with data
    /// HTTP Status: 200 OK
    /// </summary>
    /// <param name="data">Response data</param>
    /// <param name="message">Success message</param>
    /// <returns>Standard success response</returns>
    protected IActionResult SuccessResponse<T>(T data, string message = "Success")
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return a successful response without data
    /// HTTP Status: 200 OK
    /// </summary>
    /// <param name="message">Success message</param>
    /// <returns>Standard success response</returns>
    protected IActionResult SuccessResponse(string message = "Success")
    {
        return Ok(new ApiResponse
        {
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ Created Responses (201 Created) ============

    /// <summary>
    /// Return a created response with data
    /// HTTP Status: 201 Created
    /// </summary>
    /// <param name="data">Created resource data</param>
    /// <param name="message">Creation message</param>
    /// <returns>Standard created response</returns>
    protected IActionResult CreatedResponse<T>(T data, string message = "Resource created successfully")
    {
        return StatusCode(StatusCodes.Status201Created, new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return a created response with route
    /// HTTP Status: 201 Created
    /// </summary>
    /// <param name="routeName">Route name for location header</param>
    /// <param name="routeValues">Route values</param>
    /// <param name="data">Created resource data</param>
    /// <param name="message">Creation message</param>
    /// <returns>Created response with location header</returns>
    protected IActionResult CreatedResponse<T>(
        string routeName,
        object routeValues,
        T data,
        string message = "Resource created successfully")
    {
        return CreatedAtRoute(routeName, routeValues, new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ Bad Request Responses (400 Bad Request) ============

    /// <summary>
    /// Return a bad request response with message
    /// HTTP Status: 400 Bad Request
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Bad request response</returns>
    protected IActionResult BadRequestResponse(string message)
    {
        return BadRequest(new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return a bad request response with message and errors
    /// HTTP Status: 400 Bad Request
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errors">List of errors</param>
    /// <returns>Bad request response with error details</returns>
    protected IActionResult BadRequestResponse(string message, IEnumerable<string> errors)
    {
        return BadRequest(new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors.ToList(),
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return a bad request response with ModelState errors
    /// HTTP Status: 400 Bad Request
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="modelState">Model state from validation</param>
    /// <returns>Bad request response with validation errors</returns>
    protected IActionResult BadRequestResponse(string message, ModelStateDictionary modelState)
    {
        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return BadRequest(new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ Not Found Responses (404 Not Found) ============

    /// <summary>
    /// Return a not found response
    /// HTTP Status: 404 Not Found
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Not found response</returns>
    protected IActionResult NotFoundResponse(string message = "Resource not found")
    {
        return NotFound(new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return a not found response with error details
    /// HTTP Status: 404 Not Found
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errors">List of errors</param>
    /// <returns>Not found response with details</returns>
    protected IActionResult NotFoundResponse(string message, IEnumerable<string> errors)
    {
        return NotFound(new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors.ToList(),
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ Conflict Responses (409 Conflict) ============

    /// <summary>
    /// Return a conflict response (e.g., resource already exists)
    /// HTTP Status: 409 Conflict
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Conflict response</returns>
    protected IActionResult ConflictResponse(string message = "Resource conflict")
    {
        return StatusCode(StatusCodes.Status409Conflict, new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return a conflict response with error details
    /// HTTP Status: 409 Conflict
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errors">List of errors</param>
    /// <returns>Conflict response with details</returns>
    protected IActionResult ConflictResponse(string message, IEnumerable<string> errors)
    {
        return StatusCode(StatusCodes.Status409Conflict, new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors.ToList(),
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ Unauthorized Responses (401 Unauthorized) ============

    /// <summary>
    /// Return an unauthorized response
    /// HTTP Status: 401 Unauthorized
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Unauthorized response</returns>
    protected IActionResult UnauthorizedResponse(string message = "Unauthorized access")
    {
        return Unauthorized(new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ Forbidden Responses (403 Forbidden) ============

    /// <summary>
    /// Return a forbidden response (access denied)
    /// HTTP Status: 403 Forbidden
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Forbidden response</returns>
    protected IActionResult ForbiddenResponse(string message = "Access forbidden")
    {
        return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ Internal Server Error Responses (500) ============

    /// <summary>
    /// Return an internal server error response
    /// HTTP Status: 500 Internal Server Error
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Internal server error response</returns>
    protected IActionResult InternalServerErrorResponse(string message = "An internal server error occurred")
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return an internal server error response with exception details
    /// HTTP Status: 500 Internal Server Error
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="exception">Exception object</param>
    /// <returns>Internal server error response</returns>
    protected IActionResult InternalServerErrorResponse(string message, Exception exception)
    {
        var response = new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        // Only include exception details in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            response.Errors = new List<string>
            {
                exception.Message,
                exception.StackTrace ?? "No stack trace available"
            };
        }

        return StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    // ============ Unprocessable Entity Responses (422) ============

    /// <summary>
    /// Return an unprocessable entity response
    /// HTTP Status: 422 Unprocessable Entity
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Unprocessable entity response</returns>
    protected IActionResult UnprocessableEntityResponse(string message = "Request body is invalid or unprocessable")
    {
        return StatusCode(StatusCodes.Status422UnprocessableEntity, new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Return an unprocessable entity response with errors
    /// HTTP Status: 422 Unprocessable Entity
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="errors">List of errors</param>
    /// <returns>Unprocessable entity response with details</returns>
    protected IActionResult UnprocessableEntityResponse(string message, IEnumerable<string> errors)
    {
        return StatusCode(StatusCodes.Status422UnprocessableEntity, new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors.ToList(),
            Timestamp = DateTime.UtcNow
        });
    }

    // ============ No Content Responses (204) ============

    /// <summary>
    /// Return a no content response (successful delete/update with no response body)
    /// HTTP Status: 204 No Content
    /// </summary>
    /// <returns>No content response</returns>
    protected IActionResult NoContentResponse()
    {
        return NoContent();
    }

    // ============ Pagination Helper ============

    /// <summary>
    /// Get pagination info from query parameters
    /// </summary>
    /// <param name="page">Page number (1-based), default 1</param>
    /// <param name="pageSize">Items per page, default 10, max 100</param>
    /// <returns>Tuple of (page, pageSize)</returns>
    protected (int page, int pageSize) GetPaginationInfo(int page = 1, int pageSize = 10)
    {
        // Validate page
        if (page < 1)
            page = 1;

        // Validate and cap page size
        if (pageSize < 1)
            pageSize = 10;
        if (pageSize > 100)
            pageSize = 100;

        return (page, pageSize);
    }

    /// <summary>
    /// Get skip count for LINQ queries
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Number of items to skip</returns>
    protected int GetSkipCount(int page, int pageSize)
    {
        return (page - 1) * pageSize;
    }
}

// ============ API Response Models ============

/// <summary>
/// Standard API Response wrapper (with data)
/// </summary>
/// <typeparam name="T">Type of response data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error details (if any)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Standard API Response wrapper (without data)
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Error details (if any)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Paginated API Response
/// </summary>
/// <typeparam name="T">Type of data items</typeparam>
public class PaginatedApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Response data items
    /// </summary>
    public IEnumerable<T> Data { get; set; } = new List<T>();

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Has next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Has previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Error details (if any)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; }
}