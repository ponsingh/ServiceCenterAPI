using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class ReService
{
    public int ReServiceId { get; set; }

    public int OriginalServiceOrderId { get; set; }

    public int? ComplaintId { get; set; }

    public int NewServiceOrderId { get; set; }

    public int CustomerId { get; set; }

    public string ReServiceReason { get; set; } = null!;

    public string ReServiceDescription { get; set; } = null!;

    public bool IssueVerified { get; set; }

    public string? VerificationNotes { get; set; }

    public bool IsWarrantyCase { get; set; }

    public DateTime? WarrantyExpiredDate { get; set; }

    public int ApprovedByEmployeeId { get; set; }

    public int StatusId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee ApprovedByEmployee { get; set; } = null!;

    public virtual Complaint? Complaint { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ServiceOrder NewServiceOrder { get; set; } = null!;

    public virtual ServiceOrder OriginalServiceOrder { get; set; } = null!;

    public virtual ServiceOrderStatus Status { get; set; } = null!;
}
