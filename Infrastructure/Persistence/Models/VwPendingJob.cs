using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class VwPendingJob
{
    public int JobId { get; set; }

    public string Status { get; set; } = null!;

    public string? Priority { get; set; }

    public string? DeviceName { get; set; }

    public string? AssignedTechnician { get; set; }

    public DateOnly? TargetCompletionDate { get; set; }

    public int? DaysRemaining { get; set; }
}
