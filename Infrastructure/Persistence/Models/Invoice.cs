using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public int? ServiceOrderId { get; set; }

    public int? JobId { get; set; }

    public int CustomerId { get; set; }

    public DateTime InvoiceDate { get; set; }

    public decimal PartsCost { get; set; }

    public decimal LabourCost { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal? GrandTotal { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateOnly? DueDate { get; set; }

    public int? IssuedByEmployeeId { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Employee? IssuedByEmployee { get; set; }

    public virtual Job? Job { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ServiceOrder? ServiceOrder { get; set; }
}
