using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Warranty
{
    public int WarrantyId { get; set; }

    public int ItemId { get; set; }

    public string WarrantyType { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int WarrantyDays { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Item Item { get; set; } = null!;
}
