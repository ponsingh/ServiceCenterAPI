using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceCenterAPI.Application.Services;
using ServiceCenterAPI.Controllers;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Customers Controller
/// Manages all customer-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // ============ GET ENDPOINTS ============

    /// <summary>
    /// Get all customers
    /// </summary>
    /// <returns>List of all customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return SuccessResponse<IEnumerable<CustomerDto>>(customers, "Customers retrieved successfully");
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        if (id <= 0)
            return BadRequestResponse("Invalid customer ID");

        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFoundResponse($"Customer with ID {id} not found");

        return SuccessResponse<CustomerDto>(customer, "Customer retrieved successfully");
    }

    /// <summary>
    /// Get active customers only
    /// </summary>
    /// <returns>List of active customers</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveCustomers()
    {
        var customers = await _customerService.GetActiveCustomersAsync();
        return SuccessResponse<IEnumerable<CustomerDto>>(customers, "Active customers retrieved successfully");
    }

    /// <summary>
    /// Get customers by type
    /// </summary>
    /// <param name="type">Customer type</param>
    /// <returns>List of customers by type</returns>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomersByType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return BadRequestResponse("Customer type cannot be empty");

        var customers = await _customerService.GetCustomersByTypeAsync(type);
        return SuccessResponse<IEnumerable<CustomerDto>>(customers, "Customers retrieved successfully");
    }

    /// <summary>
    /// Search customers by name
    /// </summary>
    /// <param name="name">Customer name to search for</param>
    /// <returns>List of matching customers</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCustomers([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequestResponse("Search name cannot be empty");

        var customers = await _customerService.SearchCustomersByNameAsync(name);
        return SuccessResponse<IEnumerable<CustomerDto>>(customers, "Search results retrieved successfully");
    }

    /// <summary>
    /// Get customer by contact number
    /// </summary>
    /// <param name="contactNumber">Contact number</param>
    /// <returns>Customer details</returns>
    [HttpGet("contact/{contactNumber}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByContact(string contactNumber)
    {
        if (string.IsNullOrWhiteSpace(contactNumber))
            return BadRequestResponse("Contact number cannot be empty");

        var customer = await _customerService.GetCustomerByContactNumberAsync(contactNumber);
        if (customer == null)
            return NotFoundResponse($"Customer with contact number {contactNumber} not found");

        return SuccessResponse<CustomerDto>(customer, "Customer retrieved successfully");
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Customer details</returns>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequestResponse("Email cannot be empty");

        var customer = await _customerService.GetCustomerByEmailAsync(email);
        if (customer == null)
            return NotFoundResponse($"Customer with email {email} not found");

        return SuccessResponse<CustomerDto>(customer, "Customer retrieved successfully");
    }

    /// <summary>
    /// Get customer statistics
    /// </summary>
    /// <returns>Customer statistics</returns>
    [HttpGet("stats/summary")]
    [ProducesResponseType(typeof(CustomerStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerStats()
    {
        var stats = await _customerService.GetCustomerStatsAsync();
        return SuccessResponse<CustomerStatsDto>(stats, "Customer statistics retrieved successfully");
    }

    /// <summary>
    /// Get total customer count
    /// </summary>
    /// <returns>Total number of customers</returns>
    [HttpGet("stats/total-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalCount()
    {
        var count = await _customerService.GetTotalCustomerCountAsync();
        return SuccessResponse<int>(count, "Total count retrieved successfully");
    }

    /// <summary>
    /// Get all distinct customer types
    /// </summary>
    /// <returns>List of all customer types</returns>
    [HttpGet("meta/types")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCustomerTypes()
    {
        var types = await _customerService.GetAllCustomerTypesAsync();
        return SuccessResponse<IEnumerable<string>>(types, "Customer types retrieved successfully");
    }

    // ============ POST/PUT/DELETE ENDPOINTS ============

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="dto">Customer creation data</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequestResponse("Invalid customer data", ModelState);

        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            return BadRequestResponse("Customer name is required");

        if (string.IsNullOrWhiteSpace(dto.ContactNumber))
            return BadRequestResponse("Contact number is required");

        if (string.IsNullOrWhiteSpace(dto.CustomerType))
            return BadRequestResponse("Customer type is required");

        // Check if email already exists (if provided)
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            bool emailExists = await _customerService.EmailExistsAsync(dto.Email);
            if (emailExists)
                return ConflictResponse("Email already in use");
        }

        // Check if contact number already exists
        bool contactExists = await _customerService.ContactNumberExistsAsync(dto.ContactNumber);
        if (contactExists)
            return ConflictResponse("Contact number already in use");

        try
        {
            // TODO: Get userId from authenticated user (not hardcoded)
            int userId = 1; // Replace with actual authenticated user

            var customer = await _customerService.CreateCustomerAsync(dto, userId);
            return CreatedResponse<CustomerDto>(customer, "Customer created successfully");
        }
        catch (ArgumentException ex)
        {
            return BadRequestResponse(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ConflictResponse(ex.Message);
        }
    }

    /// <summary>
    /// Update a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="dto">Customer update data</param>
    /// <returns>Updated customer</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (id <= 0)
            return BadRequestResponse("Invalid customer ID");

        if (!ModelState.IsValid)
            return BadRequestResponse("Invalid update data", ModelState);

        var customer = await _customerService.UpdateCustomerAsync(id, dto, userId: 1);
        if (customer == null)
            return NotFoundResponse($"Customer with ID {id} not found");

        return SuccessResponse<CustomerDto>(customer, "Customer updated successfully");
    }

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        if (id <= 0)
            return BadRequestResponse("Invalid customer ID");

        var success = await _customerService.DeleteCustomerAsync(id);
        if (!success)
            return NotFoundResponse($"Customer with ID {id} not found");

        return SuccessResponse("Customer deleted successfully");
    }
}