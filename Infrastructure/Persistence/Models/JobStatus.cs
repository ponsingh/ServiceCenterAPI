using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class JobStatus
{
    public int StatusId { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public string Category { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? ColorCode { get; set; }

    public string? IconCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
