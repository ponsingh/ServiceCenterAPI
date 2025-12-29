using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class Inspection
{
    public int InspectionId { get; set; }

    public int ItemId { get; set; }

    public int? ServiceOrderId { get; set; }

    public int PerformedByEmployeeId { get; set; }

    public int? InspectionDefId { get; set; }

    public DateTime PerformedAt { get; set; }

    public string? OverallCondition { get; set; }

    public bool TamperFlag { get; set; }

    public bool CustomerSigned { get; set; }

    public string? StaffNotes { get; set; }

    public string? CustomerNotes { get; set; }

    public string? SignaturePath { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<InspectionAnswer> InspectionAnswers { get; set; } = new List<InspectionAnswer>();

    public virtual InspectionDefinition? InspectionDef { get; set; }

    public virtual ICollection<InspectionPhoto> InspectionPhotos { get; set; } = new List<InspectionPhoto>();

    public virtual Item Item { get; set; } = null!;

    public virtual Employee PerformedByEmployee { get; set; } = null!;

    public virtual ServiceOrder? ServiceOrder { get; set; }
}
