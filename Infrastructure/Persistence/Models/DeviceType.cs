using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class DeviceType
{
    public int DeviceTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InspectionDefinition> InspectionDefinitions { get; set; } = new List<InspectionDefinition>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
