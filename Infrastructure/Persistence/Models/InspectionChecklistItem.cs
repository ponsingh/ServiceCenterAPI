using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class InspectionChecklistItem
{
    public int ChecklistItemId { get; set; }

    public int InspectionDefId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string InputType { get; set; } = null!;

    public string? Options { get; set; }

    public bool IsMandatory { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<InspectionAnswer> InspectionAnswers { get; set; } = new List<InspectionAnswer>();

    public virtual InspectionDefinition InspectionDef { get; set; } = null!;
}
