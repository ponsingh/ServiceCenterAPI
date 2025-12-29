using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class ReturnAuthorization
{
    public int ReturnAuthId { get; set; }

    public int JobId { get; set; }

    public int ItemId { get; set; }

    public string ReturnReason { get; set; } = null!;

    public string ReturnStatus { get; set; } = null!;

    public int AuthorizedByEmployeeId { get; set; }

    public DateTime AuthorizedAt { get; set; }

    public bool ReturnedByCustomer { get; set; }

    public DateTime? ReturnedDate { get; set; }

    public int? ReplaceWith { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee AuthorizedByEmployee { get; set; } = null!;

    public virtual Item Item { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;
}
