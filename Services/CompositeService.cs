namespace ServiceCenterAPI.Services
{
    using global::ServiceCenterAPI.Application.Services;
    using global::ServiceCenterAPI.Infrastructure.Persistence.Models;
    using global::ServiceCenterAPI.Repositories;
  
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;



    

    // ============ SERVICE INTERFACE ============

    /// <summary>
    /// Service for creating complete ServiceOrder hierarchy in single transaction
    /// </summary>
    public interface ICompositeService
    {
        /// <summary>
        /// Create complete ServiceOrder with Items, Jobs, and JobParts in single transaction
        /// </summary>
        Task<ServiceOrderDetailedDto> CreateServiceOrderCompleteAsync(CreateServiceOrderCompleteDto dto);

        /// <summary>
        /// Create complete Item with Jobs and JobParts in single transaction
        /// </summary>
        Task<ItemDetailedDto> CreateItemCompleteAsync(int serviceOrderId, CreateItemCompositeDto dto);

        /// <summary>
        /// Create complete Job with JobParts in single transaction
        /// </summary>
        Task<JobDetailedDto> CreateJobCompleteAsync(int itemId, CreateJobCompositeDto dto);
    }

    // ============ SERVICE IMPLEMENTATION ============

    /// <summary>
    /// Implementation of Composite Service for bulk operations
    /// </summary>
    public class CompositeService : ICompositeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompositeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Create complete ServiceOrder with all Items, Jobs, and JobParts
        /// Everything saved in single transaction
        /// </summary>
        public async Task<ServiceOrderDetailedDto> CreateServiceOrderCompleteAsync(CreateServiceOrderCompleteDto dto)
        {
            if (dto.CustomerId <= 0)
                throw new ArgumentException("Invalid customer ID");

            // Verify customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {dto.CustomerId} not found");

            // Start transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Create ServiceOrder
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

                // Add all Items
                foreach (var itemDto in dto.Items ?? new List<CreateItemCompositeDto>())
                {
                    var item = new Item
                    {
                        ServiceOrderId = serviceOrder.ServiceOrderId,
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

                    // Add all Jobs for this Item
                    foreach (var jobDto in itemDto.Jobs ?? new List<CreateJobCompositeDto>())
                    {
                        var job = new Job
                        {
                            ItemId = item.ItemId,
                            ServiceTypeId = jobDto.ServiceTypeId,
                            ReceivedDate = jobDto.ReceivedDate,
                            AssignedTo = jobDto.AssignedTo,
                            Priority = jobDto.Priority ?? "Low priority",
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

                        // Add all JobParts for this Job
                        var jobParts = new List<JobPart>();
                        foreach (var partDto in jobDto.JobParts ?? new List<CreateJobPartCompositeDto>())
                        {
                            // Verify part exists
                            var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId);
                            if (part == null)
                                throw new InvalidOperationException($"Part with ID {partDto.PartId} not found");

                            var jobPart = new JobPart
                            {
                                JobId = job.JobId,
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

                            // Update job actual cost
                            job.ActualCost = jobParts.Sum(p => p.TotalCost ?? 0);
                            await _unitOfWork.Jobs.UpdateAsync(job);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Load and return complete DTO
                var result = await GetServiceOrderDetailedDtoAsync(serviceOrder.ServiceOrderId);
                return result ?? throw new InvalidOperationException("Failed to retrieve created service order");
            }
            catch (Exception ex)
            {
                // Rollback on any error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Create complete Item with all Jobs and JobParts
        /// Everything saved in single transaction
        /// </summary>
        public async Task<ItemDetailedDto> CreateItemCompleteAsync(int serviceOrderId, CreateItemCompositeDto dto)
        {
            if (serviceOrderId <= 0)
                throw new ArgumentException("Invalid service order ID");

            // Verify service order exists
            var serviceOrder = await _unitOfWork.ServiceOrders.GetByIdAsync(serviceOrderId);
            if (serviceOrder == null || serviceOrder.IsDeleted)
                throw new InvalidOperationException($"Service order with ID {serviceOrderId} not found");

            // Start transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Create Item
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

                // Add all Jobs for this Item
                foreach (var jobDto in dto.Jobs ?? new List<CreateJobCompositeDto>())
                {
                    var job = new Job
                    {
                        ItemId = item.ItemId,
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

                    // Add all JobParts for this Job
                    var jobParts = new List<JobPart>();
                    foreach (var partDto in jobDto.JobParts ?? new List<CreateJobPartCompositeDto>())
                    {
                        // Verify part exists
                        var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId);
                        if (part == null)
                            throw new InvalidOperationException($"Part with ID {partDto.PartId} not found");

                        var jobPart = new JobPart
                        {
                            JobId = job.JobId,
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

                        // Update job actual cost
                        job.ActualCost = jobParts.Sum(p => p.TotalCost ?? 0);
                        await _unitOfWork.Jobs.UpdateAsync(job);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Load and return complete DTO
                var result = await GetItemDetailedDtoAsync(item.ItemId);
                return result ?? throw new InvalidOperationException("Failed to retrieve created item");
            }
            catch (Exception ex)
            {
                // Rollback on any error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Create complete Job with all JobParts
        /// Everything saved in single transaction
        /// </summary>
        public async Task<JobDetailedDto> CreateJobCompleteAsync(int itemId, CreateJobCompositeDto dto)
        {
            if (itemId <= 0)
                throw new ArgumentException("Invalid item ID");

            // Verify item exists
            var item = await _unitOfWork.Items.GetByIdAsync(itemId);
            if (item == null || item.IsDeleted)
                throw new InvalidOperationException($"Item with ID {itemId} not found");

            // Start transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Create Job
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

                // Add all JobParts
                var jobParts = new List<JobPart>();
                foreach (var partDto in dto.JobParts ?? new List<CreateJobPartCompositeDto>())
                {
                    // Verify part exists
                    var part = await _unitOfWork.Parts.GetByIdAsync(partDto.PartId);
                    if (part == null)
                        throw new InvalidOperationException($"Part with ID {partDto.PartId} not found");

                    var jobPart = new JobPart
                    {
                        JobId = job.JobId,
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

                    // Update job actual cost
                    job.ActualCost = jobParts.Sum(p => p.TotalCost ?? 0);
                    await _unitOfWork.Jobs.UpdateAsync(job);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Load and return complete DTO
                var result = await GetJobDetailedDtoAsync(job.JobId);
                return result ?? throw new InvalidOperationException("Failed to retrieve created job");
            }
            catch (Exception ex)
            {
                // Rollback on any error
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // ============ HELPER METHODS ============

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
}
