using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class StockTransaction
{
    public int StockTxnId { get; set; }

    public int PartId { get; set; }

    public int ChangeQty { get; set; }

    public string TxnType { get; set; } = null!;

    public string? Reference { get; set; }

    public int? PerformedBy { get; set; }

    public string? Notes { get; set; }

    public DateTime PerformedAt { get; set; }

    public virtual Part Part { get; set; } = null!;

    public virtual Employee? PerformedByNavigation { get; set; }
}
