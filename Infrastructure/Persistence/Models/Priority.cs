using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Priority
{
    public string PriorityCode { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderSequence { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
