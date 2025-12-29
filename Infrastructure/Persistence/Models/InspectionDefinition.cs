using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class InspectionDefinition
{
    public int InspectionDefId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ApplicableDeviceTypeId { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual DeviceType? ApplicableDeviceType { get; set; }

    public virtual ICollection<InspectionChecklistItem> InspectionChecklistItems { get; set; } = new List<InspectionChecklistItem>();

    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
}
