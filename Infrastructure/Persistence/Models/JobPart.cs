using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class JobPart
{
    public int JobPartId { get; set; }

    public int JobId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal? TotalCost { get; set; }

    public bool IsWarrantyPart { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Job Job { get; set; } = null!;

    public virtual Part Part { get; set; } = null!;
}
