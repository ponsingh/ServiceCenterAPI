using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class VwInvoicePaymentSummary
{
    public int InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public decimal? GrandTotal { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal? RemainingAmount { get; set; }

    public string? PaymentStatusCalculated { get; set; }
}
