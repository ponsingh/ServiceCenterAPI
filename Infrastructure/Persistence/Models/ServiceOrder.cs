using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class ServiceOrder
{
    public int ServiceOrderId { get; set; }

    public int CustomerId { get; set; }

    public int? CreatedByEmployeeId { get; set; }

    public int? ServiceTypeId { get; set; }

    public string? ServiceOrderNumber { get; set; }

    public string? Notes { get; set; }

    public DateOnly? ExpectedPickupDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public int? StatusId { get; set; }

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual Employee? CreatedByEmployee { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<ReService> ReServiceNewServiceOrders { get; set; } = new List<ReService>();

    public virtual ICollection<ReService> ReServiceOriginalServiceOrders { get; set; } = new List<ReService>();

    public virtual ServiceType? ServiceType { get; set; }

    public virtual User? UpdatedByUser { get; set; }
}
