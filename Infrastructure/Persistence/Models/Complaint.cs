using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Complaint
{
    public int ComplaintId { get; set; }

    public int ServiceOrderId { get; set; }

    public int CustomerId { get; set; }

    public DateTime ComplaintDate { get; set; }

    public string ComplaintType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public int StatusId { get; set; }

    public int? AssignedToEmployeeId { get; set; }

    public string? ResolutionNotes { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public bool FollowUpRequired { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Employee? AssignedToEmployee { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<ReService> ReServices { get; set; } = new List<ReService>();

    public virtual ServiceOrder ServiceOrder { get; set; } = null!;

    public virtual ServiceOrderStatus Status { get; set; } = null!;
}
