using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServiceCenterAPI.Infrastructure.Persistence.Models;

namespace ServiceCenterAPI.Infrastructure.Persistence.DbContextActions;

public partial class ServiceCenterDbContext : DbContext
{
    public ServiceCenterDbContext(DbContextOptions<ServiceCenterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Complaint> Complaints { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Inspection> Inspections { get; set; }

    public virtual DbSet<InspectionAnswer> InspectionAnswers { get; set; }

    public virtual DbSet<InspectionChecklistItem> InspectionChecklistItems { get; set; }

    public virtual DbSet<InspectionDefinition> InspectionDefinitions { get; set; }

    public virtual DbSet<InspectionPhoto> InspectionPhotos { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobPart> JobParts { get; set; }

    public virtual DbSet<JobStatus> JobStatuses { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Priority> Priorities { get; set; }

    public virtual DbSet<ReService> ReServices { get; set; }

    public virtual DbSet<ReturnAuthorization> ReturnAuthorizations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SecurityLog> SecurityLogs { get; set; }

    public virtual DbSet<ServiceOrder> ServiceOrders { get; set; }

    public virtual DbSet<ServiceOrderStatus> ServiceOrderStatuses { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<StatusHistory> StatusHistories { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<VwInventoryStatus> VwInventoryStatuses { get; set; }

    public virtual DbSet<VwInvoicePaymentSummary> VwInvoicePaymentSummaries { get; set; }

    public virtual DbSet<VwPendingJob> VwPendingJobs { get; set; }

    public virtual DbSet<Warranty> Warranties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId).HasName("PK__Complain__740D89AF3205991E");

            entity.HasIndex(e => e.ComplaintDate, "IX_Complaints_ComplaintDate")
                .IsDescending()
                .HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.CustomerId, "IX_Complaints_CustomerID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.ServiceOrderId, "IX_Complaints_ServiceOrderID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.StatusId, "IX_Complaints_StatusID").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.ComplaintId).HasColumnName("ComplaintID");
            entity.Property(e => e.AssignedToEmployeeId).HasColumnName("AssignedToEmployeeID");
            entity.Property(e => e.ComplaintDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ComplaintType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.FollowUpRequired).HasDefaultValue(true);
            entity.Property(e => e.ServiceOrderId).HasColumnName("ServiceOrderID");
            entity.Property(e => e.Severity)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StatusId).HasColumnName("StatusID");

            entity.HasOne(d => d.AssignedToEmployee).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.AssignedToEmployeeId)
                .HasConstraintName("FK_Complaints_AssignedTo");

            entity.HasOne(d => d.Customer).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Complaints_Customers");

            entity.HasOne(d => d.ServiceOrder).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.ServiceOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Complaints_ServiceOrders");

            entity.HasOne(d => d.Status).WithMany(p => p.Complaints)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Complaints_Status");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B892B61F17");

            entity.HasIndex(e => e.ContactNumber, "IX_Customers_ContactNumber").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.Email, "IX_Customers_Email").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.IsActive, "IX_Customers_IsActive").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.CustomerType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.GstNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GST_Number");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.WhatsAppNumber)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CustomerCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Customers_CreatedBy");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.CustomerUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Customers_UpdatedBy");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.DeviceTypeId).HasName("PK__DeviceTy__07A6C7162FA20F2F");

            entity.HasIndex(e => e.IsActive, "IX_DeviceTypes_IsActive").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.TypeName, "UQ__DeviceTy__D4E7DFA807F3ACE3").IsUnique();

            entity.Property(e => e.DeviceTypeId).HasColumnName("DeviceTypeID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF194E9B0B7");

            entity.HasIndex(e => e.IsActive, "IX_Employees_IsActive").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.Role, "IX_Employees_Role").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.EmployeeName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.EmployeeCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Employees_CreatedBy");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.EmployeeUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Employees_UpdatedBy");

            entity.HasOne(d => d.User).WithMany(p => p.EmployeeUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Employees_Users");
        });

        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.HasKey(e => e.InspectionId).HasName("PK__Inspecti__30B2DC28C3CBCF1C");

            entity.HasIndex(e => e.ItemId, "IX_Inspections_ItemID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.ServiceOrderId, "IX_Inspections_ServiceOrderID").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.InspectionId).HasColumnName("InspectionID");
            entity.Property(e => e.InspectionDefId).HasColumnName("InspectionDefID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.OverallCondition)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PerformedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PerformedByEmployeeId).HasColumnName("PerformedByEmployeeID");
            entity.Property(e => e.ServiceOrderId).HasColumnName("ServiceOrderID");
            entity.Property(e => e.SignaturePath)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.InspectionDef).WithMany(p => p.Inspections)
                .HasForeignKey(d => d.InspectionDefId)
                .HasConstraintName("FK_Inspections_InspectionDef");

            entity.HasOne(d => d.Item).WithMany(p => p.Inspections)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inspections_Item");

            entity.HasOne(d => d.PerformedByEmployee).WithMany(p => p.Inspections)
                .HasForeignKey(d => d.PerformedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inspections_PerformedBy");

            entity.HasOne(d => d.ServiceOrder).WithMany(p => p.Inspections)
                .HasForeignKey(d => d.ServiceOrderId)
                .HasConstraintName("FK_Inspections_ServiceOrder");
        });

        modelBuilder.Entity<InspectionAnswer>(entity =>
        {
            entity.HasKey(e => e.InspectionAnswerId).HasName("PK__Inspecti__4512E30049FE8F4B");

            entity.HasIndex(e => e.InspectionId, "IX_InspectionAnswers_InspectionID");

            entity.Property(e => e.InspectionAnswerId).HasColumnName("InspectionAnswerID");
            entity.Property(e => e.ChecklistItemId).HasColumnName("ChecklistItemID");
            entity.Property(e => e.InspectionId).HasColumnName("InspectionID");

            entity.HasOne(d => d.ChecklistItem).WithMany(p => p.InspectionAnswers)
                .HasForeignKey(d => d.ChecklistItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InspectionAnswers_ChecklistItem");

            entity.HasOne(d => d.Inspection).WithMany(p => p.InspectionAnswers)
                .HasForeignKey(d => d.InspectionId)
                .HasConstraintName("FK_InspectionAnswers_Inspection");
        });

        modelBuilder.Entity<InspectionChecklistItem>(entity =>
        {
            entity.HasKey(e => e.ChecklistItemId).HasName("PK__Inspecti__407798CD16E485E2");

            entity.HasIndex(e => e.InspectionDefId, "IX_InspectionChecklist_Def");

            entity.Property(e => e.ChecklistItemId).HasColumnName("ChecklistItemID");
            entity.Property(e => e.InputType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InspectionDefId).HasColumnName("InspectionDefID");
            entity.Property(e => e.Options).HasMaxLength(500);
            entity.Property(e => e.QuestionText).HasMaxLength(400);

            entity.HasOne(d => d.InspectionDef).WithMany(p => p.InspectionChecklistItems)
                .HasForeignKey(d => d.InspectionDefId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Checklist_Def");
        });

        modelBuilder.Entity<InspectionDefinition>(entity =>
        {
            entity.HasKey(e => e.InspectionDefId).HasName("PK__Inspecti__5CF46DA1BFE0D3FC");

            entity.Property(e => e.InspectionDefId).HasColumnName("InspectionDefID");
            entity.Property(e => e.ApplicableDeviceTypeId).HasColumnName("ApplicableDeviceTypeID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(400);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.ApplicableDeviceType).WithMany(p => p.InspectionDefinitions)
                .HasForeignKey(d => d.ApplicableDeviceTypeId)
                .HasConstraintName("FK_InspectionDef_DeviceType");
        });

        modelBuilder.Entity<InspectionPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__Inspecti__21B7B582D355191A");

            entity.HasIndex(e => e.InspectionId, "IX_InspectionPhotos_InspectionID");

            entity.Property(e => e.PhotoId).HasColumnName("PhotoID");
            entity.Property(e => e.InspectionId).HasColumnName("InspectionID");
            entity.Property(e => e.PhotoCaption).HasMaxLength(250);
            entity.Property(e => e.PhotoPath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PhotoType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Inspection).WithMany(p => p.InspectionPhotos)
                .HasForeignKey(d => d.InspectionId)
                .HasConstraintName("FK_InspectionPhotos_Inspection");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAD5F950D629");

            entity.HasIndex(e => e.CustomerId, "IX_Invoices_CustomerID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.DueDate, "IX_Invoices_DueDate").HasFilter("([PaymentStatus]<>'Paid' AND [IsDeleted]=(0))");

            entity.HasIndex(e => e.JobId, "IX_Invoices_JobID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.PaymentStatus, "IX_Invoices_PaymentStatus").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.ServiceOrderId, "IX_Invoices_ServiceOrderID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.InvoiceNumber, "UQ__Invoices__D776E98110152F6B").IsUnique();

            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.GrandTotal)
                .HasComputedColumnSql("((([PartsCost]+[LabourCost])-[DiscountAmount])+[TaxAmount])", true)
                .HasColumnType("decimal(15, 2)");
            entity.Property(e => e.InvoiceDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.IssuedByEmployeeId).HasColumnName("IssuedByEmployeeID");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.LabourCost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PartsCost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ServiceOrderId).HasColumnName("ServiceOrderID");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Customer");

            entity.HasOne(d => d.IssuedByEmployee).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.IssuedByEmployeeId)
                .HasConstraintName("FK_Invoices_IssuedBy");

            entity.HasOne(d => d.Job).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_Invoices_Jobs");

            entity.HasOne(d => d.ServiceOrder).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ServiceOrderId)
                .HasConstraintName("FK_Invoices_ServiceOrder");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Items__727E83EBA4CBEBB7");

            entity.HasIndex(e => e.InspectionStatus, "IX_Items_InspectionStatus").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.SerialNo, "IX_Items_SerialNo").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.ServiceOrderId, "IX_Items_ServiceOrderID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.ServiceOrderId, e.SerialNo }, "UQ_Items_SerialNo_ServiceOrder").IsUnique();

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Accessories).HasMaxLength(250);
            entity.Property(e => e.Brand).HasMaxLength(120);
            entity.Property(e => e.ConditionOnReceipt).HasMaxLength(400);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeviceTypeId).HasColumnName("DeviceTypeID");
            entity.Property(e => e.Imei)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("IMEI");
            entity.Property(e => e.InspectedByEmployeeId).HasColumnName("InspectedByEmployeeID");
            entity.Property(e => e.InspectionStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Model).HasMaxLength(120);
            entity.Property(e => e.ReceivedConditionSummary).HasMaxLength(500);
            entity.Property(e => e.SerialNo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ServiceOrderId).HasColumnName("ServiceOrderID");

            entity.HasOne(d => d.DeviceType).WithMany(p => p.Items)
                .HasForeignKey(d => d.DeviceTypeId)
                .HasConstraintName("FK_Items_DeviceTypes");

            entity.HasOne(d => d.InspectedByEmployee).WithMany(p => p.Items)
                .HasForeignKey(d => d.InspectedByEmployeeId)
                .HasConstraintName("FK_Items_InspectedBy");

            entity.HasOne(d => d.ServiceOrder).WithMany(p => p.Items)
                .HasForeignKey(d => d.ServiceOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Items_ServiceOrders");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.Items)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Items_UpdatedBy");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__Jobs__056690E286DEFAB6");

            entity.HasIndex(e => e.AssignedTo, "IX_Jobs_AssignedTo").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.ItemId, "IX_Jobs_ItemID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.JobStatusId, "IX_Jobs_JobStatusID").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.ActualCost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.EstimatedCost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.JobStatusId).HasColumnName("JobStatusID");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Medium");
            entity.Property(e => e.ReceivedDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.AssignedTo)
                .HasConstraintName("FK_Jobs_Employees");

            entity.HasOne(d => d.Item).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Jobs_Items");

            entity.HasOne(d => d.JobStatus).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.JobStatusId)
                .HasConstraintName("FK_Jobs_JobStatus");

            entity.HasOne(d => d.PriorityNavigation).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.Priority)
                .HasConstraintName("FK_Jobs_Priority");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_Jobs_ServiceType");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.Jobs)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Jobs_UpdatedBy");
        });

        modelBuilder.Entity<JobPart>(entity =>
        {
            entity.HasKey(e => e.JobPartId).HasName("PK__JobParts__6E7158CC1681E777");

            entity.HasIndex(e => e.JobId, "IX_JobParts_JobID");

            entity.HasIndex(e => e.PartId, "IX_JobParts_PartID");

            entity.Property(e => e.JobPartId).HasColumnName("JobPartID");
            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.TotalCost)
                .HasComputedColumnSql("([Quantity]*[UnitCost])", true)
                .HasColumnType("decimal(23, 2)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Job).WithMany(p => p.JobParts)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_JobParts_Jobs");

            entity.HasOne(d => d.Part).WithMany(p => p.JobParts)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JobParts_Parts");
        });

        modelBuilder.Entity<JobStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__JobStatu__C8EE20436F30D8E0");

            entity.ToTable("JobStatus");

            entity.HasIndex(e => e.Category, "IX_JobStatus_Category");

            entity.HasIndex(e => e.IsActive, "IX_JobStatus_IsActive");

            entity.HasIndex(e => e.StatusCode, "IX_JobStatus_StatusCode");

            entity.HasIndex(e => e.StatusCode, "UQ__JobStatu__6A7B44FC72B44079").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ColorCode)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StatusCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StatusName).HasMaxLength(100);
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("PK__Parts__7C3F0D303AEFBDE7");

            entity.HasIndex(e => e.ExpiryDate, "IX_Parts_ExpiryDate").HasFilter("([ExpiryDate] IS NOT NULL AND [IsDeleted]=(0))");

            entity.HasIndex(e => e.Sku, "IX_Parts_SKU").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.StockQty, "IX_Parts_StockQty").HasFilter("([IsActive]=(1) AND [IsDeleted]=(0))");

            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PartName).HasMaxLength(200);
            entity.Property(e => e.ReorderLevel).HasDefaultValue(5);
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PartCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Parts_CreatedBy");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Parts)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_Parts_Suppliers");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.PartUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Parts_UpdatedBy");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58C09EC9AD");

            entity.HasIndex(e => e.InvoiceId, "IX_Payments_InvoiceID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.PaidAt, "IX_Payments_PaidAt").IsDescending();

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.PaidAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PaidByCustomerName).HasMaxLength(150);
            entity.Property(e => e.PaidByEmployeeId).HasColumnName("PaidByEmployeeID");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Cash");
            entity.Property(e => e.PaymentRef)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .HasColumnName("TransactionID");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Invoices");

            entity.HasOne(d => d.PaidByEmployee).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaidByEmployeeId)
                .HasConstraintName("FK_Payments_PaidBy");

            entity.HasOne(d => d.PaymentMethodNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_PaymentMethod");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodCode).HasName("PK__PaymentM__A4EBBD60195713F4");

            entity.Property(e => e.PaymentMethodCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Priority>(entity =>
        {
            entity.HasKey(e => e.PriorityCode).HasName("PK__Prioriti__3E2A44759AEA1CED");

            entity.Property(e => e.PriorityCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ReService>(entity =>
        {
            entity.HasKey(e => e.ReServiceId).HasName("PK__ReServic__E91E0784C0B592E5");

            entity.HasIndex(e => e.CustomerId, "IX_ReServices_CustomerID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.NewServiceOrderId, "IX_ReServices_NewServiceOrderID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.OriginalServiceOrderId, "IX_ReServices_OriginalServiceOrderID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.StatusId, "IX_ReServices_StatusID").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.ReServiceId).HasColumnName("ReServiceID");
            entity.Property(e => e.ApprovedByEmployeeId).HasColumnName("ApprovedByEmployeeID");
            entity.Property(e => e.ComplaintId).HasColumnName("ComplaintID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.IsWarrantyCase).HasDefaultValue(true);
            entity.Property(e => e.NewServiceOrderId).HasColumnName("NewServiceOrderID");
            entity.Property(e => e.OriginalServiceOrderId).HasColumnName("OriginalServiceOrderID");
            entity.Property(e => e.ReServiceReason)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StatusId).HasColumnName("StatusID");

            entity.HasOne(d => d.ApprovedByEmployee).WithMany(p => p.ReServices)
                .HasForeignKey(d => d.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReServices_ApprovedBy");

            entity.HasOne(d => d.Complaint).WithMany(p => p.ReServices)
                .HasForeignKey(d => d.ComplaintId)
                .HasConstraintName("FK_ReServices_Complaints");

            entity.HasOne(d => d.Customer).WithMany(p => p.ReServices)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReServices_Customers");

            entity.HasOne(d => d.NewServiceOrder).WithMany(p => p.ReServiceNewServiceOrders)
                .HasForeignKey(d => d.NewServiceOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReServices_NewSO");

            entity.HasOne(d => d.OriginalServiceOrder).WithMany(p => p.ReServiceOriginalServiceOrders)
                .HasForeignKey(d => d.OriginalServiceOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReServices_OriginalSO");

            entity.HasOne(d => d.Status).WithMany(p => p.ReServices)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReServices_Status");
        });

        modelBuilder.Entity<ReturnAuthorization>(entity =>
        {
            entity.HasKey(e => e.ReturnAuthId).HasName("PK__ReturnAu__FDA2877C768B329A");

            entity.HasIndex(e => e.ItemId, "IX_ReturnAuth_ItemID");

            entity.HasIndex(e => e.JobId, "IX_ReturnAuth_JobID");

            entity.HasIndex(e => e.ReturnStatus, "IX_ReturnAuth_ReturnStatus");

            entity.Property(e => e.ReturnAuthId).HasColumnName("ReturnAuthID");
            entity.Property(e => e.AuthorizedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.AuthorizedByEmployeeId).HasColumnName("AuthorizedByEmployeeID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.ReturnReason).HasMaxLength(500);
            entity.Property(e => e.ReturnStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Authorized");

            entity.HasOne(d => d.AuthorizedByEmployee).WithMany(p => p.ReturnAuthorizations)
                .HasForeignKey(d => d.AuthorizedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnAuth_AuthorizedBy");

            entity.HasOne(d => d.Item).WithMany(p => p.ReturnAuthorizations)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnAuth_Item");

            entity.HasOne(d => d.Job).WithMany(p => p.ReturnAuthorizations)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnAuth_Job");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A25A5B765");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61608C1DF436").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<SecurityLog>(entity =>
        {
            entity.HasKey(e => e.SecurityLogId).HasName("PK__Security__475587C5D0C0A134");

            entity.ToTable("SecurityLog");

            entity.HasIndex(e => e.Action, "IX_SecurityLog_Action");

            entity.HasIndex(e => e.LoggedAt, "IX_SecurityLog_LoggedAt").IsDescending();

            entity.HasIndex(e => e.UserId, "IX_SecurityLog_UserId");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.LoggedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TableName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.SecurityLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SecurityLog_Users");
        });

        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.ServiceOrderId).HasName("PK__ServiceO__8E1ABD052F113DE9");

            entity.HasIndex(e => e.CreatedAt, "IX_ServiceOrders_CreatedAt")
                .IsDescending()
                .HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.CustomerId, "IX_ServiceOrders_CustomerID").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.ServiceOrderNumber, "UQ__ServiceO__A1FCFE6C85044335").IsUnique();

            entity.Property(e => e.ServiceOrderId).HasColumnName("ServiceOrderID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedByEmployeeId).HasColumnName("CreatedByEmployeeID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.ServiceOrderNumber).HasMaxLength(100);
            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");

            entity.HasOne(d => d.CreatedByEmployee).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.CreatedByEmployeeId)
                .HasConstraintName("FK_ServiceOrders_Employees");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceOrders_Customers");

            entity.HasOne(d => d.ServiceType).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.ServiceTypeId)
                .HasConstraintName("FK_ServiceOrders_ServiceType");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.ServiceOrders)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_ServiceOrders_UpdatedBy");
        });

        modelBuilder.Entity<ServiceOrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__ServiceO__C8EE20435FFB880F");

            entity.ToTable("ServiceOrderStatus");

            entity.HasIndex(e => e.IsActive, "IX_ServiceOrderStatus_IsActive");

            entity.HasIndex(e => e.StatusCode, "IX_ServiceOrderStatus_StatusCode");

            entity.HasIndex(e => e.StatusCode, "UQ__ServiceO__6A7B44FCF0ECED7E").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.ColorCode)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StatusCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StatusName).HasMaxLength(100);
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(e => e.ServiceTypeId).HasName("PK__ServiceT__8ADFAA0CB630E2C4");

            entity.HasIndex(e => e.ServiceTypeName, "UQ__ServiceT__64009631E5371C36").IsUnique();

            entity.Property(e => e.ServiceTypeId).HasColumnName("ServiceTypeID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ServiceTypeName).HasMaxLength(200);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusCode).HasName("PK__Statuses__6A7B44FD8079DD1B");

            entity.Property(e => e.StatusCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.StatusHistoryId).HasName("PK__StatusHi__DB9734B19FB2D2A4");

            entity.ToTable("StatusHistory");

            entity.HasIndex(e => e.ChangedAt, "IX_StatusHistory_ChangedAt").IsDescending();

            entity.HasIndex(e => e.JobId, "IX_StatusHistory_JobID");

            entity.Property(e => e.StatusHistoryId).HasColumnName("StatusHistoryID");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OldStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusHistory_ChangedBy");

            entity.HasOne(d => d.Job).WithMany(p => p.StatusHistories)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_StatusHistory_Jobs");

            entity.HasOne(d => d.NewStatusNavigation).WithMany(p => p.StatusHistoryNewStatusNavigations)
                .HasForeignKey(d => d.NewStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusHistory_NewStatus");

            entity.HasOne(d => d.OldStatusNavigation).WithMany(p => p.StatusHistoryOldStatusNavigations)
                .HasForeignKey(d => d.OldStatus)
                .HasConstraintName("FK_StatusHistory_OldStatus");
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.StockTxnId).HasName("PK__StockTra__DBFF6D981FF3B923");

            entity.HasIndex(e => e.PartId, "IX_StockTransactions_PartID");

            entity.HasIndex(e => e.PerformedAt, "IX_StockTransactions_PerformedAt").IsDescending();

            entity.Property(e => e.StockTxnId).HasColumnName("StockTxnID");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PartId).HasColumnName("PartID");
            entity.Property(e => e.PerformedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Reference)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TxnType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Part).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransactions_Parts");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.PerformedBy)
                .HasConstraintName("FK_StockTransactions_PerformedBy");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE66694BF8BED13");

            entity.HasIndex(e => e.IsActive, "IX_Suppliers_IsActive").HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.Address).HasMaxLength(400);
            entity.Property(e => e.Contact)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.GstNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GST_Number");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SupplierName).HasMaxLength(200);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.SupplierCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Suppliers_CreatedBy");

            entity.HasOne(d => d.UpdatedByUser).WithMany(p => p.SupplierUpdatedByUsers)
                .HasForeignKey(d => d.UpdatedByUserId)
                .HasConstraintName("FK_Suppliers_UpdatedBy");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CD0C169D4");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.IsActive, "IX_Users_IsActive").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4070033AD").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341C69C62D").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A3504B587A7");

            entity.HasIndex(e => e.UserId, "IX_UserRoles_UserId");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_UserRoles_User_Role").IsUnique();

            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Users");
        });

        modelBuilder.Entity<VwInventoryStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_InventoryStatus");

            entity.Property(e => e.ExpiryStatus)
                .HasMaxLength(13)
                .IsUnicode(false);
            entity.Property(e => e.PartId)
                .ValueGeneratedOnAdd()
                .HasColumnName("PartID");
            entity.Property(e => e.PartName).HasMaxLength(200);
            entity.Property(e => e.Sku)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.StockStatus)
                .HasMaxLength(7)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwInvoicePaymentSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_InvoicePaymentSummary");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
            entity.Property(e => e.PaymentStatusCalculated)
                .HasMaxLength(11)
                .IsUnicode(false);
            entity.Property(e => e.RemainingAmount).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<VwPendingJob>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_PendingJobs");

            entity.Property(e => e.AssignedTechnician).HasMaxLength(150);
            entity.Property(e => e.DeviceName).HasMaxLength(241);
            entity.Property(e => e.JobId).HasColumnName("JobID");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Warranty>(entity =>
        {
            entity.HasKey(e => e.WarrantyId).HasName("PK__Warranty__2ED318F3C2B601EF");

            entity.ToTable("Warranty");

            entity.HasIndex(e => e.EndDate, "IX_Warranty_EndDate").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => e.ItemId, "IX_Warranty_ItemID");

            entity.Property(e => e.WarrantyId).HasColumnName("WarrantyID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.StartDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.WarrantyType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Item).WithMany(p => p.Warranties)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Warranty_Item");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
