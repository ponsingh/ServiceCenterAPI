using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class VwInventoryStatus
{
    public int PartId { get; set; }

    public string PartName { get; set; } = null!;

    public string? Sku { get; set; }

    public int StockQty { get; set; }

    public int ReorderLevel { get; set; }

    public string StockStatus { get; set; } = null!;

    public string ExpiryStatus { get; set; } = null!;
}
