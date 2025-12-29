using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class InspectionAnswer
{
    public int InspectionAnswerId { get; set; }

    public int InspectionId { get; set; }

    public int ChecklistItemId { get; set; }

    public string? AnswerText { get; set; }

    public virtual InspectionChecklistItem ChecklistItem { get; set; } = null!;

    public virtual Inspection Inspection { get; set; } = null!;
}
