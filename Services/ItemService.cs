using ServiceCenterAPI.Infrastructure.Persistence.DbContextActions;
using ServiceCenterAPI.Infrastructure.Persistence.Models;
using ServiceCenterAPI.Infrastructure.Repositories;
using ServiceCenterAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenterAPI.Application.Services;

// ============ DTOs FOR ITEM ============

/// <summary>
/// DTO for creating Item
/// </summary>
public class CreateItemDto
{
    public int? DeviceTypeId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNo { get; set; }
    public string? Imei { get; set; }
    public string? Accessories { get; set; }
    public string? ConditionOnReceipt { get; set; }
}

/// <summary>
/// DTO for updating Item
/// </summary>
public class UpdateItemDto
{
    public int? DeviceTypeId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNo { get; set; }
    public string? Imei { get; set; }
    public string? Accessories { get; set; }
    public string? ConditionOnReceipt { get; set; }
    public string? InspectionStatus { get; set; }
}

// ============ SERVICE INTERFACE ============

/// <summary>
/// Service for managing Items and their operations
/// </summary>
public interface IItemService
{
    // GET Methods
    Task<ItemDetailedDto?> GetItemDetailedAsync(int itemId);
    Task<IEnumerable<ItemDetailedDto>> GetItemsByServiceOrderAsync(int serviceOrderId);
    Task<bool> ItemExistsAsync(int itemId);

    // CREATE Methods
    Task<ItemDetailedDto> AddItemToServiceOrderAsync(int serviceOrderId, CreateItemDto dto);

    // UPDATE Methods
    Task<ItemDetailedDto?> UpdateItemAsync(int itemId, UpdateItemDto dto);

    // DELETE Methods
    Task<bool> DeleteItemAsync(int itemId);
}

// ============ SERVICE IMPLEMENTATION ============

/// <summary>
/// Implementation of Item Service
/// </summary>
public class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public ItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get Item with all its Jobs and JobParts
    /// </summary>
    public async Task<ItemDetailedDto?> GetItemDetailedAsync(int itemId)
    {
        if (itemId <= 0)
            return null;

        var item = await _unitOfWork.Items.GetByIdAsync(itemId);
        if (item == null || item.IsDeleted)
            return null;

        return await MapItemToDetailedDtoAsync(item);
    }

    /// <summary>
    /// Get all Items for a Service Order with their Jobs
    /// </summary>
    public async Task<IEnumerable<ItemDetailedDto>> GetItemsByServiceOrderAsync(int serviceOrderId)
    {
        if (serviceOrderId <= 0)
            return new List<ItemDetailedDto>();

        var items = await _unitOfWork.Items.FindAsync(i => i.ServiceOrderId == serviceOrderId && !i.IsDeleted);
        var result = new List<ItemDetailedDto>();

        foreach (var item in items)
        {
            var dto = await MapItemToDetailedDtoAsync(item);
            result.Add(dto);
        }

        return result;
    }

    /// <summary>
    /// Check if Item exists
    /// </summary>
    public async Task<bool> ItemExistsAsync(int itemId)
    {
        if (itemId <= 0)
            return false;

        var item = await _unitOfWork.Items.GetByIdAsync(itemId);
        return item != null && !item.IsDeleted;
    }

    /// <summary>
    /// Add Item to Service Order
    /// </summary>
    public async Task<ItemDetailedDto> AddItemToServiceOrderAsync(int serviceOrderId, CreateItemDto dto)
    {
        if (serviceOrderId <= 0)
            throw new ArgumentException("Invalid service order ID");

        // Verify service order exists
        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(serviceOrderId);
        if (serviceOrder == null || serviceOrder.IsDeleted)
            throw new InvalidOperationException($"Service order with ID {serviceOrderId} not found");

        // Create item
        var item = new Item
        {
            ServiceOrderId = serviceOrderId,
            DeviceTypeId = dto.DeviceTypeId,
            Brand = dto.Brand,
            Model = dto.Model,
            SerialNo = dto.SerialNo,
            Imei = dto.Imei,
            Accessories = dto.Accessories,
            ConditionOnReceipt = dto.ConditionOnReceipt,
            InspectionStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Items.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return await MapItemToDetailedDtoAsync(item);
    }

    /// <summary>
    /// Update Item
    /// </summary>
    public async Task<ItemDetailedDto?> UpdateItemAsync(int itemId, UpdateItemDto dto)
    {
        if (itemId <= 0)
            return null;

        var item = await _unitOfWork.Items.GetByIdAsync(itemId);
        if (item == null || item.IsDeleted)
            return null;

        // Update properties
        if (dto.DeviceTypeId.HasValue)
            item.DeviceTypeId = dto.DeviceTypeId;

        if (!string.IsNullOrWhiteSpace(dto.Brand))
            item.Brand = dto.Brand;

        if (!string.IsNullOrWhiteSpace(dto.Model))
            item.Model = dto.Model;

        if (!string.IsNullOrWhiteSpace(dto.SerialNo))
            item.SerialNo = dto.SerialNo;

        if (!string.IsNullOrWhiteSpace(dto.Imei))
            item.Imei = dto.Imei;

        if (!string.IsNullOrWhiteSpace(dto.Accessories))
            item.Accessories = dto.Accessories;

        if (!string.IsNullOrWhiteSpace(dto.ConditionOnReceipt))
            item.ConditionOnReceipt = dto.ConditionOnReceipt;

        if (!string.IsNullOrWhiteSpace(dto.InspectionStatus))
            item.InspectionStatus = dto.InspectionStatus;

        item.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Items.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return await GetItemDetailedAsync(itemId);
    }

    /// <summary>
    /// Delete Item (soft delete - and its jobs)
    /// </summary>
    public async Task<bool> DeleteItemAsync(int itemId)
    {
        if (itemId <= 0)
            return false;

        var item = await _unitOfWork.Items.GetByIdAsync(itemId);
        if (item == null || item.IsDeleted)
            return false;

        // Soft delete all jobs for this item
        var jobs = await _unitOfWork.Jobs.FindAsync(j => j.ItemId == itemId && !j.IsDeleted);
        foreach (var job in jobs)
        {
            job.IsDeleted = true;
            job.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Jobs.UpdateAsync(job);
        }

        // Soft delete item
        item.IsDeleted = true;
        item.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Items.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ============ HELPER METHODS ============

    private async Task<ItemDetailedDto> MapItemToDetailedDtoAsync(Item item)
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
            var jobDto = await MapJobToDetailedDtoAsync(job);
            itemDto.Jobs.Add(jobDto);
        }

        return itemDto;
    }

    private async Task<JobDetailedDto> MapJobToDetailedDtoAsync(Job job)
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