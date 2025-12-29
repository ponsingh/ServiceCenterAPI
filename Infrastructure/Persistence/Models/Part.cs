using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Part
{
    public int PartId { get; set; }

    public string PartName { get; set; } = null!;

    public string? Sku { get; set; }

    public string? Description { get; set; }

    public decimal UnitCost { get; set; }

    public decimal SellingPrice { get; set; }

    public int StockQty { get; set; }

    public int ReorderLevel { get; set; }

    public DateOnly? ManufactureDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public int? SupplierId { get; set; }

    public int? WarrantyMonths { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public int? UpdatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual ICollection<JobPart> JobParts { get; set; } = new List<JobPart>();

    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    public virtual Supplier? Supplier { get; set; }

    public virtual User? UpdatedByUser { get; set; }
}
