using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceCenterAPI.Application.Services;

namespace ServiceCenterAPI.Controllers;

/// <summary>
/// Employees Controller
/// Provides REST API endpoints for employee management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : BaseController
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Employee details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        if (id <= 0)
            return BadRequestResponse("Invalid employee ID");

        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFoundResponse($"Employee with ID {id} not found");

        return SuccessResponse(employee, "Employee retrieved successfully");
    }

    /// <summary>
    /// Get all employees
    /// </summary>
    /// <returns>List of all employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEmployees()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return SuccessResponse(employees, "Employees retrieved successfully");
    }

    /// <summary>
    /// Get active employees
    /// </summary>
    /// <returns>List of active employees</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveEmployees()
    {
        var employees = await _employeeService.GetActiveEmployeesAsync();
        return SuccessResponse(employees, "Active employees retrieved successfully");
    }

    /// <summary>
    /// Get employees by role
    /// </summary>
    /// <param name="role">Employee role</param>
    /// <returns>List of employees with specified role</returns>
    [HttpGet("role/{role}")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeesByRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequestResponse("Role cannot be empty");

        var employees = await _employeeService.GetEmployeesByRoleAsync(role);
        return SuccessResponse(employees, "Employees retrieved successfully");
    }

    /// <summary>
    /// Search employees by name
    /// </summary>
    /// <param name="name">Employee name to search for</param>
    /// <returns>List of matching employees</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchEmployeesByName([FromQuery][Required] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequestResponse("Search term cannot be empty");

        var employees = await _employeeService.SearchEmployeesByNameAsync(name);
        return SuccessResponse(employees, "Search results retrieved successfully");
    }

    /// <summary>
    /// Get employee by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Employee details</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeByUserId(int userId)
    {
        if (userId <= 0)
            return BadRequestResponse("Invalid user ID");

        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
        if (employee == null)
            return NotFoundResponse($"Employee with user ID {userId} not found");

        return SuccessResponse(employee, "Employee retrieved successfully");
    }

    /// <summary>
    /// Get employee statistics
    /// </summary>
    /// <returns>Employee statistics</returns>
    [HttpGet("stats/summary")]
    [ProducesResponseType(typeof(EmployeeStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeStats()
    {
        var stats = await _employeeService.GetEmployeeStatsAsync();
        return SuccessResponse(stats, "Employee statistics retrieved successfully");
    }

    /// <summary>
    /// Get total employee count
    /// </summary>
    /// <returns>Total count of active employees</returns>
    [HttpGet("stats/total-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalEmployeeCount()
    {
        var count = await _employeeService.GetTotalEmployeeCountAsync();
        return SuccessResponse(new { totalEmployees = count }, "Employee count retrieved successfully");
    }

    /// <summary>
    /// Get all distinct roles
    /// </summary>
    /// <returns>List of all roles</returns>
    [HttpGet("meta/roles")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _employeeService.GetAllRolesAsync();
        return SuccessResponse(roles, "Roles retrieved successfully");
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    /// <param name="dto">Employee data</param>
    /// <returns>Created employee</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequestResponse("Invalid employee data", ModelState);

        if (string.IsNullOrWhiteSpace(dto.EmployeeName))
            return BadRequestResponse("Employee name is required");

        if (string.IsNullOrWhiteSpace(dto.Role))
            return BadRequestResponse("Role is required");

        // Check if email already exists (if provided)
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            bool emailExists = await _employeeService.EmailExistsAsync(dto.Email);
            if (emailExists)
                return ConflictResponse("Email already in use");
        }

        try
        {
            // TODO: Get userId from authenticated user (not hardcoded)
            int userId = 1; // Replace with actual authenticated user

            var employee = await _employeeService.CreateEmployeeAsync(dto, userId);
            return CreatedResponse(employee, "Employee created successfully");
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
    /// Update an employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="dto">Updated employee data</param>
    /// <returns>Updated employee</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (id <= 0)
            return BadRequestResponse("Invalid employee ID");

        if (!ModelState.IsValid)
            return BadRequestResponse("Invalid employee data", ModelState);

        // Check if email is being changed and if it's unique
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var existingEmployee = await _employeeService.GetEmployeeByIdAsync(id);
            if (existingEmployee != null && existingEmployee.Email != dto.Email)
            {
                bool emailExists = await _employeeService.EmailExistsAsync(dto.Email, id);
                if (emailExists)
                    return ConflictResponse("Email already in use");
            }
        }

        try
        {
            // TODO: Get userId from authenticated user (not hardcoded)
            int userId = 1; // Replace with actual authenticated user

            var employee = await _employeeService.UpdateEmployeeAsync(id, dto, userId);
            if (employee == null)
                return NotFoundResponse($"Employee with ID {id} not found");

            return SuccessResponse(employee, "Employee updated successfully");
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
    /// Delete an employee (soft delete)
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        if (id <= 0)
            return BadRequestResponse("Invalid employee ID");

        var success = await _employeeService.DeleteEmployeeAsync(id);
        if (!success)
            return NotFoundResponse($"Employee with ID {id} not found");

        return SuccessResponse<EmployeeStatsDto>(null, "message");
        //return SuccessResponse(null, "Employee deleted successfully");
    }
}