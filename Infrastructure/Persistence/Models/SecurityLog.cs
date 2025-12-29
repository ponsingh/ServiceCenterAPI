using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class SecurityLog
{
    public int SecurityLogId { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }

    public DateTime LoggedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
