using ServiceCenterAPI.Infrastructure.Persistence.DbContextActions;
using ServiceCenterAPI.Infrastructure.Persistence.Models;
using ServiceCenterAPI.Infrastructure.Repositories;
using ServiceCenterAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceCenterAPI.Application.Services;

// ============ DTOs FOR JOB ============

/// <summary>
/// DTO for creating Job
/// </summary>
public class CreateJobDto
{
    public int? ServiceTypeId { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public int? AssignedTo { get; set; }
    public string? Priority { get; set; } = "Normal";
    public decimal? EstimatedCost { get; set; }
    public string? Diagnosis { get; set; }
    public DateOnly? TargetCompletionDate { get; set; }
    public string? Notes { get; set; }
    public int? JobStatusId { get; set; }
}

/// <summary>
/// DTO for updating Job
/// </summary>
public class UpdateJobDto
{
    public int? ServiceTypeId { get; set; }
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
}

/// <summary>
/// DTO for creating JobPart
/// </summary>
public class CreateJobPartDto
{
    public int PartId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public bool IsWarrantyPart { get; set; } = false;
}

/// <summary>
/// DTO for updating JobPart
/// </summary>
public class UpdateJobPartDto
{
    public int? Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public bool? IsWarrantyPart { get; set; }
}

/// <summary>
/// DTO for bulk adding JobParts
/// </summary>
public class BulkAddJobPartsDto
{
    public List<CreateJobPartDto> JobParts { get; set; } = new();
}

// ============ SERVICE INTERFACE ============

/// <summary>
/// Service for managing Jobs and their JobParts
/// </summary>
public interface IJobService
{
    // GET Methods
    Task<JobDetailedDto?> GetJobDetailedAsync(int jobId);
    Task<IEnumerable<JobDetailedDto>> GetJobsByItemAsync(int itemId);
    Task<bool> JobExistsAsync(int jobId);

    // CREATE Methods
    Task<JobDetailedDto> AddJobToItemAsync(int itemId, CreateJobDto dto);

    // UPDATE Methods
    Task<JobDetailedDto?> UpdateJobAsync(int jobId, UpdateJobDto dto);

    // DELETE Methods
    Task<bool> DeleteJobAsync(int jobId);

    // Job Part Operations
    Task<JobPartDetailDto> AddJobPartAsync(int jobId, CreateJobPartDto dto);
    Task<IEnumerable<JobPartDetailDto>> BulkAddJobPartsAsync(int jobId, IEnumerable<CreateJobPartDto> dtos);
    Task<JobPartDetailDto?> UpdateJobPartAsync(int jobPartId, UpdateJobPartDto dto);
    Task<bool> DeleteJobPartAsync(int jobPartId);
    Task<IEnumerable<JobPartDetailDto>> GetJobPartsAsync(int jobId);
}

// ============ SERVICE IMPLEMENTATION ============

/// <summary>
/// Implementation of Job Service
/// </summary>
public class JobService : IJobService
{
    private readonly IUnitOfWork _unitOfWork;

    public JobService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ============ JOB GET METHODS ============

    public async Task<JobDetailedDto?> GetJobDetailedAsync(int jobId)
    {
        if (jobId <= 0)
            return null;

        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        if (job == null || job.IsDeleted)
            return null;

        return await MapJobToDetailedDtoAsync(job);
    }

    public async Task<IEnumerable<JobDetailedDto>> GetJobsByItemAsync(int itemId)
    {
        if (itemId <= 0)
            return new List<JobDetailedDto>();

        var jobs = await _unitOfWork.Jobs.FindAsync(j => j.ItemId == itemId && !j.IsDeleted);
        var result = new List<JobDetailedDto>();

        foreach (var job in jobs)
        {
            var dto = await MapJobToDetailedDtoAsync(job);
            result.Add(dto);
        }

        return result;
    }

    public async Task<bool> JobExistsAsync(int jobId)
    {
        if (jobId <= 0)
            return false;

        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        return job != null && !job.IsDeleted;
    }

    // ============ JOB CREATE METHODS ============

    public async Task<JobDetailedDto> AddJobToItemAsync(int itemId, CreateJobDto dto)
    {
        if (itemId <= 0)
            throw new ArgumentException("Invalid item ID");

        var item = await _unitOfWork.Items.GetByIdAsync(itemId);
        if (item == null || item.IsDeleted)
            throw new InvalidOperationException($"Item with ID {itemId} not found");

        var job = new Job
        {
            ItemId = itemId,
            ServiceTypeId = dto.ServiceTypeId,
            ReceivedDate = dto.ReceivedDate,
            AssignedTo = dto.AssignedTo,
            Priority = dto.Priority ?? "Normal",
            EstimatedCost = dto.EstimatedCost,
            ActualCost = 0,
            Diagnosis = dto.Diagnosis,
            TargetCompletionDate = dto.TargetCompletionDate,
            Notes = dto.Notes,
            JobStatusId = dto.JobStatusId,
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Jobs.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();

        return await MapJobToDetailedDtoAsync(job);
    }

    // ============ JOB UPDATE METHODS ============

    public async Task<JobDetailedDto?> UpdateJobAsync(int jobId, UpdateJobDto dto)
    {
        if (jobId <= 0)
            return null;

        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        if (job == null || job.IsDeleted)
            return null;

        if (dto.ServiceTypeId.HasValue)
            job.ServiceTypeId = dto.ServiceTypeId;

        if (dto.AssignedTo.HasValue)
            job.AssignedTo = dto.AssignedTo;

        if (!string.IsNullOrWhiteSpace(dto.Priority))
            job.Priority = dto.Priority;

        if (dto.EstimatedCost.HasValue)
            job.EstimatedCost = dto.EstimatedCost;

        if (dto.ActualCost.HasValue)
            job.ActualCost = dto.ActualCost;

        if (!string.IsNullOrWhiteSpace(dto.Diagnosis))
            job.Diagnosis = dto.Diagnosis;

        if (dto.DiagnosisDate.HasValue)
            job.DiagnosisDate = dto.DiagnosisDate;

        if (dto.TargetCompletionDate.HasValue)
            job.TargetCompletionDate = dto.TargetCompletionDate;

        if (dto.CompletionDate.HasValue)
            job.CompletionDate = dto.CompletionDate;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            job.Notes = dto.Notes;

        if (dto.JobStatusId.HasValue)
            job.JobStatusId = dto.JobStatusId;

        job.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Jobs.UpdateAsync(job);
        await _unitOfWork.SaveChangesAsync();

        return await GetJobDetailedAsync(jobId);
    }

    // ============ JOB DELETE METHODS ============

    public async Task<bool> DeleteJobAsync(int jobId)
    {
        if (jobId <= 0)
            return false;

        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        if (job == null || job.IsDeleted)
            return false;

        var parts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == jobId);
        foreach (var part in parts)
        {
            await _unitOfWork.JobParts.DeleteAsync(part);
        }

        job.IsDeleted = true;
        job.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Jobs.UpdateAsync(job);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ============ JOB PART CREATE METHODS ============

    public async Task<JobPartDetailDto> AddJobPartAsync(int jobId, CreateJobPartDto dto)
    {
        if (jobId <= 0)
            throw new ArgumentException("Invalid job ID");

        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        if (job == null || job.IsDeleted)
            throw new InvalidOperationException($"Job with ID {jobId} not found");

        var part = await _unitOfWork.Parts.GetByIdAsync(dto.PartId);
        if (part == null)
            throw new InvalidOperationException($"Part with ID {dto.PartId} not found");

        var jobPart = new JobPart
        {
            JobId = jobId,
            PartId = dto.PartId,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            TotalCost = dto.UnitCost * dto.Quantity,
            IsWarrantyPart = dto.IsWarrantyPart,
            AddedAt = DateTime.UtcNow
        };

        await _unitOfWork.JobParts.AddAsync(jobPart);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateJobCostAsync(jobId);

        return MapJobPartToDto(jobPart);
    }

    public async Task<IEnumerable<JobPartDetailDto>> BulkAddJobPartsAsync(int jobId, IEnumerable<CreateJobPartDto> dtos)
    {
        if (jobId <= 0)
            throw new ArgumentException("Invalid job ID");

        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
        if (job == null || job.IsDeleted)
            throw new InvalidOperationException($"Job with ID {jobId} not found");

        var jobParts = new List<JobPart>();
        var dtoList = dtos.ToList();

        foreach (var dto in dtoList)
        {
            var part = await _unitOfWork.Parts.GetByIdAsync(dto.PartId);
            if (part == null)
                throw new InvalidOperationException($"Part with ID {dto.PartId} not found");

            var jobPart = new JobPart
            {
                JobId = jobId,
                PartId = dto.PartId,
                Quantity = dto.Quantity,
                UnitCost = dto.UnitCost,
                TotalCost = dto.UnitCost * dto.Quantity,
                IsWarrantyPart = dto.IsWarrantyPart,
                AddedAt = DateTime.UtcNow
            };

            jobParts.Add(jobPart);
        }

        await _unitOfWork.JobParts.AddRangeAsync(jobParts);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateJobCostAsync(jobId);

        return jobParts.Select(MapJobPartToDto).ToList();
    }

    // ============ JOB PART UPDATE METHODS ============

    public async Task<JobPartDetailDto?> UpdateJobPartAsync(int jobPartId, UpdateJobPartDto dto)
    {
        if (jobPartId <= 0)
            return null;

        var jobPart = await _unitOfWork.JobParts.GetByIdAsync(jobPartId);
        if (jobPart == null)
            return null;

        int jobId = jobPart.JobId;

        if (dto.Quantity.HasValue)
            jobPart.Quantity = dto.Quantity.Value;

        if (dto.UnitCost.HasValue)
            jobPart.UnitCost = dto.UnitCost.Value;

        if (dto.IsWarrantyPart.HasValue)
            jobPart.IsWarrantyPart = dto.IsWarrantyPart.Value;

        jobPart.TotalCost = jobPart.UnitCost * jobPart.Quantity;

        await _unitOfWork.JobParts.UpdateAsync(jobPart);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateJobCostAsync(jobId);

        return MapJobPartToDto(jobPart);
    }

    // ============ JOB PART DELETE METHODS ============

    public async Task<bool> DeleteJobPartAsync(int jobPartId)
    {
        if (jobPartId <= 0)
            return false;

        var jobPart = await _unitOfWork.JobParts.GetByIdAsync(jobPartId);
        if (jobPart == null)
            return false;

        int jobId = jobPart.JobId;

        await _unitOfWork.JobParts.DeleteAsync(jobPart);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateJobCostAsync(jobId);

        return true;
    }

    // ============ JOB PART GET METHODS ============

    public async Task<IEnumerable<JobPartDetailDto>> GetJobPartsAsync(int jobId)
    {
        if (jobId <= 0)
            return new List<JobPartDetailDto>();

        var parts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == jobId);
        return parts.Select(MapJobPartToDto).ToList();
    }

    // ============ HELPER METHODS ============

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

        var parts = await _unitOfWork.JobParts.FindAsync(p => p.JobId == job.JobId);
        foreach (var part in parts)
        {
            jobDto.JobParts.Add(MapJobPartToDto(part));
        }

        return jobDto;
    }

    private JobPartDetailDto MapJobPartToDto(JobPart part)
    {
        return new JobPartDetailDto
        {
            JobPartId = part.JobPartId,
            JobId = part.JobId,
            PartId = part.PartId,
            Quantity = part.Quantity,
            UnitCost = part.UnitCost,
            TotalCost = part.TotalCost,
            IsWarrantyPart = part.IsWarrantyPart,
            AddedAt = part.AddedAt
        };
    }

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
}