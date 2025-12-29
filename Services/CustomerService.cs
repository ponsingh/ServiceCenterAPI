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
/// Data Transfer Object for Customer (Read)
/// </summary>
public class CustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string ContactNumber { get; set; } = null!;
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string CustomerType { get; set; } = null!;
    public string? GstNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public int? UpdatedByUserId { get; set; }
}

/// <summary>
/// Data Transfer Object for Creating Customer
/// </summary>
public class CreateCustomerDto
{
    public string CustomerName { get; set; } = null!;
    public string ContactNumber { get; set; } = null!;
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string CustomerType { get; set; } = null!;
    public string? GstNumber { get; set; }
}

/// <summary>
/// Data Transfer Object for Updating Customer
/// </summary>
public class UpdateCustomerDto
{
    public string? CustomerName { get; set; }
    public string? ContactNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? CustomerType { get; set; }
    public string? GstNumber { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Data Transfer Object for Customer Statistics
/// </summary>
public class CustomerStatsDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public Dictionary<string, int> CustomersByType { get; set; } = new();
}

// ============ Service Interface ============

/// <summary>
/// Interface for Customer Service
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Get customer by ID
    /// </summary>
    Task<CustomerDto?> GetCustomerByIdAsync(int id);

    /// <summary>
    /// Get all customers
    /// </summary>
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();

    /// <summary>
    /// Get active customers only
    /// </summary>
    Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync();

    /// <summary>
    /// Get customers filtered by customer type
    /// </summary>
    Task<IEnumerable<CustomerDto>> GetCustomersByTypeAsync(string customerType);

    /// <summary>
    /// Search customers by name
    /// </summary>
    Task<IEnumerable<CustomerDto>> SearchCustomersByNameAsync(string name);

    /// <summary>
    /// Get customer by contact number
    /// </summary>
    Task<CustomerDto?> GetCustomerByContactNumberAsync(string contactNumber);

    /// <summary>
    /// Get customer by email
    /// </summary>
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);

    /// <summary>
    /// Create new customer
    /// </summary>
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto, int userId);

    /// <summary>
    /// Update customer
    /// </summary>
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto, int userId);

    /// <summary>
    /// Delete customer (soft delete)
    /// </summary>
    Task<bool> DeleteCustomerAsync(int id);

    /// <summary>
    /// Check if customer exists
    /// </summary>
    Task<bool> CustomerExistsAsync(int id);

    /// <summary>
    /// Check if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Check if email exists (exclude customer)
    /// </summary>
    Task<bool> EmailExistsAsync(string email, int excludeCustomerId);

    /// <summary>
    /// Check if contact number exists
    /// </summary>
    Task<bool> ContactNumberExistsAsync(string contactNumber);

    /// <summary>
    /// Check if contact number exists (exclude customer)
    /// </summary>
    Task<bool> ContactNumberExistsAsync(string contactNumber, int excludeCustomerId);

    /// <summary>
    /// Get customer statistics
    /// </summary>
    Task<CustomerStatsDto> GetCustomerStatsAsync();

    /// <summary>
    /// Get total customer count
    /// </summary>
    Task<int> GetTotalCustomerCountAsync();

    /// <summary>
    /// Get all distinct customer types
    /// </summary>
    Task<IEnumerable<string>> GetAllCustomerTypesAsync();
}

// ============ Service Implementation ============

/// <summary>
/// Implementation of Customer Service
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
            return null;

        return MapToDto(customer);
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.FindAsync(c => !c.IsDeleted);
        return customers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get active customers only
    /// </summary>
    public async Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.FindAsync(c =>
            !c.IsDeleted && c.IsActive);
        return customers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get customers filtered by customer type
    /// </summary>
    public async Task<IEnumerable<CustomerDto>> GetCustomersByTypeAsync(string customerType)
    {
        if (string.IsNullOrWhiteSpace(customerType))
            return new List<CustomerDto>();

        var customers = await _unitOfWork.Customers.FindAsync(c =>
            c.CustomerType == customerType && !c.IsDeleted);
        return customers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Search customers by name
    /// </summary>
    public async Task<IEnumerable<CustomerDto>> SearchCustomersByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<CustomerDto>();

        var searchTerm = name.ToLower();
        var customers = await _unitOfWork.Customers.FindAsync(c =>
            c.CustomerName.ToLower().Contains(searchTerm) && !c.IsDeleted);
        return customers.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get customer by contact number
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByContactNumberAsync(string contactNumber)
    {
        if (string.IsNullOrWhiteSpace(contactNumber))
            return null;

        var customer = (await _unitOfWork.Customers.FindAsync(c =>
            c.ContactNumber == contactNumber && !c.IsDeleted)).FirstOrDefault();

        return customer == null ? null : MapToDto(customer);
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var customer = (await _unitOfWork.Customers.FindAsync(c =>
            c.Email == email && !c.IsDeleted)).FirstOrDefault();

        return customer == null ? null : MapToDto(customer);
    }

    /// <summary>
    /// Create new customer
    /// </summary>
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto, int userId)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            throw new ArgumentException("Customer name is required");

        if (string.IsNullOrWhiteSpace(dto.ContactNumber))
            throw new ArgumentException("Contact number is required");

        if (string.IsNullOrWhiteSpace(dto.CustomerType))
            throw new ArgumentException("Customer type is required");

        // Check if email already exists (if provided)
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            bool emailExists = await EmailExistsAsync(dto.Email);
            if (emailExists)
                throw new InvalidOperationException($"Email '{dto.Email}' is already registered");
        }

        // Check if contact number already exists
        bool contactExists = await ContactNumberExistsAsync(dto.ContactNumber);
        if (contactExists)
            throw new InvalidOperationException($"Contact number '{dto.ContactNumber}' is already registered");

        var customer = new Customer
        {
            CustomerName = dto.CustomerName,
            ContactNumber = dto.ContactNumber,
            WhatsAppNumber = dto.WhatsAppNumber,
            Email = dto.Email,
            Address = dto.Address,
            CustomerType = dto.CustomerType,
            GstNumber = dto.GstNumber,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(customer);
    }

    /// <summary>
    /// Update customer
    /// </summary>
    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto, int userId)
    {
        if (id <= 0)
            return null;

        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
            return null;

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(dto.CustomerName))
            customer.CustomerName = dto.CustomerName;

        if (!string.IsNullOrWhiteSpace(dto.ContactNumber))
        {
            // Check if new contact number is unique (exclude current customer)
            bool contactExists = await ContactNumberExistsAsync(dto.ContactNumber, id);
            if (contactExists)
                throw new InvalidOperationException($"Contact number '{dto.ContactNumber}' is already registered");

            customer.ContactNumber = dto.ContactNumber;
        }

        if (dto.WhatsAppNumber != null)
            customer.WhatsAppNumber = dto.WhatsAppNumber;

        if (dto.Email != null)
        {
            // Check if new email is unique (exclude current customer)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool emailExists = await EmailExistsAsync(dto.Email, id);
                if (emailExists)
                    throw new InvalidOperationException($"Email '{dto.Email}' is already registered");
            }
            customer.Email = dto.Email;
        }

        if (dto.Address != null)
            customer.Address = dto.Address;

        if (!string.IsNullOrWhiteSpace(dto.CustomerType))
            customer.CustomerType = dto.CustomerType;

        if (dto.GstNumber != null)
            customer.GstNumber = dto.GstNumber;

        if (dto.IsActive.HasValue)
            customer.IsActive = dto.IsActive.Value;

        customer.UpdatedAt = DateTime.UtcNow;
        customer.UpdatedByUserId = userId;

        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(customer);
    }

    /// <summary>
    /// Delete customer (soft delete)
    /// </summary>
    public async Task<bool> DeleteCustomerAsync(int id)
    {
        if (id <= 0)
            return false;

        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
            return false;

        customer.IsDeleted = true;
        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Check if customer exists
    /// </summary>
    public async Task<bool> CustomerExistsAsync(int id)
    {
        if (id <= 0)
            return false;

        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        return customer != null && !customer.IsDeleted;
    }

    /// <summary>
    /// Check if email exists
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var customer = (await _unitOfWork.Customers.FindAsync(c =>
            c.Email == email && !c.IsDeleted)).FirstOrDefault();

        return customer != null;
    }

    /// <summary>
    /// Check if email exists (exclude customer)
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, int excludeCustomerId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var customer = (await _unitOfWork.Customers.FindAsync(c =>
            c.Email == email && c.CustomerId != excludeCustomerId && !c.IsDeleted)).FirstOrDefault();

        return customer != null;
    }

    /// <summary>
    /// Check if contact number exists
    /// </summary>
    public async Task<bool> ContactNumberExistsAsync(string contactNumber)
    {
        if (string.IsNullOrWhiteSpace(contactNumber))
            return false;

        var customer = (await _unitOfWork.Customers.FindAsync(c =>
            c.ContactNumber == contactNumber && !c.IsDeleted)).FirstOrDefault();

        return customer != null;
    }

    /// <summary>
    /// Check if contact number exists (exclude customer)
    /// </summary>
    public async Task<bool> ContactNumberExistsAsync(string contactNumber, int excludeCustomerId)
    {
        if (string.IsNullOrWhiteSpace(contactNumber))
            return false;

        var customer = (await _unitOfWork.Customers.FindAsync(c =>
            c.ContactNumber == contactNumber && c.CustomerId != excludeCustomerId && !c.IsDeleted)).FirstOrDefault();

        return customer != null;
    }

    /// <summary>
    /// Get customer statistics
    /// </summary>
    public async Task<CustomerStatsDto> GetCustomerStatsAsync()
    {
        var allCustomers = await _unitOfWork.Customers.FindAsync(c => !c.IsDeleted);

        var stats = new CustomerStatsDto
        {
            TotalCustomers = allCustomers.Count(),
            ActiveCustomers = allCustomers.Count(c => c.IsActive),
            InactiveCustomers = allCustomers.Count(c => !c.IsActive),
            CustomersByType = allCustomers
                .Where(c => !string.IsNullOrEmpty(c.CustomerType))
                .GroupBy(c => c.CustomerType)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    /// <summary>
    /// Get total customer count
    /// </summary>
    public async Task<int> GetTotalCustomerCountAsync()
    {
        var customers = await _unitOfWork.Customers.FindAsync(c => !c.IsDeleted);
        return customers.Count();
    }

    /// <summary>
    /// Get all distinct customer types
    /// </summary>
    public async Task<IEnumerable<string>> GetAllCustomerTypesAsync()
    {
        var customers = await _unitOfWork.Customers.FindAsync(c =>
            !c.IsDeleted && !string.IsNullOrEmpty(c.CustomerType));
        return customers
            .Select(c => c.CustomerType)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    // ============ Helper Methods ============

    /// <summary>
    /// Map Customer entity to CustomerDto
    /// </summary>
    private CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.CustomerName,
            ContactNumber = customer.ContactNumber,
            WhatsAppNumber = customer.WhatsAppNumber,
            Email = customer.Email,
            Address = customer.Address,
            CustomerType = customer.CustomerType,
            GstNumber = customer.GstNumber,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            CreatedByUserId = customer.CreatedByUserId,
            UpdatedByUserId = customer.UpdatedByUserId
        };
    }
}