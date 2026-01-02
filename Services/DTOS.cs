namespace ServiceCenterAPI.Services
{
    // ============ COMPOSITE DTOs ============

    /// <summary>
    /// DTO for creating a complete JobPart
    /// </summary>
    public class CreateJobPartCompositeDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitCost { get; set; }
        public bool IsWarrantyPart { get; set; } = false;
        public int JobPartId { get; internal set; }
    }

    /// <summary>
    /// DTO for creating a complete Job with its parts
    /// </summary>
    public class CreateJobCompositeDto
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

        /// <summary>
        /// All JobParts for this Job
        /// </summary>
        public List<CreateJobPartCompositeDto> JobParts { get; set; } = new();
        public int JobId { get; internal set; }
    }

    /// <summary>
    /// DTO for creating a complete Item with its Jobs
    /// </summary>
    public class CreateItemCompositeDto
    {
        public int? DeviceTypeId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNo { get; set; }
        public string? Imei { get; set; }
        public string? Accessories { get; set; }
        public string? ConditionOnReceipt { get; set; }

        /// <summary>
        /// All Jobs for this Item
        /// </summary>
        public List<CreateJobCompositeDto> Jobs { get; set; } = new();
        public int ItemId { get; internal set; }
    }

    /// <summary>
    /// DTO for creating a complete ServiceOrder with all children
    /// This is the main DTO sent from client
    /// </summary>
    public class CreateServiceOrderCompleteDto
    {
        public int CustomerId { get; set; }
        public int? CreatedByEmployeeId { get; set; }
        public int? ServiceTypeId { get; set; }
        public string? ServiceOrderNumber { get; set; }
        public string? Notes { get; set; }
        public DateOnly? ExpectedPickupDate { get; set; }
        public int? StatusId { get; set; }

        /// <summary>
        /// All Items for this ServiceOrder
        /// </summary>
        public List<CreateItemCompositeDto> Items { get; set; } = new();
        public int ServiceOrderId { get; internal set; }
    }



    //-----------------------

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
}
