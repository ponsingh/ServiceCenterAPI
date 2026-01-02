using ServiceCenterAPI.Infrastructure.Persistence.DbContextActions;
using ServiceCenterAPI.Infrastructure.Persistence.Models;
using ServiceCenterAPI.Infrastructure.Repositories;
using ServiceCenterAPI.Repositories;
using ServiceCenterAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenterAPI.Application.Services;

// ============ SERVICE INTERFACE ============

/// <summary>
/// Smart update service that automatically detects changes
/// Reuses existing CreateServiceOrderCompleteDto
/// </summary>
public interface ISmartUpdateCompositeService
{
    /// <summary>
    /// Update ServiceOrder by comparing client data with DB data
    /// Automatically detects: Added, Modified, Deleted items/jobs/parts
    /// 
    /// Client sends ONLY items they want to keep (new + existing)
    /// Items NOT in the list are automatically deleted
    /// </summary>
    Task<ServiceOrderDetailedDto> SmartUpdateServiceOrderAsync(CreateServiceOrderCompleteDto dto);
}

// ============ SERVICE IMPLEMENTATION ============

/// <summary>
/// Smart update service - compares client data with DB and auto-detects changes
/// Reuses existing DTOs to avoid complexity
/// </summary>
public class SmartUpdateCompositeService : ISmartUpdateCompositeService
{
    private readonly IUnitOfWork _unitOfWork;

    public SmartUpdateCompositeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Update ServiceOrder intelligently:
    /// 1. Fetch entire SO hierarchy from DB
    /// 2. Compare with client data
    /// 3. Auto-detect: Added, Modified, Deleted
    /// 4. Apply all changes in single transaction
    /// </summary>
    public async Task<ServiceOrderDetailedDto> SmartUpdateServiceOrderAsync(CreateServiceOrderCompleteDto dto)
    {
        if (dto.ServiceOrderId <= 0 && dto.ServiceOrderId == 0)
            throw new ArgumentException("ServiceOrderId is required for update");

        // Extract SO ID - either from ServiceOrderId property if it exists, or from context
        int serviceOrderId = GetServiceOrderId(dto);
        if (serviceOrderId <= 0)
            throw new ArgumentException("Invalid service order ID");

        // Fetch SO from DB
        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(serviceOrderId);
        if (serviceOrder == null || serviceOrder.IsDeleted)
            throw new InvalidOperationException($"Service order with ID {serviceOrderId} not found");

        // Start transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // ========== UPDATE ServiceOrder Fields ==========
            await UpdateServiceOrderFields(serviceOrder, dto);

            // ========== Fetch Current Items from DB ==========
            var dbItems = await _unitOfWork.Items.FindAsync(i => i.ServiceOrderId == serviceOrderId && !i.IsDeleted);

            // ========== DETECT ITEM CHANGES ==========
            var clientItemIds = dto.Items?.Where(i => i.ItemId > 0).Select(i => i.ItemId).ToList() ?? new List<int>();
            var dbItemIds = dbItems.Select(i => i.ItemId).ToList();

            // Items to delete (in DB but not in client)
            var itemsToDelete = dbItemIds.Except(clientItemIds).ToList();
            await DeleteItems(itemsToDelete);

            // Items to add (in client but ItemId is 0 or default)
            var itemsToAdd = dto.Items?.Where(i => i.ItemId <= 0).ToList() ?? new List<CreateItemCompositeDto>();
            foreach (var itemDto in itemsToAdd)
            {
                await AddItem(serviceOrder.ServiceOrderId, itemDto);
            }

            // Items to update (in both DB and client)
            var itemsToUpdate = dto.Items?.Where(i => i.ItemId > 0).ToList() ?? new List<CreateItemCompositeDto>();
            foreach (var clientItem in itemsToUpdate)
            {
                var dbItem = dbItems.FirstOrDefault(i => i.ItemId == clientItem.ItemId);
                if (dbItem != null)
                {
                    await UpdateItem(dbItem, clientItem);
                }
            }

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // Load and return complete DTO
            var result = await GetServiceOrderDetailedDtoAsync(serviceOrderId);
            return result ?? throw new InvalidOperationException("Failed to retrieve updated service order");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    // ============ SERVICEORDER UPDATES ============

    private int GetServiceOrderId(CreateServiceOrderCompleteDto dto)
    {
        // Check if ServiceOrderId property exists (added for update)
        var prop = dto.GetType().GetProperty("ServiceOrderId");
        if (prop != null)
        {
            var value = prop.GetValue(dto);
            if (value is int id && id > 0)
                return id;
        }
        return 0;
    }

    private async Task UpdateServiceOrderFields(ServiceOrder serviceOrder, CreateServiceOrderCompleteDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            serviceOrder.Notes = dto.Notes;

        if (dto.ExpectedPickupDate.HasValue)
            serviceOrder.ExpectedPickupDate = dto.ExpectedPickupDate;

        if (dto.StatusId.HasValue)
            serviceOrder.StatusId = dto.StatusId;

        serviceOrder.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ServiceOrders.UpdateAsync(serviceOrder);
        await _unitOfWork.SaveChangesAsync();
    }

    // ============ ITEM OPERATIONS ============

    private async Task AddItem(int serviceOrderId, CreateItemCompositeDto itemDto)
    {
        var item = new Item
        {
            ServiceOrderId = serviceOrderId,
            DeviceTypeId = itemDto.DeviceTypeId,
            Brand = itemDto.Brand,
            Model = itemDto.Model,
            SerialNo = itemDto.SerialNo,
            Imei = itemDto.Imei,
            Accessories = itemDto.Accessories,
            ConditionOnReceipt = itemDto.ConditionOnReceipt,
            InspectionStatus = "Pending",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.Items.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        // Add jobs for this new item
        if (itemDto.Jobs != null && itemDto.Jobs.Count > 0)
        {
            foreach (var jobDto in itemDto.Jobs)
            {
                await AddJob(item.ItemId, jobDto);
            }
        }
    }

    private async Task DeleteItems(List<int> itemsToDelete)
    {
        foreach (var itemId in itemsToDelete)
        {
            var item = await _unitOfWork.Items.GetByIdAsync(itemId);
            if (item != null && !item.IsDeleted)
            {
                // Soft delete all jobs (cascades to parts)
                var jobs = await _unitOfWork.Jobs.FindAsync(j => j.ItemId == itemId && !j.IsDeleted);
                foreach (var job in jobs)
                {
                    await DeleteJobInternal(job);
                }

                // Soft delete item
                item.IsDeleted = true;
                item.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Items.UpdateAsync(item);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateItem(Item dbItem, CreateItemCompositeDto clientItem)
    {
        // Update item fields
        if (clientItem.DeviceTypeId.HasValue)
            dbItem.DeviceTypeId = clientItem.DeviceTypeId;

        if (!string.IsNullOrWhiteSpace(clientItem.Brand))
            dbItem.Brand = clientItem.Brand;

        if (!string.IsNullOrWhiteSpace(clientItem.Model))
            dbItem.Model = clientItem.Model;

        if (!string.IsNullOrWhiteSpace(clientItem.SerialNo))
            dbItem.SerialNo = clientItem.SerialNo;

        if (!string.IsNullOrWhiteSpace(clientItem.Imei))
            dbItem.Imei = clientItem.Imei;

        if (!string.IsNullOrWhiteSpace(clientItem.Accessories))
            dbItem.Accessories = clientItem.Accessories;

        if (!string.IsNullOrWhiteSpace(clientItem.ConditionOnReceipt))
            dbItem.ConditionOnReceipt = clientItem.ConditionOnReceipt;

        dbItem.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Items.UpdateAsync(dbItem);
        await _unitOfWork.SaveChangesAsync();

        // ========== UPDATE JOBS for this item ==========
        await UpdateJobs(dbItem.ItemId, clientItem.Jobs);
    }

    // ============ JOB OPERATIONS ============

    private async Task AddJob(int itemId, CreateJobCompositeDto jobDto)
    {
        var job = new Job
        {
            ItemId = itemId,
            ServiceTypeId = jobDto.ServiceTypeId,
            ReceivedDate = jobDto.ReceivedDate,
            AssignedTo = jobDto.AssignedTo,
            Priority = jobDto.Priority ?? "Normal",
            EstimatedCost = jobDto.EstimatedCost,
            ActualCost = 0,
            Diagnosis = jobDto.Diagnosis,
            TargetCompletionDate = jobDto.TargetCompletionDate,
            Notes = jobDto.Notes,
            JobStatusId = jobDto.JobStatusId,
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Jobs.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();

        // Add parts for this job
        if (jobDto.JobParts != null && jobDto.JobParts.Count > 0)
        {
            await AddJobParts(job.JobId, jobDto.JobParts);
        }
    }

    private async Task DeleteJobInternal(Job job)
    {
        // Soft delete all parts
        var parts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == job.JobId);
        foreach (var part in parts)
        {
            await _unitOfWork.JobParts.DeleteAsync(part);
        }

        // Soft delete job
        job.IsDeleted = true;
        job.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Jobs.UpdateAsync(job);
    }

    private async Task UpdateJobs(int itemId, List<CreateJobCompositeDto>? clientJobs)
    {
        // Fetch jobs for this item from DB
        var dbJobs = await _unitOfWork.Jobs.FindAsync(j => j.ItemId == itemId && !j.IsDeleted);

        // Get job IDs from client
        var clientJobIds = clientJobs?.Where(j => j.JobId > 0).Select(j => j.JobId).ToList() ?? new List<int>();
        var dbJobIds = dbJobs.Select(j => j.JobId).ToList();

        // Jobs to delete (in DB but not in client)
        var jobsToDelete = dbJobIds.Except(clientJobIds).ToList();
        foreach (var jobId in jobsToDelete)
        {
            var job = dbJobs.FirstOrDefault(j => j.JobId == jobId);
            if (job != null)
            {
                await DeleteJobInternal(job);
            }
        }

        // Jobs to add (in client but JobId is 0 or default)
        var jobsToAdd = clientJobs?.Where(j => j.JobId <= 0).ToList() ?? new List<CreateJobCompositeDto>();
        foreach (var jobDto in jobsToAdd)
        {
            await AddJob(itemId, jobDto);
        }

        // Jobs to update (in both DB and client)
        var jobsToUpdate = clientJobs?.Where(j => j.JobId > 0).ToList() ?? new List<CreateJobCompositeDto>();
        foreach (var clientJob in jobsToUpdate)
        {
            var dbJob = dbJobs.FirstOrDefault(j => j.JobId == clientJob.JobId);
            if (dbJob != null)
            {
                await UpdateJob(dbJob, clientJob);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateJob(Job dbJob, CreateJobCompositeDto clientJob)
    {
        if (clientJob.ServiceTypeId.HasValue)
            dbJob.ServiceTypeId = clientJob.ServiceTypeId;

        if (!string.IsNullOrWhiteSpace(clientJob.Priority))
            dbJob.Priority = clientJob.Priority;

        if (clientJob.EstimatedCost.HasValue)
            dbJob.EstimatedCost = clientJob.EstimatedCost;

        if (!string.IsNullOrWhiteSpace(clientJob.Diagnosis))
            dbJob.Diagnosis = clientJob.Diagnosis;

        if (clientJob.TargetCompletionDate.HasValue)
            dbJob.TargetCompletionDate = clientJob.TargetCompletionDate;

        if (!string.IsNullOrWhiteSpace(clientJob.Notes))
            dbJob.Notes = clientJob.Notes;

        if (clientJob.JobStatusId.HasValue)
            dbJob.JobStatusId = clientJob.JobStatusId;

        dbJob.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Jobs.UpdateAsync(dbJob);
        await _unitOfWork.SaveChangesAsync();

        // ========== UPDATE PARTS for this job ==========
        await UpdateJobParts(dbJob.JobId, clientJob.JobParts);
    }

    // ============ JOB PART OPERATIONS ============

    private async Task AddJobParts(int jobId, List<CreateJobPartCompositeDto> partsToAdd)
    {
        var jobParts = new List<JobPart>();

        foreach (var partDto in partsToAdd)
        {
            // Verify part exists
            var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId);
            if (part == null)
                throw new InvalidOperationException($"Part with ID {partDto.PartId} not found");

            var jobPart = new JobPart
            {
                JobId = jobId,
                PartId = partDto.PartId,
                Quantity = partDto.Quantity,
                UnitCost = partDto.UnitCost,
                TotalCost = partDto.UnitCost * partDto.Quantity,
                IsWarrantyPart = partDto.IsWarrantyPart,
                AddedAt = DateTime.UtcNow
            };

            jobParts.Add(jobPart);
        }

        if (jobParts.Count > 0)
        {
            await _unitOfWork.JobParts.AddRangeAsync(jobParts);
            await _unitOfWork.SaveChangesAsync();

            // Recalculate job cost
            await RecalculateJobCostAsync(jobId);
        }
    }

    private async Task DeleteJobParts(List<int> partIdsToDelete)
    {
        foreach (var partId in partIdsToDelete)
        {
            var part = await _unitOfWork.JobParts.GetByIdAsync(partId);
            if (part != null)
            {
                int jobId = part.JobId;
                await _unitOfWork.JobParts.DeleteAsync(part);
                // Recalculate after deletion
                await RecalculateJobCostAsync(jobId);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateJobParts(int jobId, List<CreateJobPartCompositeDto>? clientParts)
    {
        // Fetch parts for this job from DB
        var dbParts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == jobId);

        // Get part IDs from client
        var clientPartIds = clientParts?.Where(p => p.JobPartId > 0).Select(p => p.JobPartId).ToList() ?? new List<int>();
        var dbPartIds = dbParts.Select(p => p.JobPartId).ToList();

        // Parts to delete (in DB but not in client)
        var partsToDelete = dbPartIds.Except(clientPartIds).ToList();
        await DeleteJobParts(partsToDelete);

        // Parts to add (in client but JobPartId is 0 or default)
        var partsToAdd = clientParts?.Where(p => p.JobPartId <= 0).ToList() ?? new List<CreateJobPartCompositeDto>();
        await AddJobParts(jobId, partsToAdd);

        // Parts to update (in both DB and client)
        var partsToUpdate = clientParts?.Where(p => p.JobPartId > 0).ToList() ?? new List<CreateJobPartCompositeDto>();
        foreach (var clientPart in partsToUpdate)
        {
            var dbPart = dbParts.FirstOrDefault(p => p.JobPartId == clientPart.JobPartId);
            if (dbPart != null)
            {
                if (clientPart.Quantity > 0)
                    dbPart.Quantity = clientPart.Quantity;

                if (clientPart.UnitCost > 0)
                    dbPart.UnitCost = clientPart.UnitCost;

                dbPart.TotalCost = dbPart.UnitCost * dbPart.Quantity;
                dbPart.IsWarrantyPart = clientPart.IsWarrantyPart;

                await _unitOfWork.JobParts.UpdateAsync(dbPart);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        await RecalculateJobCostAsync(jobId);
    }

    // ============ HELPER METHODS ============

    private async Task RecalculateJobCostAsync(int jobId)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        if (job == null)
            return;

        var parts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == jobId);
        decimal totalCost = parts.Sum(p => p.TotalCost ?? 0);

        job.ActualCost = totalCost;
        job.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Jobs.UpdateAsync(job);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<ServiceOrderDetailedDto?> GetServiceOrderDetailedDtoAsync(int serviceOrderId)
    {
        var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(serviceOrderId);
        if (serviceOrder == null || serviceOrder.IsDeleted)
            return null;

        var dto = new ServiceOrderDetailedDto
        {
            ServiceOrderId = serviceOrder.ServiceOrderId,
            CustomerId = serviceOrder.CustomerId,
            CreatedByEmployeeId = serviceOrder.CreatedByEmployeeId,
            ServiceTypeId = serviceOrder.ServiceTypeId,
            ServiceOrderNumber = serviceOrder.ServiceOrderNumber,
            Notes = serviceOrder.Notes,
            ExpectedPickupDate = serviceOrder.ExpectedPickupDate,
            StatusId = serviceOrder.StatusId,
            CreatedAt = serviceOrder.CreatedAt,
            UpdatedAt = serviceOrder.UpdatedAt,
            Items = new List<ItemDetailedDto>(),
            Inspections = new List<InspectionDto>(),
            Complaints = new List<ComplaintDto>(),
            Invoices = new List<InvoiceDto>()
        };

        var items = await _unitOfWork.Items.FindAsync(i => i.ServiceOrderId == serviceOrderId && !i.IsDeleted);
        foreach (var item in items)
        {
            var itemDto = await GetItemDetailedDtoAsync(item.ItemId);
            if (itemDto != null)
                dto.Items.Add(itemDto);
        }

        return dto;
    }

    private async Task<ItemDetailedDto?> GetItemDetailedDtoAsync(int itemId)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(itemId);
        if (item == null || item.IsDeleted)
            return null;

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

        var jobs = await _unitOfWork.Jobs.FindAsync(j => j.ItemId == itemId && !j.IsDeleted);
        foreach (var job in jobs)
        {
            var jobDto = await GetJobDetailedDtoAsync(job.JobId);
            if (jobDto != null)
                itemDto.Jobs.Add(jobDto);
        }

        return itemDto;
    }

    private async Task<JobDetailedDto?> GetJobDetailedDtoAsync(int jobId)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        if (job == null || job.IsDeleted)
            return null;

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

        var parts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == jobId);
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