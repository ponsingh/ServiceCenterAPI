using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? PaymentRef { get; set; }

    public string? TransactionId { get; set; }

    public int? PaidByEmployeeId { get; set; }

    public string? PaidByCustomerName { get; set; }

    public DateTime PaidAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Employee? PaidByEmployee { get; set; }

    public virtual PaymentMethod PaymentMethodNavigation { get; set; } = null!;
}
