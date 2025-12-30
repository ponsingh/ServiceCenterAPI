using ServiceCenterAPI.Infrastructure.Persistence.DbContextActions;
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
/// DTO for ServiceOrder with all child collections
/// </summary>
public class ServiceOrderDetailedDto
{
    public int ServiceOrderId { get; set; }
    public int CustomerId { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public int? ServiceTypeId { get; set; }
    public string? ServiceOrderNumber { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpectedPickupDate { get; set; }
    public int? StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Child collections
    public ICollection<ItemDetailedDto> Items { get; set; } = new List<ItemDetailedDto>();
    public ICollection<InspectionDto> Inspections { get; set; } = new List<InspectionDto>();
    public ICollection<ComplaintDto> Complaints { get; set; } = new List<ComplaintDto>();
    public ICollection<InvoiceDto> Invoices { get; set; } = new List<InvoiceDto>();
}

/// <summary>
/// DTO for creating ServiceOrder
/// </summary>
public class CreateServiceOrderDto
{
    public int CustomerId { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public int? ServiceTypeId { get; set; }
    public string? ServiceOrderNumber { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpectedPickupDate { get; set; }
    public int? StatusId { get; set; }
}

/// <summary>
/// DTO for updating ServiceOrder
/// </summary>
public class UpdateServiceOrderDto
{
    public string? ServiceOrderNumber { get; set; }
    public string? Notes { get; set; }
    public DateOnly? ExpectedPickupDate { get; set; }
    public int? StatusId { get; set; }
    public int? ServiceTypeId { get; set; }
}

/// <summary>
/// DTO for Item
/// </summary>
public class ItemDetailedDto
{
    public int ItemId { get; set; }
    public int ServiceOrderId { get; set; }
    public int? DeviceTypeId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNo { get; set; }
    public string? Imei { get; set; }
    public string? Accessories { get; set; }
    public string? ConditionOnReceipt { get; set; }
    public string? InspectionStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Child jobs
    public ICollection<JobDetailedDto> Jobs { get; set; } = new List<JobDetailedDto>();
}

/// <summary>
/// DTO for Job
/// </summary>
public class JobDetailedDto
{
    public int JobId { get; set; }
    public int ItemId { get; set; }
    public int? ServiceTypeId { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int? AssignedTo { get; set; }
    public string? Priority { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? Diagnosis { get; set; }
    public DateTime? DiagnosisDate { get; set; }
    public DateOnly? TargetCompletionDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Notes { get; set; }
    public int? JobStatusId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Child parts
    public ICollection<JobPartDetailDto> JobParts { get; set; } = new List<JobPartDetailDto>();
}

/// <summary>
/// DTO for JobPart
/// </summary>
public class JobPartDetailDto
{
    public int JobPartId { get; set; }
    public int JobId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public bool IsWarrantyPart { get; set; }
    public DateTime AddedAt { get; set; }
}

/// <summary>
/// DTO for Inspection
/// </summary>
public class InspectionDto
{
    public int InspectionId { get; set; }
    public int ServiceOrderId { get; set; }
    public DateTime? InspectionDate { get; set; }
}

/// <summary>
/// DTO for Complaint
/// </summary>
public class ComplaintDto
{
    public int ComplaintId { get; set; }
    public int ServiceOrderId { get; set; }
}

/// <summary>
/// DTO for Invoice
/// </summary>
public class InvoiceDto
{
    public int InvoiceId { get; set; }
    public int ServiceOrderId { get; set; }
    public decimal? TotalAmount { get; set; }
}

// ============ SERVICE INTERFACE ============

/// <summary>
/// Service for managing Service Orders with all child entities
/// </summary>
public interface IServiceOrderService
{
    // Get methods
    Task<ServiceOrderDetailedDto?> GetServiceOrderDetailedAsync(int id);
    Task<ServiceOrderDetailedDto?> GetServiceOrderAsync(int id);
    Task<IEnumerable<ServiceOrderDetailedDto>> GetAllServiceOrdersAsync();
    Task<bool> ServiceOrderExistsAsync(int id);

    // Add/Create methods
    Task<ServiceOrderDetailedDto> CreateServiceOrderAsync(CreateServiceOrderDto dto);

    // Update methods
    Task<ServiceOrderDetailedDto?> UpdateServiceOrderAsync(int id, UpdateServiceOrderDto dto);

    // Delete methods
    Task<bool> DeleteServiceOrderAsync(int id);

    // Search methods
    Task<IEnumerable<ServiceOrderDetailedDto>> SearchByCustomerAsync(int customerId);
    Task<IEnumerable<ServiceOrderDetailedDto>> SearchByServiceOrderNumberAsync(string serviceOrderNumber);
}

// ============ SERVICE IMPLEMENTATION ============

/// <summary>
/// Service implementation for Service Order management
/// </summary>
public class ServiceOrderService : IServiceOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiceOrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get Service Order with all child entities
    /// </summary>
    public async Task<ServiceOrderDetailedDto?> GetServiceOrderDetailedAsync(int id)
    {
        if (id <= 0)
            return null;

        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
        if (serviceOrder == null || serviceOrder.IsDeleted)
            return null;

        var dto = MapToDetailedDto(serviceOrder);

        // Load all items
        var items = await _unitOfWork.Items.FindAsync(i => i.ServiceOrderId == id && !i.IsDeleted);
        foreach (var item in items)
        {
            var itemDto = await MapItemToDetailedDto(item);
            dto.Items.Add(itemDto);
        }

        // Load all inspections
        var inspections = await _unitOfWork.Inspections.FindAsync(i => i.ServiceOrderId == id);
        foreach (var inspection in inspections)
        {
            dto.Inspections.Add(new InspectionDto
            {
                InspectionId = inspection.InspectionId,
                ServiceOrderId = inspection.ServiceOrderId ?? 0,
                InspectionDate = inspection.PerformedAt
            });
        }

        // Load all complaints
        var complaints = await _unitOfWork.Complaints.FindAsync(c => c.ServiceOrderId == id);
        foreach (var complaint in complaints)
        {
            dto.Complaints.Add(new ComplaintDto
            {
                ComplaintId = complaint.ComplaintId,
                ServiceOrderId = complaint.ServiceOrderId
            });
        }

        // Load all invoices
        var invoices = await _unitOfWork.Invoices.FindAsync(inv => inv.ServiceOrderId == id);
        foreach (var invoice in invoices)
        {
            dto.Invoices.Add(new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                ServiceOrderId = invoice.ServiceOrderId ?? 0,
                TotalAmount = invoice.GrandTotal
            });
        }

        return dto;
    }

    /// <summary>
    /// Get single Service Order (lightweight, without deep children)
    /// </summary>
    public async Task<ServiceOrderDetailedDto?> GetServiceOrderAsync(int id)
    {
        if (id <= 0)
            return null;

        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
        if (serviceOrder == null || serviceOrder.IsDeleted)
            return null;

        return MapToDetailedDto(serviceOrder);
    }

    /// <summary>
    /// Get all Service Orders
    /// </summary>
    public async Task<IEnumerable<ServiceOrderDetailedDto>> GetAllServiceOrdersAsync()
    {
        var serviceOrders = await _unitOfWork.ServiceOrders.FindAsync(so => !so.IsDeleted);
        var result = new List<ServiceOrderDetailedDto>();

        foreach (var so in serviceOrders)
        {
            var dto = await GetServiceOrderDetailedAsync(so.ServiceOrderId);
            if (dto != null)
                result.Add(dto);
        }

        return result;
    }

    /// <summary>
    /// Check if Service Order exists
    /// </summary>
    public async Task<bool> ServiceOrderExistsAsync(int id)
    {
        if (id <= 0)
            return false;

        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
        return serviceOrder != null && !serviceOrder.IsDeleted;
    }

    /// <summary>
    /// Create new Service Order
    /// </summary>
    public async Task<ServiceOrderDetailedDto> CreateServiceOrderAsync(CreateServiceOrderDto dto)
    {
        if (dto.CustomerId <= 0)
            throw new ArgumentException("Invalid customer ID");

        // Verify customer exists
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException($"Customer with ID {dto.CustomerId} not found");

        var serviceOrder = new ServiceOrder
        {
            CustomerId = dto.CustomerId,
            CreatedByEmployeeId = dto.CreatedByEmployeeId,
            ServiceTypeId = dto.ServiceTypeId,
            ServiceOrderNumber = dto.ServiceOrderNumber,
            Notes = dto.Notes,
            ExpectedPickupDate = dto.ExpectedPickupDate,
            StatusId = dto.StatusId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.ServiceOrders.AddAsync(serviceOrder);
        await _unitOfWork.SaveChangesAsync();

        return MapToDetailedDto(serviceOrder);
    }

    /// <summary>
    /// Update Service Order
    /// </summary>
    public async Task<ServiceOrderDetailedDto?> UpdateServiceOrderAsync(int id, UpdateServiceOrderDto dto)
    {
        if (id <= 0)
            return null;

        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
        if (serviceOrder == null || serviceOrder.IsDeleted)
            return null;

        if (!string.IsNullOrWhiteSpace(dto.ServiceOrderNumber))
            serviceOrder.ServiceOrderNumber = dto.ServiceOrderNumber;

        if (dto.Notes != null)
            serviceOrder.Notes = dto.Notes;

        if (dto.ExpectedPickupDate.HasValue)
            serviceOrder.ExpectedPickupDate = dto.ExpectedPickupDate;

        if (dto.StatusId.HasValue)
            serviceOrder.StatusId = dto.StatusId;

        if (dto.ServiceTypeId.HasValue)
            serviceOrder.ServiceTypeId = dto.ServiceTypeId;

        serviceOrder.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
        await _unitOfWork.SaveChangesAsync();

        return await GetServiceOrderDetailedAsync(id);
    }

    /// <summary>
    /// Delete Service Order (soft delete)
    /// </summary>
    public async Task<bool> DeleteServiceOrderAsync(int id)
    {
        if (id <= 0)
            return false;

        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(id);
        if (serviceOrder == null)
            return false;

        // Delete all items for this service order
        var items = await _unitOfWork.Items.FindAsync(i => i.ServiceOrderId == id);
        foreach (var item in items)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Items.UpdateAsync(item);
        }

        // Soft delete service order
        serviceOrder.IsDeleted = true;
        serviceOrder.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Search Service Orders by Customer
    /// </summary>
    public async Task<IEnumerable<ServiceOrderDetailedDto>> SearchByCustomerAsync(int customerId)
    {
        if (customerId <= 0)
            return new List<ServiceOrderDetailedDto>();

        var serviceOrders = await _unitOfWork.ServiceOrders.FindAsync(
            so => so.CustomerId == customerId && !so.IsDeleted);

        var result = new List<ServiceOrderDetailedDto>();
        foreach (var so in serviceOrders)
        {
            var dto = await GetServiceOrderDetailedAsync(so.ServiceOrderId);
            if (dto != null)
                result.Add(dto);
        }

        return result;
    }

    /// <summary>
    /// Search Service Orders by Service Order Number
    /// </summary>
    public async Task<IEnumerable<ServiceOrderDetailedDto>> SearchByServiceOrderNumberAsync(string serviceOrderNumber)
    {
        if (string.IsNullOrWhiteSpace(serviceOrderNumber))
            return new List<ServiceOrderDetailedDto>();

        var term = serviceOrderNumber.ToLower();
        var serviceOrders = await _unitOfWork.ServiceOrders.FindAsync(
            so => so.ServiceOrderNumber != null &&
                  so.ServiceOrderNumber.ToLower().Contains(term) &&
                  !so.IsDeleted);

        var result = new List<ServiceOrderDetailedDto>();
        foreach (var so in serviceOrders)
        {
            var dto = await GetServiceOrderDetailedAsync(so.ServiceOrderId);
            if (dto != null)
                result.Add(dto);
        }

        return result;
    }

    // ============ HELPER METHODS ============

    private ServiceOrderDetailedDto MapToDetailedDto(ServiceOrder entity)
    {
        return new ServiceOrderDetailedDto
        {
            ServiceOrderId = entity.ServiceOrderId,
            CustomerId = entity.CustomerId,
            CreatedByEmployeeId = entity.CreatedByEmployeeId,
            ServiceTypeId = entity.ServiceTypeId,
            ServiceOrderNumber = entity.ServiceOrderNumber,
            Notes = entity.Notes,
            ExpectedPickupDate = entity.ExpectedPickupDate,
            StatusId = entity.StatusId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Items = new List<ItemDetailedDto>(),
            Inspections = new List<InspectionDto>(),
            Complaints = new List<ComplaintDto>(),
            Invoices = new List<InvoiceDto>()
        };
    }

    private async Task<ItemDetailedDto> MapItemToDetailedDto(Item item)
    {
        var itemDto = new ItemDetailedDto
        {
            ItemId = item.ItemId,
            ServiceOrderId = item.ServiceOrderId,
            DeviceTypeId = item.DeviceTypeId,
            Brand = item.Brand,
            Model = item.Model,
            SerialNo = item.SerialNo,
            Imei = item.Imei,
            Accessories = item.Accessories,
            ConditionOnReceipt = item.ConditionOnReceipt,
            InspectionStatus = item.InspectionStatus,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            Jobs = new List<JobDetailedDto>()
        };

        // Load all jobs for this item
        var jobs = await _unitOfWork.Jobs.FindAsync(j => j.ItemId == item.ItemId && !j.IsDeleted);
        foreach (var job in jobs)
        {
            var jobDto = await MapJobToDetailedDto(job);
            itemDto.Jobs.Add(jobDto);
        }

        return itemDto;
    }

    private async Task<JobDetailedDto> MapJobToDetailedDto(Job job)
    {
        var jobDto = new JobDetailedDto
        {
            JobId = job.JobId,
            ItemId = job.ItemId,
            ServiceTypeId = job.ServiceTypeId,
            ReceivedDate = job.ReceivedDate,
            AssignedTo = job.AssignedTo,
            Priority = job.Priority,
            EstimatedCost = job.EstimatedCost,
            ActualCost = job.ActualCost,
            Diagnosis = job.Diagnosis,
            DiagnosisDate = job.DiagnosisDate,
            TargetCompletionDate = job.TargetCompletionDate,
            CompletionDate = job.CompletionDate,
            Notes = job.Notes,
            JobStatusId = job.JobStatusId,
            UpdatedAt = job.UpdatedAt,
            JobParts = new List<JobPartDetailDto>()
        };

        // Load all job parts
        var parts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == job.JobId);
        foreach (var part in parts)
        {
            jobDto.JobParts.Add(new JobPartDetailDto
            {
                JobPartId = part.JobPartId,
                JobId = part.JobId,
                PartId = part.PartId,
                Quantity = part.Quantity,
                UnitCost = part.UnitCost,
                TotalCost = part.TotalCost,
                IsWarrantyPart = part.IsWarrantyPart,
                AddedAt = part.AddedAt
            });
        }

        return jobDto;
    }
}