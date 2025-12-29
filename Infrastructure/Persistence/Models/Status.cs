using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Status
{
    public string StatusCode { get; set; } = null!;

    public string? Description { get; set; }

    public int? OrderSequence { get; set; }

    public virtual ICollection<StatusHistory> StatusHistoryNewStatusNavigations { get; set; } = new List<StatusHistory>();

    public virtual ICollection<StatusHistory> StatusHistoryOldStatusNavigations { get; set; } = new List<StatusHistory>();
}
