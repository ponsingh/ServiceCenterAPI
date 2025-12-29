using Microsoft.EntityFrameworkCore;
using ServiceCenterAPI.Infrastructure.Persistence;
using ServiceCenterAPI.Infrastructure.Persistence.Models;
using ServiceCenterAPI.Infrastructure.Repositories;
using ServiceCenterAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenterAPI.Application.Services;

// ============ DTOs ============

/// <summary>
/// Data Transfer Object for Employee (Read)
/// </summary>
public class EmployeeDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? UpdatedByUserId { get; set; }
}

/// <summary>
/// Data Transfer Object for Creating Employee
/// </summary>
public class CreateEmployeeDto
{
    public string EmployeeName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = null!;
    public int? UserId { get; set; }
}

/// <summary>
/// Data Transfer Object for Updating Employee
/// </summary>
public class UpdateEmployeeDto
{
    public string? EmployeeName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int? UserId { get; set; }
}

/// <summary>
/// Data Transfer Object for Employee Statistics
/// </summary>
public class EmployeeStatsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public Dictionary<string, int> EmployeesByRole { get; set; } = new();
}

// ============ Service Interface ============

/// <summary>
/// Employee Service Interface
/// Defines all operations for employee management
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Get employee by ID
    /// </summary>
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);

    /// <summary>
    /// Get all employees
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();

    /// <summary>
    /// Get only active employees
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetActiveEmployeesAsync();

    /// <summary>
    /// Get employees filtered by role
    /// </summary>
    Task<IEnumerable<EmployeeDto>> GetEmployeesByRoleAsync(string role);

    /// <summary>
    /// Search employees by first or last name
    /// </summary>
    Task<IEnumerable<EmployeeDto>> SearchEmployeesByNameAsync(string name);

    /// <summary>
    /// Get employees by user ID
    /// </summary>
    Task<EmployeeDto?> GetEmployeeByUserIdAsync(int userId);

    /// <summary>
    /// Create a new employee
    /// </summary>
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, int userId);

    /// <summary>
    /// Update an existing employee
    /// </summary>
    Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, int userId);

    /// <summary>
    /// Delete an employee (soft delete)
    /// </summary>
    Task<bool> DeleteEmployeeAsync(int id);

    /// <summary>
    /// Check if employee exists
    /// </summary>
    Task<bool> EmployeeExistsAsync(int id);

    /// <summary>
    /// Check if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Check if email already exists (excluding specific employee)
    /// </summary>
    Task<bool> EmailExistsAsync(string email, int excludeEmployeeId);

    /// <summary>
    /// Get employee statistics
    /// </summary>
    Task<EmployeeStatsDto> GetEmployeeStatsAsync();

    /// <summary>
    /// Get total count of active employees
    /// </summary>
    Task<int> GetTotalEmployeeCountAsync();

    /// <summary>
    /// Get all distinct roles
    /// </summary>
    Task<IEnumerable<string>> GetAllRolesAsync();
}

// ============ Service Implementation ============

/// <summary>
/// Employee Service Implementation
/// Handles all business logic for employee management
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null || employee.IsDeleted)
            return null;

        return MapToDto(employee);
    }

    /// <summary>
    /// Get all employees
    /// </summary>
    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => !e.IsDeleted);
        return employees.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get only active employees
    /// </summary>
    public async Task<IEnumerable<EmployeeDto>> GetActiveEmployeesAsync()
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.IsActive && !e.IsDeleted);
        return employees.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get employees filtered by role
    /// </summary>
    public async Task<IEnumerable<EmployeeDto>> GetEmployeesByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return new List<EmployeeDto>();

        var employees = await _unitOfWork.Employees.FindAsync(e =>
            e.Role == role && !e.IsDeleted);
        return employees.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Search employees by first or last name
    /// </summary>
    public async Task<IEnumerable<EmployeeDto>> SearchEmployeesByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<EmployeeDto>();

        var searchTerm = name.ToLower();
        var employees = await _unitOfWork.Employees.FindAsync(e =>
            (e.EmployeeName.ToLower().Contains(searchTerm)) && !e.IsDeleted);
        return employees.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get employee by user ID
    /// </summary>
    public async Task<EmployeeDto?> GetEmployeeByUserIdAsync(int userId)
    {
        var employee = await _unitOfWork.Employees.FirstOrDefaultAsync(e =>
            e.UserId == userId && !e.IsDeleted);

        return employee == null ? null : MapToDto(employee);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, int userId)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.EmployeeName))
            throw new ArgumentException("Employee name is required");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");

        if (string.IsNullOrWhiteSpace(dto.Phone))
            throw new ArgumentException("Phone is required");

        if (string.IsNullOrWhiteSpace(dto.Role))
            throw new ArgumentException("Role is required");

        // Check if email already exists
        bool emailExists = await EmailExistsAsync(dto.Email);
        if (emailExists)
            throw new InvalidOperationException($"Email '{dto.Email}' already exists");

        var employee = new Employee
        {
            EmployeeName = dto.EmployeeName,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role,
            UserId = dto.UserId,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        await _unitOfWork.Employees.AddAsync(employee);
        return MapToDto(employee);
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    public async Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto, int userId)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null || employee.IsDeleted)
            return null;

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(dto.EmployeeName))
            employee.EmployeeName = dto.EmployeeName;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            // Check if new email doesn't conflict with other employees
            bool emailExists = await EmailExistsAsync(dto.Email, id);
            if (emailExists)
                throw new InvalidOperationException($"Email '{dto.Email}' already exists");
            employee.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            employee.Phone = dto.Phone;

        if (!string.IsNullOrWhiteSpace(dto.Role))
            employee.Role = dto.Role;

        if (dto.IsActive.HasValue)
            employee.IsActive = dto.IsActive.Value;

        if (dto.UserId.HasValue)
            employee.UserId = dto.UserId.Value;

        // Set update tracking
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedByUserId = userId;

        await _unitOfWork.Employees.UpdateAsync(employee);
        return MapToDto(employee);
    }

    /// <summary>
    /// Delete an employee (soft delete)
    /// </summary>
    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id);
        if (employee == null || employee.IsDeleted)
            return false;

        // Soft delete
        employee.IsDeleted = true;
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Employees.UpdateAsync(employee);
        return true;
    }

    /// <summary>
    /// Check if employee exists
    /// </summary>
    public async Task<bool> EmployeeExistsAsync(int id)
    {
        return await _unitOfWork.Employees.AnyAsync(e => e.EmployeeId == id && !e.IsDeleted);
    }

    /// <summary>
    /// Check if email already exists
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _unitOfWork.Employees.AnyAsync(e =>
            e.Email == email && !e.IsDeleted);
    }

    /// <summary>
    /// Check if email already exists (excluding specific employee)
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, int excludeEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _unitOfWork.Employees.AnyAsync(e =>
            e.Email == email && e.EmployeeId != excludeEmployeeId && !e.IsDeleted);
    }

    /// <summary>
    /// Get employee statistics
    /// </summary>
    public async Task<EmployeeStatsDto> GetEmployeeStatsAsync()
    {
        var allEmployees = await _unitOfWork.Employees.FindAsync(e => !e.IsDeleted);

        var stats = new EmployeeStatsDto
        {
            TotalEmployees = allEmployees.Count(),
            ActiveEmployees = allEmployees.Count(e => e.IsActive),
            InactiveEmployees = allEmployees.Count(e => !e.IsActive),
            EmployeesByRole = allEmployees
                .Where(e => !string.IsNullOrEmpty(e.Role))
                .GroupBy(e => e.Role)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    /// <summary>
    /// Get total count of active employees
    /// </summary>
    public async Task<int> GetTotalEmployeeCountAsync()
    {
        return await _unitOfWork.Employees.CountAsync(e => !e.IsDeleted && e.IsActive);
    }

    /// <summary>
    /// Get all distinct roles
    /// </summary>
    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => !e.IsDeleted && !string.IsNullOrEmpty(e.Role));
        return employees
            .Select(e => e.Role)
            .Distinct()
            .OrderBy(r => r)
            .ToList();
    }

    // ============ Helper Methods ============

    /// <summary>
    /// Map Employee entity to EmployeeDto
    /// </summary>
    private EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            EmployeeName = employee.EmployeeName,
            Email = employee.Email,
            Phone = employee.Phone,
            Role = employee.Role,
            IsActive = employee.IsActive,
            UserId = employee.UserId,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            CreatedByUserId = employee.CreatedByUserId,
            UpdatedByUserId = employee.UpdatedByUserId
        };
    }
}