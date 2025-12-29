using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Item
{
    public int ItemId { get; set; }

    public int ServiceOrderId { get; set; }

    public int? DeviceTypeId { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNo { get; set; }

    public string? Imei { get; set; }

    public string? Accessories { get; set; }

    public string? ConditionOnReceipt { get; set; }

    public string? InspectionStatus { get; set; }

    public string? ReceivedConditionSummary { get; set; }

    public DateTime? InspectedAt { get; set; }

    public int? InspectedByEmployeeId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public virtual DeviceType? DeviceType { get; set; }

    public virtual Employee? InspectedByEmployee { get; set; }

    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<ReturnAuthorization> ReturnAuthorizations { get; set; } = new List<ReturnAuthorization>();

    public virtual ServiceOrder ServiceOrder { get; set; } = null!;

    public virtual User? UpdatedByUser { get; set; }

    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
}
