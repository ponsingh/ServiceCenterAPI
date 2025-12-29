using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Job
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

    public bool IsWarrantyClaim { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public int? JobStatusId { get; set; }

    public virtual Employee? AssignedToNavigation { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Item Item { get; set; } = null!;

    public virtual ICollection<JobPart> JobParts { get; set; } = new List<JobPart>();

    public virtual JobStatus? JobStatus { get; set; }

    public virtual Priority? PriorityNavigation { get; set; }

    public virtual ICollection<ReturnAuthorization> ReturnAuthorizations { get; set; } = new List<ReturnAuthorization>();

    public virtual ServiceType? ServiceType { get; set; }

    public virtual ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();

    public virtual User? UpdatedByUser { get; set; }
}
