using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class InspectionPhoto
{
    public int PhotoId { get; set; }

    public int InspectionId { get; set; }

    public string? PhotoPath { get; set; }

    public string? BlobId { get; set; }

    public string? PhotoCaption { get; set; }

    public string? PhotoType { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Inspection Inspection { get; set; } = null!;
}
