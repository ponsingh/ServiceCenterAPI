using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class StatusHistory
{
    public int StatusHistoryId { get; set; }

    public int JobId { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Reason { get; set; }

    public virtual Employee ChangedByNavigation { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual Status NewStatusNavigation { get; set; } = null!;

    public virtual Status? OldStatusNavigation { get; set; }
}
